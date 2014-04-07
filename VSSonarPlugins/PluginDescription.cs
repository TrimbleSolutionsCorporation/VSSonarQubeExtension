// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginDescription.cs" company="">
//   
// </copyright>
// <summary>
//   The list of plugins.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarPlugins
{
    /// <summary>
    /// The list of plugins.
    /// </summary>
    public class PluginDescription
    {
        public PluginDescription()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether disable.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the supported extensions.
        /// </summary>
        public string SupportedExtensions { get; set; }

        /// <summary>
        /// Gets or sets the supported extensions.
        /// </summary>
        public string Description { get; set; }
    }
}