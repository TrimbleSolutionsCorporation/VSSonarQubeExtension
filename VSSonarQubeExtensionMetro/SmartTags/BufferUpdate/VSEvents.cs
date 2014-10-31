// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSEvents.cs" company="">
//   
// </copyright>
// <summary>
//   The vs events.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarQubeExtension.SmartTags.BufferUpdate
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

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VsEvents"/> class.
        /// </summary>
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

            VSColorTheme.ThemeChanged += this.VSColorTheme_ThemeChanged;

            VsSonarExtensionPackage.SonarQubeModel.AnalysisModeHasChange += this.AnalysisModeHasChange;
            VsSonarExtensionPackage.SonarQubeModel.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.ConfigurationHasChanged +=
                this.AnalysisModeHasChange;
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
                VsSonarExtensionPackage.SonarQubeModel.ErrorMessage = "Something Terrible Happen";
                VsSonarExtensionPackage.SonarQubeModel.DiagnosticMessage = ex.Message + "\r\n" + ex.StackTrace;
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
            VsSonarExtensionPackage.SonarQubeModel.RefreshDataForResource(this.LastDocumentWindowWithFocus.Document.FullName);
        }

        /// <summary>
        /// The doument saved.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        private void DoumentSaved(Document document)
        {
            VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("DoumentSaved : " + document);
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
            VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("Solution Closed");
            VsSonarExtensionPackage.SonarQubeModel.ClearProjectAssociation();            
        }

        /// <summary>
        ///     The solution opened.
        /// </summary>
        private void SolutionOpened()
        {
            
            string solutionName = this.environment.ActiveSolutionName();
            string solutionPath = this.environment.ActiveSolutionPath();

            VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("Solution Opened: " + solutionName + " : " + solutionPath);
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
        /// The vs color theme_ theme changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            Color defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            Color defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);

            VsSonarExtensionPackage.SonarQubeModel.UpdateTheme(
                VsSonarExtensionPackage.ToMediaColor(defaultBackground), 
                VsSonarExtensionPackage.ToMediaColor(defaultForeground));
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
            VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("Window Activated: Kind: " + gotFocus.Kind);

            if (gotFocus.Kind != "Document")
            {
                return;
            }

            string text = this.environment.GetCurrentTextInView();
            if (string.IsNullOrEmpty(text))
            {
                VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("Text In Window Is Empty");
                return;
            }

            if (this.LastDocumentWindowWithFocus == gotFocus)
            {
                VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("Last and Current Window are the same");
                return;
            }

            VsSonarExtensionPackage.SonarQubeModel.Logger.WriteMessage("New Document Open: " + gotFocus.Document.FullName);

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