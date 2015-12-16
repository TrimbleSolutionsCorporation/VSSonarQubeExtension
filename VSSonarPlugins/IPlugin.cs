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
    using System;
    using System.Collections.Generic;

    using Types;

    /// <summary>The message.</summary>
    public class Message
    {
        /// <summary>Gets or sets the id.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the data.</summary>
        public string Data { get; set; }
    }

    /// <summary>
    /// The NotificationManager interface.
    /// </summary>
    public interface INotificationManager
    {
        /// <summary>
        /// Gets a value indicating whether [analysis change lines].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [analysis change lines]; otherwise, <c>false</c>.
        /// </value>
        bool AnalysisChangeLines { get; }

        /// <summary>
        /// Gets the user defined editor. TODO to be removed from here
        /// </summary>
        /// <value>
        /// The user defined editor.
        /// </value>
        string UserDefinedEditor { get; }

        /// <summary>The report exception.</summary>
        /// <param name="ex">The ex.</param>
        void ReportException(Exception ex);

        /// <summary>The report message.</summary>
        /// <param name="messages">The messages.</param>
        void ReportMessage(Message messages);

        /// <summary>
        /// News the issues are ready to be updated in view
        /// </summary>
        void OnIssuesUpdated();

        /// <summary>
        /// Writes the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        void WriteException(Exception ex);

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        void WriteMessage(string msg);

        /// <summary>
        /// Starteds the working.
        /// </summary>
        /// <param name="busyMessage">The busy message.</param>
        void StartedWorking(string busyMessage);

        /// <summary>
        /// Endeds the working.
        /// </summary>
        void EndedWorking();

        /// <summary>
        /// Removes the menu plugin. TODO, needs to be placed somewhere else
        /// </summary>
        /// <param name="menuPlugin">The menu plugin.</param>
        void RemoveMenuPlugin(IMenuCommandPlugin menuPlugin);

        /// <summary>
        /// Clears the cache.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Associates the project to solution. TODO to be removed
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        void AssociateProjectToSolution(string v1, string v2);

        /// <summary>
        /// Refreshes the data for resource.
        /// </summary>
        void RefreshDataForResource();

        /// <summary>
        /// Resets the failure.
        /// </summary>
        void ResetFailure();

        /// <summary>
        /// Flags the failure.
        /// </summary>
        /// <param name="v">The v.</param>
        void FlagFailure(string v);
    }

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

        /// <summary>The get licenses.</summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The <see cref="List"/>.</returns>
        Dictionary<string, VsLicense> GetLicenses(ISonarConfiguration configuration);

        /// <summary>The generate token id.</summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The <see cref="string"/>.</returns>
        string GenerateTokenId(ISonarConfiguration configuration);

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
        void AssociateProject(Resource project, ISonarConfiguration configuration, Dictionary<string, Profile> profile);

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