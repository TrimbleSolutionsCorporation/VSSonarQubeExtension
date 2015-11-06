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
        /// The conf helper
        /// </summary>
        private readonly IConfigurationHelper confHelper;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// The source dir
        /// </summary>
        private string sourceDir;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynManagerModel" /> class.
        /// </summary>
        /// <param name="pluginsIn">The plugins in.</param>
        /// <param name="notificationManagerIn">The notification manager in.</param>
        /// <param name="configHelper">The configuration helper.</param>
        public RoslynManagerModel(
            IEnumerable<IAnalysisPlugin> pluginsIn,
            INotificationManager notificationManagerIn,
            IConfigurationHelper configHelper)
        {
            this.confHelper = configHelper;
            this.plugins = pluginsIn;
            this.notificationManager = notificationManagerIn;
            this.ExtensionDiagnostics = new Dictionary<string, VSSonarExtensionDiagnostic>();
            this.InitializedInstalledDiagnostics();

            // register model
            AssociationModel.RegisterNewModelInPool(this);
        }

        /// <summary>
        /// Gets the extension diagnostics.
        /// </summary>
        /// <value>
        /// The extension diagnostics.
        /// </value>
        public Dictionary<string, VSSonarExtensionDiagnostic> ExtensionDiagnostics { get; private set; }

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
        public void EndDataAssociation()
        {
            this.associatedProject = null;
            this.sourceDir = string.Empty;
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
        public void AssociateWithNewProject(Resource project, string workDir, ISourceControlProvider sourceModelIn, IIssueTrackerPlugin sourcePlugin)
        {
            this.sourceDir = workDir;
            this.associatedProject = project;
        }

        /// <summary>
        /// Adds the new roslyn pack.
        /// </summary>
        /// <param name="dllPath">The DLL path.</param>
        /// <returns>true if ok</returns>
        public bool AddNewRoslynPack(string dllPath)
        {
            var name = Path.GetFileName(dllPath);

            if (this.ExtensionDiagnostics.ContainsKey(Path.GetFileName(name)) || !File.Exists(dllPath))
            {
                return false;
            }

            try
            {                
                this.ExtensionDiagnostics.Add(name, new VSSonarExtensionDiagnostic(name, dllPath));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            return true;
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
        private List<DiagnosticAnalyzer> CombineDiagnostics()
        {
            var listData = new List<DiagnosticAnalyzer>();

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
                    "SonarLint.dll",
                    new VSSonarExtensionDiagnostic(
                        "SonarLint.dll",
                        Path.Combine(this.extensionRunningPath, "externalAnalysers\\roslynDiagnostics", "SonarLint.dll")));

                this.ExtensionDiagnostics.Add(
                    "SonarLint.Extra.dll",
                    new VSSonarExtensionDiagnostic(
                        "SonarLint.Extra.dll",
                        Path.Combine(this.extensionRunningPath, "externalAnalysers\\roslynDiagnostics", "SonarLint.Extra.dll")));
            }

            this.SyncSettings();
        }
    }
}
