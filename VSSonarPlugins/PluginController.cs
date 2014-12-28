// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginController.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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

            var plugins = pluginLoader.LoadPluginsFromFolder(this.ExtensionFolder);

            if (plugins == null)
            {
                return;
            }

            foreach (IPlugin plugin in plugins)
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
        ///     Gets or sets the install path file.
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
        public IMenuCommandPlugin GetMenuCommandPluginToRunCommand(ISonarConfiguration configuration, string key)
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
        public PluginDescription IstallNewPlugin(string fileName, ISonarConfiguration conf)
        {
            var assembliesInFile = this.UnzipFiles(fileName);
            var assembliesToTempFolder = this.GetAssembliesInTempFolder();
            return this.LoadPluginUsingDedicatedDomain(
                assembliesInFile.ToArray(),
                this.GetReferenceAssemblies(assembliesToTempFolder), 
                this.TempInstallPathFolder, 
                conf);
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
            ISonarConfiguration conf)
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
        public bool RemovePlugin(ISonarConfiguration configuration, IConfigurationHelper configurationhelper, PluginDescription selectedPlugin)
        {
            foreach (var plugin in this.loadedPlugins)
            {
                if (plugin.GetKey(configuration).Equals(selectedPlugin.Name))
                {
                    return this.SyncFileWithRemovePlugin(plugin);
                }
            }

            foreach (var plugin in this.menuCommandPlugins)
            {
                if (plugin.GetPluginDescription(configurationhelper).Name.Equals(selectedPlugin.Name))
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
        ///     The get assemblies in temp folder.
        /// </summary>
        /// <returns>
        ///     The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        private List<string> GetAssembliesInTempFolder()
        {
            List<string> files = Directory.GetFiles(this.TempInstallPathFolder, "*.*").ToList();

            return files.ToList();
        }

        /// <summary>
        /// The get reference assemblies.
        /// </summary>
        /// <param name="listOfAssemblies">
        /// The list of assemblies.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        private List<string> GetReferenceAssemblies(List<string> listOfAssemblies)
        {
            List<string> assembliesInExtensionFolder = Directory.GetFiles(this.ExtensionFolder, "*.dll").ToList();
            var refAssemblies = new List<string>();
            foreach (string assembly in assembliesInExtensionFolder)
            {
                bool isPresent = false;
                foreach (string assemblyToImport in listOfAssemblies)
                {
                    string name = Path.GetFileName(assembly);

                    if (name != null && name.Equals(Path.GetFileName(assemblyToImport), StringComparison.CurrentCultureIgnoreCase))
                    {
                        isPresent = true;
                    }
                }

                if (!isPresent)
                {
                    refAssemblies.Add(assembly);
                }
            }

            return refAssemblies;
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
                w.WriteLine(location);
            }

            return true;
        }

        /// <summary>
        /// The unzip files.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        private List<string> UnzipFiles(string fileName)
        {
            var files = new List<string>();
            if (!Directory.Exists(this.TempInstallPathFolder))
            {
                Directory.CreateDirectory(this.TempInstallPathFolder);
            }

            using (var archive = ZipFile.OpenRead(fileName))
            {
                foreach (var entry in archive.Entries)
                {
                    var endFile = Path.Combine(this.TempInstallPathFolder, entry.FullName);
                    try
                    {
                        entry.ExtractToFile(endFile, true);
                        files.Add(endFile);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Cannot Extrac File: " + ex.Message + " : " + ex.StackTrace);
                    }
                }
            }

            return files;
        }

        /// <summary>
        ///     The upgrade plugins.
        /// </summary>
        private void UpgradePlugins()
        {
            if (File.Exists(this.InstallPathFile))
            {
                IEnumerable<string> content = File.ReadLines(this.InstallPathFile);
                foreach (string line in content)
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
                string[] files = Directory.GetFiles(this.TempInstallPathFolder, "*.*");

                foreach (string file in files)
                {
                    try
                    {
                        if (file == null)
                        {
                            continue;
                        }

                        string dest = Path.Combine(this.ExtensionFolder, Path.GetFileName(file));
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