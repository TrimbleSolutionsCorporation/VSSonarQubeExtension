// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAnalysisPluginLoader.cs" company="">
//   
// </copyright>
// <summary>
//   The AnalysisPluginLoader interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarPlugins
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// The AnalysisPluginLoader interface.
    /// </summary>
    public interface IPluginLoader
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
        ReadOnlyCollection<IPlugin> LoadPluginsFromFolder(string folder);

        /// <summary>
        /// The get error data.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetErrorData();
    }
}