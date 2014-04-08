// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMenuCommandPlugin.cs" company="">
//   
// </copyright>
// <summary>
//   The MenuCommandPlugin interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarPlugins
{
    using System.Windows.Controls;

    using ExtensionTypes;

    /// <summary>
    /// The MenuCommandPlugin interface.
    /// </summary>
    public interface IMenuCommandPlugin
    {
        /// <summary>
        /// The get header.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetHeader();

        /// <summary>
        /// The get user control.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="vshelper">
        /// The vs Helper.
        /// </param>
        /// <returns>
        /// The <see cref="UserControl"/>.
        /// </returns>
        UserControl GetUserControl(ConnectionConfiguration configuration, Resource project, IVsEnvironmentHelper vshelper);

        /// <summary>
        /// The update configuration.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="vshelper">
        /// The vshelper.
        /// </param>
        void UpdateConfiguration(ConnectionConfiguration configuration, Resource project, IVsEnvironmentHelper vshelper);

        /// <summary>
        /// The get plugin descritpion.
        /// </summary>
        /// <returns>
        /// The <see cref="PluginDescription"/>.
        /// </returns>
        PluginDescription GetPluginDescription(IVsEnvironmentHelper vsinter);
    }
}