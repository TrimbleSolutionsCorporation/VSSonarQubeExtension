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
    using EnvDTE;
    using EnvDTE80;
    using Helpers;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using StatusBar;
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using VSControls;
    using VSSonarExtensionUi.View.Helpers;
    using VSSonarPlugins;
    using DColor = System.Drawing.Color;
    using MColor = System.Windows.Media.Color;

    /// <summary>
    ///     The vs sonar extension package.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad("{05ca2046-1eb1-4813-a91f-a69df9b27698}", PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.EmptySolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideToolWindow(typeof(IssuesToolWindow), Style = VsDockStyle.Tabbed, Window = "ac305e7a-a44b-4541-8ece-3c88d5425338")]
    [ProvideToolWindow(typeof(PluginToolWindow), Style = VsDockStyle.Tabbed, MultiInstances = true)]
    [Guid(GuidList.GuidVSSonarExtensionPkgString)]
    public sealed partial class VsSonarExtensionPackage : AsyncPackage
    {
        private SolutionEventsListener listener;
        private DTE2 dte2;
        private OleMenuCommand runAnalysisCmd;
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

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            try
            {
                await base.InitializeAsync(new CancellationToken(), null);
                dte2 = (DTE2)GetGlobalService(typeof(DTE));
                string extensionRunningPath = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty).ToString();

                if (dte2 == null)
                {
                    return;
                }
                visualStudioInterface = new VsPropertiesHelper(dte2, this);
                visualStudioInterface.WriteToVisualStudioOutput(DateTime.Now + " : VsSonarExtensionPackage Initialize");
                await SetupMenuCommandsAsync(this);

                try
                {
                    string uniqueId = dte2.Version;

                    if (extensionRunningPath.ToLower().Contains(dte2.Version + "exp"))
                    {
                        uniqueId += "Exp";
                    }

                    VSSonarExtensionUi.ViewModel.SonarQubeViewModel model = await SonarQubeViewModelFactory.StartupModelWithVsVersionAsync(uniqueId, this);
                    await model.InitModelFromPackageInitialization(
                        visualStudioInterface,
                        StatusBar,
                        this,
                        AssemblyDirectory);
                    CloseToolsWindows();
                    OutputGuid = "CDA8E85D-C469-4855-878B-0E778CD0DD" + int.Parse(uniqueId.Split('.')[0]).ToString(CultureInfo.InvariantCulture);
                    StartOutputWindow(OutputGuid);

                    // start listening
                    SonarQubeViewModelFactory.SQViewModel.PluginRequest += LoadPluginIntoNewToolWindow;
                    await StartSolutionListenersAsync(visualStudioInterface);

                    // configure colours
                    DColor defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
                    DColor defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
                    SonarQubeViewModelFactory.SQViewModel.UpdateTheme(ToMediaColor(defaultBackground), ToMediaColor(defaultForeground));
                    IVsStatusbar bar = await GetServiceAsync(typeof(SVsStatusbar)) as IVsStatusbar;
                    StatusBar = new VSSStatusBar(bar, dte2);
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
            ThreadHelper.ThrowIfNotOnUIThread();
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    // Find existing windows. 
                    ToolWindowPane currentWindow = FindToolWindow(typeof(PluginToolWindow), i, false);
                    if (currentWindow == null)
                    {
                        continue;
                    }

                    IVsWindowFrame windowFrame = (IVsWindowFrame)currentWindow.Frame;
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
            ThreadHelper.ThrowIfNotOnUIThread();
            // Find existing windows. 
            ToolWindowPane currentWindow = FindToolWindow(typeof(PluginToolWindow), id, false);
            if (currentWindow != null)
            {
                return;
            }

            // Create the window with the first free ID.
            PluginToolWindow.CurrentPluginControl = control;
            PluginToolWindow.CurrentPluginName = name;
            ToolWindowPane window = (ToolWindowPane)CreateToolWindow(typeof(PluginToolWindow), id);

            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("CAnnot Create Tool");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private async Task<bool> IsSolutionLoadedAsync()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;

            if ((null == solService))
            {
                throw new NotSupportedException("IVsSolution");
            }

            ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));

            return value is bool isSolOpen && isSolOpen;
        }

        /// <summary>
        /// Starts the output window.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        private void StartOutputWindow(string guid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                IVsOutputWindow outWindow = GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                Guid customGuid = new Guid(guid);
                if (outWindow != null)
                {
                    outWindow.CreatePane(ref customGuid, "VSSonarQube Output", 1, 1);
                    outWindow.GetPane(ref customGuid, out IVsOutputWindowPane customPane);
                    ((VsPropertiesHelper)visualStudioInterface).CustomPane = customPane;
                    ((VsPropertiesHelper)visualStudioInterface).CustomPane.Activate();
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
        private async System.Threading.Tasks.Task StartSolutionListenersAsync(IVsEnvironmentHelper helper)
        {
            listener = new SolutionEventsListener(helper, visualStudioInterface, dte2, this);
            string solutionName = helper.ActiveSolutionFileNameWithExtension();
            string solutionPath = helper.ActiveSolutioRootPath();
            string fileName = helper.ActiveFileFullPath();
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("Solution Opened: " + solutionName + " : " + solutionPath);
            await SonarQubeViewModelFactory.SQViewModel.OnSolutionOpen(solutionName, solutionPath, fileName);
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
        }

        /// <summary>
        ///     The setup menu commands.
        /// </summary>
        private async System.Threading.Tasks.Task SetupMenuCommandsAsync(AsyncPackage package)
        {
            OleMenuCommandService mcs = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (null == mcs)
            {
                return;
            }

            // menu commands
            CommandID menuCommandId = new CommandID(GuidList.GuidVSSonarExtensionCmdSet, (int)PkgCmdIdList.CmdidReviewsCommand);
            sonarReviewsCommand = new OleMenuCommand(ShowIssuesToolWindow, menuCommandId);
            mcs.AddCommand(sonarReviewsCommand);

            menuCommandId = new CommandID(GuidList.GuidVSSonarExtensionCmdSet, (int)PkgCmdIdList.CmdidShowOutputCommand);
            sonarShowOutputCommand = new OleMenuCommand(ShowOutputWindow, menuCommandId);
            mcs.AddCommand(sonarShowOutputCommand);

            menuCommandId = new CommandID(GuidList.GuidVSSonarExtensionCmdSet, (int)PkgCmdIdList.CmdidShowOptionsCommand);
            sonarShowOptionsCommand = new OleMenuCommand(ShowOptionsWindow, menuCommandId);
            mcs.AddCommand(sonarShowOptionsCommand);

            // solution context menus
            menuCommandId = new CommandID(GuidList.GuidStartAnalysisSolutionCTXCmdSet, PkgCmdIdList.CmdidRunAnalysisInSolution);
            runAnalysisCmd = new OleMenuCommand(AnalyseSolutionCmd, menuCommandId);
            mcs.AddCommand(runAnalysisCmd);
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
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            StartOutputWindow(OutputGuid);
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
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            System.Collections.Generic.KeyValuePair<int, IMenuCommandPlugin> plugin = SonarQubeViewModelFactory.SQViewModel.InUsePlugin;
            ShowToolWindow(
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
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            IVsWindowFrame windowFrame;

            using (ToolWindowPane window = FindToolWindow(typeof(IssuesToolWindow), 0, true))
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