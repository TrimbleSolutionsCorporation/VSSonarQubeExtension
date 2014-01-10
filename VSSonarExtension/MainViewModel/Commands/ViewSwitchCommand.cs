// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssignOnIssueCommand.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.MainViewModel.Commands
{
    using System;
    using System.Windows.Input;

    using VSSonarExtension.MainViewModel.ViewModel;

    /// <summary>
    /// The view options command.
    /// </summary>
    public class ViewSwitchCommand : ICommand
    {
        /// <summary>
        /// The issues data model.
        /// </summary>
        private readonly ExtensionDataModel model;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewSwitchCommand"/> class.
        /// </summary>
        public ViewSwitchCommand()
        {
            var handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignOnIssueCommand"/> class. 
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="service">
        /// The service.
        /// </param>
        public ViewSwitchCommand(ExtensionDataModel model)
        {
            this.model = model;
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
            var data = parameter as string;
            if (!string.IsNullOrEmpty(data))
            {
                this.model.ExecutePlugin(data);
            }
        }
    }
}
