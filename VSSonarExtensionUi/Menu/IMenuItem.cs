// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMenuItem.cs" company="">
//   
// </copyright>
// <summary>
//   The MenuItem interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.Menu
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    /// <summary>
    ///     The MenuItem interface.
    /// </summary>
    public interface IMenuItem
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the associated command.
        /// </summary>
        ICommand AssociatedCommand { get; set; }

        /// <summary>
        ///     Gets or sets the command text.
        /// </summary>
        string CommandText { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the sub items.
        /// </summary>
        ObservableCollection<IMenuItem> SubItems { get; set; }

        #endregion
    }
}