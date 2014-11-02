// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAnalysisViewModelBase.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtensionUi.ViewModel.Helpers
{
    using System;
    using System.Collections.Generic;

    using ExtensionTypes;

    using SonarRestService;

    using VSSonarPlugins;

    /// <summary>
    /// The AnalysisViewModelBase interface.
    /// </summary>
    public interface IAnalysisViewModelBase : IViewModelBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The end data association.
        /// </summary>
        void EndDataAssociation();

        /// <summary>
        /// The get issues for resource.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="fileContent">
        /// The file content.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        List<Issue> GetIssuesForResource(Resource file, string fileContent);

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="associatedProject">
        /// The associated project.
        /// </param>
        /// <param name="userConnectionConfig">
        /// The user connection config.
        /// </param>
        /// <param name="workingDir">
        /// The working dir.
        /// </param>
        void InitDataAssociation(Resource associatedProject, ISonarConfiguration userConnectionConfig, string workingDir);

        /// <summary>
        /// The on analysis mode has change.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        void OnAnalysisModeHasChange(EventArgs e);

        /// <summary>
        /// The refresh data for resource.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="documentInView">
        /// The document in view.
        /// </param>
        void RefreshDataForResource(Resource file, string documentInView);

        /// <summary>
        /// The update services.
        /// </summary>
        /// <param name="restServiceIn">
        /// The rest service in.
        /// </param>
        /// <param name="vsenvironmenthelperIn">
        /// The vsenvironmenthelper in.
        /// </param>
        /// <param name="statusBar">
        /// The status bar.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        void UpdateServices(
            ISonarRestService restServiceIn, 
            IVsEnvironmentHelper vsenvironmenthelperIn,
            IConfigurationHelper configurationHelper,
            IVSSStatusBar statusBar, 
            IServiceProvider provider);

        #endregion
    }
}