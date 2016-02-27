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

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    [ImplementPropertyChanged]
    public class AnalysisOptionsViewModel : IOptionsViewModelBase, IOptionsModelBase
    {
        /// <summary>
        /// The minimum version
        /// </summary>
        public static readonly string MinimumVersion = "1.9.3";

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

            try
            {
                this.ReloadDataFromDisk(null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to reload from disk : " + ex.Message);
            }
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
        /// Gets or sets the CXX wrapper version.
        /// </summary>
        /// <value>
        /// The CXX wrapper version.
        /// </value>
        public string CxxWrapperVersion { get; set; }

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
        public void OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IIssueTrackerPlugin issuePlugin)
        {
            // does nothing
        }

        /// <summary>
        /// The update services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vs environment helper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public void UpdateServices(
            IVsEnvironmentHelper vsenvironmenthelperIn,
            IVSSStatusBar statusBar,
            IServiceProvider provider)
        {
            this.vsenvironmenthelper = vsenvironmenthelperIn;
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
            this.configurationHelper.WriteOptionInApplicationData(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.CxxWrapperVersionKey, this.CxxWrapperVersion);
            this.configurationHelper.WriteOptionInApplicationData(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.SonarQubeMsbuildVersionKey, this.SQMSBuildRunnerVersion);
            this.configurationHelper.WriteOptionInApplicationData(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.CxxWrapperPathKey, this.WrapperPath);
            this.configurationHelper.WriteOptionInApplicationData(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.ExcludedPluginsKey, this.ExcludedPlugins);
            this.configurationHelper.WriteOptionInApplicationData(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.IsDebugAnalysisOnKey, this.DebugIsChecked.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.LocalAnalysisTimeoutKey, this.TimeoutValue.ToString());
            this.configurationHelper.WriteOptionInApplicationData(
                Context.AnalysisGeneral,
                OwnersId.AnalysisOwnerId,
                GlobalAnalysisIds.LocalAnalysisProjectAnalysisEnabledKey,
                this.IsProjectAnalysisChecked.ToString());
            this.configurationHelper.WriteOptionInApplicationData(
                Context.AnalysisGeneral,
                OwnersId.AnalysisOwnerId,
                GlobalAnalysisIds.LocalAnalysisSolutionAnalysisEnabledKey,
                this.IsSolutionAnalysisChecked.ToString());
        }

        /// <summary>
        /// The refresh properties in view.
        /// </summary>
        /// <param name="associatedProjectIn">The associated project in.</param>
        public void ReloadDataFromDisk(Resource associatedProjectIn)
        {
            try
            {
                this.CxxWrapperVersion = this.configurationHelper.ReadSetting(
                    Context.AnalysisGeneral,
                    OwnersId.AnalysisOwnerId,
                    GlobalAnalysisIds.CxxWrapperVersionKey).Value;

                var currentVersion = new Version(this.CxxWrapperVersion.Split('-')[0]);
                if (currentVersion.CompareTo(new Version(MinimumVersion)) < 0)
                {
                    this.CxxWrapperVersion = MinimumVersion;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                this.CxxWrapperVersion = MinimumVersion;
            }

            try
            {
                this.SQMSBuildRunnerVersion = this.configurationHelper.ReadSetting(
                        Context.AnalysisGeneral,
                        OwnersId.AnalysisOwnerId,
                        GlobalAnalysisIds.SonarQubeMsbuildVersionKey).Value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                this.SQMSBuildRunnerVersion = "1.1";
            }

            try
            {
                this.WrapperPath = this.configurationHelper.ReadSetting(
                    Context.AnalysisGeneral,
                    OwnersId.AnalysisOwnerId,
                    GlobalAnalysisIds.CxxWrapperPathKey).Value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                this.WrapperPath = Path.Combine(Path.Combine(this.configurationHelper.ApplicationPath, "Wrapper", this.CxxWrapperVersion), "CxxSonarQubeMsbuidRunner.exe");
            }

            try
            {
                this.ExcludedPlugins = this.configurationHelper.ReadSetting(
                    Context.AnalysisGeneral,
                    OwnersId.AnalysisOwnerId,
                    GlobalAnalysisIds.ExcludedPluginsKey).Value;
            }
            catch (Exception)
            {
                this.ExcludedPlugins = "devcockpit,pdfreport,report,scmactivity,views,jira,scmstats";
            }

            try
            {
                this.IsProjectAnalysisChecked = bool.Parse(
                    this.configurationHelper.ReadSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.LocalAnalysisProjectAnalysisEnabledKey).Value);
            }
            catch (Exception)
            {
                this.IsProjectAnalysisChecked = true;
            }

            try
            {
                this.IsSolutionAnalysisChecked = bool.Parse(
                    this.configurationHelper.ReadSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.LocalAnalysisSolutionAnalysisEnabledKey).Value);
            }
            catch (Exception)
            {
                this.IsSolutionAnalysisChecked = true;
            }

            // ensure wrapper is available
            try
            {
                this.OnDownloadWrapperStartup();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
            this.SaveData();
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public void OnDisconnect()
        {
            // does nothing
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
        /// <param name="sourcePlugin">The source plugin.</param>
        /// <param name="availableProjects">The available projects.</param>
        /// <param name="profile">The profile.</param>
        public void AssociateWithNewProject(Resource project, string workDir, ISourceControlProvider provider, Dictionary<string, Profile> profile)
        {
            this.SourceDir = workDir;
            this.associatedProject = project;
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void OnSolutionClosed()
        {
            this.SourceDir = string.Empty;
            this.associatedProject = null;
        }

        /// <summary>
        /// Called when [download wrapper command].
        /// </summary>
        private void OnDownloadWrapperCommand()
        {
            if (string.IsNullOrEmpty((this.CxxWrapperVersion)))
            {
                MessageDisplayBox.DisplayMessage(
                    "Version cannot be empty, be sure to use tab in release page of the wrapper.",
                    helpurl: "https://github.com/jmecsoftware/sonar-cxx-msbuild-tasks/releases");
                return;
            }

            var installPath = Path.Combine(this.configurationHelper.ApplicationPath, "Wrapper", this.CxxWrapperVersion);

            if (!File.Exists(Path.Combine(installPath, "CxxSonarQubeMsbuidRunner.exe")))
            {
                var urldownload = "https://github.com/jmecsoftware/sonar-cxx-msbuild-tasks/releases/download/" + this.CxxWrapperVersion + "/CxxSonarQubeMsbuidRunner.zip";
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
                Process process = new Process();
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = cxxwrapper;
                process.StartInfo.Arguments = "/i";
                process.Start();
                process.WaitForExit();
            }
            else
            {
                MessageDisplayBox.DisplayMessage("External tools have not been installed.",
                    "Tools will be installed when the wrapper for preview and full analysis runs. If you dont have permissions, the recommended way is to define tools paths in a configuration file in your home folder.",
                    helpurl: "https://github.com/jmecsoftware/sonar-cxx-msbuild-tasks#using-the-wrapper-behind-proxy-or-were-admin-rights-are-not-available");
            }
        }

        /// <summary>
        /// Called when [download wrapper command].
        /// </summary>
        private void OnDownloadWrapperStartup()
        {
            var installPath = Path.Combine(this.configurationHelper.ApplicationPath, "Wrapper", this.CxxWrapperVersion);

            if (File.Exists(Path.Combine(installPath, "CxxSonarQubeMsbuidRunner.exe")))
            {
                this.WrapperPath = Path.Combine(installPath, "CxxSonarQubeMsbuidRunner.exe");
                return;
            }

            try
            {
                var urldownload = "https://github.com/jmecsoftware/sonar-cxx-msbuild-tasks/releases/download/" + this.CxxWrapperVersion + "/CxxSonarQubeMsbuidRunner.zip";
                var tmpFile = Path.Combine(this.configurationHelper.ApplicationPath, "CxxSonarQubeMsbuidRunner.zip");

                using (var client = new WebClient())
                {
                    if (File.Exists(tmpFile))
                    {
                        File.Delete(tmpFile);
                    }

                    client.DownloadFile(urldownload, tmpFile);

                    ZipFile.ExtractToDirectory(tmpFile, installPath);
                    this.WrapperPath = Path.Combine(installPath, "CxxSonarQubeMsbuidRunner.exe");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// The get general project option from dictionary.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetGeneralProjectOptionFromDictionary(IEnumerable<SonarQubeProperties> options, string key)
        {
            foreach (var option in options.Where(option => option.Key.Equals(key)))
            {
                return option.Value;
            }

            return string.Empty;
        }

        /// <summary>
        /// The get option from dictionary.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetOptionFromDictionary(IEnumerable<SonarQubeProperties> options, string key)
        {
            foreach (var sonarQubePropertiese in options.Where(sonarQubePropertiese => sonarQubePropertiese.Key.Equals(key)))
            {
                return sonarQubePropertiese.Value;
            }

            return string.Empty;
        }
    }
}