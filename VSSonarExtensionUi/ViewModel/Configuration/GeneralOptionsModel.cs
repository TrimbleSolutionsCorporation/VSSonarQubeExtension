// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneralOptionsModel.cs" company="">
//   
// </copyright>
// <summary>
//   The dummy options controller.
// </summary>
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

    using VSSonarExtensionUi.View;

    using VSSonarPlugins;

    using UserControl = System.Windows.Controls.UserControl;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    [ImplementPropertyChanged]
    public class GeneralOptionsModel : IViewModelBase, IOptionsViewModelBase
    {
        private readonly IVsEnvironmentHelper Vsenvironmenthelper;

        private readonly VSonarQubeOptionsViewModel viewModel;

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
        ///     The dummy control.
        /// </summary>
        private UserControl userControl;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralOptionsModel"/> class.
        /// </summary>
        /// <param name="vsenvironmenthelper">
        /// The Vsenvironmenthelper.
        /// </param>
        public GeneralOptionsModel(IVsEnvironmentHelper vsenvironmenthelper, VSonarQubeOptionsViewModel viewModel)
        {
            this.Vsenvironmenthelper = vsenvironmenthelper;
            this.viewModel = viewModel;
            this.TimeoutValue = 10;
            this.Header = "General Options";
            this.ForeGroundColor = Colors.White;
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
        /// Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        public void RefreshDataForResource(Resource fullName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the browse for java trigger.
        /// </summary>
        public RelayCommand BrowseForJavaTrigger { get; set; }

        /// <summary>
        /// Gets or sets the browse for sonar runner qube trigger.
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
        /// Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        /// Gets or sets the header.
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
        /// The apply.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Apply()
        {
            this.Vsenvironmenthelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.RunnerExecutableKey, this.SonarQubeBinary);
            this.Vsenvironmenthelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.JavaExecutableKey, this.JavaBinary);
            this.Vsenvironmenthelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.LocalAnalysisTimeoutKey, this.TimeoutValue.ToString(CultureInfo.InvariantCulture));
            this.Vsenvironmenthelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.IsDebugAnalysisOnKey, this.DebugIsChecked.ToString());
            this.Vsenvironmenthelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.ExcludedPluginsKey, this.ExcludedPlugins);

            if (this.viewModel.Project != null)
            {
                var solId = VsSonarUtils.SolutionGlobPropKey(this.viewModel.Project.Key);
                this.Vsenvironmenthelper.WriteOptionInApplicationData(solId, GlobalIds.SonarSourceKey, this.SourceDir);
                this.Vsenvironmenthelper.WriteOptionInApplicationData(solId, GlobalIds.SourceEncodingKey, this.SourceEncoding);
            }
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void EndDataAssociation()
        {
        }

        /// <summary>
        /// The exit.
        /// </summary>
        public void Exit()
        {
            var generalOptionsInDsk = this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(GlobalIds.GlobalPropsId);

            if (generalOptionsInDsk != null)
            {
                this.SetGeneralOptions(generalOptionsInDsk);
            }

            if (this.viewModel.Project != null)
            {
                var solId = VsSonarUtils.SolutionGlobPropKey(this.viewModel.Project.Key);
                var projectOptionsInDsk = this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(solId);
                this.SetProjectOptions(projectOptionsInDsk);
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
        /// The save and close.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
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
            this.JavaBinary = this.GetOptionFromDictionary(options, GlobalIds.JavaExecutableKey);
            this.SonarQubeBinary = this.GetOptionFromDictionary(options, GlobalIds.RunnerExecutableKey);
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
            string programFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java");

            if (!string.IsNullOrEmpty(programFiles))
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
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Cannot Find Java: " + ex.StackTrace);
                }
            }
            else
            {
                string programFilesX86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java");
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