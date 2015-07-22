using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSonarPlugins.Types;

namespace VSSonarPlugins
{
    interface ISourceVersionPlugin
    {
        IList<string> GetHistory(Resource item);
    }

    interface IIssueTrackerPlugin
    {
        void AttachToExistentDefect(IList<Issue> issues, int defectId);
        void CreateDefect(IList<Issue> issues);
        void GetIssuesFromResource(Resource item, ISourceVersionPlugin versionPlugin);
    }
}
