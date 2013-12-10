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

    using ExtensionHelpers;

    using ExtensionTypes;

    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;

    using PackageImplementation;

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
        /// The source buffer registered.
        /// </summary>
        private readonly bool sourceBufferRegistered;

        /// <summary>
        /// The m_disposed.
        /// </summary>
        private bool isDisposed;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferTagger"/> class.
        /// </summary>
        internal BufferTagger()
        {
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
            var vsinter = VsSonarExtensionPackage.ExtensionModelData.Vsenvironmenthelper;
            var restService = VsSonarExtensionPackage.ExtensionModelData.RestService;
            var conf = ConnectionConfigurationHelpers.GetConnectionConfiguration(vsinter, restService);
            if (VsSonarExtensionPackage.PluginControl.GetPluginToRunResource(conf, this.filePath) == null)
            {
                return;
            }

            this.SourceBuffer = sourceBuffer;
            this.registerevents = registerevents;
            if (!registerevents)
            {
                return;
            }
            
            try
            {
                this.SourceBuffer.Changed += this.BufferModified;
                this.sourceBufferRegistered = true;
            }
            catch (Exception)
            {
                this.sourceBufferRegistered = false;
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
        /// The validate project key.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static Resource AssociateSolutionWithSonarProject()
        {
            var vsinter = VsSonarExtensionPackage.ExtensionModelData.Vsenvironmenthelper;
            var restService = VsSonarExtensionPackage.ExtensionModelData.RestService;
            var conf = ConnectionConfigurationHelpers.GetConnectionConfiguration(vsinter, restService);
            var solutionName = vsinter.ActiveSolutionName();
            var key = vsinter.ReadOptionFromApplicationData(solutionName, "PROJECTKEY");

            if (!string.IsNullOrEmpty(key))
            {
                return restService.GetResourcesData(conf, key)[0];
            }

            var solutionPath = vsinter.ActiveSolutionPath();
            key = VsSonarUtils.GetProjectKey(solutionPath);
            vsinter.WriteOptionInApplicationData(solutionName, "PROJECTKEY", key);

            return string.IsNullOrEmpty(key) ? null : restService.GetResourcesData(conf, key)[0];
        }

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
                VsSonarExtensionPackage.ExtensionModelData.UpdateIssuesInEditorLocationWithModifiedBuffer(currentBuffer);
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