namespace VSSonarExtensionUi.Model.Helpers
{
    using System;
    using System.Collections.Generic;
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
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        void UpdateServices(
            IVsEnvironmentHelper vsenvironmenthelperIn,
            IVSSStatusBar statusBar,
            IServiceProvider provider);

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="configIn">The configuration in.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="sourceModel">The source model.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        void AssociateWithNewProject(
            Resource project,
            string workingDir,
            ISourceControlProvider sourceModel,
            IIssueTrackerPlugin sourcePlugin,
            IList<Resource> availableProjects);

        /// <summary>
        /// The end data association.
        /// </summary>
        void OnSolutionClosed();

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        void OnDisconnect();

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>returns view model</returns>
        object GetViewModel();
    }
}