﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalViewModel.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;

    using DifferenceEngine;

    using GalaSoft.MvvmLight.Command;

    using Helpers;

    using Model.Analysis;
    using Model.Helpers;
    using Model.Menu;

    using PropertyChanged;

    using SonarLocalAnalyser;

    using SonarRestService;
    using SonarRestService.Types;

    using View.Helpers;

    using VSSonarExtensionUi.Association;

    using VSSonarPlugins;
    using VSSonarPlugins.Helpers;
    using VSSonarPlugins.Types;

    /// <summary>
    /// The analysis types.
    /// </summary>
    public enum AnalysisTypes
    {
        /// <summary>
        ///     The file.
        /// </summary>
        FILE,

        /// <summary>
        /// The issues
        /// </summary>
        ISSUES,

        /// <summary>
        ///     The none.
        /// </summary>
        NONE
    }

    /// <summary>
    /// The local view viewModel.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class LocalViewModel : IAnalysisModelBase, IViewModelBase, IModelBase, ILocalAnalyserViewModel
    {
        private readonly string LocalViewModelIdentifier = "LocalViewModel";
        private readonly string ShowIssuesInModifiedLinesKey = "ModifiendLinesOn";
        private readonly string HideFalsePositivesKey = "HideFalsePositives";
        private readonly IList<IAnalysisPlugin> plugins = new List<IAnalysisPlugin>();
        private readonly ISonarRestService restService;
        private readonly IConfigurationHelper configurationHelper;
        private readonly ISonarLocalAnalyser localAnalyserModule;
        private readonly INotificationManager logger;
        private readonly ISQKeyTranslator keyTranslator;
        private readonly Dictionary<string, DateTime> projectBuildCache = new Dictionary<string, DateTime>();
        private readonly Dictionary<string, string> sourceInServerCache = new Dictionary<string, string>();
        private readonly Dictionary<string, List<Issue>> falseandwontfixissues = new Dictionary<string, List<Issue>>();
        private readonly Dictionary<string, List<Issue>> cachedLocalIssues = new Dictionary<string, List<Issue>>();
        private readonly Dictionary<string, List<Issue>> cachedCommandIssues = new Dictionary<string, List<Issue>>();
        private readonly Dictionary<string, List<Issue>> newAddedIssues;

        private CancellationTokenSource ct;
        private bool showFlyouts;
        private IVsEnvironmentHelper vsenvironmenthelper;
        private IVSSStatusBar statusBar;
        private IServiceProvider serviceProvier;
        private string resourceNameInView;
        private Resource associatedProject;
        private string contentInView;
        private IEnumerable<Resource> availableProjects;
        private bool fileAnalysisRunning;
        private string currentFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalViewModel" /> class.
        /// </summary>
        /// <param name="pluginsIn">The plugin data.</param>
        /// <param name="service">The service.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="translator">The translator.</param>
        /// <param name="analyser">The analyzer.</param>
        /// <param name="newIssuesList">The new issues list.</param>
        public LocalViewModel(
            IList<IAnalysisPlugin> pluginsIn,
            ISonarRestService service,
            IConfigurationHelper configurationHelper,
            INotificationManager notificationManager,
            ISQKeyTranslator translator,
            ISonarLocalAnalyser analyser,
            Dictionary<string, List<Issue>> newIssuesList)
        {
            this.newAddedIssues = newIssuesList;
            this.keyTranslator = translator;
            this.logger = notificationManager;
            this.restService = service;
            this.configurationHelper = configurationHelper;

            this.plugins = pluginsIn;
            this.Header = "Local Analysis";
            this.IssuesGridView = new IssueGridViewModel("LocalView", false, this.configurationHelper, service, notificationManager, translator, this)
            {
                ContextMenuItems = this.CreateRowContextMenu(service, translator, analyser, this.availableProjects),
                ShowContextMenu = true
            };
            this.IssuesGridView.ShowLeftFlyoutEvent += this.ShowHideLeftFlyout;
            this.AllLog = new List<string>();

            this.InitCommanding();

            this.localAnalyserModule = analyser;
            this.ShowLeftFlyOut = false;
            this.SizeOfFlyout = 0;
            this.ShowFalsePositivesAndResolvedIssues = true;
            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;

            this.ReloadSavedOptions(this.configurationHelper);

            // register model
            AssociationModel.RegisterNewModelInPool(this);
        }

        /// <summary>
        /// Registers the plugin commands.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void RegisterPluginCommands(Dictionary<string, Profile> profile)
        {
            this.PluginsCommands = new ObservableCollection<PluginCommandWrapper>();
            foreach (var plugin in this.plugins)
            {
                var commands = plugin.AdditionalCommands(profile);
                if (commands != null)
                {
                    foreach (var command in commands)
                    {
                        var commandNew = new PluginCommandWrapper(this, this.logger)
                        {
                            PluginOperation = command,
                            Name = command.Name
                        };
                        this.PluginsCommands.Add(commandNew);
                    }
                }
            }
        }

        /// <summary>
        ///     The analysis mode has change.
        /// </summary>
        public event ChangedEventHandler IssuesReadyForCollecting;

        /// <summary>
        /// Gets or sets the new issues during session command.
        /// </summary>
        /// <value>
        /// The new issues during session command.
        /// </value>
        public ICommand NewIssuesDuringSessionCommand { get; set; }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        /// The resource in view
        /// </summary>
        public VsFileItem VsFileItemInView { get; set; }

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is associated with project.
        /// </summary>
        public bool IsAssociatedWithProject { get; set; }

        /// <summary>
        ///     Gets or sets the issues grid view.
        /// </summary>
        public IssueGridViewModel IssuesGridView { get; set; }

        /// <summary>
        ///     Gets the close fly out issue search command.
        /// </summary>
        public RelayCommand CloseLeftFlyoutCommand { get; private set; }

        /// <summary>
        /// Gets or sets all log.
        /// </summary>
        /// <value>
        /// All log.
        /// </value>
        public List<string> AllLog { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show issues on modified sections of file only].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show issues on modified sections of file only]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowIssuesOnModifiedSectionsOfFileOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show false positives and resolved issues].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show false positives and resolved issues]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowFalsePositivesAndResolvedIssues { get; set; }

        /// <summary>
        ///     Gets or sets the output log.
        /// </summary>
        public string OutputLog { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [errors found during analysis].
        /// </summary>
        /// <value>
        /// <c>true</c> if [errors found during analysis]; otherwise, <c>false</c>.
        /// </value>
        public bool ErrorsFoundDuringAnalysis { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [permissions are not available].
        /// </summary>
        /// <value>
        /// <c>true</c> if [permissions are not available]; otherwise, <c>false</c>.
        /// </value>
        public bool PermissionsAreNotAvailable { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether show fly outs.
        /// </summary>
        public bool ShowLeftFlyOut
        {
            get => this.showFlyouts;

            set
            {
                this.showFlyouts = value;
                this.SizeOfFlyout = value ? 250 : 0;
            }
        }

        /// <summary>
        ///     Gets or sets the size of flyout.
        /// </summary>
        public int SizeOfFlyout { get; set; }

        /// <summary>
        ///     Gets or sets the source working directory.
        /// </summary>
        public string SourceWorkingDir { get; set; }

        /// <summary>
        /// Gets or sets the go to previous issue command.
        /// </summary>
        /// <value>
        /// The go to previous issue command.
        /// </value>
        public ICommand GoToPrevIssueCommand { get; set; }

        /// <summary>
        /// Gets or sets the plugins commands.
        /// </summary>
        /// <value>
        /// The plugins commands.
        /// </value>
        public ObservableCollection<PluginCommandWrapper> PluginsCommands { get; set; }

        /// <summary>
        /// Gets or sets the go to next issue command.
        /// </summary>
        /// <value>
        /// The go to next issue command.
        /// </value>
        public ICommand GoToNextIssueCommand { get; set; }

        /// <summary>
        /// Gets the project link.
        /// </summary>
        /// <value>
        /// The project link.
        /// </value>
        public string ProjectLink { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [analysis is running].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [analysis is running]; otherwise, <c>false</c>.
        /// </value>
        public bool AnalysisIsRunning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [navigation explanation].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [navigation explanation]; otherwise, <c>false</c>.
        /// </value>
        public bool NavigatinExplanation { get; set; }

        /// <summary>
        /// Called when [show issues on modified sections of file only changed].
        /// </summary>
        public void OnShowIssuesOnModifiedSectionsOfFileOnlyChanged()
        {
            this.cachedLocalIssues.Clear();
            this.configurationHelper.WriteSetting(Context.FileAnalysisProperties, this.LocalViewModelIdentifier, this.ShowIssuesInModifiedLinesKey, this.ShowIssuesOnModifiedSectionsOfFileOnly.ToString().ToLowerInvariant());
            this.RunLocalAnalysis(AnalysisTypes.FILE, true, this.VsFileItemInView);
        }

        /// <summary>
        /// Called when [show false positives and resolved issues changed].
        /// </summary>
        public void OnShowFalsePositivesAndResolvedIssuesChanged()
        {
            this.cachedLocalIssues.Clear();
            this.configurationHelper.WriteSetting(Context.FileAnalysisProperties, this.LocalViewModelIdentifier, this.HideFalsePositivesKey, this.ShowFalsePositivesAndResolvedIssues.ToString().ToLowerInvariant());
            if (this.VsFileItemInView != null)
            {
                this.RunLocalAnalysis(AnalysisTypes.FILE, true, this.VsFileItemInView);
            }
        }

        /// <summary>
        /// Reset Stats.
        /// </summary>
        public void ResetStats()
        {
            if (this.NavigatinExplanation)
            {
                return;
            }

            if (this.IssuesGridView != null)
            {
                this.IssuesGridView.ResetStatistics();
            }
        }

        /// <summary>
        ///     The end data association.
        /// </summary>
        public async Task OnSolutionClosed()
        {
            this.sourceInServerCache.Clear();
            this.falseandwontfixissues.Clear();
            this.newAddedIssues.Clear();
            this.cachedLocalIssues.Clear();
            this.ClearIssues();
            this.associatedProject = null;
            this.IsAssociatedWithProject = false;
            this.localAnalyserModule.OnDisconect();
            await Task.Delay(0);
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public async Task OnDisconnect()
        {
            this.newAddedIssues.Clear();
            this.sourceInServerCache.Clear();
            this.falseandwontfixissues.Clear();
            this.cachedLocalIssues.Clear();
            this.localAnalyserModule.OnDisconect();
            await Task.Delay(0);
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        /// <param name="availableProjects">The available projects.</param>
        /// <param name="issuePlugin">The issue plugin.</param>
        public async Task OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IList<IIssueTrackerPlugin> issuePlugin)
        {
            this.availableProjects = availableProjects;
            await Task.Delay(0);
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
            await Task.Delay(0);
            if (fromEditor)
            {
                return new Tuple<List<Issue>, bool>(
                    this.IssuesGridView.Issues.Where(issue => file.Key.Equals(issue.Component))
                        .ToList(), this.ShowFalsePositivesAndResolvedIssues);
            }

            return new Tuple<List<Issue>, bool>(
                this.IssuesGridView.Issues.Where(
                    issue =>
                    this.IssuesGridView.IsNotFiltered(issue) && file.Key.Equals(issue.Component))
                    .ToList(), this.ShowFalsePositivesAndResolvedIssues);
        }

        /// <summary>
        /// Called when [show flyouts changed].
        /// </summary>
        public void OnShowFlyoutsChanged()
        {
            this.SizeOfFlyout = this.ShowLeftFlyOut ? 250 : 0;
        }

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="profile">The profile.</param>
        public async Task AssociateWithNewProject(
            Resource project,
            string workingDir,
            ISourceControlProvider provider,
            Dictionary<string, Profile> profile,
            string visualStudioVersion)
        {
            if (project == null || string.IsNullOrEmpty(project.SolutionRoot))
            {
                return;
            }

            this.associatedProject = project;
            this.IsAssociatedWithProject = this.associatedProject != null;
            this.SourceWorkingDir = workingDir;

            // register additional plugin commmands
            this.RegisterPluginCommands(profile);
            await Task.Delay(0);
        }

        /// <summary>
        /// Runs the analysis.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="fromSave">if set to <c>true</c> [from save].</param>
        public void RunAnalysis(AnalysisTypes mode, bool fromSave)
        {
            this.logger.StartedWorking(mode.ToString());
            this.RunLocalAnalysis(mode, fromSave, this.VsFileItemInView);
        }

        /// <summary>
        /// The on changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        public void RefreshIssuesEditor(EventArgs e)
        {
            this.IssuesReadyForCollecting?.Invoke(this, e);
        }

        /// <summary>
        /// Shows the new added issues and lock.
        /// </summary>
        public void ShowNewAddedIssuesAndLock()
        {
            this.logger.StartedWorking("Display current new issues");
            this.AllLog.Clear();
            this.OutputLog = string.Empty;
            this.UpdateNewLocalIssues();
        }

        /// <summary>
        ///     The on selected view changed.
        /// </summary>
        public void OnSelectedViewChanged()
        {
            this.RefreshIssuesEditor(EventArgs.Empty);
        }

        /// <summary>
        /// The refresh data for resource.
        /// </summary>
        /// <param name="resourceFile">The resource file.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="content">The content.</param>
        /// <param name="fromSave">if set to <c>true</c> [from save].</param>
        public async void RefreshDataForResource(Resource resourceFile, string resourceName, string content, bool fromSave)
        {
            var resourceInView = await this.vsenvironmenthelper.VsFileItem(resourceName, this.associatedProject, resourceFile);
            this.VsFileItemInView = resourceInView;
            this.resourceNameInView = resourceName;
            this.contentInView = content;

            this.logger.StartedWorking("Running File Analysis");
            this.RunLocalAnalysis(AnalysisTypes.FILE, fromSave, resourceInView);
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
        /// Clears the issues.
        /// </summary>
        public void ClearIssues()
        {
            if (this.NavigatinExplanation)
            {
                return;
            }

            this.IssuesGridView.ResetStatistics();
            this.IssuesGridView.AllIssues.Clear();
            this.IssuesGridView.Issues.Clear();
        }

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
            this.vsenvironmenthelper = vsenvironmenthelperIn;
            this.statusBar = statusBarIn;
            this.serviceProvier = provider;
            await Task.Delay(0);
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
        /// Gets the available model, TODO: needs to be removed after view models are split into models and view models
        /// </summary>
        /// <returns>
        /// returns optinal model
        /// </returns>
        public object GetAvailableModel()
        {
            return null;
        }

        /// <summary>
        ///     The init commanding.
        /// </summary>
        private void InitCommanding()
        {
            this.NewIssuesDuringSessionCommand = new RelayCommand(this.ShowNewAddedIssuesAndLock);
            this.GoToPrevIssueCommand = new RelayCommand(this.OnGoToPrevIssueCommand);
            this.GoToNextIssueCommand = new RelayCommand(this.OnGoToNextIssueCommand);
            this.CloseLeftFlyoutCommand = new RelayCommand(this.OnCloseLeftFlyoutCommand);
        }

        /// <summary>
        /// Called when [go to next issue command].
        /// </summary>
        private void OnGoToNextIssueCommand()
        {
            this.IssuesGridView.GoToNextIssue();
        }

        /// <summary>
        /// Called when [go to previous issue command].
        /// </summary>
        private void OnGoToPrevIssueCommand()
        {
            this.IssuesGridView.GoToPrevIssue();
        }

        /// <summary>
        ///     The on flyout log viewer command.
        /// </summary>
        private void OnCloseLeftFlyoutCommand()
        {
            this.ShowLeftFlyOut = false;
            this.IssuesGridView.ShowLeftFlyOut = false;
            this.SizeOfFlyout = 0;
        }

        /// <summary>
        /// The run local analysis new.
        /// </summary>
        /// <param name="analysis">The analysis.</param>
        /// <param name="fromSave">if set to <c>true</c> [from save].</param>
        private async void RunLocalAnalysis(AnalysisTypes analysis, bool fromSave, VsFileItem itemInView)
        {
            if (this.NavigatinExplanation)
            {
                this.NavigatinExplanation = false;
                return;
            }

            if (this.localAnalyserModule == null || this.associatedProject == null)
            {
                return;
            }

            if (fromSave && this.fileAnalysisRunning && this.currentFile == itemInView.FileName)
            {
                this.logger.ReportMessage("Running already... Multiple saves detected");
                return;
            }

            this.PermissionsAreNotAvailable = false;
            try
            {
                this.associatedProject.ActiveConfiguration = await this.vsenvironmenthelper.ActiveConfiguration();
                this.associatedProject.ActivePlatform = await this.vsenvironmenthelper.ActivePlatform();
                this.logger.ResetFailure();
                switch (analysis)
                {
                    case AnalysisTypes.FILE:
                        this.fileAnalysisRunning = true;
                        this.currentFile = itemInView.FileName;
                        if (itemInView == null)
                        {
                            return;
                        }

                        await this.IssuesGridView.UpdateIssues(new List<Issue>(), null);

                        if (fromSave || !this.cachedLocalIssues.ContainsKey(itemInView.SonarResource.Key))
                        {
                            this.cachedLocalIssues.Remove(itemInView.SonarResource.Key);
                            this.PopulateFalsePositivesAndResolvedItems(itemInView);
                            var issues = await this.localAnalyserModule.AnalyseFile(
                                itemInView,
                                this.associatedProject,
                                AuthtenticationHelper.AuthToken,
                                this.keyTranslator,
                                this.vsenvironmenthelper,
                                fromSave);
                            if (this.cachedCommandIssues.ContainsKey(itemInView.SonarResource.Key))
                            {
                                this.cachedCommandIssues[itemInView.SonarResource.Key].Clear();
                                this.cachedCommandIssues[itemInView.SonarResource.Key].AddRange(issues);
                            }

                            await this.IssuesGridView.UpdateIssues(issues, null);
                            this.RefreshIssuesEditor(EventArgs.Empty);
                        }
                        else
                        {
                            // load issues from cache
                            var issues = new List<Issue>();
                            issues.AddRange(this.cachedLocalIssues[itemInView.SonarResource.Key]);
                            if (this.cachedCommandIssues.ContainsKey(itemInView.SonarResource.Key))
                            {
                                issues.AddRange(this.cachedCommandIssues[itemInView.SonarResource.Key]);
                            }

                            await this.IssuesGridView.UpdateIssues(issues, null);
                            this.RefreshIssuesEditor(EventArgs.Empty);
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                this.logger.ReportMessage(new VSSonarPlugins.Message { Id = "LocalViewModel", Data = "Analysis Failed: " + ex.Message });
                this.logger.ReportException(ex);
                this.logger.EndedWorking();
                this.logger.FlagFailure(ex.Message);
            }

            this.fileAnalysisRunning = false;
        }

        /// <summary>
        /// Populates the false positives and resolved items.
        /// </summary>
        private async void PopulateFalsePositivesAndResolvedItems(VsFileItem itemInView)
        {
            if (this.falseandwontfixissues.ContainsKey(itemInView.SonarResource.Key))
            {
                return;
            }

            try
            {
                var filter = "?componentRoots=" + itemInView.SonarResource.Key + "&resolutions=FALSE-POSITIVE,WONTFIX";

                this.CreateNewTokenOrUseOldOne();
                var issues = await Task.Run(() =>
                {
                    return this.restService.GetIssues(
                        AuthtenticationHelper.AuthToken,
                        filter,
                        this.associatedProject.Key,
                        this.ct.Token,
                        this.logger);
                }).ConfigureAwait(false);

                this.falseandwontfixissues.Add(itemInView.SonarResource.Key, issues);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void CreateNewTokenOrUseOldOne()
        {
            if (this.ct == null || this.ct.IsCancellationRequested)
            {
                this.ct = new CancellationTokenSource();
            }
        }

        /// <summary>
        /// The update local issues in view.
        /// </summary>
        private async void UpdateNewLocalIssues()
        {
            this.AnalysisIsRunning = false;
            this.logger.EndedWorking();
            var issues = new List<Issue>();
            foreach (var item in this.newAddedIssues)
            {
                issues.AddRange(item.Value);
            }

            await this.IssuesGridView.UpdateIssues(issues, null);
            this.RefreshIssuesEditor(EventArgs.Empty);
            await this.IssuesGridView.RefreshStatistics();
        }

        /// <summary>
        /// The update local issues in view.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public async void UpdateLocalIssues(object sender, EventArgs e)
        {
            this.AnalysisIsRunning = false;
            this.logger.EndedWorking();

            var exceptionMsg = e as LocalAnalysisExceptionEventArgs;
            if (exceptionMsg != null)
            {
                this.OnSelectedViewChanged();
                this.IssuesGridView.Issues.Clear();
                this.ErrorsFoundDuringAnalysis = true;
                UserExceptionMessageBox.ShowException("Analysis Failed: " + exceptionMsg.ErrorMessage, exceptionMsg.Ex, exceptionMsg.Ex.StackTrace);
                return;
            }

            var issuesFullMessage = e as LocalAnalysisEventFullAnalsysisComplete;
            if (issuesFullMessage != null)
            {
                this.logger.ReportMessage("Local Analysis Reported: " + issuesFullMessage.issues.Count);
                await this.IssuesGridView.UpdateIssues(issuesFullMessage.issues, null);
                this.RefreshIssuesEditor(EventArgs.Empty);
                return;
            }

            var commandIssues = e as LocalAnalysisEventCommandAnalsysisComplete;
            if (commandIssues != null)
            {
                if (this.cachedCommandIssues.ContainsKey(commandIssues.itemInView.SonarResource.Key))
                {
                    this.cachedCommandIssues.Remove(commandIssues.itemInView.SonarResource.Key);
                }

                this.cachedCommandIssues.Add(commandIssues.itemInView.SonarResource.Key, commandIssues.issues);
                var issuesToGrid = commandIssues.issues;

                if (this.cachedLocalIssues.ContainsKey(commandIssues.itemInView.SonarResource.Key))
                {
                    issuesToGrid.AddRange(this.cachedLocalIssues[commandIssues.itemInView.SonarResource.Key]);
                }

                await this.IssuesGridView.UpdateIssues(issuesToGrid, null);
                this.RefreshIssuesEditor(EventArgs.Empty);
                return;
            }

            var issuesMessage = e as LocalAnalysisEventFileAnalsysisComplete;
            if (issuesMessage == null)
            {
                this.logger.ReportMessage("Local Analysis on File With Some problem Reported: " + e);
                return;
            }

            if (issuesMessage.issues.Count == 0)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (this.newAddedIssues.ContainsKey(issuesMessage.resource.SonarResource.Key))
                    {
                        this.newAddedIssues[issuesMessage.resource.SonarResource.Key].Clear();
                    }

                    this.RefreshIssuesEditor(EventArgs.Empty);
                });
                return;
            }

            if (!string.IsNullOrEmpty(this.ProjectLink))
            {
                this.vsenvironmenthelper.NavigateToResource(this.ProjectLink);
                this.ProjectLink = string.Empty;
            }

            if (this.PermissionsAreNotAvailable)
            {
                MessageBox.Show("You're not authorized to execute a dry run analysis. Please contact your SonarQube administrator.");
                return;
            }

            if (System.Windows.Application.Current == null)
            {
                return;
            }

            await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
            {
                try
                {
                    var source = string.Empty;

                    if (this.sourceInServerCache.ContainsKey(issuesMessage.resource.SonarResource.Key))
                    {
                        source = this.sourceInServerCache[issuesMessage.resource.SonarResource.Key];
                    }
                    else
                    {
                        var data = this.restService.GetSourceForFileResource(AuthtenticationHelper.AuthToken, issuesMessage.resource.SonarResource.Key).Lines;
                        source = string.Join("\n", data);
                        this.sourceInServerCache.Add(issuesMessage.resource.SonarResource.Key, source);
                    }

                    var diffReport = VsSonarUtils.GetSourceDiffFromStrings(source, this.contentInView, DiffEngineLevel.FastImperfect);

                    var issuesWithoutFalsePositives = issuesMessage.issues;

                    if (!this.ShowFalsePositivesAndResolvedIssues)
                    {
                        issuesWithoutFalsePositives = this.FilterIssuesWithFalsePositives(issuesMessage.issues, issuesMessage.resource);
                    }

                    var issuesinChangedLines = VsSonarUtils.GetIssuesInModifiedLinesOnly(issuesWithoutFalsePositives, diffReport);

                    if (!this.newAddedIssues.ContainsKey(issuesMessage.resource.SonarResource.Key))
                    {
                        var newList = new List<Issue>(issuesinChangedLines);
                        this.newAddedIssues.Add(issuesMessage.resource.SonarResource.Key, newList);
                    }
                    else
                    {
                        var listOfIssues = this.newAddedIssues[issuesMessage.resource.SonarResource.Key];
                        listOfIssues.Clear();
                        listOfIssues.AddRange(issuesinChangedLines);
                    }

                    if (this.ShowIssuesOnModifiedSectionsOfFileOnly)
                    {
                        if (this.cachedCommandIssues.ContainsKey(issuesMessage.resource.SonarResource.Key))
                        {
                            issuesinChangedLines.AddRange(this.cachedCommandIssues[issuesMessage.resource.SonarResource.Key]);
                        }

                        if (this.cachedLocalIssues.ContainsKey(issuesMessage.resource.SonarResource.Key))
                        {
                            this.cachedLocalIssues.Remove(issuesMessage.resource.SonarResource.Key);
                        }

                        this.cachedLocalIssues.Add(issuesMessage.resource.SonarResource.Key, issuesinChangedLines);

                        if (!issuesMessage.resource.SonarResource.Key.Equals(this.VsFileItemInView.SonarResource.Key))
                        {
                            return;
                        }

                        await this.IssuesGridView.UpdateIssues(issuesinChangedLines, null);
                        this.RefreshIssuesEditor(EventArgs.Empty);
                    }
                    else
                    {
                        if (this.cachedCommandIssues.ContainsKey(issuesMessage.resource.SonarResource.Key))
                        {
                            issuesinChangedLines.AddRange(this.cachedCommandIssues[issuesMessage.resource.SonarResource.Key]);
                        }

                        if (this.cachedLocalIssues.ContainsKey(issuesMessage.resource.SonarResource.Key))
                        {
                            this.cachedLocalIssues.Remove(issuesMessage.resource.SonarResource.Key);
                        }
                        this.cachedLocalIssues.Add(issuesMessage.resource.SonarResource.Key, issuesWithoutFalsePositives);

                        if (!issuesMessage.resource.SonarResource.Key.Equals(this.VsFileItemInView.SonarResource.Key))
                        {
                            return;
                        }

                        await this.IssuesGridView.UpdateIssues(issuesWithoutFalsePositives, null);
                        this.RefreshIssuesEditor(EventArgs.Empty);
                    }

                    this.OnSelectedViewChanged();
                    await this.IssuesGridView.RefreshStatistics();
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (!this.newAddedIssues.ContainsKey(issuesMessage.resource.SonarResource.Key))
                        {
                            var newList = new List<Issue>(issuesMessage.issues);
                            this.newAddedIssues.Add(issuesMessage.resource.SonarResource.Key, newList);
                        }
                        else
                        {
                            var listOfIssues = this.newAddedIssues[issuesMessage.resource.SonarResource.Key];
                            listOfIssues.Clear();
                            listOfIssues.AddRange(issuesMessage.issues);
                        }

                        this.logger.ReportMessage(
                            new VSSonarPlugins.Message
                            {
                                Id = "LocalAnalyserModel",
                                Data = "Failed to check false positives and new issues : Likely new file : " + ex.Message
                            });

                        this.cachedLocalIssues.Add(issuesMessage.resource.SonarResource.Key, issuesMessage.issues);
                        if (!issuesMessage.resource.SonarResource.Key.Equals(this.VsFileItemInView.SonarResource.Key))
                        {
                            return;
                        }

                        var issues = issuesMessage.issues;
                        if (this.cachedCommandIssues.ContainsKey(issuesMessage.resource.SonarResource.Key))
                        {
                            issues.AddRange(this.cachedCommandIssues[issuesMessage.resource.SonarResource.Key]);
                        }

                        await this.IssuesGridView.UpdateIssues(issues, null);
                        this.RefreshIssuesEditor(EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                    }
                }
            });
        }

        /// <summary>
        /// Filters the issues with false positives.
        /// </summary>
        /// <param name="issues">The issues.</param>
        /// <returns>returns filtered issues</returns>
        private List<Issue> FilterIssuesWithFalsePositives(List<Issue> issues, VsFileItem itemInView)
        {
            var filteredIssues = new List<Issue>();
            try
            {
                var falseissues = this.falseandwontfixissues[itemInView.SonarResource.Key];

                foreach (var issue in issues)
                {
                    var isfalse = false;
                    foreach (var falseissue in falseissues)
                    {
                        if (issue.Rule.Equals(falseissue.Rule) && issue.Line.Equals(falseissue.Line))
                        {
                            isfalse = true;
                            issue.Resolution = falseissue.Resolution;
                            issue.Status = falseissue.Status;
                            break;
                        }
                    }

                    if (!isfalse)
                    {
                        filteredIssues.Add(issue);
                    }
                }

                return filteredIssues;
            }
            catch (Exception)
            {
                return issues;
            }
        }

        /// <summary>
        /// Wases the assembly updated.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns>True if Assemly has been modified</returns>
        private bool WasAssemblyUpdated(VsProjectItem project)
        {
            if (this.projectBuildCache.ContainsKey(project.OutputPath))
            {
                var lastUpdateTime = this.projectBuildCache[project.OutputPath];
                var currentUpdateTime = this.GetUpdateTimeFromAssembly(project.OutputPath);

                if (currentUpdateTime > lastUpdateTime)
                {
                    this.projectBuildCache.Remove(project.OutputPath);
                    this.projectBuildCache.Add(project.OutputPath, currentUpdateTime);
                    return true;
                }
            }
            else
            {
                var currentUpdateTime = this.GetUpdateTimeFromAssembly(project.OutputPath);
                this.projectBuildCache.Add(project.OutputPath, currentUpdateTime);
                return true;
            }

            this.logger.ReportMessage(new VSSonarPlugins.Message { Id = "LocalViewModel", Data = "Project output is Updated" });
            return false;
        }

        /// <summary>
        /// Gets the update time from assembly.
        /// </summary>
        /// <param name="outputPath">The output path.</param>
        /// <returns>Returns time of updated assembly.</returns>
        private DateTime GetUpdateTimeFromAssembly(string outputPath)
        {
            return File.GetLastWriteTime(outputPath);
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
        private ObservableCollection<IMenuItem> CreateRowContextMenu(
            ISonarRestService service,
            ISQKeyTranslator translator,
            ISonarLocalAnalyser analyser,
            IEnumerable<Resource> projects)
        {
            var menu = new ObservableCollection<IMenuItem>
                           {
                               SetExclusionsMenu.MakeMenu(service, this.IssuesGridView, this.logger, translator, analyser, projects),
                               SetSqaleMenu.MakeMenu(service, this.IssuesGridView, this.logger, translator, analyser),
                               MoreInfoMenu.MakeMenu(service, this.IssuesGridView, this.logger, translator, analyser)
                           };

            return menu;
        }

        /// <summary>
        /// Reloads the saved options.
        /// </summary>
        /// <param name="configurationHelperIn">The configuration helper in.</param>
        private void ReloadSavedOptions(IConfigurationHelper configurationHelperIn)
        {
            var dataShowOnModified = configurationHelperIn.ReadSetting(Context.FileAnalysisProperties, this.LocalViewModelIdentifier, this.ShowIssuesInModifiedLinesKey);
            if (dataShowOnModified != null)
            {
                this.ShowIssuesOnModifiedSectionsOfFileOnly = dataShowOnModified.Value.Equals("true") ? true : false;
            }

            var dataShowFalsePositives = configurationHelperIn.ReadSetting(Context.FileAnalysisProperties, this.LocalViewModelIdentifier, this.HideFalsePositivesKey);
            if (dataShowOnModified != null)
            {
                this.ShowFalsePositivesAndResolvedIssues = dataShowFalsePositives.Value.Equals("true") ? true : false;
            }
        }
    }
}
