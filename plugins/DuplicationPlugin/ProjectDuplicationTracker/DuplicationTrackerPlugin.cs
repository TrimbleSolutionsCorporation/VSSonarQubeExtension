// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuplicationTrackerPlugin.cs" company="Copyright © 2014 jmecsoftware">
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

namespace ProjectDuplicationTracker
{
    using System;
    using System.ComponentModel.Composition;
    using System.Reflection;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Collections.Generic;

    using VSSonarPlugins.Types;

    using VSSonarPlugins;

    /// <summary>
    /// The duplication tracker plugin.
    /// </summary>
    [Export(typeof(IPlugin))]
    public class DuplicationTrackerPlugin : IMenuCommandPlugin
    {
        /// <summary>
        /// The model.
        /// </summary>
        private ProjectDuplicationTrackerModel model;
        private readonly ISonarRestService service;
        private List<string> Dlls = new List<string>();

        public DuplicationTrackerPlugin(ISonarRestService service)
        {
            this.service = service;
            this.Desc = new PluginDescription
            {
                Description = "Duplications Plugin",
                Name = "Duplications",
                SupportedExtensions = "*",
                Version = this.GetVersion(),
                AssemblyPath = this.GetAssemblyPath()
            };
        }

        public PluginDescription Desc { get; set; }

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
            return null;
        }

        /// <summary>
        /// The get header.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetHeader()
        {
            return "Duplications";
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
            this.InitModel(configuration, project, vshelper);
            
            var view = new DuplicationUserControl(this.model);
            return view;
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
            this.InitModel(configuration, project, vshelper);

            this.model.UpdateConfiguration(configuration, project, vshelper);
        }

        public PluginDescription GetPluginDescription()
        {
            return this.Desc;
        }

        public void ResetDefaults()
        {
        }

        public void AssociateProject(Resource project, ISonarConfiguration configuration, Dictionary<string, Profile> profile, string vsversion)
        {
            this.model.SelectMainResource(project);
        }

        /// <summary>
        /// The update theme.
        /// </summary>
        /// <param name="backgroundColor">
        /// The background color.
        /// </param>
        /// <param name="foregroundColor">
        /// The foreground color.
        /// </param>
        public void UpdateTheme(Color backgroundColor, Color foregroundColor)
        {
            if (this.model != null)
            {
                this.model.UpdateColours(backgroundColor, foregroundColor);
            }
        }

        /// <summary>
        /// The init model.
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
        private void InitModel(ISonarConfiguration configuration, Resource project, IVsEnvironmentHelper vshelper)
        {
            if (this.model == null)
            {
                this.model = new ProjectDuplicationTrackerModel(configuration, vshelper, this.service);
                this.model.Login();
                this.model.SelectMainResource(project);
            }
        }

        public string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public string GetAssemblyPath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        public IList<string> DllLocations()
        {
            return this.Dlls;
        }

        public void SetDllLocation(string path)
        {
            this.Dlls.Add(path);
        }

        public void Dispose()
        {
        }


        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public void OnConnectToSonar(ISonarConfiguration configuration)
        {
            // does nothing
        }
    }
}
