// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HighlightCoverageTagger.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarQubeExtension.SmartTags.Coverage
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows.Threading;

    using ExtensionTypes;

    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;

    using VSSonarExtensionUi.Cache;

    using VSSonarQubeExtension.SmartTags.BufferUpdate;

    /// <summary>
    ///     This tagger will provide tags for every word in the buffer that
    ///     matches the word currently under the cursor.
    /// </summary>
    public class HighlightCoverageTagger : ITagger<CoverageTag>, IDisposable
    {
        #region Fields

        /// <summary>
        ///     The coverage tags.
        /// </summary>
        private volatile List<CoverageTag> coverageTags = new List<CoverageTag>();

        /// <summary>
        ///     The m disposed.
        /// </summary>
        private bool isDisposed;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightCoverageTagger"/> class.
        /// </summary>
        /// <param name="sourceBuffer">
        /// The source buffer.
        /// </param>
        public HighlightCoverageTagger(ITextBuffer sourceBuffer)
        {
            try
            {
                this.SourceBuffer = sourceBuffer;
                VsSonarExtensionPackage.SonarQubeModel.AnalysisModeHasChange += this.CoverageDataChanged;
                VsSonarExtensionPackage.SonarQubeModel.ServerViewModel.CoverageWasModified += this.CoverageDataChanged;
                ThreadPool.QueueUserWorkItem(this.ScheduleUpdate, null);
            }
            catch (Exception ex)
            {
                VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("Problems schedulling update: " + ex.Message + "::" + ex.StackTrace);
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
        ///     Gets or sets the source buffer.
        /// </summary>
        public ITextBuffer SourceBuffer { get; set; }

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

        /// <summary>
        /// The get tags.
        /// </summary>
        /// <param name="spans">
        /// The spans.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; Microsoft.VisualStudio.Text.Tagging.ITagSpan`1[T -&gt;
        ///     SmartTags.Coverage.HighlightCoverageTag]].
        /// </returns>
        public IEnumerable<ITagSpan<CoverageTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            if (!VsSonarExtensionPackage.SonarQubeModel.ServerViewModel.CoverageInEditorEnabled)
            {
                yield break;
            }

            if (this.coverageTags.Count == 0)
            {
                yield break;
            }

            ITextSnapshot snapshot = spans[0].Snapshot;

            foreach (CoverageTag tag in this.coverageTags)
            {
                ITagSpan<CoverageTag> tagSpan = tag.ToTagSpan(snapshot);
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
        /// Sets the coverage data.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CoverageDataChanged(object sender, EventArgs e)
        {
            try
            {
                VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("Coverage Changed Event Correctly Triggered");

                var document = VsEvents.GetPropertyFromBuffer<ITextDocument>(this.SourceBuffer);
                Resource resource = VsSonarExtensionPackage.SonarQubeModel.ResourceInEditor;

                if (resource == null || document == null)
                {
                    return;
                }

                if (!document.FilePath.Replace('\\', '/').EndsWith(resource.Lname, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("Getting Coverage");
                Dictionary<int, CoverageElement> coverageLine =
                    VsSonarExtensionPackage.SonarQubeModel.ServerViewModel.GetCoverageInEditor(this.SourceBuffer.CurrentSnapshot.GetText());

                VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("Got Cov Measures: " + coverageLine.Count);
                this.coverageTags.Clear();

                if (coverageLine.Count == 0)
                {
                    this.RefreshTags();
                    return;
                }

                foreach (var hit in coverageLine)
                {
                    this.coverageTags.AddRange(this.GetCoverageTagsInSpanForLine(coverageLine, hit.Key));
                }

                this.RefreshTags();
            }
            catch (Exception ex)
            {
                VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("Failed To Update Issues: " + ex.Message + " : " + ex.StackTrace);
            }
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("Dispose File");
                VsSonarExtensionPackage.SonarQubeModel.AnalysisModeHasChange -= this.CoverageDataChanged;
                VsSonarExtensionPackage.SonarQubeModel.ServerViewModel.CoverageWasModified -= this.CoverageDataChanged;

                this.SourceBuffer = null;
            }

            this.isDisposed = true;
        }

        /// <summary>
        /// The get coverage tags in span for line.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="line">
        /// The line.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        private IEnumerable<CoverageTag> GetCoverageTagsInSpanForLine(Dictionary<int, CoverageElement> data, int line)
        {
            if (data.Count == 0)
            {
                yield break;
            }

            if (!data.ContainsKey(line))
            {
                yield break;
            }

            int lineToUseinVs = line;
            if (lineToUseinVs < 0)
            {
                yield break;
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
            yield return new CoverageTag(mappedSpan, data[line].ToString());
        }

        /// <summary>
        ///     The refresh tags.
        /// </summary>
        private void RefreshTags()
        {
            Dispatcher.CurrentDispatcher.Invoke(
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
        private void ScheduleUpdate(object state)
        {
            VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("Schedulle Update After Constructor");
            Thread.Sleep(2);
            this.CoverageDataChanged(state, EventArgs.Empty);
        }

        #endregion
    }
}