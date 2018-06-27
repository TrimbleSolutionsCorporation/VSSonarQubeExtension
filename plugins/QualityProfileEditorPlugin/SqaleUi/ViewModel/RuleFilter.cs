// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleFilter.cs" company="Copyright © 2014 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
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

namespace SqaleUi.ViewModel
{
    using System;
    using System.Linq;

    using VSSonarPlugins.Types;

    using SqaleUi.helpers;

    /// <summary>
    /// The rule filter.
    /// </summary>
    public class RuleFilter : IFilter
    {
        #region Fields

        /// <summary>
        /// The filter option.
        /// </summary>
        private readonly IFilterOption filterOption;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleFilter"/> class.
        /// </summary>
        /// <param name="filterOption">
        /// The filter option.
        /// </param>
        public RuleFilter(IFilterOption filterOption)
        {
            this.filterOption = filterOption;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The filter function.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool FilterFunction(object parameter)
        {
            var isTagPresent = this.IsTagPresent((Rule)parameter);
            var isRuleEnabled = this.IsRuleEnabled((Rule)parameter);

            var include = ((Rule)parameter).ConfigKey.IndexOf(this.filterOption.FilterTermConfigKey, StringComparison.InvariantCultureIgnoreCase)
                           >= 0
                           && ((Rule)parameter).HtmlDescription.IndexOf(
                               this.filterOption.FilterTermDescription, 
                               StringComparison.InvariantCultureIgnoreCase) >= 0
                           && ((Rule)parameter).Key.IndexOf(this.filterOption.FilterTermKey, StringComparison.InvariantCultureIgnoreCase) >= 0
                           && ((Rule)parameter).Name.IndexOf(this.filterOption.FilterTermName, StringComparison.InvariantCultureIgnoreCase) >= 0
                           && ((Rule)parameter).Repo.IndexOf(this.filterOption.FilterTermRepo, StringComparison.InvariantCultureIgnoreCase) >= 0
                           && (this.filterOption.FilterTermCategory == null || ((Rule)parameter).Category.Equals(this.filterOption.FilterTermCategory))
                           && (this.filterOption.FilterTermSubCategory == null
                               || ((Rule)parameter).Subcategory.Equals(this.filterOption.FilterTermSubCategory))
                           && (this.filterOption.FilterTermRemediationFunction == null
                               || ((Rule)parameter).RemediationFunction.Equals(this.filterOption.FilterTermRemediationFunction))
                           && (this.filterOption.FilterTermSeverity == null || ((Rule)parameter).Severity.Equals(this.filterOption.FilterTermSeverity));

            return include && isTagPresent && isRuleEnabled;
        }

        /// <summary>
        /// The is enabled.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsEnabled()
        {
            return !string.IsNullOrEmpty(this.filterOption.FilterTermConfigKey) || !string.IsNullOrEmpty(this.filterOption.FilterTermDescription)
                   || !string.IsNullOrEmpty(this.filterOption.FilterTermKey) || !string.IsNullOrEmpty(this.filterOption.FilterTermName)
                   || !string.IsNullOrEmpty(this.filterOption.FilterTermRepo) || !string.IsNullOrEmpty(this.filterOption.FilterTermTag)
                   || this.filterOption.FilterTermCategory != null || this.filterOption.FilterTermSubCategory != null
                   || this.filterOption.FilterTermRemediationFunction != null || this.filterOption.FilterTermSeverity != null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The is rule enabled.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IsRuleEnabled(Rule parameter)
        {
            if (this.filterOption.FilterTermEnabled == null)
            {
                return true;
            }

            if (parameter.Enabled && this.filterOption.FilterTermEnabled.Contains("Enabled"))
            {
                return true;
            }

            if (!parameter.Enabled && this.filterOption.FilterTermEnabled.Contains("Disabled"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The is tag present.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IsTagPresent(Rule parameter)
        {
            if (string.IsNullOrEmpty(this.filterOption.FilterTermTag))
            {
                return true;
            }

            return parameter.Tags.Any(tag => tag.IndexOf(this.filterOption.FilterTermTag, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        #endregion
    }
}