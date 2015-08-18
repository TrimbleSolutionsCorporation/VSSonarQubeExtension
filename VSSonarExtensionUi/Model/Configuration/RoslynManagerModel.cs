namespace VSSonarExtensionUi.Model.Configuration
{
    using Microsoft.CodeAnalysis.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    public class RoslynManagerModel : IOptionsModelBase
    {
        const string ID = "RoslynManagerModel";
        const string PATHKEY = "RoslynDllPaths";
        
        private readonly string pluginsRunningPath = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "");
        private readonly string extensionRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")).ToString();
        private IConfigurationHelper confHelper;
        private readonly IEnumerable<IAnalysisPlugin> plugins;
        private readonly INotificationManager notificationManager;

        public Dictionary<string, VSSonarExtensionDiagnostic> ExtensionDiagnostics { get; private set; }

        public RoslynManagerModel(IEnumerable<IAnalysisPlugin> pluginsIn, INotificationManager notificationManagerIn, IConfigurationHelper vsHelperIn)
        {
            this.confHelper = vsHelperIn;
            this.plugins = pluginsIn;
            this.notificationManager = notificationManagerIn;
            this.ExtensionDiagnostics = new Dictionary<string, VSSonarExtensionDiagnostic>();
            this.InitializedInstalledDiagnostics();
        }

        public bool AddNewRoslynPack(string dllPath)
        {
            var name = Path.GetFileName(dllPath);

            if (this.ExtensionDiagnostics.ContainsKey(Path.GetFileName(name)) || !File.Exists(dllPath))
            {
                return false;
            }

            try
            {
                
                ExtensionDiagnostics.Add(name, new VSSonarExtensionDiagnostic(name, dllPath));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

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

        private void SyncSettings()
        {
            if (this.confHelper != null)
            {
                var data = string.Join(";", ExtensionDiagnostics.Select(m => m.Value.Path).ToArray());
                confHelper.WriteOptionInApplicationData(Context.AnalysisGeneral, ID, PATHKEY, data, true, false);
            }

            this.SyncDiagnosticsInPlugins();
        }

        private List<DiagnosticAnalyzer> CombineDiagnostics()
        {
            var listData = new List<DiagnosticAnalyzer>();

            foreach (var item in this.ExtensionDiagnostics)
            {
                listData.AddRange(item.Value.AvailableChecks);
            }

            return listData;
        }

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
                            ExtensionDiagnostics.Add(name, new VSSonarExtensionDiagnostic(name, item));
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
                ExtensionDiagnostics.Add("SonarLint.dll", new VSSonarExtensionDiagnostic("SonarLint.dll",
                    Path.Combine(extensionRunningPath, "externalAnalysers\\roslynDiagnostics", "SonarLint.dll")));
                ExtensionDiagnostics.Add("SonarLint.Extra.dll", new VSSonarExtensionDiagnostic("SonarLint.dll",
                    Path.Combine(extensionRunningPath, "externalAnalysers\\roslynDiagnostics", "SonarLint.Extra.dll")));
            }

            this.SyncSettings();
        }

        public void RefreshPropertiesInView(Resource associatedProject)
        {
        }

        public void UpdateServices(ISonarRestService restServiceIn, IVsEnvironmentHelper vsenvironmenthelperIn, IConfigurationHelper configurationHelper, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            this.confHelper = configurationHelper;
        }

        public void SaveCurrentViewToDisk(IConfigurationHelper configurationHelper)
        {
            this.SyncSettings();
        }
    }
}
