// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewModelBase.cs" company="">
//   
// </copyright>
// <summary>
//   The ViewModelBase interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.ViewModel.Helpers
{
    using System.Windows.Media;

    /// <summary>
    ///     The ViewModelBase interface.
    /// </summary>
    public interface IViewModelBase
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the back ground color.
        /// </summary>
        Color BackGroundColor { get; set; }

        /// <summary>
        /// Gets or sets the fore ground color.
        /// </summary>
        Color ForeGroundColor { get; set; }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        string Header { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The update colours.
        /// </summary>
        /// <param name="background">
        /// The background.
        /// </param>
        /// <param name="foreground">
        /// The foreground.
        /// </param>
        void UpdateColours(Color background, Color foreground);

        #endregion
    }
}