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
    using System.Collections.ObjectModel;
    using System.Windows;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    using PropertyChanged;

    using SonarRestService;
    using System.Windows.Input;
    /// <summary>
    ///     The server project viewer view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class ProjectViewModel
    {
        #region Fields

        /// <summary>
        ///     The selected profile.
        /// </summary>
        private Resource selectedProject;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="QualityViewerViewModel" /> class.
        /// </summary>
        public ProjectViewModel()
        {
            this.Service = new SonarRestService(new JsonSonarConnector());
            this.Projects = new ObservableCollection<Resource>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QualityViewerViewModel" /> class.
        /// </summary>
        /// <param name="config">
        ///     The config.
        /// </param>
        public ProjectViewModel(ISonarRestService service, ISonarConfiguration config)
        {
            this.Configuration = config;
            this.RestService = service;
            this.Service = new SonarRestService(new JsonSonarConnector());
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
        public ICommand ImportProfileCommand { get; set; }

        /// <summary>
        ///     Gets or sets the projects.
        /// </summary>
        public ObservableCollection<Resource> Projects { get; set; }

        /// <summary>
        ///     Gets or sets the refresh data command.
        /// </summary>
        public ICommand RefreshDataCommand { get; set; }

        /// <summary>
        ///     Gets or sets the selected profile.
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
                if (value != null)
                {
                    this.CanExecuteImportProfileCommand = true;
                }
                else
                {
                    this.CanExecuteImportProfileCommand = false;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the service.
        /// </summary>
        public ISonarRestService Service { get; set; }

        public ISonarRestService RestService { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     The execute import profile command.
        /// </summary>
        /// <param name="window">
        ///     The window.
        /// </param>
        private void ExecuteImportProfileCommand(Window window)
        {
            window.Hide();
        }

        /// <summary>
        ///     The execute refresh data command.
        /// </summary>
        private void ExecuteRefreshDataCommand(object data)
        {
            this.Projects = new ObservableCollection<Resource>(this.RestService.GetProjectsList(this.Configuration));
        }

        /// <summary>
        ///     The start command.
        /// </summary>
        private void StartCommand()
        {
            this.ImportProfileCommand = new RelayCommand<Window>(this.ExecuteImportProfileCommand);
            this.RefreshDataCommand = new RelayCommand<object>(this.ExecuteRefreshDataCommand);
        }

        #endregion
    }
}