// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFilterOption.cs" company="">
//   
// </copyright>
// <summary>
//   The FilterOption interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.Helpers
{
    using ExtensionTypes;

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