// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFilterCommand.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarExtensionUi.Helpers
{
    using System.Windows.Input;

    /// <summary>
    ///     The FilterOption interface.
    /// </summary>
    public interface IFilterCommand
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the clear clear filter term rule command.
        /// </summary>
        ICommand ClearClearFilterTermRuleCommand { get; set; }

        /// <summary>
        /// Gets or sets the clear filter term assignee command.
        /// </summary>
        ICommand ClearFilterTermAssigneeCommand { get; set; }

        /// <summary>
        /// Gets or sets the clear filter term component command.
        /// </summary>
        ICommand ClearFilterTermComponentCommand { get; set; }

        /// <summary>
        /// Gets or sets the clear filter term is new command.
        /// </summary>
        ICommand ClearFilterTermIsNewCommand { get; set; }

        /// <summary>
        /// Gets or sets the clear filter term message command.
        /// </summary>
        ICommand ClearFilterTermMessageCommand { get; set; }

        /// <summary>
        /// Gets or sets the clear filter term project command.
        /// </summary>
        ICommand ClearFilterTermProjectCommand { get; set; }

        /// <summary>
        /// Gets or sets the clear filter term resolution command.
        /// </summary>
        ICommand ClearFilterTermResolutionCommand { get; set; }

        /// <summary>
        /// Gets or sets the clear filter term severity command.
        /// </summary>
        ICommand ClearFilterTermSeverityCommand { get; set; }

        /// <summary>
        /// Gets or sets the clear filter term status command.
        /// </summary>
        ICommand ClearFilterTermStatusCommand { get; set; }

        /// <summary>
        /// Gets or sets the filter apply command.
        /// </summary>
        ICommand FilterApplyCommand { get; set; }

        /// <summary>
        /// Gets or sets the filter clear all command.
        /// </summary>
        ICommand FilterClearAllCommand { get; set; }

        #endregion
    }
}