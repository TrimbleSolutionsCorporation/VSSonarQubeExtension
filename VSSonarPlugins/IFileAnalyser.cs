// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileAnalyser.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

    
    using VSSonarPlugins.Types;


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
    public interface IFileAnalyser
    {
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
        List<Issue> ExecuteAnalysisOnFile(VsProjectItem itemInView, Profile externlProfile, Resource project, ISonarConfiguration conf);

        /// <summary>
        /// The get issues.
        /// </summary>
        /// <param name="issue">
        /// The issue.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        bool IsIssueSupported(Issue issue, ISonarConfiguration conf);
    }
}
