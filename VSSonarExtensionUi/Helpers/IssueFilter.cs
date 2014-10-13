// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssueFilter.cs" company="">
//   
// </copyright>
// <summary>
//   The rule filter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.Helpers
{
    using System;

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