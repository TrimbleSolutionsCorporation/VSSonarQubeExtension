namespace VSSonarExtensionUi.Model.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// Model base, register model in SonarQubeViewModel for automatic update of association
    /// </summary>
    public interface IModelBase
    {
        /// <summary>
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vs environment helper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        void UpdateServices(
            IVsEnvironmentHelper vsenvironmenthelperIn,
            IVSSStatusBar statusBar,
            IServiceProvider provider);

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="sourceModel">The source model.</param>
        /// <param name="profile">The quality profile.</param>
        void AssociateWithNewProject(
            Resource project,
            string workingDir,
            ISourceControlProvider sourceModel,
            Dictionary<string, Profile> profile);

        /// <summary>
        /// The end data association.
        /// </summary>
        void OnSolutionClosed();

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        void OnDisconnect();

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="availableProjects">The available projects.</param>
        void OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IIssueTrackerPlugin sourcePlugin);

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>returns view model</returns>
        object GetViewModel();
    }
}