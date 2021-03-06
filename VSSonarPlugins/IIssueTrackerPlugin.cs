﻿namespace VSSonarPlugins
{
    using SonarRestService.Types;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Types;

    /// <summary>
    /// Issue tracker interface.
    /// </summary>
    public interface IIssueTrackerPlugin : IPlugin
    {
        /// <summary>
        /// when called it will attach the current sonar issue to a existent
        /// issue in your issue tracking system
        /// </summary>
        /// <param name="issues">The issues.</param>
        /// <param name="defectId">The defect identifier.</param>
        /// <returns>
        /// Returns issue tracking information for the attached defect. 
        /// This information will be attached as comment to issue in Sonar.
        /// </returns>
        Task<string> AttachToExistentDefect(IList<Issue> issues, string defectId);

		/// <summary>
		/// creates a new issue in the existent
		/// </summary>
		/// <param name="issues">The issues.</param>
		/// <param name="defectId">The defect identifier.</param>
		/// <returns>
		/// Returns issue tracking information
		/// </returns>
		Task<string> AttachToNewDefect(IList<Issue> issues);

		/// <summary>
		/// Gets the defect from commit message.
		/// </summary>
		/// <param name="commitMessage">The commit message.</param>
		/// <returns>Returns defect using commit message</returns>
		Task<Defect> GetDefectFromCommitMessage(string commitMessage);

		/// <summary>
		/// Gets the defect from commit message.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns>
		/// Returns defect using commit message
		/// </returns>
		Task<Defect> GetDefect(string id);
    }
}