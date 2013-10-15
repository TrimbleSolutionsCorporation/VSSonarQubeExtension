// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BufferTagger.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.SmartTags.BufferUpdate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    using EnvDTE;

    using ExtensionViewModel.ViewModel;

    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;

    using PackageImplementation;

    using Window = EnvDTE.Window;

    /// <summary>
    /// The sonar glyph tagger.
    /// </summary>
    public sealed class BufferTagger : ITagger<IClassificationTag>, IDisposable
    {
        #region Fields

        /// <summary>
        /// The file Path.
        /// </summary>
        private readonly string filePath;

        /// <summary>
        /// The registerevents.
        /// </summary>
        private readonly bool registerevents;

        /// <summary>
        /// The documents events.
        /// </summary>
        private readonly DocumentEvents documentsEvents;

        /// <summary>
        /// The events.
        /// </summary>
        private readonly Events events;

        /// <summary>
        /// The source buffer registered.
        /// </summary>
        private readonly bool sourceBufferRegistered;

        /// <summary>
        /// The document saved registered.
        /// </summary>
        private readonly bool documentSavedRegistered;

        /// <summary>
        /// The window activated registered.
        /// </summary>
        private readonly bool windowActivatedRegistered;

        /// <summary>
        /// The solution opened registered.
        /// </summary>
        private readonly bool solutionOpenedRegistered;

        /// <summary>
        /// The m_disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// The reference source.
        /// </summary>
        private string referenceSource;

        /// <summary>
        /// The last focus window.
        /// </summary>
        private Window lastFocusWindow;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferTagger"/> class.
        /// </summary>
        internal BufferTagger()
        {
            this.solutionOpenedRegistered = false;
            this.windowActivatedRegistered = false;
            this.documentSavedRegistered = false;
            this.sourceBufferRegistered = false;
            this.registerevents = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferTagger"/> class. 
        /// </summary>
        /// <param name="sourceBuffer">
        /// The source Buffer.
        /// </param>
        /// <param name="filePath">
        /// The file Path.
        /// </param>
        /// <param name="registerevents">
        /// The registerevents.
        /// </param>
        internal BufferTagger(ITextBuffer sourceBuffer, string filePath, bool registerevents)
        {
            if (sourceBuffer == null)
            {
                throw new ArgumentNullException("sourceBuffer");
            }

            this.filePath = filePath.Replace('\\', '/');
            if (VsSonarExtensionPackage.PluginControl.GetPluginToRunResource(this.filePath) == null)
            {
                return;
            }

            this.SourceBuffer = sourceBuffer;
            this.registerevents = registerevents;
            if (!registerevents)
            {
                return;
            }
            
            var model = VsSonarExtensionPackage.ExtensionModelData;
            var environment = model.Vsenvironmenthelper.Environment();
            this.events = environment.Events;
            this.documentsEvents = this.events.DocumentEvents;

            // save reference point
            this.referenceSource = this.SourceBuffer.CurrentSnapshot.GetText();
            this.AssociateResource(false);
            try
            {
                this.SourceBuffer.Changed += this.BufferModified;
                this.sourceBufferRegistered = true;
            }
            catch (Exception)
            {
                this.sourceBufferRegistered = false;
            }

            try
            {
                this.documentsEvents.DocumentSaved += this.DoumentSaved;
                this.documentSavedRegistered = true;
            }
            catch (Exception)
            {
                this.documentSavedRegistered = false;
            }

            try
            {
                environment.Events.WindowEvents.WindowActivated += this.WindowActivated;
                this.windowActivatedRegistered = true;
            }
            catch (Exception)
            {
                this.windowActivatedRegistered = false;
            }

            try
            {
                environment.Events.SolutionEvents.Opened += this.SolutionOpened;
                this.solutionOpenedRegistered = true;
            }
            catch (Exception)
            {
                this.solutionOpenedRegistered = false;
            }
        }

        #endregion

        #region Public Events

        /// <summary>
        /// The tags changed.
        /// </summary>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the source buffer.
        /// </summary>
        public ITextBuffer SourceBuffer { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get property from buffer.
        /// </summary>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <typeparam name="T">
        /// The tag
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public static T GetPropertyFromBuffer<T>(ITextBuffer buffer)
        {
            try
            {
                foreach (var item in buffer.Properties.PropertyList.Where(item => item.Value is T))
                {
                    return (T)item.Value;
                }
            }
            catch (Exception ex)
            {
                VsSonarExtensionPackage.ExtensionModelData.ErrorMessage = "Something Terrible Happen";
                VsSonarExtensionPackage.ExtensionModelData.DiagnosticMessage = ex.Message + "\r\n" + ex.StackTrace;
                MessageBox.Show("Oh Crap, this will probably crash: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            return default(T);
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Explicit Interface Methods

        /// <summary>
        /// This method creates SonarGlyphTag TagSpans over a set of SnapshotSpans.
        /// </summary>
        /// <param name="spans">
        /// A set of spans we want to get tags for.
        /// </param>
        /// <returns>
        /// The list of SonarGlyphTag TagSpans.
        /// </returns>
        IEnumerable<ITagSpan<IClassificationTag>> ITagger<IClassificationTag>.GetTags(
            NormalizedSnapshotSpanCollection spans)
        {
            yield break;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The buffer modified.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BufferModified(object sender, TextContentChangedEventArgs e)
        {
            try
            {
                var currentBuffer = this.SourceBuffer.CurrentSnapshot.GetText();
                VsSonarExtensionPackage.ExtensionModelData.UpdateIssuesLocationWithModifiedBuffer(currentBuffer);
            }
            catch (Exception ex)
            {
                VsSonarExtensionPackage.ExtensionModelData.ErrorMessage = "Something Terrible Happen";
                VsSonarExtensionPackage.ExtensionModelData.DiagnosticMessage = ex.Message + "\r\n" + ex.StackTrace;
            }
        }

        /// <summary>
        /// The doument saved.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        private void DoumentSaved(Document document)
        {
            try
            {
                if (document == null || !document.FullName.Replace('\\', '/').Equals(this.filePath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (VsSonarExtensionPackage.ExtensionModelData.ServerAnalysis != ExtensionDataModel.AnalysesType.Local
                    && VsSonarExtensionPackage.ExtensionModelData.ServerAnalysis
                    != ExtensionDataModel.AnalysesType.Localuser)
                {
                    return;
                }

                this.referenceSource = this.SourceBuffer.CurrentSnapshot.GetText();
                VsSonarExtensionPackage.ExtensionModelData.LastReferenceSource = this.referenceSource;
                VsSonarExtensionPackage.ExtensionModelData.UpdateDataInEditor(this.filePath, this.referenceSource);
            }
            catch (Exception ex)
            {
                VsSonarExtensionPackage.ExtensionModelData.ErrorMessage = "Something Terrible Happen";
                VsSonarExtensionPackage.ExtensionModelData.DiagnosticMessage = ex.Message + "\r\n" + ex.StackTrace;
            }
        }

        /// <summary>
        /// The solution opened.
        /// </summary>
        private void SolutionOpened()
        {
            this.AssociateResource(true);            
        }

        /// <summary>
        /// The solution opened.
        /// </summary>
        /// <param name="forceAssociation">
        /// The force Association.
        /// </param>
        private void AssociateResource(bool forceAssociation)
        {
            try
            {
                if (!forceAssociation && VsSonarExtensionPackage.ExtensionModelData.AssociatedProject != null)
                {
                    return;
                }

                VsSonarExtensionPackage.ExtensionModelData.ResetProfile();
                var solutionPath = VsSonarExtensionPackage.ExtensionModelData.Vsenvironmenthelper.ActiveSolutionPath();
                var newAssociation = VsSonarExtensionPackage.AssociateSolutionWithSonarProject(solutionPath);
                if (newAssociation == null)
                {
                    VsSonarExtensionPackage.ExtensionModelData.ErrorMessage =
                        "Cannot Associate Current File in View with Any Project, Check Sonar Settings";
                    return;
                }

                VsSonarExtensionPackage.ExtensionModelData.AssociateProjectToSolution(newAssociation.AssociatedProject);
                var currentBuffer = this.SourceBuffer.CurrentSnapshot.GetText();
                var fillePath = GetPropertyFromBuffer<ITextDocument>(this.SourceBuffer).FilePath.Replace('\\', '/');
                VsSonarExtensionPackage.ExtensionModelData.UpdateDataInEditor(fillePath, currentBuffer);
            }
            catch (Exception e)
            {
                VsSonarExtensionPackage.ExtensionModelData.ErrorMessage =
                    "Cannot Associate Solution with Any Project, Check Settings";
                VsSonarExtensionPackage.ExtensionModelData.DiagnosticMessage = e.Message + "\r\n" + e.StackTrace;
            }
        }

        /// <summary>
        /// The window activated.
        /// </summary>
        /// <param name="gotFocus">
        /// The got focus.
        /// </param>
        /// <param name="lostFocus">
        /// The lost focus.
        /// </param>
        private void WindowActivated(Window gotFocus, Window lostFocus)
        {
            if (gotFocus.Kind != "Document" || !this.filePath.Equals(gotFocus.Document.FullName.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            this.lastFocusWindow = lostFocus;

            try
            {
                var currentBuffer = this.SourceBuffer.CurrentSnapshot.GetText();
                VsSonarExtensionPackage.ExtensionModelData.UpdateDataInEditor(this.filePath.Replace('\\', '/'), currentBuffer);
            }
            catch (Exception ex)
            {
                VsSonarExtensionPackage.ExtensionModelData.ErrorMessage = "Something Terrible Happen";
                VsSonarExtensionPackage.ExtensionModelData.DiagnosticMessage = ex.Message + "\r\n" + ex.StackTrace;
            }
        }

        #endregion

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
                    if (this.registerevents)
                    {
                        var model = VsSonarExtensionPackage.ExtensionModelData;
                        var environment = model.Vsenvironmenthelper.Environment();

                        if (this.windowActivatedRegistered)
                        {
                            environment.Events.WindowEvents.WindowActivated -= this.WindowActivated;
                        }

                        if (this.documentSavedRegistered)
                        {
                            this.documentsEvents.DocumentSaved -= this.DoumentSaved;
                        }

                        if (this.solutionOpenedRegistered)
                        {
                            environment.Events.SolutionEvents.Opened -= this.SolutionOpened;
                        }

                        if (this.sourceBufferRegistered)
                        {
                            this.SourceBuffer.Changed -= this.BufferModified;
                        }

                        BufferUpdateProvider.AllBufferTaggger.Remove(this.filePath);
                    }

                    this.SourceBuffer = null;
                }

                this.isDisposed = true;
            }
        }
    }
}