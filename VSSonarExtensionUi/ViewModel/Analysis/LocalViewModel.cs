// --------------------------------------------------------------------------------------------------------------------
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
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
    using View.Helpers;
    using VSSonarExtensionUi.Association;
    using VSSonarPlugins;
    using VSSonarPlugins.Helpers;
    using VSSonarPlugins.Types;
        
    /// <summary>
    ///     The analysis types.
    /// </summary>
    public enum AnalysisTypes
    {
        /// <summary>
        ///     The preview.
        /// </summary>
        PREVIEW, 

        /// <summary>
        ///     The incremental.
        /// </summary>
        INCREMENTAL, 

        /// <summary>
        ///     The file.
        /// </summary>
        FILE, 

        /// <summary>
        ///     The analysis.
        /// </summary>
        ANALYSIS,

        /// <summary>
        ///     The none.
        /// </summary>
        NONE
    }

    /// <summary>
    /// The local view viewModel.
    /// </summary>
    [ImplementPropertyChanged]
    public class LocalViewModel : IAnalysisModelBase, IViewModelBase, IModelBase
    {
        /// <summary>
        /// The plugin list
        /// </summary>
        private readonly IList<IAnalysisPlugin> plugins = new List<IAnalysisPlugin>();

        /// <summary>
        /// The rest service
        /// </summary>
        private readonly ISonarRestService restService;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        /// The local analyzer module
        /// </summary>
        private readonly ISonarLocalAnalyser localAnalyserModule;

        /// <summary>
        /// The notification manager
        /// </summary>
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// The key translator
        /// </summary>
        private readonly ISQKeyTranslator keyTranslator;

        /// <summary>
        /// The project build cache
        /// </summary>
        private readonly Dictionary<string, DateTime> projectBuildCache = new Dictionary<string, DateTime>();

        /// <summary>
        /// The source in server
        /// </summary>
        private readonly Dictionary<string, string> sourceInServerCache = new Dictionary<string, string>();

        /// <summary>
        /// The false positives per resource
        /// </summary>
        private readonly Dictionary<string, List<Issue>> falseandwontfixissues = new Dictionary<string, List<Issue>>();

        /// <summary>
        /// The new added issues
        /// </summary>
        private readonly Dictionary<string, List<Issue>> newAddedIssues;

        /// <summary>
        ///     The show fly outs.
        /// </summary>
        private bool showFlyouts;

        /// <summary>
        /// The vs environment helper
        /// </summary>
        private IVsEnvironmentHelper vsenvironmenthelper;

        /// <summary>
        /// The status bar
        /// </summary>
        private IVSSStatusBar statusBar;

        /// <summary>
        /// The service provider
        /// </summary>
        private IServiceProvider serviceProvier;

        /// <summary>
        /// The resource path in view
        /// </summary>
        private string resourceNameInView;

        /// <summary>
        /// The resource in view
        /// </summary>
        private Resource resourceInView;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// The content in view
        /// </summary>
        private string contentInView;

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
            this.notificationManager = notificationManager;
            this.restService = service;
            this.configurationHelper = configurationHelper;

            this.plugins = pluginsIn;
            this.Header = "Local Analysis";
            this.ShowFalsePositivesAndResolvedIssues = true;
            this.IssuesGridView = new IssueGridViewModel("LocalView", false, this.configurationHelper, service, notificationManager, translator);
            this.IssuesGridView.ContextMenuItems = this.CreateRowContextMenu(service, translator, analyser);
            this.IssuesGridView.ShowContextMenu = true;
            this.IssuesGridView.ShowLeftFlyoutEvent += this.ShowHideLeftFlyout;
            this.AllLog = new List<string>();

            this.InitCommanding();

            this.localAnalyserModule = analyser;
            this.localAnalyserModule.StdOutEvent += this.UpdateOutputMessagesFromPlugin;
            this.localAnalyserModule.LocalAnalysisCompleted += this.UpdateLocalIssues;

            this.ShowLeftFlyOut = false;
            this.SizeOfFlyout = 0;

            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;

            // register model
            AssociationModel.RegisterNewModelInPool(this);
        }

        /// <summary>
        ///     The analysis mode has change.
        /// </summary>
        public event ChangedEventHandler IssuesReadyForCollecting;

        /// <summary>
        ///     Gets or sets the analysis command.
        /// </summary>
        public ICommand AnalysisCommand { get; set; }

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
        ///     Gets or sets a value indicating whether can run analysis.
        /// </summary>
        [AlsoNotifyFor("CanRunLocalAnalysis")]
        public bool CanRunAnalysis { get; set; }

        /// <summary>Gets or sets the associating text tool tip.</summary>
        public string AssociatingTextTooltip { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether file analysis is enabled.
        /// </summary>
        public bool FileAnalysisIsEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        ///     Gets or sets the incremental command.
        /// </summary>
        public ICommand IncrementalCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is associated with project.
        /// </summary>
        [AlsoNotifyFor("CanRunLocalAnalysis")]
        public bool IsAssociatedWithProject { get; set; }

        /// <summary>Gets a value indicating whether can run local analysis.</summary>
        [AlsoNotifyFor("CanCancelAnalysis")]
        public bool CanRunLocalAnalysis
        {
            get
            {
                if (!this.IsAssociatedWithProject)
                {                    
                    return false;
                }

                return this.CanRunAnalysis;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can run local analysis and its associated.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can run local analysis and its associated; otherwise, <c>false</c>.
        /// </value>
        public bool CanCancelAnalysis
        {
            get
            {
                if (!this.IsAssociatedWithProject)
                {
                    return false;
                }

                return !this.CanRunLocalAnalysis;
            }
        }
        
        /// <summary>
        ///     Gets or sets the issues grid view.
        /// </summary>
        public IssueGridViewModel IssuesGridView { get; set; }

        /// <summary>
        ///     Gets or sets the open source directory command.
        /// </summary>
        public ICommand OpenSourceDirCommand { get; set; }

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
        ///     Gets or sets the preview command.
        /// </summary>
        public ICommand PreviewCommand { get; set; }

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
            get
            {
                return this.showFlyouts;
            }

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
        ///     Gets or sets the stop local analysis command.
        /// </summary>
        public ICommand StopLocalAnalysisCommand { get; set; }

        /// <summary>
        /// Gets or sets the open log command.
        /// </summary>
        /// <value>
        /// The open log command.
        /// </value>
        public ICommand OpenLogCommand { get; set; }

        /// <summary>
        /// Gets or sets the go to previous issue command.
        /// </summary>
        /// <value>
        /// The go to previous issue command.
        /// </value>
        public ICommand GoToPrevIssueCommand { get; set; }

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
        /// Called when [show issues on modified sections of file only changed].
        /// </summary>
        public void OnShowIssuesOnModifiedSectionsOfFileOnlyChanged()
        {
            this.UpdateLocalIssues(this, null);
        }

        /// <summary>
        /// Called when [show false positives and resolved issues changed].
        /// </summary>
        public void OnShowFalsePositivesAndResolvedIssuesChanged()
        {
            this.UpdateLocalIssues(this, null);
        }

        /// <summary>
        /// Reset Stats.
        /// </summary>
        public void ResetStats()
        {
            if (this.IssuesGridView != null)
            {
                this.IssuesGridView.ResetStatistics();
            }
        }

        /// <summary>
        ///     The end data association.
        /// </summary>
        public void OnSolutionClosed()
        {
            this.sourceInServerCache.Clear();
            this.falseandwontfixissues.Clear();
            this.newAddedIssues.Clear();
            this.ClearIssues();
            this.associatedProject = null;
            this.CanRunAnalysis = false;
            this.IsAssociatedWithProject = false;
            this.localAnalyserModule.OnDisconect();
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public void OnDisconnect()
        {
            this.newAddedIssues.Clear();
            this.sourceInServerCache.Clear();
            this.falseandwontfixissues.Clear();
            this.localAnalyserModule.OnDisconect();
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        /// <param name="availableProjects">The available projects.</param>
        /// <param name="issuePlugin">The issue plugin.</param>
        public void OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IIssueTrackerPlugin issuePlugin)
        {
            // does nothing
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
        public List<Issue> GetIssuesForResource(Resource file, string fileContent, out bool shownfalseandresolved)
        {
            shownfalseandresolved = this.ShowFalsePositivesAndResolvedIssues;

            return
                this.IssuesGridView.Issues.Where(
                    issue =>
                    this.IssuesGridView.IsNotFiltered(issue) && file.Key.Equals(issue.Component))
                    .ToList();
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
        public void AssociateWithNewProject(Resource project, string workingDir, ISourceControlProvider provider, Dictionary<string, Profile> profile)
        {
            if (project == null || string.IsNullOrEmpty(project.SolutionRoot))
            {
                return;
            }

            this.associatedProject = project;
            this.IsAssociatedWithProject = this.associatedProject != null;
            this.SourceWorkingDir = workingDir;
            this.CanRunAnalysis = true;
            this.FileAnalysisIsEnabled = true;
        }

        /// <summary>
        /// The on analysis command.
        /// </summary>
        public void OnAnalysisCommand()
        {
            this.FileAnalysisIsEnabled = false;
            this.notificationManager.StartedWorking("Running Full Analysis");
            this.CanRunAnalysis = false;
            this.RunLocalAnalysis(AnalysisTypes.ANALYSIS, false);
        }

        /// <summary>
        /// Runs the analysis.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="fromSave">if set to <c>true</c> [from save].</param>
        public void RunAnalysis(AnalysisTypes mode, bool fromSave)
        {
            this.FileAnalysisIsEnabled = false;
            this.notificationManager.StartedWorking(mode.ToString());
            this.CanRunAnalysis = false;
            this.RunLocalAnalysis(mode, fromSave);
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
        ///     The on file analysis is enabled changed.
        /// </summary>
        public void OnFileAnalysisIsEnabledChanged()
        {
            if (!this.CanRunAnalysis)
            {
                return;
            }

            if (this.FileAnalysisIsEnabled)
            {
                this.RunLocalAnalysis(AnalysisTypes.FILE, false);
            }
        }

        /// <summary>
        ///     The on incremental command.
        /// </summary>
        public void OnIncrementalCommand()
        {
            this.CanRunAnalysis = false;
            this.FileAnalysisIsEnabled = false;
            this.notificationManager.StartedWorking("Running Incremental Analysis");
            this.RunLocalAnalysis(AnalysisTypes.INCREMENTAL, false);
        }

        /// <summary>
        /// Shows the new added issues and lock.
        /// </summary>
        public void ShowNewAddedIssuesAndLock()
        {
            this.FileAnalysisIsEnabled = true;
            this.notificationManager.StartedWorking("Display current new issues");
            this.CanRunAnalysis = true;
            this.AllLog.Clear();
            this.OutputLog = string.Empty;
            this.UpdateNewLocalIssues();
        }

        /// <summary>
        ///     The on preview command.
        /// </summary>
        public void OnPreviewCommand()
        {
            this.FileAnalysisIsEnabled = false;
            this.notificationManager.StartedWorking("Running Preview Analysis");
            this.CanRunAnalysis = false;
            this.RunLocalAnalysis(AnalysisTypes.PREVIEW, false);
        }

        /// <summary>
        ///     The on selected view changed.
        /// </summary>
        public void OnSelectedViewChanged()
        {
            this.OnAnalysisModeHasChange(EventArgs.Empty);
        }

        /// <summary>
        /// The refresh data for resource.
        /// </summary>
        /// <param name="resourceFile">The resource file.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="content">The content.</param>
        /// <param name="fromSave">if set to <c>true</c> [from save].</param>
        public void RefreshDataForResource(Resource resourceFile, string resourceName, string content, bool fromSave)
        {
            this.resourceInView = resourceFile;
            this.resourceNameInView = resourceName;
            this.contentInView = content;

            if (this.FileAnalysisIsEnabled)
            {
                this.notificationManager.StartedWorking("Running File Analysis");
                this.RunLocalAnalysis(AnalysisTypes.FILE, fromSave);
            }
            else
            {
                this.OnSelectedViewChanged();
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
        /// Clears the issues.
        /// </summary>
        public void ClearIssues()
        {
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
        public void UpdateServices(
            IVsEnvironmentHelper vsenvironmenthelperIn,
            IVSSStatusBar statusBarIn, 
            IServiceProvider provider)
        {
            this.vsenvironmenthelper = vsenvironmenthelperIn;
            this.statusBar = statusBarIn;
            this.serviceProvier = provider;
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

        /// <summary>The trigger a project analysis.</summary>
        /// <param name="project">The project.</param>
        public void TriggerAProjectAnalysis(VsProjectItem project)
        {
            if (this.FileAnalysisIsEnabled && this.WasAssemblyUpdated(project))
            {
                var isProjectAnalysisOn = true;

                try
                {
                    isProjectAnalysisOn = bool.Parse(
                        this.configurationHelper.ReadSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.LocalAnalysisProjectAnalysisEnabledKey).Value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                if (isProjectAnalysisOn)
                {
                    this.localAnalyserModule.RunProjectAnalysis(project, AuthtenticationHelper.AuthToken);
                }
                else
                {
                    this.notificationManager.ReportMessage(new VSSonarPlugins.Message { Id = "LocalViewModel", Data = "Project Analysis Is Disabled, Enable in Options" });
                }                
            }
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
            this.OpenSourceDirCommand = new RelayCommand(this.OnOpenSourceDirCommand);
            this.IncrementalCommand = new RelayCommand(this.OnIncrementalCommand);
            this.PreviewCommand = new RelayCommand(this.OnPreviewCommand);
            this.AnalysisCommand = new RelayCommand(this.OnAnalysisCommand);
            this.NewIssuesDuringSessionCommand = new RelayCommand(this.ShowNewAddedIssuesAndLock);
            this.StopLocalAnalysisCommand = new RelayCommand(this.OnStopLocalAnalysisCommand);
            this.OpenLogCommand = new RelayCommand(this.OnOpenLogCommand);
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
        ///     The on open source dir command.
        /// </summary>
        private void OnOpenSourceDirCommand()
        {
            using (var filedialog = new OpenFileDialog { Filter = @"visual studio solution|*.sln" })
            {
                DialogResult result = filedialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    if (File.Exists(filedialog.FileName))
                    {
                        try
                        {
                            this.notificationManager.AssociateProjectToSolution(
                                Path.GetFileName(filedialog.FileName),
                                Directory.GetParent(filedialog.FileName).ToString());
                        }
                        catch (Exception ex)
                        {
                            UserExceptionMessageBox.ShowException(@"Could Not Associate Solution With: " + ex.Message, ex);
                        }
                    }
                    else
                    {
                        UserExceptionMessageBox.ShowException(@"Error Choosing File, File Does not exits", null);
                    }
                }
            }
        }

        /// <summary>
        ///     The on stop local analysis command.
        /// </summary>
        private void OnStopLocalAnalysisCommand()
        {
            if (this.localAnalyserModule == null)
            {
                return;
            }

            this.localAnalyserModule.StopAllExecution();
            this.FileAnalysisIsEnabled = false;
            this.CanRunAnalysis = true;
            this.notificationManager.EndedWorking();
        }

        /// <summary>
        /// The run local analysis new.
        /// </summary>
        /// <param name="analysis">The analysis.</param>
        /// <param name="fromSave">if set to <c>true</c> [from save].</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void RunLocalAnalysis(AnalysisTypes analysis, bool fromSave)
        {
            if (this.localAnalyserModule == null)
            {
                return;
            }

            this.PermissionsAreNotAvailable = false;
            try
            {
                this.associatedProject.ActiveConfiguration = this.vsenvironmenthelper.ActiveConfiguration();
                this.associatedProject.ActivePlatform = this.vsenvironmenthelper.ActivePlatform();
                this.notificationManager.ResetFailure();
                switch (analysis)
                {                    
                    case AnalysisTypes.FILE:

                        if (this.resourceInView == null)
                        {
                            return;
                        }

                        this.PopulateFalsePositivesAndResolvedItems();

                        try
                        {
                            var itemInView = this.vsenvironmenthelper.VsFileItem(this.resourceNameInView, this.associatedProject, this.resourceInView);
                            this.localAnalyserModule.AnalyseFile(
                                itemInView,
                                this.associatedProject,
                                this.notificationManager.AnalysisChangeLines,
                                AuthtenticationHelper.AuthToken.SonarVersion,
                                AuthtenticationHelper.AuthToken,
                                this.keyTranslator,
                                this.vsenvironmenthelper,
                                fromSave);
                        }
                        catch (Exception ex)
                        {
                            this.notificationManager.ReportMessage(new VSSonarPlugins.Message { Id = "LocalViewModel", Data = "Analysis Failed: " + ex.Message });
                            this.notificationManager.ReportException(ex);
                        }

                        break;
                    case AnalysisTypes.ANALYSIS:
                        this.ValidateAdminRights();
                        this.AllLog.Clear();
                        this.OutputLog = string.Empty;
                        this.localAnalyserModule.RunFullAnalysis(
                            this.associatedProject,
                            AuthtenticationHelper.AuthToken.SonarVersion,
                            AuthtenticationHelper.AuthToken);
                        break;
                    case AnalysisTypes.INCREMENTAL:
                        this.ValidateAdminRights();
                        this.AllLog.Clear();
                        this.OutputLog = string.Empty;
                        this.localAnalyserModule.RunIncrementalAnalysis(
                            this.associatedProject,
                            AuthtenticationHelper.AuthToken.SonarVersion,
                            AuthtenticationHelper.AuthToken);
                        break;
                    case AnalysisTypes.PREVIEW:
                        this.ValidateAdminRights();
                        this.AllLog.Clear();
                        this.OutputLog = string.Empty;
                        this.localAnalyserModule.RunPreviewAnalysis(
                            this.associatedProject,
                            AuthtenticationHelper.AuthToken.SonarVersion,
                            AuthtenticationHelper.AuthToken);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                this.notificationManager.ReportMessage(new VSSonarPlugins.Message { Id = "LocalViewModel", Data = "Analysis Failed: " + ex.Message });
                this.notificationManager.ReportException(ex);
                this.notificationManager.EndedWorking();
                this.notificationManager.FlagFailure(ex.Message);
            }
        }

        /// <summary>
        /// Validates the admin rights.
        /// </summary>
        private void ValidateAdminRights()
        {
            if (!this.vsenvironmenthelper.DoIHaveAdminRights() && !File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "cxx-user-options.xml")))
            {
                MessageDisplayBox.DisplayMessage(
                    "Admin rights not found.",
                    "Installing third party analyzers during analysis requires administration rights. Its recommended to create a cxx-user-options.xml in your home folder with location of the tools.",
                    "https://github.com/jmecsoftware/sonar-cxx-msbuild-tasks#using-the-wrapper-behind-proxy-or-were-admin-rights-are-not-available");
            }
        }

        /// <summary>
        /// Populates the false positives and resolved items.
        /// </summary>
        private void PopulateFalsePositivesAndResolvedItems()
        {
            if (this.falseandwontfixissues.ContainsKey(this.resourceInView.Key.Trim()))
            {
                return;
            }

            using (var bw = new BackgroundWorker { WorkerReportsProgress = true })
            {
                bw.RunWorkerCompleted += delegate
                {
                };

                bw.DoWork +=
                    delegate
                    {
                        try
                        {
                            var filter = "?componentRoots=" + this.resourceInView.Key.Trim() + "&resolutions=FALSE-POSITIVE,WONTFIX";
                            var issues = this.restService.GetIssues(AuthtenticationHelper.AuthToken, filter, this.associatedProject.Key);
                            this.falseandwontfixissues.Add(this.resourceInView.Key.Trim(), issues);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    };

                bw.RunWorkerAsync(); 
            }
        }


        /// <summary>
        /// The update local issues in view.
        /// </summary>
        private void UpdateNewLocalIssues()
        {
            this.CanRunAnalysis = true;
            this.AnalysisIsRunning = false;
            this.notificationManager.EndedWorking();
            var issues = new List<Issue>();
            foreach (var item in this.newAddedIssues)
            {
                issues.AddRange(item.Value);
            }

            this.IssuesGridView.UpdateIssues(issues);
            this.IssuesGridView.RefreshStatistics();
        }

        /// <summary>
        /// The update local issues in view.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void UpdateLocalIssues(object sender, EventArgs e)
        {
            this.CanRunAnalysis = true;
            this.AnalysisIsRunning = false;
            this.notificationManager.EndedWorking();

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

            try
            {
                var exceptionMsg = (LocalAnalysisEventArgs)e;
                if (exceptionMsg != null && exceptionMsg.Ex != null)
                {
                    this.OnSelectedViewChanged();
                    this.IssuesGridView.Issues.Clear();
                    this.ErrorsFoundDuringAnalysis = true;
                    UserExceptionMessageBox.ShowException("Analysis Failed: " + exceptionMsg.ErrorMessage, exceptionMsg.Ex, exceptionMsg.Ex.StackTrace);
                    return;
                }
            }
            catch (Exception ex)
            {
                this.ErrorsFoundDuringAnalysis = true;
                this.OnSelectedViewChanged();
                this.IssuesGridView.Issues.Clear();
                UserExceptionMessageBox.ShowException("Analysis Failed: ", ex, ex.StackTrace);
            }

            try
            {
                if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(
                        () =>
                            {
                                var issues = this.localAnalyserModule.GetIssues(
                                        AuthtenticationHelper.AuthToken,
                                        this.associatedProject);

                                if (!this.FileAnalysisIsEnabled)
                                {
                                    this.IssuesGridView.UpdateIssues(issues);
                                    this.OnSelectedViewChanged();
                                    return;
                                }


                                try
                                {
                                    var source = string.Empty;

                                    if (this.sourceInServerCache.ContainsKey(this.resourceInView.Key))
                                    {
                                        source = sourceInServerCache[this.resourceInView.Key];
                                    }
                                    else
                                    {
                                        var data = restService.GetSourceForFileResource(AuthtenticationHelper.AuthToken, this.resourceInView.Key).Lines;
                                        source = string.Join("\n", data);
                                        sourceInServerCache.Add(this.resourceInView.Key, source);
                                    }

                                    ArrayList diffReport = VsSonarUtils.GetSourceDiffFromStrings(source, this.contentInView, DiffEngineLevel.FastImperfect);

                                    var issuesWithoutFalsePositives = issues;

                                    if (!this.ShowFalsePositivesAndResolvedIssues)
                                    {
                                        issuesWithoutFalsePositives = this.FilterIssuesWithFalsePositives(issues);
                                    }

                                    var issuesinChangedLines = VsSonarUtils.GetIssuesInModifiedLinesOnly(issuesWithoutFalsePositives, diffReport);

                                    if (!this.newAddedIssues.ContainsKey(this.resourceInView.Key))
                                    {
                                        var newList = new List<Issue>(issuesinChangedLines);
                                        this.newAddedIssues.Add(this.resourceInView.Key, newList);
                                        this.notificationManager.OnNewIssuesUpdated();
                                    }
                                    else
                                    {
                                        var listOfIssues = this.newAddedIssues[this.resourceInView.Key];
                                        var currentCnt = listOfIssues.Count;
                                        var newCnt = issuesinChangedLines.Count;
                                        listOfIssues.Clear();
                                        listOfIssues.AddRange(issuesinChangedLines);

                                        if (currentCnt != newCnt)
                                        {
                                            this.notificationManager.OnNewIssuesUpdated();
                                        }
                                    }

                                    if (this.ShowIssuesOnModifiedSectionsOfFileOnly)
                                    {                                        
                                        this.IssuesGridView.UpdateIssues(issuesinChangedLines);
                                    }
                                    else
                                    {
                                        this.IssuesGridView.UpdateIssues(issuesWithoutFalsePositives);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (!this.newAddedIssues.ContainsKey(this.resourceInView.Key))
                                    {
                                        var newList = new List<Issue>(issues);
                                        this.newAddedIssues.Add(this.resourceInView.Key, newList);
                                        this.notificationManager.OnNewIssuesUpdated();
                                    }
                                    else
                                    {
                                        var listOfIssues = this.newAddedIssues[this.resourceInView.Key];
                                        var currentCnt = listOfIssues.Count;
                                        var newCnt = issues.Count;
                                        listOfIssues.Clear();
                                        listOfIssues.AddRange(issues);

                                        if (currentCnt != newCnt)
                                        {
                                            this.notificationManager.OnNewIssuesUpdated();
                                        }
                                    }

                                    this.notificationManager.ReportMessage(
                                        new VSSonarPlugins.Message
                                        {
                                            Id = "LocalAnalyserModel",
                                            Data = "Failed to check false positives and new issues : Likely new file : " + ex.Message
                                        });
                                    this.IssuesGridView.UpdateIssues(issues);
                                }

                                this.OnSelectedViewChanged();
                            });
                }
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Analysis Finish", ex, "Cannot retrieve Any Issues From Analysis. For the installed plugins");
            }

            this.IssuesGridView.RefreshStatistics();
        }

        /// <summary>
        /// Filters the issues with false positives.
        /// </summary>
        /// <param name="issues">The issues.</param>
        /// <returns>returns filtered issues</returns>
        private List<Issue> FilterIssuesWithFalsePositives(List<Issue> issues)
        {
            var filteredIssues = new List<Issue>();
            try
            {
                var falseissues = this.falseandwontfixissues[this.resourceInView.Key];

                foreach (var issue in issues)
                {
                    bool isfalse = false;
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
        /// The update output messages from plugin.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void UpdateOutputMessagesFromPlugin(object sender, EventArgs e)
        {
            var exceptionMsg = (LocalAnalysisEventArgs)e;
            this.AllLog.Add(exceptionMsg.ErrorMessage);
            this.vsenvironmenthelper.WriteToVisualStudioOutput(exceptionMsg.ErrorMessage);

            if (!this.AnalysisIsRunning)
            {
                this.AnalysisIsRunning = true;
            }
            
            if (exceptionMsg.ErrorMessage.Contains(
                "You're not authorized to execute a dry run analysis. Please contact your SonarQube administrator."))
            {
                this.PermissionsAreNotAvailable = true;
            }

            if (exceptionMsg.ErrorMessage.Contains("INFO  - HTML Issues Report generated:"))
            {
                string[] split = { "INFO  - HTML Issues Report generated:" };
                var reportPath = exceptionMsg.ErrorMessage.Split(split, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                
                if (File.Exists(reportPath))
                {
                    this.vsenvironmenthelper.NavigateToResource(reportPath);
                }
            }

            if (exceptionMsg.ErrorMessage.Contains("ANALYSIS SUCCESSFUL, you can browse"))
            {
                string[] separatingChars = { "ANALYSIS SUCCESSFUL, you can browse" };
                this.ProjectLink = exceptionMsg.ErrorMessage.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries)[1];
            }
        }

        /// <summary>
        /// Called when [open log command].
        /// </summary>
        private void OnOpenLogCommand()
        {
            var logFile = this.configurationHelper.UserLogForAnalysisFile();
            var logFolder = Directory.GetParent(logFile).ToString();

            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }

            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            File.WriteAllLines(this.configurationHelper.UserLogForAnalysisFile(), this.AllLog);

            this.vsenvironmenthelper.OpenResourceInVisualStudio(
                this.configurationHelper.UserLogForAnalysisFile(),
                0,
                this.notificationManager.UserDefinedEditor);
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

            this.notificationManager.ReportMessage(new VSSonarPlugins.Message { Id = "LocalViewModel", Data = "Project output is Updated" });
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
        private ObservableCollection<IMenuItem> CreateRowContextMenu(ISonarRestService service, ISQKeyTranslator translator, ISonarLocalAnalyser analyser)
        {
            var menu = new ObservableCollection<IMenuItem>
                           {
                               SetExclusionsMenu.MakeMenu(service, this.IssuesGridView, this.notificationManager, translator, analyser),
                               SetSqaleMenu.MakeMenu(service, this.IssuesGridView, this.notificationManager, translator, analyser)
                           };

            return menu;
        }
    }
}