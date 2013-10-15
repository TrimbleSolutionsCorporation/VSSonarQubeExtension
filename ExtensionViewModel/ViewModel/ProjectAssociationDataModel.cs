// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectAssociationDataModel.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace ExtensionViewModel.ViewModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using ExtensionTypes;
    using ExtensionViewModel.Commands;
    using SonarRestService;

    /// <summary>
    /// The project association data model.
    /// </summary>
    public class ProjectAssociationDataModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The rest service.
        /// </summary>
        private readonly ISonarRestService restService;

        /// <summary>
        /// The project resources.
        /// </summary>
        private List<Resource> projectResources;

        /// <summary>
        /// The selected project.
        /// </summary>
        private Resource selectedProject;

        /// <summary>
        /// The associated project.
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectAssociationDataModel"/> class.
        /// This is here only for blend to recognize the data class 
        /// </summary>
        public ProjectAssociationDataModel()
        {
            this.projectResources = new List<Resource>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectAssociationDataModel"/> class.
        /// </summary>
        /// <param name="restService">
        /// The rest service.
        /// </param>
        /// <param name="userConfiguration">
        /// The user configuration.
        /// </param>
        /// <param name="projectKey">
        /// The project Key.
        /// </param>
        public ProjectAssociationDataModel(ISonarRestService restService, ConnectionConfiguration userConfiguration, string projectKey)
        {
            this.restService = restService;
            this.UserConfiguration = userConfiguration;
            this.ProjectResources = restService.GetProjectsList(this.UserConfiguration);

            this.AssociateCommand = new AssociateCommand(this);
            this.RetriveProjectsFromServerCommand = new RetriveProjectsFromServerCommand(this, this.restService);

            if (!string.IsNullOrEmpty(projectKey))
            {
                var profile = this.restService.GetQualityProfile(userConfiguration, projectKey);
                this.Profile = this.restService.GetEnabledRulesInProfile(userConfiguration, profile[0].Lang, profile[0].Metrics[0].Data)[0];
                foreach (var projectResource in this.ProjectResources)
                {
                    if (projectResource.Key.Equals(projectKey))
                    {
                        this.AssociatedProject = projectResource;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectAssociationDataModel"/> class.
        /// </summary>
        /// <param name="restService">
        /// The rest service.
        /// </param>
        /// <param name="userConfiguration">
        /// The user configuration.
        /// </param>
        public ProjectAssociationDataModel(ISonarRestService restService, ConnectionConfiguration userConfiguration)
        {
            this.restService = restService;
            this.UserConfiguration = userConfiguration;
            if (userConfiguration != null)
            {
                this.ProjectResources = restService.GetProjectsList(this.UserConfiguration);
                this.AssociateCommand = new AssociateCommand(this);
                this.RetriveProjectsFromServerCommand = new RetriveProjectsFromServerCommand(this, this.restService);
            }
        }

        /// <summary>
        /// The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the assign on issue command.
        /// </summary>
        public AssociateCommand AssociateCommand { get; set; }

        /// <summary>
        /// Gets or sets the retrive projects from server.
        /// </summary>
        public RetriveProjectsFromServerCommand RetriveProjectsFromServerCommand { get; set; }

        /// <summary>
        /// Gets or sets the profile.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Gets or sets the user configuration.
        /// </summary>
        public ConnectionConfiguration UserConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the selected project.
        /// </summary>
        public Resource AssociatedProject
        {
            get
            {
                return this.associatedProject;
            }

            set
            {
                this.associatedProject = value;
                this.OnPropertyChanged("AssociatedProject");
            }
        }

        /// <summary>
        /// Gets or sets the selected project.
        /// </summary>
        public Resource SelectedProject
        {
            get
            {
                return this.selectedProject;
            }

            set
            {
                this.selectedProject = value;
                this.OnPropertyChanged("SelectedProject");
            }
        }
        
        /// <summary>
        /// Gets or sets the users list.
        /// </summary>
        public List<Resource> ProjectResources
        {
            get
            {
                return this.projectResources;
            }

            set
            {
                this.projectResources = value;
                this.OnPropertyChanged("ProjectResources");
            }
        }

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        protected void OnPropertyChanged(string name)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
