// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarGlyphTagger.cs" company="">
//   
// </copyright>
// <summary>
//   Empty SonarGlyphTag class.
// </summary>
// ------------------------------------------------------------------------------------------------------------------
namespace TeklaOy.VSSonarExtension.SmartTags.Glyph
{
    using System;
    using System.Collections.Generic;

    using ExtensionViewModel.ViewModel;

    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;
    using TeklaOy.VSSonarExtension.PackageImplementation;
    using TeklaOy.VSSonarExtension.SmartTags.BufferUpdate;

    /// <summary>
    /// The sonar glyph tagger.
    /// </summary>
    public sealed class SonarGlyphTagger : ITagger<SonarGlyphTag>, IDisposable
    {
        /// <summary>
        /// The update lock.
        /// </summary>
        private readonly object updateLock = new object();

        /// <summary>
        /// The file path.
        /// </summary>
        private readonly string filePath;

        /// <summary>
        /// The register events.
        /// </summary>
        private readonly bool registerEvents;

        /// <summary>
        /// The is disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarGlyphTagger"/> class.
        /// </summary>
        /// <param name="sourceBuffer">
        /// The source Buffer.
        /// </param>
        /// <param name="registerEvents">
        /// The register Events.
        /// </param>
        /// <param name="filePath">
        /// The file Path.
        /// </param>
        internal SonarGlyphTagger(ITextBuffer sourceBuffer, bool registerEvents, string filePath)
        {
            if (sourceBuffer == null)
            {
                throw new ArgumentNullException("sourceBuffer");
            }

            this.filePath = filePath;
            this.registerEvents = registerEvents;
            this.SourceBuffer = sourceBuffer;
            if (registerEvents)
            {
                VsSonarExtensionPackage.ExtensionModelData.PropertyChanged += this.IssuesListChanged;
            }
        }

        /// <summary>
        /// The tags changed.
        /// </summary>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        /// <summary>
        /// Gets or sets the source buffer.
        /// </summary>
        public ITextBuffer SourceBuffer { get; set; }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This method creates SonarGlyphTag TagSpans over a set of SnapshotSpans.
        /// </summary>
        /// <param name="spans">A set of spans we want to get tags for.</param>
        /// <returns>The list of SonarGlyphTag TagSpans.</returns>
        IEnumerable<ITagSpan<SonarGlyphTag>> ITagger<SonarGlyphTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var issuesInEditor = VsSonarExtensionPackage.ExtensionModelData.IssuesInEditor;

            if (spans.Count == 0 || issuesInEditor.Count == 0 || VsSonarExtensionPackage.ExtensionModelData.DisableEditorTags)
            {
                yield break;
            }

            if (VsSonarExtensionPackage.ExtensionModelData.ServerAnalysis == ExtensionDataModel.AnalysesType.Off)
            {
                yield break;
            }

            var prevline = issuesInEditor[0].Line;

            foreach (var issue in issuesInEditor)
            {
                if (prevline != issue.Line)
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
                    yield return new TagSpan<SonarGlyphTag>(new SnapshotSpan(span.Start, span.Length), new SonarGlyphTag());
                }

                prevline = issue.Line;
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

            var lastspan = new SnapshotSpan(this.SourceBuffer.CurrentSnapshot, lasttextsnapshot.Start, lasttextsnapshot.Length);
            yield return new TagSpan<SonarGlyphTag>(new SnapshotSpan(lastspan.Start, lastspan.Length), new SonarGlyphTag());
        }

        /// <summary>
        /// The execute violation checker.
        /// </summary>
        private void ExecuteViolationChecker()
        {
            if (VsSonarExtensionPackage.ExtensionModelData == null ||
                VsSonarExtensionPackage.ExtensionModelData.ResourceInEditor == null)
            {
                return;
            }

            var document = BufferTagger.GetPropertyFromBuffer<ITextDocument>(this.SourceBuffer);
            var resource = VsSonarExtensionPackage.ExtensionModelData.ResourceInEditor;

            if (!document.FilePath.Replace("\\", "/").EndsWith(resource.Lname))
            {
                return;
            }

            lock (this.updateLock)
            {
                var tempEvent = this.TagsChanged;
                if (tempEvent != null)
                {
                    tempEvent(this, new SnapshotSpanEventArgs(new SnapshotSpan(this.SourceBuffer.CurrentSnapshot, 0, this.SourceBuffer.CurrentSnapshot.Length)));
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
        private void IssuesListChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IssuesInEditor"))
            {
                this.ExecuteViolationChecker();
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
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    if (this.registerEvents)
                    {
                        VsSonarExtensionPackage.ExtensionModelData.PropertyChanged -= this.IssuesListChanged;
                        SonarGlyphProvider.AllSpellingTags.Remove(this.filePath);
                    }

                    this.SourceBuffer = null;
                }

                this.isDisposed = true;
            }
        }
    }
}
