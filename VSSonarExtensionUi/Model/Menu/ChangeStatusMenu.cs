// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeStatusHandler.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;

    using Association;
    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using View.Helpers;
    using ViewModel.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using System.Collections.Generic;
    using SonarRestService.Types;
    using SonarRestService;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Linq;

    /// <summary>
    /// The issue handler menu.
    /// </summary>
    internal class ChangeStatusMenu : IMenuItem
    {
        /// <summary>
        ///     The model.
        /// </summary>
        private readonly IssueGridViewModel model;

        /// <summary>
        ///     The rest.
        /// </summary>
        private readonly ISonarRestService rest;

        /// <summary>
        /// The manager
        /// </summary>
        private readonly INotificationManager logger;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStatusMenu" /> class.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="notmanager">The notmanager.</param>
        private ChangeStatusMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager notmanager)
        {
            this.model = model;
            this.rest = rest;
            this.logger = notmanager;
            this.ExecuteCommand = new RelayCommand(this.OncChangeIssueStatus);
            this.SubItems = new ObservableCollection<IMenuItem>();

            // register menu for data sync
            AssociationModel.RegisterNewModelInPool(this);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the associated command.
        /// </summary>
        public ICommand ExecuteCommand { get; set; }

        /// <summary>
        ///     Gets or sets the command text.
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the sub items.
        /// </summary>
        public ObservableCollection<IMenuItem> SubItems { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        /// <param name="availableProjects">The available projects.</param>
        /// <param name="plugin">The plugin.</param>
        public void OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IList<IIssueTrackerPlugin> plugin)
        {
            // does nothing
        }

        /// <summary>
        /// The make menu.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="notmanager">The notmanager.</param>
        /// <returns>
        /// The <see cref="IMenuItem" />.
        /// </returns>
        public static IMenuItem MakeMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager notmanager)
        {
            var topLel = new ChangeStatusMenu(rest, model, notmanager) { CommandText = "Status", IsEnabled = false };

            topLel.SubItems.Add(new ChangeStatusMenu(rest, model, notmanager) { CommandText = "Confirm", IsEnabled = true });
            topLel.SubItems.Add(new ChangeStatusMenu(rest, model, notmanager) { CommandText = "Fix", IsEnabled = true });
            topLel.SubItems.Add(new ChangeStatusMenu(rest, model, notmanager) { CommandText = "False Positive", IsEnabled = true });
            return topLel;
        }

        /// <summary>
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public void UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            // menu not accessing services
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="configIn">The configuration in.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public void AssociateWithNewProject(
            Resource project,
            string workingDir,
            ISourceControlProvider provider,
            Dictionary<string, Profile> profile,
            string visualStudioVersion)
        {
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void OnSolutionClosed()
        {
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public void OnDisconnect()
        {
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

        #endregion

        #region Methods

        /// <summary>
        ///     The on associate command.
        /// </summary>
        private async void OncChangeIssueStatus()
        {
            try
            {
                if (this.CommandText.Equals("False Positive"))
                {
                        this.logger.StartedWorking("Marking Issues as False Posiive");
                        var replies = await this.rest.MarkIssuesAsFalsePositive(
							AuthtenticationHelper.AuthToken,
							this.model.SelectedItems.Cast<Issue>().ToList(),
							string.Empty,
							this.logger,
							new CancellationTokenSource().Token);
                        this.model.RefreshView();
						this.logger.EndedWorking();
                }

                if (this.CommandText.Equals("Confirm"))
                {
                    this.logger.StartedWorking("Confirming Issues");
                    var replies = await this.rest.ConfirmIssues(
						AuthtenticationHelper.AuthToken,
						this.model.SelectedItems.Cast<Issue>().ToList(),
						string.Empty,
						this.logger,
						new CancellationTokenSource().Token);
					this.logger.EndedWorking();
					this.model.RefreshView();
                }

                if (this.CommandText.Equals("Fix"))
                {
					this.logger.StartedWorking("Marking issues as fixed");
					var replies = await this.rest.ResolveIssues(
						AuthtenticationHelper.AuthToken,
						this.model.SelectedItems.Cast<Issue>().ToList(),
						string.Empty,
						this.logger,
						new CancellationTokenSource().Token);
					this.model.RefreshView();
					this.logger.EndedWorking();
                }
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Cannot Modify Status Of Issues", ex);
            }
        }

        #endregion
    }
}