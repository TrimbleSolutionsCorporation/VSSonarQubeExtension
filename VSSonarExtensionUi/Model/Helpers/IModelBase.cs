namespace VSSonarExtensionUi.Model.Helpers
{
    using System;

    using SonarLocalAnalyser;
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
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        void AssociateWithNewProject(ISonarConfiguration config, Resource project, string workingDir);

        /// <summary>
        /// The end data association.
        /// </summary>
        void EndDataAssociation();

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>returns view model</returns>
        object GetViewModel();
    }
}