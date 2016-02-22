namespace VSSonarExtensionUi.Model.Helpers
{
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// source control model
    /// </summary>
    internal class RestSourceControlModel : ISourceControlProvider
    {
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
        public RestSourceControlModel(INotificationManager manager, ISonarRestService service)
        {
            this.service = service;
            this.manager = manager;
        }

        public BlameLine GetBlameByLine(string filePath, int line)
        {
            return null;
        }

        public BlameLine GetBlameByLine(Resource resource, int line)
        {
            return this.service.GetBlameLine(AuthtenticationHelper.AuthToken, resource.Key, line);
        }

        public string GetBranch()
        {
            return string.Empty;
        }
    }
}
