namespace VSSonarExtensionUi.Model.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using Association;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;
    using System.Net;
    using System.IO.Compression;
    using SonarRestService;
    using SonarRestService.Types;
    using System.Threading.Tasks;

    /// <summary>
    /// Roslyn manager model
    /// </summary>
    public class RoslynManagerModel : IOptionsModelBase
    {
        /// <summary>
        /// The identifier
        /// </summary>
        private const string ID = "RoslynManagerModel";

        /// <summary>
        /// The plugins
        /// </summary>
        private readonly IEnumerable<IAnalysisPlugin> plugins;

        /// <summary>
        /// The notification manager
        /// </summary>
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// The conf helper
        /// </summary>
        private readonly IConfigurationHelper confHelper;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// The rest
        /// </summary>
        private readonly ISonarRestService rest;

        /// <summary>
        /// The roslyn default path
        /// </summary>
        private readonly string roslynHomePath;

        /// <summary>
        /// The roslyn external user diag path
        /// </summary>
        private readonly string roslynEmbbedDiagPath;

        /// <summary>
        /// The ver sion controller
        /// </summary>
        private readonly EmbbedVersionController embedVersionController;

        /// <summary>
        /// The roslyn external user diag path
        /// </summary>
        private readonly string roslynExternalUserDiagPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynManagerModel" /> class.
        /// </summary>
        /// <param name="pluginsIn">The plugins in.</param>
        /// <param name="notificationManagerIn">The notification manager in.</param>
        /// <param name="configHelper">The configuration helper.</param>
        public RoslynManagerModel(
            IEnumerable<IAnalysisPlugin> pluginsIn,
            INotificationManager notificationManagerIn,
            IConfigurationHelper configHelper,
            ISonarRestService rest)
        {
            this.roslynHomePath = Path.Combine(configHelper.ApplicationPath, "Diagnostics");
            this.roslynExternalUserDiagPath = Path.Combine(this.roslynHomePath, "UserDiagnostics");
            this.roslynEmbbedDiagPath = Path.Combine(this.roslynHomePath, "InternalDiagnostics");

            if (!Directory.Exists(this.roslynHomePath))
            {
                Directory.CreateDirectory(this.roslynHomePath);
            }

            if (!Directory.Exists(this.roslynExternalUserDiagPath))
            {
                Directory.CreateDirectory(this.roslynExternalUserDiagPath);
            }

            if (!Directory.Exists(this.roslynEmbbedDiagPath))
            {
                Directory.CreateDirectory(this.roslynEmbbedDiagPath);
            }

            this.rest = rest;
            this.confHelper = configHelper;
            this.plugins = pluginsIn;
            this.notificationManager = notificationManagerIn;
            this.ExtensionDiagnostics = new Dictionary<string, VSSonarExtensionDiagnostic>();
            this.embedVersionController = new EmbbedVersionController(this.notificationManager, rest, configHelper, this.roslynEmbbedDiagPath);

            // register model
            AssociationModel.RegisterNewModelInPool(this);
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        public void OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IList<IIssueTrackerPlugin> issuePlugin)
        {
            this.ExtensionDiagnostics.Clear();
            this.embedVersionController.InitializedServerDiagnostics(configuration);
            this.InitializedServerDiagnostics(configuration);
        }

        /// <summary>
        /// Automatics the detect installed analyzers.
        /// </summary>
        public string AutoDetectInstalledAnalysers()
        {
            var paths = new Dictionary<string, string>();
            using (var workspace = MSBuildWorkspace.Create())
            {
                var solution = workspace.OpenSolutionAsync(Path.Combine(this.associatedProject.SolutionRoot, this.associatedProject.SolutionName)).Result;

                foreach (var item in solution.Projects)
                {
                    foreach (var analyser in item.AnalyzerReferences)
                    {
                        var name = Path.GetFileNameWithoutExtension(analyser.FullPath);
                        if (!paths.ContainsKey(name) && !name.Contains("SonarLint") && !name.Contains("SonarAnalyzer"))
                        {
                            paths.Add(name, analyser.FullPath);
                        }
                    }
                }
            }

            var isOk = true;
            foreach (var item in paths)
            {
                if (!this.AddNewRoslynPack(item.Value, false))
                {
                    this.notificationManager.WriteMessageToLog("Failed to add roslyn dll: " + item.Value);
                    isOk = false;
                }
            }

            return isOk ? paths.Count == 0 ? "No diagnostics found" : "" : "Not all diagnostics were imported, check output log";
        }

        /// <summary>
        /// Verifies the existence of roslyn plugin.
        /// </summary>
        /// <returns></returns>
        public bool VerifyExistenceOfRoslynPlugin()
        {
            try
            {
                var installedplugins = this.rest.GetInstalledPlugins(AuthtenticationHelper.AuthToken);
                return installedplugins.ContainsKey("Roslyn");
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the extension diagnostics.
        /// </summary>
        /// <value>
        /// The extension diagnostics.
        /// </value>
        public Dictionary<string, VSSonarExtensionDiagnostic> ExtensionDiagnostics { get; private set; }

        /// <summary>
        /// Gets the source directory.
        /// </summary>
        /// <value>
        /// The source directory.
        /// </value>
        public string SourceDir { get; private set; }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        /// <value>
        /// The profile.
        /// </value>
        public Dictionary<string, Profile> Profile { get; private set; }

        /// <summary>
        /// Reloads the data from disk.
        /// </summary>
        /// <param name="associatedProjectIn">The associated project in.</param>
        public async Task ReloadDataFromDisk(Resource associatedProjectIn)
        {
			// does not save data to disk
			await Task.Delay(1);
        }

        /// <summary>
        /// Removes the DLL from list.
        /// </summary>
        /// <param name="selectedDllDiagnostic">The selected DLL diagnostic.</param>
        internal void RemoveDllFromList(VSSonarExtensionDiagnostic selectedDllDiagnostic)
        {
            this.ExtensionDiagnostics.Remove(selectedDllDiagnostic.Name);
        }

        /// <summary>
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vs environment helper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public void UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            // does not access visual studio services
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public void OnDisconnect()
        {
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>
        /// returns view model
        /// </returns>
        public object GetViewModel()
        {
            return null;
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void OnSolutionClosed()
        {
            this.associatedProject = null;
            this.SourceDir = string.Empty;
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        public void SaveData()
        {
            this.SyncSettings();
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workDir">The work directory.</param>
        /// <param name="sourceModelIn">The source model in.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public void AssociateWithNewProject(
            Resource project,
            string workDir,
            ISourceControlProvider sourceModelIn,
            Dictionary<string, Profile> profile,
            string visualStudioVersion)
        {
            this.Profile = profile;
            this.SourceDir = workDir;
            this.associatedProject = project;
        }

        /// <summary>
        /// Adds the new roslyn pack.
        /// </summary>
        /// <param name="dllPath">The DLL path.</param>
        /// <returns>true if ok</returns>
        public bool AddNewRoslynPack(string dllPath, bool updateProps)
        {
            var name = Path.GetFileName(dllPath);

            if (!File.Exists(dllPath))
            {
                return false;
            }

            try
            {
                var diagnostic = new VSSonarExtensionDiagnostic(name, dllPath);

                if (this.ExtensionDiagnostics.ContainsKey(name))
                {
                    this.notificationManager.ReportMessage(new Message { Id = "RoslynManager", Data = name + " already added to list." });
                    return false;
                }

                if (diagnostic.AvailableChecks.Count != 0)
                {
                    this.SyncDiagnosticInServer(diagnostic);
                    this.ExtensionDiagnostics.Add(name, diagnostic);
                }
                else
                {
                    this.notificationManager.ReportMessage(new Message { Id = "RoslynManager", Data = name + " contains 0 diagnostics, will not import. Perhaps its a reference." });
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.notificationManager.ReportMessage(new Message { Id = "RoslynManager", Data = name + " failed, will skip: " + ex.Message });
                this.notificationManager.ReportException(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Synchronizes the diagnostic in server.
        /// </summary>
        /// <param name="diagnostic">The diagnostic.</param>
        private void SyncDiagnosticInServer(VSSonarExtensionDiagnostic diagnostic)
        {
            if (diagnostic.AvailableChecks.Count == 0)
            {
                return;
            }

            foreach (var check in diagnostic.AvailableChecks)
            {
                foreach (var diag in check.Diagnostic.SupportedDiagnostics)
                {
                    foreach (var lang in check.Languages)
                    {
                        var language = GetLanguage(lang);
                        this.CreateRule(language, diag);
                    }
                }
            }
        }

        private void CreateRule(string language, DiagnosticDescriptor diag)
        {
            var repoid = "roslyn-" + language;

            var templaterule = new Rule();
            templaterule.Name = "Template Rule";
            templaterule.Key = "roslyn-cs:TemplateRule";

            var desc = string.Format("<p>{0}<a href=\"{1}\">Help Url</a></p>", (diag.Description.ToString()), diag.HelpLinkUri);
            var markdown = string.Format("*{0}* [Help Url]({1})", diag.Description.ToString(), diag.HelpLinkUri);

            var rule = new Rule();
            rule.HtmlDescription = desc;
            rule.MarkDownDescription = markdown;
            rule.Key = repoid + ":" + diag.Id;
            rule.Name = diag.Title.ToString();
            rule.Repo = repoid;
            rule.Severity = Severity.MAJOR;
            foreach (var error in this.rest.CreateRule(AuthtenticationHelper.AuthToken, rule, templaterule))
            {
                this.notificationManager.WriteMessageToLog("Failed to create rule: " + error);
            }
        }

        private string GetLanguage(string lang)
        {
            if (lang.Equals("C#"))
            {
                return "cs";
            }
            else
            {
                return "vbnet";
            }
        }

        private Rule GetRuleFromProfile(string lang, string id)
        {
            if (this.Profile == null)
            {
                return null;
            }

            if (lang.Equals("C#"))
            {
                var profile = this.Profile["cs"];
                if (profile != null)
                {
                    return profile.GetRule("roslyn-cs:" + id);
                }
            }
            else
            {
                var profile = this.Profile["vbnet"];
                if (profile != null)
                {
                    return profile.GetRule("roslyn-cs:" + id);
                }
            }

            return null;
        }

        /// <summary>
        /// Synchronizes the diagnostics in plugins.
        /// </summary>
        public void SyncDiagnosticsInPlugins()
        {
            foreach (var plugin in this.plugins)
            {
                try
                {
                    var roslynPlugin = plugin as IRoslynPlugin;
                    if (roslynPlugin != null)
                    {
                        roslynPlugin.SetDiagnostics(this.CombineDiagnostics());
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Synchronizes the settings.
        /// </summary>
        private void SyncSettings()
        {
            this.SyncDiagnosticsInPlugins();
        }

        /// <summary>
        /// Combines the diagnostics.
        /// </summary>
        /// <returns>returns diagnostics</returns>
        private List<DiagnosticAnalyzerType> CombineDiagnostics()
        {
            var listData = new List<DiagnosticAnalyzerType>();

            foreach (var item in this.ExtensionDiagnostics)
            {
                listData.AddRange(item.Value.AvailableChecks);
            }

            return listData;
        }

        /// <summary>
        /// Initializes the installed diagnostics.
        /// </summary>
        public void InitializedServerDiagnostics(ISonarConfiguration authentication)
        {
            if (!this.ExtensionDiagnostics.Any())
            {
                bool hasRoslynPlugin = VerifyExistenceOfRoslynPlugin();

                // load defined props in server and load up
                var props = this.rest.GetSettings(authentication);
                var isRoslynPath = props.FirstOrDefault(x => x.key.Equals("sonar.roslyn.diagnostic.path"));

                if (isRoslynPath != null)
                {
                    var folders = isRoslynPath.Value.Split(';');
                    foreach (var folder in folders)
                    {
                        if (Directory.Exists(folder) && !this.roslynExternalUserDiagPath.ToLower().Equals(folder.ToLower()))
                        {
                            this.LoadDiagnosticsFromPath(folder, hasRoslynPlugin);
                        }
                    }
                }

                this.LoadDiagnosticsFromPath(this.roslynExternalUserDiagPath, hasRoslynPlugin);
                
                foreach (var item in this.embedVersionController.GetInstalledPaths())
                {
                    try
                    {
                        this.LoadDiagnosticsFromPath(item, false);
                    }
                    catch (Exception ex)
                    {
                        this.notificationManager.WriteMessageToLog("Failed to load diagnostics from: " + item + " : " + ex.Message);
                    }
                }                    
            }

            this.SyncSettings();
        }

        private void LoadDiagnosticsFromPath(string folderPath, bool syncInServer)
        {
            var diagnostics = Directory.GetFiles(folderPath);

            foreach (var diagnostic in diagnostics)
            {
                try
                {
                    var fileName = Path.GetFileName(diagnostic);

                    if (this.ExtensionDiagnostics.ContainsKey(fileName))
                    {
                        continue;
                    }

                    var newdata = new VSSonarExtensionDiagnostic(fileName, diagnostic);

                    if (newdata.AvailableChecks.Count > 0)
                    {
                        this.ExtensionDiagnostics.Add(fileName, newdata);

                        if (syncInServer)
                        {
                            this.SyncDiagnosticInServer(newdata);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.notificationManager.WriteMessageToLog("Failed to load user diagnostics from: " + diagnostic + " : " + ex.Message);
                }
            }
        }
    }
}
