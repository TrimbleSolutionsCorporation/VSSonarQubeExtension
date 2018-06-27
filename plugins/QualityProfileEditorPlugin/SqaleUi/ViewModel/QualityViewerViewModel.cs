// --------------------------------------------------------------------------------------------------------------------
// <copyright file="qualityviewerviewmodel.cs" company="Copyright © 2014 jmecsoftware">
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

namespace SqaleUi.ViewModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Media;

    using VSSonarPlugins.Types;

    using PropertyChanged;

    using VSSonarPlugins;
    using SonarRestService;

    using SqaleUi.Menus;
    using System.Windows.Input;
    /// <summary>
    ///     The server project viewer view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class QualityViewerViewModel : IViewModelTheme
    {
        #region Fields

        /// <summary>
        /// The selected profile.
        /// </summary>
        private Profile selectedProfile;

        /// <summary>
        ///     The selected profile.
        /// </summary>
        private SonarProject selectedProject;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="QualityViewerViewModel" /> class.
        /// </summary>
        public QualityViewerViewModel()
        {
            this.Profiles = new ObservableCollection<Profile>();
            this.Service = new SonarRestService(new JsonSonarConnector());
            this.Projects = new ObservableCollection<SonarProject>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QualityViewerViewModel"/> class.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="showOnlyProfiles">
        /// The show Only Profiles.
        /// </param>
        public QualityViewerViewModel(ISonarConfiguration config, ISqaleGridVm model, bool showOnlyProfiles = false)
        {
            this.Model = model;
            this.Configuration = config;
            this.ShowOnlyProfiles = showOnlyProfiles;

            this.Service = new SonarRestService(new JsonSonarConnector());
            this.Profiles = new ObservableCollection<Profile>();
            this.Projects = new ObservableCollection<SonarProject>();
            this.StartCommand();

            this.ExecuteRefreshDataCommand(null);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether can execute import profile command.
        /// </summary>
        public bool CanExecuteImportProfileCommand { get; set; }

        /// <summary>
        ///     Gets or sets the configuration.
        /// </summary>
        public ISonarConfiguration Configuration { get; set; }

        /// <summary>
        ///     Gets or sets the import profile command.
        /// </summary>
        public RelayCommand<Window> ImportProfileCommand { get; set; }

        /// <summary>
        ///     Gets or sets the model.
        /// </summary>
        public ISqaleGridVm Model { get; set; }

        /// <summary>
        ///     Gets or sets the projects.
        /// </summary>
        public ObservableCollection<Profile> Profiles { get; set; }

        /// <summary>
        /// Gets or sets the projects.
        /// </summary>
        public ObservableCollection<SonarProject> Projects { get; set; }

        /// <summary>
        /// Gets or sets the refresh data command.
        /// </summary>
        public ICommand RefreshDataCommand { get; set; }

        /// <summary>
        /// Gets or sets the selected profile.
        /// </summary>
        public Profile SelectedProfile
        {
            get
            {
                return this.selectedProfile;
            }

            set
            {
                this.selectedProfile = value;
                this.CanExecuteImportProfileCommand = value != null;
            }
        }

        /// <summary>
        ///     Gets or sets the selected profile.
        /// </summary>
        [AlsoNotifyFor("SelectedProfile")]
        public SonarProject SelectedProject
        {
            get
            {
                return this.selectedProject;
            }

            set
            {
                this.selectedProject = value;
                this.Profiles.Clear();
                if (value != null)
                {
                    foreach (Profile profile in value.Profiles)
                    {
                        this.Profiles.Add(profile);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the service.
        /// </summary>
        public ISonarRestService Service { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether show only profiles.
        /// </summary>
        public bool ShowOnlyProfiles { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The execute import profile command.
        /// </summary>
        /// <param name="window">
        /// The window.
        /// </param>
        private void ExecuteImportProfileCommand(Window window)
        {
            if (this.SelectedProfile.GetAllRules().Count == 0 || this.Model.ProfileRules.Count == 0)
            {
                this.Service.GetRulesForProfileUsingRulesApp(this.Configuration, this.SelectedProfile, true);

                this.Service.GetRulesForProfileUsingRulesApp(this.Configuration, this.SelectedProfile, false);
            }

            this.Model.SelectedProfile = this.SelectedProfile;
            this.Model.MergeRulesIntoProject(this.SelectedProfile.GetAllRules());
            this.Model.SetConnectedToServer(true);

            window.Hide();
        }

        /// <summary>
        /// The execute refresh data command.
        /// </summary>
        private void ExecuteRefreshDataCommand(object data)
        {
            this.Projects.Clear();
            this.Profiles.Clear();
            if (this.ShowOnlyProfiles)
            {
                List<Profile> profiles = this.Service.GetProfilesUsingRulesApp(this.Configuration);
                foreach (Profile profile in profiles)
                {
                    this.Profiles.Add(profile);
                }
            }
            else
            {
                List<SonarProject> projects = this.Service.GetProjects(this.Configuration);
                List<Profile> profiles = this.Service.GetProfilesUsingRulesApp(this.Configuration);
                foreach (SonarProject sonarProject in projects)
                {
                    foreach (Profile profile in this.Service.GetQualityProfilesForProject(this.Configuration, new Resource { Key = sonarProject.Key }))
                    {
                        foreach (Profile profile1 in profiles)
                        {
                            if (profile1.Name.Equals(profile.Name) && profile1.Language.Equals(profile.Language))
                            {
                                profile.Key = profile1.Key;
                            }
                        }

                        sonarProject.Profiles.Add(profile);
                    }

                    this.Projects.Add(sonarProject);
                }
            }
        }

        /// <summary>
        ///     The start command.
        /// </summary>
        private void StartCommand()
        {
            this.ImportProfileCommand = new RelayCommand<Window>(this.ExecuteImportProfileCommand);
            this.RefreshDataCommand = new RelayCommand<object>(this.ExecuteRefreshDataCommand);
        }

        public void UpdateColors(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        public Color BackGroundColor { get; set; }

        public Color ForeGroundColor { get; set; }
        #endregion
    }
}