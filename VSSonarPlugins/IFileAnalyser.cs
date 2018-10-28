// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileAnalyser.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The analysis mode.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarPlugins
{
    using SonarRestService.Types;
    using System.Collections.Generic;

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
        /// <param name="itemInView">The item in view.</param>
        /// <param name="project">The project.</param>
        /// <param name="conf">The conf.</param>
        /// <param name="fromSave">if set to <c>true</c> [from save].</param>
        /// <returns>
        /// The <see cref="List" />.
        /// </returns>
        List<Issue> ExecuteAnalysisOnFile(VsFileItem itemInView, Resource project, ISonarConfiguration conf, bool fromSave);

        /// <summary>The get issues.</summary>
        /// <param name="issue">The issue.</param>
        /// <param name="conf">The conf.</param>
        /// <returns>The <see cref="List"/>.</returns>
        bool IsIssueSupported(Issue issue, ISonarConfiguration conf);
    }
}
