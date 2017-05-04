// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPluginController.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using VSSonarPlugins.Types;

    

    /// <summary>
    ///     The PluginController interface.
    /// </summary>
    public interface IPluginController
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get error data.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetErrorData();

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
        /// The <see cref="IMenuCommandPlugin"/>.
        /// </returns>
        IPlugin IstallNewPlugin(string fileName, ISonarConfiguration conf,
            IConfigurationHelper helper,
            INotificationManager manager,
            IVsEnvironmentHelper vshelper,
            string folder);

        /// <summary>
        /// The pick plugin from multiple supported plugins.
        /// </summary>
        /// <param name="pluginsToUse">
        /// The plugins to use.
        /// </param>
        /// <returns>
        /// The <see cref="IAnalysisPlugin"/>.
        /// </returns>
        IAnalysisPlugin PickPluginFromMultipleSupportedPlugins(ReadOnlyCollection<IAnalysisPlugin> pluginsToUse);

        /// <summary>
        /// The remove plugin.
        /// </summary>
        /// <param name="conf">
        /// The conf.
        /// </param>
        /// <param name="selectedPlugin">
        /// The selected plugin.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool RemovePlugin(IPlugin selectedPlugin, IList<IPlugin> installedPlugins);


        List<IPlugin> LoadPluginsFromPluginFolder(INotificationManager manager, IConfigurationHelper helper, IVsEnvironmentHelper vshelper, IEnumerable<string> files, string folder);

        List<string> DeployPlugin(string fileName, string folder);

        string ExtensionFolder { get; }

        #endregion
    }
}