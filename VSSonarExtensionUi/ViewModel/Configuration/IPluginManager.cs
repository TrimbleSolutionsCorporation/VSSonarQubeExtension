// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPluginManager.cs" company="Copyright © 2015 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2014 [Jorge Costa, Jorge.Costa@tekla.com]
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
namespace VSSonarExtensionUi.ViewModel.Configuration
{
    using System.Collections.Generic;

    using Model.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    using SonarRestService.Types;
    using SonarRestService;
    using System.Threading.Tasks;

    /// <summary>
    /// the plugin manager
    /// </summary>
    public interface IPluginManager
    {
        /// <summary>
        /// Gets the source code plugins.
        /// </summary>
        /// <value>
        /// The source code plugins.
        /// </value>
        IList<ISourceVersionPlugin> SourceCodePlugins { get; }

        /// <summary>
        /// Gets the issue tracker plugins.
        /// </summary>
        /// <value>
        /// The issue tracker plugins.
        /// </value>
        IList<IIssueTrackerPlugin> IssueTrackerPlugins { get; }

        /// <summary>
        /// Gets the analysis plugins.
        /// </summary>
        /// <value>
        /// The analysis plugins.
        /// </value>
        IList<IAnalysisPlugin> AnalysisPlugins { get; }

        /// <summary>
        /// Gets the menu plugins.
        /// </summary>
        /// <value>
        /// The menu plugins.
        /// </value>
        IList<IMenuCommandPlugin> MenuPlugins { get; }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="authToken">The authentication token.</param>
        /// <param name="associatedProject">The associated project.</param>
        /// <param name="openSolutionPath">The open solution path.</param>
        /// <param name="sourceControl">The source control.</param>
        Task AssociateWithNewProject(
            Resource associatedProject,
            string openSolutionPath,
            ISourceControlProvider sourceControl,
            Dictionary<string, Profile> profile,
            string visualStudioVersion);
    }
}