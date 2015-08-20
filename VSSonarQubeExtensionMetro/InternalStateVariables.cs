// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InternalStateVariables.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarQubeExtension
{
    using EnvDTE80;
    using Microsoft.VisualStudio.Shell;
    using SonarRestService;
    using VSSonarExtensionUi.ViewModel;
    using VSSonarPlugins;
    using VSSonarQubeExtension.Helpers;

    /// <summary>
    /// The vs sonar extension package.
    /// </summary>
    public sealed partial class VsSonarExtensionPackage
    {
        /// <summary>
        /// The application object.
        /// </summary>
        private DTE2 dte2;

        /// <summary>
        /// The sonar reviews command bar.
        /// </summary>
        private OleMenuCommand sonarReviewsCommandBar;

        /// <summary>
        /// The run analysis cmd.
        /// </summary>
        private OleMenuCommand runAnalysisCmd;

        /// <summary>
        /// The run analysis in project cmd.
        /// </summary>
        private OleMenuCommand runAnalysisInProjectCmd;

        /// <summary>
        /// The sonar reviews command.
        /// </summary>
        private OleMenuCommand sonarReviewsCommand;

        /// <summary>
        /// The visual studio interface.
        /// </summary>
        private IVsEnvironmentHelper visualStudioInterface;

        /// <summary>
        /// Gets or sets the vs events.
        /// </summary>
        public VsEvents VsEvents { get; set; }
    }

    /// <summary>
    /// model factory
    /// </summary>
    internal class SonarQubeViewModelFactory
    {
        /// <summary>
        /// The model
        /// </summary>
        private static SonarQubeViewModel model;

        /// <summary>
        /// Prevents a default instance of the <see cref="SonarQubeViewModelFactory"/> class from being created.
        /// </summary>
        private SonarQubeViewModelFactory()
        {
        }

        /// <summary>
        /// Gets the sq view model.
        /// </summary>
        /// <value>
        /// The sq view model.
        /// </value>
        public static SonarQubeViewModel SQViewModel
        {
            get
            {
                if (model == null)
                {
                    model = new SonarQubeViewModel(string.Empty);
                }

                return model;
            }
        }


        /// <summary>
        /// Startups the model with vs version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns>returns model</returns>
        public static SonarQubeViewModel StartupModelWithVsVersion(string version)
        {
            if (model == null)
            {
                model = new VSSonarExtensionUi.ViewModel.SonarQubeViewModel(version);
            }

            return model;
        }
    }
}
