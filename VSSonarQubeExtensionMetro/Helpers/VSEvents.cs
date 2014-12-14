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

namespace VSSonarQubeExtension.Helpers
{
    using System;
    using System.Drawing;
    using System.Linq;

    using EnvDTE;

    using EnvDTE80;

    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Text;

    using VSSonarPlugins;

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

        private VsSonarExtensionPackage package;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VsEvents"/> class.
        /// </summary>
        /// <param name="environment">
        ///     The environment.
        /// </param>
        /// <param name="dte2">
        ///     The dte 2.
        /// </param>
        /// <param name="vsSonarExtensionPackage"></param>
        public VsEvents(IVsEnvironmentHelper environment, DTE2 dte2, VsSonarExtensionPackage vsSonarExtensionPackage)
        {
            this.package = vsSonarExtensionPackage;
            this.environment = environment;
            this.SolutionEvents = dte2.Events;
            this.visualStudioEvents = dte2.Events.DTEEvents;
            this.DocumentsEvents = this.SolutionEvents.DocumentEvents;

            this.SolutionEvents.SolutionEvents.Opened += this.SolutionOpened;
            this.SolutionEvents.SolutionEvents.AfterClosing += this.SolutionClosed;
            this.SolutionEvents.WindowEvents.WindowActivated += this.WindowActivated;
            this.SolutionEvents.WindowEvents.WindowClosing += this.WindowClosed;
            this.DocumentsEvents.DocumentSaved += this.DoumentSaved;
            this.visualStudioEvents.OnStartupComplete += this.CloseToolWindows;

            VSColorTheme.ThemeChanged += this.VSColorTheme_ThemeChanged;

            SonarQubeViewModelFactory.SQViewModel.AnalysisModeHasChange += this.AnalysisModeHasChange;
            SonarQubeViewModelFactory.SQViewModel.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.ConfigurationHasChanged +=
                this.AnalysisModeHasChange;
        }

        private void CloseToolWindows()
        {
            this.package.CloseToolWindow();
        }

        private DTEEvents visualStudioEvents { get; set; }

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
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <typeparam>
        ///     <name>T</name>
        /// </typeparam>
        /// <returns>
        /// The <see>
        ///         <cref>T</cref>
        ///     </see>
        ///     .
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
                SonarQubeViewModelFactory.SQViewModel.ErrorMessage = "Something Terrible Happen";
                SonarQubeViewModelFactory.SQViewModel.DiagnosticMessage = ex.Message + "\r\n" + ex.StackTrace;
            }

            return default(T);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The analysis mode has change.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void AnalysisModeHasChange(object sender, EventArgs e)
        {
            SonarQubeViewModelFactory.SQViewModel.RefreshDataForResource(this.LastDocumentWindowWithFocus.Document.FullName);
        }

        /// <summary>
        /// The doument saved.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        private void DoumentSaved(Document document)
        {
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessage("DoumentSaved : " + document);
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
                SonarQubeViewModelFactory.SQViewModel.RefreshDataForResource(document.FullName);
            }
            catch (Exception ex)
            {
                SonarQubeViewModelFactory.SQViewModel.ErrorMessage = "Something Terrible Happen";
                SonarQubeViewModelFactory.SQViewModel.DiagnosticMessage = ex.Message + "\r\n" + ex.StackTrace;
            }
        }

        /// <summary>
        ///     The solution closed.
        /// </summary>
        private void SolutionClosed()
        {
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessage("Solution Closed");
            SonarQubeViewModelFactory.SQViewModel.ClearProjectAssociation();            
        }

        /// <summary>
        ///     The solution opened.
        /// </summary>
        private void SolutionOpened()
        {
            
            string solutionName = this.environment.ActiveSolutionName();
            string solutionPath = this.environment.ActiveSolutionPath();

            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessage("Solution Opened: " + solutionName + " : " + solutionPath);
            SonarQubeViewModelFactory.SQViewModel.AssociateProjectToSolution(solutionName, solutionPath);

            string text = this.environment.GetCurrentTextInView();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            string fileName = this.environment.ActiveFileFullPath();
            SonarQubeViewModelFactory.SQViewModel.RefreshDataForResource(fileName);
        }

        /// <summary>
        /// The vs color theme_ theme changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            Color defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            Color defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);

            SonarQubeViewModelFactory.SQViewModel.UpdateTheme(
                VsSonarExtensionPackage.ToMediaColor(defaultBackground), 
                VsSonarExtensionPackage.ToMediaColor(defaultForeground));
        }

        private void WindowClosed(Window window)
        {
            if (window.Kind != "Document")
            {
                return;
            }

            if (window.Document == null)
            {
                SonarQubeViewModelFactory.SQViewModel.ClosedWindow(window.Caption);
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
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessage("Window Activated: Kind: " + gotFocus.Kind);

            if (gotFocus.Kind != "Document")
            {
                return;
            }


            try
            {
                if (this.LastDocumentWindowWithFocus == gotFocus)
                {
                    SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessage("Last and Current Window are the same");
                    return;
                }

                SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessage("New Document Open: " + gotFocus.Document.FullName);

                this.LastDocumentWindowWithFocus = gotFocus;
                SonarQubeViewModelFactory.SQViewModel.RefreshDataForResource(gotFocus.Document.FullName);
            }
            catch (Exception ex)
            {
                SonarQubeViewModelFactory.SQViewModel.Logger.WriteException(ex);
                SonarQubeViewModelFactory.SQViewModel.ErrorMessage = "Something Terrible Happen";
                SonarQubeViewModelFactory.SQViewModel.DiagnosticMessage = ex.Message + "\r\n" + ex.StackTrace;
            }
        }

        #endregion
    }
}