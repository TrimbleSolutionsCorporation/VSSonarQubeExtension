﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MoreInfoMenu.cs" company="Copyright © 2016 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2014 [Jorge Costa, Jorge.Costa@tekla.com]
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
namespace VSSonarExtensionUi.Model.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Association;

    using GalaSoft.MvvmLight.Command;

    using Helpers;

    using SonarLocalAnalyser;

    using SonarRestService;
    using SonarRestService.Types;

    using View.Helpers;

    using ViewModel.Helpers;

    using VSSonarPlugins;

    /// <summary>
    /// Source Control Related Actions
    /// </summary>
    public class MoreInfoMenu : IMenuItem
    {
        /// <summary>
        /// The rest service
        /// </summary>
        private readonly ISonarRestService rest;

        /// <summary>
        /// The model service
        /// </summary>
        private readonly IssueGridViewModel model;

        /// <summary>
        /// The manager
        /// </summary>
        private readonly INotificationManager manager;

        /// <summary>
        /// The tranlator
        /// </summary>
        private readonly ISQKeyTranslator tranlator;

        /// <summary>
        /// The analyser
        /// </summary>
        private readonly ISonarLocalAnalyser analyser;

        /// <summary>
        /// The source control
        /// </summary>
        private ISourceControlProvider sourceControl;

        /// <summary>
        /// The vshelper
        /// </summary>
        private IVsEnvironmentHelper vshelper;

        /// <summary>
        /// The assign project
        /// </summary>
        private Resource assignProject;

        /// <summary>
        /// The available projects
        /// </summary>
        private IList<Resource> availableProjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceControlMenu" /> class.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="translator">The translator.</param>
        public MoreInfoMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager manager, ISQKeyTranslator translator, ISonarLocalAnalyser analyser)
        {
            this.analyser = analyser;
            this.rest = rest;
            this.model = model;
            this.manager = manager;
            this.tranlator = translator;

            this.ExecuteCommand = new RelayCommand(this.OnShowMoreInfoCommand);
            this.SubItems = new ObservableCollection<IMenuItem>();

            // register menu for data sync
            AssociationModel.RegisterNewModelInPool(this);
        }

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// Gets or sets the associated command.
        /// </summary>
        public ICommand ExecuteCommand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the sub items.
        /// </summary>
        public ObservableCollection<IMenuItem> SubItems { get; set; }

        /// <summary>
        /// The make menu.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="translator">The translator.</param>
        /// <returns>
        /// The <see cref="IMenuItem" />.
        /// </returns>
        public static IMenuItem MakeMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager manager, ISQKeyTranslator translator, ISonarLocalAnalyser analyser)
        {
            var topLel = new MoreInfoMenu(rest, model, manager, translator, analyser) { CommandText = "More Info", IsEnabled = true };
            return topLel;
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="sourceModel">The source model.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public async Task AssociateWithNewProject(
            Resource project,
            string workingDir,
            ISourceControlProvider sourceModel,
            Dictionary<string, Profile> profile,
            string visualStudioVersion)
        {
            await Task.Delay(0);
            this.sourceControl = sourceModel;
            this.assignProject = project;
        }

        /// <summary>
        /// Refreshes the menu data for menu that have options that
        /// are context dependent on the selected issues.
        /// </summary>
        public async Task RefreshMenuData()
        {
			// not necessary
			await Task.Delay(0);
		}

        /// <summary>
        /// Cancels the refresh data.
        /// </summary>
        public async Task CancelRefreshData()
        {
			// not necessary
			await Task.Delay(0);
		}

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        /// <param name="availableProjectsIn">The available projects in.</param>
        public async Task OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjectsIn, IList<IIssueTrackerPlugin> sourcePlugin)
        {
            this.availableProjects = availableProjectsIn.ToList();
            await Task.Delay(0);
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public async Task OnSolutionClosed()
        {
            // not needed
            await Task.Delay(0);
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public async Task OnDisconnect()
        {
            this.availableProjects = null;
            await Task.Delay(0);
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>
        /// returns view model
        /// </returns>
        public object GetViewModel()
        {
            return null;
        }

        /// <summary>
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vs environment helper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public async Task UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            this.vshelper = vsenvironmenthelperIn;
            await Task.Delay(0);
        }

        /// <summary>
        /// Called when [source control command].
        /// </summary>
        private void OnShowMoreInfoCommand()
        {
            if (this.model.SelectedItems == null || this.model.SelectedItems.Count == 0)
            {
                return;
            }

            try
            {
                if (this.CommandText.Equals("More Info"))
                {
                    if (!string.IsNullOrEmpty(this.model.SelectedIssue.HelpUrl))
                    {
                        this.vshelper.NavigateToResource(this.model.SelectedIssue.HelpUrl);
                    }
                    else
                    {
                        MessageDisplayBox.DisplayMessage("Issue does not have help URL  defined.");
                    }                    
                }
            }
            catch (Exception ex)
            {
                this.manager.ReportMessage(new Message { Id = "MoreInfoMenu", Data = "Failed to perform operation: " + ex.Message });
                this.manager.ReportException(ex);
            }
        }
    }
}
