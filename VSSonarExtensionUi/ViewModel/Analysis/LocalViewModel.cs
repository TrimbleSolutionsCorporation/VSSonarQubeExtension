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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;

    using ExtensionTypes;

    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using SonarLocalAnalyser;

    using SonarRestService;

    using VSSonarExtensionUi.Helpers;
    using VSSonarExtensionUi.View.Helpers;
    using VSSonarExtensionUi.ViewModel.Helpers;

    using VSSonarPlugins;

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
    ///     The local view viewModel.
    /// </summary>
    [ImplementPropertyChanged]
    public class LocalViewModel : IAnalysisViewModelBase
    {
        #region Fields

        /// <summary>
        ///     The sonar qube view viewModel.
        /// </summary>
        private readonly SonarQubeViewModel sonarQubeViewModel;

        /// <summary>
        ///     The show flyouts.
        /// </summary>
        private bool showFlyouts;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalViewModel" /> class.
        /// </summary>
        public LocalViewModel()
        {
            this.IssuesGridView = new IssueGridViewModel();
            this.OuputLogLines = new PaginatedObservableCollection<string>(100);
            this.Header = "Local Analysis";
            this.InitCommanding();
            this.InitFileAnalysis();

            this.ShowFlyouts = true;
            this.SizeOfFlyout = 150;

            this.ForeGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalViewModel"/> class.
        /// </summary>
        /// <param name="sonarQubeViewModel">
        /// The sonar qube view viewModel.
        /// </param>
        /// <param name="plugins">
        /// The plugins.
        /// </param>
        /// <param name="service">
        /// The service.
        /// </param>
        /// <param name="helper">
        /// The helper.
        /// </param>
        public LocalViewModel(
            SonarQubeViewModel sonarQubeViewModel, 
            List<IAnalysisPlugin> plugins, 
            ISonarRestService service, 
            IVsEnvironmentHelper helper)
        {
            this.RestService = service;
            this.Vsenvironmenthelper = helper;
            this.sonarQubeViewModel = sonarQubeViewModel;
            this.Header = "Local Analysis";
            this.IssuesGridView = new IssueGridViewModel(sonarQubeViewModel, false);
            this.OuputLogLines = new PaginatedObservableCollection<string>(300);

            this.InitCommanding();
            this.InitFileAnalysis();

            this.LocalAnalyserModule = new SonarLocalAnalyser(plugins, this.RestService, this.Vsenvironmenthelper);
            this.LocalAnalyserModule.StdOutEvent += this.UpdateOutputMessagesFromPlugin;
            this.LocalAnalyserModule.LocalAnalysisCompleted += this.UpdateLocalIssues;

            this.ShowFlyouts = false;
            this.SizeOfFlyout = 0;

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
        ///     Gets or sets the analysis command.
        /// </summary>
        public ICommand AnalysisCommand { get; set; }

        /// <summary>
        ///     Gets or sets the associated project.
        /// </summary>
        public Resource AssociatedProject { get; set; }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can run analysis.
        /// </summary>
        public bool CanRunAnalysis { get; set; }

        /// <summary>
        ///     Gets or sets the close flyout log viewer command.
        /// </summary>
        public ICommand CloseFlyoutLogViewerCommand { get; set; }

        /// <summary>
        ///     Gets or sets the document in view.
        /// </summary>
        public Resource DocumentInView { get; set; }

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
        public bool IsAssociatedWithProject { get; set; }

        /// <summary>
        ///     Gets or sets the issues grid view.
        /// </summary>
        public IssueGridViewModel IssuesGridView { get; set; }

        /// <summary>
        ///     Gets or sets the local analyser module.
        /// </summary>
        public ISonarLocalAnalyser LocalAnalyserModule { get; set; }

        /// <summary>
        ///     Gets or sets the open source dir command.
        /// </summary>
        public ICommand OpenSourceDirCommand { get; set; }

        /// <summary>
        ///     Gets or sets the ouput log lines.
        /// </summary>
        public PaginatedObservableCollection<string> OuputLogLines { get; set; }

        /// <summary>
        ///     Gets or sets the output log.
        /// </summary>
        public string OutputLog { get; set; }

        /// <summary>
        ///     Gets or sets the preview command.
        /// </summary>
        public ICommand PreviewCommand { get; set; }

        /// <summary>
        ///     Gets or sets the resource in editor.
        /// </summary>
        public string ResourceInEditor { get; set; }

        /// <summary>
        ///     Gets or sets the rest service.
        /// </summary>
        public ISonarRestService RestService { get; set; }

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
        ///     Gets or sets the size of flyout.
        /// </summary>
        public int SizeOfFlyout { get; set; }

        /// <summary>
        ///     Gets or sets the source working dir.
        /// </summary>
        public string SourceWorkingDir { get; set; }

        /// <summary>
        ///     Gets or sets the status bar.
        /// </summary>
        public IVSSStatusBar StatusBar { get; set; }

        /// <summary>
        ///     Gets or sets the stop local analysis command.
        /// </summary>
        public ICommand StopLocalAnalysisCommand { get; set; }

        /// <summary>
        ///     Gets or sets the vsenvironmenthelper.
        /// </summary>
        public IVsEnvironmentHelper Vsenvironmenthelper { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The end data association.
        /// </summary>
        public void EndDataAssociation()
        {
            this.AssociatedProject = null;
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
            return this.IssuesGridView.Issues.Where(issue => this.IssuesGridView.IsNotFiltered(issue)).ToList();
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
            this.IsAssociatedWithProject = this.AssociatedProject != null;

            this.SourceWorkingDir = workingDir;

            if (!string.IsNullOrEmpty(this.SourceWorkingDir) && Directory.Exists(this.SourceWorkingDir))
            {
                this.CanRunAnalysis = true;
            }
        }

        /// <summary>
        ///     The on analysis command.
        /// </summary>
        public void OnAnalysisCommand()
        {
            this.FileAnalysisIsEnabled = false;
            this.sonarQubeViewModel.IsExtensionBusy = true;
            this.CanRunAnalysis = false;
            this.sonarQubeViewModel.BusyToolTip = "Running Full Analysis";
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
                this.Vsenvironmenthelper.WriteOptionInApplicationData(
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
            this.sonarQubeViewModel.IsExtensionBusy = true;
            this.sonarQubeViewModel.BusyToolTip = "Running Incremental Analysis";
            this.RunLocalAnalysis(AnalysisTypes.INCREMENTAL);
        }

        /// <summary>
        ///     The on preview command.
        /// </summary>
        public void OnPreviewCommand()
        {
            this.FileAnalysisIsEnabled = false;
            this.sonarQubeViewModel.IsExtensionBusy = true;
            this.CanRunAnalysis = false;
            this.sonarQubeViewModel.BusyToolTip = "Running Preview Analysis";
            this.RunLocalAnalysis(AnalysisTypes.PREVIEW);
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
            this.DocumentInView = fullName;
            this.ResourceInEditor = documentInView;

            if (this.FileAnalysisIsEnabled)
            {
                this.sonarQubeViewModel.IsExtensionBusy = true;
                this.sonarQubeViewModel.BusyToolTip = "Running File Analysis";
                this.RunLocalAnalysis(AnalysisTypes.FILE);
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
            this.RestService = restServiceIn;
            this.Vsenvironmenthelper = vsenvironmenthelperIn;
            this.StatusBar = statusBar;
            this.ServiceProvier = provider;
            this.IssuesGridView.Vsenvironmenthelper = vsenvironmenthelperIn;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The clear association.
        /// </summary>
        internal void ClearAssociation()
        {
            this.AssociatedProject = null;
            this.CanRunAnalysis = false;
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
            this.StopLocalAnalysisCommand = new RelayCommand(this.OnStopLocalAnalysisCommand);

            this.FlyoutLogViewerCommand = new RelayCommand(this.OnFlyoutLogViewerCommand);
            this.CloseFlyoutLogViewerCommand = new RelayCommand(this.OnCloseFlyoutLogViewerCommand);
        }

        /// <summary>
        ///     The init file analysis.
        /// </summary>
        private void InitFileAnalysis()
        {
            string option = this.Vsenvironmenthelper.ReadOptionFromApplicationData("SonarOptionsGeneral", "FileAnalysisIsEnabled");
            if (string.IsNullOrEmpty(option))
            {
                this.FileAnalysisIsEnabled = true;
                return;
            }

            this.FileAnalysisIsEnabled = option.Equals("TRUE");
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
                    this.sonarQubeViewModel.AssociateProjectToSolution(
                        Path.GetFileName(filedialog.FileName), 
                        Directory.GetParent(filedialog.FileName).ToString());
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
            if (this.LocalAnalyserModule == null)
            {
                return;
            }

            this.LocalAnalyserModule.StopAllExecution();
        }

        /// <summary>
        /// The run local analysis new.
        /// </summary>
        /// <param name="analysis">
        /// The analysis.
        /// </param>
        private void RunLocalAnalysis(AnalysisTypes analysis)
        {
            this.OutputLog = string.Empty;
            try
            {
                switch (analysis)
                {
                    case AnalysisTypes.FILE:
                        this.LocalAnalyserModule.AnalyseFile(
                            this.Vsenvironmenthelper.VsProjectItem(this.sonarQubeViewModel.DocumentInView), 
                            this.AssociatedProject, 
                            this.sonarQubeViewModel.AnalysisChangeLines, 
                            this.sonarQubeViewModel.SonarVersion, 
                            this.sonarQubeViewModel.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig);
                        break;
                    case AnalysisTypes.ANALYSIS:
                        this.LocalAnalyserModule.RunFullAnalysis(
                            this.SourceWorkingDir, 
                            this.AssociatedProject, 
                            this.sonarQubeViewModel.SonarVersion, 
                            this.sonarQubeViewModel.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig);
                        break;
                    case AnalysisTypes.INCREMENTAL:
                        this.LocalAnalyserModule.RunIncrementalAnalysis(
                            this.SourceWorkingDir, 
                            this.AssociatedProject, 
                            this.sonarQubeViewModel.SonarVersion, 
                            this.sonarQubeViewModel.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig);
                        break;
                    case AnalysisTypes.PREVIEW:
                        this.LocalAnalyserModule.RunPreviewAnalysis(
                            this.SourceWorkingDir, 
                            this.AssociatedProject, 
                            this.sonarQubeViewModel.SonarVersion, 
                            this.sonarQubeViewModel.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig);
                        break;
                }
            }
            catch (VSSonarExtension ex)
            {
                UserExceptionMessageBox.ShowException("Analysis Failed: ", ex);
                this.CanRunAnalysis = true;
                this.sonarQubeViewModel.IsExtensionBusy = false;
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Critical Error: Please Report This Error: ", ex);
                this.CanRunAnalysis = true;
                this.sonarQubeViewModel.IsExtensionBusy = false;
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
            this.sonarQubeViewModel.IsExtensionBusy = false;

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
                                    this.LocalAnalyserModule.GetIssues(
                                        this.sonarQubeViewModel.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig, 
                                        this.AssociatedProject));
                                this.OnSelectedViewChanged();
                            });
                }
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Analysis Finish", ex, "Can Retrive Any Issues From Analysis. For the installed plugins");
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
            Application.Current.Dispatcher.Invoke(() => this.OuputLogLines.Insert(0, exceptionMsg.ErrorMessage));
            this.Vsenvironmenthelper.WriteToVisualStudioOutput(exceptionMsg.ErrorMessage);
        }

        #endregion
    }
}