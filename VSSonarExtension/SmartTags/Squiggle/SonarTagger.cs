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

    using ExtensionTypes;

    using ExtensionViewModel.ViewModel;

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
        ///     The file path.
        /// </summary>
        private readonly string filePath;

        /// <summary>
        ///     The register events.
        /// </summary>
        private readonly bool registerEvents;

        /// <summary>
        ///     The update lock.
        /// </summary>
        private readonly object updateLock = new object();

        /// <summary>
        /// The m disposed.
        /// </summary>
        private bool isDisposed;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarTagger"/> class.
        /// </summary>
        /// <param name="sourceBuffer">
        /// The source buffer.
        /// </param>
        /// <param name="registerEvents">
        /// The register Events.
        /// </param>
        /// <param name="filePath">
        /// The file Path.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// source buffer
        /// </exception>
        public SonarTagger(ITextBuffer sourceBuffer, bool registerEvents, string filePath)
        {
            if (sourceBuffer == null)
            {
                throw new ArgumentNullException("sourceBuffer");
            }

            this.filePath = filePath;
            this.SourceBuffer = sourceBuffer;
            this.registerEvents = registerEvents;
            if (registerEvents)
            {
                VsSonarExtensionPackage.ExtensionModelData.PropertyChanged += this.IssuesListChanged;
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

        #endregion

        #region Explicit Interface Methods

        /// <summary>
        /// The get tags.
        /// </summary>
        /// <param name="spans">
        /// The spans.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; Microsoft.VisualStudio.Text.Tagging.ITagSpan`1[T -&gt; SmartTags.SonarTag]].
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// spans are null
        /// </exception>
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

                    var span = new SnapshotSpan(this.SourceBuffer.CurrentSnapshot, textsnapshot.Start, textsnapshot.Length);
                    var issuesToSpan = issuesPerLine;
                    issuesPerLine = new List<Issue>();
                    yield return new TagSpan<SonarTag>(new SnapshotSpan(span.Start, span.Length), new SonarTag(issuesToSpan));
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
                new TagSpan<SonarTag>(new SnapshotSpan(lastspan.Start, lastspan.Length), new SonarTag(issuesPerLine));
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
                    if (this.registerEvents)
                    {
                        VsSonarExtensionPackage.ExtensionModelData.PropertyChanged -= this.IssuesListChanged;
                        SonarTaggerProvider.AllSpellingTags.Remove(this.filePath);
                    }

                    this.SourceBuffer = null;
                }

                this.isDisposed = true;
            }
        }

        /// <summary>
        ///     The execute violation checker.
        /// </summary>
        private void ExecuteViolationChecker()
        {
            if (VsSonarExtensionPackage.ExtensionModelData == null
                || VsSonarExtensionPackage.ExtensionModelData.ResourceInEditor == null)
            {
                return;
            }

            var document = BufferTagger.GetPropertyFromBuffer<ITextDocument>(this.SourceBuffer);
            var resource = VsSonarExtensionPackage.ExtensionModelData.ResourceInEditor;

            if (!document.FilePath.Replace('\\', '/').EndsWith(resource.Lname, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            lock (this.updateLock)
            {
                EventHandler<SnapshotSpanEventArgs> tempEvent = this.TagsChanged;
                if (tempEvent != null)
                {
                    tempEvent(
                        this, 
                        new SnapshotSpanEventArgs(
                            new SnapshotSpan(
                                this.SourceBuffer.CurrentSnapshot, 0, this.SourceBuffer.CurrentSnapshot.Length)));
                }
            }
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
            if (e.PropertyName.Equals("IssuesInEditor"))
            {
                this.ExecuteViolationChecker();
            }
        }

        #endregion
    }
}