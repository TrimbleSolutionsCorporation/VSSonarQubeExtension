// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginController.cs" company="">
//   
// </copyright>
// <summary>
//   The local analyser.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarPlugins
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    using ExtensionTypes;

    /// <summary>
    ///     The local analyser.
    /// </summary>
    public class PluginController : IPluginController
    {
        #region Static Fields

        /// <summary>
        ///     The installfile.
        /// </summary>
        public static readonly string Installfile = "InstallFile";

        /// <summary>
        ///     The tempinstallfolder.
        /// </summary>
        public static readonly string Tempinstallfolder = "TempInstallFolder";

        #endregion

        #region Fields

        /// <summary>
        ///     The loaded plugins.
        /// </summary>
        private readonly List<IAnalysisPlugin> loadedPlugins = new List<IAnalysisPlugin>();

        /// <summary>
        ///     The loaded plugins.
        /// </summary>
        private readonly List<IMenuCommandPlugin> menuCommandPlugins = new List<IMenuCommandPlugin>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginController" /> class.
        /// </summary>
        public PluginController()
        {
            this.ExtensionFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            if (this.ExtensionFolder == null)
            {
                return;
            }

            this.ExtensionFolder = this.ExtensionFolder.Replace("file:\\", string.Empty);
            this.TempInstallPathFolder = Path.Combine(this.ExtensionFolder, Tempinstallfolder);
            this.InstallPathFile = Path.Combine(this.ExtensionFolder, Installfile);

            this.UpgradePlugins();

            var pluginLoader = new PluginLoader();

            foreach (var plugin in pluginLoader.LoadPluginsFromFolder(this.ExtensionFolder))
            {
                try
                {
                    this.loadedPlugins.Add((IAnalysisPlugin)plugin);
                }
                catch (Exception)
                {
                    try
                    {
                        this.menuCommandPlugins.Add((IMenuCommandPlugin)plugin);
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("Cannot Import Plugin");
                    }
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     Gets or sets the extension folder.
        /// </summary>
        public string ExtensionFolder { get; set; }

        /// <summary>
        /// Gets or sets the install path file.
        /// </summary>
        public string InstallPathFile { get; set; }

        /// <summary>
        ///     Gets or sets the temp install path folder.
        /// </summary>
        public string TempInstallPathFolder { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The current domain_ assembly resolve.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="Assembly"/>.
        /// </returns>
        public Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return typeof(PluginSandBoxLoader).Assembly;
        }

        /// <summary>
        ///     The get error data.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string GetErrorData()
        {
            return this.ErrorMessage;
        }

        /// <summary>
        /// The get menu command plugin to run command.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="IMenuCommandPlugin"/>.
        /// </returns>
        public IMenuCommandPlugin GetMenuCommandPluginToRunCommand(ConnectionConfiguration configuration, string key)
        {
            if (this.menuCommandPlugins == null)
            {
                return null;
            }

            return (from plugin in this.menuCommandPlugins where plugin.GetHeader().Equals(key) select plugin).FirstOrDefault();
        }

        /// <summary>
        ///     The get plugins options.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public List<IMenuCommandPlugin> GetMenuItemPlugins()
        {
            return this.menuCommandPlugins;
        }

        /// <summary>
        ///     The get plugins options.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public List<IAnalysisPlugin> GetPlugins()
        {
            return this.loadedPlugins;
        }

        /// <summary>
        /// The istall new plugin.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="conf">
        /// The conf.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public PluginDescription IstallNewPlugin(string fileName, ConnectionConfiguration conf)
        {
            PluginDescription plugindesc;
            if (!Directory.Exists(this.TempInstallPathFolder))
            {
                Directory.CreateDirectory(this.TempInstallPathFolder);
            }

            using (ZipArchive archive = ZipFile.OpenRead(fileName))
            {
                var listOfAssemblies = new List<string>();
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string endFile = Path.Combine(this.TempInstallPathFolder, entry.FullName);
                    try
                    {
                        entry.ExtractToFile(endFile, true);
                        listOfAssemblies.Add(endFile);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Cannot Extrac File: " + ex.Message + " : " + ex.StackTrace);
                    }
                }

                List<string> refAssemblies = Directory.GetFiles(this.ExtensionFolder, "*.dll").ToList();
                plugindesc = this.LoadPluginUsingDedicatedDomain(listOfAssemblies.ToArray(), refAssemblies, this.TempInstallPathFolder, conf);
            }

            return plugindesc;
        }

        /// <summary>
        /// The load plugin using dedicated domain.
        /// </summary>
        /// <param name="assemblies">
        /// The assemblies.
        /// </param>
        /// <param name="refAssemblies">
        /// The ref assemblies.
        /// </param>
        /// <param name="basePath">
        /// The base path.
        /// </param>
        /// <param name="conf">
        /// The conf.
        /// </param>
        /// <returns>
        /// The <see cref="PluginDescription"/>.
        /// </returns>
        public PluginDescription LoadPluginUsingDedicatedDomain(
            string[] assemblies, 
            List<string> refAssemblies, 
            string basePath, 
            ConnectionConfiguration conf)
        {
            PluginDescription pluginDesc = null;
            IEnumerable<string> refAssembliesInDropFolder = this.DropRefAssembliesIntoBasePath(refAssemblies);

            try
            {
                var trustedLoadGrantSet = new PermissionSet(PermissionState.Unrestricted);
                var trustedLoadSetup = new AppDomainSetup
                                           {
                                               ApplicationBase = this.TempInstallPathFolder, 
                                               LoaderOptimization = LoaderOptimization.MultiDomainHost
                                           };

                AppDomain domain = AppDomain.CreateDomain("PluginCheckerDomain", null, trustedLoadSetup, trustedLoadGrantSet);

                PluginSandBoxLoader loader = PluginSandBoxLoader.Sandbox(domain, assemblies, this.CurrentDomainAssemblyResolve);
                try
                {
                    pluginDesc = loader.LoadPluginDescription(conf);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        this.ErrorMessage = "Cannot Load Plugin: " + ex.InnerException.Message + " : " + ex.InnerException.StackTrace;
                    }
                    else
                    {
                        this.ErrorMessage = "Cannot Load Plugin: " + ex.Message + " : " + ex.StackTrace;
                    }

                    Debug.WriteLine(ex.Message + " " + ex.StackTrace);
                }

                AppDomain.Unload(domain);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "Cannot Create Domain: " + ex.Message + " : " + ex.StackTrace;
            }

            if (pluginDesc == null)
            {
                foreach (string assembly in assemblies)
                {
                    File.Delete(assembly);
                }
            }

            foreach (string assembly in refAssembliesInDropFolder)
            {
                File.Delete(assembly);
            }

            return pluginDesc;
        }

        /// <summary>
        /// The pick plugin from multiple supported plugins.
        /// </summary>
        /// <param name="pluginsToUse">
        /// The plugins to use.
        /// </param>
        /// <returns>
        /// The <see cref="IAnalysisPlugin"/>.
        /// </returns>
        public IAnalysisPlugin PickPluginFromMultipleSupportedPlugins(ReadOnlyCollection<IAnalysisPlugin> pluginsToUse)
        {
            if (pluginsToUse != null && pluginsToUse.Any())
            {
                return pluginsToUse.First();
            }

            return null;
        }

        /// <summary>
        /// The remove plugin.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="selectedPlugin">
        /// The selected plugin.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool RemovePlugin(ConnectionConfiguration configuration, PluginDescription selectedPlugin)
        {
            foreach (IAnalysisPlugin plugin in this.loadedPlugins)
            {
                if (plugin.GetKey(configuration).Equals(selectedPlugin.Name))
                {
                    return this.SyncFileWithRemovePlugin(plugin);
                }
            }

            foreach (IMenuCommandPlugin plugin in this.menuCommandPlugins)
            {
                if (plugin.GetHeader().Equals(selectedPlugin.Name))
                {
                    return this.SyncFileWithRemovePlugin(plugin);
                }
            }

            return false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The drop ref assemblies into base path.
        /// </summary>
        /// <param name="refAssemblies">
        /// The ref assemblies.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        private IEnumerable<string> DropRefAssembliesIntoBasePath(IEnumerable<string> refAssemblies)
        {
            var newRefAssemblies = new List<string>();
            foreach (string assembly in refAssemblies)
            {
                if (assembly == null)
                {
                    continue;
                }

                string dropFile = Path.Combine(this.TempInstallPathFolder, Path.GetFileName(assembly));
                if (!File.Exists(dropFile))
                {
                    File.Copy(assembly, dropFile);
                }

                newRefAssemblies.Add(dropFile);
            }

            return newRefAssemblies;
        }

        /// <summary>
        /// The sync file with remove plugin.
        /// </summary>
        /// <param name="plugin">
        /// The plugin.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool SyncFileWithRemovePlugin(IPlugin plugin)
        {
            string location = plugin.GetAssemblyPath();

            if (!File.Exists(location))
            {
                return false;
            }

            using (StreamWriter w = File.AppendText(this.InstallPathFile))
            {
                w.Write(location);
            }

            return true;
        }

        /// <summary>
        /// The upgrade plugins.
        /// </summary>
        private void UpgradePlugins()
        {
            if (File.Exists(this.InstallPathFile))
            {
                var content = File.ReadLines(this.InstallPathFile);
                foreach (var line in content)
                {
                    if (File.Exists(line))
                    {
                        File.Delete(line);
                    }
                }

                File.Delete(this.InstallPathFile);
            }

            if (Directory.Exists(this.TempInstallPathFolder))
            {
                var files = Directory.GetFiles(this.TempInstallPathFolder, "*.*");

                foreach (var file in files)
                {
                    try
                    {
                        if (file == null)
                        {
                            continue;
                        }

                        var dest = Path.Combine(this.ExtensionFolder, Path.GetFileName(file));
                        File.Copy(file, dest, true);
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Cannot Move File: " + file + " : " + ex.Message);
                    }
                }
            }
        }

        #endregion
    }
}