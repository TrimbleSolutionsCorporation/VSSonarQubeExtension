namespace VSSonarExtensionUi.Model.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// Source control interface
    /// </summary>
    public interface ISourceControlProvider
    {
        /// <summary>
        /// Gets the blame by line.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="line">The line.</param>
        /// <returns>Returns Blame by line</returns>
        BlameLine GetBlameByLine(string filePath, int line);

        /// <summary>
        /// Gets the branch.
        /// </summary>
        /// <returns>
        /// Returns current branch.
        /// </returns>
        string GetBranch();
    }

    /// <summary>
    /// source control model
    /// </summary>
    internal class SourceControlModel : ISourceControlProvider
    {
        /// <summary>
        /// The plugins
        /// </summary>
        private readonly IEnumerable<ISourceVersionPlugin> plugins;

        /// <summary>
        /// The base path
        /// </summary>
        private readonly string basePath;

        /// <summary>
        /// The supported plugin
        /// </summary>
        private readonly ISourceVersionPlugin supportedPlugin;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceControlModel" /> class.
        /// </summary>
        /// <param name="sourceControlPlugins">The source control plugins.</param>
        /// <param name="basePath">The base path.</param>
        public SourceControlModel(IEnumerable<ISourceVersionPlugin> sourceControlPlugins, string basePath)
        {
            this.basePath = basePath;
            this.plugins = sourceControlPlugins;

            if (string.IsNullOrEmpty(basePath))
            {
                this.supportedPlugin = null;
                return;
            }

            foreach (var plugin in sourceControlPlugins)
            {
                plugin.InitializeRepository(basePath); 
            }

            this.supportedPlugin = this.GetSupportedPlugin();
        }

        /// <summary>
        /// Gets the blame by line.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="line">The line.</param>
        /// <returns>Return blame by line</returns>
        public BlameLine GetBlameByLine(string filePath, int line)
        {
            if (this.supportedPlugin != null)
            {
                return this.supportedPlugin.GetBlameByLine(filePath, line);
            }

            return null;
        }

        /// <summary>
        /// Gets the branch.
        /// </summary>
        /// <returns>
        /// Returns current branch.
        /// </returns>
        public string GetBranch()
        {
            if (this.supportedPlugin == null)
            {
                return string.Empty;
            }

            return this.supportedPlugin.GetBranch();
        }

        /// <summary>
        /// Gets the supported plugin.
        /// </summary>
        /// <returns>supported plugin or null</returns>
        private ISourceVersionPlugin GetSupportedPlugin()
        {
            return this.plugins.SingleOrDefault(c => c.IsSupported());
        }
    }
}
