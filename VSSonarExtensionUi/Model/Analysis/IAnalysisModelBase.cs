// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAnalysisViewModelBase.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2014 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The AnalysisViewModelBase interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.Model.Analysis
{
    using System;
    using System.Collections.Generic;

    using ViewModel.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// The AnalysisViewModelBase interface.
    /// </summary>
    public interface IAnalysisModelBase
    {
        /// <summary>The trigger a project analysis.</summary>
        /// <param name="project">The project.</param>
        void TriggerAProjectAnalysis(VsProjectItem project);

        /// <summary>The get issues for resource.</summary>
        /// <param name="file">The file.</param>
        /// <param name="fileContent">The file content.</param>
        /// <returns>The <see>
        ///         <cref>List</cref>
        ///     </see>
        /// .</returns>
        List<Issue> GetIssuesForResource(Resource file, string fileContent);
        
        /// <summary>The on analysis mode has change.</summary>
        /// <param name="e">The e.</param>
        void OnAnalysisModeHasChange(EventArgs e);

        /// <summary>
        /// The refresh data for resource.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="documentInView">The document in view.</param>
        /// <param name="fileContent">Content of the file.</param>
        void RefreshDataForResource(Resource file, string documentInView, string fileContent);

        /// <summary>Reset Stats.</summary>
        void ResetStats();

        /// <summary>
        /// Clears the issues.
        /// </summary>
        void ClearIssues();
    }
}