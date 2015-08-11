using System.Collections.Generic;
using VSSonarPlugins.Types;

namespace VSSonarPlugins
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// issue tracking provider
    /// </summary>
    // --------------------------------------------------------------------------------
    public interface IIssueTrackerPlugin : IPlugin
    {
        /// <summary>
        /// when called it will attach the current sonar issue to a existent 
        /// issue in your issue tracking system
        /// </summary>
        /// <param name="issues"></param>
        /// <param name="defectId"></param>
        /// <returns></returns>
        void AttachToExistentDefect(IList<Issue> issues, int defectId);

        /// <summary>
        /// creates a new issue in the existent 
        /// </summary>
        /// <param name="issues"></param>
        /// <returns></returns>
        void CreateDefect(IList<Issue> issues);

        /// <summary>
        /// returns a list of defects from current tracking system
        /// </summary>
        /// <param name="item"></param>
        /// <param name="versionPlugin"></param>
        /// <returns></returns>
        void GetIssuesFromResource(Resource item, ISourceVersionPlugin versionPlugin);
    }
}