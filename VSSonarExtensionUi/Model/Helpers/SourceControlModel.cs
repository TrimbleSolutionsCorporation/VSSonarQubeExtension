namespace VSSonarExtensionUi.Model.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using SonarRestService.Types;
    using SonarRestService;

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
        /// Gets the blame by line.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        BlameLine GetBlameByLine(Resource resource, int line);

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
        /// The manager
        /// </summary>
        private readonly INotificationManager manager;

        /// <summary>
        /// The service
        /// </summary>
        private readonly ISonarRestService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceControlModel" /> class.
        /// </summary>
        /// <param name="sourceControlPlugins">The source control plugins.</param>
        /// <param name="basePath">The base path.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="service">The service.</param>
        public SourceControlModel(IEnumerable<ISourceVersionPlugin> sourceControlPlugins, string basePath, INotificationManager manager, ISonarRestService service)
        {
            this.service = service;
            this.manager = manager;
            this.basePath = basePath;
            this.plugins = sourceControlPlugins;

            if (string.IsNullOrEmpty(basePath))
            {
                this.supportedPlugin = null;
                return;
            }

            foreach (var plugin in sourceControlPlugins)
            {

                try
                {
                    plugin.InitializeRepository(basePath); 
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                } 
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
            else
            {
                if (this.manager != null)
                {
                    this.manager.ReportMessage(new Message { Id = "IssueTrackerMenu", Data = "Project not supported by any source control plugin" });
                }                
            }

            return null;
        }

        public BlameLine GetBlameByLine(Resource resource, int line)
        {
            return this.service.GetBlameLine(AuthtenticationHelper.AuthToken, resource.Key, line);
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
