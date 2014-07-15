// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginSandBoxLoader.cs" company="">
//   
// </copyright>
// <summary>
//   The assembly sand box loader.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarPlugins
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    using ExtensionTypes;

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
                this.trustedAssembly.Add(Assembly.LoadFrom(assemblyFilename));
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
            AppDomain.CurrentDomain.AssemblyResolve += handler;

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
        public PluginDescription LoadPluginDescription(ConnectionConfiguration conf)
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

                            var desc = new PluginDescription
                                           {
                                               Status = "Plugin to be loaded after VS restart",
                                               Name = plugin.GetKey(conf),
                                               Enabled = false,
                                               SupportedExtensions = plugin.GetLanguageKey(),
                                               Version = assembly.GetName().Version.ToString()
                                           };
                            return desc;
                        }

                        if (typeof(IMenuCommandPlugin).IsAssignableFrom(type))
                        {
                            Debug.WriteLine("Can Cast Type In Assembly To: " + typeof(IMenuCommandPlugin).FullName);
                            var plugin = (IMenuCommandPlugin)Activator.CreateInstance(type);
                            var desc = new PluginDescription
                                           {
                                               Description = "Plugin to be loaded after VS restart",
                                               Name = plugin.GetHeader(),
                                               Enabled = false,
                                               Version = assembly.GetName().Version.ToString()
                                           };
                            return desc;
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