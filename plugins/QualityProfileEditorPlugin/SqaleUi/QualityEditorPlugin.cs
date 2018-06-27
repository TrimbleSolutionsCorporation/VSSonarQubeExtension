// --------------------------------------------------------------------------------------------------------------------
// <copyright file="qualityeditorplugin.cs" company="Copyright © 2014 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
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

namespace SqaleUi
{
    using System;
    using System.ComponentModel.Composition;
    using System.Reflection;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Collections.Generic;

    using VSSonarPlugins.Types;

    using SonarRestService;

    using SqaleUi.View;
    using SqaleUi.ViewModel;

    using VSSonarPlugins;
    using System.Security.Permissions;
    using System.Runtime.Serialization;

    /// <summary>
    /// The quality editor plugin.
    /// </summary>
    [Export(typeof(IPlugin))]
    public class QualityEditorPlugin : IMenuCommandPlugin
    {
        private readonly ISonarRestService service;

        List<string> dlls = new List<string>();

        public QualityEditorPlugin(ISonarRestService service)
        {
            this.service = service;
            this.Desc = new PluginDescription
            {
                Description = "Quality Editor Plugin",
                Name = "QualityEditorPlugin",
                SupportedExtensions = "*",
                Version = this.GetVersion(),
                AssemblyPath = this.GetAssemblyPath()
            };
        }
        #region Public Properties

        /// <summary>
        /// Gets or sets the editor.
        /// </summary>
        public SqaleGridVs Editor { get; set; }

        #endregion

        #region Public Methods and Operators

        public IPluginControlOption GetPluginControlOptions(ISonarConfiguration configuration)
        {
            return null;
        }

        /// <summary>
        /// The generate token id.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GenerateTokenId(ISonarConfiguration configuration)
        {
            return string.Empty;
        }

        public IPluginControlOption GetPluginControlOptions(Resource project, ISonarConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get assembly path.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetAssemblyPath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        public void UpdateTheme(Color backgroundColor, Color foregroundColor)
        {
            if (this.Editor != null)
            {
                this.Model.UpdateColors(backgroundColor, foregroundColor);
            }
        }

        /// <summary>
        /// The get header.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetHeader()
        {
            return "Sonar Quality Editor";
        }

        /// <summary>
        /// The get plugin description.
        /// </summary>
        /// <param name="vsinter">
        /// The vsinter.
        /// </param>
        /// <returns>
        /// The <see cref="PluginDescription"/>.
        /// </returns>
        public PluginDescription GetPluginDescription()
        {
            return this.Desc;
        }

        public void ResetDefaults()
        {
        }

        public void AssociateProject(Resource project, ISonarConfiguration configuration, Dictionary<string, Profile> profile, string vsversion)
        {
            this.Model.Project = project;
        }

        /// <summary>
        /// The get user control.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="vshelper">
        /// The vshelper.
        /// </param>
        /// <returns>
        /// The <see cref="UserControl"/>.
        /// </returns>
        public UserControl GetUserControl(ISonarConfiguration configuration, Resource project, IVsEnvironmentHelper vshelper)
        {
            if (this.Editor == null)
            {
                this.Model = new SqaleGridVmVs(project, new SonarRestService(new JsonSonarConnector()), configuration);
                this.Editor = new SqaleGridVs(this.Model);
            }

            return this.Editor;
        }

        public SqaleGridVmVs Model { get; set; }

        /// <summary>
        /// The get version.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// The update configuration.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="vshelper">
        /// The vshelper.
        /// </param>
        public void UpdateConfiguration(ISonarConfiguration configuration, Resource project, IVsEnvironmentHelper vshelper)
        {
            if (this.Editor == null)
            {
                this.Model = new SqaleGridVmVs(project, new SonarRestService(new JsonSonarConnector()), configuration);
                this.Editor = new SqaleGridVs(this.Model);
            }

            this.Model.UpdateConfiguration(configuration, project, vshelper);
        }

        public IList<string> DllLocations()
        {
            return this.dlls;
        }

        public void SetDllLocation(string path)
        {
            this.dlls.Add(path);
        }

        public void Dispose()
        {
        }

        #endregion

        public PluginDescription Desc { get; set; }


        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public void OnConnectToSonar(ISonarConfiguration configuration)
        {
            // nothing
        }
    }
}