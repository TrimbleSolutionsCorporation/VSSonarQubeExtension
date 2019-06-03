// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSSonarQubeExtensionPackage.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Controls;

    using EnvDTE;
    using EnvDTE80;

    using Helpers;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using StatusBar;
    using VSControls;
    using VSSonarExtensionUi.View.Helpers;
    using VSSonarExtensionUi.ViewModel.Analysis;
    using VSSonarPlugins;

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
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    [ProvideToolWindow(typeof(IssuesToolWindow), Style = VsDockStyle.Tabbed, Window = "ac305e7a-a44b-4541-8ece-3c88d5425338")]
    [ProvideToolWindow(typeof(PluginToolWindow), Style = VsDockStyle.Tabbed, MultiInstances = true)]
    [Guid(GuidList.GuidVSSonarExtensionPkgString)]
    public sealed partial class VsSonarExtensionPackage : Package
    {
        private SolutionEventsListener listener;
        private DTE2 dte2;
        private OleMenuCommand sonarReviewsCommandBar;
        private OleMenuCommand runAnalysisCmd;
        private OleMenuCommand runAnalysisInProjectCmd;
        private OleMenuCommand sonarReviewsCommand;
        private OleMenuCommand sonarShowOutputCommand;
        private OleMenuCommand sonarShowOptionsCommand;
        private IVsEnvironmentHelper visualStudioInterface;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VsSonarExtensionPackage" /> class.
        /// </summary>
        public VsSonarExtensionPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this));
        }

        /// <summary>
        /// Gets or sets the status bar.
        /// </summary>
        public VSSStatusBar StatusBar { get; set; }

        /// <summary>
        /// Gets or sets the vs events.
        /// </summary>
        /// <value>
        /// The vs events.
        /// </value>
        public VsEvents VsEvents { get; set; }

        /// <summary>
        /// Gets the assembly directory.
        /// </summary>
        /// <value>
        /// The assembly directory.
        /// </value>
        public string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Gets the output unique identifier.
        /// </summary>
        /// <value>
        /// The output unique identifier.
        /// </value>
        public string OutputGuid { get; private set; }
       
        /// <summary>
        /// To the color of the media.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>current ide color</returns>
        public static MColor ToMediaColor(DColor color)
        {
            return MColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        ///     The show tool window.
        /// </summary>
        public void CloseToolsWindows()
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
                    ErrorHandler.ThrowOnFailure(windowFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave));
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

        /// <summary>
        ///     The initialize.
        /// </summary>
        protected override async void Initialize()
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

                    this.visualStudioInterface.WriteToVisualStudioOutput(DateTime.Now + " : VsSonarExtensionPackage Initialize");

                    this.VsEvents = new VsEvents(this.visualStudioInterface, this.dte2, this);
                    var bar = this.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
                    this.StatusBar = new VSSStatusBar(bar, this.dte2);
                    var extensionRunningPath = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty).ToString();

                    var uniqueId = this.dte2.Version;

                    if (extensionRunningPath.ToLower().Contains(this.dte2.Version + "exp"))
                    {
                        uniqueId += "Exp";
                    }

					var model = SonarQubeViewModelFactory.StartupModelWithVsVersion(uniqueId, this);
					await model.InitModelFromPackageInitialization(
                        this.visualStudioInterface,
                        this.StatusBar,
                        this,
                        this.AssemblyDirectory);
                   
                    this.CloseToolsWindows();
                    this.OutputGuid = "CDA8E85D-C469-4855-878B-0E778CD0DD" + int.Parse(uniqueId.Split('.')[0]).ToString(CultureInfo.InvariantCulture);
                    this.StartOutputWindow(this.OutputGuid);

                    // start listening
                    SonarQubeViewModelFactory.SQViewModel.PluginRequest += this.LoadPluginIntoNewToolWindow;
                    this.StartSolutionListeners(this.visualStudioInterface);

                    // configure colours
                    DColor defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
                    DColor defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
                    SonarQubeViewModelFactory.SQViewModel.UpdateTheme(ToMediaColor(defaultBackground), ToMediaColor(defaultForeground));
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
        /// Starts the output window.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        private void StartOutputWindow(string guid)
        {
            try
            {
                var outWindow = GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                var customGuid = new Guid(guid);
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
                UserExceptionMessageBox.ShowException("VsSonarExtensionPackage Failed Create Output Window: " + guid, ex);
            }
        }

        /// <summary>
        /// Starts the solution listeners.
        /// </summary>
        /// <param name="helper">The helper.</param>
        private void StartSolutionListeners(IVsEnvironmentHelper helper)
        {
            this.listener = new SolutionEventsListener(helper);
            var triedOnceAlready = false;

            this.listener.OnAfterOpenProject += () =>
            {
                if (!SonarQubeViewModelFactory.SQViewModel.AssociationModule.IsAssociated && !triedOnceAlready)
                {
                    triedOnceAlready = true;
                    string solutionName = this.visualStudioInterface.ActiveSolutionName();
                    string solutionPath = this.visualStudioInterface.ActiveSolutionPath();

                    SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("Solution Opened: " + solutionName + " : " + solutionPath);

                    System.Threading.Tasks.Task.Run(async () =>
                    {
                        await SonarQubeViewModelFactory.SQViewModel.OnSolutionOpen(solutionName, solutionPath, string.Empty);
                    });
                }
            };
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
            SonarQubeViewModelFactory.SQViewModel.StartAnalysisWindow(AnalysisTypes.ANALYSIS, false);
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

            // menu commands
            var menuCommandId = new CommandID(GuidList.GuidVSSonarExtensionCmdSet, (int)PkgCmdIdList.CmdidReviewsCommand);
            this.sonarReviewsCommand = new OleMenuCommand(this.ShowIssuesToolWindow, menuCommandId);
            mcs.AddCommand(this.sonarReviewsCommand);

            menuCommandId = new CommandID(GuidList.GuidVSSonarExtensionCmdSet, (int)PkgCmdIdList.CmdidShowOutputCommand);
            this.sonarShowOutputCommand = new OleMenuCommand(this.ShowOutputWindow, menuCommandId);
            mcs.AddCommand(this.sonarShowOutputCommand);

            menuCommandId = new CommandID(GuidList.GuidVSSonarExtensionCmdSet, (int)PkgCmdIdList.CmdidShowOptionsCommand);
            this.sonarShowOptionsCommand = new OleMenuCommand(this.ShowOptionsWindow, menuCommandId);
            mcs.AddCommand(this.sonarShowOptionsCommand);

            // solution context menus
            menuCommandId = new CommandID(GuidList.GuidStartAnalysisSolutionCTXCmdSet, PkgCmdIdList.CmdidRunAnalysisInSolution);
            this.runAnalysisCmd = new OleMenuCommand(this.AnalyseSolutionCmd, menuCommandId);
            mcs.AddCommand(this.runAnalysisCmd);
        }

        /// <summary>
        /// Shows the options window.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ShowOptionsWindow(object sender, EventArgs e)
        {
            SonarQubeViewModelFactory.SQViewModel.LaunchExtensionProperties();
        }

        /// <summary>
        /// Shows the output window.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ShowOutputWindow(object sender, EventArgs e)
        {
            this.StartOutputWindow(this.OutputGuid);
        }

        /// <summary>
        /// The load plugin into new tool window.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        private void LoadPluginIntoNewToolWindow(object sender, EventArgs eventArgs)
        {
            var plugin = SonarQubeViewModelFactory.SQViewModel.InUsePlugin;
            this.ShowToolWindow(
                plugin.Value.GetUserControl(
                VSSonarExtensionUi.Model.Helpers.AuthtenticationHelper.AuthToken,
                SonarQubeViewModelFactory.SQViewModel.AssociationModule.AssociatedProject,
                SonarQubeViewModelFactory.SQViewModel.VsHelper),
                plugin.Key,
                plugin.Value.GetPluginDescription().Name);

            DColor defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            DColor defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);

            plugin.Value.UpdateTheme(ToMediaColor(defaultBackground), ToMediaColor(defaultForeground));
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
    }
}