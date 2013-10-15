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
        /// The plugins.
        /// </summary>
        [ImportMany]
        private IEnumerable<Lazy<IPlugin>> plugins;

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
        /// The get plugin to run resource.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <returns>
        /// The <see cref="IPlugin"/>.
        /// </returns>
        public IPlugin GetPluginToRunResource(string resource)
        {
            if (this.loadedPlugins == null)
            {
                return null;
            }

            var pluginsToUse = new List<IPlugin>();

            foreach (var plugin in this.loadedPlugins)
            {
                if (plugin.IsSupported(resource))
                {
                    if (plugin.GetUsePluginControlOptions() == null)
                    {
                        pluginsToUse.Add(plugin);
                        continue;
                    }

                    if (plugin.GetUsePluginControlOptions().IsEnabled())
                    {
                        pluginsToUse.Add(plugin);
                    }
                }
            }

            return this.PickPluginFromMultipleSupportedPlugins(pluginsToUse);
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
