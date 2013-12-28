// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPlugin.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Windows.Controls;
    using System.Windows.Documents;

    using ExtensionTypes;

    /// <summary>
    /// The Plugin interface.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// The get key.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetKey(ConnectionConfiguration configuration);

        /// <summary>
        /// The get use plugin control options.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see cref="UserControl"/>.
        /// </returns>
        IPluginsOptions GetPluginControlOptions(ConnectionConfiguration configuration, Resource project);

        /// <summary>
        /// The get plugin control options.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see cref="IPluginsOptions"/>.
        /// </returns>
        IPluginsOptions GetPluginControlOptions(ConnectionConfiguration configuration);

        /// <summary>
        /// The is supported.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="resource">
        /// The language.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool IsSupported(ConnectionConfiguration configuration, string resource);

        /// <summary>
        /// The is supported.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool IsSupported(ConnectionConfiguration configuration, Resource resource);

        /// <summary>
        /// The get resource key.
        /// </summary>
        /// <param name="projectItem">
        /// The project item.
        /// </param>
        /// <param name="projectKey">
        /// The project key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetResourceKey(VsProjectItem projectItem, string projectKey);

        /// <summary>
        /// The get local analysis extensions.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        ILocalAnalyserExtension GetLocalAnalysisExtension(ConnectionConfiguration configuration, Resource project);

        /// <summary>
        /// The get server analyser extensions.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        IServerAnalyserExtension GetServerAnalyserExtension(ConnectionConfiguration configuration, Resource project);

        /// <summary>
        /// The get licenses.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="forceRetriveFromServer">
        /// The force Retrive From Server.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        Dictionary<string, VsLicense> GetLicenses(ConnectionConfiguration configuration, bool forceRetriveFromServer);

        /// <summary>
        /// The generate token id.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GenerateTokenId(ConnectionConfiguration configuration);
    }
}
