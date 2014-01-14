// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HighlightCoverageTagger.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.SmartTags.Coverage
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

    using VSSonarExtension.MainViewModel.Cache;
    using VSSonarExtension.PackageImplementation;
    using VSSonarExtension.SmartTags.BufferUpdate;

    /// <summary>
    ///     This tagger will provide tags for every word in the buffer that
    ///     matches the word currently under the cursor.
    /// </summary>
    public class HighlightCoverageTagger : ITagger<CoverageTag>, IDisposable
    {
        #region Fields

        private volatile List<CoverageTag> coverageTags = new List<CoverageTag>();

        /// <summary>
        /// The dispatcher.
        /// </summary>
        private readonly Dispatcher dispatcher;

        /// <summary>
        /// The dirty spans var.
        /// </summary>
        private List<SnapshotSpan> dirtySpansVar;

        /// <summary>
        /// The m disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        ///     The temp coverage line.
        /// </summary>
        private Dictionary<int, string> tempCoverageLine;

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
        /// Initializes a new instance of the <see cref="HighlightCoverageTagger"/> class.
        /// </summary>
        /// <param name="sourceBuffer">
        /// The source buffer.
        /// </param>
        public HighlightCoverageTagger(ITextBuffer sourceBuffer)
        {
            this.SourceBuffer = sourceBuffer;
            VsSonarExtensionPackage.ExtensionModelData.PropertyChanged += this.CoverageDataChanged;

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
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; Microsoft.VisualStudio.Text.Tagging.ITagSpan`1[T -&gt; SmartTags.Coverage.HighlightCoverageTag]].
        /// </returns>
        public IEnumerable<ITagSpan<CoverageTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            if (!VsSonarExtensionPackage.ExtensionModelData.CoverageInEditorEnabled)
            {
                yield break;
            }

            if (this.coverageTags.Count == 0)
            {
                yield break;
            }

            ITextSnapshot snapshot = spans[0].Snapshot;

            foreach (var tag in this.coverageTags)
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
        private void CoverageDataChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == null)
                {
                    return;
                }

                if (!e.PropertyName.Equals("CoverageInEditor"))
                {
                    return;
                }

                var document = VsEvents.GetPropertyFromBuffer<ITextDocument>(this.SourceBuffer);
                var resource = VsSonarExtensionPackage.ExtensionModelData.ResourceInEditor;

                if (resource == null || document == null)
                {
                    return;
                }


                if (!document.FilePath.Replace('\\', '/').EndsWith(resource.Lname, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var coverageLine = VsSonarExtensionPackage.ExtensionModelData.GetCoverageInEditor(this.SourceBuffer.CurrentSnapshot.GetText());
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
                Debug.WriteLine("Failed To Update Issues: " + ex.Message + " : " + ex.StackTrace);
            }
        }

        /// <summary>
        /// The refresh tags.
        /// </summary>
        private void RefreshTags()
        {
            this.dispatcher.Invoke(
            () =>
            {
                var tempEvent = this.TagsChanged;
                if (tempEvent != null)
                {
                    tempEvent(
                        this,
                        new SnapshotSpanEventArgs(
                            new SnapshotSpan(this.SourceBuffer.CurrentSnapshot, 0, this.SourceBuffer.CurrentSnapshot.Length)));
                }
            });
        }

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

            var lineToUseinVs = line;
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
                    VsSonarExtensionPackage.ExtensionModelData.PropertyChanged -= this.CoverageDataChanged;

                this.SourceBuffer = null;
            }

            this.isDisposed = true;
        }

        /// <summary>
        /// The update data after constructor.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void UpdateDataAfterConstructor(object obj)
        {
            this.CoverageDataChanged(obj, new PropertyChangedEventArgs("CoverageInEditor"));
        }

        #endregion
    }
}