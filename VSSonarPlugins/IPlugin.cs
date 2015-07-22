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
    using System.Windows.Controls;

    using VSSonarPlugins.Types;

    /// <summary>The message.</summary>
    public class Message
    {
        /// <summary>Gets or sets the id.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the data.</summary>
        public string Data { get; set; }
    }

    /// <summary>The NotificationManager interface.</summary>
    public interface INotificationManager
    {
        /// <summary>The report exception.</summary>
        /// <param name="ex">The ex.</param>
        void ReportException(Exception ex);

        /// <summary>The report message.</summary>
        /// <param name="messages">The messages.</param>
        void ReportMessage(Message messages);
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

        /// <summary>The associate project.</summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        void AssociateProject(Resource project, ISonarConfiguration configuration);

        IList<string> DllLocations();

        void SetDllLocation(string path);
    }
}