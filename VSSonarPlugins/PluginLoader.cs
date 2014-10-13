// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginLoader.cs" company="">
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
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    /// <summary>
    ///     The local analyser.
    /// </summary>
    public class PluginLoader : IPluginLoader
    {
        #region Fields

        /// <summary>
        ///     The error data.
        /// </summary>
        private string errorData;

        /// <summary>
        ///     The plugins.
        /// </summary>
        [ImportMany]
        private IEnumerable<Lazy<IPlugin>> plugins;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The get error data.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string GetErrorData()
        {
            return this.errorData;
        }

        /// <summary>
        /// The load plugins from folder.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>ReadOnlyCollection</cref>
        ///     </see>
        ///     .
        /// </returns>
        public ReadOnlyCollection<IPlugin> LoadPluginsFromFolder(string folder)
        {
            var sensors = new AggregateCatalog();
            sensors.Catalogs.Add(new AssemblyCatalog(typeof(PluginLoader).Assembly));
            sensors.Catalogs.Add(new DirectoryCatalog(folder));
            var compositonContainer = new CompositionContainer(sensors);
            try
            {
                compositonContainer.ComposeParts(this);
                return new ReadOnlyCollection<IPlugin>(this.plugins.Select(plugin => plugin.Value).ToList());
            }
            catch (Exception compositionException)
            {
                if (compositionException is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = compositionException as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    foreach (var loaderException in loaderExceptions)
                    {
                        Debug.WriteLine(loaderException.Message + " " + loaderException.StackTrace);    
                    }
                    
                }

                this.errorData = compositionException.Message + " " + compositionException.StackTrace;
                Debug.WriteLine(compositionException.ToString());
                return null;
            }
        }

        #endregion
    }
}