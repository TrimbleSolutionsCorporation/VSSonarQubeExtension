// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarTagger.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarExtension.SmartTags.Squiggle
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows.Threading;

    using ExtensionTypes;

    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;

    using VSSonarExtension.PackageImplementation;
    using VSSonarExtension.SmartTags.BufferUpdate;

    /// <summary>
    ///     The sonar tagger.
    /// </summary>
    public partial class SonarTagger : ITagger<SonarTag>, IDisposable
    {
        #region Fields

        /// <summary>
        /// The dispatcher.
        /// </summary>
        private readonly Dispatcher dispatcher;

        /// <summary>
        /// The dirty spans var.
        /// </summary>
        private List<SnapshotSpan> dirtySpansVar;

        // ITagAggregator<IErrorTag> _naturalTextTagger;

        /// <summary>
        ///     The m disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        ///     The sonar tags.
        /// </summary>
        private volatile List<SonarTag> sonarTags = new List<SonarTag>();

        /// <summary>
        /// The timer.
        /// </summary>
        private DispatcherTimer timer;

        /// <summary>
        /// The update thread.
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
            VsSonarExtensionPackage.ExtensionModelData.PropertyChanged += this.IssuesListChanged;

            this.dispatcher = Dispatcher.CurrentDispatcher;
            this.dirtySpansVar = new List<SnapshotSpan>();

            try
            {
                this.ScheduleUpdate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problems schedulling update: " + ex.Message + "::" + ex.StackTrace);
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
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /*
        IEnumerable<ITagSpan<SonarTag>> ITagger<SonarTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var issuesInEditor = VsSonarExtensionPackage.ExtensionModelData.IssuesInEditor;

            if (spans.Count == 0 || issuesInEditor.Count == 0 || VsSonarExtensionPackage.ExtensionModelData.DisableEditorTags)
            {
                yield break;
            }

            var issuesPerLine = new List<Issue>();

            var currline = issuesInEditor[0].Line;
            var prevline = currline;

            foreach (var issue in issuesInEditor)
            {
                currline = issue.Line;

                if (currline != prevline)
                {
                    var lineToUseinVs = prevline - 1;
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

                    //var span = new SnapshotSpan(this.SourceBuffer.CurrentSnapshot, textsnapshot.Start, textsnapshot.Length);
                    var issuesToSpan = issuesPerLine;
                    issuesPerLine = new List<Issue>();

                    var mappedSpan = new SnapshotSpan(this.SourceBuffer.CurrentSnapshot, textsnapshot.Start, textsnapshot.Length);

                    yield return new TagSpan<SonarTag>(mappedSpan, new SonarTag(issuesToSpan, mappedSpan));
                }

                issuesPerLine.Add(issue);
                prevline = currline;
            }

            var lastlineToUseinVs = prevline - 1;
            if (lastlineToUseinVs < 0)
            {
                lastlineToUseinVs = 0;
            }

            ITextSnapshotLine lasttextsnapshot;

            try
            {
                lasttextsnapshot = this.SourceBuffer.CurrentSnapshot.GetLineFromLineNumber(lastlineToUseinVs);
            }
            catch (Exception)
            {
                yield break;
            }

            var lastspan = new SnapshotSpan(
                this.SourceBuffer.CurrentSnapshot, lasttextsnapshot.Start, lasttextsnapshot.Length);
            yield return
                new TagSpan<SonarTag>(new SnapshotSpan(lastspan.Start, lastspan.Length), new SonarTag(issuesPerLine, lastspan));
        }
        */

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
            List<Issue> issuesInEditor = VsSonarExtensionPackage.ExtensionModelData.IssuesInEditor;

            if (spans.Count == 0 || issuesInEditor.Count == 0 || VsSonarExtensionPackage.ExtensionModelData.DisableEditorTags)
            {
                yield break;
            }

            if (spans.Count == 0)
            {
                yield break;
            }

            List<SonarTag> tags = this.sonarTags;

            if (tags.Count == 0)
            {
                yield break;
            }

            ITextSnapshot snapshot = spans[0].Snapshot;

            foreach (SonarTag tag in tags)
            {
                ITagSpan<SonarTag> tagSpan = tag.ToTagSpan(snapshot);
                if (tagSpan.Span.Length == 0)
                {
                    continue;
                }

                if (spans.IntersectsWith(new NormalizedSnapshotSpanCollection(tagSpan.Span)))
                {
                    yield return tagSpan;
                }
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
                    VsSonarExtensionPackage.ExtensionModelData.PropertyChanged -= this.IssuesListChanged;
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
        /// The <see>
        ///     <cref>IEnumerable</cref>
        /// </see>
        ///     .
        /// </returns>
        private IEnumerable<SonarTag> GetSonarTagsInSpanForLine(List<Issue> issuesInEditor, int line)
        {
            if (issuesInEditor.Count == 0 || VsSonarExtensionPackage.ExtensionModelData.DisableEditorTags)
            {
                yield break;
            }

            var currentIssuesPerLine = issuesInEditor.Where(issue => issue.Line == line).ToList();

            var lineToUseinVs = line - 1;
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

            var mappedSpan = new SnapshotSpan(this.SourceBuffer.CurrentSnapshot, textsnapshot.Start, textsnapshot.Length);
            yield return new SonarTag(currentIssuesPerLine, mappedSpan);
        }

        /// <summary>
        /// The issues list changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void IssuesListChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == null)
                {
                    return;
                }

                if (!e.PropertyName.Equals("IssuesInEditor"))
                {
                    return;
                }

                var document = BufferTagger.GetPropertyFromBuffer<ITextDocument>(this.SourceBuffer);
                Resource resource = VsSonarExtensionPackage.ExtensionModelData.ResourceInEditor;

                if (!document.FilePath.Replace('\\', '/').EndsWith(resource.Lname, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                List<Issue> issuesInEditor = VsSonarExtensionPackage.ExtensionModelData.IssuesInEditor;

                if (issuesInEditor.Count == 0)
                {
                    var span = new SnapshotSpan(this.SourceBuffer.CurrentSnapshot, 0, this.SourceBuffer.CurrentSnapshot.Length);

                    this.dispatcher.Invoke(
                        () =>
                            {
                                EventHandler<SnapshotSpanEventArgs> temp = this.TagsChanged;
                                if (temp != null)
                                {
                                    temp(this, new SnapshotSpanEventArgs(span));
                                }
                            });

                    return;
                }

                IList<SnapshotSpan> dirtySpans = this.dirtySpansVar;
                this.dirtySpansVar = new List<SnapshotSpan>();
                this.sonarTags.Clear();

                foreach (Issue issue in issuesInEditor)
                {
                    dirtySpans.Clear();
                    ITextSnapshotLine textsnapshot = this.SourceBuffer.CurrentSnapshot.GetLineFromLineNumber(issue.Line);
                    var newDirtySpan = new SnapshotSpan(this.SourceBuffer.CurrentSnapshot, textsnapshot.Start, textsnapshot.Length);
                    dirtySpans.Add(newDirtySpan);

                    ITextSnapshot snapshot = this.SourceBuffer.CurrentSnapshot;
                    var dirty = new NormalizedSnapshotSpanCollection(dirtySpans.Select(span => span.TranslateTo(snapshot, SpanTrackingMode.EdgeInclusive)));

                    if (dirty.Count == 0)
                    {
                        Debug.WriteLine("The list of dirty spans is empty when normalized, which shouldn't be possible.");
                        return;
                    }

                    var currentSonarTag = new List<SonarTag>();
                    var newSonarTag = new List<SonarTag>();

                    var removed = currentSonarTag.RemoveAll(tag => tag.ToTagSpan(snapshot).Span.OverlapsWith(dirty[0]));
                    newSonarTag.AddRange(this.GetSonarTagsInSpanForLine(issuesInEditor, issue.Line));

                    removed += currentSonarTag.RemoveAll(tag => tag.ToTagSpan(snapshot).Span.IsEmpty);

                    if (newSonarTag.Count != 0 || removed != 0)
                    {
                        currentSonarTag.AddRange(newSonarTag);

                        this.dispatcher.Invoke(
                            () =>
                                {
                                    this.sonarTags.AddRange(currentSonarTag);

                                    var temp = this.TagsChanged;
                                    if (temp != null)
                                    {
                                        temp(this, new SnapshotSpanEventArgs(dirty[0]));
                                    }
                                });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed To Update Issues: " + ex.Message + " : " + ex.StackTrace);
            }
        }

        /// <summary>
        /// The schedule update.
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
            this.IssuesListChanged(obj, new PropertyChangedEventArgs("IssuesInEditor"));
        }

        #endregion
    }
}