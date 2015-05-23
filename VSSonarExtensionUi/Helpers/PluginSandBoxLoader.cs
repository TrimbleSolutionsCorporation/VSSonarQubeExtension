// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginSandBoxLoader.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarExtensionUi.Helpers
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using SonarRestService;

    /// <summary>
    ///     The assembly sand box loader.
    /// </summary>
    public class PluginSandBoxLoader : MarshalByRefObject
    {
        #region Fields

        /// <summary>
        ///     The trusted assembly.
        /// </summary>
        private readonly List<Assembly> trustedAssembly = new List<Assembly>();

        #endregion

        // Create a SandBox to load Assemblies with "Full Trust"
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginSandBoxLoader"/> class.
        /// </summary>
        /// <param name="assemblyFilenames">
        /// The assembly filenames.
        /// </param>
        public PluginSandBoxLoader(IEnumerable<string> assemblyFilenames)
        {
            foreach (var assemblyFilename in assemblyFilenames)
            {
                this.trustedAssembly.Add(Assembly.Load(File.ReadAllBytes(assemblyFilename)));
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The sandbox.
        /// </summary>
        /// <param name="domain">
        /// The domain.
        /// </param>
        /// <param name="assemblyFilenames">
        /// The assembly filenames.
        /// </param>
        /// <param name="handler">
        /// The handler.
        /// </param>
        /// <returns>
        /// The <see cref="PluginSandBoxLoader"/>.
        /// </returns>
        public static PluginSandBoxLoader Sandbox(
            AppDomain domain, 
            string[] assemblyFilenames, 
            ResolveEventHandler handler)
        {
            //AppDomain.CurrentDomain.AssemblyResolve += handler;

            var loader =
                domain.CreateInstanceAndUnwrap(
                    typeof(PluginSandBoxLoader).Assembly.GetName().FullName, 
                    typeof(PluginSandBoxLoader).FullName, 
                    false, 
                    BindingFlags.Default, 
                    null, 
                    new object[] { assemblyFilenames }, 
                    CultureInfo.InvariantCulture, 
                    null) as PluginSandBoxLoader;

            return loader;
        }

        /// <summary>
        /// The is plugin found.
        /// </summary>
        /// <param name="conf">
        /// The conf.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public PluginDescription LoadPluginDescription(ISonarConfiguration conf, IConfigurationHelper helper)
        {
            foreach (var assembly in this.trustedAssembly)
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
                            var plugin = (IAnalysisPlugin)Activator.CreateInstance(type);
                            return plugin.GetPluginDescription();
                        }

                        if (typeof(IMenuCommandPlugin).IsAssignableFrom(type))
                        {
                            Debug.WriteLine("Can Cast Type In Assembly To: " + typeof(IMenuCommandPlugin).FullName);
                            var plugin = (IMenuCommandPlugin)Activator.CreateInstance(type);
                            return plugin.GetPluginDescription();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(
                            "Cannot Cast Type In Assembly To: " + typeof(IAnalysisPlugin).FullName + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                        Debug.WriteLine(ex.InnerException.Message + " : " + ex.InnerException.StackTrace);
                        throw;
                    }
                }
            }

            return null;
        }

        public IPlugin LoadPlugin(ISonarConfiguration conf, IConfigurationHelper helper, INotificationManager manager, IVsEnvironmentHelper vshelper)
        {
            foreach (var assembly in this.trustedAssembly)
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
                            var obj = type.GetConstructor(new[] { typeof(INotificationManager), typeof(IConfigurationHelper), typeof(ISonarRestService), typeof(IVsEnvironmentHelper), typeof(IVSSonarQubeCmdExecutor) });
                            if (obj != null)
                            {
                                object[] lobject = { manager, helper, new SonarRestService(new JsonSonarConnector()), vshelper, new VSSonarQubeCmdExecutor.VSSonarQubeCmdExecutor(60000) };
                                return (IPlugin)obj.Invoke(lobject);
                            }

                            return (IPlugin)Activator.CreateInstance(type);
                        }

                        if (typeof(IMenuCommandPlugin).IsAssignableFrom(type))
                        {
                            Debug.WriteLine("Can Cast Type In Assembly To: " + typeof(IMenuCommandPlugin).FullName);

                            var obj = type.GetConstructor(new[] { typeof(ISonarRestService) });
                            if (obj != null)
                            {
                                object[] lobject = { new SonarRestService(new JsonSonarConnector()) };
                                return (IPlugin)obj.Invoke(lobject);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(
                            "Cannot Cast Type In Assembly To: " + typeof(IAnalysisPlugin).FullName + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                        Debug.WriteLine(ex.InnerException.Message + " : " + ex.InnerException.StackTrace);
                        throw;
                    }
                }
            }

            return null;
        }

        #endregion
    }
}