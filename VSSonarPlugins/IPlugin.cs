// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPlugin.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarPlugins
{
    using SonarRestService.Types;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Types;

    /// <summary>
    ///     The Plugin interface. INotificationManager and IConfigurationHelper are injected during instantioation
    /// </summary>
    public interface IPlugin : IDisposable
    {
        /// <summary>The get use plugin control options.</summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The <see cref="UserControl"/>.</returns>
        IPluginControlOption GetPluginControlOptions(Resource project, ISonarConfiguration configuration);

        /// <summary>
        ///     The get plugin descritpion.
        /// </summary>
        /// <param name="vsinter">
        ///     The vsinter.
        /// </param>
        /// <returns>
        ///     The <see cref="PluginDescription" />.
        /// </returns>
        PluginDescription GetPluginDescription();

        /// <summary>
        ///     The reset defaults.
        /// </summary>
        void ResetDefaults();

        /// <summary>
        /// The associate project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="profile">The profile.</param>
        Task<bool> AssociateProject(
            Resource project,
            ISonarConfiguration configuration,
            Dictionary<string, Profile> profile,
            string visualStudioVersion);

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        Task<bool> OnConnectToSonar(ISonarConfiguration configuration);

        /// <summary>
        /// DLLs the locations.
        /// </summary>
        /// <returns></returns>
        IList<string> DllLocations();

        /// <summary>
        /// Sets the DLL location.
        /// </summary>
        /// <param name="path">The path.</param>
        void SetDllLocation(string path);
    }
}