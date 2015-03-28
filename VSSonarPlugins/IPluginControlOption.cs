namespace VSSonarPlugins
{
    using System.Windows.Controls;

    using ExtensionTypes;

    /// <summary>
    /// The PluginControlOption interface.
    /// </summary>
    public interface IPluginControlOption
    {
        UserControl GetOptionControlUserInterface();

        void RefreshDataInUi(Resource project, IConfigurationHelper helper);

        void SaveDataInUi(Resource project, IConfigurationHelper helper);
    }
}