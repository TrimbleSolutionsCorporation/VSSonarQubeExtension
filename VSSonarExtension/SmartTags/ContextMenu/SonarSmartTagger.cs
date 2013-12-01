// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarSmartTagger.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.SmartTags.ContextMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using EnvDTE;

    using ExtensionTypes;

    using ExtensionViewModel.ViewModel;

    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;

    using VSSonarExtension.PackageImplementation;
    using VSSonarExtension.SmartTags.BufferUpdate;

    /// <summary>
    ///     Tagger for Spelling smart tags.
    /// </summary>
    public class SonarSmartTagger : ITagger<SonarSmartTag>, IDisposable
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
        /// The text view.
        /// </summary>
        private readonly ITextView textView;

        /// <summary>
        /// The m disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// The curr line.
        /// </summary>
        private int currLine;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarSmartTagger"/> class.
        /// </summary>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <param name="textView">
        /// The text view.
        /// </param>
        /// <param name="registerEvents">
        /// The register events.
        /// </param>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <exception>
        ///     <cref>ArgumentNullException</cref>
        /// </exception>
        public SonarSmartTagger(ITextBuffer buffer, ITextView textView, bool registerEvents, string filePath)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            this.registerEvents = registerEvents;
            this.filePath = filePath;
            this.SourceBuffer = buffer;
            if (!registerEvents)
            {
                return;
            }

            this.textView = textView;
            textView.MouseHover += this.MouseHover;
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

        /// <summary>
        /// The get tags.
        /// </summary>
        /// <param name="spans">
        /// The spans.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; Microsoft.VisualStudio.Text.Tagging.ITagSpan`1[T -&gt; VSSonarExtension.SonarTags.SonarSmartTag]].
        /// </returns>
        public IEnumerable<ITagSpan<SonarSmartTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var issuesInEditor = VsSonarExtensionPackage.ExtensionModelData.IssuesInEditor;

            if (spans.Count == 0 || issuesInEditor.Count == 0 || VsSonarExtensionPackage.ExtensionModelData.DisableEditorTags)
            {
                yield break;
            }

            var issuesPerLine = issuesInEditor.Where(issue => this.currLine == issue.Line).ToList();

            if (issuesPerLine.Count > 0)
            {
                int lineToUseinVs = this.currLine - 1;
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
                List<Issue> issuesToSpan = issuesPerLine;
                yield return
                    new TagSpan<SonarSmartTag>(
                        new SnapshotSpan(span.Start, span.Length),
                        new SonarSmartTag(this.GetSmartTagActions(issuesToSpan)));
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
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.registerEvents)
                {
                    this.textView.MouseHover -= this.MouseHover;
                    SonarSmartTaggerProvider.AllSpellingTags.Remove(this.filePath);
                }

                this.SourceBuffer = null;
            }

            this.isDisposed = true;
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

            try
            {               
                var document = BufferTagger.GetPropertyFromBuffer<ITextDocument>(this.SourceBuffer);
                var resource = VsSonarExtensionPackage.ExtensionModelData.ResourceInEditor;

                if (!document.FilePath.Replace('\\', '/').EndsWith(resource.Lname, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                lock (this.updateLock)
                {
                    var tempEvent = this.TagsChanged;
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
            catch (Exception ex)
            {
                VsSonarExtensionPackage.ExtensionModelData.ErrorMessage = "Something Terrible Happen";
                VsSonarExtensionPackage.ExtensionModelData.DiagnosticMessage = ex.Message + "\r\n" + ex.StackTrace;
            }
        }

        /// <summary>
        /// The get smart tag actions.
        /// </summary>
        /// <param name="violations">
        /// The violations.
        /// </param>
        /// <returns>
        /// The System.Collections.ObjectModel.ReadOnlyCollection`1[T -&gt; Microsoft.VisualStudio.Language.Intellisense.SmartTagActionSet].
        /// </returns>
        private ReadOnlyCollection<SmartTagActionSet> GetSmartTagActions(IEnumerable<Issue> violations)
        {
            var smartTagSets = new List<SmartTagActionSet>();

            List<ISmartTagAction> actions =
                violations.Select(
                    violation =>
                    new SonarTagDisplayViolationsAction(violation, VsSonarExtensionPackage.ExtensionModelData))
                          .Cast<ISmartTagAction>()
                          .ToList();

            if (actions.Count > 0)
            {
                smartTagSets.Add(new SmartTagActionSet(actions.AsReadOnly()));
            }

            smartTagSets.Add(new SmartTagActionSet(actions.AsReadOnly()));

            return smartTagSets.AsReadOnly();
        }

        /// <summary>
        /// The mouse hover.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MouseHover(object sender, MouseHoverEventArgs e)
        {
            var model = VsSonarExtensionPackage.ExtensionModelData;
            var environment = model.Vsenvironmenthelper.Environment();

            var objSel = environment.ActiveDocument.Selection as TextSelection;

            if (objSel != null)
            {
                if (objSel.ActivePoint.Line != this.currLine)
                {
                    this.currLine = objSel.ActivePoint.Line;
                    this.ExecuteViolationChecker();
                }
            }
        }

        #endregion
    }
}