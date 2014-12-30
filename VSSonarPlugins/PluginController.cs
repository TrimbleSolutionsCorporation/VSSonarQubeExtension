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
            this.PluginsFolder = Path.Combine(this.ExtensionFolder, "plugins");

            var pluginLoader = new PluginLoader();
            AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomainAssemblyResolve;
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

        public string PluginsFolder { get; set; }
        
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

        public List<IPlugin> LoadPluginsFromPluginFolder()
        {
            var folder = this.PluginsFolder;
            var pluginsData = new List<IPlugin>();

                var assemblies = new Dictionary<string, Assembly>();
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                var files = Directory.GetFiles(folder);
                foreach (var file in files)
                {
                    assemblies.Add(file, AppDomain.CurrentDomain.Load(File.ReadAllBytes(file)));
                }

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var plugin = this.LoadPlugin(assembly.Value);
                        if (plugin != null)
                        {
                            pluginsData.Add(plugin);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        File.Delete(assembly.Key);
                    }
                }



            return pluginsData;
        }

        public IPlugin IstallNewPlugin(string fileName, ISonarConfiguration conf)
        {
            var assembliesInFile = this.UnzipFiles(fileName, this.TempInstallPathFolder);
            var assembliesToTempFolder = this.GetAssembliesInTempFolder();
            var plugins = this.LoadPlugin(
                assembliesInFile.ToArray(),
                this.TempInstallPathFolder,
                conf);

            if (plugins != null)
            {
                Directory.Delete(this.TempInstallPathFolder, true);
                this.UnzipFiles(fileName, this.PluginsFolder);
            }

            return plugins;
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
            ISonarConfiguration conf)
        {
            try
            {
                var loader = PluginSandBoxLoader.Sandbox(AppDomain.CurrentDomain, assemblies, this.CurrentDomainAssemblyResolve);
                try
                {
                    var pluginDesc = loader.LoadPlugin(conf);

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


        public IPlugin LoadPlugin(Assembly assembly)
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
                        return (IPlugin)Activator.CreateInstance(type);
                    }

                    if (typeof(IMenuCommandPlugin).IsAssignableFrom(type))
                    {
                        Debug.WriteLine("Can Cast Type In Assembly To: " + typeof(IMenuCommandPlugin).FullName);
                        return (IPlugin)Activator.CreateInstance(type);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        "Cannot Cast Type In Assembly To: " + typeof(IAnalysisPlugin).FullName + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
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
        public bool RemovePlugin(IPlugin selectedPlugin)
        {
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