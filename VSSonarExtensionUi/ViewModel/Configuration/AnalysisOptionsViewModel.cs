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
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Media;

    using ExtensionHelpers;

    using ExtensionTypes;

    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using SonarRestService;

    using VSSonarExtensionUi.View.Helpers;
    using VSSonarExtensionUi.ViewModel.Helpers;

    using VSSonarPlugins;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    [ImplementPropertyChanged]
    public class AnalysisOptionsViewModel : IViewModelBase, IOptionsViewModelBase
    {
        #region Static Fields

        /// <summary>
        ///     The defaut value sonar sources.
        /// </summary>
        public static readonly string DefautValueSonarSources = ".";

        /// <summary>
        ///     The excluded plugins defaut value.
        /// </summary>
        public static readonly string ExcludedPluginsDefautValue = "devcockpit,pdfreport,report,scmactivity,views,jira,scmstats";

        #endregion

        #region Fields

        /// <summary>
        ///     The view model.
        /// </summary>
        private readonly VSonarQubeOptionsViewModel viewModel;

        /// <summary>
        ///     The vsenvironmenthelper.
        /// </summary>
        private IVsEnvironmentHelper vsenvironmenthelper;

        private IConfigurationHelper configurationHelper;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisOptionsViewModel"/> class.
        /// </summary>
        /// <param name="vsenvironmenthelper">
        /// The Vsenvironmenthelper.
        /// </param>
        /// <param name="viewModel">
        /// The view Model.
        /// </param>
        public AnalysisOptionsViewModel(IVsEnvironmentHelper vsenvironmenthelper, VSonarQubeOptionsViewModel viewModel, IConfigurationHelper configurationHelper)
        {
            this.vsenvironmenthelper = vsenvironmenthelper;
            this.configurationHelper = configurationHelper;
            this.viewModel = viewModel;
            this.TimeoutValue = 10;
            this.Header = "Analysis Options";
            this.ForeGroundColor = Colors.Black;
            this.ForeGroundColor = Colors.Black;

            if (string.IsNullOrEmpty(this.SonarQubeBinary))
            {
                this.GetDefaultSonarRunnerLocation();
            }

            if (string.IsNullOrEmpty(this.JavaBinary))
            {
                this.GetDefaultJavaLocationIfAvailable();
            }

            this.BrowseForJavaTrigger = new RelayCommand(this.OnBrowseForJavaTrigger);
            this.BrowseForSonarRunnerQubeTrigger = new RelayCommand(this.OnBrowseForSonarRunnerQubeTrigger);

            this.ResetDefaults();
        }

        #endregion

        #region Public Properties

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
        ///     Gets or sets the browse for java trigger.
        /// </summary>
        public RelayCommand BrowseForJavaTrigger { get; set; }

        /// <summary>
        ///     Gets or sets the browse for sonar runner qube trigger.
        /// </summary>
        public RelayCommand BrowseForSonarRunnerQubeTrigger { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether debug is checked.
        /// </summary>
        public bool DebugIsChecked { get; set; }

        /// <summary>
        ///     Gets or sets the excluded plugins.
        /// </summary>
        public string ExcludedPlugins { get; set; }

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is solution open.
        /// </summary>
        public bool IsSolutionOpen { get; set; }

        /// <summary>
        ///     Gets or sets the java binary.
        /// </summary>
        public string JavaBinary { get; set; }

        /// <summary>
        ///     Gets or sets the language.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        ///     Gets or sets the project id.
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        ///     Gets or sets the java binary.
        /// </summary>
        public string ProjectVersion { get; set; }

        /// <summary>
        ///     Gets or sets the sonar qube binary.
        /// </summary>
        public string SonarQubeBinary { get; set; }

        /// <summary>
        ///     Gets or sets the java binary.
        /// </summary>
        public string SourceDir { get; set; }

        /// <summary>
        ///     Gets or sets the java binary.
        /// </summary>
        public string SourceEncoding { get; set; }

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

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The update services.
        /// </summary>
        /// <param name="restServiceIn">
        /// The rest service in.
        /// </param>
        /// <param name="vsenvironmenthelperIn">
        /// The vsenvironmenthelper in.
        /// </param>
        /// <param name="statusBar">
        /// The status bar.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public void UpdateServices(
            ISonarRestService restServiceIn,
            IVsEnvironmentHelper vsenvironmenthelperIn,
            IConfigurationHelper configurationHelper,
            IVSSStatusBar statusBar,
            IServiceProvider provider)
        {
            this.configurationHelper = configurationHelper;
            this.vsenvironmenthelper = vsenvironmenthelperIn;
        }

        /// <summary>
        ///     The apply.
        /// </summary>
        public void Apply()
        {
            this.configurationHelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.RunnerExecutableKey, this.SonarQubeBinary);
            this.configurationHelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.JavaExecutableKey, this.JavaBinary);
            this.configurationHelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.LocalAnalysisTimeoutKey, 
                this.TimeoutValue.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.IsDebugAnalysisOnKey, 
                this.DebugIsChecked.ToString());
            this.configurationHelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.ExcludedPluginsKey, this.ExcludedPlugins);

            if (this.viewModel.Project != null)
            {
                string solId = VsSonarUtils.SolutionGlobPropKey(this.viewModel.Project.Key);
                this.configurationHelper.WriteOptionInApplicationData(solId, GlobalIds.SonarSourceKey, this.SourceDir);
                this.configurationHelper.WriteOptionInApplicationData(solId, GlobalIds.SourceEncodingKey, this.SourceEncoding);
            }
        }

        /// <summary>
        ///     The end data association.
        /// </summary>
        public void EndDataAssociation()
        {
            this.IsSolutionOpen = false;
        }

        /// <summary>
        ///     The exit.
        /// </summary>
        public void Exit()
        {
            Dictionary<string, string> generalOptionsInDsk = this.configurationHelper.ReadAllAvailableOptionsInSettings(GlobalIds.GlobalPropsId);

            if (generalOptionsInDsk != null)
            {
                this.SetGeneralOptions(generalOptionsInDsk);
            }

            if (this.viewModel.Project != null)
            {
                string solId = VsSonarUtils.SolutionGlobPropKey(this.viewModel.Project.Key);
                Dictionary<string, string> projectOptionsInDsk = this.configurationHelper.ReadAllAvailableOptionsInSettings(solId);
                this.SetProjectOptions(projectOptionsInDsk);
            }
        }

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="associatedProject">
        /// The associated project.
        /// </param>
        /// <param name="userConnectionConfig">
        /// The user connection config.
        /// </param>
        /// <param name="workingDir">
        /// The working dir.
        /// </param>
        public void InitDataAssociation(Resource associatedProject, ISonarConfiguration userConnectionConfig, string workingDir)
        {
            this.IsSolutionOpen = true;
            this.EnsureProjectMandatoryPropertiesAreSet();
        }

        /// <summary>
        ///     Gets or sets a value indicating whether browse for java trigger.
        /// </summary>
        public void OnBrowseForJavaTrigger()
        {
            var filedialog = new OpenFileDialog { Filter = @"Java executable|java.exe" };
            DialogResult result = filedialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (File.Exists(filedialog.FileName))
                {
                    this.JavaBinary = filedialog.FileName;
                }
                else
                {
                    UserExceptionMessageBox.ShowException(@"Error Choosing File, File Does not exits", null);
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether browse for sonar runner qube trigger.
        /// </summary>
        public void OnBrowseForSonarRunnerQubeTrigger()
        {
            var filedialog = new OpenFileDialog { Filter = @"SonarQube executable|sonar-runner.bat" };
            DialogResult result = filedialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (File.Exists(filedialog.FileName))
                {
                    this.SonarQubeBinary = filedialog.FileName;
                }
                else
                {
                    UserExceptionMessageBox.ShowException(@"Error Choosing File, File Does not exits", null);
                }
            }
        }

        /// <summary>
        ///     The reset defaults.
        /// </summary>
        public void ResetDefaults()
        {
            try
            {
                this.EnsureGeneralMandatoryPropertiesAreSet();
                this.EnsureProjectMandatoryPropertiesAreSet();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
            }
        }

        /// <summary>
        ///     The save and close.
        /// </summary>
        public void SaveAndClose()
        {
            this.Apply();
        }

        /// <summary>
        /// The set options.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        public void SetGeneralOptions(Dictionary<string, string> options)
        {
            if (File.Exists(this.GetOptionFromDictionary(options, GlobalIds.JavaExecutableKey)))
            {
                this.JavaBinary = this.GetOptionFromDictionary(options, GlobalIds.JavaExecutableKey);
            }

            if (File.Exists(this.GetOptionFromDictionary(options, GlobalIds.RunnerExecutableKey)))
            {
                this.SonarQubeBinary = this.GetOptionFromDictionary(options, GlobalIds.RunnerExecutableKey);
            }

            this.ExcludedPlugins = this.GetOptionFromDictionary(options, GlobalIds.ExcludedPluginsKey);

            try
            {
                string timeout = this.GetOptionFromDictionary(options, GlobalIds.LocalAnalysisTimeoutKey);
                this.TimeoutValue = int.Parse(timeout);
            }
            catch
            {
                this.TimeoutValue = 10;
            }

            try
            {
                this.DebugIsChecked = bool.Parse(this.GetOptionFromDictionary(options, GlobalIds.IsDebugAnalysisOnKey));
            }
            catch
            {
                this.DebugIsChecked = false;
            }

            this.EnsureGeneralMandatoryPropertiesAreSet();
        }

        /// <summary>
        /// The set options.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        public void SetProjectOptions(Dictionary<string, string> options)
        {
            // set solution general properties
            if (this.viewModel.Project != null)
            {
                this.SourceDir = this.GetGeneralProjectOptionFromDictionary(options, GlobalIds.SonarSourceKey);
                this.SourceEncoding = this.GetGeneralProjectOptionFromDictionary(options, GlobalIds.SourceEncodingKey);
                this.Language = this.viewModel.Project.Lang;
                this.ProjectId = this.viewModel.Project.Key;
                this.ProjectVersion = this.viewModel.Project.Version;
            }

            this.EnsureProjectMandatoryPropertiesAreSet();
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

        #endregion

        #region Methods

        /// <summary>
        ///     The ensure mandatory properties are set.
        /// </summary>
        private void EnsureGeneralMandatoryPropertiesAreSet()
        {
            if (string.IsNullOrEmpty(this.SonarQubeBinary))
            {
                this.GetDefaultSonarRunnerLocation();
            }

            if (string.IsNullOrEmpty(this.JavaBinary))
            {
                this.GetDefaultJavaLocationIfAvailable();
            }

            if (string.IsNullOrEmpty(this.ExcludedPlugins))
            {
                this.ExcludedPlugins = ExcludedPluginsDefautValue;
            }
        }

        /// <summary>
        ///     The ensure mandatory properties are set.
        /// </summary>
        private void EnsureProjectMandatoryPropertiesAreSet()
        {
            if (this.viewModel.Project == null)
            {
                return;
            }

            // set mandatory properties, that normally are not available in server or plugin
            if (string.IsNullOrEmpty(this.SourceDir))
            {
                this.SourceDir = DefautValueSonarSources;
            }

            if (string.IsNullOrEmpty(this.ProjectVersion))
            {
                this.ProjectVersion = "work";
            }

            if (string.IsNullOrEmpty(this.SourceEncoding))
            {
                this.SourceEncoding = "UTF-8";
            }
        }

        /// <summary>
        ///     The get default java location if available.
        /// </summary>
        private void GetDefaultJavaLocationIfAvailable()
        {
            string programFiles = Path.Combine("C:\\Program Files", "Java");
            string programFilesX86 = Path.Combine("C:\\Program Files (x86)", "Java");

            if (Directory.Exists(programFiles))
            {
                try
                {
                    FileInfo[] fileList = new DirectoryInfo(programFiles).GetFiles("java.exe", SearchOption.AllDirectories);
                    if (fileList.Length > 0)
                    {
                        if (fileList[0].DirectoryName != null)
                        {
                            this.JavaBinary = Path.Combine(fileList[0].DirectoryName, fileList[0].Name);
                        }
                    }

                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Cannot Find Java in : " + ex.StackTrace);
                }
            }
            if (Directory.Exists(programFilesX86))
            {
                if (!string.IsNullOrEmpty(programFilesX86))
                {
                    try
                    {
                        FileInfo[] fileList = new DirectoryInfo(programFilesX86).GetFiles("java.exe", SearchOption.AllDirectories);
                        if (fileList.Length > 0)
                        {
                            this.JavaBinary = fileList[0].Name;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Cannot Find Java: " + ex.StackTrace);
                    }
                }
            }
        }

        /// <summary>
        ///     The get default sonar runner location.
        /// </summary>
        private void GetDefaultSonarRunnerLocation()
        {
            string runnigPath = AssemblyDirectory;
            string sonarRunner = Path.Combine(runnigPath, "SonarRunner\\bin\\sonar-runner.bat");
            if (File.Exists(sonarRunner))
            {
                this.SonarQubeBinary = sonarRunner;
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
        private string GetGeneralProjectOptionFromDictionary(Dictionary<string, string> options, string key)
        {
            return options.ContainsKey(key) ? options[key] : string.Empty;
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
        private string GetOptionFromDictionary(Dictionary<string, string> options, string key)
        {
            return options.ContainsKey(key) ? options[key] : string.Empty;
        }

        #endregion
    }
}