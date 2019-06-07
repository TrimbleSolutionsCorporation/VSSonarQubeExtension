// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JiraPlugin.cs" company="Copyright © 2017 Trimble Solutions Corporation">
//     Copyright (C) 2017 [Trimble Solutions Corporation, tekla.buildmaster@trimble.com]
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
    using System.Text;

    using JiraConnector;
    using Newtonsoft.Json;
    using SonarRestService.Types;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// The cpp plugin.
    /// </summary>
    [Export(typeof(IPlugin))]
    public class JiraPlugin : IIssueTrackerPlugin
    {
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
        /// The jira integration
        /// </summary>
        private JiraConnector jiraIntegration;

        /// <summary>
        /// The user conf
        /// </summary>
        private ISonarConfiguration userConf;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// Initializes a new instance of the <see cref="JiraPlugin"/> class.
        /// </summary>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="configuration">The configuration.</param>
        public JiraPlugin(INotificationManager notificationManager, ISonarConfiguration configuration)
        {
            this.userConf = configuration;
            this.notificationManager = notificationManager;
            this.descrition = new PluginDescription();
            this.descrition.Enabled = true;
            this.descrition.Description = "Jira Plugin";
            this.descrition.Name = "Jira Plugin";
            this.descrition.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.descrition.AssemblyPath = Assembly.GetExecutingAssembly().Location;
            this.jiraIntegration = new JiraConnector(this.notificationManager);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JiraPlugin"/> class.
        /// </summary>
        /// <param name="notificationManager">The notification manager.</param>
        public JiraPlugin(INotificationManager notificationManager)
        {
            this.notificationManager = notificationManager;
            this.descrition = new PluginDescription();
            this.descrition.Enabled = true;
            this.descrition.Description = "Jira Plugin";
            this.descrition.Name = "Jira Plugin";
            this.descrition.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.descrition.AssemblyPath = Assembly.GetExecutingAssembly().Location;
            this.jiraIntegration = new JiraConnector(this.notificationManager);
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
        /// <param name="profiles">The profiles.</param>
        /// <param name="vsVersion">The vs version.</param>
        public void AssociateProject(Resource project, ISonarConfiguration configuration, Dictionary<string, Profile> profiles, string vsVersion)
        {
            this.associatedProject = project;
            this.userConf = configuration;
            this.jiraIntegration = new JiraConnector(this.notificationManager);
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public void OnConnectToSonar(ISonarConfiguration configuration)
        {
            this.userConf = configuration;
            this.jiraIntegration = new JiraConnector(this.notificationManager);
        }

        /// <summary>
        /// Associates the project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="vsVersion">The vs version.</param>
        public void AssociateProject(Resource project, ISonarConfiguration configuration, string vsVersion)
        {
            this.associatedProject = project;
            this.userConf = configuration;
            this.jiraIntegration = new JiraConnector(this.notificationManager);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // not needed
        }

        /// <summary>
        /// Attaches to existent defect.
        /// </summary>
        /// <param name="issues">The issues.</param>
        /// <param name="defectId">The defect identifier.</param>
        /// <returns>
        /// url for link in tt.
        /// </returns>
        public string AttachToExistentDefect(IList<Issue> issues, string defectId)
        {
            if (issues == null || issues.Count == 0)
            {
                return string.Empty;
            }

            var notes = this.GatherNotesInPlainText(issues, this.associatedProject);
            string result = this.jiraIntegration.UpdateIssue(defectId, notes.ToString());
            if (result != "Created")
                {
                    Debug.WriteLine("Could not update the issue:" + result);
                }
                else
                {
                    return this.jiraIntegration.GetUrlForDefect(defectId);
                }
            
            return string.Empty;
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <param name="issuedata">The issuedata.</param>
        /// <returns>description</returns>
        public string GetDescription(string issuedata)
        {
            string description = string.Empty;
            try
            {
                    dynamic dynobj = JsonConvert.DeserializeObject(issuedata);
                    return description = dynobj.fields.description.Value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return description;
        }

        /// <summary>
        /// Attaches to new defect.
        /// </summary>
        /// <param name="issues">The issues.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>defect number</returns>
        public string AttachToNewDefect(IList<Issue> issues, out string id)
        {
            if (issues == null || issues.Count == 0)
            {
                id = string.Empty;
                return string.Empty;
            }

            var summary = string.Empty;
            string notes = this.GatherNotes(issues, this.associatedProject, out summary);

            try
            {
                // create defect
                // "SonarQube: Issues Pending Resolution [from VSSonarExtension] " + summary, notes.ToString()
                var defect = this.jiraIntegration.CreateIssue("SonarQube: Issues Pending Resolution [from VSSonarExtension] " + summary, notes.ToString());
                if (defect != string.Empty)
                {
                    id = defect.ToString();
                    return this.jiraIntegration.GetUrlForDefect(defect);
                }
            }
            catch (Exception ex)
            {
                this.notificationManager.ReportMessage(new Message { Id = "JiraPlugin", Data = "Failed to Create Jira Issue" });
                this.notificationManager.ReportException(ex);
            }

            id = string.Empty;
            return string.Empty;
        }

        /// <summary>
        /// DLLs the locations.
        /// </summary>
        /// <returns> locations of dlls</returns>
        public IList<string> DllLocations()
        {
            return this.dllLocations;
        }

        /// <summary>
        /// Generates the token identifier.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>token id</returns>
        public string GenerateTokenId(ISonarConfiguration configuration)
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the defect.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>defect</returns>
        public Defect GetDefect(string id)
        {
            var defect = new Defect();
            defect.Id = id;
            try
            {
                string issuedata = this.jiraIntegration.GetIssue(id);
                if (issuedata != string.Empty || issuedata != "Unauthorized")
                {
                    dynamic dynobj = JsonConvert.DeserializeObject(issuedata);
                    defect.Status = dynobj.fields.status.name.Value;
                    defect.Summary = dynobj.fields.summary.Value;
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
        /// <returns>defect</returns>
        public Defect GetDefectFromCommitMessage(string commitMessage)
        {
            return null;
        }

        /// <summary>
        /// Gets the plugin control options.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>null</returns>
        public IPluginControlOption GetPluginControlOptions(Resource project, ISonarConfiguration configuration)
        {
            return null;
        }

        /// <summary>
        /// Gets the plugin description.
        /// </summary>
        /// <returns>plugin description</returns>
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
                    var debt = issue.Effort
                        .Replace("min", string.Empty)
                        .Replace("sec", string.Empty)
                        .Replace("hour", string.Empty)
                        .Replace("day", string.Empty)
                        .Replace("d", string.Empty)
                        .Replace("h", string.Empty);
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
        /// <param name="project">The project.</param>
        /// <param name="summaryOut">The summary out.</param>
        /// <returns>
        /// notes
        /// </returns>
        private string GatherNotes(IList<Issue> issues, Resource project, out string summaryOut)
        {
            this.NumberOfBlockers = 0;
            this.NumberOfCriticals = 0;
            this.NumberOfMajors = 0;
            this.TechnicalDebt = 0;

            StringBuilder notes = new StringBuilder();
            notes.AppendLine("h2. {color:#205081}Issues are pending resolution{color}");
            if (project != null)
            {
                notes.AppendLine("Project : " + project.SolutionName + " : Key : " + project.Key);
            }
            else
            {
                notes.AppendLine("Project : Multiple Projects");
            }

            foreach (var issue in issues)
            {
                var compElelms = issue.Component.Split(':');
				notes.AppendLine(this.GetLineForIssue(issue, compElelms));                
                this.PopulateStatistics(issue);
            }

            summaryOut = "Issues: " + issues.Count 
                + " => Blockers: " + this.NumberOfBlockers 
                + " Critical: " + this.NumberOfCriticals 
                + " Majors: " + this.NumberOfMajors 
                + " Debt: " + this.TechnicalDebt + " mn";
            var summary = "Issues: " + issues.Count 
                + " => Blockers: " + this.NumberOfBlockers 
                + " Critical: " + this.NumberOfCriticals 
                + " Majors: " + this.NumberOfMajors 
                + " Debt: " + this.TechnicalDebt + " mn";
            notes.AppendLine(summary);

            return notes.ToString();
        }

		private string GetLineForIssue(Issue issue, string[] compElelms)
		{
			var url = "    [" + issue.Assignee + "] " + issue.Message + " : " + compElelms[compElelms.Length - 1] + " : " + issue.Line + " : ";
			var openIssueString = string.Format("[Open Issue|{0}/issues?issues={1}&open={1}]", this.userConf.Hostname.TrimEnd('/'), issue.Key);
			var openFileString = string.Format("[Open File|{0}/component?id={1}&line={2}]", this.userConf.Hostname.TrimEnd('/'), issue.Component, issue.Line);
			return url + openIssueString + openFileString; 
		}

		/// <summary>
		/// Gathers the notes form test track.
		/// </summary>
		/// <param name="issues">The issues.</param>
		/// <param name="project">The project.</param>
		/// <returns>notes</returns>
		private string GatherNotesInPlainText(IList<Issue> issues, Resource project)
        {
            this.NumberOfBlockers = 0;
            this.NumberOfCriticals = 0;
            this.NumberOfMajors = 0;
            this.TechnicalDebt = 0;

            StringBuilder notes = new StringBuilder();
            notes.AppendLine("Issues are pending resolution:");
            notes.AppendLine(string.Empty);

            if (project != null)
            {
                notes.AppendLine("Project : " + project.SolutionName + " : Key : " + project.Key);
            }

            HashSet<string> plans = new HashSet<string>();

            foreach (var issue in issues)
            {
                var compElelms = issue.Component.Split(':');
                notes.AppendLine(this.GetLineForIssue(issue, compElelms));
                this.PopulateStatistics(issue);
            }

            if (plans.Count != 0)
            {
                notes.AppendLine(string.Empty);
                notes.AppendLine("The above issues are contained in the following action plans:");
                int i = 1;
                foreach (var plan in plans)
                {
                    var url = this.userConf.Hostname.TrimEnd('/') + "/issues/search#actionPlans=" + plan;
                    notes.AppendLine("    " + i.ToString() + " : " + url);
                    i++;
                }
            }

            notes.AppendLine(string.Empty);
            var summary = "Issues: " + issues.Count 
                + "  => Blockers: " + this.NumberOfBlockers
                + " Critical: " + this.NumberOfCriticals
                + " Majors: " + this.NumberOfMajors
                + " Debt: " + this.TechnicalDebt + " mn";
            notes.AppendLine(summary);
            return notes.ToString();
        }
    }
}