// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssuesSearchViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   The issues search view model.
// </summary>
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
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    using ExtensionTypes;

    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using SonarRestService;

    using VSSonarExtensionUi.ViewModel.Helpers;

    using VSSonarPlugins;

    /// <summary>
    ///     The issues search view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class IssuesSearchViewModel : IAnalysisViewModelBase
    {
        #region Constants

        /// <summary>
        ///     The issues filter view model key.
        /// </summary>
        private const string IssuesFilterViewModelKey = "IssuesFilterViewModel";

        #endregion

        #region Fields

        /// <summary>
        ///     The sonar qube view model.
        /// </summary>
        private readonly SonarQubeViewModel sonarQubeViewModel;

        /// <summary>
        ///     The rest service.
        /// </summary>
        private ISonarRestService restService;

        /// <summary>
        ///     The show flyouts.
        /// </summary>
        private bool showFlyouts;

        /// <summary>
        ///     The vs helper.
        /// </summary>
        private IVsEnvironmentHelper visualStudioHelper;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuesSearchViewModel"/> class.
        /// </summary>
        /// <param name="sonarQubeViewModel">
        /// The sonar qube view model.
        /// </param>
        /// <param name="visualStudioHelper">
        /// The visual studio helper.
        /// </param>
        /// <param name="restService">
        /// The rest service.
        /// </param>
        public IssuesSearchViewModel(SonarQubeViewModel sonarQubeViewModel, IVsEnvironmentHelper visualStudioHelper, ISonarRestService restService)
        {
            this.visualStudioHelper = visualStudioHelper;
            this.restService = restService;
            this.sonarQubeViewModel = sonarQubeViewModel;
            this.Header = "Issues";
            this.SonarVersion = 4.5;
            this.UsersList = new ObservableCollection<User>();
            this.IssuesGridView = new IssueGridViewModel(sonarQubeViewModel, true);

            this.InitCommanding();

            this.ForeGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The analysis mode has change.
        /// </summary>
        public event ChangedEventHandler IssuesReadyForCollecting;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the assignee in filter.
        /// </summary>
        public User Assignee { get; set; }

        /// <summary>
        ///     Gets or sets the associated project.
        /// </summary>
        public Resource AssociatedProject { get; set; }

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
        ///     Gets or sets the configuration.
        /// </summary>
        public ISonarConfiguration Configuration { get; set; }

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
        ///     Gets or sets the service provier.
        /// </summary>
        public IServiceProvider ServiceProvier { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether show flyouts.
        /// </summary>
        public bool ShowFlyouts
        {
            get
            {
                return this.showFlyouts;
            }

            set
            {
                this.showFlyouts = value;
                this.SizeOfFlyout = value ? 150 : 0;
            }
        }

        /// <summary>
        ///     Gets the size of flyout.
        /// </summary>
        public int SizeOfFlyout { get; private set; }

        /// <summary>
        ///     Gets the sonar version.
        /// </summary>
        public double SonarVersion { get; private set; }

        /// <summary>
        ///     Gets or sets the status bar.
        /// </summary>
        public IVSSStatusBar StatusBar { get; set; }

        /// <summary>
        ///     Gets or sets the users list.
        /// </summary>
        public ObservableCollection<User> UsersList { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The end data association.
        /// </summary>
        public void EndDataAssociation()
        {
            this.IssuesGridView.Issues.Clear();
            this.AssociatedProject = null;
            this.CanQUeryIssues = false;
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
            if (this.CanQUeryIssues)
            {
                return
                    this.IssuesGridView.Issues.Where(
                        issue =>
                        this.IssuesGridView.IsNotFiltered(issue) && (file.Key.Equals(issue.Component) || file.Key.Equals(issue.ComponentSafe)))
                        .ToList();
            }

            return new List<Issue>();
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
            this.CanQUeryIssues = true;
            this.sonarQubeViewModel.IsExtensionBusy = false;

            List<User> usortedList = this.restService.GetUserList(sonarCubeConfiguration);
            if (usortedList != null && usortedList.Count > 0)
            {
                this.UsersList = new ObservableCollection<User>(usortedList.OrderBy(i => i.Login));
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
        ///     The on selected view changed.
        /// </summary>
        public void OnSelectedViewChanged()
        {
            this.OnAnalysisModeHasChange(EventArgs.Empty);
            Debug.WriteLine("Name Changed");
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
            this.DocumentInView = documentInView;
            this.ResourceInEditor = fullName;
            this.OnSelectedViewChanged();
        }

        /// <summary>
        ///     The retrieve issues using current filter.
        /// </summary>
        public void RetrieveIssuesUsingCurrentFilter()
        {
            this.SaveFilterToDisk();

            if (this.SonarVersion < 3.6)
            {
                this.IssuesGridView.UpdateIssues(this.restService.GetIssuesForProjects(this.Configuration, this.AssociatedProject.Key));
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

            this.IssuesGridView.UpdateIssues(this.restService.GetIssues(this.Configuration, request, this.AssociatedProject.Key));
        }

        /// <summary>
        ///     The save filter to disk.
        /// </summary>
        public void SaveFilterToDisk()
        {
            if (this.visualStudioHelper != null)
            {
                this.visualStudioHelper.WriteOptionInApplicationData(
                    IssuesFilterViewModelKey, 
                    "IsStatusOpenChecked", 
                    this.IsStatusOpenChecked.ToString(CultureInfo.InvariantCulture));
                this.visualStudioHelper.WriteOptionInApplicationData(
                    IssuesFilterViewModelKey, 
                    "IsStatusClosedChecked", 
                    this.IsStatusClosedChecked.ToString(CultureInfo.InvariantCulture));
                this.visualStudioHelper.WriteOptionInApplicationData(
                    IssuesFilterViewModelKey, 
                    "IsStatusResolvedChecked", 
                    this.IsStatusResolvedChecked.ToString(CultureInfo.InvariantCulture));
                this.visualStudioHelper.WriteOptionInApplicationData(
                    IssuesFilterViewModelKey, 
                    "IsStatusConfirmedChecked", 
                    this.IsStatusConfirmedChecked.ToString(CultureInfo.InvariantCulture));
                this.visualStudioHelper.WriteOptionInApplicationData(
                    IssuesFilterViewModelKey, 
                    "IsStatusReopenedChecked", 
                    this.IsStatusReopenedChecked.ToString(CultureInfo.InvariantCulture));
                this.visualStudioHelper.WriteOptionInApplicationData(
                    IssuesFilterViewModelKey, 
                    "IsBlockerChecked", 
                    this.IsBlockerChecked.ToString(CultureInfo.InvariantCulture));
                this.visualStudioHelper.WriteOptionInApplicationData(
                    IssuesFilterViewModelKey, 
                    "IsCriticalChecked", 
                    this.IsCriticalChecked.ToString(CultureInfo.InvariantCulture));
                this.visualStudioHelper.WriteOptionInApplicationData(
                    IssuesFilterViewModelKey, 
                    "IsMajaorChecked", 
                    this.IsMajaorChecked.ToString(CultureInfo.InvariantCulture));
                this.visualStudioHelper.WriteOptionInApplicationData(
                    IssuesFilterViewModelKey, 
                    "IsMinorChecked", 
                    this.IsMinorChecked.ToString(CultureInfo.InvariantCulture));
                this.visualStudioHelper.WriteOptionInApplicationData(
                    IssuesFilterViewModelKey, 
                    "IsInfoChecked", 
                    this.IsInfoChecked.ToString(CultureInfo.InvariantCulture));
                this.visualStudioHelper.WriteOptionInApplicationData(
                    IssuesFilterViewModelKey, 
                    "IsFalsePositiveChecked", 
                    this.IsFalsePositiveChecked.ToString(CultureInfo.InvariantCulture));
                this.visualStudioHelper.WriteOptionInApplicationData(
                    IssuesFilterViewModelKey, 
                    "IsRemovedChecked", 
                    this.IsRemovedChecked.ToString(CultureInfo.InvariantCulture));
                this.visualStudioHelper.WriteOptionInApplicationData(
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
        }

        /// <summary>
        /// The update services.
        /// </summary>
        /// <param name="restServiceIn">
        /// The rest service in.
        /// </param>
        /// <param name="vsenvironmenthelperIn">
        /// The vsenvironmenthelper in.
        /// </param>
        /// <param name="statusBar">
        /// The status bar.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public void UpdateServices(
            ISonarRestService restServiceIn, 
            IVsEnvironmentHelper vsenvironmenthelperIn, 
            IVSSStatusBar statusBar, 
            IServiceProvider provider)
        {
            this.restService = restServiceIn;
            this.visualStudioHelper = vsenvironmenthelperIn;
            this.StatusBar = statusBar;
            this.ServiceProvier = provider;
        }

        #endregion

        #region Methods

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
            this.sonarQubeViewModel.IsExtensionBusy = true;

            var bw = new BackgroundWorker { WorkerReportsProgress = true };

            bw.RunWorkerCompleted += delegate
                {
                    Application.Current.Dispatcher.Invoke(
                        delegate
                            {
                                this.OnSelectedViewChanged();
                                this.CanQUeryIssues = true;
                                this.sonarQubeViewModel.IsExtensionBusy = false;
                            });
                };

            bw.DoWork +=
                delegate { this.IssuesGridView.UpdateIssues(this.restService.GetIssuesForProjects(this.Configuration, this.AssociatedProject.Key)); };

            bw.RunWorkerAsync();
        }

        /// <summary>
        ///     The on get all issues since last analysis command.
        /// </summary>
        private void OnGetAllIssuesSinceLastAnalysisCommand()
        {
            this.CanQUeryIssues = false;
            this.sonarQubeViewModel.IsExtensionBusy = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
                {
                    this.CanQUeryIssues = true;
                    this.sonarQubeViewModel.IsExtensionBusy = false;
                    this.OnSelectedViewChanged();
                };

            bw.DoWork +=
                delegate
                    {
                        this.IssuesGridView.UpdateIssues(
                            this.restService.GetIssuesForProjectsCreatedAfterDate(
                                this.Configuration, 
                                this.AssociatedProject.Key, 
                                this.AssociatedProject.Date));
                    };

            bw.RunWorkerAsync();
        }

        /// <summary>
        ///     The on get all my issues command.
        /// </summary>
        private void OnGetAllMyIssuesCommand()
        {
            this.CanQUeryIssues = false;
            this.sonarQubeViewModel.IsExtensionBusy = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
                {
                    this.CanQUeryIssues = true;
                    this.sonarQubeViewModel.IsExtensionBusy = false;
                    this.OnSelectedViewChanged();
                };

            bw.DoWork +=
                delegate
                    {
                        this.IssuesGridView.UpdateIssues(this.restService.GetAllIssuesByAssignee(this.Configuration, this.Configuration.Username));
                    };

            bw.RunWorkerAsync();
        }

        /// <summary>
        ///     The on get issues by filter command.
        /// </summary>
        private void OnGetIssuesByFilterCommand()
        {
            this.CanQUeryIssues = false;
            this.sonarQubeViewModel.IsExtensionBusy = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
                {
                    this.CanQUeryIssues = true;
                    this.sonarQubeViewModel.IsExtensionBusy = false;
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
            this.sonarQubeViewModel.IsExtensionBusy = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
                {
                    this.CanQUeryIssues = true;
                    this.sonarQubeViewModel.IsExtensionBusy = false;
                    this.OnSelectedViewChanged();
                };

            bw.DoWork +=
                delegate
                    {
                        this.IssuesGridView.UpdateIssues(
                            this.restService.GetIssuesByAssigneeInProject(this.Configuration, this.AssociatedProject.Key, this.Configuration.Username));
                    };

            bw.RunWorkerAsync();
        }

        #endregion
    }
}