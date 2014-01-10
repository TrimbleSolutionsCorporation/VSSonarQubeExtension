// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssueWindow.xaml.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.MainView
{
    using System.Windows.Controls;

    using VSSonarExtension.MainView.Commands;
    using VSSonarExtension.MainViewModel.ViewModel;

    /// <summary>
    /// Interaction logic for IssueWindow.xaml
    /// </summary>
    public partial class IssueWindow
    {
        /// <summary>
        /// The issues options filter.
        /// </summary>
        private ExtensionDataModel dataModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueWindow"/> class.
        /// </summary>
        /// <param name="dataModelIn">
        /// The data Model In.
        /// </param>
        public IssueWindow(ExtensionDataModel dataModelIn)
        {
            this.InitializeComponent();

            // bind data with view model
            this.dataModel = dataModelIn;
            this.DataContext = null;
            this.DataContext = dataModelIn;

            this.BindCommandsInWindow(dataModelIn);
        }

        /// <summary>
        /// The update data context.
        /// </summary>
        /// <param name="dataModelIn">
        /// The data model in.
        /// </param>
        public void UpdateDataContext(ExtensionDataModel dataModelIn)
        {
            // bind data with view model
            this.dataModel = dataModelIn;
            this.DataContext = null;
            this.DataContext = dataModelIn;

            this.BindCommandsInWindow(dataModelIn);
        }

        /// <summary>
        /// The issues data grid selection changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void IssuesDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var elems = this.IssuesDataGrid.SelectedItems;
            this.dataModel.SelectedIssuesInView = elems;
        }

        /// <summary>
        /// The bind commands in window.
        /// </summary>
        /// <param name="dataModelIn">
        /// The data model in.
        /// </param>
        private void BindCommandsInWindow(ExtensionDataModel dataModelIn)
        {
            // bind commands
            this.ConfPluginsOptions.DataContext = new ExtensionMenuCommand(dataModelIn);
            this.ConfIssuesWindow.DataContext = new ExtensionMenuCommand(dataModelIn);
        }
    }
}