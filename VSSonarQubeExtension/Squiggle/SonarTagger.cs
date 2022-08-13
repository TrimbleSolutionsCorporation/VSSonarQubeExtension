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
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;
    using SonarRestService.Types;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows.Threading;

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
        private readonly List<SonarTag> sonarTags = new List<SonarTag>();

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

            SourceBuffer = sourceBuffer;
            dispatcher = Dispatcher.CurrentDispatcher;

            try
            {
                ScheduleUpdate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problems schedulling update: " + ex.Message + "::" + ex.StackTrace);
            }

			if (SonarQubeViewModelFactory.SQViewModel.ServerViewModel != null)
			{
				SonarQubeViewModelFactory.SQViewModel.ServerViewModel.IssuesReadyForCollecting += IssuesListChanged;
			}

			if (SonarQubeViewModelFactory.SQViewModel.LocalViewModel != null)
			{
				SonarQubeViewModelFactory.SQViewModel.LocalViewModel.IssuesReadyForCollecting += IssuesListChanged;
			}

			if (SonarQubeViewModelFactory.SQViewModel.IssuesSearchModel != null)
			{
				SonarQubeViewModelFactory.SQViewModel.IssuesSearchModel.IssuesReadyForCollecting += IssuesListChanged;
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
            Dispose(true);
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

            if (sonarTags.Count == 0)
            {
                yield break;
            }

            ITextSnapshot snapshot = spans[0].Snapshot;

            foreach (SonarTag tag in sonarTags)
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
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (SonarQubeViewModelFactory.SQViewModel.ServerViewModel != null)
                    {
                        SonarQubeViewModelFactory.SQViewModel.ServerViewModel.IssuesReadyForCollecting -= IssuesListChanged;
                    }

                    if (SonarQubeViewModelFactory.SQViewModel.LocalViewModel != null)
                    {
                        SonarQubeViewModelFactory.SQViewModel.LocalViewModel.IssuesReadyForCollecting -= IssuesListChanged;
                    }

                    if (SonarQubeViewModelFactory.SQViewModel.IssuesSearchModel != null)
                    {
                        SonarQubeViewModelFactory.SQViewModel.IssuesSearchModel.IssuesReadyForCollecting -= IssuesListChanged;
                    }                    

                    SourceBuffer = null;
                }

                isDisposed = true;
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
                textsnapshot = SourceBuffer.CurrentSnapshot.GetLineFromLineNumber(lineToUseinVs);
            }
            catch (Exception)
            {
                yield break;
            }

            int lineStart = GetLeadingWhitespaceLength(textsnapshot.GetText());

            SnapshotSpan mappedSpan = new SnapshotSpan(SourceBuffer.CurrentSnapshot, textsnapshot.Start + lineStart, textsnapshot.Length - lineStart);
            yield return new SonarTag(currentIssuesPerLine, mappedSpan);
        }

        /// <summary>
        /// The issues list changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void IssuesListChanged(object sender, EventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
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


                if (resource.Lname != null && !document.Replace('\\', '/').ToLower().EndsWith(resource.Lname.ToLower(), StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (resource.Name != null && !document.Replace('\\', '/').ToLower().EndsWith(resource.Name.ToLower(), StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                sonarTags.Clear();
                Tuple<List<Issue>, bool> data = await SonarQubeViewModelFactory.SQViewModel.GetIssuesInEditor(
                    resource, 
                    SourceBuffer.CurrentSnapshot.GetText());

				List<Issue> issuesInEditor = data.Item1;
				bool showFalseAndResolved = data.Item2;
				if (issuesInEditor == null || issuesInEditor.Count == 0)
                {
                    await RefreshTagsAsync();
                    return;
                }

                Dictionary<int, string> alreadyAddLine = new Dictionary<int, string>();
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
                    sonarTags.AddRange(GetSonarTagsInSpanForLine(issuesInEditor, issue.Line));
                }

                await RefreshTagsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed To Update Issues: " + ex.Message + " : " + ex.StackTrace);
            }
        }

        /// <summary>
        ///     The refresh tags.
        /// </summary>
        private async System.Threading.Tasks.Task RefreshTagsAsync()
        {
            // At this point, we’re on whatever thread the caller was on (UI thread or background).
            // The first thing we do is call a VS service, so ensure we’re on the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            EventHandler<SnapshotSpanEventArgs> tempEvent = TagsChanged;
            if (tempEvent != null)
            {
                tempEvent(
                    this, 
                    new SnapshotSpanEventArgs(
                        new SnapshotSpan(SourceBuffer.CurrentSnapshot, 0, SourceBuffer.CurrentSnapshot.Length)));
            }
        }

        /// <summary>
        ///     The schedule update.
        /// </summary>
        private void ScheduleUpdate()
        {
            if (timer == null)
            {
                timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle, dispatcher) { Interval = TimeSpan.FromMilliseconds(500) };

                timer.Tick += (sender, args) =>
                    {
                        if (updateThread != null && updateThread.IsAlive)
                        {
                            return;
                        }

                        timer.Stop();

                        updateThread = new Thread(UpdateDataAfterConstructor) { Name = "Spell Check", Priority = ThreadPriority.Normal };

                        if (!updateThread.TrySetApartmentState(ApartmentState.STA))
                        {
                            Debug.Fail("Unable to set thread apartment state to STA, things *will* break.");
                        }

                        updateThread.Start();
                    };
            }

            timer.Stop();
            timer.Start();
        }

        /// <summary>
        /// The update data after constructor.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void UpdateDataAfterConstructor(object obj)
        {
            IssuesListChanged(obj, EventArgs.Empty);
        }

        #endregion
    }
}
