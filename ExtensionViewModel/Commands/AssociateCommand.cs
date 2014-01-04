// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssociateCommand.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace ExtensionViewModel.Commands
{
    using System;
    using System.Windows.Forms;
    using System.Windows.Input;

    using ExtensionHelpers;

    using ExtensionViewModel.ViewModel;

    /// <summary>
    /// The view options command.
    /// </summary>
    public class AssociateCommand : ICommand
    {
        /// <summary>
        /// The issues data model.
        /// </summary>
        private readonly ExtensionDataModel model;

        /// <summary>
        ///     The vsenvironmenthelper.
        /// </summary>
        private readonly IVsEnvironmentHelper vsenvironmenthelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssociateCommand"/> class.
        /// </summary>
        public AssociateCommand()
        {
            var handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssociateCommand"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="vsenvironmenthelper">
        /// The vsenvironmenthelper.
        /// </param>
        public AssociateCommand(ExtensionDataModel model, IVsEnvironmentHelper vsenvironmenthelper)
        {
            this.model = model;
            this.vsenvironmenthelper = vsenvironmenthelper;
        }

        /// <summary>
        /// The can execute changed.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// The can execute.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        public void Execute(object parameter)
        {
            var solutionName = this.vsenvironmenthelper.ActiveSolutionName();
            var selectedProjectInFilter = this.model.SelectedProjectInFilter;
            if (selectedProjectInFilter == null || string.IsNullOrEmpty(solutionName))
            {
                return;
            }

            if (this.model.AssociatedProjectKey == null)
            {                  
                this.vsenvironmenthelper.WriteOptionInApplicationData(solutionName, "PROJECTKEY", selectedProjectInFilter.Key);
            }
            else
            {
                var dialogResult = MessageBox.Show("Do you want to save this option to environment, so it will reuse this association when Visual Studio Restart?", "Save Association to Disk", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(solutionName, "PROJECTKEY", selectedProjectInFilter.Key);
                }
            }

            this.model.AssociateProjectToSolution();
        }
    }
}
