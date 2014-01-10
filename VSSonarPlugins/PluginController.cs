// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginController.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using ExtensionTypes;

    /// <summary>
    /// The local analyser.
    /// </summary>
    public class PluginController : IPluginController
    {
        /// <summary>
        /// The loaded plugins.
        /// </summary>
        private readonly ReadOnlyCollection<IPlugin> loadedPlugins;

        /// <summary>
        /// The loaded plugins.
        /// </summary>
        private readonly ReadOnlyCollection<IMenuCommandPlugin> menuCommandPlugins;

        /// <summary>
        /// The plugins.
        /// </summary>
        [ImportMany]
        private IEnumerable<Lazy<IPlugin>> plugins;

        /// <summary>
        /// The plugins.
        /// </summary>
        [ImportMany]
        private IEnumerable<Lazy<IMenuCommandPlugin>> menuPlugins;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginController"/> class. 
        /// </summary>
        public PluginController()
        {
            var sensors = new AggregateCatalog();
            sensors.Catalogs.Add(new AssemblyCatalog(typeof(PluginController).Assembly));
            var directoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            if (directoryName == null)
            {
                return;
            }

            var path = directoryName.Replace("file:\\", string.Empty);
            {
                sensors.Catalogs.Add(new DirectoryCatalog(path));
                var compositonContainer = new CompositionContainer(sensors);
                try
                {
                    compositonContainer.ComposeParts(this);
                }
                catch (CompositionException compositionException)
                {
                    Debug.WriteLine(compositionException.ToString());
                }
            }

            if (this.plugins != null)
            {
                this.loadedPlugins = new ReadOnlyCollection<IPlugin>(this.plugins.Select(plugin => plugin.Value).ToList());
            }

            if (this.menuPlugins != null)
            {
                this.menuCommandPlugins = new ReadOnlyCollection<IMenuCommandPlugin>(this.menuPlugins.Select(plugin => plugin.Value).ToList());
            }
        }

        /// <summary>
        /// The get plugins options.
        /// </summary>
        /// <returns>
        /// The <see>
        ///     <cref>Dictionary</cref>
        /// </see>
        ///     .
        /// </returns>
        public ReadOnlyCollection<IPlugin> GetPlugins()
        {
            return this.loadedPlugins;
        }

        /// <summary>
        /// The get plugins options.
        /// </summary>
        /// <returns>
        /// The <see>
        ///     <cref>Dictionary</cref>
        /// </see>
        ///     .
        /// </returns>
        public ReadOnlyCollection<IMenuCommandPlugin> GetMenuItemPlugins()
        {
            return this.menuCommandPlugins;
        }

        /// <summary>
        /// The get plugin to run resource.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see cref="IPlugin"/>.
        /// </returns>
        public IPlugin GetPluginToRunResource(ConnectionConfiguration configuration, Resource project)
        {
            if (this.loadedPlugins == null)
            {
                return null;
            }

            var pluginsToUse = new List<IPlugin>();

            foreach (var plugin in this.loadedPlugins)
            {
                if (plugin.IsSupported(configuration, project))
                {
                    pluginsToUse.Add(plugin);
                }
            }

            return this.PickPluginFromMultipleSupportedPlugins(pluginsToUse);
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

            return (from plugin in this.menuPlugins where plugin.Value.GetHeader().Equals(key) select plugin.Value).FirstOrDefault();
        }

        /// <summary>
        /// The pick plugin from multiple supported plugins.
        /// </summary>
        /// <param name="pluginsToUse">
        /// The plugins to use.
        /// </param>
        /// <returns>
        /// The <see cref="IPlugin"/>.
        /// </returns>
        public IPlugin PickPluginFromMultipleSupportedPlugins(List<IPlugin> pluginsToUse)
        {
            if (pluginsToUse != null && pluginsToUse.Any())
            {
                return pluginsToUse.First();
            }

            return null;
        }
    }
}
