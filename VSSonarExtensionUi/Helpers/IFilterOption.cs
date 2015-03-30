// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFilterOption.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

using VSSonarPlugins.Types;
namespace VSSonarExtensionUi.Helpers
{
    

    /// <summary>
    /// The FilterOption interface.
    /// </summary>
    public interface IFilterOption
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the filter term assignee.
        /// </summary>
        string FilterTermAssignee { get; set; }

        /// <summary>
        /// Gets or sets the filter term component.
        /// </summary>
        string FilterTermComponent { get; set; }

        /// <summary>
        /// Gets or sets the filter term is new.
        /// </summary>
        string FilterTermIsNew { get; set; }

        /// <summary>
        /// Gets or sets the filter term message.
        /// </summary>
        string FilterTermMessage { get; set; }

        /// <summary>
        /// Gets or sets the filter term project.
        /// </summary>
        string FilterTermProject { get; set; }

        /// <summary>
        /// Gets or sets the filter term resolution.
        /// </summary>
        Resolution? FilterTermResolution { get; set; }

        /// <summary>
        /// Gets or sets the filter term rule.
        /// </summary>
        string FilterTermRule { get; set; }

        /// <summary>
        /// Gets or sets the filter term severity.
        /// </summary>
        Severity? FilterTermSeverity { get; set; }

        /// <summary>
        /// Gets or sets the filter term status.
        /// </summary>
        IssueStatus? FilterTermStatus { get; set; }


        #endregion
    }
}