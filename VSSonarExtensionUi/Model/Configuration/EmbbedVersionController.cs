﻿namespace VSSonarExtensionUi.Model.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using SonarRestService;
    using SonarRestService.Types;

    using VSSonarPlugins;

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
        /// prevents double download
        /// </summary>
        private bool isDownloaded;

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
			var installedPlugins = this.rest.GetInstalledPlugins(configuration);
            var versionData = this.GetPluginsVersionData(installedPlugins);
            if (string.IsNullOrEmpty(versionData))
            {
                return;
            }

			var version = new VersionData();
			try
			{
				var zipVersion = this.GetVersionFromName(versionData);		
				version.DownloadPath = "/static/csharp/SonarAnalyzer-" + zipVersion + ".zip";
				version.InstallPath = Path.Combine(this.roslynHomeDiagPath, "csharp", zipVersion);
				this.InternalVersionsCsharp.Add(version);
			}
			catch (Exception)
			{
			}

			version = new VersionData
			{
				DownloadPath = "/static/csharp/SonarLint.zip",
				InstallPath = Path.Combine(this.roslynHomeDiagPath, "csharp", "SonarLint")
			};
			this.InternalVersionsCsharp.Add(version);
			version = new VersionData
			{
				InstallPath = Path.Combine(this.roslynHomeDiagPath, "csharp", "SonarLint"),
				DownloadPath = "/static/csharp/SonarAnalyzer.zip"
			};
			this.InternalVersionsCsharp.Add(version);

            var settings = this.rest.GetSettings(configuration);
			version = new VersionData
			{
				InstallPath = Path.Combine(this.roslynHomeDiagPath, "csharp", "SonarLint")
			};

			var result = settings.FirstOrDefault(x => x.Key == "sonaranalyzer-cs.staticResourceName");

            if (result != null)
            {
                version.DownloadPath = "/static/csharp/" + result.Value;
                this.InternalVersionsCsharp.Add(version);
            }
        }

        private string GetPluginsVersionData(Dictionary<string, string> installedPlugins)
        {
            if (installedPlugins.ContainsKey("SonarC#"))
            {
                return installedPlugins["SonarC#"];
            }

            if (installedPlugins.ContainsKey("C# Code Quality and Security"))
            {
                return installedPlugins["C# Code Quality and Security"];
            }

            return string.Empty;
        }

        private string GetVersionFromName(string v)
		{
            // 7.7 (build 7192)
            // 8.13.1 (build 21947)
            var countP = v.Count(f => f == '.');
            if (countP == 2)
            {
                var elements = v.TrimEnd(')').Split(' ');
                return elements[0] + "." +  elements[2];
            }
            else
            {
                var elements = v.TrimEnd(')').Split(' ');
                return elements[0] + ".0." + elements[2];
            }
		}

		/// <summary>
		/// Initializeds the server diagnostics.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		internal async Task InitializedServerDiagnostics(ISonarConfiguration configuration)
        {
            try
            {
                this.SelectCSharpZipFileToUse(configuration);
                if (this.InUsePluginsWithDiagnostics.Count > 0)
                {
                    var elementsToRemove = new List<VersionData>();
                    foreach (var item in this.InUsePluginsWithDiagnostics)
                    {
                        if (!await this.SyncAnalysersFromServer(configuration, item))
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
        private async Task<bool> SyncAnalysersFromServer(ISonarConfiguration authentication, VersionData versionToUse)
        {
            if (this.isDownloaded)
            {
                return true;
            }

            this.isDownloaded = true;

            var tmpFile = Path.GetTempFileName();
            var tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var isOk = false;

            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }

            try
            {
                isOk = await DownloadFileFromServer(authentication, versionToUse, tmpFile, tmpDir);
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

        private static async Task<bool> DownloadFileFromServer(ISonarConfiguration authentication, VersionData versionToUse, string tmpFile, string tmpDir)
        {
            try
            {
                var urldownload = authentication.Hostname + versionToUse.DownloadPath;
                using (var client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(urldownload, tmpFile);

                    ZipFile.ExtractToDirectory(tmpFile, tmpDir);

                    var files = Directory.GetFiles(tmpDir);

                    if (!Directory.Exists(versionToUse.InstallPath))
                    {
                        Directory.CreateDirectory(versionToUse.InstallPath);
                    }

                    foreach (var file in files)
                    {
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
