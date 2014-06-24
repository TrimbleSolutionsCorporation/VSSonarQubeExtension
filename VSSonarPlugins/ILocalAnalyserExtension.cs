// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILocalAnalyserExtension.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarPlugins
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows.Documents;

    using ExtensionTypes;


    /// <summary>
    /// The analysis mode.
    /// </summary>
    public enum AnalysisMode
    {
        /// <summary>
        /// The file.
        /// </summary>
        File,

        /// <summary>
        /// The incremental.
        /// </summary>
        Incremental,

        /// <summary>
        /// The preview.
        /// </summary>
        Preview,

        /// <summary>
        /// The full.
        /// </summary>
        Full,
    }

    /// <summary>
    /// The Sensors interface.
    /// </summary>
    public interface ILocalAnalyserExtension
    {
        /// <summary>
        /// The model will register to all extensions, so after analysis each
        /// extension will need to trigger this event to tell the model that issues
        /// are ready to read.
        /// Analysis will be done in a separate thread, therefore this is mandatory
        /// to be done for the extension to work
        /// </summary>
        event EventHandler LocalAnalysisCompleted;

        /// <summary>
        /// The std out event.
        /// </summary>
        event EventHandler StdOutEvent;

        /// <summary>
        /// The std err event.
        /// </summary>
        event EventHandler StdErrEvent;

        /// <summary>
        /// The get file analyser thread.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="projectKey">
        /// The project key.
        /// </param>
        /// <param name="profile">
        /// The profile.
        /// </param>
        /// <param name="fileSourceInServer">
        /// The file Source In Server.
        /// </param>
        /// <param name="onModifiedLinesOnly">
        /// The on Modified Lines Only.
        /// </param>
        /// <returns>
        /// The <see cref="Thread"/>.
        /// </returns>
        Thread GetFileAnalyserThread(VsProjectItem item, Resource project, Profile profile, string fileSourceInServer, bool onModifiedLinesOnly);

        /// <summary>
        /// The stop all execution.
        /// </summary>
        /// <param name="runningThread">
        /// The running thread.
        /// </param>
        void StopAllExecution(Thread runningThread);

        /// <summary>
        /// The execute analysis on file.
        /// </summary>
        /// <param name="itemInView">
        /// The item in view.
        /// </param>
        /// <param name="externlProfile">
        /// The externl profile.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        List<Issue> ExecuteAnalysisOnFile(VsProjectItem itemInView, Profile externlProfile, Resource project);

        /// <summary>
        /// The get issues.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        List<Issue> GetIssues();

        /// <summary>
        /// The get issues.
        /// </summary>
        /// <param name="issues">
        /// The issues.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        List<Issue> GetSupportedIssues(List<Issue> issues);

        /// <summary>
        /// The get local analysis paramenters.
        /// </summary>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see cref="ICollection"/>.
        /// </returns>
        List<SonarQubeProperties> GetLocalAnalysisParamenters(Resource project);
    }
}
