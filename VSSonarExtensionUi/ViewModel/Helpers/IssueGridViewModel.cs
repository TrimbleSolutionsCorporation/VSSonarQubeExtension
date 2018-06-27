// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssueGridViewModel.cs" company="Copyright © 2015 Tekla Corporation. Tekla is a Trimble Company">
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

    using Model.Helpers;
    using Model.Menu;
    using PropertyChanged;

    using SonarLocalAnalyser;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using View.Helpers;
    using VSSonarExtensionUi.Association;
    using VSSonarExtensionUi.ViewModel.Analysis;

    /// <summary>
    /// The issue grid view viewModel.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class IssueGridViewModel : IViewModelBase, IDataModel, IFilterCommand, IFilterOption, IModelBase
    {
        #region Fields

        private readonly object dataRefreshLock = new object();

        /// <summary>
        ///     The data grid options key.
        /// </summary>
        private readonly string dataGridOptionsKey = "DataGridOptions";

        /// <summary>
        ///     The filter.
        /// </summary>
        private readonly IFilter filter;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        /// The rest service
        /// </summary>
        private readonly ISonarRestService restService;

        /// <summary>
        /// The notification manager
        /// </summary>
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// The key translator
        /// </summary>
        private readonly ISQKeyTranslator keyTranslator;

        /// <summary>
        /// The statusbar
        /// </summary>
        private IVSSStatusBar statusbar;

        /// <summary>
        /// The provider
        /// </summary>
        private IServiceProvider provider;

        /// <summary>
        /// The vsenvironmenthelper
        /// </summary>
        private IVsEnvironmentHelper vsenvironmenthelper;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// The source work dir
        /// </summary>
        private string sourceWorkDir;
        private readonly ILocalAnalyserViewModel localanalyser;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueGridViewModel" /> class.
        /// </summary>
        /// <param name="rowContextMenu">The row Context Menu.</param>
        /// <param name="gridId">The grid Id.</param>
        /// <param name="showSqaleRating">if set to <c>true</c> [show sqale rating].</param>
        /// <param name="helper">The helper.</param>
        /// <param name="restServiceIn">The rest service in.</param>
        /// <param name="notManager">The not manager.</param>
        /// <param name="keyTranslatorIn">The key translator in.</param>
        public IssueGridViewModel(
            string gridId,
            bool showSqaleRating,
            IConfigurationHelper helper,
            ISonarRestService restServiceIn,
            INotificationManager notManager,
            ISQKeyTranslator keyTranslatorIn,
            ILocalAnalyserViewModel localanalyser = null)
        {
            this.localanalyser = localanalyser;
            this.keyTranslator = keyTranslatorIn;
            this.notificationManager = notManager;
            this.restService = restServiceIn;

            this.dataGridOptionsKey += gridId;
            this.configurationHelper = helper;
            this.AllIssues = new AsyncObservableCollection<Issue>();
            this.Issues = new AsyncObservableCollection<Issue>();

            this.OpenInIssueTrackerCommand = new RelayCommand(this.OnOpenInIssueTrackerCommand);
            this.OpenInVsCommand = new RelayCommand<IList>(this.OnOpenInVsCommand);
            this.MouseEventCommand = new RelayCommand(this.OnMouseEventCommand);

            this.CloseFlyoutCommand = new RelayCommand(() => this.IsCommentEnabled = false);

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
            this.ContextMenuItems = new ObservableCollection<IMenuItem>();

            this.filter = new IssueFilter(this);
            this.InitFilterCommanding();

            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;

            this.ShowSqaleRating = showSqaleRating;
            this.CommentsFlyoutEnabled = true;
            this.ExplanationFlyoutEnabled = true;

            // register model
            AssociationModel.RegisterNewModelInPool(this);
            SonarQubeViewModel.RegisterNewViewModelInPool(this);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     The analysis mode has change.
        /// </summary>
        public event ChangedEventHandler ShowLeftFlyoutEvent;

        /// <summary>
        ///     The analysis mode has change.
        /// </summary>
        public event ChangedEventHandler NavigateDownEvent;

        /// <summary>
        ///     The analysis mode has change.
        /// </summary>
        public event ChangedEventHandler NavigateUpEvent;
        
        /// <summary>
        /// Gets or sets the all issues.
        /// </summary>
        public AsyncObservableCollection<Issue> AllIssues { get; set; }

        /// <summary>Gets or sets the go to prev issue command.</summary>
        public ICommand GoToPrevIssueCommand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [comments flyout enabled].
        /// </summary>
        /// <value>
        /// <c>true</c> if [comments flyout enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool CommentsFlyoutEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [explanation flyout enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [explanation flyout enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool ExplanationFlyoutEnabled { get; set; }

        /// <summary>Gets or sets the go to next issue command.</summary>
        public ICommand GoToNextIssueCommand { get; set; }

        /// <summary>
        ///     Gets or sets the assignee index.
        /// </summary>
        public int AssigneeIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the author.
        /// </summary>
        /// <value>
        /// The index of the author.
        /// </value>
        public int AuthorIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether assignee visible.
        /// </summary>
        public bool AssigneeVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [author visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [author visible]; otherwise, <c>false</c>.
        /// </value>
        public bool AuthorVisible { get; set; }

        /// <summary>
        /// Gets or sets the selected issue.
        /// </summary>
        /// <value>
        /// The selected issue.
        /// </value>
        public Issue SelectedIssue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is comment enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is comment enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsCommentEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the busy indicator tooltip.
        /// </summary>
        public string BusyIndicatorTooltip { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show left fly out].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show left fly out]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowLeftFlyOut { get; set; }

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
        /// Gets or sets the clear filter term author command.
        /// </summary>
        /// <value>
        /// The clear filter term author command.
        /// </value>
        public ICommand ClearFilterTermAuthorCommand { get; set; }

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
        /// Gets the clear filter term issue tracker command.
        /// </summary>
        /// <value>
        /// The clear filter term issue tracker command.
        /// </value>
        public ICommand ClearFilterTermIssueTrackerCommand { get; private set; }

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
        public int EffortIndex { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether effort to fix is visible.
        /// </summary>
        public bool EffortVisible { get; set; }

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
        /// Gets or sets the filter term author.
        /// </summary>
        /// <value>
        /// The filter term author.
        /// </value>
        public string FilterTermAuthor { get; set; }

        /// <summary>
        ///     Gets or sets the filter term component.
        /// </summary>
        public string FilterTermComponent { get; set; }

        /// <summary>
        /// Gets or sets the filter term issue tracker identifier.
        /// </summary>
        /// <value>
        /// The filter term issue tracker identifier.
        /// </value>
        public string FilterTermIssueTrackerId { get; set; }

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
        /// Gets or sets the open in issue tracker command.
        /// </summary>
        /// <value>
        /// The open in issue tracker command.
        /// </value>
        public ICommand OpenInIssueTrackerCommand { get; set; }

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
        /// Gets or sets the index of the issue tracker identifier.
        /// </summary>
        /// <value>
        /// The index of the issue tracker identifier.
        /// </value>
        public int IssueTrackerIdIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [issue tracker identifier visible].
        /// </summary>
        /// <value>
        /// <c>true</c> if [issue tracker identifier visible]; otherwise, <c>false</c>.
        /// </value>
        public bool IssueTrackerIdVisible { get; set; }

        /// <summary>
        /// Gets or sets the index of the selected.
        /// </summary>
        /// <value>
        /// The index of the selected.
        /// </value>
        public int SelectedIndex { get; set; }

        /// <summary>
        /// Gets or sets the effort to fix.
        /// </summary>
        /// <value>
        /// The effort to fix.
        /// </value>
        public long TotalEffort { get; set; }

        /// <summary>
        /// Gets or sets the number of issues.
        /// </summary>
        /// <value>
        /// The number of issues.
        /// </value>
        public int NumberOfIssues { get; set; }

        /// <summary>
        /// Gets or sets the number of majors.
        /// </summary>
        /// <value>
        /// The number of majors.
        /// </value>
        public int NumberOfMajors { get; set; }

        /// <summary>
        /// Gets or sets the number of criticals.
        /// </summary>
        /// <value>
        /// The number of criticals.
        /// </value>
        public int NumberOfCriticals { get; set; }

        /// <summary>
        /// Gets or sets the number of blockers.
        /// </summary>
        /// <value>
        /// The number of blockers.
        /// </value>
        public int NumberOfBlockers { get; set; }

        /// <summary>
        /// Gets or sets the effort to fix string.
        /// </summary>
        /// <value>
        /// The effort to fix string.
        /// </value>
        public string TotalEffortStr { get; set; }

        /// <summary>
        /// Gets or sets the number of issues string.
        /// </summary>
        /// <value>
        /// The number of issues string.
        /// </value>
        public string NumberOfIssuesStr { get; set; }

        /// <summary>
        /// Gets or sets the number of majors string.
        /// </summary>
        /// <value>
        /// The number of majors string.
        /// </value>
        public string NumberOfMajorsStr { get; set; }

        /// <summary>
        /// Gets or sets the number of criticals string.
        /// </summary>
        /// <value>
        /// The number of criticals string.
        /// </value>
        public string NumberOfCriticalsStr { get; set; }

        /// <summary>
        /// Gets or sets the number of blockers string.
        /// </summary>
        /// <value>
        /// The number of blockers string.
        /// </value>
        public string NumberOfBlockersStr { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show sqale rating].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show sqale rating]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSqaleRating { get; set; }

        /// <summary>
        /// Gets or sets the sqale rating.
        /// </summary>
        /// <value>
        /// The sqale rating.
        /// </value>
        public string SqaleRating { get; set; }

        /// <summary>
        /// Gets or sets the sqale rating string.
        /// </summary>
        /// <value>
        /// The sqale rating string.
        /// </value>
        public string SqaleRatingStr { get; set; }

        /// <summary>
        /// Gets the close flyout command.
        /// </summary>
        /// <value>
        /// The close flyout command.
        /// </value>
        public ICommand CloseFlyoutCommand { get; private set; }

        /// <summary>
        /// Gets or sets the selected explanation item.
        /// </summary>
        /// <value>
        /// The selected explanation item.
        /// </value>
        public ExplanationLine SelectedExplanationItem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is explanation enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is explanation enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsExplanationEnabled { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        public void OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IList<IIssueTrackerPlugin> issuePlugin)
        {
            // does nothing
        }

        /// <summary>
        /// Goes to next issue.
        /// </summary>
        public void GoToNextIssue()
        {
            if (this.SelectedIndex < this.Issues.Count)
            {
                this.SelectedIndex++;
            }

            this.OnOpenInVsCommand(this.SelectedItems);
        }

        /// <summary>
        /// Called when [selected issue changed].
        /// </summary>
        public void OnSelectedIssueChanged()
        {
            this.ValidateCommentFlyout();
            this.ValidateExplanationFlyout();

        }

        /// <summary>
        /// Called when [selected explanation item changed].
        /// </summary>
        public void OnSelectedExplanationItemChanged()
        {
            if(this.localanalyser == null || this.SelectedExplanationItem == null)
            {
                return;
            }

            this.localanalyser.NavigatinExplanation = true;
            this.vsenvironmenthelper.OpenResourceInVisualStudio(this.sourceWorkDir, this.SelectedExplanationItem.Path, this.SelectedExplanationItem.Line);
        }

        private void ValidateExplanationFlyout()
        {
            if(!this.ExplanationFlyoutEnabled)
            {
                this.IsExplanationEnabled = false;
                return;
            }

            if(this.SelectedIssue != null && this.SelectedIssue.Explanation.Count != 0)
            {
                this.IsExplanationEnabled = true;
            }
            else
            {
                this.IsExplanationEnabled = false;
            }
        }

        private void ValidateCommentFlyout()
        {
            if(!this.CommentsFlyoutEnabled)
            {
                this.IsCommentEnabled = false;
                return;
            }

            if(this.SelectedIssue != null && this.SelectedIssue.Comments.Count != 0)
            {
                this.IsCommentEnabled = true;
            }
            else
            {
                this.IsCommentEnabled = false;
            }
        }

        /// <summary>
        /// Goes to previous issue.
        /// </summary>
        public void GoToPrevIssue()
        {
            if (this.SelectedIndex > 0)
            {
                this.SelectedIndex--;
            }

            this.OnOpenInVsCommand(this.SelectedItems);
        }

        /// <summary>
        /// Resets the statistics.
        /// </summary>
        public void ResetStatistics()
        {
            this.NumberOfBlockers = 0;
            this.NumberOfCriticals = 0;
            this.NumberOfMajors = 0;
            this.NumberOfIssues = 0;
            this.TotalEffort = 0;

            this.UpdateStatistics();
        }

        /// <summary>
        /// Resets the columns view.
        /// </summary>
        public void ResetColumnsView()
        {
            this.ResetWindowDefaults();
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
        /// Called when [show left fly out changed].
        /// </summary>
        public void OnShowLeftFlyOutChanged()
        {
            this.ShowLeftFlyout(EventArgs.Empty);
        }

        /// <summary>
        /// Called when [open in issue tracker command].
        /// </summary>
        public void OnOpenInIssueTrackerCommand()
        {
            string[] delimiters = { "<br/>" };
            var link = string.Empty;
            if (this.SelectedIssue != null && this.SelectedIssue.Comments.Count > 0)
            {
                foreach (var comment in this.SelectedIssue.Comments)
                {
                    // [VSSonarQubeExtension] Attached to issue: 116179<br/>link<br/>
                    if (comment.HtmlText.Contains("[VSSonarQubeExtension] Attached to issue"))
                    {
                        link = comment.HtmlText.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)[1];
                    }
                }
            }

            if (!string.IsNullOrEmpty(link))
            {
                try
                {
                    Process.Start(link);
                }
                catch (Exception ex)
                {
                    MessageDisplayBox.DisplayMessage("Failed to open issue in Issue Tracker", ex.Message);                    
                }                
            }
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
            if (list == null || this.associatedProject == null)
            {
                return;
            }

            List<Issue> selectedItemsList = list.Cast<Issue>().ToList();

            foreach (Issue issue in selectedItemsList)
            {
                if (this.vsenvironmenthelper == null)
                {
                    return;
                }

                var translatedPath = string.Empty;

                try
                {
                    translatedPath = this.keyTranslator.TranslateKey(issue.Component, this.vsenvironmenthelper, this.associatedProject.BranchName);
                    if (string.IsNullOrEmpty(translatedPath) && string.IsNullOrEmpty(issue.LocalPath))
                    {
                        IssueGridViewModel.ReportTranslationException(issue, translatedPath, this.notificationManager, this.restService, this.associatedProject);
                        return;
                    }

                    if (string.IsNullOrEmpty(translatedPath)) {
                        translatedPath = issue.LocalPath;
                    }

                    this.vsenvironmenthelper.OpenResourceInVisualStudio(this.sourceWorkDir, translatedPath, issue.Line);
                }
                catch (Exception ex)
                {
                    IssueGridViewModel.ReportTranslationException(issue, translatedPath, this.notificationManager, this.restService, this.associatedProject, ex);
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
        public void UpdateIssues(IEnumerable<Issue> listOfIssues)
        {
            Application.Current.Dispatcher.Invoke(
                delegate
                    {
                        try
                        {
                            this.Issues.Clear();
                            this.AllIssues.Clear();
                            foreach (Issue listOfIssue in listOfIssues)
                            {
                                this.AllIssues.Add(listOfIssue);
                                this.Issues.Add(listOfIssue);
                            }

                            this.notificationManager.OnNewIssuesUpdated();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Message: " + ex.Message);
                        }
                    });


            // reload menu data
            using (var bw = new BackgroundWorker { WorkerReportsProgress = false })
            {
                bw.RunWorkerCompleted += delegate { Application.Current.Dispatcher.Invoke(delegate { }); };

                bw.DoWork += delegate
                {
                    this.ReloadMenuData();
                };

                bw.RunWorkerAsync();
            }

            this.RefreshStatistics();
        }

        /// <summary>
        /// Refreshes the statistics.
        /// </summary>
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
   
        /// <summary>
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="providerIn">The provider in.</param>
        public void UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider providerIn)
        {
            this.vsenvironmenthelper = vsenvironmenthelperIn;
            this.statusbar = statusBar;
            this.provider = providerIn;
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>
        /// returns view model
        /// </returns>
        public object GetViewModel()
        {
            return this;
        }

        /// <summary>
        /// Gets the available model, TODO: needs to be removed after viewmodels are split into models and view models
        /// </summary>
        /// <returns>
        /// returns optinal model
        /// </returns>
        public object GetAvailableModel()
        {
            return null;
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="providerIn">The provider in.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public void AssociateWithNewProject(
            Resource project,
            string workingDir,
            ISourceControlProvider providerIn,
            Dictionary<string, Profile> profile,
            string visualStudioVersion)
        {
            this.sourceWorkDir = workingDir;
            this.associatedProject = project;
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void OnSolutionClosed()
        {
            this.sourceWorkDir = string.Empty;
            this.associatedProject = null;
        }

        /// <summary>
        /// Reloads the menu data.
        /// </summary>
        public void ReloadMenuData()
        {
            foreach (var item in this.ContextMenuItems)
            {
                item.CancelRefreshData();
            }

            lock (this.dataRefreshLock)
            {
                foreach (var item in this.ContextMenuItems)
                {
                    item.RefreshMenuData();
                }
            }
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public void OnDisconnect()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the value for option.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultvalue">The defaultvalue.</param>
        /// <param name="owner">The owner.</param>
        /// <returns>return value for option</returns>
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
                    this.notificationManager.ReportMessage(new Message { Data = "Filter Failed: " + ex.Message });
                    this.Issues.Add(issue);
                }
            }

            this.notificationManager.OnIssuesUpdated();
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
        ///     The init filter commanding.
        /// </summary>
        private void InitFilterCommanding()
        {
            this.FilterTermMessage = string.Empty;
            this.FilterTermComponent = string.Empty;
            this.FilterTermProject = string.Empty;
            this.FilterTermRule = string.Empty;
            this.FilterTermAssignee = string.Empty;
            this.FilterTermAuthor = string.Empty;
            this.FilterTermIssueTrackerId = null;
            this.FilterTermStatus = null;
            this.FilterTermSeverity = null;
            this.FilterTermResolution = null;
            this.FilterTermIsNew = null;

            this.ClearClearFilterTermRuleCommand = new RelayCommand<object>(this.OnClearClearFilterTermRuleCommand);
            this.ClearFilterTermAssigneeCommand = new RelayCommand<object>(this.OnClearFilterTermAssigneeCommand);
            this.ClearFilterTermAuthorCommand = new RelayCommand<object>(this.OnClearFilterTermAuthorCommand);
            this.ClearFilterTermComponentCommand = new RelayCommand<object>(this.OnClearFilterTermComponentCommand);
            this.ClearFilterTermIssueTrackerCommand = new RelayCommand<object>(this.OnClearFilterTermIssueTrackerCommand);            
            this.ClearFilterTermIsNewCommand = new RelayCommand<object>(this.OnClearFilterTermIsNewCommand);
            this.ClearFilterTermMessageCommand = new RelayCommand<object>(this.OnClearFilterTermMessageCommand);
            this.ClearFilterTermProjectCommand = new RelayCommand<object>(this.OnClearFilterTermProjectCommand);
            this.ClearFilterTermResolutionCommand = new RelayCommand<object>(this.OnClearFilterTermResolutionCommand);
            this.ClearFilterTermSeverityCommand = new RelayCommand<object>(this.OnClearFilterTermSeverityCommand);
            this.ClearFilterTermStatusCommand = new RelayCommand<object>(this.OnClearFilterTermStatusCommand);
            this.FilterApplyCommand = new RelayCommand(this.OnFilterApplyCommand);
            this.FilterClearAllCommand = new RelayCommand<object>(this.OnFilterClearAllCommand);

            this.GoToPrevIssueCommand = new RelayCommand(this.OnGoToPrevIssueCommand);
            this.GoToNextIssueCommand = new RelayCommand(this.OnGoToNextIssueCommand);
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
        /// The on clear filter term assignee command.
        /// </summary>
        /// <param name="obj">The obj.</param>
        private void OnClearFilterTermAuthorCommand(object obj)
        {
            this.FilterTermAuthor = string.Empty;
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
        /// Called when [clear filter term issue tracker command].
        /// </summary>
        /// <param name="obj">The object.</param>
        private void OnClearFilterTermIssueTrackerCommand(object obj)
        {
            this.FilterTermIssueTrackerId = null;
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
        private void OnFilterApplyCommand()
        {
            if (this.AllIssues == null || !this.AllIssues.Any())
            {
                this.ResetStatistics();
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
                    this.notificationManager.WriteMessageToLog("Filter Failed: " + ex.Message);
                    this.Issues.Add(issue);
                }
            }

            this.UpdateStatistics();
            this.notificationManager.OnIssuesUpdated();
        }

        /// <summary>
        /// Updates the statistics.
        /// </summary>
        private void UpdateStatistics()
        {
            this.NumberOfIssuesStr = "Issues: " + this.NumberOfIssues;
            this.NumberOfBlockersStr = "Blockers: " + this.NumberOfBlockers;
            this.NumberOfCriticalsStr = "Criticals: " + this.NumberOfCriticals;
            this.NumberOfMajorsStr = "Majors: " + this.NumberOfMajors;
            this.TotalEffortStr = "Effort: " + this.TotalEffort + " min";
        }

        /// <summary>
        /// Populates the statistics.
        /// </summary>
        /// <param name="issue">The issue.</param>
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
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(issue.Effort))
            {
                try
                {
                    var effort = issue.Effort.Replace("min", "").Replace("sec", "").Replace("hour", "").Replace("day", "").Replace("d", "").Replace("h", "");
                    this.TotalEffort += int.Parse(effort);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
        
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
            this.FilterTermAuthor = string.Empty;
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
                this.notificationManager.ReportException(ex);
            }

            this.RefreshStatistics();
        }

        /// <summary>
        /// The on mouse event command.
        /// </summary>
        private void OnMouseEventCommand()
        {
        }

        /// <summary>
        /// The read window options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="owner">The owner.</param>
        private void ReadWindowOptions(IConfigurationHelper options, string owner)
        {
            try
            {
                this.ComponentIndex = int.Parse(GetValueForOption(options, "ComponentIndex", "1", owner), CultureInfo.InvariantCulture);
                this.LineIndex = int.Parse(GetValueForOption(options, "LineIndex", "2", owner), CultureInfo.InvariantCulture);
                this.MessageIndex = int.Parse(GetValueForOption(options, "MessageIndex", "3", owner), CultureInfo.InvariantCulture);
                this.StatusIndex = int.Parse(GetValueForOption(options, "StatusIndex", "4", owner), CultureInfo.InvariantCulture);
                this.SeverityIndex = int.Parse(GetValueForOption(options, "SeverityIndex", "5", owner), CultureInfo.InvariantCulture);
                this.EffortIndex = int.Parse(GetValueForOption(options, "EffortIndex", "6", owner), CultureInfo.InvariantCulture);
                this.IssueTrackerIdIndex = int.Parse(GetValueForOption(options, "IssueTrackerIdIndex", "7", owner), CultureInfo.InvariantCulture);
                this.IsNewIndex = int.Parse(GetValueForOption(options, "IsNewIndex", "8", owner), CultureInfo.InvariantCulture);
                this.RuleIndex = int.Parse(GetValueForOption(options, "RuleIndex", "9", owner), CultureInfo.InvariantCulture);
                this.AssigneeIndex = int.Parse(GetValueForOption(options, "AssigneeIndex", "10", owner), CultureInfo.InvariantCulture);
                this.AuthorIndex = int.Parse(GetValueForOption(options, "AuthorIndex", "11", owner), CultureInfo.InvariantCulture);
                this.CreationDateIndex = int.Parse(GetValueForOption(options, "CreationDateIndex", "12", owner), CultureInfo.InvariantCulture);
                this.ProjectIndex = int.Parse(GetValueForOption(options, "ProjectIndex", "13", owner), CultureInfo.InvariantCulture);
                this.ResolutionIndex = int.Parse(GetValueForOption(options, "ResolutionIndex", "14", owner), CultureInfo.InvariantCulture);
                this.UpdateDateIndex = int.Parse(GetValueForOption(options, "UpdateDateIndex", "15", owner), CultureInfo.InvariantCulture);
                this.CloseDateIndex = int.Parse(GetValueForOption(options, "CloseDateIndex", "16", owner), CultureInfo.InvariantCulture);
                this.KeyIndex = int.Parse(GetValueForOption(options, "KeyIndex", "17", owner), CultureInfo.InvariantCulture);
                this.IdIndex = int.Parse(GetValueForOption(options, "IdIndex", "18", owner), CultureInfo.InvariantCulture);

                this.ComponentVisible = bool.Parse(GetValueForOption(options, "ComponentVisible", "true", owner));
                this.LineVisible = bool.Parse(GetValueForOption(options, "LineVisible", "true", owner));
                this.MessageVisible = bool.Parse(GetValueForOption(options, "MessageVisible", "true", owner));
                this.StatusVisible = bool.Parse(GetValueForOption(options, "StatusVisible", "true", owner));
                this.SeverityVisible = bool.Parse(GetValueForOption(options, "SeverityVisible", "true", owner));
                this.EffortVisible = bool.Parse(GetValueForOption(options, "EffortVisible", "true", owner));
                this.IssueTrackerIdVisible = bool.Parse(GetValueForOption(options, "IssueTrackerIdVisible", "true", owner));
                this.AssigneeVisible = bool.Parse(GetValueForOption(options, "AssigneeVisible", "true", owner));
                this.AuthorVisible = bool.Parse(GetValueForOption(options, "AuthorVisible", "true", owner));
                this.RuleVisible = bool.Parse(GetValueForOption(options, "RuleVisible", "true", owner));
                this.CreationDateVisible = bool.Parse(GetValueForOption(options, "CreationDateVisible", "false", owner));
                this.ProjectVisible = bool.Parse(GetValueForOption(options, "ProjectVisible", "false", owner));
                this.ResolutionVisible = bool.Parse(GetValueForOption(options, "ResolutionVisible", "false", owner));
                this.UpdateDateVisible = bool.Parse(GetValueForOption(options, "UpdateDateVisible", "false", owner));
                this.CloseDateVisible = bool.Parse(GetValueForOption(options, "CloseDateVisible", "false", owner));
                this.KeyVisible = bool.Parse(GetValueForOption(options, "KeyVisible", "false", owner));
                this.IdVisible = bool.Parse(GetValueForOption(options, "IdVisible", "false", owner));
                this.IsNewVisible = bool.Parse(GetValueForOption(options, "IsNewVisible", "false", owner));
                
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
            int index = -1;
            // visible items
            this.ComponentIndex = this.GetIndex(ref index);
            this.LineIndex = this.GetIndex(ref index);
            this.MessageIndex = this.GetIndex(ref index);
            this.SeverityIndex = this.GetIndex(ref index);
            this.StatusIndex = this.GetIndex(ref index);
            this.EffortIndex = this.GetIndex(ref index);
            this.AssigneeIndex = this.GetIndex(ref index);
            this.AuthorIndex = this.GetIndex(ref index);
            this.IssueTrackerIdIndex = this.GetIndex(ref index);
            this.IsNewIndex = this.GetIndex(ref index);

            // other values
            this.CreationDateIndex = this.GetIndex(ref index);
            this.CloseDateIndex = this.GetIndex(ref index);
            this.ProjectIndex = this.GetIndex(ref index);
            this.UpdateDateIndex = this.GetIndex(ref index);
            this.RuleIndex = this.GetIndex(ref index);
            this.ResolutionIndex = this.GetIndex(ref index);
            this.KeyIndex = this.GetIndex(ref index);
            this.IdIndex = this.GetIndex(ref index);
            this.ViolationIdIndex = this.GetIndex(ref index);

            // visible items by default
            this.ComponentVisible = true;
            this.LineVisible = true;
            this.MessageVisible = true;
            this.SeverityVisible = true;
            this.StatusVisible = true;
            this.EffortVisible = true;
            this.AssigneeVisible = true;
            this.AuthorVisible = true;
            this.IssueTrackerIdVisible = true;
            this.IsNewVisible = true;

            // other 
            this.RuleVisible = false;
            this.CreationDateVisible = false;
            this.ProjectVisible = false;
            this.ResolutionVisible = false;
            this.UpdateDateVisible = false;
            this.CloseDateVisible = false;
            this.KeyVisible = false;
            this.IdVisible = false;

            this.SaveWindowOptions();
            this.ContextVisibilityMenuItems = this.CreateColumnVisibiltyMenu();
        }

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private int GetIndex(ref int index)
        {
            index++;
            return index;
        }

        /// <summary>
        ///     The write window options.
        /// </summary>
        private void SaveWindowOptions()
        {
            if (this.ComponentIndex < 0 || this.LineIndex < 0 || this.AuthorIndex < 0  || this.AssigneeIndex < 0 || this.MessageIndex < 0 || this.StatusIndex < 0
                || this.RuleIndex < 0 || this.CreationDateIndex < 0 || this.ProjectIndex < 0 || this.ResolutionIndex < 0 || this.EffortIndex < 0
                || this.UpdateDateIndex < 0 || this.CloseDateIndex < 0 || this.KeyIndex < 0 || this.IsNewIndex < 0 || this.ViolationIdIndex < 0)
            {
                return;
            }

            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "ComponentIndex", 
                this.ComponentIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "LineIndex", 
                this.LineIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "AssigneeIndex", 
                this.AssigneeIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties,
                this.dataGridOptionsKey,
                "AuthorIndex",
                this.AuthorIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "MessageIndex", 
                this.MessageIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "StatusIndex", 
                this.StatusIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "SeverityIndex", 
                this.SeverityIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "RuleIndex", 
                this.RuleIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "CreationDateIndex", 
                this.CreationDateIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "ProjectIndex", 
                this.ProjectIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "ResolutionIndex", 
                this.ResolutionIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "EffortIndex", 
                this.EffortIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "UpdateDateIndex", 
                this.UpdateDateIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "CloseDateIndex", 
                this.CloseDateIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "KeyIndex", 
                this.KeyIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "IdIndex", 
                this.IdIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "IsNewIndex", 
                this.IsNewIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties, 
                this.dataGridOptionsKey, 
                "ViolationIdIndex", 
                this.ViolationIdIndex.ToString(CultureInfo.InvariantCulture));
            this.configurationHelper.WriteSetting(
                Context.UIProperties,
                this.dataGridOptionsKey,
                "IssueTrackerIdIndex",
                this.IssueTrackerIdIndex.ToString(CultureInfo.InvariantCulture));

            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "CreationDateVisible", this.CreationDateVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "CloseDateVisible", this.CloseDateVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "EffortVisible", this.EffortVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "ProjectVisible", this.ProjectVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "UpdateDateVisible", this.UpdateDateVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "StatusVisible", this.StatusVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "SeverityVisible", this.SeverityVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "RuleVisible", this.RuleVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "ResolutionVisible", this.ResolutionVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "AssigneeVisible", this.AssigneeVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "AuthorVisible", this.AuthorVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "IsNewVisible", this.IsNewVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "KeyVisible", this.KeyVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "IdVisible", this.IdVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "ViolationIdVisible", this.ViolationIdVisible.ToString());
            this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, "IssueTrackerIdVisible", this.IssueTrackerIdVisible.ToString());
        }


        /// <summary>
        /// Shows the left flyout.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ShowLeftFlyout(EventArgs e)
        {
            if (this.ShowLeftFlyoutEvent != null)
            {
                this.ShowLeftFlyoutEvent(this, e);
            }
        }

        /// <summary>The on go to next issue command.</summary>
        private void OnGoToNextIssueCommand()
        {
            this.GoToNextIssue();
        }

        /// <summary>The on go to prev issue command.</summary>
        private void OnGoToPrevIssueCommand()
        {
            this.GoToPrevIssue();
        }

        /// <summary>
        ///     The write window options.
        /// </summary>
        private void WriteWindowOptions()
        {
            int i = 0;
            foreach (PropertyInfo propertyInfo in typeof(Issue).GetProperties())
            {
                this.configurationHelper.WriteSetting(
                    Context.UIProperties, 
                    this.dataGridOptionsKey, 
                    propertyInfo.Name + "Index", 
                    i.ToString(CultureInfo.InvariantCulture));
                this.configurationHelper.WriteSetting(Context.UIProperties, this.dataGridOptionsKey, propertyInfo.Name + "Visible", "true");
                i++;
            }
        }

        /// <summary>
        /// Reports the translation exception.
        /// </summary>
        /// <param name="issue">The issue.</param>
        /// <param name="translatedPath">The translated path.</param>
        /// <param name="ex">The ex.</param>
        public static void ReportTranslationException(Issue issue, string translatedPath, INotificationManager manager, ISonarRestService service, Resource associatedProject, Exception ex = null)
        {
            if (ex != null)
            {
                manager.ReportMessage(new Message() { Id = "OnInEditor", Data = ex.Message + " : " + issue.Component });
                manager.ReportException(ex);
            }

            manager.ReportMessage(new Message() { Id = "OnInEditor ", Data = "Was Not Able To Translate Path For Resource:  " + issue.Component });
            manager.ReportMessage(new Message() { Id = "OnInEditor ", Data = "Solution = " + associatedProject.SolutionRoot });
            manager.ReportMessage(new Message() { Id = "OnInEditor", Data = "Translated Path = " + translatedPath });

            try
            {
                service.GetResourcesData(AuthtenticationHelper.AuthToken, issue.Component);
            }
            catch (Exception exConnection)
            {
                manager.ReportMessage(new Message() { Id = "OnInEditor : Failed to get Data For Resource", Data = exConnection.Message + " : " + issue.Component });
                manager.ReportException(exConnection);
            }
        }

        #endregion
    }
}