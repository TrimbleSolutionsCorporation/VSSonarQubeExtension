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

using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("JiraPlugin.Test")]
namespace VSSQTestTrackPlugin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Atlassian.Jira;
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
        private readonly string configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "JiraSetup.cfg");
        private readonly PluginDescription descrition;
        private readonly IList<string> dllLocations = new List<string>();
        private readonly INotificationManager notificationManager;
        private ISonarConfiguration userConf;
        private Resource associatedProject;
        private bool jiraEnabled;
        private int NumberOfBlockers;
        private int NumberOfCriticals;
        private int NumberOfMajors;
        public Dictionary<string, string> MandatoryFields;
        public Dictionary<string, string> CustomFields;
        private int TechnicalDebt;

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
            this.jiraEnabled = false;
            this.CustomFields = new Dictionary<string, string>();
            this.MandatoryFields = new Dictionary<string, string>();
            LoadConfigurationFile(this.configFile, notificationManager);
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
            this.jiraEnabled = false;
            this.CustomFields = new Dictionary<string, string>();
            this.MandatoryFields = new Dictionary<string, string>();
            LoadConfigurationFile(this.configFile, notificationManager);
        }

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
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public void OnConnectToSonar(ISonarConfiguration configuration)
        {
            this.userConf = configuration;
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
        public async Task<string> AttachToExistentDefect(IList<SonarRestService.Types.Issue> issues, string defectId)
        {
            if (!this.jiraEnabled)
            {
                this.notificationManager.ReportMessage("Jira plugin not enabled because of invalid configuration");
                return string.Empty;
            }

            if (issues == null || issues.Count == 0)
            {
                return string.Empty;
            }

            var notes = this.GatherNotes(issues, this.associatedProject);
            var jira = Jira.CreateRestClient(
                this.MandatoryFields["JiraURL"],
                this.MandatoryFields["UserName"],
                this.MandatoryFields["Password"]);

            var issue = await jira.Issues.GetIssueAsync(defectId);
            var comment = await issue.AddCommentAsync(notes[0]);

            if (notes.Count > 1)
            {
                for (int i = 1; i < notes.Count; i++)
                {
                    var commentNew = await issue.AddCommentAsync(notes[i].ToString());
                    if (commentNew == null)
                    {
                        this.notificationManager.ReportMessage("Failed to add additional elements to ticket");
                    }
                }
            }

            if (comment != null)
            {
                return defectId;
            }

            this.notificationManager.ReportMessage("Failed to attach to jira ticket");            
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
        public async Task<string> AttachToNewDefect(IList<SonarRestService.Types.Issue> issues)
        {
            if (!this.jiraEnabled)
            {
                this.notificationManager.ReportMessage("Jira plugin not enabled because of invalid configuration");
                return string.Empty;
            }

            if (issues == null || issues.Count == 0)
            {
                return string.Empty;
            }

            var notes = this.GatherNotes(issues, this.associatedProject);

            var summary = "Issues: " + issues.Count
                + " => Blockers: " + this.NumberOfBlockers
                + " Critical: " + this.NumberOfCriticals
                + " Majors: " + this.NumberOfMajors
                + " Debt: " + this.TechnicalDebt + " mn"
                + " Team: " + issues[0].Team;

            try
            {
                // create defect
                // "SonarQube: Issues Pending Resolution [from VSSonarExtension] " + summary, notes.ToString()
                var jira = Jira.CreateRestClient(
                    this.MandatoryFields["JiraURL"],
                    this.MandatoryFields["UserName"],
                    this.MandatoryFields["Password"]);

                var issue = jira.CreateIssue(this.MandatoryFields["JiraProject"]);
                issue.Type = this.MandatoryFields["IssueType"];
                issue.Summary = "SonarQube: Issues Pending Resolution [from VSSonarExtension] " + summary;
                issue.Description = notes[0];
                issue.Components.Add(this.MandatoryFields["Components"]);

                foreach (var customField in this.CustomFields)
                {
                    try
                    {
                        issue[customField.Key] = customField.Value;
                    }
                    catch (Exception ex)
                    {
                        this.notificationManager.ReportMessage(new Message { Id = "JiraPlugin", Data = "Custom Field Incorrectly Configured: " +  ex.Message});
                        this.notificationManager.ReportException(ex);
                    }
                }
                
                var defect = await issue.SaveChangesAsync();


                if (notes.Count > 1)
                {
                    for (int i = 1; i < notes.Count; i++)
                    {
                        var comment = await issue.AddCommentAsync(notes[i].ToString());
                        if (comment == null)
                        {
                            this.notificationManager.ReportMessage("Failed to add additional elements to ticket");
                        }
                    }
                }

                if (defect != null)
                {
                    this.notificationManager.ReportMessage("Ticket Created: " + defect.Key.Value);
                    return defect.Key.Value;
                }
            }
            catch (Exception ex)
            {
                this.notificationManager.ReportMessage(new Message { Id = "JiraPlugin", Data = "Failed to Create Jira Issue" });
                this.notificationManager.ReportException(ex);
            }

            this.notificationManager.ReportMessage(new Message { Id = "JiraPlugin", Data = "Failed to Create Jira Issue for unknown reason" });
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
        /// Gets the defect.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>defect</returns>
        public async Task<Defect> GetDefect(string id)
        {
            var defect = new Defect();
            defect.Id = id;
            var jira = Jira.CreateRestClient(
                this.MandatoryFields["JiraURL"],
                this.MandatoryFields["UserName"],
                this.MandatoryFields["Password"]);
            var issue = await jira.Issues.GetIssueAsync(id);

            if (issue != null)
            {
                defect.Status = issue.Status.Description;
                defect.Summary = issue.Summary;
            }
            else
            {
                this.notificationManager.ReportMessage("Failed to retrieve issue from jira: " + id);
            }

            return defect;
        }

        /// <summary>
        /// Gets the defect from commit message.
        /// </summary>
        /// <param name="commitMessage">The commit message.</param>
        /// <returns>defect</returns>
        public Task<Defect> GetDefectFromCommitMessage(string commitMessage)
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
        private void PopulateStatistics(SonarRestService.Types.Issue issue)
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
        private List<string> GatherNotes(IList<SonarRestService.Types.Issue> issues, Resource project)
        {
            var notesList = new List<string>();
            this.NumberOfBlockers = 0;
            this.NumberOfCriticals = 0;
            this.NumberOfMajors = 0;
            this.TechnicalDebt = 0;

            StringBuilder header = new StringBuilder();
            header.AppendLine("h2. {color:#205081}Issues are pending resolution{color}");
            if (project != null)
            {
                header.AppendLine("Project : " + project.SolutionName + " : Key : " + project.Key);
            }
            else
            {
                header.AppendLine("Project : Multiple Projects");
            }

            var notesIssues = new StringBuilder();
            foreach (var issue in issues)
            {
                var compElelms = issue.Component.Split(':');
                notesIssues.AppendLine(this.GetLineForIssue(issue, compElelms));               
                this.PopulateStatistics(issue);

                if (notesIssues.ToString().Length > 32000)
                {
                    // reset string
                    notesList.Add(notesIssues.ToString());
                    notesIssues = new StringBuilder();
                }
            }

            notesList.Add(notesIssues.ToString());

            var finalNotes = new List<string>();
            foreach (var item in notesList)
            {
                finalNotes.Add(header.ToString() + item);
            }

            return finalNotes;
        }

		private string GetLineForIssue(SonarRestService.Types.Issue issue, string[] compElelms)
		{
			var url = "    [" + issue.Assignee + "] " + issue.Message + " : " + compElelms[compElelms.Length - 1] + " : " + issue.Line + " : ";
			var openIssueString = string.Format("[Open Issue|{0}/issues?issues={1}&open={1}]", this.userConf.Hostname.TrimEnd('/'), issue.Key);
			return url + openIssueString; 
		}

        /// <summary>
        /// load config
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="manager"></param>
        public void LoadConfigurationFile(string configFile, INotificationManager manager)
        {
            this.CustomFields.Clear();
            this.MandatoryFields.Clear();
            if (File.Exists(configFile))
            {
                try
                {
                    var lines = File.ReadAllLines(configFile);
                    foreach (var line in lines)
                    {
                        if (line.Contains(";"))
                        {
                            var elements = line.Split(';');

                            if (line.StartsWith("Custom;"))
                            {
                                if (!this.CustomFields.ContainsKey(elements[0]))
                                {
                                    this.CustomFields.Add(elements[1], elements[2]);
                                }

                                continue;
                            }

                            if (!this.MandatoryFields.ContainsKey(elements[0]))
                            {
                                this.MandatoryFields.Add(elements[0], elements[1]);
                            }
                        }
                    }

                    this.jiraEnabled = true;
                }
                catch (Exception ex)
                {
                    manager.ReportMessage("Jira integration disable, configuration file broken: " + ex.Message);
                    this.jiraEnabled = false;
                }
            }
            else
            {
                this.jiraEnabled = false;
            }
        }
    }
}