// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenResourceMenu.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Windows.Input;



    using GalaSoft.MvvmLight.Command;

    using View.Helpers;
    using ViewModel;
    using ViewModel.Helpers;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using SonarLocalAnalyser;
    using Helpers;
    using Association;

    /// <summary>
    ///     The issue handler menu.
    /// </summary>
    internal class OpenResourceMenu : IMenuItem
    {
        #region Fields

        /// <summary>
        /// The model.
        /// </summary>
        private readonly IssueGridViewModel model;

        /// <summary>
        /// The rest.
        /// </summary>
        private readonly ISonarRestService rest;

        /// <summary>
        /// The vs helper.
        /// </summary>
        private IVsEnvironmentHelper visualStudioHelper;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// The source dir
        /// </summary>
        private string sourceDir;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenResourceMenu" /> class.
        /// Initializes a new instance of the <see cref="ChangeStatusMenu" /> class.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        private OpenResourceMenu(ISonarRestService rest, IssueGridViewModel model)
        {
            this.model = model;
            this.rest = rest;
            this.ExecuteCommand = new RelayCommand(this.OnAssociateCommand);
            this.SubItems = new ObservableCollection<IMenuItem>();

            // register menu for data sync
            AssociationModel.RegisterNewModelInPool(this);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the associated command.
        /// </summary>
        public ICommand ExecuteCommand { get; set; }

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the sub items.
        /// </summary>
        public ObservableCollection<IMenuItem> SubItems { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The make menu.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <returns>
        /// The <see cref="IMenuItem" />.
        /// </returns>
        public static IMenuItem MakeMenu(ISonarRestService rest, IssueGridViewModel model)
        {
            var topLel = new OpenResourceMenu(rest, model) { CommandText = "Open", IsEnabled = false };

            topLel.SubItems.Add(new OpenResourceMenu(rest, model) { CommandText = "Visual Studio", IsEnabled = true });
            topLel.SubItems.Add(new OpenResourceMenu(rest, model) { CommandText = "Browser", IsEnabled = true });
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
            this.visualStudioHelper = vsenvironmenthelperIn;
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="configIn">The configuration in.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public void AssociateWithNewProject(Resource project, string workingDir, ISourceControlProvider provider, IIssueTrackerPlugin sourcePlugin)
        {
            this.associatedProject = project;
            this.sourceDir = workingDir;
        }

        /// <summary>
        /// Refreshes the menu data for menu that have options that
        /// are context dependent on the selected issues.
        /// </summary>
        public void RefreshMenuData()
        {
            // not necessary
        }

        /// <summary>
        /// Cancels the refresh data.
        /// </summary>
        public void CancelRefreshData()
        {
            // not necessay
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void EndDataAssociation()
        {
            this.associatedProject = null;
            this.sourceDir = string.Empty;
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


        #endregion

        #region Methods

        /// <summary>
        /// The on associate command.
        /// </summary>
        private void OnAssociateCommand()
        {
            try
            {
                if (this.CommandText.Equals("Browser"))
                {
                    foreach (var issueobj in this.model.SelectedItems)
                    {
                        var issue = issueobj as Issue;
                        if (issue == null)
                        {
                            continue;
                        }

                        var resources = this.rest.GetResourcesData(AuthtenticationHelper.AuthToken, issue.Component);
                        this.visualStudioHelper.NavigateToResource(AuthtenticationHelper.AuthToken.Hostname + "/resource/index/" + resources[0].Id);
                    }
                }

                if (this.CommandText.Equals("Visual Studio"))
                {
                    this.model.OnOpenInVsCommand(this.model.SelectedItems);
                }
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Cannot Open Issue in Editor: " + ex.Message + " please check vs output log for detailed information", ex);
            }
        }

        #endregion
    }
}