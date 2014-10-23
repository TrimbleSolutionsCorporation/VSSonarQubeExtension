// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSSonarQubeExtensionPackage.cs" company="">
//   
// </copyright>
// <summary>
//   The vs sonar extension package.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarQubeExtension
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows.Controls;

    using EnvDTE;

    using EnvDTE80;

    using ExtensionHelpers;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using SonarRestService;

    using VSSonarExtensionUi.View.Helpers;
    using VSSonarExtensionUi.ViewModel;

    using VSSonarQubeExtension.SmartTags.BufferUpdate;
    using VSSonarQubeExtension.SmartTags.StatusBar;
    using VSSonarQubeExtension.VSControls;

    using DColor = System.Drawing.Color;
    using MColor = System.Windows.Media.Color;

    /// <summary>
    ///     The vs sonar extension package.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad("{05ca2046-1eb1-4813-a91f-a69df9b27698}")]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideToolWindow(typeof(IssuesToolWindow), Style = VsDockStyle.Tabbed, Window = "ac305e7a-a44b-4541-8ece-3c88d5425338")]
    [ProvideToolWindow(typeof(PluginToolWindow), Style = VsDockStyle.Tabbed, MultiInstances = true)]
    [Guid(GuidList.GuidVSSonarExtensionPkgString)]
    public sealed partial class VsSonarExtensionPackage : Package
    {
        #region Static Fields

        /// <summary>
        ///     The issue model.
        /// </summary>
        public static readonly SonarQubeViewModel SonarQubeModel = new SonarQubeViewModel();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="VsSonarExtensionPackage" /> class.
        /// </summary>
        public VsSonarExtensionPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this));
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the status bar.
        /// </summary>
        public VSSStatusBar StatusBar { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The to media color.
        /// </summary>
        /// <param name="color">
        /// The color.
        /// </param>
        /// <returns>
        /// The <see cref="Color"/>.
        /// </returns>
        public static MColor ToMediaColor(DColor color)
        {
            return MColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        ///     The show tool window.
        /// </summary>
        public void CloseToolWindow()
        {
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    // Find existing windows. 
                    ToolWindowPane currentWindow = this.FindToolWindow(typeof(PluginToolWindow), i, false);
                    if (currentWindow == null)
                    {
                        continue;
                    }

                    var windowFrame = (IVsWindowFrame)currentWindow.Frame;
                    ErrorHandler.ThrowOnFailure(windowFrame.Hide());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// The show tool window.
        /// </summary>
        /// <param name="control">
        /// The control.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <exception>
        /// <cref>NotSupportedException</cref>
        /// </exception>
        public void ShowToolWindow(UserControl control, int id, string name)
        {
            // Find existing windows. 
            ToolWindowPane currentWindow = this.FindToolWindow(typeof(PluginToolWindow), id, false);
            if (currentWindow != null)
            {
                return;
            }

            // Create the window with the first free ID.
            PluginToolWindow.CurrentPluginControl = control;
            PluginToolWindow.CurrentPluginName = name;
            var window = (ToolWindowPane)this.CreateToolWindow(typeof(PluginToolWindow), id);

            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("CAnnot Create Tool");
            }

            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The initialize.
        /// </summary>
        protected override void Initialize()
        {
            try
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this));
                base.Initialize();

                this.SetupMenuCommands();
                this.dte2 = (DTE2)GetGlobalService(typeof(DTE));

                if (this.dte2 == null)
                {
                    return;
                }

                try
                {
                    this.visualStudioInterface = new VsPropertiesHelper(this.dte2, this);
                    this.restService = new SonarRestService(new JsonSonarConnector());
                    this.VsEvents = new VsEvents(this.visualStudioInterface, this.dte2);
                    var bar = this.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
                    this.StatusBar = new VSSStatusBar(bar, this.dte2);

                    SonarQubeModel.ExtensionDataModelUpdate(this.restService, this.visualStudioInterface, this.StatusBar, this);

                    DColor defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
                    DColor defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);

                    SonarQubeModel.UpdateTheme(ToMediaColor(defaultBackground), ToMediaColor(defaultForeground));

                    try
                    {
                        var outWindow = GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                        var customGuid = new Guid("CDA8E85D-C469-4855-878B-0E778CD0DD53");
                        if (outWindow != null)
                        {
                            outWindow.CreatePane(ref customGuid, "VSSonarQube Output", 1, 1);
                            IVsOutputWindowPane customPane;
                            outWindow.GetPane(ref customGuid, out customPane);
                            ((VsPropertiesHelper)this.visualStudioInterface).CustomPane = customPane;
                            ((VsPropertiesHelper)this.visualStudioInterface).CustomPane.Activate();
                        }

                        // this.UpdateModelInToolWindow(SonarQubeModel);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    UserExceptionMessageBox.ShowException("SonarQubeExtension not able to start", ex);
                }
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Extension Failed to Start", ex);
                throw;
            }
        }

        /// <summary>
        /// The analyse solution cmd.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void AnalyseSolutionCmd(object sender, EventArgs e)
        {
            SonarQubeModel.LocalViewModel.OnPreviewCommand();
        }

        /// <summary>
        ///     The setup menu commands.
        /// </summary>
        private void SetupMenuCommands()
        {
            var mcs = this.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (null == mcs)
            {
                return;
            }

            var menuCommandId = new CommandID(GuidList.GuidVSSonarExtensionCmdSet, (int)PkgCmdIdList.CmdidReviewsCommand);
            this.sonarReviewsCommand = new OleMenuCommand(this.ShowIssuesToolWindow, menuCommandId);
            mcs.AddCommand(this.sonarReviewsCommand);

            menuCommandId = new CommandID(GuidList.GuidShowInitialToolbarCmdSet, (int)PkgCmdIdList.ToolBarReportReviews);
            this.sonarReviewsCommandBar = new OleMenuCommand(this.ShowIssuesToolWindow, menuCommandId);
            mcs.AddCommand(this.sonarReviewsCommandBar);

            // CONTEXT MENUS
            menuCommandId = new CommandID(GuidList.GuidStartAnalysisSolutionCTXCmdSet, PkgCmdIdList.CmdidRunAnalysisInSolution);
            this.runAnalysisCmd = new OleMenuCommand(this.AnalyseSolutionCmd, menuCommandId);
            mcs.AddCommand(this.runAnalysisCmd);

            menuCommandId = new CommandID(GuidList.GuidStartAnalysisSolutionCTXCmdSet, PkgCmdIdList.CmdidRunAnalysisInProject);
            this.runAnalysisInProjectCmd = new OleMenuCommand(this.ShowIssuesToolWindow, menuCommandId);
            mcs.AddCommand(this.runAnalysisInProjectCmd);
        }

        /// <summary>
        /// The show issues tool window.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <exception>
        /// <cref>NotSupportedException</cref>
        /// </exception>
        private void ShowIssuesToolWindow(object sender, EventArgs e)
        {
            IVsWindowFrame windowFrame;

            using (ToolWindowPane window = this.FindToolWindow(typeof(IssuesToolWindow), 0, true))
            {
                if ((null == window) || (null == window.Frame))
                {
                    throw new NotSupportedException("Window Not Found");
                }

                windowFrame = (IVsWindowFrame)window.Frame;
            }

            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        /// <summary>
        /// The update model in tool window.
        /// </summary>
        /// <param name="modelToUse">
        /// The model to use.
        /// </param>
        private void UpdateModelInToolWindow(SonarQubeViewModel modelToUse)
        {
            // init basic model without project association
            ToolWindowPane window = this.FindToolWindow(typeof(IssuesToolWindow), 0, true) as IssuesToolWindow;

            if (null == window || modelToUse == null)
            {
                return;
            }

            var win = window as IssuesToolWindow;
            win.UpdateModel(modelToUse);
        }

        #endregion
    }
}