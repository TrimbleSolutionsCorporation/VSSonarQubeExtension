// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSSonarQubeExtensionPackage.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarQubeExtension
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows.Controls;

    using EnvDTE;

    using EnvDTE80;

    using ExtensionHelpers;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using SonarRestService;
    using VSSonarQubeExtension.SmartTags.BufferUpdate;
    using VSSonarQubeExtension.SmartTags.StatusBar;
    using VSSonarQubeExtension.VSControls;
    using VSSonarQubeExtension.VSControls.DialogOptions;
    using VSSonarExtensionUi.ViewModel;
    using VSSonarExtensionUi.View;



    /// <summary>
    ///     The vs sonar extension package.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(SonarGeneralOptionsPage), "Sonar Options", "General", 1000, 1001, true)]
    [ProvideProfile(typeof(SonarGeneralOptionsPage), "Sonar Options", "General", 1002, 1003, true)]
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
        public static readonly SonarQubeViewModel ExtensionModelData = new SonarQubeViewModel();

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

        #region Methods

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
        ///     <cref>NotSupportedException</cref>
        /// </exception>
        public void CloseToolWindow()
        {
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    // Find existing windows. 
                    var currentWindow = this.FindToolWindow(typeof(PluginToolWindow), i, false);
                    if (currentWindow == null)
                    {
                        continue;
                    }

                    var windowFrame = (IVsWindowFrame)currentWindow.Frame;
                    ErrorHandler.ThrowOnFailure(windowFrame.Hide());
                }
                catch (Exception)
                {
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
        ///     <cref>NotSupportedException</cref>
        /// </exception>
        public void ShowToolWindow(UserControl control, int id, string name)
        {
            // Find existing windows. 
            var currentWindow = this.FindToolWindow(typeof(PluginToolWindow), id, false);
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

                this.visualStudioInterface = new VsPropertiesHelper(this.dte2, this);
                this.restService = new SonarRestService(new JsonSonarConnector());
                this.VsEvents = new VsEvents(ExtensionModelData, this.visualStudioInterface, this.dte2);

                // int configuration options
                this.InitOptions();

                try
                {
                    ExtensionModelData.VsHelper = this.visualStudioInterface;
                    ExtensionModelData.SonarRestConnector = this.restService;

                    this.UpdateModelInToolWindow(ExtensionModelData);

                    try
                    {
                        var outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                        var customGuid = new Guid("CDA8E85D-C469-4855-878B-0E778CD0DD53");
                        if (outWindow != null)
                        {
                            outWindow.CreatePane(ref customGuid, "VSSonarQube Output", 1, 1);
                            IVsOutputWindowPane customPane;
                            outWindow.GetPane(ref customGuid, out customPane);
                            ((VsPropertiesHelper)this.visualStudioInterface).CustomPane = customPane;
                            ((VsPropertiesHelper)this.visualStudioInterface).CustomPane.Activate();
                        }
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
        ///     The init general options.
        /// </summary>
        private void InitGeneralOptions()
        {
            this.visualStudioInterface.WriteDefaultOption(
                "Sonar Options", "General", "SonarHost", "http://localhost:9000");
        }

        /// <summary>
        ///     The init options.
        /// </summary>
        private void InitOptions()
        {
            this.InitGeneralOptions();
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

            using (var window = this.FindToolWindow(typeof(IssuesToolWindow), 0, true))
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
            var bar = GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            this.StatusBar = new VSSStatusBar(bar, this.dte2);
            modelToUse.ExtensionDataModelUpdate(new SonarRestService(new JsonSonarConnector()), new VsPropertiesHelper(this.dte2, this), null, this.StatusBar, this);
            win.UpdateModel(modelToUse);
        }

        public VSSStatusBar StatusBar { get; set; }

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
            ExtensionModelData.LocalViewModel.OnPreviewCommand();
        }

        #endregion
    }


}