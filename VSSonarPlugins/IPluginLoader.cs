// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPluginLoader.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IPluginLoader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarPlugins
{
    /// <summary>
    /// The PluginLoader interface.
    /// </summary>
    public interface IPluginLoader
    {
        /// <summary>
        /// The get error data.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetErrorData();
    }
}