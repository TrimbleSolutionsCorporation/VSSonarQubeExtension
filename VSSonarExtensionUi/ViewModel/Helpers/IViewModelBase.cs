using System.Windows.Media;

namespace VSSonarExtensionUi.ViewModel.Helpers
{

    public interface IViewModelBase
    {
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
    }
}