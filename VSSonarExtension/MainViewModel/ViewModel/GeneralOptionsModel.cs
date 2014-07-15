// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneralOptionsModel.cs" company="">
//   
// </copyright>
// <summary>
//   The dummy options controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtension.MainViewModel.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;

    using ExtensionTypes;

    using VSSonarExtension.MainView;

    using VSSonarPlugins;

    using UserControl = System.Windows.Controls.UserControl;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    public class GeneralOptionsModel : INotifyPropertyChanged
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
        ///     The debug is checked.
        /// </summary>
        private bool debugIsChecked;

        /// <summary>
        ///     The is solution open.
        /// </summary>
        private bool isSolutionOpen;

        /// <summary>
        ///     The javabinary.
        /// </summary>
        private string javabinary;

        /// <summary>
        ///     The language.
        /// </summary>
        private string language;

        /// <summary>
        ///     The project.
        /// </summary>
        private Resource project;

        /// <summary>
        ///     The project id.
        /// </summary>
        private string projectId;

        /// <summary>
        ///     The project version.
        /// </summary>
        private string projectVersion;

        /// <summary>
        ///     The sonar qube binary.
        /// </summary>
        private string sonarQubeBinary;

        /// <summary>
        ///     The source dir.
        /// </summary>
        private string sourceDir;

        /// <summary>
        ///     The source encoding.
        /// </summary>
        private string sourceEncoding;

        /// <summary>
        ///     The timeout value.
        /// </summary>
        private int timeoutValue = 10;

        /// <summary>
        ///     The dummy control.
        /// </summary>
        private UserControl userControl;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralOptionsModel"/> class.
        /// </summary>
        public GeneralOptionsModel()
        {
            if (string.IsNullOrEmpty(this.SonarQubeBinary))
            {
                this.GetDefaultSonarRunnerLocation();
            }

            if (string.IsNullOrEmpty(this.JavaBinary))
            {
                this.GetDefaultJavaLocationIfAvailable();
            }

            this.ResetDefaults();
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

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
        ///     Gets or sets a value indicating whether browse for java trigger.
        /// </summary>
        public bool BrowseForJavaTrigger
        {
            get
            {
                return true;
            }

            set
            {
                var filedialog = new OpenFileDialog { Filter = @"Java executable|java.exe" };
                DialogResult result = filedialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    if (File.Exists(filedialog.FileName))
                    {
                        this.JavaBinary = filedialog.FileName;
                        this.OnPropertyChanged("JavaBinary");
                    }
                    else
                    {
                        MessageBox.Show(@"Error Choosing File, File Does not exits");
                    }
                }

                this.OnPropertyChanged("BrowseForJavaTrigger");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether browse for sonar runner qube trigger.
        /// </summary>
        public bool BrowseForSonarRunnerQubeTrigger
        {
            get
            {
                return true;
            }

            set
            {
                var filedialog = new OpenFileDialog { Filter = @"SonarQube executable|sonar-runner.bat" };
                DialogResult result = filedialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    if (File.Exists(filedialog.FileName))
                    {
                        this.SonarQubeBinary = filedialog.FileName;
                        this.OnPropertyChanged("SonarQubeBinary");
                    }
                    else
                    {
                        MessageBox.Show(@"Error Choosing File, File Does not exits");
                    }
                }

                this.OnPropertyChanged("BrowseForSonarRunnerQubeTrigger");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether debug is checked.
        /// </summary>
        public bool DebugIsChecked
        {
            get
            {
                return this.debugIsChecked;
            }

            set
            {
                this.debugIsChecked = value;
                this.OnPropertyChanged("DebugIsChecked");
            }
        }

        /// <summary>
        ///     Gets or sets the excluded plugins.
        /// </summary>
        public string ExcludedPlugins { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is solution open.
        /// </summary>
        public bool IsSolutionOpen
        {
            get
            {
                return this.isSolutionOpen;
            }

            set
            {
                this.isSolutionOpen = value;
                this.OnPropertyChanged("IsSolutionOpen");
            }
        }

        /// <summary>
        ///     Gets or sets the java binary.
        /// </summary>
        public string JavaBinary
        {
            get
            {
                return this.javabinary;
            }

            set
            {
                this.javabinary = value;
                this.OnPropertyChanged("JavaBinary");
            }
        }

        /// <summary>
        ///     Gets or sets the language.
        /// </summary>
        public string Language
        {
            get
            {
                return this.language;
            }

            set
            {
                this.language = value;
                this.OnPropertyChanged("Language");
            }
        }

        /// <summary>
        ///     Gets or sets the project.
        /// </summary>
        public Resource Project
        {
            get
            {
                return this.project;
            }

            set
            {
                this.project = value;
                this.IsSolutionOpen = value != null;
            }
        }

        /// <summary>
        ///     Gets or sets the project id.
        /// </summary>
        public string ProjectId
        {
            get
            {
                return this.projectId;
            }

            set
            {
                this.projectId = value;
                this.OnPropertyChanged("ProjectId");
            }
        }

        /// <summary>
        ///     Gets or sets the java binary.
        /// </summary>
        public string ProjectVersion
        {
            get
            {
                return this.projectVersion;
            }

            set
            {
                this.projectVersion = value;
                this.OnPropertyChanged("ProjectVersion");
            }
        }

        /// <summary>
        ///     Gets or sets the sonar qube binary.
        /// </summary>
        public string SonarQubeBinary
        {
            get
            {
                return this.sonarQubeBinary;
            }

            set
            {
                this.sonarQubeBinary = value;
                this.OnPropertyChanged("SonarQubeBinary");
            }
        }

        /// <summary>
        ///     Gets or sets the java binary.
        /// </summary>
        public string SourceDir
        {
            get
            {
                return this.sourceDir;
            }

            set
            {
                this.sourceDir = value;
                this.OnPropertyChanged("SourceDir");
            }
        }

        /// <summary>
        ///     Gets or sets the java binary.
        /// </summary>
        public string SourceEncoding
        {
            get
            {
                return this.sourceEncoding;
            }

            set
            {
                this.sourceEncoding = value;
                this.OnPropertyChanged("SourceEncoding");
            }
        }

        /// <summary>
        ///     Gets or sets the timeout value.
        /// </summary>
        public int TimeoutValue
        {
            get
            {
                return this.timeoutValue;
            }

            set
            {
                this.timeoutValue = value;
                this.OnPropertyChanged("TimeoutValue");
                this.OnPropertyChanged("TimeoutValueString");
            }
        }

        /// <summary>
        ///     Gets the timeout value string.
        /// </summary>
        public string TimeoutValueString
        {
            get
            {
                return "Timeout = " + this.timeoutValue.ToString(CultureInfo.InvariantCulture) + " min";
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The get user control options.
        /// </summary>
        /// <returns>
        ///     The <see cref="UserControl" />.
        /// </returns>
        public UserControl GetUserControlOptions()
        {
            return this.userControl ?? (this.userControl = new GeneralOptionsControl(this));
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
            if (this.Project != null)
            {
                this.SourceDir = this.GetGeneralProjectOptionFromDictionary(options, GlobalIds.SonarSourceKey);
                this.SourceEncoding = this.GetGeneralProjectOptionFromDictionary(options, GlobalIds.SourceEncodingKey);
                this.Language = this.Project.Lang;
                this.ProjectId = this.Project.Key;
                this.ProjectVersion = this.Project.Version;
            }

            this.EnsureProjectMandatoryPropertiesAreSet();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

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
            if (this.Project == null)
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