// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFilterCommand.cs" company="">
//   
// </copyright>
// <summary>
//   The FilterOption interface.
// </summary>
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