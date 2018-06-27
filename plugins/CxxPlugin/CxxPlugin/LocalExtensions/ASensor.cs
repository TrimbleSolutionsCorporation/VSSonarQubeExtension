// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ASensor.cs" company="Copyright © 2014 jmecsoftware">
//   Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
// </copyright>
// <summary>
//   The Sensor interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CxxPlugin.LocalExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    ///     The Sensor interface.
    /// </summary>
    public abstract class ASensor
    {
        /// <summary>
        ///     The repository key.
        /// </summary>
        protected readonly string RepositoryKey;

        /// <summary>
        ///     The use std out.
        /// </summary>
        protected readonly bool UseStdout;

        /// <summary>
        /// The configuration helper.
        /// </summary>
        protected readonly IConfigurationHelper ConfigurationHelper;

        /// <summary>
        /// The notification manager.
        /// </summary>
        protected readonly INotificationManager NotificationManager;

        /// <summary>
        /// The sonar rest service.
        /// </summary>
        protected readonly ISonarRestService SonarRestService;

        /// <summary>
        ///     The command line error.
        /// </summary>
        private readonly List<string> commandLineError = new List<string>();

        /// <summary>
        ///     The CommandLineOuput.
        /// </summary>
        private readonly List<string> commandLineOuput = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ASensor" /> class.
        /// </summary>
        /// <param name="repositoryKey">The repository key.</param>
        /// <param name="useStdout">The use std out.</param>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="sonarRestService">The sonar rest service.</param>
        protected ASensor(
            string repositoryKey, 
            bool useStdout, 
            INotificationManager notificationManager, 
            IConfigurationHelper configurationHelper, 
            ISonarRestService sonarRestService)
        {
            this.NotificationManager = notificationManager;
            this.ConfigurationHelper = configurationHelper;
            this.SonarRestService = sonarRestService;
            this.RepositoryKey = repositoryKey;
            this.UseStdout = useStdout;
        }

        /// <summary>
        /// The launch sensor.
        /// </summary>
        /// <param name="caller">The caller.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="filePath">The file Path.</param>
        /// <param name="executor">The executor.</param>
        /// <returns>
        /// The <see cref="Process" />.
        /// </returns>
        public virtual List<string> LaunchSensor(
            object caller, 
            EventHandler logger, 
            string filePath, 
            IVSSonarQubeCmdExecutor executor)
        {
            var commandline = "[" + Directory.GetParent(filePath) + "] : " + this.GetCommand() + " "
                              + this.GetArguments(filePath);
            CxxPlugin.WriteLogMessage(this.NotificationManager, caller.GetType().ToString(), commandline);
            executor.ExecuteCommand(
                this.GetCommand(), 
                this.GetArguments(filePath), 
                this.GetEnvironment(), 
                string.Empty);

            return this.UseStdout ? executor.GetStdOut() : executor.GetStdError();
        }

        /// <summary>
        ///     The get key.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string GetKey()
        {
            return this.RepositoryKey;
        }

        /// <summary>
        /// The get violations.
        /// </summary>
        /// <param name="lines">
        /// The lines.
        /// </param>
        /// <returns>
        /// The VSSonarPlugin.SonarInterface.ResponseMappings.Violations.ViolationsResponse.
        /// </returns>
        public abstract List<Issue> GetViolations(List<string> lines);

        /// <summary>
        ///     The get environment.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public abstract Dictionary<string, string> GetEnvironment();

        /// <summary>
        ///     The get command.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public abstract string GetCommand();

        /// <summary>
        /// The get arguments.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        public abstract string GetArguments(string filePath);

        /// <summary>
        /// Updates the profile.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="profileIn">The profile in.</param>
        public abstract void UpdateProfile(
            Resource project,
            ISonarConfiguration configuration,
            Dictionary<string, Profile> profileIn,
            string vsVersion);

        /// <summary>
        /// The read get property.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected string ReadGetProperty(string key)
        {
            try
            {
                return
                    this.ConfigurationHelper.ReadSetting(
                        Context.FileAnalysisProperties,
                        "CxxPlugin", 
                        key).Value;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// The write property.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="sync">
        /// The sync.
        /// </param>
        /// <param name="skipIfFound">
        /// The skip if found.
        /// </param>
        protected void WriteProperty(string key, string value, bool sync = false, bool skipIfFound = false)
        {
            this.ConfigurationHelper.WriteSetting(
                Context.FileAnalysisProperties,
                "CxxPlugin", 
                key, 
                value, 
                sync, 
                skipIfFound);
        }

        /// <summary>
        /// The process output data received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                this.commandLineOuput.Add(e.Data);
            }
        }

        /// <summary>
        /// The process error data received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                this.commandLineError.Add(e.Data);
            }
        }
    }
}