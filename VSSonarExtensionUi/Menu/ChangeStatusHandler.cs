// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeStatusHandler.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

    /// <summary>
    ///     The issue handler menu.
    /// </summary>
    internal class ChangeStatusHandler : IMenuItem
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

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStatusHandler"/> class.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="rest">
        /// The rest.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        private ChangeStatusHandler(ISonarConfiguration config, ISonarRestService rest, IssueGridViewModel model)
        {
            this.model = model;
            this.rest = rest;
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
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="IMenuItem"/>.
        /// </returns>
        public static IMenuItem MakeMenu(ISonarConfiguration config, ISonarRestService rest, IssueGridViewModel model)
        {
            var topLel = new ChangeStatusHandler(config, rest, model) { CommandText = "Status", IsEnabled = false };

            topLel.SubItems.Add(new ChangeStatusHandler(config, rest, model) { CommandText = "Confirm", IsEnabled = true });
            topLel.SubItems.Add(new ChangeStatusHandler(config, rest, model) { CommandText = "Fix", IsEnabled = true });
            topLel.SubItems.Add(new ChangeStatusHandler(config, rest, model) { CommandText = "False Positive", IsEnabled = true });
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
                if (this.CommandText.Equals("False Positive"))
                {
                    this.rest.MarkIssuesAsFalsePositive(this.config, this.model.SelectedItems, string.Empty);
                    this.model.RefreshView();
                }

                if (this.CommandText.Equals("Confirm"))
                {
                    this.rest.ConfirmIssues(this.config, this.model.SelectedItems, string.Empty);
                    this.model.RefreshView();
                }

                if (this.CommandText.Equals("Fix"))
                {
                    this.rest.ResolveIssues(this.config, this.model.SelectedItems, string.Empty);
                    this.model.RefreshView();
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