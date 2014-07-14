namespace VSSonarPlugins
{
    using System.Collections.ObjectModel;

    public interface IMenuPluginLoader : IPluginLoader
    {
        /// <summary>
        /// The load plugins from folder.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <returns>
        /// The <see cref="ReadOnlyCollection{T}"/>.
        /// </returns>
        ReadOnlyCollection<IMenuCommandPlugin> LoadPluginsFromFolder(string folder);
    }
}