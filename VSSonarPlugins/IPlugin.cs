// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPlugin.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Windows.Media;
    using ExtensionTypes;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System;

    public class Message
    {
        public string Id { get; set; }
        public string Data { get; set; }
    }

    public interface INotificationManager
    {
        void ReportException(Exception ex);

        void ReportMessage(Message messages);
    }

    /// <summary>
    /// The Plugin interface. INotificationManager and IConfigurationHelper are injected during instantioation
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// The get use plugin control options.
        /// </summary>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see cref="UserControl"/>.
        /// </returns>
        IPluginControlOption GetPluginControlOptions(Resource project, ISonarConfiguration configuration);

        /// <summary>
        /// The get licenses.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        Dictionary<string, VsLicense> GetLicenses(ISonarConfiguration configuration);

        /// <summary>
        /// The generate token id.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GenerateTokenId(ISonarConfiguration configuration);

        /// <summary>
        /// The get plugin descritpion.
        /// </summary>
        /// <param name="vsinter">
        /// The vsinter.
        /// </param>
        /// <returns>
        /// The <see cref="PluginDescription"/>.
        /// </returns>
        PluginDescription GetPluginDescription();

        /// <summary>
        /// The reset defaults.
        /// </summary>
        void ResetDefaults();

        void AssociateProject(Resource project, ISonarConfiguration configuration);
    }


}