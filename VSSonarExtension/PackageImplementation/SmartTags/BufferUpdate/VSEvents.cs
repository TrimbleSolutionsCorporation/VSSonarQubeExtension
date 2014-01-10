// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSEvents.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Linq;
    using System.Windows;

    using EnvDTE;

    using EnvDTE80;

    using ExtensionHelpers;

    using ExtensionTypes;

    using Microsoft.VisualStudio.Text;

    using VSSonarExtension.MainViewModel.ViewModel;
    using VSSonarExtension.PackageImplementation;

    using VSSonarPlugins;

    using Window = EnvDTE.Window;

    /// <summary>
    /// The vs events.
    /// </summary>
    public class VsEvents
    {
        /// <summary>
        /// The environment.
        /// </summary>
        private readonly IVsEnvironmentHelper environment;

        /// <summary>
        /// The model.
        /// </summary>
        private readonly ExtensionDataModel model;

        /// <summary>
        /// The documents events.
        /// </summary>
        private readonly DocumentEvents documentsEvents;

        /// <summary>
        /// The events.
        /// </summary>
        private readonly Events events;

        /// <summary>
        /// Initializes a new instance of the <see cref="VsEvents"/> class.
        /// </summary>
        /// <param name="model">
        ///     The model.
        /// </param>
        /// <param name="environment">
        ///     The environment.
        /// </param>
        /// <param name="dte2"></param>
        public VsEvents(ExtensionDataModel model, IVsEnvironmentHelper environment, DTE2 dte2)
        {
            this.model = model;
            this.environment = environment;
            this.events = dte2.Events;
            this.documentsEvents = this.events.DocumentEvents;

            this.events.SolutionEvents.Opened += this.SolutionOpened;
            this.events.SolutionEvents.AfterClosing += this.SolutionClosed;
            this.events.WindowEvents.WindowActivated += this.WindowActivated;
            this.documentsEvents.DocumentSaved += this.DoumentSaved;
        }

        /// <summary>
        /// The doument saved.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        private void DoumentSaved(Document document)
        {
            if (document == null)
            {
                return;
            }

            var text = this.environment.GetCurrentTextInView();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                this.model.RefreshDataForResource(document.FullName);
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
            this.model.AssociateProjectToSolution();

            var text = this.environment.GetCurrentTextInView();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var fileName = this.environment.ActiveFileFullPath();
            this.model.RefreshDataForResource(fileName);
        }

        /// <summary>
        /// The get property from buffer.
        /// </summary>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
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
            }

            return default(T);
        }



        /// <summary>
        /// The solution closed.
        /// </summary>
        private void SolutionClosed()
        {
            this.model.ClearProjectAssociation();
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
            if (gotFocus.Kind != "Document")
            {
                return;
            }

            var text = this.environment.GetCurrentTextInView();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (this.LastDocumentWindowWithFocus == gotFocus)
            {
                return;
            }

            try
            {
                this.LastDocumentWindowWithFocus = gotFocus;
                this.model.RefreshDataForResource(gotFocus.Document.FullName);
            }
            catch (Exception ex)
            {
                this.model.ErrorMessage = "Something Terrible Happen";
                this.model.DiagnosticMessage = ex.Message + "\r\n" + ex.StackTrace;
            }
        }

        /// <summary>
        /// Gets or sets the last document window with focus.
        /// </summary>
        public Window LastDocumentWindowWithFocus { get; set; }
    }
}
