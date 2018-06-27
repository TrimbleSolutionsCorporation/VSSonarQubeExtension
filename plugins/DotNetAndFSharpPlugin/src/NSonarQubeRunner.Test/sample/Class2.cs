namespace VSSonarExtensionUi.Model.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;

    /// <summary>
    /// controls the available versions in server
    /// </summary>
    internal class EmbbedVersionController
    {
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
            public string LowVersion { get; set; }

            public string HighVersion { get; set; }

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
        public EmbbedVersionController(string roslynDefaultPath)
        {
            this.roslynHomeDiagPath = roslynDefaultPath;





            this.GenerateVersionData();
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
        private void GenerateVersionData()
        {
            var version = new VersionData();
            version.HighVersion = "4.9";
            version.LowVersion = "4.0";
            version.DownloadPath = "/static/csharp/SonarLint.zip";
            this.InternalVersionsCsharp.Add(version);
            version = new VersionData();
            version.HighVersion = "";
            version.LowVersion = "5.0";
            version.DownloadPath = "/static/csharp/SonarAnalyzer.zip";
            this.InternalVersionsCsharp.Add(version);
        }

        /// <summary>
        /// Initializeds the server diagnostics.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        internal void InitializedServerDiagnostics()
        {
            try
            {
                this.InUsePluginsWithDiagnostics.Clear();
                this.SelectCSharpZipFileToUse();
                if (this.InUsePluginsWithDiagnostics.Count > 0)
                {
                    foreach (var item in this.InUsePluginsWithDiagnostics)
                    {
                        this.SyncAnalysersFromServer(item);
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
        private void SyncAnalysersFromServer(VersionData versionToUse)
        {
            var tmpFile = Path.GetTempFileName();
            var tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }

            if (!Directory.Exists(versionToUse.InstallPath))
            {
                Directory.CreateDirectory(versionToUse.InstallPath);
            }

            try
            {
                var urldownload = versionToUse.DownloadPath;
                using (var client = new WebClient())
                {
                    client.DownloadFile(urldownload, tmpFile);


                    var files = Directory.GetFiles(tmpDir);

                    foreach (var file in files)
                    {
                        var endPath = Path.Combine(versionToUse.InstallPath, Path.GetFileName(file));
                        File.Copy(file, endPath, true);
                    }
                }
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
        }

        /// <summary>
        /// Selects the zip file to use.
        /// </summary>
        private void SelectCSharpZipFileToUse()
        {
            this.InUsePluginsWithDiagnostics.Clear();
            var pluginVersion = "2.0";
            var versionInServer = new Version(pluginVersion.Split('-')[0]);
            foreach (var version in this.InternalVersionsCsharp)
            {
                var versionLower = new Version(version.LowVersion);
                var result = versionInServer.CompareTo(versionLower);
                if (result >= 0)
                {
                    if (string.IsNullOrEmpty(version.HighVersion))
                    {
                        version.InstallPath = Path.Combine(this.roslynHomeDiagPath, "csharp", pluginVersion);
                        this.InUsePluginsWithDiagnostics.Add(version);
                        break;
                    }
                    else
                    {
                        var versionHigher = new Version(version.HighVersion);
                        var result2 = versionInServer.CompareTo(versionHigher);
                        if (result2 <= 0)
                        {
                            version.InstallPath = Path.Combine(this.roslynHomeDiagPath, "csharp", pluginVersion);
                            this.InUsePluginsWithDiagnostics.Add(version);
                            break;
                        }
                    }
                }
            }
        }
    }
}