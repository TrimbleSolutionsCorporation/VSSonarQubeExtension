// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneralOptionsControlView.xaml.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtensionUi.View.Configuration
{
    using VSSonarExtensionUi.ViewModel.Configuration;

    /// <summary>
    ///     Interaction logic for GeneralOptionsControl.xaml
    /// </summary>
    public partial class GeneralOptionsControlView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralOptionsControlView"/> class.
        /// </summary>
        public GeneralOptionsControlView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralOptionsControlView"/> class. 
        /// Initializes a new instance of the <see cref="GeneralOptionsControl"/> class.
        /// </summary>
        /// <param name="controller">
        /// The controller.
        /// </param>
        public GeneralOptionsControlView(AnalysisOptions controller)
        {
            this.InitializeComponent();
            this.DataContext = controller;
        }

        #endregion
    }
}