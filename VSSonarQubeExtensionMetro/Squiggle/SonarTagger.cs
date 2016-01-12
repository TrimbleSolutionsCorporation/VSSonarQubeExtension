// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarTagger.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarQubeExtension.Squiggle
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows.Threading;

    

    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;

    using VSSonarQubeExtension.Helpers;
    using VSSonarPlugins.Types;

    /// <summary>
    ///     The sonar tagger.
    /// </summary>
    public partial class SonarTagger : ITagger<SonarTag>, IDisposable
    {
        #region Fields

        /// <summary>
        ///     The dispatcher.
        /// </summary>
        private readonly Dispatcher dispatcher;

        /// <summary>
        ///     The m disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        ///     The sonar tags.
        /// </summary>
        private volatile List<SonarTag> sonarTags = new List<SonarTag>();

        /// <summary>
        ///     The timer.
        /// </summary>
        private DispatcherTimer timer;

        /// <summary>
        ///     The update thread.
        /// </summary>
        private Thread updateThread;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarTagger"/> class.
        /// </summary>
        /// <param name="sourceBuffer">
        /// The source buffer.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// source buffer
        /// </exception>
        public SonarTagger(ITextBuffer sourceBuffer)
        {
            if (sourceBuffer == null)
            {
                throw new ArgumentNullException("sourceBuffer");
            }

            this.SourceBuffer = sourceBuffer;
            this.dispatcher = Dispatcher.CurrentDispatcher;

            try
            {
                this.ScheduleUpdate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problems schedulling update: " + ex.Message + "::" + ex.StackTrace);
            }

            try
            {
                SonarQubeViewModelFactory.SQViewModel.ServerViewModel.IssuesReadyForCollecting += this.IssuesListChanged;
                SonarQubeViewModelFactory.SQViewModel.LocalViewModel.IssuesReadyForCollecting += this.IssuesListChanged;
                SonarQubeViewModelFactory.SQViewModel.IssuesSearchModel.IssuesReadyForCollecting += this.IssuesListChanged;
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException(ex.Message);
            }
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The tags changed.
        /// </summary>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the buffer.
        /// </summary>
        public ITextBuffer SourceBuffer { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get leading whitespace length.
        /// </summary>
        /// <param name="s">
        /// The s.
        /// </param>
        /// <param name="tabLength">
        /// The tab length.
        /// </param>
        /// <param name="trimToLowerTab">
        /// The trim to lower tab.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int GetLeadingWhitespaceLength(string s, int tabLength = 4, bool trimToLowerTab = true)
        {
            if (s.Length < tabLength)
            {
                return 0;
            }

            int whiteSpaceCount = 0;

            while (char.IsWhiteSpace(s[whiteSpaceCount]))
            {
                whiteSpaceCount++;
            }

            if (whiteSpaceCount < tabLength)
            {
                return 0;
            }

            if (trimToLowerTab)
            {
                whiteSpaceCount -= whiteSpaceCount % tabLength;
            }

            return whiteSpaceCount;
        }

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The get tags.
        /// </summary>
        /// <param name="spans">
        /// The spans.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public IEnumerable<ITagSpan<SonarTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            if (spans.Count == 0)
            {
                yield break;
            }

            if (this.sonarTags.Count == 0)
            {
                yield break;
            }

            ITextSnapshot snapshot = spans[0].Snapshot;

            foreach (SonarTag tag in this.sonarTags)
            {
                ITagSpan<SonarTag> tagSpan = tag.ToTagSpan(snapshot);
                if (tagSpan.Span.Length == 0)
                {
                    continue;
                }

                yield return tagSpan;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    SonarQubeViewModelFactory.SQViewModel.ServerViewModel.IssuesReadyForCollecting -= this.IssuesListChanged;
                    SonarQubeViewModelFactory.SQViewModel.LocalViewModel.IssuesReadyForCollecting -= this.IssuesListChanged;
                    SonarQubeViewModelFactory.SQViewModel.IssuesSearchModel.IssuesReadyForCollecting -= this.IssuesListChanged;

                    this.SourceBuffer = null;
                }

                this.isDisposed = true;
            }
        }

        /// <summary>
        /// The get sonar tags in span for line.
        /// </summary>
        /// <param name="issuesInEditor">
        /// The issues in editor.
        /// </param>
        /// <param name="line">
        /// The line.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        private IEnumerable<SonarTag> GetSonarTagsInSpanForLine(List<Issue> issuesInEditor, int line)
        {
            if (issuesInEditor.Count == 0)
            {
                yield break;
            }

            List<Issue> currentIssuesPerLine = issuesInEditor.Where(issue => issue.Line == line).ToList();

            int lineToUseinVs = line - 1;
            if (lineToUseinVs < 0)
            {
                lineToUseinVs = 0;
            }

            ITextSnapshotLine textsnapshot;

            try
            {
                textsnapshot = this.SourceBuffer.CurrentSnapshot.GetLineFromLineNumber(lineToUseinVs);
            }
            catch (Exception)
            {
                yield break;
            }

            int lineStart = GetLeadingWhitespaceLength(textsnapshot.GetText());

            var mappedSpan = new SnapshotSpan(this.SourceBuffer.CurrentSnapshot, textsnapshot.Start + lineStart, textsnapshot.Length - lineStart);
            yield return new SonarTag(currentIssuesPerLine, mappedSpan);
        }

        /// <summary>
        /// The issues list changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void IssuesListChanged(object sender, EventArgs e)
        {
            try
            {                
                Resource resource = SonarQubeViewModelFactory.SQViewModel.ResourceInEditor;

                if (resource == null)
                {
                    return;
                }

                string document = SonarQubeViewModelFactory.SQViewModel.VsHelper.GetCurrentDocumentInView();
                if (string.IsNullOrEmpty(document))
                {
                    return;
                }

                if (!document.Replace('\\', '/').ToLower().EndsWith(resource.Lname.ToLower(), StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                this.sonarTags.Clear();
                bool showFalseAndResolved = false;
                List<Issue> issuesInEditor = SonarQubeViewModelFactory.SQViewModel.GetIssuesInEditor(
                    resource, 
                    this.SourceBuffer.CurrentSnapshot.GetText(), out showFalseAndResolved);

                if (issuesInEditor == null || issuesInEditor.Count == 0)
                {
                    this.RefreshTags();
                    return;
                }

                var alreadyAddLine = new Dictionary<int, string>();
                foreach (Issue issue in issuesInEditor)
                {
                    if (!showFalseAndResolved)
                    {
                        if (issue.Status == IssueStatus.CLOSED || issue.Status == IssueStatus.RESOLVED)
                        {
                            continue;
                        }
                    }

                    if (alreadyAddLine.ContainsKey(issue.Line))
                    {
                        continue;
                    }

                    alreadyAddLine.Add(issue.Line, string.Empty);
                    this.sonarTags.AddRange(this.GetSonarTagsInSpanForLine(issuesInEditor, issue.Line));
                }

                this.RefreshTags();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed To Update Issues: " + ex.Message + " : " + ex.StackTrace);
            }
        }

        /// <summary>
        ///     The refresh tags.
        /// </summary>
        private void RefreshTags()
        {
            this.dispatcher.Invoke(
                () =>
                    {
                        EventHandler<SnapshotSpanEventArgs> tempEvent = this.TagsChanged;
                        if (tempEvent != null)
                        {
                            tempEvent(
                                this, 
                                new SnapshotSpanEventArgs(
                                    new SnapshotSpan(this.SourceBuffer.CurrentSnapshot, 0, this.SourceBuffer.CurrentSnapshot.Length)));
                        }
                    });
        }

        /// <summary>
        ///     The schedule update.
        /// </summary>
        private void ScheduleUpdate()
        {
            if (this.timer == null)
            {
                this.timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle, this.dispatcher) { Interval = TimeSpan.FromMilliseconds(500) };

                this.timer.Tick += (sender, args) =>
                    {
                        if (this.updateThread != null && this.updateThread.IsAlive)
                        {
                            return;
                        }

                        this.timer.Stop();

                        this.updateThread = new Thread(this.UpdateDataAfterConstructor) { Name = "Spell Check", Priority = ThreadPriority.Normal };

                        if (!this.updateThread.TrySetApartmentState(ApartmentState.STA))
                        {
                            Debug.Fail("Unable to set thread apartment state to STA, things *will* break.");
                        }

                        this.updateThread.Start();
                    };
            }

            this.timer.Stop();
            this.timer.Start();
        }

        /// <summary>
        /// The update data after constructor.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void UpdateDataAfterConstructor(object obj)
        {
            this.IssuesListChanged(obj, EventArgs.Empty);
        }

        #endregion
    }
}