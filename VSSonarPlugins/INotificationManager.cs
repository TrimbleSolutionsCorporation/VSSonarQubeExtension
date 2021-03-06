﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPlugin.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarPlugins
{
    using SonarRestService;

    /// <summary>
    /// The NotificationManager interface.
    /// </summary>
    public interface INotificationManager : ILogManager, IRestLogger
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

        /// <summary>
        /// News the issues are ready to be updated in view
        /// </summary>
        void OnIssuesUpdated();

        /// <summary>
        /// Called when [new issues updated].
        /// </summary>
        void OnNewIssuesUpdated();

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

        /// <summary>
        /// Resets the and establish a new connection to server.
        /// </summary>
        void ResetAndEstablishANewConnectionToServer();
    }
}