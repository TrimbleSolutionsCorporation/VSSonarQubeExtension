namespace VSSonarExtensionUi.Model.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using VSSonarPlugins;
    using SonarRestService;
    using SonarRestService.Types;

    /// <summary>
    /// controls the available versions in server
    /// </summary>
    internal class EmbbedVersionController
    {
        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configHelper;

        /// <summary>
        /// The notification manager
        /// </summary>
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// The rest
        /// </summary>
        private readonly ISonarRestService rest;

        /// <summary>
        /// The internal versions csharp
        /// </summary>
        private readonly List<VersionData> InternalVersionsCsharp = new List<VersionData>();

        /// <summary>
        /// The in use plugins with diagnostics
        /// </summary>
        private readonly List<VersionData> InUsePluginsWithDiagnostics = new List<VersionData>();

        /// <summary>
        /// The roslyn home diag path
        /// </summary>
        private readonly string roslynHomeDiagPath;

        /// <summary>
        /// internal version data
        /// </summary>
        private class VersionData
        {
            public string DownloadPath { get; set; }

            public string InstallPath { get; set; } 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbbedVersionController"/> class.
        /// </summary>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="restService">The rest service.</param>
        /// <param name="configHelper">The configuration helper.</param>
        /// <param name="roslynDefaultPath">The roslyn default path.</param>
        public EmbbedVersionController(
            INotificationManager notificationManager,
            ISonarRestService restService,
            IConfigurationHelper configHelper,
            string roslynDefaultPath)
        {
            this.roslynHomeDiagPath = roslynDefaultPath;
            this.notificationManager = notificationManager;
            this.configHelper = configHelper;
            this.rest = restService;
        }

        /// <summary>
        /// Gets the installed paths.
        /// </summary>
        /// <returns></returns>
        public List<string> GetInstalledPaths()
        {
            var listOfPaths = new List<string>();
            foreach (var item in this.InUsePluginsWithDiagnostics)
            {
                listOfPaths.Add(item.InstallPath);
            }

            return listOfPaths;
        }

        /// <summary>
        /// Generates the version data.
        /// </summary>
        private void GenerateVersionData(ISonarConfiguration configuration)
        {
            var version = new VersionData();
            version.DownloadPath = "/static/csharp/SonarLint.zip";
            version.InstallPath = Path.Combine(this.roslynHomeDiagPath, "csharp", "lint");
            this.InternalVersionsCsharp.Add(version);
            version = new VersionData();
            version.InstallPath = Path.Combine(this.roslynHomeDiagPath, "csharp", "analyser");
            version.DownloadPath = "/static/csharp/SonarAnalyzer.zip";
            this.InternalVersionsCsharp.Add(version);

            var settings = this.rest.GetSettings(configuration);
            version = new VersionData();
            version.InstallPath = Path.Combine(this.roslynHomeDiagPath, "csharp", "analyser");

            var result = settings.FirstOrDefault(x => x.key == "sonaranalyzer-cs.staticResourceName");

            if (result != null)
            {
                version.DownloadPath = "/static/csharp/" + result.Value;
                this.InternalVersionsCsharp.Add(version);
            }
        }

        /// <summary>
        /// Initializeds the server diagnostics.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        internal void InitializedServerDiagnostics(ISonarConfiguration configuration)
        {

            try
            {
                this.SelectCSharpZipFileToUse(configuration);
                if (this.InUsePluginsWithDiagnostics.Count > 0)
                {
                    List<VersionData> elementsToRemove = new List<VersionData>();
                    foreach (var item in this.InUsePluginsWithDiagnostics)
                    {
                        if (!this.SyncAnalysersFromServer(configuration, item))
                        {
                            elementsToRemove.Add(item);
                        }
                    }

                    foreach (var item in elementsToRemove)
                    {
                        this.InUsePluginsWithDiagnostics.Remove(item);
                    }
                    
                }          
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Synchronizes the analysers from server.
        /// </summary>
        /// <param name="authentication">The authentication.</param>
        /// <param name="versionToUse">The version to use.</param>
        private bool SyncAnalysersFromServer(ISonarConfiguration authentication, VersionData versionToUse)
        {
            var tmpFile = Path.GetTempFileName();
            var tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            bool isOk = false;

            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }

            try
            {
                isOk = DownloadFileFromServer(authentication, versionToUse, tmpFile, tmpDir);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }

            if (Directory.Exists(tmpDir))
            {
                Directory.Delete(tmpDir, true);
            }

            return isOk;
        }

        private static bool DownloadFileFromServer(ISonarConfiguration authentication, VersionData versionToUse, string tmpFile, string tmpDir)
        {
            try
            {
                var urldownload = authentication.Hostname + versionToUse.DownloadPath;
                using (var client = new WebClient())
                {
                    client.DownloadFile(urldownload, tmpFile);

                    ZipFile.ExtractToDirectory(tmpFile, tmpDir);

                    var files = Directory.GetFiles(tmpDir);
                    var versionDir = "";

                    foreach (var file in files)
                    {
                        if (string.IsNullOrEmpty(versionDir))
                        {
                            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(file);
                            string ver = fvi.FileVersion;
                            versionDir = Path.Combine(versionToUse.InstallPath, ver);
                            versionToUse.InstallPath = Path.Combine(versionToUse.InstallPath, ver);

                            if (!Directory.Exists(versionToUse.InstallPath))
                            {
                                Directory.CreateDirectory(versionToUse.InstallPath);
                            }
                        }

                        var endPath = Path.Combine(versionToUse.InstallPath, Path.GetFileName(file));
                        if (!File.Exists(endPath))
                        {
                            File.Copy(file, endPath, true);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Selects the zip file to use.
        /// </summary>
        private void SelectCSharpZipFileToUse(ISonarConfiguration configuration)
        {
            this.GenerateVersionData(configuration);
            this.InUsePluginsWithDiagnostics.Clear();
            foreach (var version in this.InternalVersionsCsharp)
            {
                version.InstallPath = Path.Combine(this.roslynHomeDiagPath, "csharp");
                this.InUsePluginsWithDiagnostics.Add(version);
            }
        }
    }
}