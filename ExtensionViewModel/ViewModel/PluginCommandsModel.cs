// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionDataModel.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Windows.Controls;

    using ExtensionViewModel.Commands;

    /// <summary>
    ///     The issues list.
    /// </summary>
    public partial class ExtensionDataModel
    {
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
                var list = new List<MenuItem>();
                var itemcontrol = new MenuItem { Header = "Plugin X" };
                list.Add(itemcontrol);
                return list;
            }
        }

        public void ExecutePlugin(string data)
        {
        }
    }
}