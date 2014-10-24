// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSEvents.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarQubeExtension.SmartTags.BufferUpdate
{
    using System;
    using System.Drawing;
    using System.Linq;

    using EnvDTE;

    using EnvDTE80;

    using ExtensionHelpers;

    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Text;
    using VSSonarQubeExtension;

    using VSSonarPlugins;
    using VSSonarExtensionUi.ViewModel;

    /// <summary>
    ///     The vs events.
    /// </summary>
    public class VsEvents
    {
        #region Fields

        /// <summary>
        ///     The documents events.
        /// </summary>
        public readonly DocumentEvents DocumentsEvents;

        /// <summary>
        ///     The events.
        /// </summary>
        public readonly Events SolutionEvents;

        /// <summary>
        ///     The environment.
        /// </summary>
        private readonly IVsEnvironmentHelper environment;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VsEvents"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="environment">
        /// The environment.
        /// </param>
        /// <param name="dte2">
        /// The dte 2.
        /// </param>
        public VsEvents(IVsEnvironmentHelper environment, DTE2 dte2)
        {
            this.environment = environment;
            this.SolutionEvents = dte2.Events;
            this.DocumentsEvents = this.SolutionEvents.DocumentEvents;

            this.SolutionEvents.SolutionEvents.Opened += this.SolutionOpened;
            this.SolutionEvents.SolutionEvents.AfterClosing += this.SolutionClosed;
            this.SolutionEvents.WindowEvents.WindowActivated += this.WindowActivated;
            this.DocumentsEvents.DocumentSaved += this.DoumentSaved;

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;


            VsSonarExtensionPackage.SonarQubeModel.AnalysisModeHasChange += this.AnalysisModeHasChange;
            VsSonarExtensionPackage.SonarQubeModel.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.ConfigurationHasChanged += this.AnalysisModeHasChange;
        }

        private void AnalysisModeHasChange(object sender, EventArgs e)
        {
            VsSonarExtensionPackage.SonarQubeModel.RefreshDataForResource(this.LastDocumentWindowWithFocus.Document.FullName);            
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            Color defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            Color defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);

            VsSonarExtensionPackage.SonarQubeModel.UpdateTheme(VsSonarExtensionPackage.ToMediaColor(defaultBackground), VsSonarExtensionPackage.ToMediaColor(defaultForeground));

        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the last document window with focus.
        /// </summary>
        public Window LastDocumentWindowWithFocus { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get property from buffer.
        /// </summary>
        /// <param>
        /// The buffer.
        ///     <name>buffer</name>
        /// </param>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <typeparam>
        /// <name>T</name>
        /// </typeparam>
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
                VsSonarExtensionPackage.SonarQubeModel.ErrorMessage = "Something Terrible Happen";
                VsSonarExtensionPackage.SonarQubeModel.DiagnosticMessage = ex.Message + "\r\n" + ex.StackTrace;
            }

            return default(T);
        }

        #endregion

        #region Methods

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

            string text = this.environment.GetCurrentTextInView();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                VsSonarExtensionPackage.SonarQubeModel.RefreshDataForResource(document.FullName);
            }
            catch (Exception ex)
            {
                VsSonarExtensionPackage.SonarQubeModel.ErrorMessage = "Something Terrible Happen";
                VsSonarExtensionPackage.SonarQubeModel.DiagnosticMessage = ex.Message + "\r\n" + ex.StackTrace;
            }
        }

        /// <summary>
        ///     The solution closed.
        /// </summary>
        private void SolutionClosed()
        {
            VsSonarExtensionPackage.SonarQubeModel.ClearProjectAssociation();
        }

        /// <summary>
        ///     The solution opened.
        /// </summary>
        private void SolutionOpened()
        {
            var solutionName = this.environment.ActiveSolutionName();
            var solutionPath = this.environment.ActiveSolutionPath();

            VsSonarExtensionPackage.SonarQubeModel.AssociateProjectToSolution(solutionName, solutionPath);

            string text = this.environment.GetCurrentTextInView();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            string fileName = this.environment.ActiveFileFullPath();
            VsSonarExtensionPackage.SonarQubeModel.RefreshDataForResource(fileName);
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

            string text = this.environment.GetCurrentTextInView();
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
                VsSonarExtensionPackage.SonarQubeModel.RefreshDataForResource(gotFocus.Document.FullName);
            }
            catch (Exception ex)
            {
                VsSonarExtensionPackage.SonarQubeModel.ErrorMessage = "Something Terrible Happen";
                VsSonarExtensionPackage.SonarQubeModel.DiagnosticMessage = ex.Message + "\r\n" + ex.StackTrace;
            }
        }

        #endregion
    }
}