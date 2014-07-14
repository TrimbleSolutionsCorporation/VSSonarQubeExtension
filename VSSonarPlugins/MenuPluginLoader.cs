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
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;

    using ExtensionTypes;

    /// <summary>
    /// The local analyser.
    /// </summary>
    public class MenuPluginLoader : IMenuPluginLoader
    {
        /// <summary>
        /// The plugins.
        /// </summary>
        [ImportMany]
        private IEnumerable<Lazy<IMenuCommandPlugin>> menuPlugins;

        private string errorData;

        /// <summary>
        /// The load plugins from folder.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <returns>
        /// The <see cref="ReadOnlyCollection"/>.
        /// </returns>
        public ReadOnlyCollection<IMenuCommandPlugin> LoadPluginsFromFolder(string folder)
        {
            
            var sensors = new AggregateCatalog();
            sensors.Catalogs.Add(new AssemblyCatalog(typeof(PluginController).Assembly));
            sensors.Catalogs.Add(new DirectoryCatalog(folder));
            var compositonContainer = new CompositionContainer(sensors);
            try
            {
                compositonContainer.ComposeParts(this);
                return new ReadOnlyCollection<IMenuCommandPlugin>(this.menuPlugins.Select(plugin => plugin.Value).ToList());
            }
            catch (Exception compositionException)
            {
                this.errorData = compositionException.Message + " " + compositionException.StackTrace;
                Debug.WriteLine(compositionException.ToString());
            }

            return null;
        }

        public string GetErrorData()
        {
            return this.errorData;
        }
    }
}
