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
    using GalaSoft.MvvmLight.Command;
    using global::CxxPlugin.Commands;
    using LocalExtensions;
    using PropertyChanged;
    using SonarRestService;
    using SonarRestService.Types;
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
        /// tools path
        /// </summary>
        private readonly string toolsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".vssonarextension");

        public CxxOptionsController()
        {
            this.OpenCommand = new CxxOpenFileCommand(this, new CxxService());
            this.ConfigureToolsCommand = new RelayCommand(this.OnConfigureToolsCommand);
            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
            this.CxxVersion = "3.2.2";
            this.SetOptions();
            Task.Run(() => OnConfigureToolsCommand());
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
            this.OpenCommand = new CxxOpenFileCommand(this, new CxxService());
            this.ConfigureToolsCommand = new RelayCommand(this.OnConfigureToolsCommand);
            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
            this.CxxVersion = "3.2.2";
            this.SetOptions();
            Task.Run(() => OnConfigureToolsCommand());
        }

        /// <summary>The get option control user interface.</summary>
        /// <returns>The <see cref="UserControl" />.</returns>
        public UserControl GetOptionControlUserInterface()
        {
            return this.cxxControl ?? (this.cxxControl = new CxxUserControl(this));
        }

        private async void OnConfigureToolsCommand()
        {
            if (this.IsDownloading || File.Exists(Path.Combine(this.toolsPath, "downloadlock")))
            {
                this.IsDownloading = File.Exists(Path.Combine(this.toolsPath, "downloadlock"));
                MessageBox.Show("Please Wait For Tools to Donwload");
                return;
            }
            var installPath = Path.Combine(this.toolsPath, "Wrapper", this.CxxVersion);

            if (!File.Exists(Path.Combine(installPath, "CxxSonarQubeRunnerWin.bat")))
            {
                Directory.CreateDirectory(this.toolsPath);
                File.Create(Path.Combine(this.toolsPath, "downloadlock"));
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

                File.Delete(Path.Combine(this.toolsPath, "downloadlock"));
                File.Delete(tmpFile);
            }

            // setup tools
            var toolsPath = Path.Combine(installPath, "Tools");
            if (File.Exists(Path.Combine(toolsPath, "Cppcheck", "Cppcheck.exe")))
            {
                if (!File.Exists(this.CppCheckExecutable))
                {
                    this.CppCheckExecutable = Path.Combine(toolsPath, "Cppcheck", "Cppcheck.exe");
                }
            }

            if (File.Exists(Path.Combine(toolsPath, "CppLint", "cpplint_mod.py")))
            {
                if (!File.Exists(this.CustomExecutable))
                {
                    this.CustomArguments = Path.Combine(toolsPath, "CppLint", "cpplint_mod.py") + " --output=vs7";
                    this.CustomExecutable = Path.Combine(toolsPath, "Python27", "python.exe");
                }
            }

            if (File.Exists(Path.Combine(toolsPath, "rats", "rats.exe")))
            {
                if (!File.Exists(this.RatsExecutable))
                {
                    this.RatsExecutable = Path.Combine(toolsPath, "rats", "rats.exe");
                }
            }

            if (File.Exists(Path.Combine(toolsPath, "vera", "bin", "vera++.exe")))
            {
                if (!File.Exists(this.VeraExecutable))
                {
                    this.VeraExecutable = Path.Combine(toolsPath, "vera", "bin", "vera++.exe");
                }
            }

            if (!File.Exists(this.ClangTidyExecutable))
            {
                this.ClangTidyExecutable = @"C:\Program Files\LLVM\bin\clang-tidy.exe";
            }

            this.UseEmbeddedVersion();
            this.IsDownloading = false;
            SaveDataInUi(null, configuration);
        }


        /// <summary>Initializes a new instance of the <see cref="CxxOptionsController"/> class.</summary>
        /// <param name="service">The service.</param>
        public CxxOptionsController(ICxxIoService service)
        {
            this.cxxControl = null;
            this.OpenCommand = service != null
                                   ? new CxxOpenFileCommand(this, service)
                                   : new CxxOpenFileCommand(this, new CxxService());

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

        /// <summary>Gets or sets the cpp check arguments.</summary>
        public string CppCheckArguments { get; set; }

        /// <summary>Gets or sets the cpp check environment.</summary>
        public string CppCheckEnvironment { get; set; }

        /// <summary>
        /// Gets or sets the clang tidy executable.
        /// </summary>
        /// <value>
        /// The clang tidy executable.
        /// </value>
        public string ClangTidyExecutable { get; set; }

        /// <summary>Gets or sets the cpp check executable.</summary>
        public string CppCheckExecutable { get; set; }

        /// <summary>Gets or sets the custom arguments.</summary>
        public string CustomArguments { get; set; }

        /// <summary>Gets or sets the custom environment.</summary>
        public string CustomEnvironment { get; set; }

        /// <summary>Gets or sets the custom executable.</summary>
        public string CustomExecutable { get; set; }

        /// <summary>Gets or sets the custom key.</summary>
        public string CustomKey { get; set; }

        /// <summary>Gets or sets the open command.</summary>
        public CxxOpenFileCommand OpenCommand { get; set; }

        public ICommand ConfigureToolsCommand { get; set; }

        /// <summary>Gets or sets the project.</summary>
        public Resource Project { get; set; }

        /// <summary>Gets or sets the properties to runner.</summary>
        public string PropertiesToRunner { get; set; }

        /// <summary>Gets or sets the rats environment.</summary>
        public string RatsEnvironment { get; set; }

        /// <summary>Gets or sets the rats executable.</summary>
        public string RatsExecutable { get; set; }

        /// <summary>Gets or sets the rats arguments.</summary>
        public string RatsArguments { get; set; }

        /// <summary>Gets or sets the vera arguments.</summary>
        public string VeraArguments { get; set; }

        /// <summary>Gets or sets the vera environment.</summary>
        public string VeraEnvironment { get; set; }

        /// <summary>Gets or sets the vera executable.</summary>
        public string VeraExecutable { get; set; }

        /// <summary>Gets or sets the pc lint executable.</summary>
        public string PcLintExecutable { get; set; }

        /// <summary>Gets or sets the pc lint arguments.</summary>
        public string PcLintArguments { get; set; }

        /// <summary>Gets or sets the pc lint environment.</summary>
        public string PcLintEnvironment { get; set; }

        /// <summary>Gets or sets the project working dir.</summary>
        public string ProjectWorkingDir { get; set; }

        /// <summary>Gets or sets a value indicating whether project is associated.</summary>
        public bool ProjectIsAssociated { get; set; }

        /// <summary>
        /// Gets the CXX lint.
        /// </summary>
        /// <value>
        /// The CXX lint.
        /// </value>
        public string CxxLint { get; set; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get; private set; }
        public bool IsDownloading { get; private set; }
        public string CxxVersion { get; set; }

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
        public bool OnConnectToSonar(ISonarConfiguration configurationUser)
        {
            if (string.IsNullOrEmpty(this.CxxLint) || !File.Exists(this.CxxLint))
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
            this.CxxLint = this.GetOptionIfExists(CxxLintSensor.LinterProp);

            this.VeraExecutable = this.GetOptionIfExists("VeraExecutable");
            this.VeraArguments = this.GetOptionIfExists("VeraArguments");
            this.VeraEnvironment = this.GetOptionIfExists("VeraEnvironment");

            this.PcLintExecutable = this.GetOptionIfExists("PcLintExecutable");
            this.PcLintArguments = this.GetOptionIfExists("PcLintArguments");
            this.PcLintEnvironment = this.GetOptionIfExists("PcLintEnvironment");

            this.RatsExecutable = this.GetOptionIfExists("RatsExecutable");
            this.RatsArguments = this.GetOptionIfExists("RatsArguments");
            this.RatsEnvironment = this.GetOptionIfExists("RatsEnvironment");

            this.CppCheckExecutable = this.GetOptionIfExists("CppCheckExecutable");
            this.CppCheckArguments = this.GetOptionIfExists("CppCheckArguments");
            this.CppCheckEnvironment = this.GetOptionIfExists("CppCheckEnvironment");

            this.CustomExecutable = this.GetOptionIfExists("CustomExecutable");
            this.CustomArguments = this.GetOptionIfExists("CustomArguments");
            this.CustomKey = this.GetOptionIfExists("CustomKey");
            this.CustomEnvironment = this.GetOptionIfExists("CustomEnvironment");

            this.ClangTidyExecutable = this.GetOptionIfExists("ClangExecutable");
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
            this.SaveOption(CxxLintSensor.LinterProp, this.CxxLint);

            this.SaveOption("VeraExecutable", this.VeraExecutable);
            this.SaveOption("VeraArguments", this.VeraArguments);
            this.SaveOption("VeraEnvironment", this.VeraEnvironment);

            this.SaveOption("PcLintExecutable", this.PcLintExecutable);
            this.SaveOption("PcLintArguments", this.PcLintArguments);
            this.SaveOption("PcLintEnvironment", this.PcLintEnvironment);

            this.SaveOption("RatsExecutable", this.RatsExecutable);
            this.SaveOption("RatsArguments", this.RatsArguments);
            this.SaveOption("RatsEnvironment", this.RatsEnvironment);

            this.SaveOption("CppCheckExecutable", this.CppCheckExecutable);
            this.SaveOption("CppCheckArguments", this.CppCheckArguments);
            this.SaveOption("CppCheckEnvironment", this.CppCheckEnvironment);

            this.SaveOption("CustomExecutable", this.CustomExecutable);
            this.SaveOption("CustomArguments", this.CustomArguments);
            this.SaveOption("CustomKey", this.CustomKey);
            this.SaveOption("CustomEnvironment", this.CustomEnvironment);

            this.SaveOption("ClangExecutable", this.ClangTidyExecutable);
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
            this.CxxLint = Path.Combine(this.toolsPath, "cxx-lint-0.9.5-SNAPSHOT.jar");
            this.WriteResourceToFile("CxxPlugin.Resources.cxx-lint-0.9.5-SNAPSHOT.jar", this.CxxLint);
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
                    this.SaveOption(CxxLintSensor.LinterProp, this.CxxLint);
                }
            }
        }
    }
}