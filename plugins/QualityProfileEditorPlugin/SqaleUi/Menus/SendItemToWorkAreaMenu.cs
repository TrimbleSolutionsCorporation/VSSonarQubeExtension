// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SendItemToWorkAreaMenu.cs" company="Copyright © 2014 jmecsoftware">
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

namespace SqaleUi.Menus
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    using SqaleUi.ViewModel;

    /// <summary>
    /// The send item to work area menu.
    /// </summary>
    public class SendItemToWorkAreaMenu : IMenuItem
    {
        #region Fields

        /// <summary>
        /// The sqale grid vm.
        /// </summary>
        private readonly ISqaleGridVm sqaleGridVm;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SendItemToWorkAreaMenu"/> class.
        /// </summary>
        /// <param name="sqaleGridVm">
        /// The sqale grid vm.
        /// </param>
        private SendItemToWorkAreaMenu(ISqaleGridVm sqaleGridVm)
        {
            this.sqaleGridVm = sqaleGridVm;

            this.AssociatedCommand = new RelayCommand<object>(this.OnAssociatedCommand);
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
        /// <param name="sqaleGridVm">
        /// The sqale grid vm.
        /// </param>
        /// <param name="mainModel">
        /// The main model.
        /// </param>
        /// <returns>
        /// The <see cref="SendItemToWorkAreaMenu"/>.
        /// </returns>
        public static SendItemToWorkAreaMenu MakeMenu(SqaleGridVm sqaleGridVm, SqaleEditorControlViewModel mainModel)
        {
            var menu = new SendItemToWorkAreaMenu(sqaleGridVm) { CommandText = "Send selected items to work area", IsEnabled = false };
            menu.SubItems.Add(new SendItemToWorkAreaMenu(sqaleGridVm) { CommandText = "New Work Area", IsEnabled = false });

            foreach (SqaleGridVm tab in mainModel.Tabs)
            {
                if (!tab.Header.Equals("Project"))
                {
                    menu.SubItems.Add(new SendItemToWorkAreaMenu(sqaleGridVm) { CommandText = tab.Header, IsEnabled = false });
                }
            }

            return menu;
        }

        /// <summary>
        /// The refresh menu items.
        /// </summary>
        /// <param name="menus">
        /// The menus.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="gridModel">
        /// The grid model.
        /// </param>
        /// <param name="isenabled">
        /// The isenabled.
        /// </param>
        public static void RefreshMenuItems(
            ObservableCollection<IMenuItem> menus, 
            SqaleEditorControlViewModel model, 
            ISqaleGridVm gridModel, 
            bool isenabled)
        {
            var listOfFilesToRemove = new List<IMenuItem>();
            foreach (IMenuItem item in menus)
            {
                if (item is SendItemToWorkAreaMenu)
                {
                    foreach (IMenuItem menuItem in item.SubItems)
                    {
                        if (menuItem.CommandText.Equals("New Work Area"))
                        {
                            continue;
                        }

                        bool found = false;
                        foreach (SqaleGridVm tab in model.Tabs)
                        {
                            if (tab.Header.Equals(menuItem.CommandText))
                            {
                                found = true;
                            }
                        }

                        if (!found)
                        {
                            listOfFilesToRemove.Add(menuItem);
                        }
                    }

                    for (int i = 0; i < listOfFilesToRemove.Count; i++)
                    {
                        item.SubItems.Remove(listOfFilesToRemove[i]);
                    }
                }
            }

            foreach (SqaleGridVm tab in model.Tabs)
            {
                if (tab.Header.Equals("Project"))
                {
                    continue;
                }

                foreach (IMenuItem item in menus)
                {
                    if (item is SendItemToWorkAreaMenu)
                    {
                        bool found = false;

                        foreach (IMenuItem menuItem in item.SubItems)
                        {
                            if (tab.Header.Equals(menuItem.CommandText))
                            {
                                found = true;
                            }
                        }

                        if (!found)
                        {
                            item.SubItems.Add(new SendItemToWorkAreaMenu(gridModel) { CommandText = tab.Header, IsEnabled = isenabled });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The refresh menu items status.
        /// </summary>
        /// <param name="menus">
        /// The menus.
        /// </param>
        /// <param name="enableCommands">
        /// The enable commands.
        /// </param>
        public static void RefreshMenuItemsStatus(ObservableCollection<IMenuItem> menus, bool enableCommands)
        {
            foreach (IMenuItem item in menus)
            {
                if (item is SendItemToWorkAreaMenu)
                {
                    foreach (IMenuItem menu in item.SubItems)
                    {
                        menu.IsEnabled = enableCommands;
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on associated command.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnAssociatedCommand(object obj)
        {
            if (this.IsEnabled)
            {
                this.sqaleGridVm.SendSelectedItemsToWorkArea(this.CommandText);
            }
        }

        #endregion
    }
}