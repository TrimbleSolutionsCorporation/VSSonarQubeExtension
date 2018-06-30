// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JiraConnector.cs" company="Copyright © 2017 Trimble Solutions Corporation">
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

namespace JiraConnector
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    using Newtonsoft.Json;
    using VSSonarPlugins;

    /// <summary>
    /// authenticator to jira
    /// </summary>
    public class JiraConnector
    {
        /// <summary>
        /// The configuration file
        /// </summary>
        private readonly string configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "JiraSetup.cfg");

        /// <summary>
        /// The cookieinfo file
        /// </summary>
        private readonly string cookieinfoFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "jiracookie.txt");

        /// <summary>
        /// The execution path
        /// </summary>
        private readonly string executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty));

        /// <summary>
        /// The logger
        /// </summary>
        private readonly INotificationManager logger;

        /// <summary>
        /// The jira url
        /// </summary>
        private readonly JiraRestAPIMethods jiraRestAPIMethods;

        /// <summary>
        /// The found in version
        /// </summary>
        private string foundInVersion;

        /// <summary>
        /// The project
        /// </summary>
        private string project;

        /// <summary>
        /// The user name
        /// </summary>
        private string userName;

        /// <summary>
        /// The password
        /// </summary>
        private string password;

        /// <summary>
        /// The jcookie
        /// </summary>
        private string jcookie;

        /// <summary>
        /// The jira url
        /// </summary>
        private string jiraurl;

        /// <summary>
        /// The jira segment
        /// </summary>
        private string segment;

        /// <summary>
        /// The jira component
        /// </summary>
        private string components;

        /// <summary>
        /// The jira issuetype
        /// </summary>
        private string issuetype;

        /// <summary>
        /// The jira issue origin
        /// </summary>
        private string origin;

        /// <summary>
        /// The jira issue defectivesince
        /// </summary>
        private string defectivesince;

        /// <summary>
        /// The jira issue type
        /// </summary>
        private string type;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JiraConnector" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">logger</exception>
        public JiraConnector(INotificationManager logger)
        {
            try
            {
                if (File.Exists(this.configFile))
                {
                    this.GetServerAndProject();
                }

                this.jcookie = this.GetCookie(false);
                this.jiraRestAPIMethods = new JiraRestAPIMethods();
            }
            catch (Exception ex)
            {
                this.logger.ReportMessage("Failed to create connector: " + ex.Message);
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = logger;
        }

        /// <summary>
        /// Get url for the defect.
        /// </summary>
        /// <param name="defect">
        /// The user name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetUrlForDefect(string defect)
        {
           return this.jiraurl + "projects/" + this.project + "/issues/" + defect + "?filter=allissues";
        }

        /// <summary>
        /// Get cookie to authenticate to Jira.
        /// </summary>
        /// <param name="force">The force.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">logger</exception>
        public string GetCookie(bool force)
        {
            string cookie = string.Empty;
            if (File.Exists(this.cookieinfoFile))
            {
                cookie = File.ReadAllText(this.cookieinfoFile);
            }

            try
            {
                if (force || cookie == string.Empty)
                {
                    cookie = this.jiraRestAPIMethods.GetAuthenticationCookie(this.userName, this.password, this.jiraurl + "rest/auth/1/");
                    if (cookie == string.Empty)
                    {
                        this.logger.ReportMessage("Cannot get authentication cookie for Jira. Check the username and password in config file.");
                        return string.Empty;
                    }

                    if (File.Exists(this.cookieinfoFile))
                    {
                        File.Delete(this.cookieinfoFile);
                    }

                    File.WriteAllText(this.cookieinfoFile, cookie);
                }
            }
            catch(Exception e)
            {
                this.logger.ReportMessage("Failed to authentication: " + e.Message);
            }

            return cookie;
        }

        /// <summary>
        /// Create Jira issue.
        /// </summary>
        /// <param name="summary">The summary of the issue.</param>
        /// <param name="notes">The notes or the description to add to the issue.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        public string CreateIssue(string summary, string notes)
        {
            if(this.jcookie == string.Empty)
            {
                this.GetCookie(true);
            }

            var jsondata = this.CreateJsondata(summary, notes);
            var result = this.jiraRestAPIMethods.CreateIssue(this.jiraurl + "rest/api/2/", this.jcookie, jsondata);
            if (result[0].Result == "Unauthorized" || result[0].Result.Contains("Exception"))
            {
                this.jcookie = this.GetCookie(true);
                result = this.jiraRestAPIMethods.CreateIssue(this.jiraurl + "rest/api/2/", this.jcookie, jsondata);
            }

            if (result[0].Result != "Created")
            {
                return string.Empty;
            }

            return result[0].Newissueid;
        }

        /// <summary>
        /// Create Json data for Jira issue.
        /// </summary>
        /// <param name="summary">
        /// The summary of the issue.
        /// </param>
        /// <param name="notes">
        /// The notes or the description to add to the issue.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string CreateJsondata(string summary, string notes)
        {
            var data = new JIssue();
            data.fields.summary = summary;
            data.fields.project.key = this.project;
            data.fields.issuetype.name = this.issuetype;
            data.fields.description = notes;
            data.fields.customfield_12015.value = this.segment;
            data.fields.components.Add(new Components { name = this.components });
            data.fields.customfield_12106.value = this.origin;
            data.fields.customfield_12114.Add(new VersionFound { name = this.foundInVersion });
            data.fields.customfield_12104.value = this.defectivesince;
            data.fields.customfield_10312.value = this.type;
            string jsondata = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            return jsondata;
        }

        /// <summary>
        /// Get Jira issue.
        /// </summary>
        /// <param name="defectId">The defect identifier.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        public string GetIssue(string defectId)
        {
            string issue = string.Empty;
            try
            {
                if (this.jcookie == string.Empty)
                {
                    this.GetCookie(true);
                }

                issue = this.jiraRestAPIMethods.GetIssue(this.jcookie, this.jiraurl + "rest/api/2/issue/" + defectId);

                if (issue.Contains("Unauthorized"))
                {
                    this.jcookie = this.GetCookie(true);
                    issue = this.jiraRestAPIMethods.GetIssue(this.jcookie, this.jiraurl + "rest/api/2/issue/" + defectId);
                }
            }
            catch(Exception e)
            {
                this.logger.ReportMessage("Cannot get issue from jira " + defectId + e);
            }

            return issue;
        }

        /// <summary>
        /// Update Jira issue descrition.
        /// </summary>
        /// <param name="defectId">The defect identifier.</param>
        /// <param name="notes">The notes.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        public string UpdateIssueDescription(string defectId, string notes)
        {
            var update = new Update();
            update.update.description.Add(new Description { set = notes });
            string jsondata = JsonConvert.SerializeObject(update, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            string result = string.Empty;
            try
            {
                if (this.jcookie == string.Empty)
                {
                    this.GetCookie(true);
                }

                result = this.jiraRestAPIMethods.UpdateIssue(this.jiraurl + "rest/api/2/issue/" + defectId, this.jcookie, jsondata, defectId);
                if (result.Contains("Unauthorized"))
                {
                    this.jcookie = this.GetCookie(true);
                    result = this.jiraRestAPIMethods.UpdateIssue(this.jiraurl + "rest/api/2/issue/" + defectId, this.jcookie, jsondata, defectId);
                }

                Debug.WriteLine("Updated issue" + defectId + ".Result: " + result);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Cannot update issue " + defectId + e);
            }

            return result;
        }

        /// <summary>
        /// Update Jira issue Add comment.
        /// </summary>
        /// <param name="defectId">The defect identifier.</param>
        /// <param name="notes">The notes.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">logger</exception>
        public string UpdateIssue(string defectId, string notes)
        {
            string result = string.Empty;
            var comment = new Comment
            {
                body = notes
            };
            string jsondata = JsonConvert.SerializeObject(comment, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            try
            {
                result = this.jiraRestAPIMethods.AddComment(this.jiraurl + "rest/api/2/issue/" + defectId + "/", this.jcookie, jsondata, defectId);
                if (result.Contains("Unauthorized"))
                {
                    this.jcookie = this.GetCookie(true);
                    result = this.jiraRestAPIMethods.AddComment(this.jiraurl + "rest/api/2/issue/", this.jcookie, notes, defectId);
                }

                this.logger.ReportMessage("Updated issue" + defectId + ".Result: " + result);
            }
            catch (Exception e)
            {
                this.logger.ReportMessage("Cannot update issue " + defectId + e);
            }

            return result;
        }

        /// <summary>
        /// Gets the server and project.
        /// </summary>
        private void GetServerAndProject()
        {
            foreach (var line in File.ReadAllLines(this.configFile))
            {
                var elems = line.Split(';');

                if (line.Trim().StartsWith("UserName"))
                {
                    try
                    {
                        this.userName = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                if (line.Trim().StartsWith("Password"))
                {
                    try
                    {
                        this.password = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                if (line.Trim().StartsWith("JiraURL"))
                {
                    try
                    {
                        this.jiraurl = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                if (line.Trim().StartsWith("JiraProject"))
                {
                    try
                    {
                        this.project = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                if (line.Trim().StartsWith("FoundInVersion"))
                {
                    try
                    {
                        this.foundInVersion = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        this.foundInVersion = "Work";
                    }
                }

                if (line.Trim().StartsWith("Segment"))
                {
                    try
                    {
                        this.segment = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        this.segment = "Product Development";
                    }
                }

                if (line.Trim().StartsWith("Components"))
                {
                    try
                    {
                        this.components = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        this.components = "PD internal - Code Analysis";
                    }
                }

                if (line.Trim().StartsWith("IssueType"))
                {
                    try
                    {
                        this.issuetype = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        this.issuetype = "Technical Debt";
                    }
                }

                if (line.Trim().StartsWith("Origin"))
                {
                    try
                    {
                        this.origin = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        this.origin = "SonarQube";
                    }
                }

                if (line.Trim().StartsWith("DefectiveSince"))
                {
                    try
                    {
                        this.defectivesince = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                if (line.Trim().StartsWith("Type"))
                {
                    try
                    {
                        this.type = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        this.origin = "Technical debt";
                    }
                }
            }
        }
        #endregion
    }
}
