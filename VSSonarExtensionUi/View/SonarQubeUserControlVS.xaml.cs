// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarQubeUserControlVS.xaml.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtensionUi.View
{
    using System;
    using System.Diagnostics;

    using VSSonarExtensionUi.ViewModel;

    /// <summary>
    ///     Interaction logic for SonarQubeWindow.xaml
    /// </summary>
    public partial class SonarQubeUserControlVs
    {
        #region Fields

        /// <summary>
        ///     The data model.
        /// </summary>
        private SonarQubeViewModel dataModel;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeUserControlVs"/> class.
        /// </summary>
        public SonarQubeUserControlVs()
        {
            try
            {
                this.InitializeComponent();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeUserControlVs"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public SonarQubeUserControlVs(SonarQubeViewModel model)
        {
            this.DataContext = model;
            try
            {
                this.InitializeComponent();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The update data context.
        /// </summary>
        /// <param name="dataModelIn">
        /// The data model in.
        /// </param>
        public void UpdateDataContext(SonarQubeViewModel dataModelIn)
        {
            // bind data with view model
            this.dataModel = dataModelIn;
            this.DataContext = null;
            this.DataContext = dataModelIn;
        }

        #endregion
    }
}