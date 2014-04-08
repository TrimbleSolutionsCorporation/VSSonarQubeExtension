// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginCommandsModel.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarExtension.MainViewModel.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    using VSSonarExtension.MainViewModel.Commands;

    using VSSonarPlugins;

    using MenuItem = System.Windows.Controls.MenuItem;

    /// <summary>
    ///     The issues list.
    /// </summary>
    public partial class ExtensionDataModel
    {
        /// <summary>
        /// The plugins.
        /// </summary>
        private readonly Dictionary<int, IMenuCommandPlugin> menuItemPlugins = new Dictionary<int, IMenuCommandPlugin>();

        /// <summary>
        /// Gets or sets the view switch command.
        /// </summary>
        public ViewSwitchCommand ToolSwitchCommand { get; set; }

        /// <summary>
        /// Gets the tools provided by plugins.
        /// </summary>
        public IEnumerable<MenuItem> ToolsProvidedByPlugins
        {
            get
            {
                var menuItemsList = new List<MenuItem>();
                if (this.PluginControl == null)
                {
                    return menuItemsList;
                }

                var plugins = this.PluginControl.GetMenuItemPlugins();

                if (plugins == null)
                {
                    return menuItemsList;
                }

                foreach (var plugin in plugins)
                {
                    menuItemsList.Add(new MenuItem { Header = plugin.GetHeader() });
                    this.menuItemPlugins.Add(this.menuItemPlugins.Count, plugin);
                }

                return menuItemsList;
            }
        }

        /// <summary>
        /// The execute plugin.
        /// </summary>
        /// <param name="header">
        /// The data.
        /// </param>
        public void ExecutePlugin(string header)
        {
            foreach (var plugin in this.menuItemPlugins)
            {
                if (plugin.Value.GetHeader().Equals(header))
                {
                    var isEnabled =
                        this.vsenvironmenthelper.ReadOptionFromApplicationData(
                            GlobalIds.PluginEnabledControlId,
                            plugin.Value.GetPluginDescription(this.vsenvironmenthelper).Name);

                    if (string.IsNullOrEmpty(isEnabled)
                        || isEnabled.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.VSPackage.ShowToolWindow(
                            plugin.Value.GetUserControl(
                                this.UserConfiguration,
                                this.AssociatedProject,
                                this.vsenvironmenthelper),
                            plugin.Key,
                            plugin.Value.GetHeader());
                    }
                    else
                    {
                        MessageBox.Show(
                            plugin.Value.GetPluginDescription(this.vsenvironmenthelper).Name + " is disabled");
                    }
                }
            }
        }
    }
}