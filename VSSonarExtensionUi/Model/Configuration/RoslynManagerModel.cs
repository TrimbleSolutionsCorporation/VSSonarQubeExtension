namespace VSSonarExtensionUi.Model.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Helpers;
    using Microsoft.CodeAnalysis.Diagnostics;
    using SonarLocalAnalyser;
    using ViewModel;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using Association;
    using View.Helpers;
    using Microsoft.CodeAnalysis;

    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.MSBuild;


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
        /// The pathkey
        /// </summary>
        private const string PATHKEY = "RoslynDllPaths";

        /// <summary>
        /// The plugins
        /// </summary>
        private readonly IEnumerable<IAnalysisPlugin> plugins;

        /// <summary>
        /// The notification manager
        /// </summary>
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// The plugins running path
        /// </summary>
        private readonly string pluginsRunningPath = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty);

        /// <summary>
        /// The extension running path
        /// </summary>
        private readonly string extensionRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty)).ToString();

        /// <summary>
        /// The extension running path
        /// </summary>
        private readonly string extensionsBasePath = Directory.GetParent(Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty)).ToString()).ToString();

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
            this.rest = rest;
            this.confHelper = configHelper;
            this.plugins = pluginsIn;
            this.notificationManager = notificationManagerIn;
            this.ExtensionDiagnostics = new Dictionary<string, VSSonarExtensionDiagnostic>();
            this.InitializedInstalledDiagnostics();

            // register model
            AssociationModel.RegisterNewModelInPool(this);
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
                        if (!paths.ContainsKey(name) && !name.Contains("SonarLint"))
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
                    this.notificationManager.WriteMessage("Failed to add roslyn dll: " + item.Value);
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
        public Dictionary<string, Profile> Profile { get; private set; }

        /// <summary>
        /// Reloads the data from disk.
        /// </summary>
        /// <param name="associatedProjectIn">The associated project in.</param>
        public void ReloadDataFromDisk(Resource associatedProjectIn)
        {
            // does not save data to disk
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
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
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
        /// <param name="workDir">The work dir.</param>
        /// <param name="sourceModelIn">The source model in.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public void AssociateWithNewProject(Resource project, string workDir, ISourceControlProvider sourceModelIn, IIssueTrackerPlugin sourcePlugin, IList<Resource> availableProjects, Dictionary<string, Profile> profile)
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
                    this.SyncDiagnosticInServer(diagnostic, updateProps);
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
        private void SyncDiagnosticInServer(VSSonarExtensionDiagnostic diagnostic, bool updateProperties)
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

            if (updateProperties)
            {
                if (this.Profile == null)
                {
                    this.UpdateDiagnosticPathInServer(this.rest.GetProperties(AuthtenticationHelper.AuthToken), diagnostic.Path);
                }
                else
                {
                    this.UpdateDiagnosticPathInServer(this.rest.GetProperties(AuthtenticationHelper.AuthToken, this.associatedProject), diagnostic.Path);
                }
            }
        }

        /// <summary>
        /// Updates the diagnostic path in server.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="path">The path.</param>
        private void UpdateDiagnosticPathInServer(Dictionary<string, string> properties, string path)
        {
            if (properties.ContainsKey("sonar.roslyn.diagnostic.path"))
            {
                var value = properties["sonar.roslyn.diagnostic.path"] + ";" + path;
                this.rest.UpdateProperty(AuthtenticationHelper.AuthToken, "sonar.roslyn.diagnostic.path", value, null);
            }
            else
            {
                this.rest.UpdateProperty(AuthtenticationHelper.AuthToken, "sonar.roslyn.diagnostic.path", path, null);
            }
        }

        private void CreateRule(string language, DiagnosticDescriptor diag)
        {
            var repoid = "roslyn-" + language;

            var templaterule = new Rule();
            templaterule.Name = "Template Rule";
            templaterule.Key = "roslyn-cs:TemplateRule";

            var desc = string.Format("<p>%s<a href=\"%s\">Help Url</a></p>", (diag.Description.ToString()), diag.HelpLinkUri);
            var markdown = string.Format("*%s* [Help Url](%s)", diag.Description.ToString(), diag.HelpLinkUri);

            var rule = new Rule();
            rule.HtmlDescription = desc;
            rule.MarkDownDescription = markdown;
            rule.Key = repoid + ":" + diag.Id;
            rule.Name = diag.Title.ToString();
            rule.Repo = repoid;
            rule.Severity = Severity.MAJOR;
            foreach (var error in this.rest.CreateRule(AuthtenticationHelper.AuthToken, rule, templaterule))
            {
                this.notificationManager.WriteMessage("Failed to create rule: " + error);
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
            if (this.confHelper != null)
            {
                var data = string.Join(";", this.ExtensionDiagnostics.Select(m => m.Value.Path).ToArray());
                this.confHelper.WriteOptionInApplicationData(Context.AnalysisGeneral, ID, PATHKEY, data, true, false);
            }

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
        /// Initializeds the installed diagnostics.
        /// </summary>
        private void InitializedInstalledDiagnostics()
        {

            if (this.confHelper != null)
            {
                try
                {
                    var roslynDef = this.confHelper.ReadSetting(Context.AnalysisGeneral, ID, PATHKEY).Value;
                    var splitExistent = roslynDef.Split(';');

                    foreach (var item in splitExistent)
                    {
                        if (File.Exists(item))
                        {
                            if (!item.Contains(this.extensionRunningPath) && item.Contains(this.extensionsBasePath))
                            {
                                continue;
                            }

                            var name = Path.GetFileName(item);
                            this.ExtensionDiagnostics.Add(name, new VSSonarExtensionDiagnostic(name, item));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            if (!this.ExtensionDiagnostics.Any())
            {
                this.ExtensionDiagnostics.Add(
                    "SonarLint.CSharp.dll",
                    new VSSonarExtensionDiagnostic(
                        "SonarLint.CSharp.dll",
                        Path.Combine(this.extensionRunningPath, "externalAnalysers\\roslynDiagnostics", "SonarLint.CSharp.dll")));

                this.ExtensionDiagnostics.Add(
                    "SonarLint.VisualBasic.dll",
                    new VSSonarExtensionDiagnostic(
                        "SonarLint.VisualBasic.dll",
                        Path.Combine(this.extensionRunningPath, "externalAnalysers\\roslynDiagnostics", "SonarLint.VisualBasic.dll")));

                this.ExtensionDiagnostics.Add(
                    "StyleCop.Analyzers.dll",
                    new VSSonarExtensionDiagnostic(
                        "StyleCop.Analyzers.dll",
                        Path.Combine(this.extensionRunningPath, "externalAnalysers\\roslynDiagnostics", "StyleCop.Analyzers.dll")));
                
            }

            this.SyncSettings();
        }
    }
}
