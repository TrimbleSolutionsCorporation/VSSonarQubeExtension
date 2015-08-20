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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;

    using GalaSoft.MvvmLight.Command;
    using Helpers;

    using Model.Analysis;
    using Model.Helpers;
    using PropertyChanged;
    using SonarLocalAnalyser;
    using View.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using Application = System.Windows.Application;

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
        #region Fields

        /// <summary>
        /// The plugins
        /// </summary>
        private readonly List<IAnalysisPlugin> plugins = new List<IAnalysisPlugin>();

        /// <summary>
        /// The rest service
        /// </summary>
        private readonly ISonarRestService restService;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        /// The local analyser module
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
        ///     The show flyouts.
        /// </summary>
        private bool showFlyouts;

        /// <summary>
        /// The vsenvironmenthelper
        /// </summary>
        private IVsEnvironmentHelper vsenvironmenthelper;

        /// <summary>
        /// The status bar
        /// </summary>
        private IVSSStatusBar statusBar;

        /// <summary>
        /// The service provier
        /// </summary>
        private IServiceProvider serviceProvier;

        /// <summary>
        /// The user conf
        /// </summary>
        private ISonarConfiguration userConf;

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

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalViewModel" /> class.
        /// </summary>
        /// <param name="pluginsIn">The plugins in. TODO they must be also updatetable</param>
        /// <param name="service">The service.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="translator">The translator.</param>
        public LocalViewModel(
            List<IAnalysisPlugin> pluginsIn, 
            ISonarRestService service, 
            IConfigurationHelper configurationHelper,
            INotificationManager notificationManager,
            ISQKeyTranslator translator)
        {
            this.keyTranslator = translator;
            this.notificationManager = notificationManager;
            this.restService = service;
            this.configurationHelper = configurationHelper;

            this.plugins = pluginsIn;
            this.Header = "Local Analysis";
            this.IssuesGridView = new IssueGridViewModel(false, "LocalView", false, this.configurationHelper, service, notificationManager, translator);
            this.OuputLogLines = new PaginatedObservableCollection<string>(300);
            this.AllLog = new List<string>();


            this.InitCommanding();
            this.InitFileAnalysis();

            this.localAnalyserModule = new SonarLocalAnalyser(this.plugins, this.restService, this.configurationHelper, this.notificationManager);
            this.localAnalyserModule.StdOutEvent += this.UpdateOutputMessagesFromPlugin;
            this.localAnalyserModule.LocalAnalysisCompleted += this.UpdateLocalIssues;
            this.localAnalyserModule.AssociateCommandCompeted += this.UpdateAssociateCommand;

            this.ShowFlyouts = false;
            this.SizeOfFlyout = 0;

            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;

            // register model
            SonarQubeViewModel.RegisterNewModelInPool(this);
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
        ///     Gets or sets the analysis command.
        /// </summary>
        public ICommand AnalysisCommand { get; set; }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>Gets or sets a value indicating whether loading sonar data.</summary>
        [AlsoNotifyFor("CanRunLocalAnalysis")]
        public bool LoadingSonarData { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can run analysis.
        /// </summary>
        public bool CanRunAnalysis { get; set; }

        /// <summary>Gets or sets the associating text tooltip.</summary>
        public string AssociatingTextTooltip { get; set; }

        /// <summary>
        ///     Gets or sets the close flyout log viewer command.
        /// </summary>
        public ICommand CloseFlyoutLogViewerCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether file analysis is enabled.
        /// </summary>
        public bool FileAnalysisIsEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the flyout log viewer command.
        /// </summary>
        public ICommand FlyoutLogViewerCommand { get; set; }

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
        public bool CanRunLocalAnalysis
        {
            get
            {
                if (this.LoadingSonarData)
                {
                    this.AssociatingTextTooltip = "Loading SonarQube profile Data";
                }
                else
                {
                    this.AssociatingTextTooltip = string.Empty;
                }

                return this.IsAssociatedWithProject && !this.LoadingSonarData;
            }
        }

        /// <summary>
        ///     Gets or sets the issues grid view.
        /// </summary>
        public IssueGridViewModel IssuesGridView { get; set; }

        /// <summary>
        ///     Gets or sets the open source dir command.
        /// </summary>
        public ICommand OpenSourceDirCommand { get; set; }

        /// <summary>
        ///     Gets or sets the ouput log lines.
        /// </summary>
        public PaginatedObservableCollection<string> OuputLogLines { get; set; }

        /// <summary>
        /// Gets or sets all log.
        /// </summary>
        /// <value>
        /// All log.
        /// </value>
        public List<string> AllLog { get; set; }

        /// <summary>
        ///     Gets or sets the output log.
        /// </summary>
        public string OutputLog { get; set; }

        /// <summary>
        ///     Gets or sets the preview command.
        /// </summary>
        public ICommand PreviewCommand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [permissions are not available].
        /// </summary>
        /// <value>
        /// <c>true</c> if [permissions are not available]; otherwise, <c>false</c>.
        /// </value>
        public bool PermissionsAreNotAvailable { get; set; }

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
        ///     Gets or sets the size of flyout.
        /// </summary>
        public int SizeOfFlyout { get; set; }

        /// <summary>
        ///     Gets or sets the source working dir.
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

        #endregion

        #region Public Methods and Operators

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
        public void EndDataAssociation()
        {
            this.associatedProject = null;
            this.CanRunAnalysis = false;
            this.IsAssociatedWithProject = false;
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
            return
                this.IssuesGridView.Issues.Where(
                    issue =>
                    this.IssuesGridView.IsNotFiltered(issue) && file.Key.Equals(issue.Component))
                    .ToList();
        }

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        public void AssociateWithNewProject(ISonarConfiguration config, Resource project, string workingDir)
        {
            this.userConf = config;
            this.associatedProject = project;
            this.IsAssociatedWithProject = this.associatedProject != null;

            this.SourceWorkingDir = workingDir;

            if (!string.IsNullOrEmpty(this.SourceWorkingDir) && Directory.Exists(this.SourceWorkingDir))
            {
                this.CanRunAnalysis = true;
                this.LoadingSonarData = true;
                this.localAnalyserModule.AssociateWithProject(project, config);
            }
        }

        /// <summary>
        ///     The on analysis command.
        /// </summary>
        public void OnAnalysisCommand()
        {
            this.FileAnalysisIsEnabled = false;            
            this.notificationManager.StartedWorking("Running Full Analysis");
            this.CanRunAnalysis = false;
            this.RunLocalAnalysis(AnalysisTypes.ANALYSIS);
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
            if (this.CanRunAnalysis)
            {
                this.configurationHelper.WriteOptionInApplicationData(
                    Context.AnalysisGeneral,
                    "SonarOptionsGeneral", 
                    "FileAnalysisIsEnabled", 
                    this.FileAnalysisIsEnabled ? "TRUE" : "FALSE");
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
            this.RunLocalAnalysis(AnalysisTypes.INCREMENTAL);
        }

        /// <summary>
        ///     The on preview command.
        /// </summary>
        public void OnPreviewCommand()
        {
            this.FileAnalysisIsEnabled = false;
            this.notificationManager.StartedWorking("Running Preview Analysis");
            this.CanRunAnalysis = false;
            this.RunLocalAnalysis(AnalysisTypes.PREVIEW);
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
        public void RefreshDataForResource(Resource resourceFile, string resourceName)
        {
            this.resourceInView = resourceFile;
            this.resourceNameInView = resourceName;

            if (this.FileAnalysisIsEnabled)
            {
                this.notificationManager.StartedWorking("Running File Analysis");
                this.RunLocalAnalysis(AnalysisTypes.FILE);
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
            if (this.FileAnalysisIsEnabled)
            {
                this.localAnalyserModule.RunProjectAnalysis(project, AuthtenticationHelper.AuthToken);
            }
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

        #endregion

        #region Methods

        /// <summary>
        ///     The init commanding.
        /// </summary>
        private void InitCommanding()
        {
            this.OpenSourceDirCommand = new RelayCommand(this.OnOpenSourceDirCommand);
            this.IncrementalCommand = new RelayCommand(this.OnIncrementalCommand);
            this.PreviewCommand = new RelayCommand(this.OnPreviewCommand);
            this.AnalysisCommand = new RelayCommand(this.OnAnalysisCommand);
            this.StopLocalAnalysisCommand = new RelayCommand(this.OnStopLocalAnalysisCommand);
            this.OpenLogCommand = new RelayCommand(this.OnOpenLogCommand);
            this.GoToPrevIssueCommand = new RelayCommand(this.OnGoToPrevIssueCommand);
            this.GoToNextIssueCommand = new RelayCommand(this.OnGoToNextIssueCommand);

            this.FlyoutLogViewerCommand = new RelayCommand(this.OnFlyoutLogViewerCommand);
            this.CloseFlyoutLogViewerCommand = new RelayCommand(this.OnCloseFlyoutLogViewerCommand);
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
        ///     The init file analysis.
        /// </summary>
        private void InitFileAnalysis()
        {
            try
            {
                var option = this.configurationHelper.ReadSetting(Context.AnalysisGeneral, "SonarOptionsGeneral", "FileAnalysisIsEnabled");
                this.FileAnalysisIsEnabled = option.Value.Equals("TRUE");
            }
            catch (Exception)
            {
                this.FileAnalysisIsEnabled = true;
            }
        }

        /// <summary>
        ///     The on close flyout log viewer command.
        /// </summary>
        private void OnCloseFlyoutLogViewerCommand()
        {
            this.ShowFlyouts = true;
            this.SizeOfFlyout = 150;
        }

        /// <summary>
        ///     The on flyout log viewer command.
        /// </summary>
        private void OnFlyoutLogViewerCommand()
        {
            this.ShowFlyouts = false;
            this.SizeOfFlyout = 0;
        }

        /// <summary>
        ///     The on open source dir command.
        /// </summary>
        private void OnOpenSourceDirCommand()
        {
            var filedialog = new OpenFileDialog { Filter = @"visual studio solution|*.sln" };
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
        }

        /// <summary>
        /// The run local analysis new.
        /// </summary>
        /// <param name="analysis">
        /// The analysis.
        /// </param>
        private void RunLocalAnalysis(AnalysisTypes analysis)
        {
            this.PermissionsAreNotAvailable = false;
            try
            {
                this.associatedProject.ActiveConfiguration = this.vsenvironmenthelper.ActiveConfiguration();
                this.associatedProject.ActivePlatform = this.vsenvironmenthelper.ActivePlatform();
                this.associatedProject.SolutionRoot = this.SourceWorkingDir;

                switch (analysis)
                {
                    case AnalysisTypes.FILE:
                        var itemInView = this.vsenvironmenthelper.VsFileItem(this.resourceNameInView, this.associatedProject, this.resourceInView);
                        this.localAnalyserModule.AnalyseFile(
                            itemInView, 
                            this.associatedProject,
                            this.notificationManager.AnalysisChangeLines, 
                            this.userConf.SonarVersion,
                            AuthtenticationHelper.AuthToken,
                            this.keyTranslator,
                            this.vsenvironmenthelper);
                        break;
                    case AnalysisTypes.ANALYSIS:
                        this.OuputLogLines.Clear();
                        this.AllLog.Clear();
                        this.OutputLog = string.Empty;
                        this.localAnalyserModule.RunFullAnalysis(
                            this.associatedProject, 
                            this.userConf.SonarVersion,
                            AuthtenticationHelper.AuthToken);
                        break;
                    case AnalysisTypes.INCREMENTAL:
                        this.OuputLogLines.Clear();
                        this.AllLog.Clear();
                        this.OutputLog = string.Empty;
                        this.localAnalyserModule.RunIncrementalAnalysis(
                            this.associatedProject, 
                            this.userConf.SonarVersion,
                            AuthtenticationHelper.AuthToken);
                        break;
                    case AnalysisTypes.PREVIEW:
                        this.OuputLogLines.Clear();
                        this.AllLog.Clear();
                        this.OutputLog = string.Empty;
                        this.localAnalyserModule.RunPreviewAnalysis(
                            this.associatedProject, 
                            this.userConf.SonarVersion,
                            AuthtenticationHelper.AuthToken);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (VSSonarExtension ex)
            {
                this.notificationManager.ReportMessage(new VSSonarPlugins.Message() { Id = "LocalViewModel", Data = "Analysis Failed: " + ex.Message });
                this.notificationManager.ReportException(ex);
                this.CanRunAnalysis = true;
                this.notificationManager.EndedWorking();
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Critical Error: Please Report This Error: cwd: " + this.SourceWorkingDir + " AssociatedProject: " + this.associatedProject, ex);
                this.CanRunAnalysis = true;
                this.notificationManager.EndedWorking();
            }
        }

        /// <summary>
        /// The update local issues in view.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void UpdateLocalIssues(object sender, EventArgs e)
        {
            this.CanRunAnalysis = true;
            this.notificationManager.EndedWorking();

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
                    UserExceptionMessageBox.ShowException("Analysis Ended: " + exceptionMsg.ErrorMessage, exceptionMsg.Ex, exceptionMsg.Ex.StackTrace);
                    return;
                }
            }
            catch (Exception ex)
            {
                this.OnSelectedViewChanged();
                this.IssuesGridView.Issues.Clear();
                UserExceptionMessageBox.ShowException("Analysis Ended: ", ex, ex.StackTrace);
            }

            try
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(
                        () =>
                            {
                                this.IssuesGridView.UpdateIssues(
                                    this.localAnalyserModule.GetIssues(
                                        AuthtenticationHelper.AuthToken, 
                                        this.associatedProject));
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
            Application.Current.Dispatcher.Invoke(() => this.OuputLogLines.Insert(0, exceptionMsg.ErrorMessage));
            this.AllLog.Add(exceptionMsg.ErrorMessage);
            this.vsenvironmenthelper.WriteToVisualStudioOutput(exceptionMsg.ErrorMessage);

            if (exceptionMsg.ErrorMessage.Contains(
                "You're not authorized to execute a dry run analysis. Please contact your SonarQube administrator."))
            {
                this.PermissionsAreNotAvailable = true;
            }
        }

        /// <summary>The update associate command.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void UpdateAssociateCommand(object sender, EventArgs e)
        {
            this.LoadingSonarData = false;

            this.notificationManager.RefreshDataForResource();
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

        #endregion
    }
}