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
    using ViewModel;
    using ViewModel.Analysis;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// Search model for issues, viewmodel IssuesSearchViewModel
    /// </summary>
    public class IssuesSearchModel : IAnalysisModelBase
    {
        /// <summary>
        ///     The vs helper.
        /// </summary>
        private IVsEnvironmentHelper visualStudioHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuesSearchModel" /> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="restService">The rest service.</param>
        public IssuesSearchModel(
            SonarQubeViewModel model,
            IConfigurationHelper configurationHelper,
            ISonarRestService restService)
        {
            this.SonarQubeViewModel = model;
            this.ConfigurationHelper = configurationHelper;
            this.RestService = restService;
            this.IssuesSearchViewModel = new IssuesSearchViewModel(this);
        }

        /// <summary>
        ///     The analysis mode has change.
        /// </summary>
        public event ChangedEventHandler IssuesReadyForCollecting;

        /// <summary>
        ///     Gets or sets the configuration.
        /// </summary>
        public ISonarConfiguration Configuration { get; set; }

        /// <summary>
        ///     Gets or sets the status bar.
        /// </summary>
        public IVSSStatusBar StatusBar { get; set; }

        /// <summary>
        ///     Gets or sets the service provier.
        /// </summary>
        public IServiceProvider ServiceProvier { get; set; }

        /// <summary>
        /// Gets the sonar qube view model.
        /// </summary>
        /// <value>
        /// The sonar qube view model.
        /// </value>
        public SonarQubeViewModel SonarQubeViewModel { get; private set; }

        /// <summary>
        /// Gets the configuration helper.
        /// </summary>
        /// <value>
        /// The configuration helper.
        /// </value>
        public IConfigurationHelper ConfigurationHelper { get; private set; }

        /// <summary>
        ///     Gets or sets the associated project.
        /// </summary>
        public Resource AssociatedProject { get; set; }

        /// <summary>
        /// Gets the rest service.
        /// </summary>
        /// <value>
        /// The rest service.
        /// </value>
        public ISonarRestService RestService { get; private set; }
        public IssuesSearchViewModel IssuesSearchViewModel { get; private set; }

        /// <summary>The update services.</summary>
        /// <param name="restServiceIn">The rest service in.</param>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="configurationHelperIn">The configuration Helper.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public void UpdateServices(
            ISonarRestService restServiceIn,
            IVsEnvironmentHelper vsenvironmenthelperIn,
            IConfigurationHelper configurationHelperIn,
            IVSSStatusBar statusBar,
            IServiceProvider provider)
        {
            this.RestService = restServiceIn;
            this.visualStudioHelper = vsenvironmenthelperIn;
            this.ConfigurationHelper = configurationHelperIn;
            this.StatusBar = statusBar;
            this.ServiceProvier = provider;

            this.IssuesSearchViewModel.IssuesGridView.Vsenvironmenthelper = vsenvironmenthelperIn;
            this.IssuesSearchViewModel.IssuesGridView.UpdateVsService(this.visualStudioHelper);
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
            if (this.IssuesSearchViewModel.CanQUeryIssues)
            {
                return
                    this.IssuesSearchViewModel.IssuesGridView.Issues.Where(
                        issue =>
                        this.IssuesSearchViewModel.IssuesGridView.IsNotFiltered(issue) && file.Key.Equals(issue.Component)).ToList();
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
            this.IssuesSearchViewModel.IssuesGridView.Issues.Clear();
            this.AssociatedProject = null;
            this.IssuesSearchViewModel.CanQUeryIssues = false;
        }

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="associatedProject">
        /// The associated project.
        /// </param>
        /// <param name="sonarCubeConfiguration">
        /// The sonar cube configuration.
        /// </param>
        /// <param name="workingDir">
        /// The working dir.
        /// </param>
        public void InitDataAssociation(Resource associatedProject, ISonarConfiguration sonarCubeConfiguration, string workingDir)
        {
            this.AssociatedProject = associatedProject;
            this.Configuration = sonarCubeConfiguration;
            this.IssuesSearchViewModel.CanQUeryIssues = true;
            this.SonarQubeViewModel.IsExtensionBusy = false;

            List<User> usortedList = this.RestService.GetUserList(sonarCubeConfiguration);
            if (usortedList != null && usortedList.Count > 0)
            {
                this.IssuesSearchViewModel.UsersList = new ObservableCollection<User>(usortedList.OrderBy(i => i.Login));
            }

            List<SonarActionPlan> usortedListofPlan = this.RestService.GetAvailableActionPlan(sonarCubeConfiguration, associatedProject.Key);
            if (usortedListofPlan != null && usortedListofPlan.Count > 0)
            {
                this.IssuesSearchViewModel.AvailableActionPlans = new ObservableCollection<SonarActionPlan>(usortedListofPlan.OrderBy(i => i.Name));
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
            this.IssuesSearchViewModel.DocumentInView = documentInView;
            this.IssuesSearchViewModel.ResourceInEditor = fullName;
            this.IssuesSearchViewModel.OnSelectedViewChanged();
        }

        /// <summary>
        /// Reset Stats.
        /// </summary>
        public void ResetStats()
        {
            if (this.IssuesSearchViewModel.IssuesGridView != null)
            {
                this.IssuesSearchViewModel.IssuesGridView.ResetStatistics();
            }
        }

        /// <summary>
        /// Gets all issues in project.
        /// </summary>
        /// <returns>all issues in project</returns>
        public IEnumerable<Issue> GetAllIssuesInProject()
        {
            return this.RestService.GetIssuesForProjects(this.Configuration, this.AssociatedProject.Key);
        }

        /// <summary>
        /// Gets the user issues in project.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public IEnumerable<Issue> GetUserIssuesInProject(string userName)
        {
            return this.RestService.GetIssuesByAssigneeInProject(this.Configuration, this.AssociatedProject.Key, userName);
        }

        /// <summary>
        /// Gets the user issues in project.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public IEnumerable<Issue> GetUserIssues(string userName)
        {
            return this.RestService.GetAllIssuesByAssignee(this.Configuration, userName);
        }

        public IEnumerable<Issue> GetIssuesSinceSinceDate(DateTime date)
        {
            return this.RestService.GetIssuesForProjectsCreatedAfterDate(
                                this.Configuration,
                                this.AssociatedProject.Key, date);
        }

                                    
        /// <summary>
        /// The retrieve issues using current filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>all issues for requested filter</returns>
        public IEnumerable<Issue> GetIssuesUsingFilter(string filter)
        {
            if (this.SonarQubeViewModel.SonarVersion < 3.6)
            {
                return this.RestService.GetIssuesForProjects(this.Configuration, this.AssociatedProject.Key);
            }

            string request = "?componentRoots=" + this.AssociatedProject.Key + filter;
            return this.RestService.GetIssues(this.Configuration, request, this.AssociatedProject.Key);
        }
    }
}
