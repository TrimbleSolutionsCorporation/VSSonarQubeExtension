// --------------------------------------------------------------------------------------------------------------------
// <copyright file="testtrackplugin.cs" company="Copyright © 2015 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
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

namespace VSSQTestTrackPlugin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using TestTrackConnector;
    using System.Text;

    /// <summary>
    /// The cpp plugin.
    /// </summary>
    [Export(typeof(IPlugin))]
    public class TestTrackPlugin : IIssueTrackerPlugin
    {
        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// The descrition.
        /// </summary>
        private readonly PluginDescription descrition;

        /// <summary>
        /// The DLL locations
        /// </summary>
        private readonly IList<string> dllLocations = new List<string>();

        /// <summary>
        /// The notification manager.
        /// </summary>
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// The test track integration
        /// </summary>
        private TestTrackConnector testTrackIntegration;

        /// <summary>
        /// The user conf
        /// </summary>
        private ISonarConfiguration userConf;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQGitPlugin" /> class.
        /// </summary>
        /// <param name="notificationManager">The notification manager.</param>
        public TestTrackPlugin(INotificationManager notificationManager, ISonarConfiguration configuration)
        {
            this.userConf = configuration;
            this.notificationManager = notificationManager;
            this.descrition = new PluginDescription();
            this.descrition.Enabled = true;
            this.descrition.Description = "TestTrack Plugin";
            this.descrition.Name = "TestTrack Plugin";
            this.descrition.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.descrition.AssemblyPath = Assembly.GetExecutingAssembly().Location;
            this.testTrackIntegration = new TestTrackConnector(configuration.Username, configuration.Password, true, new TtConnection());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestTrackPlugin"/> class.
        /// </summary>
        /// <param name="notificationManager">The notification manager.</param>
        public TestTrackPlugin(INotificationManager notificationManager)
        {
            this.notificationManager = notificationManager;
            this.descrition = new PluginDescription();
            this.descrition.Enabled = true;
            this.descrition.Description = "TestTrack Plugin";
            this.descrition.Name = "TestTrack Plugin";
            this.descrition.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.descrition.AssemblyPath = Assembly.GetExecutingAssembly().Location;
        }

        /// <summary>
        /// Gets the number of blockers.
        /// </summary>
        /// <value>
        /// The number of blockers.
        /// </value>
        public int NumberOfBlockers { get; private set; }

        /// <summary>
        /// Gets the number of criticals.
        /// </summary>
        /// <value>
        /// The number of criticals.
        /// </value>
        public int NumberOfCriticals { get; private set; }

        /// <summary>
        /// Gets the number of majors.
        /// </summary>
        /// <value>
        /// The number of majors.
        /// </value>
        public int NumberOfMajors { get; private set; }

        /// <summary>
        /// Gets the technical debt.
        /// </summary>
        /// <value>
        /// The technical debt.
        /// </value>
        public int TechnicalDebt { get; private set; }

        /// <summary>
        /// Associates the project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        public void AssociateProject(Resource project, ISonarConfiguration configuration, Dictionary<string, Profile> profiles, string vsVersion)
        {
            this.associatedProject = project;
            this.userConf = configuration;
            this.testTrackIntegration = new TestTrackConnector(configuration.Username, configuration.Password, true, new TtConnection());
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="config">The configuration.</param>
        public void OnConnectToSonar(ISonarConfiguration config)
        {
            this.userConf = config;
            this.testTrackIntegration = new TestTrackConnector(config.Username, config.Password, true, new TtConnection());
        }

        /// <summary>
        /// Associates the project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="connector">The connector.</param>
        public void AssociateProject(Resource project, ISonarConfiguration configuration, IIssueManagementConnection connector, string vsVersion)
        {
            this.associatedProject = project;
            this.userConf = configuration;
            this.testTrackIntegration = new TestTrackConnector(configuration.Username, configuration.Password, true, connector);
        }

        /// <summary>
        /// Attaches to existent defect.
        /// </summary>
        /// <param name="issues">The issues.</param>
        /// <param name="defectId">The defect identifier.</param>
        /// <returns>url for link in tt.</returns>
        public string AttachToExistentDefect(IList<Issue> issues, string defectId)
        {
            if (issues == null || issues.Count == 0)
            {
                return string.Empty;
            }

            var notes = this.GatherNotesFormTestTrackInPlainText(issues, this.associatedProject);
            if (this.testTrackIntegration.AttachCommentToTestTrackItem(long.Parse(defectId), notes))
            {
                return this.testTrackIntegration.GetUrlForDefect(long.Parse(defectId));
            }

            return string.Empty;
        }

        /// <summary>
        /// Attaches to new defect.
        /// </summary>
        /// <param name="issues">The issues.</param>
        /// <returns></returns>
        public string AttachToNewDefect(IList<Issue> issues, out string id)
        {
            if (issues == null || issues.Count == 0)
            {
                id = "";
                return string.Empty;
            }

            var summary = string.Empty;
            string notes = GatherNotesFormTestTrack(issues, this.associatedProject, out summary);

            try
            {
                var defect = this.testTrackIntegration.CreateDefect("SonarQube: Issues Pending Resolution [from VSSonarExtension] " + summary, notes.ToString());
                if (defect != -1)
                {
                    id = defect.ToString();
                    return this.testTrackIntegration.GetUrlForDefect(defect);
                }               
            }
            catch (Exception ex)
            {
                this.notificationManager.ReportMessage(new Message { Id = "TestTrackPlugin", Data = "Failed to Create TestTrack Issue" });
                this.notificationManager.ReportException(ex);
            }

            id = "";
            return string.Empty;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.testTrackIntegration.Disconnect();
        }

        public IList<string> DllLocations()
        {
            return this.dllLocations;
        }

        /// <summary>
        /// Generates the token identifier.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public string GenerateTokenId(ISonarConfiguration configuration)
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the defect.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public Defect GetDefect(string id)
        {
            var defect = new Defect();
            defect.Id = id;
            try
            {
                var defectintt = this.testTrackIntegration.GetDefect(long.Parse(defect.Id));
                if (defectintt != null)
                {
                    defect.Status = defectintt.Status.ToString();
                    defect.Summary = defectintt.Summary;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return defect;
        }

        /// <summary>
        /// Gets the defect from commit message.
        /// </summary>
        /// <param name="commitMessage">The commit message.</param>
        /// <returns></returns>
        public Defect GetDefectFromCommitMessage(string commitMessage)
        {
            string [] splitArray = { "feature/", "bugfix/"};

            var messageToMatch = commitMessage.ToLower();
            if (commitMessage.Contains("feature/") || commitMessage.Contains("bugfix/"))
            {
                messageToMatch = commitMessage.Split(splitArray, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            foreach (Match item in Regex.Matches(messageToMatch, "\\d+"))
            {
                var defect = new Defect();
                defect.Id = item.Value;
                try
                {
                    var defectintt = this.testTrackIntegration.GetDefect(long.Parse(defect.Id));
                    if (defectintt != null)
                    {
                        defect.Status = defectintt.Status.ToString();
                        defect.Summary = defectintt.Summary;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                return defect;
            }

            return null;
        }

        /// <summary>
        /// Gets the plugin control options.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public IPluginControlOption GetPluginControlOptions(Resource project, ISonarConfiguration configuration)
        {
            return null;
        }

        /// <summary>
        /// Gets the plugin description.
        /// </summary>
        /// <returns></returns>
        public PluginDescription GetPluginDescription()
        {
            return this.descrition;
        }

        /// <summary>
        /// Resets the defaults.
        /// </summary>
        public void ResetDefaults()
        {
            // not in use
        }

        /// <summary>
        /// Sets the DLL location.
        /// </summary>
        /// <param name="path">The path.</param>
        public void SetDllLocation(string path)
        {
            this.dllLocations.Add(path);
        }

        /// <summary>
        /// Populates the statistics.
        /// </summary>
        /// <param name="issue">The issue.</param>
        private void PopulateStatistics(Issue issue)
        {
            switch (issue.Severity)
            {
                case Severity.BLOCKER:
                    this.NumberOfBlockers++;
                    break;
                case Severity.CRITICAL:
                    this.NumberOfCriticals++;
                    break;
                case Severity.MAJOR:
                    this.NumberOfMajors++;
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(issue.Effort))
            {
                try
                {
                    var debt = issue.Effort.Replace("min", "").Replace("sec", "").Replace("hour", "").Replace("day", "").Replace("d", "").Replace("h", "");
                    this.TechnicalDebt += int.Parse(debt);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Gathers the notes form test track.
        /// </summary>
        /// <param name="issues">The issues.</param>
        /// <returns></returns>
        private string GatherNotesFormTestTrack(IList<Issue> issues, Resource project, out string summaryOut)
        {
            this.NumberOfBlockers = 0;
            this.NumberOfCriticals = 0;
            this.NumberOfMajors = 0;
            this.TechnicalDebt = 0;

            StringBuilder notes = new StringBuilder();
            notes.AppendLine("<p align=\"left\"><span style=\" font-size:12pt; color:#3366ff;\">Issues are pending resolution</span></p>");
            notes.AppendLine("<hr />");
            if (project != null)
            {
                notes.AppendLine("<p> Project : " + project.SolutionName + " : Key : " + project.Key);
            }
            else
            {
                notes.AppendLine("<p> Project : Multiple Projects" );
            }

            notes.AppendLine("<hr />");

            HashSet<string> plans = new HashSet<string>();

            foreach (var issue in issues)
            {
                var compElelms = issue.Component.Split(':');
                var url = "    [" + issue.Assignee + "] " + issue.Message + " : " + compElelms[compElelms.Length - 1] + " : " + issue.Line + " : ";
                notes.AppendLine("<p>    " + url + "<a href=\"" + this.userConf.Hostname.TrimEnd('/') + "/issues/search#issues=" + issue.Key + "\">See in SonarQube</a></p>");
                this.PopulateStatistics(issue);
            }

            notes.AppendLine("<hr />");
            summaryOut = "Issues: " + issues.Count + " => Blockers: " + this.NumberOfBlockers + " Critical: " + this.NumberOfCriticals + " Majors: " + this.NumberOfMajors + " Debt: " + this.TechnicalDebt + " mn";
            var summary = "<p>Issues: " + issues.Count + " => Blockers: " + this.NumberOfBlockers + " Critical: " + this.NumberOfCriticals + " Majors: " + this.NumberOfMajors + " Debt: " + this.TechnicalDebt + " mn</p>";
            notes.AppendLine(summary);

            notes.AppendLine("<p><br /> </p>");

            return notes.ToString();
        }

        /// <summary>
        /// Gathers the notes form test track.
        /// </summary>
        /// <param name="issues">The issues.</param>
        /// <returns></returns>
        private string GatherNotesFormTestTrackInPlainText(IList<Issue> issues, Resource project)
        {
            this.NumberOfBlockers = 0;
            this.NumberOfCriticals = 0;
            this.NumberOfMajors = 0;
            this.TechnicalDebt = 0;

            StringBuilder notes = new StringBuilder();
            notes.AppendLine("Issues are pending resolution:");
            notes.AppendLine("");

            if (project != null)
            {
                notes.AppendLine("Project : " + project.SolutionName + " : Key : " + project.Key);
            }

            HashSet<string> plans = new HashSet<string>();

            foreach (var issue in issues)
            {
                var compElelms = issue.Component.Split(':');
                var data = "    [" + issue.Assignee + "] " + issue.Message + " : " + compElelms[compElelms.Length - 1] + " : " + issue.Line + " : ";
                notes.AppendLine("    " + data + " => " + this.userConf.Hostname.TrimEnd('/') + "/issues/search#issues=" + issue.Key);
                this.PopulateStatistics(issue);
            }

            if (plans.Count != 0)
            {
                notes.AppendLine("");
                notes.AppendLine("The above issues are contained in the following action plans:");
                int i = 1;
                foreach (var plan in plans)
                {
                    var url = this.userConf.Hostname.TrimEnd('/') + "/issues/search#actionPlans=" + plan;
                    notes.AppendLine("    " + i.ToString() + " : " + url);
                    i++;
                }
            }

            notes.AppendLine("");
            var summary = "Issues: " + issues.Count + " Blockers: " + this.NumberOfBlockers + " Critical: " + this.NumberOfCriticals + " Majors: " + this.NumberOfMajors + " Debt: " + this.TechnicalDebt + " mn";
            notes.AppendLine(summary);
            return notes.ToString();
        }        
    }
}