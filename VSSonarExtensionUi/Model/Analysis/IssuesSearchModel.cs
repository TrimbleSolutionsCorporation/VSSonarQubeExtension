// --------------------------------------------------------------------------------------------------------------------
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
    using System.Linq;
    using Helpers;

    using SonarLocalAnalyser;
    using ViewModel;
    using ViewModel.Analysis;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    

    /// <summary>
    /// Search model for issues, viewmodel IssuesSearchViewModel
    /// </summary>
    public class IssuesSearchModel : IAnalysisModelBase, IModelBase
    {
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
        private readonly INotificationManager notificationmanager;

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
        /// The user conf
        /// </summary>
        private ISonarConfiguration userConf;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuesSearchModel" /> class.
        /// </summary>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="restService">The rest service.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="translator">The translator.</param>
        public IssuesSearchModel(
            IConfigurationHelper configurationHelper,
            ISonarRestService restService,
            INotificationManager manager,
            ISQKeyTranslator translator)
        {
            this.notificationmanager = manager;
            this.configurationHelper = configurationHelper;
            this.restService = restService;
            this.issuesSearchViewModel = new IssuesSearchViewModel(this, manager, this.configurationHelper, restService, translator);

            SonarQubeViewModel.RegisterNewModelInPool(this);
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
        public void UpdateServices(
            IVsEnvironmentHelper vsenvironmenthelperIn,
            IVSSStatusBar statusBarIn,
            IServiceProvider provider)
        {
            this.visualStudioHelper = vsenvironmenthelperIn;
            this.statusBar = statusBarIn;
            this.serviceProvier = provider;
        }

        /// <summary>
        /// The get issues for resource.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="fileContent">
        /// The file content.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public List<Issue> GetIssuesForResource(Resource file, string fileContent)
        {
            if (this.issuesSearchViewModel.CanQUeryIssues)
            {
                return
                    this.issuesSearchViewModel.IssuesGridView.Issues.Where(
                        issue =>
                        this.issuesSearchViewModel.IssuesGridView.IsNotFiltered(issue) && file.Key.Equals(issue.Component)).ToList();
            }

            return new List<Issue>();
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
        public void EndDataAssociation()
        {
            this.issuesSearchViewModel.IssuesGridView.Issues.Clear();
            this.associatedProject = null;
            this.issuesSearchViewModel.CanQUeryIssues = false;
        }

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        public void AssociateWithNewProject(ISonarConfiguration config, Resource project, string workingDir)
        {
            this.associatedProject = project;
            this.userConf = config;
            this.issuesSearchViewModel.CanQUeryIssues = true;
            this.notificationmanager.EndedWorking();

            List<User> usortedList = this.restService.GetUserList(config);
            if (usortedList != null && usortedList.Count > 0)
            {
                this.issuesSearchViewModel.UsersList = new ObservableCollection<User>(usortedList.OrderBy(i => i.Login));
            }

            List<SonarActionPlan> usortedListofPlan = this.restService.GetAvailableActionPlan(config, this.associatedProject.Key);
            if (usortedListofPlan != null && usortedListofPlan.Count > 0)
            {
                this.issuesSearchViewModel.AvailableActionPlans = new ObservableCollection<SonarActionPlan>(usortedListofPlan.OrderBy(i => i.Name));
            }
        }

        /// <summary>
        /// The on changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        public void OnAnalysisModeHasChange(EventArgs e)
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
        public void RefreshDataForResource(Resource fullName, string documentInView)
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
        public IEnumerable<Issue> GetAllIssuesInProject()
        {
            return this.restService.GetIssuesForProjects(this.userConf, this.associatedProject.Key);
        }

        /// <summary>
        /// Gets the user issues in project.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        /// returs all issues in project
        /// </returns>
        public IEnumerable<Issue> GetUserIssuesInProject(string userName)
        {
            return this.restService.GetIssuesByAssigneeInProject(this.userConf, this.associatedProject.Key, userName);
        }

        /// <summary>
        /// Gets the user issues in project.
        /// </summary>
        /// <returns>
        /// returs all issues in project for current user
        /// </returns>
        public IEnumerable<Issue> GetCurrentUserIssuesInProject()
        {
            return this.restService.GetIssuesByAssigneeInProject(this.userConf, this.associatedProject.Key, this.userConf.Username);
        }

        /// <summary>
        /// Gets the user issues in project.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>
        /// returns all users issues
        /// </returns>
        public IEnumerable<Issue> GetUserIssues(string userName)
        {
            return this.restService.GetAllIssuesByAssignee(this.userConf, userName);
        }

        /// <summary>
        /// Gets the current user issues.
        /// </summary>
        /// <returns>gets current logged user issues</returns>
        public IEnumerable<Issue> GetCurrentUserIssues()
        {
            return this.restService.GetAllIssuesByAssignee(this.userConf, this.userConf.Username);
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
        /// Gets the issues since since date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>
        /// returns issues since a date
        /// </returns>
        public IEnumerable<Issue> GetIssuesSinceSinceDate(DateTime date)
        {
            return this.restService.GetIssuesForProjectsCreatedAfterDate(this.userConf, this.associatedProject.Key, date);
        }

        /// <summary>
        /// Gets the issues since last project date.
        /// </summary>
        /// <returns>returns issues since last project update date</returns>
        public IEnumerable<Issue> GetIssuesSinceLastProjectDate()
        {
            return this.restService.GetIssuesForProjectsCreatedAfterDate(this.userConf, this.associatedProject.Key, this.associatedProject.Date);
        }

        /// <summary>
        /// The retrieve issues using current filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>all issues for requested filter</returns>
        public IEnumerable<Issue> GetIssuesUsingFilter(string filter)
        {
            if (AuthtenticationHelper.AuthToken.SonarVersion < 3.6)
            {
                return this.restService.GetIssuesForProjects(this.userConf, this.associatedProject.Key);
            }

            string request = "?componentRoots=" + this.associatedProject.Key + filter;
            return this.restService.GetIssues(this.userConf, request, this.associatedProject.Key);
        }

        /// <summary>
        /// Clears the issues.
        /// </summary>
        public void ClearIssues()
        {
            this.issuesSearchViewModel.IssuesGridView.Issues.Clear();
            this.issuesSearchViewModel.IssuesGridView.AllIssues.Clear();
        }
    }
}
