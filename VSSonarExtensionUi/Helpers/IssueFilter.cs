// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssueFilter.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtensionUi.Helpers
{
    using System;
    using System.Diagnostics;

    using ExtensionTypes;

    /// <summary>
    ///     The rule filter.
    /// </summary>
    public class IssueFilter : IFilter
    {
        #region Fields

        /// <summary>
        ///     The filter option.
        /// </summary>
        private readonly IFilterOption filterOption;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueFilter"/> class. 
        /// Initializes a new instance of the <see cref="RuleFilter"/> class.
        /// </summary>
        /// <param name="filterOption">
        /// The filter option.
        /// </param>
        public IssueFilter(IFilterOption filterOption)
        {
            this.filterOption = filterOption;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The filter function.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool FilterFunction(object value)
        {
            try
            {
                var parameter = value as Issue;
                if (parameter == null)
                {
                    return true;
                }

                bool issuesStatus = this.FilterByIssueStatus(parameter);
                bool issuesSeverity = this.FilterBySeverity(parameter);
                bool issuesResolution = this.FilterByResolution(parameter);
                bool issuesIsNew = this.FilterByIsNew(parameter);

                bool include = (parameter.Message == null || parameter.Message.IndexOf(this.filterOption.FilterTermMessage, StringComparison.InvariantCultureIgnoreCase) >= 0)
                               && (parameter.Component == null || parameter.Component.IndexOf(this.filterOption.FilterTermComponent, StringComparison.InvariantCultureIgnoreCase) >= 0)
                               && (parameter.Project == null || parameter.Project.IndexOf(this.filterOption.FilterTermProject, StringComparison.InvariantCultureIgnoreCase) >= 0)
                               && (parameter.Rule == null || parameter.Rule.IndexOf(this.filterOption.FilterTermRule, StringComparison.InvariantCultureIgnoreCase) >= 0)
                               && (parameter.Assignee == null || parameter.Assignee.IndexOf(this.filterOption.FilterTermAssignee, StringComparison.InvariantCultureIgnoreCase) >= 0);

                return include && issuesStatus && issuesSeverity && issuesResolution && issuesIsNew;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The filter by is new.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool FilterByIsNew(Issue parameter)
        {
            if (this.filterOption.FilterTermIsNew == null)
            {
                return true;
            }

            if (parameter.IsNew && this.filterOption.FilterTermIsNew.Contains("yes"))
            {
                return true;
            }

            if (!parameter.IsNew && this.filterOption.FilterTermIsNew.Contains("no"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The filter by issue status.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool FilterByIssueStatus(Issue parameter)
        {
            if (this.filterOption.FilterTermStatus == null)
            {
                return true;
            }

            if (this.filterOption.FilterTermStatus.Equals(parameter.Status))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The filter by resolution.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool FilterByResolution(Issue parameter)
        {
            if (this.filterOption.FilterTermResolution == null)
            {
                return true;
            }

            if (this.filterOption.FilterTermResolution.Equals(parameter.Resolution))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The filter by severity.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool FilterBySeverity(Issue parameter)
        {
            if (this.filterOption.FilterTermSeverity == null)
            {
                return true;
            }

            if (this.filterOption.FilterTermSeverity.Equals(parameter.Severity))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}