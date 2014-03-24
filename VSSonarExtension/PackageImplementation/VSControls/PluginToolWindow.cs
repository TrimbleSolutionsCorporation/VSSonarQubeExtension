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
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Controls;

    using ExtensionTypes;

    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// The issues tool window.
    /// </summary>
    public sealed class PluginToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginToolWindow"/> class.
        /// </summary>
        public PluginToolWindow() : base(null)
        {
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;
            //if (PackageImplementation.VsSonarExtensionPackage.CurrentPluginName == null)
            //{
            //    throw new NotImplementedException("No Plugin Defined");
           // }

            //this.Caption = PackageImplementation.VsSonarExtensionPackage.CurrentPluginName;
            //this.Content = PackageImplementation.VsSonarExtensionPackage.CurrentPluginControl;
        }
    }
}
