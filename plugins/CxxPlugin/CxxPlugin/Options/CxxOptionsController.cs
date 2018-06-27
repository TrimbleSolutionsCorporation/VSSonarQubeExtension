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
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using global::CxxPlugin.Commands;

    using GalaSoft.MvvmLight.Command;
    using LocalExtensions;
    using PropertyChanged;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using System.Net;
    using System.IO;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows.Input;
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
        private readonly IConfigurationHelper configuration;

        /// <summary>
        /// The service
        /// </summary>
        private readonly ISonarRestService service;

        /// <summary>
        /// The CXX lint install path
        /// </summary>
        private readonly string cxxLintInstallPath;

        /// <summary>
        ///     The dummy control.
        /// </summary>
        private UserControl cxxControl;

        /// <summary>
        /// The user conf
        /// </summary>
        private ISonarConfiguration userConf;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CxxOptionsController" /> class.
        /// </summary>
        public CxxOptionsController()
        {
            this.cxxControl = null;
            this.OpenCommand = new CxxOpenFileCommand(this, new CxxService());
            this.DownloadCxxLintFromServerCommand = new RelayCommand(this.OnDownloadCxxLintFromServerCommand);
            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CxxOptionsController" /> class.
        /// </summary>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="service">The service.</param>
        public CxxOptionsController(IConfigurationHelper configurationHelper, ISonarRestService service)
        {
            this.cxxLintInstallPath = Path.Combine(configurationHelper.ApplicationPath, "CxxLint");
            Directory.CreateDirectory(this.cxxLintInstallPath);
            this.service = service;
            this.configuration = configurationHelper;
            this.cxxControl = null;
            this.OpenCommand = new CxxOpenFileCommand(this, new CxxService());
            this.DownloadCxxLintFromServerCommand = new RelayCommand(this.OnDownloadCxxLintFromServerCommand);
            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
        }

        /// <summary>Initializes a new instance of the <see cref="CxxOptionsController"/> class.</summary>
        /// <param name="service">The service.</param>
        public CxxOptionsController(ICxxIoService service)
        {
            this.cxxControl = null;
            this.OpenCommand = service != null
                                   ? new CxxOpenFileCommand(this, service)
                                   : new CxxOpenFileCommand(this, new CxxService());

            this.DownloadCxxLintFromServerCommand = new RelayCommand(this.OnDownloadCxxLintFromServerCommand);
            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
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

        /// <summary>
        /// Gets or sets the download CXX lint from server command.
        /// </summary>
        /// <value>
        /// The download CXX lint from server command.
        /// </value>
        public ICommand DownloadCxxLintFromServerCommand { get; set; }

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
        public void OnConnectToSonar(ISonarConfiguration configurationUser)
        {
            if (string.IsNullOrEmpty(this.CxxLint) || !File.Exists(this.CxxLint))
            {
                this.userConf = configurationUser;
                if (!this.DownloadLintFromServer(false))
                {
                    this.UseEmbeddedVersion();
                }
            }
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

        /// <summary>The get option control user interface.</summary>
        /// <returns>The <see cref="UserControl" />.</returns>
        public UserControl GetOptionControlUserInterface()
        {
            return this.cxxControl ?? (this.cxxControl = new CxxUserControl(this));
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
                    this.configuration.ReadSetting(Context.FileAnalysisProperties, "CxxPlugin", key)
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
            this.configuration.WriteSetting(
                Context.FileAnalysisProperties, 
                "CxxPlugin", 
                key, 
                value);
        }


        /// <summary>
        /// Called when [download CXX lint from server command].
        /// </summary>
        private void OnDownloadCxxLintFromServerCommand()
        {
            if (this.userConf == null)
            {
                MessageBox.Show("Unable to download unless extension is connected to Server");
                return;
            }

            if (!this.DownloadLintFromServer(true))
            {
                MessageBox.Show("Will use embedded version, since linter was not found in installed version in server.");
                this.UseEmbeddedVersion();
            }
        }

        /// <summary>
        /// Downloads the lint from server.
        /// </summary>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <returns>true if ok</returns>
        private bool DownloadLintFromServer(bool force)
        {
            try
            {
                var plugins = this.service.GetInstalledPlugins(this.userConf);
                if (!plugins.ContainsKey("C++ (Community)"))
                {
                    this.ErrorMessage = "C++ (Community) not installed in server. Please install version 0.9.5 or above.";
                    return false;
                }

                var cxxPluginVersion = plugins["C++ (Community)"].Replace("\"", string.Empty);
                var urldownload = this.userConf.Hostname + "/static/cxx/cxx-lint-" + cxxPluginVersion + ".jar";
                this.CxxLint = Path.Combine(this.cxxLintInstallPath, "cxx-lint-" + cxxPluginVersion + ".jar");

                if (!File.Exists(this.CxxLint) || force)
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(urldownload, this.CxxLint);
                    }
                }

                this.SaveOption(CxxLintSensor.LinterProp, this.CxxLint);
                return true;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "C++ (Community) is installed but older than 0.9.5. Please install version 0.9.5 or above.";
                return false;
            }
        }

        /// <summary>
        /// Uses the embedded version.
        /// </summary>
        private void UseEmbeddedVersion()
        {
            // use embedded version
            this.CxxLint = Path.Combine(this.cxxLintInstallPath, "cxx-lint-0.9.5-SNAPSHOT.jar");
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