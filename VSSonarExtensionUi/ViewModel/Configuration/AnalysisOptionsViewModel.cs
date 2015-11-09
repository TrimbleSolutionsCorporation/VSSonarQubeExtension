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
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Media;

    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using Model.Helpers;
    using PropertyChanged;
    using SonarLocalAnalyser;
    using View.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using VSSonarExtensionUi.Association;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    [ImplementPropertyChanged]
    public class AnalysisOptionsViewModel : IOptionsViewModelBase, IOptionsModelBase
    {
        #region Static Fields

        /// <summary>
        ///     The defaut value sonar sources.
        /// </summary>
        public static readonly string DefautValueSonarSources = ".";

        #endregion

        #region Fields

        /// <summary>
        ///     The view model.
        /// </summary>
        private readonly VSonarQubeOptionsViewModel viewModel;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        ///     The vsenvironmenthelper.
        /// </summary>
        private IVsEnvironmentHelper vsenvironmenthelper;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        #endregion

        #region Constructors and Destructors

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
            this.BrowseForJavaTrigger = new RelayCommand(this.OnBrowseForJavaTrigger);
            this.BrowseForSonarRunnerQubeTrigger = new RelayCommand(this.OnBrowseForSonarRunnerQubeTrigger);

            SonarQubeViewModel.RegisterNewViewModelInPool(this);
            AssociationModel.RegisterNewModelInPool(this);
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
        ///     Gets or sets the java binary.
        /// </summary>
        public string ProjectVersion { get; set; }

        /// <summary>
        ///     Gets or sets the java binary.
        /// </summary>
        public string SourceDir { get; set; }

        /// <summary>
        ///     Gets or sets the java binary.
        /// </summary>
        public string SourceEncoding { get; set; }

        /// <summary>
        /// Gets or sets the sonar qube binary.
        /// </summary>
        /// <value>
        /// The sonar qube binary.
        /// </value>
        public string SonarQubeBinary { get; set; }

        /// <summary>
        /// Gets or sets the java binary.
        /// </summary>
        /// <value>
        /// The java binary.
        /// </value>
        public string JavaBinary { get; set; }

        /// <summary>
        /// Gets or sets the excluded plugins.
        /// </summary>
        /// <value>
        /// The excluded plugins.
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
        /// Gets or sets the project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        private Resource Project { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The update services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
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
            this.configurationHelper.WriteOptionInApplicationData(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.JavaExecutableKey, this.JavaBinary);
            this.configurationHelper.WriteOptionInApplicationData(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.RunnerExecutableKey, this.SonarQubeBinary);
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

            if (this.Project != null)
            {
                this.configurationHelper.WriteOptionInApplicationData(Context.AnalysisProject, this.Project.Key, GlobalAnalysisIds.SonarSourceKey, this.SourceDir);
                this.configurationHelper.WriteOptionInApplicationData(Context.AnalysisProject, this.Project.Key, GlobalAnalysisIds.SourceEncodingKey, this.SourceEncoding);
            }
        }

        /// <summary>
        /// The refresh properties in view.
        /// </summary>
        /// <param name="associatedProjectIn">The associated project in.</param>
        public void ReloadDataFromDisk(Resource associatedProjectIn)
        {
            this.Project = associatedProjectIn;

            this.JavaBinary = this.configurationHelper.ReadSetting(
                Context.AnalysisGeneral,
                OwnersId.AnalysisOwnerId,
                GlobalAnalysisIds.JavaExecutableKey).Value;

            this.SonarQubeBinary = this.configurationHelper.ReadSetting(
                Context.AnalysisGeneral,
                OwnersId.AnalysisOwnerId,
                GlobalAnalysisIds.RunnerExecutableKey).Value;

            this.ExcludedPlugins = this.configurationHelper.ReadSetting(
                Context.AnalysisGeneral,
                OwnersId.AnalysisOwnerId,
                GlobalAnalysisIds.ExcludedPluginsKey).Value;


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

            try
            {
                this.DebugIsChecked = bool.Parse(this.configurationHelper.ReadSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId,    GlobalAnalysisIds.IsDebugAnalysisOnKey).Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            try
            {
                this.TimeoutValue =
                    int.Parse(
                        this.configurationHelper.ReadSetting(
                            Context.AnalysisGeneral,
                            OwnersId.AnalysisOwnerId,
                            GlobalAnalysisIds.LocalAnalysisTimeoutKey).Value);
            }
            catch (Exception ex)
            {
                this.TimeoutValue = 60000;
                Debug.WriteLine(ex.Message);
            }

            if (this.Project != null)
            {
                this.IsSolutionOpen = true;
            }
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
        /// The set options.
        /// </summary>
        public void SetGeneralOptions()
        {
            this.SonarQubeBinary =
                this.configurationHelper.ReadSetting(
                    Context.AnalysisGeneral,
                    OwnersId.AnalysisOwnerId,
                    GlobalAnalysisIds.RunnerExecutableKey).Value;

            this.JavaBinary =
                this.configurationHelper.ReadSetting(
                    Context.AnalysisGeneral,
                    OwnersId.AnalysisOwnerId,
                    GlobalAnalysisIds.JavaExecutableKey).Value;

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
        /// Gets the available model, TODO: needs to be removed after viewmodels are split into models and view models
        /// </summary>
        /// <returns>
        /// returns optinal model
        /// </returns>
        public object GetAvailableModel()
        {
            return null;
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workDir">The work dir.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public void AssociateWithNewProject(Resource project, string workDir, ISourceControlProvider provider, IIssueTrackerPlugin sourcePlugin)
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

        #endregion

        #region Methods

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

        #endregion
    }
}