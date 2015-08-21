// --------------------------------------------------------------------------------------------------------------------
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
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using Model.Analysis;
    using PropertyChanged;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using SonarLocalAnalyser;


    /// <summary>
    /// The issues search view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class IssuesSearchViewModel : IViewModelBase
    {
        /// <summary>
        ///     The issues filter view model key.
        /// </summary>
        private const string IssuesFilterViewModelKey = "IssuesFilterViewModel";

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
        /// Initializes a new instance of the <see cref="IssuesSearchViewModel" /> class.
        /// </summary>
        /// <param name="searchModel">The search model.</param>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="restService">The rest service.</param>
        /// <param name="translator">The translator.</param>
        public IssuesSearchViewModel(
            IssuesSearchModel searchModel,
            INotificationManager notificationManager,
            IConfigurationHelper configurationHelper,
            ISonarRestService restService,
            ISQKeyTranslator translator)
        {
            this.notificationManager = notificationManager;
            this.configurationHelper = configurationHelper;
            this.searchModel = searchModel;
            this.Header = "Issues Search";
            this.AvailableActionPlans = new ObservableCollection<SonarActionPlan>();
            this.UsersList = new ObservableCollection<User>();
            this.IssuesGridView = new IssueGridViewModel(true, "SearchView", false, configurationHelper, restService, notificationManager, translator);

            this.InitCommanding();

            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
            this.CreatedBeforeDate = DateTime.Now;
            this.CreatedSinceDate = DateTime.Now;

            SonarQubeViewModel.RegisterNewViewModelInPool(this);
        }

        /// <summary>
        /// Gets or sets the available gates.
        /// </summary>
        /// <value>
        /// The available gates.
        /// </value>
        public ObservableCollection<SonarActionPlan> AvailableActionPlans { get; set; }

        /// <summary>
        /// Gets or sets the selected gate.
        /// </summary>
        /// <value>
        /// The selected gate.
        /// </value>
        public SonarActionPlan SelectedActionPlan { get; set; }

        /// <summary>
        ///     Gets or sets the assignee in filter.
        /// </summary>
        public User Assignee { get; set; }

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
        public RelayCommand CloseFlyoutIssueSearchCommand { get; private set; }

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
        ///     Gets the flyout issue search command.
        /// </summary>
        public RelayCommand FlyoutIssueSearchCommand { get; private set; }

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

        /// <summary>Gets or sets the go to prev issue command.</summary>
        public ICommand GoToPrevIssueCommand { get; set; }

        /// <summary>Gets or sets the go to next issue command.</summary>
        public ICommand GoToNextIssueCommand { get; set; }

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
        ///     Gets or sets a value indicating whether is reporter checked.
        /// </summary>
        public bool IsReporterChecked { get; set; }

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
        ///     Gets or sets the reporter in filter.
        /// </summary>
        public User Reporter { get; set; }

        /// <summary>
        ///     Gets or sets the resource in editor.
        /// </summary>
        public Resource ResourceInEditor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether show flyouts.
        /// </summary>
        public bool ShowFlyouts { get; set; }

        /// <summary>
        ///     Gets the size of flyout.
        /// </summary>
        public int SizeOfFlyout { get; private set; }

        /// <summary>
        ///     Gets or sets the users list.
        /// </summary>
        public ObservableCollection<User> UsersList { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is action plan selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is action plan selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsActionPlanSelected { get; private set; }

        /// <summary>
        /// Called when [show flyouts changed].
        /// </summary>
        public void OnShowFlyoutsChanged()
        {
            this.SizeOfFlyout = this.ShowFlyouts ? 150 : 0;
        }

        /// <summary>
        ///     The save filter to disk.
        /// </summary>
        public void SaveFilterToDisk()
        {
            if (this.configurationHelper != null)
            {
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.UIProperties,
                    IssuesFilterViewModelKey,
                    "IsStatusOpenChecked",
                    this.IsStatusOpenChecked.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.UIProperties,
                    IssuesFilterViewModelKey,
                    "IsStatusClosedChecked",
                    this.IsStatusClosedChecked.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.UIProperties,
                    IssuesFilterViewModelKey,
                    "IsStatusResolvedChecked",
                    this.IsStatusResolvedChecked.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.UIProperties,
                    IssuesFilterViewModelKey,
                    "IsStatusConfirmedChecked",
                    this.IsStatusConfirmedChecked.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.UIProperties,
                    IssuesFilterViewModelKey,
                    "IsStatusReopenedChecked",
                    this.IsStatusReopenedChecked.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.UIProperties,
                    IssuesFilterViewModelKey,
                    "IsBlockerChecked",
                    this.IsBlockerChecked.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.UIProperties,
                    IssuesFilterViewModelKey,
                    "IsCriticalChecked",
                    this.IsCriticalChecked.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.UIProperties,
                    IssuesFilterViewModelKey,
                    "IsMajaorChecked",
                    this.IsMajaorChecked.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.UIProperties,
                    IssuesFilterViewModelKey,
                    "IsMinorChecked",
                    this.IsMinorChecked.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.UIProperties,
                    IssuesFilterViewModelKey,
                    "IsInfoChecked",
                    this.IsInfoChecked.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.UIProperties,
                    IssuesFilterViewModelKey,
                    "IsFalsePositiveChecked",
                    this.IsFalsePositiveChecked.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.UIProperties,
                    IssuesFilterViewModelKey,
                    "IsRemovedChecked",
                    this.IsRemovedChecked.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.UIProperties,
                    IssuesFilterViewModelKey,
                    "IsFixedChecked",
                    this.IsFixedChecked.ToString(CultureInfo.InvariantCulture));
            }
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
            this.searchModel.OnAnalysisModeHasChange(EventArgs.Empty);
            Debug.WriteLine("Name Changed");
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
        /// Filters the action plans.
        /// </summary>
        /// <returns>return filter action plan</returns>
        private string FilterActionPlans()
        {
            string str = string.Empty;

            if (this.IsActionPlanSelected)
            {
                str += this.SelectedActionPlan.Key;
            }

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            return "&actionPlans=" + str;
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
            this.GetIssuesByFilterCommand = new RelayCommand(this.OnGetIssuesByFilterCommand);
            this.GoToPrevIssueCommand = new RelayCommand(this.OnGoToPrevIssueCommand);
            this.GoToNextIssueCommand = new RelayCommand(this.OnGoToNextIssueCommand);

            this.GetAllIssuesFromProjectCommand = new RelayCommand(this.OnGetAllIssuesInProject);
            this.GetAllIssuesSinceLastAnalysisCommand = new RelayCommand(this.OnGetAllIssuesSinceLastAnalysisCommand);
            this.GetMyIssuesInProjectCommand = new RelayCommand(this.OnGetMyIssuesInProjectCommand);
            this.GetAllMyIssuesCommand = new RelayCommand(this.OnGetAllMyIssuesCommand);

            this.FlyoutIssueSearchCommand = new RelayCommand(this.OnFlyoutIssueSearchCommand);
            this.CloseFlyoutIssueSearchCommand = new RelayCommand(this.OnCloseFlyoutIssueSearchCommand);
        }

        /// <summary>
        ///     The on close flyout issue search command.
        /// </summary>
        private void OnCloseFlyoutIssueSearchCommand()
        {
            this.ShowFlyouts = false;
            this.SizeOfFlyout = 0;
        }

        /// <summary>
        ///     The on flyout issue search command.
        /// </summary>
        private void OnFlyoutIssueSearchCommand()
        {
            this.ShowFlyouts = true;
            this.SizeOfFlyout = 150;
        }

        /// <summary>
        ///     The get all issues in project.
        /// </summary>
        private void OnGetAllIssuesInProject()
        {
            this.CanQUeryIssues = false;
            this.notificationManager.StartedWorking("Getting All Issues For Project");

            var bw = new BackgroundWorker { WorkerReportsProgress = true };

            bw.RunWorkerCompleted += delegate
            {
                Application.Current.Dispatcher.Invoke(
                    delegate
                    {
                        this.OnSelectedViewChanged();
                        this.CanQUeryIssues = true;
                        this.notificationManager.EndedWorking();
                    });
            };

            bw.DoWork +=
                delegate { this.IssuesGridView.UpdateIssues(this.searchModel.GetAllIssuesInProject()); };

            bw.RunWorkerAsync();
        }

        /// <summary>
        ///     The on get all issues since last analysis command.
        /// </summary>
        private void OnGetAllIssuesSinceLastAnalysisCommand()
        {
            this.CanQUeryIssues = false;
            this.notificationManager.StartedWorking("Getting All Issues For Project Since Last Analysis");
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
                {
                    this.CanQUeryIssues = true;
                    this.notificationManager.EndedWorking();
                    this.OnSelectedViewChanged();
                };

            bw.DoWork +=
                delegate
                    {
                        this.IssuesGridView.UpdateIssues(this.searchModel.GetIssuesSinceLastProjectDate());
                    };

            bw.RunWorkerAsync();
        }

        /// <summary>
        ///     The on get all my issues command.
        /// </summary>
        private void OnGetAllMyIssuesCommand()
        {
            this.CanQUeryIssues = false;
            this.notificationManager.StartedWorking("Getting All My Issues in Project");
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
                {
                    this.CanQUeryIssues = true;
                    this.notificationManager.EndedWorking();
                    this.OnSelectedViewChanged();
                };

            bw.DoWork +=
                delegate
                    {
                        this.IssuesGridView.UpdateIssues(this.searchModel.GetCurrentUserIssues());
                    };

            bw.RunWorkerAsync();
        }

        /// <summary>The on go to next issue command.</summary>
        private void OnGoToNextIssueCommand()
        {
            this.IssuesGridView.GoToNextIssue();
        }

        /// <summary>The on go to prev issue command.</summary>
        private void OnGoToPrevIssueCommand()
        {
            this.IssuesGridView.GoToPrevIssue();
        }

        /// <summary>
        ///     The on get issues by filter command.
        /// </summary>
        private void OnGetIssuesByFilterCommand()
        {
            this.CanQUeryIssues = false;
            this.notificationManager.StartedWorking("Getting All Issues by filter");
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
                {
                    this.CanQUeryIssues = true;
                    this.notificationManager.EndedWorking();
                    this.OnSelectedViewChanged();
                };

            bw.DoWork += delegate { this.RetrieveIssuesUsingCurrentFilter(); };

            bw.RunWorkerAsync();
        }

        /// <summary>
        ///     The on get my issues in project command.
        /// </summary>
        private void OnGetMyIssuesInProjectCommand()
        {
            this.CanQUeryIssues = false;
            this.notificationManager.StartedWorking("Getting My Issues For Project");
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
                {
                    this.CanQUeryIssues = true;
                    this.notificationManager.EndedWorking();
                    this.OnSelectedViewChanged();
                };

            bw.DoWork +=
                delegate
                    {
                        this.IssuesGridView.UpdateIssues(
                            this.searchModel.GetCurrentUserIssuesInProject());
                    };

            bw.RunWorkerAsync();
        }

        /// <summary>
        ///     The retrieve issues using current filter.
        /// </summary>
        private void RetrieveIssuesUsingCurrentFilter()
        {
            this.SaveFilterToDisk();

            string request = string.Empty;

            if (this.IsAssigneeChecked)
            {
                request += "&assignees=" + this.Assignee.Login;
            }

            if (this.IsReporterChecked)
            {
                request += "&reporters=" + this.Reporter.Login;
            }

            if (this.IsDateBeforeChecked)
            {
                request += "&createdBefore=" + Convert.ToString(this.CreatedBeforeDate.Year) + "-" + Convert.ToString(this.CreatedBeforeDate.Month)
                           + "-" + Convert.ToString(this.CreatedBeforeDate.Day);
            }

            if (this.IsDateSinceChecked)
            {
                request += "&createdAfter=" + Convert.ToString(this.CreatedSinceDate.Year) + "-" + Convert.ToString(this.CreatedSinceDate.Month) + "-"
                           + Convert.ToString(this.CreatedSinceDate.Day);
            }

            request += this.FilterSeverities();
            request += this.FilterStatus();
            request += this.FilterResolutions();
            request += this.FilterActionPlans();

            this.IssuesGridView.UpdateIssues(this.searchModel.GetIssuesUsingFilter(request, this.IsFilterBySSCMChecked));
            
        }
    }
}