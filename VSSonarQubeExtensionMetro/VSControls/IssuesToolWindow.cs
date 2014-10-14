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

namespace VSSonarQubeExtension.VSControls
{
    using System.Runtime.InteropServices;

    using Microsoft.VisualStudio.Shell;
    using VSSonarExtensionUi.ViewModel;
    using VSSonarExtensionUi.View;



    /// <summary>
    /// The issues tool window.
    /// </summary>
    [Guid("ac305e7a-a44b-4541-8ece-3c88d5425338")]
    public sealed class IssuesToolWindow : ToolWindowPane
    {
        /// <summary>
        /// The window to use.
        /// </summary>
        private readonly SonarQubeUserControl windowToUse;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuesToolWindow"/> class.
        /// </summary>
        public IssuesToolWindow() : base(null)
        {
            this.Caption = "Issues Window";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;
            this.windowToUse = new SonarQubeUserControl();
            this.Content = this.windowToUse;
        }

        /// <summary>
        /// The update model.
        /// </summary>
        /// <param name="myModel">
        /// The my model.
        /// </param>
        public void UpdateModel(SonarQubeViewModel myModel)
        {
            this.windowToUse.UpdateDataContext(myModel);
        }
    }
}
