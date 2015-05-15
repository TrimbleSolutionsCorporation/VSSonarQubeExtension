// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssueGridViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   The issue grid view viewModel.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.ViewModel.Helpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    

    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using VSSonarExtensionUi.Helpers;
    using VSSonarExtensionUi.Menu;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    ///     The issue grid view viewModel.
    /// </summary>
    [ImplementPropertyChanged]
    public class IssueGridViewModel : IViewModelBase, IDataModel, IFilterCommand, IFilterOption
    {
        #region Fields

        /// <summary>
        ///     The data grid options key.
        /// </summary>
        private readonly string dataGridOptionsKey = "DataGridOptions";

        /// <summary>
        ///     The filter.
        /// </summary>
        private readonly IFilter filter;

        /// <summary>
        ///     The viewModel.
        /// </summary>
        private readonly SonarQubeViewModel model;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueGridViewModel"/> class.
        /// </summary>
        /// <param name="model">
        /// The viewModel.
        /// </param>
        /// <param name="rowContextMenu">
        /// The row Context Menu.
        /// </param>
        /// <param name="gridId">
        /// The grid Id.
        /// </param>
        public IssueGridViewModel(SonarQubeViewModel model, bool rowContextMenu, string gridId, bool showSqaleRating, IConfigurationHelper helper)
        {
            this.dataGridOptionsKey += gridId;
            this.model = model;
            this.Vsenvironmenthelper = model.VsHelper;
            this.configurationHelper = helper;
            this.AllIssues = new AsyncObservableCollection<Issue>();
            this.Issues = new AsyncObservableCollection<Issue>();

            this.OpenInVsCommand = new RelayCommand<IList>(this.OnOpenInVsCommand);
            this.MouseEventCommand = new RelayCommand(this.OnMouseEventCommand);

            this.SelectionChangedCommand = new RelayCommand<IList>(
                items =>
                    {
                        this.SelectedItems = items;
                        this.IssuesCounter = this.Issues.Count.ToString(CultureInfo.InvariantCulture);
                    });

            this.ColumnHeaderChangedCommand = new RelayCommand(this.OnColumnHeaderChangedCommand);
            this.RestoreUserSettingsInIssuesDataGrid();

            this.ShowHeaderContextMenu = true;
            this.ContextVisibilityMenuItems = this.CreateColumnVisibiltyMenu();
            this.ShowContextMenu = rowContextMenu;
            this.ContextMenuItems = this.CreateRowContextMenu();

            this.filter = new IssueFilter(this);
            this.InitFilterCommanding();

            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;

            this.ShowSqaleRating = showSqaleRating;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the all issues.
        /// </summary>
        public AsyncObservableCollection<Issue> AllIssues { get; set; }

        /// <summary>
        ///     Gets or sets the assignee index.
        /// </summary>
        public int AssigneeIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether assignee visible.
        /// </summary>
        public bool AssigneeVisible { get; set; }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the busy indicator tooltip.
        /// </summary>
        public string BusyIndicatorTooltip { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can hide coloumn command.
        /// </summary>
        public bool CanHideColoumnCommand { get; set; }

        /// <summary>
        ///     Gets or sets the clear clear filter term rule command.
        /// </summary>
        public ICommand ClearClearFilterTermRuleCommand { get; set; }

        /// <summary>
        ///     Gets or sets the clear filter term assignee command.
        /// </summary>
        public ICommand ClearFilterTermAssigneeCommand { get; set; }

        /// <summary>
        ///     Gets or sets the clear filter term component command.
        /// </summary>
        public ICommand ClearFilterTermComponentCommand { get; set; }

        /// <summary>
        ///     Gets or sets the clear filter term is new command.
        /// </summary>
        public ICommand ClearFilterTermIsNewCommand { get; set; }

        /// <summary>
        ///     Gets or sets the clear filter term message command.
        /// </summary>
        public ICommand ClearFilterTermMessageCommand { get; set; }

        /// <summary>
        ///     Gets or sets the clear filter term project command.
        /// </summary>
        public ICommand ClearFilterTermProjectCommand { get; set; }

        /// <summary>
        ///     Gets or sets the clear filter term resolution command.
        /// </summary>
        public ICommand ClearFilterTermResolutionCommand { get; set; }

        /// <summary>
        ///     Gets or sets the clear filter term severity command.
        /// </summary>
        public ICommand ClearFilterTermSeverityCommand { get; set; }

        /// <summary>
        ///     Gets or sets the clear filter term status command.
        /// </summary>
        public ICommand ClearFilterTermStatusCommand { get; set; }

        /// <summary>
        ///     Gets or sets the close date index.
        /// </summary>
        public int CloseDateIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether close date visible.
        /// </summary>
        public bool CloseDateVisible { get; set; }

        /// <summary>
        ///     Gets or sets the column header changed command.
        /// </summary>
        public ICommand ColumnHeaderChangedCommand { get; set; }

        /// <summary>
        ///     Gets or sets the component index.
        /// </summary>
        public int ComponentIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether component visible.
        /// </summary>
        public bool ComponentVisible { get; set; }

        /// <summary>
        ///     Gets or sets the context menu items.
        /// </summary>
        public ObservableCollection<IMenuItem> ContextMenuItems { get; set; }

        /// <summary>
        ///     Gets or sets the context visibility menu items.
        /// </summary>
        public ObservableCollection<IMenuItem> ContextVisibilityMenuItems { get; set; }

        /// <summary>
        ///     Gets or sets the creation date index.
        /// </summary>
        public int CreationDateIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether Creation Date visible.
        /// </summary>
        public bool CreationDateVisible { get; set; }

        /// <summary>
        ///     Gets or sets the effort to fix index.
        /// </summary>
        public int EffortToFixIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether effort to fix is visible.
        /// </summary>
        public bool EffortToFixVisible { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether extension is busy.
        /// </summary>
        public bool ExtensionIsBusy { get; set; }

        /// <summary>
        ///     Gets or sets the filter apply command.
        /// </summary>
        public ICommand FilterApplyCommand { get; set; }

        /// <summary>
        ///     Gets or sets the filter clear all command.
        /// </summary>
        public ICommand FilterClearAllCommand { get; set; }

        /// <summary>
        ///     Gets or sets the filter term assignee.
        /// </summary>
        public string FilterTermAssignee { get; set; }

        /// <summary>
        ///     Gets or sets the filter term component.
        /// </summary>
        public string FilterTermComponent { get; set; }

        /// <summary>
        ///     Gets or sets the filter term is new.
        /// </summary>
        public string FilterTermIsNew { get; set; }

        /// <summary>
        ///     Gets or sets the filter term message.
        /// </summary>
        public string FilterTermMessage { get; set; }

        /// <summary>
        ///     Gets or sets the filter term project.
        /// </summary>
        public string FilterTermProject { get; set; }

        /// <summary>
        ///     Gets or sets the filter term resolution.
        /// </summary>
        public Resolution? FilterTermResolution { get; set; }

        /// <summary>
        ///     Gets or sets the filter term rule.
        /// </summary>
        public string FilterTermRule { get; set; }

        /// <summary>
        ///     Gets or sets the filter term severity.
        /// </summary>
        public Severity? FilterTermSeverity { get; set; }

        /// <summary>
        ///     Gets or sets the filter term status.
        /// </summary>
        public IssueStatus? FilterTermStatus { get; set; }

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        ///     Gets or sets the id index.
        /// </summary>
        public int IdIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether id visible.
        /// </summary>
        public bool IdVisible { get; set; }

        /// <summary>
        ///     Gets or sets the id index.
        /// </summary>
        public int IsNewIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether id visible.
        /// </summary>
        public bool IsNewVisible { get; set; }

        /// <summary>
        ///     Gets or sets the issues.
        /// </summary>
        [AlsoNotifyFor("IssuesCounter")]
        public AsyncObservableCollection<Issue> Issues { get; set; }

        /// <summary>
        ///     Gets or sets the issues counter.
        /// </summary>
        public string IssuesCounter { get; set; }

        /// <summary>
        ///     Gets or sets the key index.
        /// </summary>
        public int KeyIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether key visible.
        /// </summary>
        public bool KeyVisible { get; set; }

        /// <summary>
        ///     Gets or sets the line index.
        /// </summary>
        public int LineIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether line visible.
        /// </summary>
        public bool LineVisible { get; set; }

        /// <summary>
        ///     Gets or sets the message index.
        /// </summary>
        public int MessageIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether message visible.
        /// </summary>
        public bool MessageVisible { get; set; }

        /// <summary>
        ///     Gets or sets the mouse event command.
        /// </summary>
        public ICommand MouseEventCommand { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the open in vs command.
        /// </summary>
        public ICommand OpenInVsCommand { get; set; }

        /// <summary>
        ///     Gets or sets the project index.
        /// </summary>
        public int ProjectIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether project visible.
        /// </summary>
        public bool ProjectVisible { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether query for issues is running.
        /// </summary>
        public bool QueryForIssuesIsRunning { get; set; }

        /// <summary>
        ///     Gets or sets the resolution index.
        /// </summary>
        public int ResolutionIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether resolution visible.
        /// </summary>
        public bool ResolutionVisible { get; set; }

        /// <summary>
        ///     Gets or sets the rule index.
        /// </summary>
        public int RuleIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether rule visible.
        /// </summary>
        public bool RuleVisible { get; set; }

        /// <summary>
        ///     Gets or sets the selected items.
        /// </summary>
        public IList SelectedItems { get; set; }

        /// <summary>
        ///     Gets or sets the selection changed command.
        /// </summary>
        public ICommand SelectionChangedCommand { get; set; }

        /// <summary>
        ///     Gets or sets the severity index.
        /// </summary>
        public int SeverityIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether severity visible.
        /// </summary>
        public bool SeverityVisible { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether show context menu.
        /// </summary>
        public bool ShowContextMenu { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether show header context menu.
        /// </summary>
        public bool ShowHeaderContextMenu { get; set; }

        /// <summary>
        ///     Gets or sets the status index.
        /// </summary>
        public int StatusIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether status visible.
        /// </summary>
        public bool StatusVisible { get; set; }

        /// <summary>
        ///     Gets or sets the update date index.
        /// </summary>
        public int UpdateDateIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether update date visible.
        /// </summary>
        public bool UpdateDateVisible { get; set; }

        /// <summary>
        ///     Gets or sets the violation id index.
        /// </summary>
        public int ViolationIdIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether violation id visible.
        /// </summary>
        public bool ViolationIdVisible { get; set; }

        /// <summary>
        ///     Gets or sets the vsenvironmenthelper.
        /// </summary>
        public IVsEnvironmentHelper Vsenvironmenthelper { get; set; }

        public int SelectedIndex { get; set; }

        #endregion

        #region Public Methods and Operators

        public void GoToNextIssue()
        {
            if (this.SelectedIndex < this.Issues.Count)
            {
                this.SelectedIndex++;
            }

            this.OnOpenInVsCommand(this.SelectedItems);
        }

        public void GoToPrevIssue()
        {
            if (this.SelectedIndex > 0)
            {
                this.SelectedIndex--;
            }

            this.OnOpenInVsCommand(this.SelectedItems);
        }

        /// <summary>
        /// The is not filtered.
        /// </summary>
        /// <param name="issue">
        /// The issue.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsNotFiltered(Issue issue)
        {
            return this.filter.FilterFunction(issue);
        }

        /// <summary>
        /// The on open in vs command.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        public void OnOpenInVsCommand(object parameter)
        {
            var list = parameter as IList;
            if (list == null)
            {
                return;
            }

            List<Issue> selectedItemsList = list.Cast<Issue>().ToList();

            foreach (Issue issue in selectedItemsList)
            {
                string filename = issue.Component;
                try
                {
                    List<Resource> resources =
                        this.model.SonarRestConnector.GetResourcesData(
                            AuthtenticationHelper.AuthToken, 
                            issue.Component);
                    filename = resources[0].Name;
                }
                catch (Exception ex)
                {
                    this.model.NotificationManager.ReportException(ex);
                }

                if (this.Vsenvironmenthelper == null)
                {
                    return;
                }


                try
                {
                    var path = this.model.SonarKeyTranslator.TranslateKey(issue.Component, this.Vsenvironmenthelper);
                    this.Vsenvironmenthelper.OpenResourceInVisualStudio(this.model.OpenSolutionPath, path, issue.Line);
                }
                catch (Exception ex)
                {
                    this.model.NotificationManager.ReportMessage(new Message() { Id = "OnInEditor", Data = ex.Message + " : " + issue.Component });
                    this.model.NotificationManager.ReportMessage(new Message() { Id = "OnInEditor ", Data = "Solution = " + this.model.OpenSolutionPath });
                    this.model.NotificationManager.ReportMessage(new Message() { Id = "OnInEditor", Data = "Project = " + this.model.AssociatedProject });
                    this.model.NotificationManager.ReportException(ex);
                }
            }
        }

        /// <summary>
        /// The process changes.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="propertyChangedEventArgs">
        /// The property changed event args.
        /// </param>
        public void ProcessChanges(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
        }

        /// <summary>
        ///     The refresh view.
        /// </summary>
        public void RefreshView()
        {
            this.OnFilterApplyCommand();
        }

        /// <summary>
        ///     The restore user settings.
        /// </summary>
        public void RestoreUserSettingsInIssuesDataGrid()
        {
            if (this.configurationHelper == null)
            {
                this.ResetWindowDefaults();
                return;
            }

            this.ReadWindowOptions(this.configurationHelper, this.dataGridOptionsKey);
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
        /// The update issues.
        /// </summary>
        /// <param name="listOfIssues">
        /// The list of issues.
        /// </param>
        public void UpdateIssues(List<Issue> listOfIssues)
        {
            Application.Current.Dispatcher.Invoke(
                delegate
                    {
                        try
                        {
                            this.Issues.Clear();
                            this.AllIssues.Clear();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Message: " + ex.Message);
                        }

                        foreach (Issue listOfIssue in listOfIssues)
                        {
                            this.AllIssues.Add(listOfIssue);
                            this.Issues.Add(listOfIssue);
                        }
                    });

            this.RefreshView();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The clear filter.
        /// </summary>
        private void ClearFilter()
        {
            this.Issues.Clear();
            foreach (Issue issue in this.AllIssues)
            {
                try
                {
                    if (this.filter.FilterFunction(issue))
                    {
                        this.Issues.Add(issue);
                    }
                }
                catch (Exception ex)
                {
                    this.model.Logger.WriteMessage("Filter Failed: " + ex.Message);
                    this.Issues.Add(issue);
                }
            }

            this.model.OnIssuesChangeEvent();
        }

        /// <summary>
        ///     The create column visibilty menu.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>ObservableCollection</cref>
        ///     </see>
        ///     .
        /// </returns>
        private ObservableCollection<IMenuItem> CreateColumnVisibiltyMenu()
        {
            PropertyInfo[] props = typeof(Issue).GetProperties();
            List<string> submenus = props.Select(propertyInfo => propertyInfo.Name).ToList();

            var menu = new ObservableCollection<IMenuItem> { ShowHideIssueColumn.MakeMenu(this, this.configurationHelper, submenus, this.dataGridOptionsKey) };

            return menu;
        }

        /// <summary>
        ///     The create row context menu.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>ObservableCollection</cref>
        ///     </see>
        ///     .
        /// </returns>
        private ObservableCollection<IMenuItem> CreateRowContextMenu()
        {
            var menu = new ObservableCollection<IMenuItem>
                           {
                               ChangeStatusHandler.MakeMenu(
                                   AuthtenticationHelper.AuthToken, 
                                   this.model.SonarRestConnector, 
                                   this, this.model), 
                               OpenResourceMenu.MakeMenu(
                                   AuthtenticationHelper.AuthToken, 
                                   this.model.SonarRestConnector, 
                                   this.Vsenvironmenthelper, 
                                   this)
                           };

            return menu;
        }

        /// <summary>
        ///     The init filter commanding.
        /// </summary>
        private void InitFilterCommanding()
        {
            this.FilterTermMessage = string.Empty;
            this.FilterTermComponent = string.Empty;
            this.FilterTermProject = string.Empty;
            this.FilterTermRule = string.Empty;
            this.FilterTermAssignee = string.Empty;
            this.FilterTermStatus = null;
            this.FilterTermSeverity = null;
            this.FilterTermResolution = null;
            this.FilterTermIsNew = null;

            this.ClearClearFilterTermRuleCommand = new RelayCommand<object>(this.OnClearClearFilterTermRuleCommand);
            this.ClearFilterTermAssigneeCommand = new RelayCommand<object>(this.OnClearFilterTermAssigneeCommand);
            this.ClearFilterTermComponentCommand = new RelayCommand<object>(this.OnClearFilterTermComponentCommand);
            this.ClearFilterTermIsNewCommand = new RelayCommand<object>(this.OnClearFilterTermIsNewCommand);
            this.ClearFilterTermMessageCommand = new RelayCommand<object>(this.OnClearFilterTermMessageCommand);
            this.ClearFilterTermProjectCommand = new RelayCommand<object>(this.OnClearFilterTermProjectCommand);
            this.ClearFilterTermResolutionCommand = new RelayCommand<object>(this.OnClearFilterTermResolutionCommand);
            this.ClearFilterTermSeverityCommand = new RelayCommand<object>(this.OnClearFilterTermSeverityCommand);
            this.ClearFilterTermStatusCommand = new RelayCommand<object>(this.OnClearFilterTermStatusCommand);
            this.FilterApplyCommand = new RelayCommand(this.OnFilterApplyCommand);
            this.FilterClearAllCommand = new RelayCommand<object>(this.OnFilterClearAllCommand);
        }

        public void UpdateVsService(IVsEnvironmentHelper service)
        {
            if (this.ContextMenuItems == null)
            {
                return;
            }

            foreach (var menu in this.ContextMenuItems)
            {
                menu.UpdateServices(service);
            }
        }

        /// <summary>
        /// The on clear clear filter term rule command.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnClearClearFilterTermRuleCommand(object obj)
        {
            this.FilterTermRule = string.Empty;
            this.ClearFilter();
        }

        /// <summary>
        /// The on clear filter term assignee command.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnClearFilterTermAssigneeCommand(object obj)
        {
            this.FilterTermAssignee = string.Empty;
            this.ClearFilter();
        }

        /// <summary>
        /// The on clear filter term component command.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnClearFilterTermComponentCommand(object obj)
        {
            this.FilterTermComponent = string.Empty;
            this.ClearFilter();
        }

        /// <summary>
        /// The on clear filter term is new command.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnClearFilterTermIsNewCommand(object obj)
        {
            this.FilterTermIsNew = null;
            this.ClearFilter();
        }

        /// <summary>
        /// The on clear filter term message command.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnClearFilterTermMessageCommand(object obj)
        {
            this.FilterTermMessage = string.Empty;
            this.ClearFilter();
        }

        /// <summary>
        /// The on clear filter term project command.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnClearFilterTermProjectCommand(object obj)
        {
            this.FilterTermProject = string.Empty;
            this.ClearFilter();
        }

        /// <summary>
        /// The on clear filter term resolution command.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnClearFilterTermResolutionCommand(object obj)
        {
            this.FilterTermResolution = null;
            this.ClearFilter();
        }

        /// <summary>
        /// The on clear filter term severity command.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnClearFilterTermSeverityCommand(object obj)
        {
            this.FilterTermSeverity = null;
            this.ClearFilter();
        }

        /// <summary>
        /// The on clear filter term status command.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnClearFilterTermStatusCommand(object obj)
        {
            this.FilterTermStatus = null;
            this.ClearFilter();
        }

        /// <summary>
        ///     The on column header changed command.
        /// </summary>
        private void OnColumnHeaderChangedCommand()
        {
            this.SaveWindowOptions();
        }

        /// <summary>
        /// The on filter apply command.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnFilterApplyCommand()
        {
            if (this.AllIssues == null || !this.AllIssues.Any())
            {
                return;
            }

            this.ResetStatistics();
            this.Issues.Clear();
            foreach (Issue issue in this.AllIssues)
            {
                try
                {
                    if (this.filter.FilterFunction(issue))
                    {
                        this.PopulateStatistics(issue);
                        this.Issues.Add(issue);
                    }
                }
                catch (Exception ex)
                {
                    this.model.Logger.WriteMessage("Filter Failed: " + ex.Message);
                    this.Issues.Add(issue);
                }
            }

            this.UpdateStatistics();
            this.model.OnIssuesChangeEvent();
        }

        public void RefreshStatistics()
        {
            if (this.Issues == null || !this.Issues.Any())
            {
                return;
            }

            this.ResetStatistics();
            foreach (Issue issue in this.Issues)
            {
                this.PopulateStatistics(issue);
            }

            this.UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            this.NumberOfIssuesStr = "Issues: " + this.NumberOfIssues;
            this.NumberOfBlockersStr = "Blockers: " + this.NumberOfBlockers;
            this.NumberOfCriticalsStr = "Criticals: " + this.NumberOfCriticals;
            this.NumberOfMajorsStr = "Majors: " + this.NumberOfMajors;
            this.TechnicalDebtStr = "Debt: " + this.TechnicalDebt + " mn";

            if (this.ShowSqaleRating)
            {
                //this.SqaleRatingStr = "Rating: " + this.SqaleRating;
            }
        }

        private void PopulateStatistics(Issue issue)
        {
            this.NumberOfIssues++;

            switch (issue.Severity)
            {
                case Severity.BLOCKER: this.NumberOfBlockers++;
                    break;
                case Severity.CRITICAL: this.NumberOfCriticals++;
                    break;
                case Severity.MAJOR: this.NumberOfMajors++;
                    break;
            }

            if (!string.IsNullOrEmpty(issue.Debt))
            {
                try
                {
                    var debt = issue.Debt.Replace("min", "").Replace("sec", "").Replace("hour", "").Replace("day", "").Replace("d", "").Replace("h", "");
                    this.TechnicalDebt += int.Parse(debt);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public void ResetStatistics()
        {
            this.NumberOfBlockers = 0;
            this.NumberOfCriticals = 0;
            this.NumberOfMajors = 0;
            this.NumberOfIssues = 0;
            this.TechnicalDebt = 0;

            this.UpdateStatistics();
        }

        public long TechnicalDebt { get; set; }

        public int NumberOfIssues { get; set; }

        public int NumberOfMajors { get; set; }

        public int NumberOfCriticals { get; set; }

        public int NumberOfBlockers { get; set; }

        public string TechnicalDebtStr { get; set; }

        public string NumberOfIssuesStr { get; set; }

        public string NumberOfMajorsStr { get; set; }

        public string NumberOfCriticalsStr { get; set; }

        public string NumberOfBlockersStr { get; set; }

        public bool ShowSqaleRating { get; set; }

        public string SqaleRating { get; set; }
        public string SqaleRatingStr { get; set; }
        /// <summary>Gets or sets a value indicating whether debt visible.</summary>
        public bool DebtVisible { get; set; }

        /// <summary>Gets or sets the debt index.</summary>
        public int DebtIndex { get; set; }

        /// <summary>
        /// The on filter clear all command.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnFilterClearAllCommand(object obj)
        {
            this.FilterTermStatus = null;
            this.FilterTermSeverity = null;
            this.FilterTermResolution = null;
            this.FilterTermIsNew = null;

            this.FilterTermProject = string.Empty;
            this.FilterTermMessage = string.Empty;
            this.FilterTermComponent = string.Empty;
            this.FilterTermAssignee = string.Empty;
            this.FilterTermRule = string.Empty;

            if (this.AllIssues == null || this.AllIssues.Count == 0)
            {
                return;
            }

            try
            {
                this.ClearFilter();
            }
            catch (Exception ex)
            {
                this.model.NotificationManager.ReportException(ex);
            }
        }

        /// <summary>
        ///     The on mouse event command.
        /// </summary>
        private void OnMouseEventCommand()
        {
        }

        private static string GetValueForOption(IConfigurationHelper helper, string key, string defaultvalue, string owner)
        {
            try
            {
                return helper.ReadSetting(Context.UIProperties, owner, key).Value;
            }
            catch (Exception)
            {
                return defaultvalue;
            }
        }

        /// <summary>
        /// The read window options.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        private void ReadWindowOptions(IConfigurationHelper options, string owner)
        {
            try
            {
                this.ComponentIndex = int.Parse(GetValueForOption(options, "ComponentIndex", "1", owner), CultureInfo.InvariantCulture);
                this.LineIndex = int.Parse(GetValueForOption(options, "LineIndex", "2", owner), CultureInfo.InvariantCulture);
                this.AssigneeIndex = int.Parse(GetValueForOption(options, "AssigneeIndex", "3", owner), CultureInfo.InvariantCulture);
                this.MessageIndex = int.Parse(GetValueForOption(options, "MessageIndex", "4", owner), CultureInfo.InvariantCulture);
                this.StatusIndex = int.Parse(GetValueForOption(options, "StatusIndex", "5", owner), CultureInfo.InvariantCulture);
                this.SeverityIndex = int.Parse(GetValueForOption(options, "SeverityIndex", "6", owner), CultureInfo.InvariantCulture);
                this.RuleIndex = int.Parse(GetValueForOption(options, "RuleIndex", "7", owner), CultureInfo.InvariantCulture);
                this.CreationDateIndex = int.Parse(GetValueForOption(options, "CreationDateIndex", "8", owner), CultureInfo.InvariantCulture);
                this.ProjectIndex = int.Parse(GetValueForOption(options, "ProjectIndex", "9", owner), CultureInfo.InvariantCulture);
                this.ResolutionIndex = int.Parse(GetValueForOption(options, "ResolutionIndex", "10", owner), CultureInfo.InvariantCulture);
                this.EffortToFixIndex = int.Parse(GetValueForOption(options, "EffortToFixIndex", "11", owner), CultureInfo.InvariantCulture);
                this.UpdateDateIndex = int.Parse(GetValueForOption(options, "UpdateDateIndex", "12", owner), CultureInfo.InvariantCulture);
                this.CloseDateIndex = int.Parse(GetValueForOption(options, "CloseDateIndex", "13", owner), CultureInfo.InvariantCulture);
                this.KeyIndex = int.Parse(GetValueForOption(options, "KeyIndex", "14", owner), CultureInfo.InvariantCulture);
                this.IdIndex = int.Parse(GetValueForOption(options, "IdIndex", "15", owner), CultureInfo.InvariantCulture);
                this.IsNewIndex = int.Parse(GetValueForOption(options, "IsNewIndex", "16", owner), CultureInfo.InvariantCulture);
                this.DebtIndex = int.Parse(GetValueForOption(options, "DebtIndex", "17", owner), CultureInfo.InvariantCulture);

                this.ComponentVisible = bool.Parse(GetValueForOption(options, "ComponentVisible", "true", owner));
                this.LineVisible = bool.Parse(GetValueForOption(options, "LineVisible", "true", owner));
                this.AssigneeVisible = bool.Parse(GetValueForOption(options, "AssigneeVisible", "true", owner));
                this.MessageVisible = bool.Parse(GetValueForOption(options, "MessageVisible", "true", owner));
                this.StatusVisible = bool.Parse(GetValueForOption(options, "StatusVisible", "true", owner));
                this.SeverityVisible = bool.Parse(GetValueForOption(options, "SeverityVisible", "true", owner));
                this.RuleVisible = bool.Parse(GetValueForOption(options, "RuleVisible", "true", owner));
                this.CreationDateVisible = bool.Parse(GetValueForOption(options, "CreationDateVisible", "true", owner));
                this.ProjectVisible = bool.Parse(GetValueForOption(options, "ProjectVisible", "true", owner));
                this.ResolutionVisible = bool.Parse(GetValueForOption(options, "ResolutionVisible", "true", owner));
                this.EffortToFixVisible = bool.Parse(GetValueForOption(options, "EffortToFixVisible", "true", owner));
                this.UpdateDateVisible = bool.Parse(GetValueForOption(options, "UpdateDateVisible", "true", owner));
                this.CloseDateVisible = bool.Parse(GetValueForOption(options, "CloseDateVisible", "true", owner));
                this.KeyVisible = bool.Parse(GetValueForOption(options, "KeyVisible", "true", owner));
                this.IdVisible = bool.Parse(GetValueForOption(options, "IdVisible", "true", owner));
                this.IsNewVisible = bool.Parse(GetValueForOption(options, "IsNewVisible", "true", owner));
                this.DebtVisible = bool.Parse(GetValueForOption(options, "DebtVisible", "true", owner));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        ///     The reset window defaults.
        /// </summary>
        private void ResetWindowDefaults()
        {
            this.ComponentIndex = 0;
            this.MessageIndex = 1;
            this.LineIndex = 2;

            this.CreationDateIndex = 3;
            this.CloseDateIndex = 4;
            this.EffortToFixIndex = 5;            
            this.ProjectIndex = 6;
            this.UpdateDateIndex = 7;
            this.StatusIndex = 8;
            this.SeverityIndex = 9;
            this.RuleIndex = 10;
            this.ResolutionIndex = 11;
            this.AssigneeIndex = 12;
            this.IsNewIndex = 13;
            this.KeyIndex = 14;
            this.IdIndex = 15;
            this.ViolationIdIndex = 16;

            this.ComponentVisible = true;
            this.LineVisible = true;
            this.AssigneeVisible = false;
            this.MessageVisible = true;
            this.StatusVisible = true;
            this.SeverityVisible = true;
            this.RuleVisible = false;
            this.CreationDateVisible = false;
            this.ProjectVisible = false;
            this.ResolutionVisible = false;
            this.EffortToFixVisible = true;
            this.UpdateDateVisible = false;
            this.CloseDateVisible = false;
            this.KeyVisible = false;
            this.IdVisible = false;
        }

        /// <summary>
        ///     The write window options.
        /// </summary>
        private void SaveWindowOptions()
        {
            if (this.ComponentIndex < 0 || this.LineIndex < 0 || this.AssigneeIndex < 0 || this.MessageIndex < 0 || this.StatusIndex < 0
                || this.RuleIndex < 0 || this.CreationDateIndex < 0 || this.ProjectIndex < 0 || this.ResolutionIndex < 0 || this.EffortToFixIndex < 0
                || this.UpdateDateIndex < 0 || this.CloseDateIndex < 0 || this.KeyIndex < 0 || this.IsNewIndex < 0 || this.ViolationIdIndex < 0)
            {
                return;
            }

            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "ComponentIndex", 
                this.ComponentIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "LineIndex", 
                this.LineIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "AssigneeIndex", 
                this.AssigneeIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "MessageIndex", 
                this.MessageIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "StatusIndex", 
                this.StatusIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "SeverityIndex", 
                this.SeverityIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "RuleIndex", 
                this.RuleIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "CreationDateIndex", 
                this.CreationDateIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "ProjectIndex", 
                this.ProjectIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "ResolutionIndex", 
                this.ResolutionIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "EffortToFixIndex", 
                this.EffortToFixIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "UpdateDateIndex", 
                this.UpdateDateIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "CloseDateIndex", 
                this.CloseDateIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "KeyIndex", 
                this.KeyIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "IdIndex", 
                this.IdIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "IsNewIndex", 
                this.IsNewIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                this.dataGridOptionsKey, 
                "ViolationIdIndex", 
                this.ViolationIdIndex.ToString(CultureInfo.InvariantCulture));

            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "CreationDateVisible", this.CreationDateVisible.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "CloseDateVisible", this.CloseDateVisible.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "EffortToFixVisible", this.EffortToFixVisible.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "ProjectVisible", this.ProjectVisible.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "UpdateDateVisible", this.UpdateDateVisible.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "StatusVisible", this.StatusVisible.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "SeverityVisible", this.SeverityVisible.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "RuleVisible", this.RuleVisible.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "ResolutionVisible", this.ResolutionVisible.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "AssigneeVisible", this.AssigneeVisible.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "IsNewVisible", this.IsNewVisible.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "KeyVisible", this.KeyVisible.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "IdVisible", this.IdVisible.ToString());
            this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, "ViolationIdVisible", this.ViolationIdVisible.ToString());
        }

        /// <summary>
        ///     The write window options.
        /// </summary>
        private void WriteWindowOptions()
        {
            int i = 0;
            foreach (PropertyInfo propertyInfo in typeof(Issue).GetProperties())
            {
                this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, 
                    this.dataGridOptionsKey, 
                    propertyInfo.Name + "Index", 
                    i.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteOptionInApplicationData(Context.UIProperties, this.dataGridOptionsKey, propertyInfo.Name + "Visible", "true");
                i++;
            }
        }

        #endregion

        public readonly IConfigurationHelper configurationHelper;
    }
}