﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssuesSearchViewModel.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtensionUi.ViewModel.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    using GalaSoft.MvvmLight.Command;
    using Helpers;

    using Model.Analysis;
    using Model.Helpers;
    using Model.Menu;
    using PropertyChanged;
    using SonarLocalAnalyser;
    using View.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using SonarRestService.Types;
    using SonarRestService;
    using System.Windows.Forms;

    /// <summary>
    /// The issues search view model.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class IssuesSearchViewModel : IViewModelBase
    {
        /// <summary>
        ///     The issues filter view model key.
        /// </summary>
        private const string IssuesFilterViewModelKey = "IssuesFilterViewModel";

        /// <summary>
        /// The component list
        /// </summary>
        private readonly List<Resource> componentList = new List<Resource>();

        /// <summary>
        /// The search model
        /// </summary>
        private readonly IssuesSearchModel searchModel;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        /// The notification manager
        /// </summary>
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// The rest service
        /// </summary>
        private readonly ISonarRestService restService;

        /// <summary>
        /// The saved search model
        /// </summary>
        private readonly SearchModel savedSearchModel;

        /// <summary>
        /// The vshelper
        /// </summary>
        private IVsEnvironmentHelper vshelper;
        private bool isComponentWindowLaunched;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuesSearchViewModel" /> class.
        /// </summary>
        /// <param name="searchModel">The search model.</param>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="restService">The rest service.</param>
        /// <param name="translator">The translator.</param>
        /// <param name="analyser">The analyser.</param>
        public IssuesSearchViewModel(
            IssuesSearchModel searchModel,
            INotificationManager notificationManager,
            IConfigurationHelper configurationHelper,
            ISonarRestService restService,
            ISQKeyTranslator translator,
            ISonarLocalAnalyser analyser,
            IList<IIssueTrackerPlugin> plugins)
        {
            this.restService = restService;
            this.notificationManager = notificationManager;
            this.configurationHelper = configurationHelper;
            this.searchModel = searchModel;
            this.Header = "Issues Search";
            this.AvailableProjects = new List<Resource>();
            this.AssigneeList = new ObservableCollection<User>();
            this.Teams = new ObservableCollection<Team>();

            this.AvailableSearches = new ObservableCollection<string>();
            this.IssuesGridView = new IssueGridViewModel("SearchView", false, configurationHelper, restService, notificationManager, translator);
            this.savedSearchModel = new SearchModel(this.AvailableSearches);
            this.IssuesGridView.ContextMenuItems = this.CreateRowContextMenu(restService, translator, analyser, plugins);
            this.IssuesGridView.ShowContextMenu = true;
            this.IssuesGridView.ShowLeftFlyoutEvent += this.ShowHideLeftFlyout;
            this.SizeOfFlyout = 0;
            this.InitCommanding();

            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
            this.CreatedBeforeDate = DateTime.Now;
            this.CreatedSinceDate = DateTime.Now;

            SonarQubeViewModel.RegisterNewViewModelInPool(this);
        }

        internal void UpdateServices(IVsEnvironmentHelper visualStudioHelper)
        {
            this.vshelper = visualStudioHelper;
        }

        /// <summary>
        /// Gets or sets the selected search.
        /// </summary>
        /// <value>
        /// The selected search.
        /// </value>
        public string SelectedSearch { get; set; }

        /// <summary>
        /// Gets the available searches.
        /// </summary>
        /// <value>
        /// The available searches.
        /// </value>
        public ObservableCollection<string> AvailableSearches { get; private set; }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can q uery issues.
        /// </summary>
        public bool CanQUeryIssues { get; set; }

        /// <summary>
        ///     Gets the close flyout issue search command.
        /// </summary>
        public RelayCommand CloseLeftFlyoutCommand { get; private set; }

        /// <summary>
        /// Gets the cancel query command.
        /// </summary>
        /// <value>
        /// The cancel query command.
        /// </value>
        public ICommand CancelQueryCommand { get; private set; }

        /// <summary>
        /// commad 
        /// </summary>
        public ICommand LoadTeamsFileCommand { get; private set; }

        /// <summary>
        /// Gets the launch compo search dialog command.
        /// </summary>
        /// <value>
        /// The launch compo search dialog command.
        /// </value>
        public ICommand LaunchCompoSearchDialogCommand { get; private set; }

        /// <summary>
        /// Gets the load saved search command.
        /// </summary>
        /// <value>
        /// The load saved search command.
        /// </value>
        public ICommand LoadSavedSearchCommand { get; private set; }

        /// <summary>
        /// Gets the save search command.
        /// </summary>
        /// <value>
        /// The save search command.
        /// </value>
        public ICommand SaveSearchCommand { get; private set; }

        /// <summary>
        /// Gets the save as search command.
        /// </summary>
        /// <value>
        /// The save as search command.
        /// </value>
        public ICommand SaveAsSearchCommand { get; private set; }

        /// <summary>
        /// Gets the delete saved search command.
        /// </summary>
        /// <value>
        /// The delete saved search command.
        /// </value>
        public ICommand DeleteSavedSearchCommand { get; private set; }

        /// <summary>
        ///     Gets or sets the created before date.
        /// </summary>
        public DateTime CreatedBeforeDate { get; set; }

        /// <summary>
        ///     Gets or sets the created since date.
        /// </summary>
        public DateTime CreatedSinceDate { get; set; }

        /// <summary>
        ///     Gets or sets the document in view.
        /// </summary>
        public string DocumentInView { get; set; }

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the get all issues from project command.
        /// </summary>
        public ICommand GetAllIssuesFromProjectCommand { get; set; }

        /// <summary>
        ///     Gets or sets the get all issues since last analysis command.
        /// </summary>
        public ICommand GetAllIssuesSinceLastAnalysisCommand { get; set; }

        /// <summary>
        ///     Gets or sets the get all my issues command.
        /// </summary>
        public ICommand GetAllMyIssuesCommand { get; set; }

        /// <summary>
        ///     Gets or sets the get issues by filter command.
        /// </summary>
        public ICommand GetIssuesByFilterCommand { get; set; }

        /// <summary>
        /// refersh users data
        /// </summary>
        public ICommand RefreshUsersDataCommand { get; set; }

        /// <summary>
        ///     Gets or sets the get my issues in project command.
        /// </summary>
        public ICommand GetMyIssuesInProjectCommand { get; set; }

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is assignee checked.
        /// </summary>
        public bool IsAssigneeChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is author enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is author enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthorEnabled { get; set; }

        /// <summary>
        /// is teams search enabled
        /// </summary>
        public bool IsTeamsSearchEnabled { get; set; }

        /// <summary>
        /// Gets or sets the author search query.
        /// </summary>
        /// <value>
        /// The author search query.
        /// </value>
        public string AuthorSearchQuery { get; set; }

        /// <summary>
        /// Gets or sets the tag search query.
        /// </summary>
        /// <value>
        /// The tag search query.
        /// </value>
        public string TagSearchQuery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is componenet checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is componenet checked; otherwise, <c>false</c>.
        /// </value>
        public bool IsComponenetChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is blocker checked.
        /// </summary>
        public bool IsBlockerChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is filter by SSCM checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is filter by SSCM checked; otherwise, <c>false</c>.
        /// </value>
        public bool IsFilterBySSCMChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is critical checked.
        /// </summary>
        public bool IsCriticalChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is date before checked.
        /// </summary>
        public bool IsDateBeforeChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is date since checked.
        /// </summary>
        public bool IsDateSinceChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is false positive checked.
        /// </summary>
        public bool IsFalsePositiveChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is fixed checked.
        /// </summary>
        public bool IsFixedChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is tag search enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tag search enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsTagSearchEnabled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is info checked.
        /// </summary>
        public bool IsInfoChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is majaor checked.
        /// </summary>
        public bool IsMajaorChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is minor checked.
        /// </summary>
        public bool IsMinorChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is removed checked.
        /// </summary>
        public bool IsRemovedChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is status closed checked.
        /// </summary>
        public bool IsStatusClosedChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is status confirmed checked.
        /// </summary>
        public bool IsStatusConfirmedChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is status open checked.
        /// </summary>
        public bool IsStatusOpenChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is status reopened checked.
        /// </summary>
        public bool IsStatusReopenedChecked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is status resolved checked.
        /// </summary>
        public bool IsStatusResolvedChecked { get; set; }

        /// <summary>
        ///     Gets or sets the issues grid view.
        /// </summary>
        public IssueGridViewModel IssuesGridView { get; set; }

        /// <summary>
        ///     Gets or sets the resource in editor.
        /// </summary>
        public Resource ResourceInEditor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether show flyouts.
        /// </summary>
        public bool ShowLeftFlyOut { get; set; }

        /// <summary>
        ///     Gets the size of flyout.
        /// </summary>
        public int SizeOfFlyout { get; private set; }

        /// <summary>
        ///     Gets or sets the users list.
        /// </summary>
        public ObservableCollection<User> AssigneeList { get; set; }

        /// <summary>
        /// Teams
        /// </summary>
        public ObservableCollection<Team> Teams { get; set; }

        /// <summary>
        /// Gets the available projects.
        /// </summary>
        /// <value>
        /// The available projects.
        /// </value>
        public IEnumerable<Resource> AvailableProjects { get; internal set; }

        /// <summary>
        /// Called when [show flyouts changed].
        /// </summary>
        public void OnShowFlyoutsChanged()
        {
            this.SizeOfFlyout = this.ShowLeftFlyOut ? 350 : 0;
        }

        /// <summary>
        /// The update colours.
        /// </summary>
        /// <param name="background">
        /// The background.
        /// </param>
        /// <param name="foreground">
        /// The foreground.
        /// </param>
        public void UpdateColours(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;

            this.IssuesGridView.UpdateColours(background, foreground);
        }

        /// <summary>
        ///     The on selected view changed.
        /// </summary>
        public void OnSelectedViewChanged()
        {
            this.searchModel.RefreshIssuesEditor(EventArgs.Empty);
            Debug.WriteLine("Name Changed");
        }

        /// <summary>
        /// Called when [is assignee checked changed].
        /// </summary>
        public void OnIsAssigneeCheckedChanged()
        {
            if (!this.IsAssigneeChecked)
            {
                foreach (var user in this.AssigneeList)
                {
                    user.Selected = false;
                }
            }
        }

        /// <summary>
        /// Gets the available model.
        /// </summary>
        /// <returns>returns configured available model</returns>
        public object GetAvailableModel()
        {
            return this.searchModel;
        }

        /// <summary>
        ///     The get filter resolutions.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        private string FilterResolutions()
        {
            string str = string.Empty;

            if (this.IsFalsePositiveChecked)
            {
                str += "FALSE-POSITIVE,";
            }

            if (this.IsRemovedChecked)
            {
                str += "REMOVED,";
            }

            if (this.IsFixedChecked)
            {
                str += "FIXED,";
            }

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            return "&resolutions=" + str.Substring(0, str.Length - 1);
        }

        /// <summary>
        /// Filters the components keys.
        /// </summary>
        /// <returns>filterd keys</returns>
        private string FilterComponentsKeys(string component)
        {
            if (!this.IsComponenetChecked)
            {
                return string.Empty;
            }

            return "&componentKeys=" + component;
        }

        /// <summary>
        ///     The get filter severities.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        private string FilterSeverities()
        {
            string str = string.Empty;

            if (this.IsBlockerChecked)
            {
                str += "BLOCKER,";
            }

            if (this.IsCriticalChecked)
            {
                str += "CRITICAL,";
            }

            if (this.IsMajaorChecked)
            {
                str += "MAJOR,";
            }

            if (this.IsMinorChecked)
            {
                str += "MINOR,";
            }

            if (this.IsInfoChecked)
            {
                str += "INFO,";
            }

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            return "&severities=" + str.Substring(0, str.Length - 1);
        }

        /// <summary>
        ///     The get filter status.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        private string FilterStatus()
        {
            string str = string.Empty;

            if (this.IsStatusClosedChecked)
            {
                str += "CLOSED,";
            }

            if (this.IsStatusConfirmedChecked)
            {
                str += "CONFIRMED,";
            }

            if (this.IsStatusOpenChecked)
            {
                str += "OPEN,";
            }

            if (this.IsStatusReopenedChecked)
            {
                str += "REOPENED,";
            }

            if (this.IsStatusResolvedChecked)
            {
                str += "RESOLVED,";
            }

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            return "&statuses=" + str.Substring(0, str.Length - 1);
        }

        /// <summary>
        ///     The init commanding.
        /// </summary>
        private void InitCommanding()
        {
            this.CanQUeryIssues = false;
            this.RefreshUsersDataCommand = new RelayCommand(this.OnRefreshUsersDataCommand);
            this.GetIssuesByFilterCommand = new RelayCommand(this.OnGetIssuesByFilterCommand);
            this.GetAllIssuesFromProjectCommand = new RelayCommand(this.OnGetAllIssuesInProject);
            this.GetAllIssuesSinceLastAnalysisCommand = new RelayCommand(this.OnGetAllIssuesSinceLastAnalysisCommand);
            this.GetMyIssuesInProjectCommand = new RelayCommand(this.OnGetMyIssuesInProjectCommand);
            this.GetAllMyIssuesCommand = new RelayCommand(this.OnGetAllMyIssuesCommand);
            this.CloseLeftFlyoutCommand = new RelayCommand(this.OnCloseFlyoutIssueSearchCommand);
            this.CancelQueryCommand = new RelayCommand(this.OnCancelQueryCommand);
            this.LaunchCompoSearchDialogCommand = new RelayCommand(this.OnLaunchCompoSearchDialogCommand);

            this.LoadSavedSearchCommand = new RelayCommand(this.OnLoadSavedSearchCommand);
            this.SaveSearchCommand = new RelayCommand(this.OnSaveSearchCommand);
            this.SaveAsSearchCommand = new RelayCommand(this.OnSaveAsSearchCommand);
            this.DeleteSavedSearchCommand = new RelayCommand(this.OnDeleteSavedSearchCommand);
            this.LoadTeamsFileCommand = new RelayCommand(this.OnLoadTeamsFileCommand);
        }

        private async void OnRefreshUsersDataCommand()
        {
            await this.searchModel.RefreshUsersData();
        }

        /// <summary>
        /// loads a user teams file
        /// </summary>
        private async void OnLoadTeamsFileCommand()
        {
            var openFileDialog1 = new OpenFileDialog()
            {
                FileName = "Select a teams file",
                Filter = "json team file (*.json)|*.json",
                Title = "Open Teams File"
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (await this.searchModel.LoadAndSaveTeams(openFileDialog1.FileName))
                {
                    System.Windows.MessageBox.Show("You can now search with Teams information");
                }				
            }
        }

        /// <summary>
        /// Called when [save as search command].
        /// </summary>
        private void OnDeleteSavedSearchCommand()
        {
            if (this.SelectedSearch == null)
            {
                return;
            }

            this.savedSearchModel.DeleteSearch(this.SelectedSearch);
        }

        /// <summary>
        /// Called when [save as search command].
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void OnSaveAsSearchCommand()
        {
            var search = this.GetCurrentSearch();
            var data = PromptUserData.Prompt("Save as....", "Save as....");
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            search.Name = data;
            this.savedSearchModel.SaveSearch(search);
        }

        /// <summary>
        /// Called when [load saved search command].
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void OnLoadSavedSearchCommand()
        {
            if (this.SelectedSearch == null)
            {
                return;
            }

            this.ReloadSearch(this.savedSearchModel.GetSearchByName(this.SelectedSearch));
        }

        /// <summary>
        /// Called when [save search command].
        /// </summary>
        private void OnSaveSearchCommand()
        {
            var search = this.GetCurrentSearch();

            if (this.SelectedSearch != null)
            {
                var data = QuestionUser.GetInput("You will overwrite " + this.SelectedSearch + ", are you Sure?");
                if (data)
                {
                    search.Name = this.SelectedSearch;
                    this.savedSearchModel.SaveSearch(search);
                }
            }
            else
            {
                this.OnSaveAsSearchCommand();
            }
        }

        /// <summary>
        /// Called when [cancel query command].
        /// </summary>
        private void OnCancelQueryCommand()
        {
            this.searchModel.CancelQuery();
        }

        /// <summary>
        /// Called when [launch compo search dialog command].
        /// </summary>
        private void OnLaunchCompoSearchDialogCommand()
        {
            if (this.isComponentWindowLaunched)
            {
                return;
            }

            this.isComponentWindowLaunched = true;

            try
            {
                var compoenentsList = SearchComponenetDialog.SearchComponents(
                    AuthtenticationHelper.AuthToken,
                    this.restService,
                    this.AvailableProjects.ToList(),
                    this.componentList,
                    this.Teams.ToList(),
                    this.vshelper,
                    this.notificationManager as IRestLogger);
                this.componentList.Clear();
                this.componentList.AddRange(compoenentsList);
                
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Dialog has Crashed: " + ex.Message);
            }

            this.isComponentWindowLaunched = false;
        }

        /// <summary>
        ///     The on close flyout issue search command.
        /// </summary>
        private void OnCloseFlyoutIssueSearchCommand()
        {
            this.ShowLeftFlyOut = false;
            this.IssuesGridView.ShowLeftFlyOut = false;
            this.SizeOfFlyout = 0;
        }

        /// <summary>
        ///     The get all issues in project.
        /// </summary>
        private async void OnGetAllIssuesInProject()
        {
            this.CanQUeryIssues = false;
            this.notificationManager.StartedWorking("Getting All Issues For Project");
            var issues = await this.searchModel.GetAllIssuesInProject();
            await this.IssuesGridView.UpdateIssues(issues, this.Teams);
            this.OnSelectedViewChanged();
            this.CanQUeryIssues = true;
            this.notificationManager.EndedWorking();
        }

        /// <summary>
        ///     The on get all issues since last analysis command.
        /// </summary>
        private async void OnGetAllIssuesSinceLastAnalysisCommand()
        {
            this.CanQUeryIssues = false;
            this.notificationManager.StartedWorking("Getting All Issues For Project Since Last Analysis");
            var issues = await this.searchModel.GetIssuesSinceLastProjectDate();
            await this.IssuesGridView.UpdateIssues(issues, this.Teams);
            this.CanQUeryIssues = true;
            this.notificationManager.EndedWorking();
            this.OnSelectedViewChanged();
        }

        /// <summary>
        ///     The on get all my issues command.
        /// </summary>
        private async void OnGetAllMyIssuesCommand()
        {
            this.CanQUeryIssues = false;
            this.notificationManager.StartedWorking("Getting All My Issues in Project");
            await this.IssuesGridView.UpdateIssues(await this.searchModel.GetCurrentUserIssues(), this.Teams);
            this.CanQUeryIssues = true;
            this.notificationManager.EndedWorking();
            this.OnSelectedViewChanged();
        }

        /// <summary>
        ///     The on get issues by filter command.
        /// </summary>
        private async void OnGetIssuesByFilterCommand()
        {
            this.CanQUeryIssues = false;
            this.notificationManager.StartedWorking("Getting All Issues by filter");
            await this.RetrieveIssuesUsingCurrentFilter();
            this.CanQUeryIssues = true;
            this.notificationManager.EndedWorking();
            this.OnSelectedViewChanged();
        }

        /// <summary>
        ///     The on get my issues in project command.
        /// </summary>
        private async void OnGetMyIssuesInProjectCommand()
        {
            this.CanQUeryIssues = false;
            this.notificationManager.StartedWorking("Getting My Issues For Project");
            await this.IssuesGridView.UpdateIssues(await 
                this.searchModel.GetCurrentUserIssuesInProject(), this.Teams);
            this.CanQUeryIssues = true;
            this.notificationManager.EndedWorking();
            this.OnSelectedViewChanged();
        }

        /// <summary>
        ///     The retrieve issues using current filter.
        /// </summary>
        private async Task RetrieveIssuesUsingCurrentFilter()
        {
            string request = string.Empty;

            request += this.FilterByUsers();
            request += this.FilterByDate();
            request += this.FilterSeverities();
            request += this.FilterStatus();
            request += this.FilterResolutions();

            var issues = new List<Issue>();
            if (!this.IsComponenetChecked)
            {
                var results = await this.searchModel.GetIssuesUsingFilter(request, this.IsFilterBySSCMChecked, this.componentList.Count > 0 && this.IsComponenetChecked);
                issues.AddRange(results.Item1);
            }
            else
            {
                foreach (var componentKey in this.componentList)
                {
                    this.notificationManager.ReportMessage("Search Issues in: " + componentKey.Key);
                    var finalRequest = request + this.FilterComponentsKeys(componentKey.Key);
                    var results = await this.searchModel.GetIssuesUsingFilter(finalRequest, this.IsFilterBySSCMChecked, this.componentList.Count > 0 && this.IsComponenetChecked);
                    issues.AddRange(results.Item1);

                    if (results.Item2)
                    {
                        break;
                    }
                }
            }

            await this.IssuesGridView.UpdateIssues(issues, this.Teams);
        }

        /// <summary>
        /// Filters the by users.
        /// </summary>
        /// <returns>use search query</returns>
        private string FilterByUsers()
        {
            var request = string.Empty;
            if (this.IsAssigneeChecked)
            {
                try
                {
                    var users = this.AssigneeList.Where(i => i.Selected).Select(i => i.Login).Aggregate((i, j) => i + "," + j);
                    request += "&assignees=" + users;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            if (this.IsAuthorEnabled && !string.IsNullOrEmpty(this.AuthorSearchQuery))
            {
                request += "&authors=" + this.AuthorSearchQuery;
            }

            if (this.IsTagSearchEnabled && !string.IsNullOrEmpty(this.TagSearchQuery))
            {
                request += "&tags=" + this.TagSearchQuery;
            }

            return request;
        }

        /// <summary>
        /// Filters the by date.
        /// </summary>
        /// <returns>filters date</returns>
        private string FilterByDate()
        {
            var request = string.Empty;

            if (this.IsDateBeforeChecked)
            {
                var stringDate = string.Format("{0:yyyy-MM-dd}", this.CreatedBeforeDate);
                request += "&createdBefore=" + stringDate;
            }

            if (this.IsDateSinceChecked)
            {
                var stringDate = string.Format("{0:yyyy-MM-dd}", this.CreatedSinceDate);
                request += "&createdAfter=" + stringDate;
            }

            return request;
        }

        /// <summary>
        /// Shows the hide left flyout.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ShowHideLeftFlyout(object sender, EventArgs e)
        {
            if (this.IssuesGridView == null)
            {
                return;
            }

            this.ShowLeftFlyOut = this.IssuesGridView.ShowLeftFlyOut;
            this.OnShowFlyoutsChanged();
        }

        /// <summary>
        /// The create row context menu.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="translator">The translator.</param>
        /// <param name="analyser">The analyser.</param>
        /// <returns>
        /// The
        /// <see><cref>ObservableCollection</cref></see>
        /// .
        /// </returns>
        private ObservableCollection<IMenuItem> CreateRowContextMenu(ISonarRestService service, ISQKeyTranslator translator, ISonarLocalAnalyser analyser, IList<IIssueTrackerPlugin> plugins)
        {
            var menu = new ObservableCollection<IMenuItem>
                           {
                               ChangeStatusMenu.MakeMenu(service, this.IssuesGridView, this.notificationManager),
                               OpenResourceMenu.MakeMenu(service, this.IssuesGridView),
                               SourceControlMenu.MakeMenu(service, this.IssuesGridView, this.notificationManager, translator),
                               IssueTrackerMenu.MakeMenu(service, this.IssuesGridView, this.notificationManager, translator, plugins),
                               AssignMenu.MakeMenu(service, this.IssuesGridView, this.notificationManager),
                               AssignTagMenu.MakeMenu(service, this.IssuesGridView, this.notificationManager as IRestLogger),
                               SetSqaleMenu.MakeMenu(service, this.IssuesGridView, this.notificationManager, translator, analyser)
                           };

            return menu;
        }

        /// <summary>
        /// Gets the current search.
        /// </summary>
        /// <returns>returns current search in view</returns>
        private Search GetCurrentSearch()
        {
            var search = new Search();
            search.Components = new List<Resource>(this.componentList);
            if (this.IsComponenetChecked)
            {
                search.ComponenetsEnabled = true;
            }

            search.Assignees = this.AssigneeList.ToList();
            if (this.IsAssigneeChecked)
            {
                search.AssigneesEnabled = true;
            }

            search.Authors = this.AuthorSearchQuery;
            if (this.IsAuthorEnabled)
            {
                search.AuthorsEnabled = true;
            }

            search.Tags = this.TagSearchQuery;
            if (this.IsTagSearchEnabled)
            {
                search.TagSearchEnabled = true;
            }

            search.SinceDate = this.CreatedSinceDate;
            if (this.IsDateSinceChecked)
            {
                search.SinceDateEnabled = true;
            }
            if (this.IsFilterBySSCMChecked)
            {
                search.FilterBySSCM = true;
            }

            search.BeforeDate = this.CreatedBeforeDate;
            if (this.IsDateBeforeChecked)
            {
                search.BeforeDateEnabled = true;
            }

            search.Status = new List<IssueStatus>();
            if (this.IsStatusClosedChecked)
            {
                search.Status.Add(IssueStatus.CLOSED);
            }

            if (this.IsStatusOpenChecked)
            {
                search.Status.Add(IssueStatus.OPEN);
            }

            if (this.IsStatusResolvedChecked)
            {
                search.Status.Add(IssueStatus.RESOLVED);
            }

            if (this.IsStatusReopenedChecked)
            {
                search.Status.Add(IssueStatus.REOPENED);
            }

            if (this.IsStatusConfirmedChecked)
            {
                search.Status.Add(IssueStatus.CONFIRMED);
            }

            search.Severities = new List<Severity>();
            if (this.IsBlockerChecked)
            {
                search.Severities.Add(Severity.BLOCKER);
            }

            if (this.IsCriticalChecked)
            {
                search.Severities.Add(Severity.CRITICAL);
            }

            if (this.IsMajaorChecked)
            {
                search.Severities.Add(Severity.MAJOR);
            }

            if (this.IsMinorChecked)
            {
                search.Severities.Add(Severity.MINOR);
            }

            if (this.IsInfoChecked)
            {
                search.Severities.Add(Severity.INFO);
            }

            search.Resolutions = new List<Resolution>();
            if (this.IsFalsePositiveChecked)
            {
                search.Resolutions.Add(Resolution.FALSE_POSITIVE);
            }

            if (this.IsRemovedChecked)
            {
                search.Resolutions.Add(Resolution.REMOVED);
            }

            if (this.IsFixedChecked)
            {
                search.Resolutions.Add(Resolution.FIXED);
            }

            return search;
        }

        /// <summary>
        /// Reloads the search.
        /// </summary>
        /// <param name="search">The search.</param>
        private void ReloadSearch(Search search)
        {            
            this.componentList.Clear();
            this.componentList.AddRange(search.Components);
            this.IsComponenetChecked = search.ComponenetsEnabled;

            this.IsAssigneeChecked = search.AssigneesEnabled;
            foreach (var assignee in search.Assignees)
            {
                foreach (var currAssignee in this.AssigneeList)
                {
                    if (assignee.Name.Equals(currAssignee.Name))
                    {
                        currAssignee.Selected = assignee.Selected;
                    }
                }
            }

            this.IsAuthorEnabled = search.AuthorsEnabled;
            this.AuthorSearchQuery = search.Authors;

            this.IsTagSearchEnabled = search.TagSearchEnabled;
            this.TagSearchQuery = search.Tags;

            this.IsDateSinceChecked = search.SinceDateEnabled;
            this.IsFilterBySSCMChecked = search.FilterBySSCM;
            this.CreatedSinceDate = search.SinceDate;
            this.IsDateBeforeChecked = search.BeforeDateEnabled;
            this.CreatedBeforeDate = search.BeforeDate;

            if (search.Status.Contains(IssueStatus.CLOSED))
            {
                this.IsStatusClosedChecked = true;
            }
            else
            {
                this.IsStatusClosedChecked = false;
            }
             
            if (search.Status.Contains(IssueStatus.CONFIRMED))
            {
                this.IsStatusConfirmedChecked = true;
            }
            else
            {
                this.IsStatusConfirmedChecked = false;
            }

            if (search.Status.Contains(IssueStatus.OPEN))
            {
                this.IsStatusOpenChecked = true;
            }
            else
            {
                this.IsStatusOpenChecked = false;
            }

            if (search.Status.Contains(IssueStatus.REOPENED))
            {
                this.IsStatusReopenedChecked = true;
            }
            else
            {
                this.IsStatusReopenedChecked = false;
            }

            if (search.Status.Contains(IssueStatus.RESOLVED))
            {
                this.IsStatusResolvedChecked = true;
            }
            else
            {
                this.IsStatusResolvedChecked = false;
            }


            if (search.Severities.Contains(Severity.BLOCKER))
            {
                this.IsBlockerChecked = true;
            }
            else
            {
                this.IsBlockerChecked = false;
            }

            if (search.Severities.Contains(Severity.CRITICAL))
            {
                this.IsCriticalChecked = true;
            }
            else
            {
                this.IsCriticalChecked = false;
            }

            if (search.Severities.Contains(Severity.INFO))
            {
                this.IsInfoChecked = true;
            }
            else
            {
                this.IsInfoChecked = false;
            }

            if (search.Severities.Contains(Severity.MAJOR))
            {
                this.IsMajaorChecked = true;
            }
            else
            {
                this.IsMajaorChecked = false;
            }

            if (search.Severities.Contains(Severity.MINOR))
            {
                this.IsMinorChecked = true;
            }
            else
            {
                this.IsMinorChecked = false;
            }

            if (search.Resolutions.Contains(Resolution.FALSE_POSITIVE))
            {
                this.IsFalsePositiveChecked = true;
            }
            else
            {
                this.IsFalsePositiveChecked = false;
            }

            if (search.Resolutions.Contains(Resolution.REMOVED))
            {
                this.IsRemovedChecked = true;
            }
            else
            {
                this.IsRemovedChecked = false;
            }

            if (search.Resolutions.Contains(Resolution.FIXED))
            {
                this.IsFixedChecked = true;
            }
            else
            {
                this.IsFixedChecked = false;
            }
        }
    }
}