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
namespace VSSonarExtensionUi.Model.PluginManager
{
    using Helpers;
    using SonarRestService;
    using SonarRestService.Types;
    using SonarRestServiceImpl;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    ///     The local analyser.
    /// </summary>    
    public class PluginController : IPluginController
    {
        #region Static Fields

        /// <summary>
        ///     The tempinstallfolder.
        /// </summary>
        public static readonly string Tempinstallfolder = "TempInstallFolder";

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

            var pluginLoader = new PluginLoader();
            AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomainAssemblyResolve;

            this.LoadAllAssembliesFromExtensionFolder();
        }

        /// <summary>The load all assemblies from extension folder.</summary>
        private void LoadAllAssembliesFromExtensionFolder()
        {
#if ENABLEDLOAD 
            var assemblyRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();

            foreach (var dll in Directory.GetFiles(assemblyRunningPath, "*.dll"))
            {
                try
                {
                    Assembly.LoadFrom(dll);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
#endif
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
            var assemblyToGet = args.Name.Replace(".resources,", ",");
            try
            {
                foreach (Assembly assemb in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    var fullName = assemb.FullName;

                    if (fullName.Equals(assemblyToGet))
                    {
                        return assemb;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            try
            {
                var files = Directory.GetFiles(this.ExtensionFolder);
                foreach (var file in files)
                {
                    if (file.ToLower().EndsWith(".dll") || file.ToLower().EndsWith(".exe"))
                    {
                        var fullName = AssemblyName.GetAssemblyName(file).FullName;
                        if (fullName.Equals(assemblyToGet))
                        {
                            return Assembly.LoadFrom(file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
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

        public List<IPlugin> LoadPluginsFromPluginFolder(INotificationManager manager, IConfigurationHelper helper, IVsEnvironmentHelper vshelper, IEnumerable<string> files, string pluginsFolder)
        {
            var pluginsData = new List<IPlugin>();

            var assemblies = new Dictionary<string, Assembly>();
            if (!Directory.Exists(pluginsFolder))
            {
                Directory.CreateDirectory(pluginsFolder);
            }

            var filesToUse = Directory.GetFiles(pluginsFolder);

            if (files != null && files.Any())
            {
                filesToUse = files.ToArray();
            }

            foreach (var file in filesToUse)
            {
                if (file.EndsWith(".dll"))
                {
                    try
                    {
                        assemblies.Add(file, AppDomain.CurrentDomain.Load(File.ReadAllBytes(file)));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("CannotLoad: " + file + " : " + ex.Message);
                    }
                }
            }

            foreach (var assembly in assemblies)
            {
                try
                {
                    if (assembly.Value.Location.EndsWith("FSharp.Core.dll"))
                    {
                        continue;
                    }

                    var plugin = this.LoadPlugin(assembly.Value, manager, helper, vshelper, AuthtenticationHelper.AuthToken);

                    if (plugin != null)
                    {
                        var references = assembly.Value.GetReferencedAssemblies();

                        foreach (var assemblyref in references)
                        {
                            var fileName = assemblyref.Name + ".dll";
                            var file = Path.Combine(pluginsFolder, fileName);

                            if (File.Exists(file))
                            {
                                plugin.SetDllLocation(file);
                            }
                        }

                        plugin.SetDllLocation(assembly.Key);
                        pluginsData.Add(plugin);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is System.Reflection.ReflectionTypeLoadException)
                    {
                        var typeLoadException = ex as ReflectionTypeLoadException;
                        var loaderExceptions = typeLoadException.LoaderExceptions;
                        foreach (var loadingException in loaderExceptions)
                        {
                            Debug.WriteLine(loadingException.Message);
                            Debug.WriteLine(loadingException.InnerException);
                        }

                    }


                    File.Delete(assembly.Key);
                }
            }

            return pluginsData;
        }

        public List<string> DeployPlugin(string fileName, string pluginsFolder)
        {
            return this.UnzipFiles(fileName, pluginsFolder);
        }


        public IPlugin IstallNewPlugin(string fileName,
            ISonarConfiguration conf,
            IConfigurationHelper helper,
            INotificationManager manager,
            IVsEnvironmentHelper vshelper,
            string pluginsFolder)
        {
            var assembliesInFile = this.UnzipFiles(fileName, this.TempInstallPathFolder);
            var assembliesToTempFolder = this.GetAssembliesInTempFolder();
            var plugin = this.LoadPlugin(
                assembliesInFile.ToArray(),
                this.TempInstallPathFolder,
                conf, helper, manager, vshelper);

            if (plugin != null)
            {
                Directory.Delete(this.TempInstallPathFolder, true);
                this.UnzipFiles(fileName, pluginsFolder);
            }

            foreach (string path in assembliesInFile)
            {
                var file = Path.GetFileName(path);
                plugin.SetDllLocation(Path.Combine(pluginsFolder, file));
            }

            return plugin;
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
        public IPlugin LoadPlugin(
            string[] assemblies,
            string basePath,
            ISonarConfiguration conf,
            IConfigurationHelper helper,
            INotificationManager manager,
            IVsEnvironmentHelper vshelper)
        {
            try
            {
                var loader = PluginSandBoxLoader.Sandbox(AppDomain.CurrentDomain, assemblies, this.CurrentDomainAssemblyResolve);
                try
                {
                    var pluginDesc = loader.LoadPlugin(conf, helper, manager, vshelper);

                    foreach (string assembly in assemblies)
                    {
                        File.Delete(assembly);
                    }

                    return pluginDesc;
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
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "Cannot Create Domain: " + ex.Message + " : " + ex.StackTrace;
            }

            return null;
        }


        /// <summary>
        /// Loads the plugin.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="helper">The helper.</param>
        /// <param name="vshelper">The vshelper.</param>
        /// <param name="userConfig">The user configuration.</param>
        /// <returns></returns>
        public IPlugin LoadPlugin(Assembly assembly, INotificationManager manager, IConfigurationHelper helper, IVsEnvironmentHelper vshelper, ISonarConfiguration userConfig)
        {
            var types2 = assembly.GetTypes();
            foreach (var type in types2)
            {
                Debug.WriteLine("Type In Assembly:" + type.FullName);

                try
                {
                    if (typeof(IAnalysisPlugin).IsAssignableFrom(type))
                    {
                        Debug.WriteLine("Can Cast Type In Assembly To: " + typeof(IAnalysisPlugin).FullName);
                        var obj = type.GetConstructor(new[] { typeof(INotificationManager), typeof(IConfigurationHelper), typeof(ISonarRestService), typeof(IVsEnvironmentHelper) });
                        if (obj != null)
                        {
                            object[] lobject = new object[] { manager, helper, new SonarService(new JsonSonarConnector()), vshelper };
                            return (IPlugin)obj.Invoke(lobject);
                        }
                        else
                        {
                            return (IPlugin)Activator.CreateInstance(type);
                        }
                    }

                    if (typeof(IMenuCommandPlugin).IsAssignableFrom(type))
                    {
                        Debug.WriteLine("Can Cast Type In Assembly To: " + typeof(IMenuCommandPlugin).FullName);
                        var obj = type.GetConstructor(new[] { typeof(ISonarRestService) });
                        if (obj != null)
                        {
                            object[] lobject = { new SonarService(new JsonSonarConnector()) };
                            return (IPlugin)obj.Invoke(lobject);
                        }
                    }

                    if (typeof(ISourceVersionPlugin).IsAssignableFrom(type))
                    {
                        Debug.WriteLine("Can Cast Type In Assembly To: " + typeof(ISourceVersionPlugin).FullName);
                        var obj = type.GetConstructor(new[] { typeof(INotificationManager) });
                        if (obj != null)
                        {
                            object[] lobject = { manager };
                            return (IPlugin)obj.Invoke(lobject);
                        }
                    }

                    if (typeof(IIssueTrackerPlugin).IsAssignableFrom(type))
                    {
                        Debug.WriteLine("Can Cast Type In Assembly To: " + typeof(IIssueTrackerPlugin).FullName);
                        var obj = type.GetConstructor(new[] { typeof(INotificationManager) });
                        if (obj != null)
                        {
                            object[] lobject = { manager };
                            return (IPlugin)obj.Invoke(lobject);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        "Cannot Cast Type: " + ex.Message + "\r\n" + ex.StackTrace);
                    Debug.WriteLine(ex.InnerException.Message + " : " + ex.InnerException.StackTrace);
                }
            }

            return null;
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
        public bool RemovePlugin(IPlugin selectedPlugin, IList<IPlugin> installedPlugins)
        {
            bool removedOk = true;
            foreach (var path in selectedPlugin.DllLocations())
            {
                if (IsReferencedByOtherPlugins(selectedPlugin, installedPlugins, path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception)
                    {
                        removedOk = false;
                    }
                }
            }

            return removedOk;
        }

        private static bool IsReferencedByOtherPlugins(IPlugin selectedPlugin, IList<IPlugin> installedPlugins, string path)
        {
            foreach (var plugin in installedPlugins)
            {
                if (plugin.GetPluginDescription().Name.Equals(selectedPlugin.GetPluginDescription().Name))
                {
                    continue;
                }

                foreach (var pathOfInstalled in plugin.DllLocations())
                {
                    if (pathOfInstalled.Equals(path))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Methods

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
        private List<string> UnzipFiles(string fileName, string folderToInstall)
        {
            var files = new List<string>();
            if (!Directory.Exists(folderToInstall))
            {
                Directory.CreateDirectory(folderToInstall);
            }

            using (var archive = ZipFile.OpenRead(fileName))
            {
                foreach (var entry in archive.Entries)
                {
                    var endFile = Path.Combine(folderToInstall, entry.FullName);
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

        #endregion
    }
}