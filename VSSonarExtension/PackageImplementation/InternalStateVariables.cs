// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InternalStateVariables.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.PackageImplementation
{
    using System.Threading;

    using EnvDTE80;

    using ExtensionHelpers;

    using Microsoft.VisualStudio.Shell;

    using SonarRestService;

    /// <summary>
    /// The vs sonar extension package.
    /// </summary>
    public sealed partial class VsSonarExtensionPackage
    {
        /// <summary>
        /// The sem.
        /// </summary>
        private readonly Semaphore sem = new Semaphore(1, 1);

        /// <summary>
        /// The application object.
        /// </summary>
        private DTE2 dte2;

        /// <summary>
        /// These are the commands controls
        /// </summary>
        private OleMenuCommand sonarViolationMenuCommand;

        /// <summary>
        /// The sonar violation command bar.
        /// </summary>
        private OleMenuCommand sonarViolationCommandBar;

        /// <summary>
        /// The sonar local analysis menu command.
        /// </summary>
        private OleMenuCommand sonarLocalAnalysisMenuCommand;

        /// <summary>
        /// The sonar local analysis command bar.
        /// </summary>
        private OleMenuCommand sonarLocalAnalysisCommandBar;

        /// <summary>
        /// The sonar local analysis added menu command.
        /// </summary>
        private OleMenuCommand sonarLocalAnalysisAddedMenuCommand;

        /// <summary>
        /// The sonar local analysis added command bar.
        /// </summary>
        private OleMenuCommand sonarLocalAnalysisAddedCommandBar;

        /// <summary>
        /// The sonar coverage menu command.
        /// </summary>
        private OleMenuCommand sonarCoverageMenuCommand;

        /// <summary>
        /// The sonar coverage command bar.
        /// </summary>
        private OleMenuCommand sonarCoverageCommandBar;

        /// <summary>
        /// The sonar source diff command bar.
        /// </summary>
        private OleMenuCommand sonarSourceDiffCommandBar;

        /// <summary>
        /// The sonar reviews command bar.
        /// </summary>
        private OleMenuCommand sonarReviewsCommandBar;
        
        /// <summary>
        /// The sonar source diff menu command.
        /// </summary>
        private OleMenuCommand sonarSourceDiffMenuCommand;

        /// <summary>
        /// The sonar reviews command.
        /// </summary>
        private OleMenuCommand sonarReviewsCommand;

        /// <summary>
        /// Gets or sets the rest service.
        /// </summary>
        private ISonarRestService restService;

        /// <summary>
        /// The visual studio interface.
        /// </summary>
        private IVsEnvironmentHelper visualStudioInterface;
    }
}
