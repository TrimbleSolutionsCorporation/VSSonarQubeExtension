// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssuesSearchViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   The issues search view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.ViewModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;

    using ExtensionTypes;

    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using SonarRestService;

    using VSSonarExtensionUi.Cache;

    using VSSonarPlugins;
    using System;
    using System.Linq;
    using System.Collections.ObjectModel;
    using System.Globalization;


    /// <summary>
    /// The issues search view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class IssuesSearchViewModel : IViewModelBase
    {
        #region Static Fields

        private readonly ModelEditorCache localEditorCache = new ModelEditorCache();

        /// <summary>
        /// The comments width default.
        /// </summary>
        private static readonly GridLength CommentsWidthDefault = new GridLength(250);

        #endregion

        #region Fields

        /// <summary>
        /// The sonar qube view model.
        /// </summary>
        private readonly SonarQubeViewModel sonarQubeViewModel;

        /// <summary>
        /// The comments.
        /// </summary>
        private List<Comment> comments;

        public ISonarConfiguration Configuration { get; set; }

        private readonly ISonarRestService restService;

        private readonly IVsEnvironmentHelper vsHelper;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuesSearchViewModel"/> class.
        /// </summary>
        public IssuesSearchViewModel()
        {
            this.Header = "Issues";
            this.SonarVersion = 4.5;
            this.UsersList = new ObservableCollection<User>();
            this.IssuesGridView = new IssueGridViewModel();

            this.InitCommanding();
        }

        private void InitCommanding()
        {
            this.CanQUeryIssues = false;
            this.GetIssuesByFilterCommand = new RelayCommand(this.OnGetIssuesByFilterCommand, () => this.CanQUeryIssues);
            this.GetAllIssuesFromProjectCommand = new RelayCommand(this.OnGetAllIssuesInProject, () => this.CanQUeryIssues);
            this.GetAllIssuesSinceLastAnalysisCommand = new RelayCommand(this.OnGetAllIssuesSinceLastAnalysisCommand, () => this.CanQUeryIssues);
            this.GetMyIssuesInProjectCommand = new RelayCommand(this.OnGetMyIssuesInProjectCommand, () => this.CanQUeryIssues);
            this.GetAllMyIssuesCommand = new RelayCommand(this.OnGetAllMyIssuesCommand, () => this.CanQUeryIssues);
    }







        /// <summary>
        /// Initializes a new instance of the <see cref="IssuesSearchViewModel"/> class.
        /// </summary>
        /// <param name="sonarQubeViewModel">
        ///     The sonar qube view model.
        /// </param>
        /// <param name="vsHelper"></param>
        /// <param name="restService"></param>
        public IssuesSearchViewModel(SonarQubeViewModel sonarQubeViewModel, IVsEnvironmentHelper vsHelper, ISonarRestService restService)
        {
            this.vsHelper = vsHelper;
            this.restService = restService;
            this.sonarQubeViewModel = sonarQubeViewModel;
            this.Header = "Issues";
            this.SonarVersion = 4.5;
            this.UsersList = new ObservableCollection<User>();
            this.IssuesGridView = new IssueGridViewModel(sonarQubeViewModel, this, true);

            this.InitCommanding();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        ///     Gets or sets the issues grid view.
        /// </summary>
        public IssueGridViewModel IssuesGridView { get; set; }

        public Resource AssociatedProject { get; set; }

        public ICommand GetAllIssuesFromProjectCommand { get; set; }
        public ICommand GetIssuesByFilterCommand { get; set; }

        

        public ICommand GetAllIssuesSinceLastAnalysisCommand { get; set; }

        public ICommand GetMyIssuesInProjectCommand { get; set; }

        public ICommand GetAllMyIssuesCommand { get; set; }


        private void OnGetAllMyIssuesCommand()
        {
            this.CanQUeryIssues = false;this.sonarQubeViewModel.IsExtensionBusy = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
            {
                this.CanQUeryIssues = true;this.sonarQubeViewModel.IsExtensionBusy = false;
                this.IssuesGridView.UpdateIssues(this.localEditorCache.GetIssues());
            };

            bw.DoWork +=
                delegate
                {
                    this.ReplaceAllIssuesInCache(
                        this.restService.GetAllIssuesByAssignee(
                            this.Configuration,
                            this.Configuration.Username));
                };

            bw.RunWorkerAsync();
        }

        private void OnGetMyIssuesInProjectCommand()
        {
            this.CanQUeryIssues = false;this.sonarQubeViewModel.IsExtensionBusy = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
            {
                this.CanQUeryIssues = true;this.sonarQubeViewModel.IsExtensionBusy = false;
                this.IssuesGridView.UpdateIssues(this.localEditorCache.GetIssues());
            };

            bw.DoWork +=
                delegate
                {
                    this.ReplaceAllIssuesInCache(
                        this.restService.GetIssuesByAssigneeInProject(
                            this.Configuration,
                            this.AssociatedProject.Key,
                            this.Configuration.Username));
                };

            bw.RunWorkerAsync();
        }

        private void OnGetIssuesByFilterCommand()
        {
            this.CanQUeryIssues = false;this.sonarQubeViewModel.IsExtensionBusy = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
                {
                    this.IssuesGridView.UpdateIssues(this.localEditorCache.GetIssues());
                    this.CanQUeryIssues = true;this.sonarQubeViewModel.IsExtensionBusy = false;                    
                };

            bw.DoWork += delegate { this.RetrieveIssuesUsingCurrentFilter(); };

            bw.RunWorkerAsync();
        }

        /// <summary>
        ///     The retrieve issues using current filter.
        /// </summary>
        public void RetrieveIssuesUsingCurrentFilter()
        {
            this.SaveFilterToDisk();

            if (this.SonarVersion < 3.6)
            {
                this.ReplaceAllIssuesInCache(this.restService.GetIssuesForProjects(this.Configuration, this.AssociatedProject.Key));
                return;
            }

            string request = "?componentRoots=" + this.AssociatedProject.Key;

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

            this.ReplaceAllIssuesInCache(this.restService.GetIssues(this.Configuration, request, this.AssociatedProject.Key));
        }

        internal void InitDataAssociation(Resource associatedProject, ISonarConfiguration sonarCubeConfiguration)
        {
            this.AssociatedProject = associatedProject;
            this.Configuration = sonarCubeConfiguration;
            this.CanQUeryIssues = true;this.sonarQubeViewModel.IsExtensionBusy = false;

            List<User> usortedList = restService.GetUserList(sonarCubeConfiguration);
            if (usortedList != null && usortedList.Count > 0)
            {
                this.UsersList = new ObservableCollection<User>(usortedList.OrderBy(i => i.Login));
            }
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

        private void OnGetAllIssuesSinceLastAnalysisCommand()
        {
            this.CanQUeryIssues = false;this.sonarQubeViewModel.IsExtensionBusy = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
            {
                this.CanQUeryIssues = true;this.sonarQubeViewModel.IsExtensionBusy = false;
                this.IssuesGridView.UpdateIssues(this.localEditorCache.GetIssues());
            };

            bw.DoWork +=
                delegate
                {
                    this.ReplaceAllIssuesInCache(
                        this.restService.GetIssuesForProjectsCreatedAfterDate(
                            this.Configuration,
                            this.AssociatedProject.Key,
                            this.AssociatedProject.Date));
                };

            bw.RunWorkerAsync();
        }

        /// <summary>
        /// The get all issues in project.
        /// </summary>
        private void OnGetAllIssuesInProject()
        {
            this.CanQUeryIssues = false;this.sonarQubeViewModel.IsExtensionBusy = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
                {
                    this.CanQUeryIssues = true;this.sonarQubeViewModel.IsExtensionBusy = false;
                    this.IssuesGridView.UpdateIssues(this.localEditorCache.GetIssues());
                };

            bw.DoWork +=
                delegate
                {
                    this.ReplaceAllIssuesInCache(
                        this.restService.GetIssuesForProjects(
                            this.Configuration,
                            this.AssociatedProject.Key));
                };

            bw.RunWorkerAsync();
        }

        public bool CanQUeryIssues { get; set; }

        /// <summary>
        /// The replace all issues in cache.
        /// </summary>
        /// <param name="getAllIssuesByAssignee">
        /// The get all issues by assignee.
        /// </param>
        public void ReplaceAllIssuesInCache(List<Issue> getAllIssuesByAssignee)
        {
            this.localEditorCache.UpdateIssues(getAllIssuesByAssignee);
            this.RefreshIssuesInViews();
        }

        /// <summary>
        ///     The refresh issues.
        /// </summary>
        public void RefreshIssuesInViews()
        {
//            this.OnPropertyChanged("Issues");
//            this.OnPropertyChanged("IssuesInEditor");
        }


        #region Fields

        /// <summary>
        /// The issues filter view model key.
        /// </summary>
        private const string IssuesFilterViewModelKey = "IssuesFilterViewModel";

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the created before date.
        /// </summary>
        public DateTime CreatedBeforeDate { get; set; }

        /// <summary>
        ///     Gets or sets the created since date.
        /// </summary>
        public DateTime CreatedSinceDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is assignee checked.
        /// </summary>
        public bool IsAssigneeChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is blocker checked.
        /// </summary>
        public bool IsBlockerChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is critical checked.
        /// </summary>
        public bool IsCriticalChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is false positive checked.
        /// </summary>
        public bool IsFalsePositiveChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is fixed checked.
        /// </summary>
        public bool IsFixedChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is info checked.
        /// </summary>
        public bool IsInfoChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is majaor checked.
        /// </summary>
        public bool IsMajaorChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is minor checked.
        /// </summary>
        public bool IsMinorChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is removed checked.
        /// </summary>
        public bool IsRemovedChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is reporter checked.
        /// </summary>
        public bool IsReporterChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is status closed checked.
        /// </summary>
        public bool IsStatusClosedChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is status confirmed checked.
        /// </summary>
        public bool IsStatusConfirmedChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is status open checked.
        /// </summary>
        public bool IsStatusOpenChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is status reopened checked.
        /// </summary>
        public bool IsStatusReopenedChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is status resolved checked.
        /// </summary>
        public bool IsStatusResolvedChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is date before checked.
        /// </summary>
        public bool IsDateBeforeChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is date since checked.
        /// </summary>
        public bool IsDateSinceChecked { get; set; }

        /// <summary>
        ///     Gets or sets the users list.
        /// </summary>
        public ObservableCollection<User> UsersList { get; set; }

        /// <summary>
        /// Gets or sets the reporter in filter.
        /// </summary>
        public User Reporter { get; set; }

        /// <summary>
        /// Gets or sets the assignee in filter.
        /// </summary>
        public User Assignee { get; set; }
        public double SonarVersion { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// The save filter to disk.
        /// </summary>
        public void SaveFilterToDisk()
        {
            if (this.vsHelper != null)
            {
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusOpenChecked", this.IsStatusOpenChecked.ToString(CultureInfo.InvariantCulture));
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusClosedChecked", this.IsStatusClosedChecked.ToString(CultureInfo.InvariantCulture));
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusResolvedChecked", this.IsStatusResolvedChecked.ToString(CultureInfo.InvariantCulture));
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusConfirmedChecked", this.IsStatusConfirmedChecked.ToString(CultureInfo.InvariantCulture));
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusReopenedChecked", this.IsStatusReopenedChecked.ToString(CultureInfo.InvariantCulture));
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsBlockerChecked", this.IsBlockerChecked.ToString(CultureInfo.InvariantCulture));
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsCriticalChecked", this.IsCriticalChecked.ToString(CultureInfo.InvariantCulture));
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsMajaorChecked", this.IsMajaorChecked.ToString(CultureInfo.InvariantCulture));
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsMinorChecked", this.IsMinorChecked.ToString(CultureInfo.InvariantCulture));
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsInfoChecked", this.IsInfoChecked.ToString(CultureInfo.InvariantCulture));
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsFalsePositiveChecked", this.IsFalsePositiveChecked.ToString(CultureInfo.InvariantCulture));
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsRemovedChecked", this.IsRemovedChecked.ToString(CultureInfo.InvariantCulture));
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsFixedChecked", this.IsFixedChecked.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        ///     The restore user filtering options.
        /// </summary>
        private void RestoreUserFilteringOptions()
        {
            this.CreatedBeforeDate = DateTime.Now;
            this.CreatedSinceDate = DateTime.Now;
            if (this.vsHelper == null)
            {
                this.IsStatusOpenChecked = true;
                this.IsStatusClosedChecked = true;
                this.IsStatusResolvedChecked = true;
                this.IsStatusConfirmedChecked = true;
                this.IsStatusReopenedChecked = true;
                this.IsBlockerChecked = true;
                this.IsCriticalChecked = true;
                this.IsMajaorChecked = true;
                this.IsMinorChecked = true;
                this.IsInfoChecked = true;
                this.IsFalsePositiveChecked = true;
                this.IsRemovedChecked = true;
                this.IsFixedChecked = true;

                return;
            }

            Dictionary<string, string> options =
                this.vsHelper.ReadAllAvailableOptionsInSettings(IssuesFilterViewModelKey);
            if (options != null && options.Count > 0)
            {
                this.IsStatusOpenChecked = bool.Parse(options["IsStatusOpenChecked"]);
                this.IsStatusClosedChecked = bool.Parse(options["IsStatusClosedChecked"]);
                this.IsStatusResolvedChecked = bool.Parse(options["IsStatusResolvedChecked"]);
                this.IsStatusConfirmedChecked = bool.Parse(options["IsStatusConfirmedChecked"]);
                this.IsStatusReopenedChecked = bool.Parse(options["IsStatusReopenedChecked"]);
                this.IsBlockerChecked = bool.Parse(options["IsBlockerChecked"]);
                this.IsCriticalChecked = bool.Parse(options["IsCriticalChecked"]);
                this.IsMajaorChecked = bool.Parse(options["IsMajaorChecked"]);
                this.IsMinorChecked = bool.Parse(options["IsMinorChecked"]);
                this.IsInfoChecked = bool.Parse(options["IsInfoChecked"]);
                this.IsFalsePositiveChecked = bool.Parse(options["IsFalsePositiveChecked"]);
                this.IsRemovedChecked = bool.Parse(options["IsRemovedChecked"]);
                this.IsFixedChecked = bool.Parse(options["IsFixedChecked"]);
            }
            else
            {
                this.IsStatusOpenChecked = true;
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusOpenChecked", "true");
                this.IsStatusClosedChecked = true;
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusClosedChecked", "true");
                this.IsStatusResolvedChecked = true;
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusResolvedChecked", "true");
                this.IsStatusConfirmedChecked = true;
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusConfirmedChecked", "true");
                this.IsStatusReopenedChecked = true;
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusReopenedChecked", "true");
                this.IsBlockerChecked = true;
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsBlockerChecked", "true");
                this.IsCriticalChecked = true;
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsCriticalChecked", "true");
                this.IsMajaorChecked = true;
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsMajaorChecked", "true");
                this.IsMinorChecked = true;
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsMinorChecked", "true");
                this.IsInfoChecked = true;
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsInfoChecked", "true");
                this.IsFalsePositiveChecked = true;
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsFalsePositiveChecked", "true");
                this.IsRemovedChecked = true;
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsRemovedChecked", "true");
                this.IsFixedChecked = true;
                this.vsHelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsFixedChecked", "true");
            }
        }

        #endregion

        #endregion
    }
}