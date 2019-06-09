// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisOptionsViewModel.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2014 [Jorge Costa, Jorge.Costa@tekla.com]
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

namespace VSSonarExtensionUi.ViewModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
        
    using System.Net;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;
    
    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using Model.Helpers;
    using PropertyChanged;
    using View.Helpers;
    using VSSonarExtensionUi.Association;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    using SonarRestService.Types;
    using SonarRestService;
    using System.Threading.Tasks;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class AnalysisOptionsViewModel : IOptionsViewModelBase, IOptionsModelBase
    {
        /// <summary>
        ///     The default value sonar sources.
        /// </summary>
        public static readonly string DefautValueSonarSources = ".";

        /// <summary>
        ///     The view model.
        /// </summary>
        private readonly VSonarQubeOptionsViewModel viewModel;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        ///     The vs environment helper.
        /// </summary>
        private IVsEnvironmentHelper vsenvironmenthelper;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisOptionsViewModel" /> class.
        /// </summary>
        /// <param name="viewModel">The view Model.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        public AnalysisOptionsViewModel(
            VSonarQubeOptionsViewModel viewModel, 
            IConfigurationHelper configurationHelper)
        {
            this.configurationHelper = configurationHelper;
            this.viewModel = viewModel;
            this.TimeoutValue = 10;
            this.IsSolutionAnalysisChecked = true;
            this.IsProjectAnalysisChecked = true;
            this.Header = "Analysis Options";
            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
            this.DownloadWrapperCommand = new RelayCommand(this.OnDownloadWrapperCommand);

            SonarQubeViewModel.RegisterNewViewModelInPool(this);
            AssociationModel.RegisterNewModelInPool(this);
			Task.Run(() => this.ReloadDataFromDisk(null));
        }

        /// <summary>
        ///     Gets the assembly directory.
        /// </summary>
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        /// Gets or sets the download wrapper command.
        /// </summary>
        /// <value>
        /// The download wrapper command.
        /// </value>
        public ICommand DownloadWrapperCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether debug is checked.
        /// </summary>
        public bool DebugIsChecked { get; set; }

        /// <summary>
        /// Gets or sets the SQMS build runner.
        /// </summary>
        /// <value>
        /// The SQMS build runner.
        /// </value>
        public string SQMSBuildRunnerVersion { get; set; }

        /// <summary>
        /// Gets or sets the wrapper path.
        /// </summary>
        /// <value>
        /// The wrapper path.
        /// </value>
        public string WrapperPath { get; set; }

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is solution analysis checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is solution analysis checked; otherwise, <c>false</c>.
        /// </value>
        public bool IsSolutionAnalysisChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is project analysis checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is project analysis checked; otherwise, <c>false</c>.
        /// </value>
        public bool IsProjectAnalysisChecked { get; set; }

        /// <summary>
        ///     Gets or sets the language.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        ///     Gets or sets the project id.
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        ///     Gets or sets the Java binary.
        /// </summary>
        public string ProjectVersion { get; set; }

        /// <summary>
        ///     Gets or sets the Java binary.
        /// </summary>
        public string SourceDir { get; set; }

        /// <summary>
        ///     Gets or sets the Java binary.
        /// </summary>
        public string SourceEncoding { get; set; }

        /// <summary>
        /// Gets or sets the excluded plugin.
        /// </summary>
        /// <value>
        /// The excluded plugin.
        /// </value>
        public string ExcludedPlugins { get; set; }

        /// <summary>
        ///     Gets or sets the timeout value.
        /// </summary>
        [AlsoNotifyFor("TimeoutValueString")]
        public int TimeoutValue { get; set; }

        /// <summary>
        ///     Gets the timeout value string.
        /// </summary>
        public string TimeoutValueString
        {
            get
            {
                return "Timeout = " + this.TimeoutValue.ToString(CultureInfo.InvariantCulture) + " min";
            }
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        /// <param name="availableProjects">The available projects.</param>
        /// <param name="issuePlugin">The issue plugin.</param>
        public async Task OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IList<IIssueTrackerPlugin> issuePlugin)
        {
            // does nothing
            await Task.Delay(0);
        }

        /// <summary>
        /// The update services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vs environment helper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public async Task UpdateServices(
            IVsEnvironmentHelper vsenvironmenthelperIn,
            IVSSStatusBar statusBar,
            IServiceProvider provider)
        {
            this.vsenvironmenthelper = vsenvironmenthelperIn;
            await Task.Delay(0);
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>
        /// returns view model
        /// </returns>
        public object GetViewModel()
        {
            return this;
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        public void SaveData()
        {
            // general props
            this.configurationHelper.WriteSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.SonarQubeMsbuildVersionKey, this.SQMSBuildRunnerVersion);
            this.configurationHelper.WriteSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.CxxWrapperPathKey, this.WrapperPath);
            this.configurationHelper.WriteSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.ExcludedPluginsKey, this.ExcludedPlugins);
            this.configurationHelper.WriteSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.IsDebugAnalysisOnKey, this.DebugIsChecked.ToString());
            this.configurationHelper.WriteSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.LocalAnalysisTimeoutKey, this.TimeoutValue.ToString());
            this.configurationHelper.WriteSetting(
                Context.AnalysisGeneral,
                OwnersId.AnalysisOwnerId,
                GlobalAnalysisIds.LocalAnalysisProjectAnalysisEnabledKey,
                this.IsProjectAnalysisChecked.ToString());
            this.configurationHelper.WriteSetting(
                Context.AnalysisGeneral,
                OwnersId.AnalysisOwnerId,
                GlobalAnalysisIds.LocalAnalysisSolutionAnalysisEnabledKey,
                this.IsSolutionAnalysisChecked.ToString());
        }

        /// <summary>
        /// The refresh properties in view.
        /// </summary>
        /// <param name="associatedProjectIn">The associated project in.</param>
        public async Task ReloadDataFromDisk(Resource associatedProjectIn)
        {
			this.IsProjectAnalysisChecked = true;

			var dataSet = string.Empty;
			this.configurationHelper.ReadInSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.ExcludedPluginsKey, out dataSet, "devcockpit,pdfreport,report,scmactivity,views,jira,scmstats");
			this.ExcludedPlugins = dataSet;

			this.configurationHelper.ReadInSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.LocalAnalysisProjectAnalysisEnabledKey, out dataSet, "true");
			this.IsProjectAnalysisChecked = bool.Parse(dataSet);

			this.configurationHelper.ReadInSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.LocalAnalysisSolutionAnalysisEnabledKey, out dataSet, "true");
			this.IsSolutionAnalysisChecked = bool.Parse(dataSet);

            // ensure wrapper is available
			await this.OnDownloadWrapperStartup();            
            this.SaveData();
        }

		/// <summary>
		/// Called when [disconnect].
		/// </summary>
		public async Task OnDisconnect()
        {
            // does nothing
            await Task.Delay(0);
        }

        /// <summary>
        /// The set options.
        /// </summary>
        public void SetGeneralOptions()
        {
            this.ExcludedPlugins =
                this.configurationHelper.ReadSetting(
                    Context.AnalysisGeneral,
                    OwnersId.AnalysisOwnerId,
                    GlobalAnalysisIds.ExcludedPluginsKey).Value;

            try
            {
                string timeout =
                    this.configurationHelper.ReadSetting(
                        Context.AnalysisGeneral,
                        OwnersId.AnalysisOwnerId,
                        GlobalAnalysisIds.LocalAnalysisTimeoutKey).Value;

                this.TimeoutValue = int.Parse(timeout);
            }
            catch
            {
                this.TimeoutValue = 10;
            }

            try
            {
                var debugCheck =
                    this.configurationHelper.ReadSetting(
                        Context.AnalysisGeneral,
                        OwnersId.AnalysisOwnerId,
                        GlobalAnalysisIds.IsDebugAnalysisOnKey).Value;

                this.DebugIsChecked = bool.Parse(debugCheck);
            }
            catch
            {
                this.DebugIsChecked = false;
            }
        }

        /// <summary>
        /// The update colours.
        /// </summary>
        /// <param name="background">
        /// The background.
        /// </param>
        /// <param name="foreground">
        /// The foreground.
        /// </param>
        public void UpdateColours(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        /// <summary>
        /// Gets the available model, TODO: needs to be removed after view models are split into models and view models
        /// </summary>
        /// <returns>
        /// returns model
        /// </returns>
        public object GetAvailableModel()
        {
            return null;
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="workDir">The work dir.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="profile">The profile.</param>
        public async Task AssociateWithNewProject(
            Resource project,
            string workDir,
            ISourceControlProvider provider,
            Dictionary<string, Profile> profile,
            string visualStudioVersion)
        {
            this.SourceDir = workDir;
            this.associatedProject = project;
            await Task.Delay(0);
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public async Task OnSolutionClosed()
        {
            this.SourceDir = string.Empty;
            this.associatedProject = null;
            await Task.Delay(0);
        }

        /// <summary>
        /// Called when [download wrapper command].
        /// </summary>
        private void OnDownloadWrapperCommand()
        {
            var installPath = Path.Combine(this.configurationHelper.ApplicationPath, "Wrapper");

            if (!File.Exists(Path.Combine(installPath, "CxxSonarQubeMsbuidRunner.exe")))
            {
                var urldownload = "https://github.com/jmecsoftware/sonar-cxx-msbuild-tasks/releases/download/3.0.1/CxxSonarQubeMsbuidRunner.zip";
                var tmpFile = Path.Combine(this.configurationHelper.ApplicationPath, "CxxSonarQubeMsbuidRunner.zip");
                try
                {
                    using (var client = new WebClient())
                    {
                        if (File.Exists(tmpFile))
                        {
                            File.Delete(tmpFile);
                        }

                        client.DownloadFile(urldownload, tmpFile);

                        ZipFile.ExtractToDirectory(tmpFile, installPath);
                    }

                    if (!File.Exists(Path.Combine(installPath, "CxxSonarQubeMsbuidRunner.exe")))
                    {
                        MessageDisplayBox.DisplayMessage(
                            "Wrapper installation failed, expected in  " + Path.Combine(installPath, "CxxSonarQubeMsbuidRunner.exe") + " : download manually and unzip to that folder",
                            helpurl: "https://github.com/jmecsoftware/sonar-cxx-msbuild-tasks/releases");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageDisplayBox.DisplayMessage(
                        "Wrapper installation failed : " + ex.Message + " please be sure you have access to url. If not download manually and change the wrapper path.",
                        helpurl: "https://github.com/jmecsoftware/sonar-cxx-msbuild-tasks/releases");
                }
            }

            this.WrapperPath = Path.Combine(installPath, "CxxSonarQubeMsbuidRunner.exe");
            MessageDisplayBox.DisplayMessage("Wrapper installed in  " + Path.Combine(installPath, "CxxSonarQubeMsbuidRunner.exe") + " : Dont forget to save or apply.");
            this.InstallTools(Path.Combine(installPath, "CxxSonarQubeMsbuidRunner.exe"));
        }

        /// <summary>
        /// Installs the tools.
        /// </summary>
        /// <param name="cxxwrapper">The cxx wrapper.</param>
        private void InstallTools(string cxxwrapper)
        {
            bool canInstall = QuestionUser.GetInput("this will install any third party tools using Chocolatey, elevated rights will be requested. Do you want to proceed?");
            if (canInstall)
            {
                using (var process = new Process())
                {
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.FileName = cxxwrapper;
                    process.StartInfo.Arguments = "/i";
                    process.Start();
                    process.WaitForExit();
                }
            }
            else
            {
                MessageDisplayBox.DisplayMessage(
                    "External tools have not been installed.",
                    "Tools will be installed when the wrapper for preview and full analysis runs. " +
                    "If you dont have permissions, the recommended way is to define tools paths in a configuration file in your home folder.",
                    helpurl: "https://github.com/jmecsoftware/sonar-cxx-msbuild-tasks#using-the-wrapper-behind-proxy-or-were-admin-rights-are-not-available");
            }
        }

        /// <summary>
        /// Called when [download wrapper command].
        /// </summary>
        private async Task OnDownloadWrapperStartup()
        {
            var installPath = Path.Combine(this.configurationHelper.ApplicationPath, "Wrapper", "3.0.1");

            if (File.Exists(Path.Combine(installPath, "CxxSonarQubeRunnerWin.bat")))
            {
                this.WrapperPath = Path.Combine(installPath, "CxxSonarQubeRunnerWin.bat");
                return;
            }

			await Task.Run(() =>
			{
				var urldownload = "https://github.com/jmecsoftware/CxxSonarQubeRunner/releases/download/3.0.1/CxxSonarQubeRunnerWin.zip";
				var tmpFile = Path.Combine(this.configurationHelper.ApplicationPath, "CxxSonarQubeRunnerWin.zip");

				using (var client = new WebClient())
				{
					if (File.Exists(tmpFile))
					{
						File.Delete(tmpFile);
					}

					client.DownloadFile(urldownload, tmpFile);
					ZipFile.ExtractToDirectory(tmpFile, installPath);
					this.WrapperPath = Path.Combine(installPath, "CxxSonarQubeRunnerWin.bat");
				}
			});
        }
    }
}