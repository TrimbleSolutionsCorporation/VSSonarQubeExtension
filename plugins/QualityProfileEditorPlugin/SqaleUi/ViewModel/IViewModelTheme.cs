// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewModelTheme.cs" company="">
//   
// </copyright>
// <summary>
//   The ViewModelTheme interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SqaleUi.ViewModel
{
    using System.Windows.Media;

    /// <summary>
    /// The ViewModelTheme interface.
    /// </summary>
    public interface IViewModelTheme
    {
        /// <summary>
        /// The update colours.
        /// </summary>
        /// <param name="background">
        /// The background.
        /// </param>
        /// <param name="foreground">
        /// The foreground.
        /// </param>
        void UpdateColors(Color background, Color foreground);
    }
}