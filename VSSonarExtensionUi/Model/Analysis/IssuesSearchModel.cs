﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssuesSearchModel.cs" company="Copyright © 2015 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarExtensionUi.Model.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Association;
    using Helpers;
    using SonarRestService.Types;
    using SonarRestService;
    using SonarLocalAnalyser;
    using ViewModel;
    using ViewModel.Analysis;
    using VSSonarExtensionUi.ViewModel.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using System.Windows;

    /// <summary>
    /// Search model for issues, viewmodel IssuesSearchViewModel
    /// </summary>
    public class IssuesSearchModel : IAnalysisModelBase, IModelBase
    {
        /// <summary>
        /// The ct
        /// </summary>
        private CancellationTokenSource ct;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        /// The rest service
        /// </summary>
        private readonly ISonarRestService restService;

        /// <summary>
        /// The issues search view model
        /// </summary>
        private readonly IssuesSearchViewModel issuesSearchViewModel;

        /// <summary>
        /// The notificationmanager
        /// </summary>
        private readonly INotificationManager logger;

        /// <summary>
        /// The key translator
        /// </summary>
        private readonly ISQKeyTranslator keyTranslator;

        /// <summary>
        ///     The vs helper.
        /// </summary>
        private IVsEnvironmentHelper visualStudioHelper;

        /// <summary>
        /// The status bar
        /// </summary>
        private IVSSStatusBar statusBar;

        /// <summary>
        /// The service provier
        /// </summary>
        private IServiceProvider serviceProvier;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// The source model
        /// </summary>
        private ISourceControlProvider sourceModel;

        /// <summary>
        /// The available projects
        /// </summary>
        private IEnumerable<Resource> availableProjects;

        /// <summary>
        /// The rest source model
        /// </summary>
        private readonly ISourceControlProvider restSourceModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuesSearchModel" /> class.
        /// </summary>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="restService">The rest service.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="translator">The translator.</param>
        /// <param name="analyser">The analyser.</param>
        public IssuesSearchModel(
            IConfigurationHelper configurationHelper,
            ISonarRestService restService,
            INotificationManager manager,
            ISQKeyTranslator translator,
            ISonarLocalAnalyser analyser,
            IList<IIssueTrackerPlugin> issuetrackerplugins)
        {
            this.keyTranslator = translator;
            this.logger = manager;
            this.configurationHelper = configurationHelper;
            this.restService = restService;
            this.issuesSearchViewModel = new IssuesSearchViewModel(this, manager, this.configurationHelper, restService, translator, analyser, issuetrackerplugins);
            this.restSourceModel = new RestSourceControlModel(manager, restService);
            AssociationModel.RegisterNewModelInPool(this);
        }

        /// <summary>
        ///     The analysis mode has change.
        /// </summary>
        public event ChangedEventHandler IssuesReadyForCollecting;

        /// <summary>
        /// The update services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBarIn">The status bar in.</param>
        /// <param name="provider">The provider.</param>
        public async Task UpdateServices(
            IVsEnvironmentHelper vsenvironmenthelperIn,
            IVSSStatusBar statusBarIn,
            IServiceProvider provider)
        {
            await Task.Delay(0);
            this.visualStudioHelper = vsenvironmenthelperIn;
            this.statusBar = statusBarIn;
            this.serviceProvier = provider;
            this.issuesSearchViewModel.UpdateServices(this.visualStudioHelper);
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        /// <param name="availableProjectsIn">The available projects in.</param>
        public async Task OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjectsIn, IList<IIssueTrackerPlugin> sourcePlugin)
        {
            // does nothing
            this.availableProjects = availableProjectsIn;
            this.issuesSearchViewModel.AvailableProjects = availableProjectsIn;
            this.issuesSearchViewModel.CanQUeryIssues = true;
            this.logger.EndedWorking();
            await this.RefreshUsersData();
        }

        private async Task AugmentUserInformationWithTeams(ObservableCollection<User> assigneeList)
        {
            var userTeamsFile = this.configurationHelper.ReadSetting(
                Context.GlobalPropsId,
                OwnersId.ApplicationOwnerId,
                GlobalIds.TeamsFile);
            if (userTeamsFile == null)
            {
                this.logger.ReportMessage("username not configured in settings");
                return;
            }


            try
            {
                List<Team> usortedListTeams = await this.restService.GetTeams(assigneeList, userTeamsFile.Value);
                this.issuesSearchViewModel.Teams.Clear();
                foreach (var team in usortedListTeams.OrderBy(i => i.Name))
                {
                    this.issuesSearchViewModel.Teams.Add(team);
                }
            }
            catch (Exception ex)
            {
                this.logger.ReportMessage("Failed to update teams: " + ex.Message);
            }
        }

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="sourceModelIn">The source model in.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        /// <param name="profile">The profile.</param>
        public async Task AssociateWithNewProject(
            Resource project,
            string workingDir,
            ISourceControlProvider sourceModelIn,
            Dictionary<string, Profile> profile,
            string visualStudioVersion)
        {
            this.sourceModel = sourceModelIn;
            this.associatedProject = project;
            this.issuesSearchViewModel.CanQUeryIssues = true;
            this.logger.EndedWorking();

            List<User> usortedList = await this.restService.GetUserList(AuthtenticationHelper.AuthToken);
            if (usortedList != null && usortedList.Count > 0)
            {
                this.issuesSearchViewModel.AssigneeList = new ObservableCollection<User>(usortedList.OrderBy(i => i.Name));
            }
        }

        /// <summary>
        /// The get issues for resource.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="fileContent">The file content.</param>
        /// <param name="shownfalseandresolved">The shown false and resolved.</param>
        /// <returns>
        /// The
        /// <see><cref>List</cref></see>
        /// .
        /// </returns>
        public async Task<Tuple<List<Issue>, bool>> GetIssuesForResource(Resource file, string fileContent, bool fromEditor)
        {
            if (this.issuesSearchViewModel.CanQUeryIssues)
            {
                await Task.Delay(0);
                return new Tuple<List<Issue>, bool>(
                        this.issuesSearchViewModel.IssuesGridView.Issues.Where(
                            issue =>
                            this.issuesSearchViewModel.IssuesGridView.IsNotFiltered(issue) && file.Key.Equals(issue.Component)).ToList(), false);
            }

            return new Tuple<List<Issue>, bool>(new List<Issue>(), false);
        }

        /// <summary>The trigger a project analysis.</summary>
        /// <param name="project">The project.</param>
        public void TriggerAProjectAnalysis(VsProjectItem project)
        {
            // is not a analyser
        }

        /// <summary>
        ///     The end data association.
        /// </summary>
        public async Task OnSolutionClosed()
        {
            this.ClearIssues();
            this.associatedProject = null;
            this.issuesSearchViewModel.CanQUeryIssues = false;
            await Task.Delay(0);
        }

        /// <summary>
        /// The on changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        public void RefreshIssuesEditor(EventArgs e)
        {
            if (this.IssuesReadyForCollecting != null)
            {
                this.IssuesReadyForCollecting(this, e);
            }
        }

        /// <summary>
        /// The refresh data for resource.
        /// </summary>
        /// <param name="fullName">
        /// The full name.
        /// </param>
        /// <param name="documentInView">
        /// The document in view.
        /// </param>
        public void RefreshDataForResource(Resource fullName, string documentInView, string content, bool fromSource)
        {
            this.issuesSearchViewModel.DocumentInView = documentInView;
            this.issuesSearchViewModel.ResourceInEditor = fullName;
            this.issuesSearchViewModel.OnSelectedViewChanged();
        }

        /// <summary>
        /// Reset Stats.
        /// </summary>
        public void ResetStats()
        {
            if (this.issuesSearchViewModel.IssuesGridView != null)
            {
                this.issuesSearchViewModel.IssuesGridView.ResetStatistics();
            }
        }

        /// <summary>
        /// Gets all issues in project.
        /// </summary>
        /// <returns>all issues in project</returns>
        public async Task<IEnumerable<Issue>> GetAllIssuesInProject()
        {
            if (this.associatedProject == null)
            {
                this.logger.FlagFailure("Feature available only when solution is open.");
                return new List<Issue>();
            }

            this.CreateNewTokenOrUseOldOne();
            return await this.restService.GetIssuesForProjects(
                AuthtenticationHelper.AuthToken,
                this.associatedProject.Key,
                this.ct.Token,
                this.logger);
        }

        /// <summary>
        /// Gets the user issues in project.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        /// returs all issues in project
        /// </returns>
        public async Task<IEnumerable<Issue>> GetUserIssuesInProject(string userName)
        {
            if (this.associatedProject == null)
            {
                this.logger.FlagFailure("Feature available only when solution is open.");
                return new List<Issue>();
            }

            this.CreateNewTokenOrUseOldOne();
            return await this.restService.GetIssuesByAssigneeInProject(
                AuthtenticationHelper.AuthToken,
                this.associatedProject.Key,
                userName,
                this.ct.Token,
                this.logger);
        }

        /// <summary>
        /// Gets the user issues in project.
        /// </summary>
        /// <returns>
        /// returs all issues in project for current user
        /// </returns>
        public async Task<IEnumerable<Issue>> GetCurrentUserIssuesInProject()
        {
            if (this.associatedProject == null)
            {
                this.logger.FlagFailure("Feature available only when solution is open.");
                return new List<Issue>();
            }

            this.CreateNewTokenOrUseOldOne();
            return await this.restService.GetIssuesByAssigneeInProject(
                AuthtenticationHelper.AuthToken,
                this.associatedProject.Key,
                AuthtenticationHelper.AuthToken.Username,
                this.ct.Token,
                this.logger);
        }

        /// <summary>
        /// Gets the user issues in project.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        /// returns all users issues
        /// </returns>
        public async Task<IEnumerable<Issue>> GetUserIssues(string userName)
        {
            this.CreateNewTokenOrUseOldOne();
            return await this.restService.GetAllIssuesByAssignee(
                AuthtenticationHelper.AuthToken,
                userName,
                this.ct.Token,
                this.logger);
        }

        /// <summary>
        /// Gets the current user issues.
        /// </summary>
        /// <returns>gets current logged user issues</returns>
        public async Task<IEnumerable<Issue>> GetCurrentUserIssues()
        {
            var userLoginValue = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.UserLogin);
            if (userLoginValue == null)
            {
                this.logger.ReportMessage("username not configured in settings");
                return new List<Issue>();
            }

            return await GetUserIssues(userLoginValue.Value);
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>
        /// returns view model
        /// </returns>
        public object GetViewModel()
        {
            return this.issuesSearchViewModel;
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public async Task OnDisconnect()
        {
            // not used
            await Task.Delay(0);
        }

        /// <summary>
        /// Gets the issues since since date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>
        /// returns issues since a date
        /// </returns>
        public async Task<IEnumerable<Issue>> GetIssuesSinceSinceDate(DateTime date)
        {
            this.CreateNewTokenOrUseOldOne();
            return await this.restService.GetIssuesForProjectsCreatedAfterDate(
                AuthtenticationHelper.AuthToken,
                this.associatedProject.Key,
                date,
                this.ct.Token,
                this.logger);
        }

        /// <summary>
        /// Gets the issues since last project date.
        /// </summary>
        /// <returns>returns issues since last project update date</returns>
        public async Task<IEnumerable<Issue>> GetIssuesSinceLastProjectDate()
        {
            this.CreateNewTokenOrUseOldOne();
            return await this.restService.GetIssuesForProjectsCreatedAfterDate(
                AuthtenticationHelper.AuthToken, 
                this.associatedProject.Key, 
                this.associatedProject.Date, 
                this.ct.Token,
                this.logger);
        }

        /// <summary>
        /// The retrieve issues using current filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="filterSSCM">if set to <c>true</c> [filter SSCM].</param>
        /// <param name="componentsSet">if set to <c>true</c> [components set].</param>
        /// <returns>
        /// all issues for requested filter and if query was canceled by user
        /// </returns>
        public async Task<Tuple<IEnumerable<Issue>, bool>> GetIssuesUsingFilter(string filter, bool filterSSCM, bool componentsSet)
        {
            this.logger.ResetFailure();
            string request = "?" + filter;
            string key = string.Empty;
            if (this.associatedProject != null && !componentsSet)
            {
                request = "?componentRoots=" + this.associatedProject.Key + filter;
                key = this.associatedProject.Key;
            }

            this.CreateNewTokenOrUseOldOne();
            var issues = await this.restService.GetIssues(
                AuthtenticationHelper.AuthToken,
                request,
                key,
                this.ct.Token,
                this.logger);
            if (!filterSSCM)
            {
                return new Tuple<IEnumerable<Issue>, bool>(issues, this.ct.IsCancellationRequested);
            }

            return new Tuple<IEnumerable<Issue>, bool>(await this.FilterIssuesBySSCM(issues), this.ct.IsCancellationRequested);
        }

        /// <summary>
        /// Clears the issues.
        /// </summary>
        public void ClearIssues()
        {
            this.issuesSearchViewModel.IssuesGridView.ResetStatistics();
            this.issuesSearchViewModel.IssuesGridView.Issues.Clear();
            this.issuesSearchViewModel.IssuesGridView.AllIssues.Clear();
        }


        /// <summary>
        /// Filters the issues by SSCM.
        /// </summary>
        /// <param name="issues">The issues.</param>
        /// <returns>Returns all filtered issues</returns>
        public async Task<IEnumerable<Issue>> FilterIssuesBySSCM(List<Issue> issues)
        {
            var filteredIssues = new List<Issue>();

            foreach (var issue in issues)
            {
                var issueFiltered = await this.FilterIssuesBySSCM(issue);
                if (issueFiltered != null)
                {
                    filteredIssues.Add(issueFiltered);
                }
            }

            return filteredIssues;
        }

        private void CreateNewTokenOrUseOldOne()
        {
            if (this.ct == null || this.ct.IsCancellationRequested)
            {
                this.ct = new CancellationTokenSource();
            }
        }

        /// <summary>
        /// Filters the issues by SSCM.
        /// </summary>
        /// <param name="issue">The issue.</param>
        /// <returns>
        /// Returns issue.
        /// </returns>
        private async Task<Issue> FilterIssuesBySSCM(Issue issue)
        {
            // file level issues are returned regardless
            if (issue.Line == 0 || issue.Line == -1)
            {
                return issue;
            }

            try
            {
                var blameLine = await this.restSourceModel.GetBlameByLine(new Resource { Key = issue.Component }, issue.Line);
                if (blameLine != null)
                {
                    if (blameLine.Date < this.issuesSearchViewModel.CreatedSinceDate)
                    {
                        return null;
                    }
                }
                else
                {
                    if (this.associatedProject == null)
                    {
                        this.logger.ReportMessage(
                            new Message
                            {
                                Id = "IssuesSearchModel",
                                Data = "Blame info not available in server, local blame available only associated : " + issue.Component + " : " + issue.Line
                            });

                        return issue;
                    }

                    var translatedPath = string.Empty;

                    try
                    {
                        translatedPath = this.keyTranslator.TranslateKey(issue.Component, this.visualStudioHelper, this.associatedProject.BranchName);

                        if (!File.Exists(translatedPath))
                        {
                            var message = "Search Model Failed : Translator Failed:  Key : " +
                                issue.Component + " - Path : " + translatedPath + " - KeyType : " + this.keyTranslator.GetLookupType().ToString();
                            this.logger.ReportMessage(new Message { Id = "IssuesSearchModel", Data = message });
                            return issue;
                        }
                    }
                    catch (Exception ex)
                    {
                        IssueGridViewModel.ReportTranslationException(issue, translatedPath, this.logger, this.restService, this.associatedProject, ex);
                        return issue;
                    }

                    var blameLocalLine = await this.sourceModel.GetBlameByLine(translatedPath, issue.Line);
                    if (blameLocalLine != null)
                    {
                        if (blameLocalLine.Date < this.issuesSearchViewModel.CreatedSinceDate)
                        {
                            return null;
                        }
                    }

                    this.logger.ReportMessage(
                        new Message
                        {
                            Id = "IssuesSearchModel",
                            Data = "Blame Failed, Filtering Not Available for : " + translatedPath + " : " + issue.Line
                        });
                }
            }
            catch (Exception ex)
            {
                this.logger.ReportMessage(
                    new Message
                    {
                        Id = "IssuesSearchModel",
                        Data = "Blame throw exception, please report: " + ex.Message + " : " + issue.Component
                    });

                this.logger.ReportException(ex);
            }
            
            return issue;
        }
        
        internal async Task RefreshUsersData()
        {
            List<User> usortedList = await this.restService.GetUserList(AuthtenticationHelper.AuthToken);
            if (usortedList != null && usortedList.Count > 0)
            {
                this.issuesSearchViewModel.AssigneeList = new ObservableCollection<User>(usortedList.OrderBy(i => i.Name));
                await this.AugmentUserInformationWithTeams(this.issuesSearchViewModel.AssigneeList);
            }
        }

        internal async Task<bool> LoadAndSaveTeams(string fileName)
        {
            if (!File.Exists(fileName))
            {
                this.logger.ReportMessage("Teams File Not Found: " + fileName);
                return false;
            }

            try
            {
                await this.AugmentUserInformationWithTeams(this.issuesSearchViewModel.AssigneeList);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid xml: " + ex.Message);
                return false;
            }

            this.configurationHelper.WriteSetting(
                Context.GlobalPropsId,
                OwnersId.ApplicationOwnerId,
                GlobalIds.TeamsFile, fileName, true);
        
            return true;
        }

        /// <summary>
        /// Cancels the query.
        /// </summary>
        internal void CancelQuery()
        {
            if (this.ct == null)
            {
                return;
            }

            this.ct.Cancel();
            this.restService.CancelRequest();
        }
    }
}
