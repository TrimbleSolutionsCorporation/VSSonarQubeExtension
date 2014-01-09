// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssuesToolWindow.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.VSControls
{
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using ExtensionTypes;

    using ExtensionView;

    using ExtensionViewModel.ViewModel;

    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// The issues tool window.
    /// </summary>
    public sealed class PluginToolWindow : ToolWindowPane
    {
        /// <summary>
        /// The window to use.
        /// </summary>
        private readonly UserControl windowToUse;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuesToolWindow"/> class.
        /// </summary>
        public PluginToolWindow() : base(null)
        {
            this.Caption = "Plugin Window";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;
            this.windowToUse = new UserControl();
            this.Content = this.windowToUse;
        }

        /// <summary>
        /// The update model.
        /// </summary>
        /// <param name="myModel">
        /// The my model.
        /// </param>
        /// <param name="ID">
        /// The ID.
        /// </param>
        public void UpdateModel(UserControl myModel, string ID)
        {
            this.Caption = ID;
            this.Content = null;
            this.Content = myModel;
        }
    }
}

