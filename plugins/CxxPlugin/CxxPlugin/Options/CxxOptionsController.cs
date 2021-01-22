// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxOptionsController.cs" company="Copyright © 2014 jmecsoftware">
//   Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
// </copyright>
// <summary>
//   The dummy options controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CxxPlugin.Options
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    using GalaSoft.MvvmLight.Command;

    using global::CxxPlugin.Commands;

    using PropertyChanged;

    using SonarRestService;
    using SonarRestService.Types;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// The dummy options controller.
    /// </summary>
    /// <seealso cref="VSSonarPlugins.IPluginControlOption" />
    [AddINotifyPropertyChangedInterface]
    public class CxxOptionsController : IPluginControlOption
    {
        /// <summary>
        ///     The configuration.
        /// </summary>
        private static IConfigurationHelper configuration;

        /// <summary>
        /// The service
        /// </summary>
        private readonly ISonarRestService service;

        /// <summary>
        ///     The dummy control.
        /// </summary>
        private UserControl cxxControl;

        /// <summary>
        /// The user conf
        /// </summary>
        private ISonarConfiguration userConf;

        /// <summary>
        /// Cxx options controller
        /// </summary>
        public CxxOptionsController()
        {
            this.ConfigureToolsCommand = new RelayCommand(this.OnConfigureToolsCommand);
            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
            this.CxxSettings = CxxConfiguration.CxxSettings;
            this.CxxVersion = this.CxxSettings.CxxVersion;
            this.LocationForConfigurationFile = CxxConfiguration.SettingsFile;
            this.SetOptions();
            Task.Run(() => this.OnConfigureToolsCommand());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CxxOptionsController" /> class.
        /// </summary>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="service">The service.</param>
        public CxxOptionsController(IConfigurationHelper configurationHelper, ISonarRestService service)
        {
            this.service = service;
            configuration = configurationHelper;
            this.cxxControl = null;
            this.ConfigureToolsCommand = new RelayCommand(this.OnConfigureToolsCommand);
            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
            this.CxxSettings = CxxConfiguration.CxxSettings;
            this.CxxVersion = this.CxxSettings.CxxVersion;
            this.SetOptions();
            this.LocationForConfigurationFile = CxxConfiguration.SettingsFile;
            Task.Run(() => this.OnConfigureToolsCommand());
        }

        /// <summary>Initializes a new instance of the <see cref="CxxOptionsController"/> class.</summary>
        /// <param name="service">The service.</param>
        public CxxOptionsController(ICxxIoService service)
        {
            this.cxxControl = null;

            this.ConfigureToolsCommand = new RelayCommand(this.OnConfigureToolsCommand);
            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
            this.CxxVersion = "3.2.2";
        }

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        /// Gets Cxx Settings
        /// </summary>
        public CxxSettings CxxSettings { get; private set; }

        /// <summary>
        /// Gets or sets configure tools command
        /// </summary>
        public ICommand ConfigureToolsCommand { get; set; }

        /// <summary>Gets or sets the project.</summary>
        public Resource Project { get; set; }

        /// <summary>Gets or sets the properties to runner.</summary>
        public string PropertiesToRunner { get; set; }

        /// <summary>Gets or sets the project working dir.</summary>
        public string ProjectWorkingDir { get; set; }

        /// <summary>Gets or sets a value indicating whether project is associated.</summary>
        public bool ProjectIsAssociated { get; set; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get; private set; }

        /// <summary>Gets a value indicating whether project is associated.</summary>
        public bool IsDownloading { get; private set; }

        /// <summary>
        /// Gets or sets version
        /// </summary>
        public string CxxVersion { get; set; }
        public string LocationForConfigurationFile { get; private set; }

        /// <summary>
        /// Gets CxxSettingsData
        /// </summary>
        public string CxxSettingsData { get; set; }

        /// <summary>The get option control user interface.</summary>
        /// <returns>The <see cref="UserControl" />.</returns>
        public UserControl GetOptionControlUserInterface()
        {
            return this.cxxControl ?? (this.cxxControl = new CxxUserControl(this));
        }

        /// <summary>The get user control options.</summary>
        /// <param name="projectIn">The project.</param>
        /// <returns>The <see cref="UserControl"/>.</returns>
        public UserControl GetUserControlOptions(Resource projectIn)
        {
            this.Project = projectIn;
            return this.cxxControl ?? (this.cxxControl = new CxxUserControl(this));
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configurationUser">The configuration user.</param>
        /// <returns>true if ok</returns>
        public bool OnConnectToSonar(ISonarConfiguration configurationUser)
        {
            if (string.IsNullOrEmpty(this.CxxSettings.LinterProp) || !File.Exists(this.CxxSettings.LinterProp))
            {
                this.userConf = configurationUser;
                this.UseEmbeddedVersion();
            }

            return true;
        }

        /// <summary>
        ///     The set options.
        /// </summary>
        public void SetOptions()
        {
            this.CxxSettingsData = File.ReadAllText(CxxConfiguration.SettingsFile);
        }

        /// <summary>The refresh data in ui.</summary>
        /// <param name="project">The project.</param>
        /// <param name="helper">The helper.</param>
        public void RefreshDataInUi(Resource project, IConfigurationHelper helper)
        {
            // read properties
            this.SetOptions();

            this.Project = project;

            if (this.Project == null)
            {
                this.ProjectIsAssociated = false;
                return;
            }

            if (string.IsNullOrEmpty(this.Project.Lang))
            {
                this.ProjectIsAssociated = true;
                return;
            }

            this.ProjectIsAssociated = CxxPlugin.IsSupported(this.Project);
        }

        /// <summary>The save data in ui.</summary>
        /// <param name="project">The project.</param>
        /// <param name="helper">The helper.</param>
        public void SaveDataInUi(Resource project, IConfigurationHelper helper)
        {
            this.CxxSettingsData = File.ReadAllText(CxxConfiguration.SettingsFile);
        }

        /// <summary>The refresh colours.</summary>
        /// <param name="foreground">The foreground.</param>
        /// <param name="background">The background.</param>
        public void RefreshColours(Color foreground, Color background)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        /// <summary>The get option if exists.</summary>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetOptionIfExists(string key)
        {
            try
            {
                return
                    configuration.ReadSetting(Context.FileAnalysisProperties, "CxxPlugin", key)
                        .Value;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>The save option.</summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void SaveOption(string key, string value)
        {
            configuration.WriteSetting(
                Context.FileAnalysisProperties,
                "CxxPlugin",
                key,
                value);
        }

        /// <summary>
        /// Uses the embedded version.
        /// </summary>
        private void UseEmbeddedVersion()
        {
            // use embedded version
            this.CxxSettings.LinterProp = Path.Combine(CxxConfiguration.SettingsFolder, "cxx-lint-0.9.5-SNAPSHOT.jar");
            this.WriteResourceToFile("CxxPlugin.Resources.cxx-lint-0.9.5-SNAPSHOT.jar", this.CxxSettings.LinterProp);
        }

        /// <summary>
        /// Writes the resource to file.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="fileName">Name of the file.</param>
        private void WriteResourceToFile(string resourceName, string fileName)
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }

        private async void OnConfigureToolsCommand()
        {
            if (this.IsDownloading || File.Exists(CxxConfiguration.TempDownloadLockFile))
            {
                this.IsDownloading = File.Exists(CxxConfiguration.TempDownloadLockFile);
                return;
            }

            this.UseEmbeddedVersion();

            var installPath = Path.Combine(CxxConfiguration.WrapperFolder, this.CxxSettings.CxxVersion);

            if (!File.Exists(Path.Combine(installPath, "CxxSonarQubeRunnerWin.bat")))
            {
                Directory.CreateDirectory(CxxConfiguration.SettingsFolder);
                File.Create(CxxConfiguration.TempDownloadLockFile);
                this.IsDownloading = true;
                var urldownload = $"https://github.com/jmecsoftware/CxxSonarQubeRunner/releases/download/{this.CxxVersion}/CxxSonarQubeRunner.zip";
                var tmpFile = Path.Combine(Path.GetTempPath(), "CxxSonarQubeRunner.zip");
                try
                {
                    using (var client = new WebClient())
                    {
                        if (File.Exists(tmpFile))
                        {
                            File.Delete(tmpFile);
                        }

                        await client.DownloadFileTaskAsync(urldownload, tmpFile);

                        ZipFile.ExtractToDirectory(tmpFile, installPath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Wrapper installation failed : " + ex.Message + " please be sure you have access to url." +
                        "If not download manually and change the wrapper path: https://github.com/jmecsoftware/CxxSonarQubeRunner/releases");
                }

                if (!File.Exists(Path.Combine(installPath, "CxxSonarQubeRunnerWin.bat")))
                {
                    MessageBox.Show(
                        "Wrapper installation failed, expected in  " + Path.Combine(installPath, "CxxSonarQubeRunnerWin.bat") +
                        " : download manually and unzip to that folder: https://github.com/jmecsoftware/CxxSonarQubeRunner/releases");
                    return;
                }

                File.Delete(CxxConfiguration.TempDownloadLockFile);
                File.Delete(tmpFile);

                // setup tools
                this.CxxSettings = CxxConfiguration.CreateSettingsElement(this.CxxSettings.CxxVersion);
                CxxConfiguration.SyncSettings(this.CxxSettings);
            }

            this.IsDownloading = false;
            this.SaveDataInUi(null, configuration);
        }
    }
}
