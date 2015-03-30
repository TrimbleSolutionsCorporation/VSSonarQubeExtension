namespace VSSonarPlugins
{
    using System.Windows.Controls;
    using System.Windows.Media;

    using VSSonarPlugins.Types;

    /// <summary>
    ///     The PluginControlOption interface.
    /// </summary>
    public interface IPluginControlOption
    {
        /// <summary>The get option control user interface.</summary>
        /// <returns>The <see cref="UserControl"/>.</returns>
        UserControl GetOptionControlUserInterface();

        /// <summary>The refresh data in ui.</summary>
        /// <param name="project">The project.</param>
        /// <param name="helper">The helper.</param>
        void RefreshDataInUi(Resource project, IConfigurationHelper helper);

        /// <summary>The save data in ui.</summary>
        /// <param name="project">The project.</param>
        /// <param name="helper">The helper.</param>
        void SaveDataInUi(Resource project, IConfigurationHelper helper);

        /// <summary>The refresh colours.</summary>
        /// <param name="foreground">The foreground.</param>
        /// <param name="background">The background.</param>
        void RefreshColours(Color foreground, Color background);
    }
}