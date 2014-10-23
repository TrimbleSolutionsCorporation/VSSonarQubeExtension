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

namespace VSSonarExtensionUi.Menu
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    using ExtensionTypes;

    using GalaSoft.MvvmLight.Command;

    using SonarRestService;

    using VSSonarExtensionUi.View.Helpers;
    using VSSonarExtensionUi.ViewModel;
    using VSSonarExtensionUi.ViewModel.Helpers;

    using VSSonarPlugins;

    /// <summary>
    ///     The issue handler menu.
    /// </summary>
    internal class OpenResourceMenu : IMenuItem
    {
        #region Fields

        /// <summary>
        /// The config.
        /// </summary>
        private readonly ISonarConfiguration config;

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
        private readonly IVsEnvironmentHelper visualStudioHelper;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenResourceMenu"/> class. 
        /// Initializes a new instance of the <see cref="ChangeStatusHandler"/> class.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="rest">
        /// The rest.
        /// </param>
        /// <param name="visualStudioHelper">
        /// The visual Studio Helper.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        private OpenResourceMenu(ISonarConfiguration config, ISonarRestService rest, IVsEnvironmentHelper visualStudioHelper, IssueGridViewModel model)
        {
            this.model = model;
            this.rest = rest;
            this.visualStudioHelper = visualStudioHelper;
            this.config = config;
            this.AssociatedCommand = new RelayCommand(this.OnAssociateCommand);
            this.SubItems = new ObservableCollection<IMenuItem>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the associated command.
        /// </summary>
        public ICommand AssociatedCommand { get; set; }

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
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="rest">
        /// The rest.
        /// </param>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="IMenuItem"/>.
        /// </returns>
        public static IMenuItem MakeMenu(ISonarConfiguration config, ISonarRestService rest, IVsEnvironmentHelper helper, IssueGridViewModel model)
        {
            var topLel = new OpenResourceMenu(config, rest, helper, model) { CommandText = "Open", IsEnabled = false };

            topLel.SubItems.Add(new OpenResourceMenu(config, rest, helper, model) { CommandText = "Visual Studio", IsEnabled = true });
            topLel.SubItems.Add(new OpenResourceMenu(config, rest, helper, model) { CommandText = "Browser", IsEnabled = true });
            return topLel;
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

                        var resources = this.rest.GetResourcesData(this.config, issue.Component);
                        this.visualStudioHelper.NavigateToResource(this.config.Hostname + "/resource/index/" + resources[0].Id);
                    }
                }

                if (this.CommandText.Equals("Visual Studio"))
                {
                    this.model.OnOpenInVsCommand(this.model.SelectedItems);
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