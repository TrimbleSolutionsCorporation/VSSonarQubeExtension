// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServerViewModel.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Windows.Input;
    using System.Windows.Media;


    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using SonarRestService;

    using VSSonarExtensionUi.Cache;
    using VSSonarExtensionUi.ViewModel.Helpers;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using VSSonarPlugins.Helpers;

    /// <summary>
    ///     The server view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class ServerViewModel : IAnalysisViewModelBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServerViewModel" /> class.
        /// </summary>
        /// <param name="sonarQubeViewModel">
        ///     The sonar Qube View Model.
        /// </param>
        /// <param name="vsenvironmenthelper">
        ///     The vsenvironmenthelper.
        /// </param>
        /// <param name="restservice">
        ///     The restservice.
        /// </param>
        /// <param name="config">
        ///     The config.
        /// </param>
        public ServerViewModel(
            SonarQubeViewModel sonarQubeViewModel,
            IVsEnvironmentHelper vsenvironmenthelper,
            IConfigurationHelper configurationHelper,
            ISonarRestService restservice,
            ISonarConfiguration config)
        {
            this.sonarQubeViewModel = sonarQubeViewModel;
            this.vsenvironmenthelper = vsenvironmenthelper;
            this.configurationHelper = configurationHelper;
            this.restservice = restservice;
            this.config = config;

            this.Header = "Server Analysis";
            this.AlreadyOpenDiffs = new SortedSet<string>();
            this.IssuesGridView = new IssueGridViewModel(sonarQubeViewModel, true, "ServerView", true, this.configurationHelper);
            this.InitCommanding();

            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
        }

        #endregion

        public void UpdateOpenDiffWindowList(string fullName)
        {
            string[] delimiters = { "vs." };

            var files = fullName.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            if (files.Count() != 2)
            {
                return;
            }

            if (this.AlreadyOpenDiffs.Contains(files[1].Trim().Replace("local.", "")))
            {
                this.AlreadyOpenDiffs.Remove(files[1].Trim().Replace("local.", ""));
                this.vsenvironmenthelper.ClearDiffFile(files[0].Trim(), files[1].Trim());
            }
        }

        #region Fields

        /// <summary>
        ///     The config.
        /// </summary>
        private readonly ISonarConfiguration config;

        /// <summary>
        ///     The local editor cache.
        /// </summary>
        private readonly ModelEditorCache localEditorCache = new ModelEditorCache();

        /// <summary>
        ///     The sonar qube view model.
        /// </summary>
        private readonly SonarQubeViewModel sonarQubeViewModel;

        /// <summary>
        ///     The restservice.
        /// </summary>
        private ISonarRestService restservice;

        /// <summary>
        ///     The vsenvironmenthelper.
        /// </summary>
        private IVsEnvironmentHelper vsenvironmenthelper;

        private IConfigurationHelper configurationHelper;

        #endregion

        #region Public Events

        /// <summary>
        ///     The coverage was modified.
        /// </summary>
        public event ChangedEventHandler CoverageWasModified;

        /// <summary>
        ///     The analysis mode has change.
        /// </summary>
        public event ChangedEventHandler IssuesReadyForCollecting;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the associated project.
        /// </summary>
        public Resource AssociatedProject { get; set; }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the configuration.
        /// </summary>
        public object Configuration { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether coverage in editor enabled.
        /// </summary>
        public bool CoverageInEditorEnabled { get; set; }

        /// <summary>
        ///     Gets the display source diff command.
        /// </summary>
        public ICommand DisplaySourceDiffCommand { get; private set; }

        public ICommand GoToPrevIssueCommand { get; set; }

        public ICommand GoToNextIssueCommand { get; set; }

        /// <summary>
        ///     Gets or sets the document in view.
        /// </summary>
        public string DocumentInView { get; set; }

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        ///     Gets a value indicating whether is running in visual studio.
        /// </summary>
        public bool IsRunningInVisualStudio { get; private set; }

        /// <summary>
        ///     Gets the issues grid view.
        /// </summary>
        public IssueGridViewModel IssuesGridView { get; private set; }

        /// <summary>
        ///     Gets or sets the resource in editor.
        /// </summary>
        public Resource ResourceInEditor { get; set; }

        /// <summary>
        ///     Gets or sets the service provier.
        /// </summary>
        public IServiceProvider ServiceProvier { get; set; }

        /// <summary>
        ///     Gets or sets the status bar.
        /// </summary>
        public IVSSStatusBar StatusBar { get; set; }

        #endregion

        #region Public Methods and Operators

        public void ResetStats()
        {
            if (this.IssuesGridView != null)
            {
                this.IssuesGridView.ResetStatistics();
            }
        }

        /// <summary>
        ///     The clear cache.
        /// </summary>
        public void ClearCache()
        {
            this.localEditorCache.ClearData();
        }

        /// <summary>The trigger a project analysis.</summary>
        /// <param name="project">The project.</param>
        public void TriggerAProjectAnalysis(VsProjectItem project)
        {
        }

        /// <summary>
        ///     The end data association.
        /// </summary>
        public void EndDataAssociation()
        {
            this.IsRunningInVisualStudio = false;
            this.AssociatedProject = null;
        }

        /// <summary>
        ///     The get coverage in editor.
        /// </summary>
        /// <param name="currentSourceBuffer">
        ///     The current source buffer.
        /// </param>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public Dictionary<int, CoverageElement> GetCoverageInEditor(string currentSourceBuffer)
        {
            if (this.CoverageInEditorEnabled)
            {
                this.sonarQubeViewModel.Logger.WriteMessage("Requested Coverage - Coverge enabled");
                return this.localEditorCache.GetCoverageDataForResource(this.ResourceInEditor, currentSourceBuffer);
            }

            return new Dictionary<int, CoverageElement>();
        }

        /// <summary>
        ///     The get issues for resource.
        /// </summary>
        /// <param name="file">
        ///     The file.
        /// </param>
        /// <param name="fileContent">
        ///     The file content.
        /// </param>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public List<Issue> GetIssuesForResource(Resource file, string fileContent)
        {
            if (this.DocumentInView == null || this.ResourceInEditor == null)
            {
                return new List<Issue>();
            }
            this.IssuesGridView.RefreshStatistics();
            var issuesWithModifiedData = this.localEditorCache.GetIssuesForResource(this.ResourceInEditor, fileContent);
            return issuesWithModifiedData.Where(issue => this.IssuesGridView.IsNotFiltered(issue)).ToList();
        }

        /// <summary>
        ///     The init data association.
        /// </summary>
        /// <param name="associatedProject">
        ///     The associated project.
        /// </param>
        /// <param name="userConnectionConfig">
        ///     The user connection config.
        /// </param>
        /// <param name="workingDir">
        ///     The working dir.
        /// </param>
        public void InitDataAssociation(Resource associatedProject, ISonarConfiguration userConnectionConfig, string workingDir)
        {
            this.AssociatedProject = associatedProject;
            this.Configuration = userConnectionConfig;
            if (this.sonarQubeViewModel.VsHelper != null)
            {
                this.IsRunningInVisualStudio = this.sonarQubeViewModel.VsHelper.AreWeRunningInVisualStudio();
            }
            else
            {
                this.IsRunningInVisualStudio = false;
            }
        }

        /// <summary>
        ///     The on changed.
        /// </summary>
        /// <param name="e">
        ///     The e.
        /// </param>
        public void OnAnalysisModeHasChange(EventArgs e)
        {
            if (this.IssuesReadyForCollecting != null)
            {
                this.sonarQubeViewModel.Logger.WriteMessage("Trigger issues update");
                this.IssuesReadyForCollecting(this, e);
                if (this.CoverageWasModified != null)
                {
                    this.sonarQubeViewModel.Logger.WriteMessage("Trigger Coverage Update");
                    this.CoverageWasModified(this, e);
                }
            }
        }

        /// <summary>
        ///     The on coverage in editor enabled changed.
        /// </summary>
        public void OnCoverageInEditorEnabledChanged()
        {
            this.OnCoverageWasModified(EventArgs.Empty);
            this.sonarQubeViewModel.Logger.WriteMessage("CoverageInEditorEnabled Changed");
        }

        /// <summary>
        ///     The on selected view changed.
        /// </summary>
        public void OnSelectedViewChanged()
        {
            this.OnAnalysisModeHasChange(EventArgs.Empty);
            this.sonarQubeViewModel.Logger.WriteMessage("OnSelectedViewChanged Changed");
        }

        /// <summary>
        ///     The refresh data for resource.
        /// </summary>
        /// <param name="res">
        ///     The res.
        /// </param>
        /// <param name="documentInView">
        ///     The document in view.
        /// </param>
        public void RefreshDataForResource(Resource res, string documentInView)
        {
            this.DocumentInView = documentInView;
            this.ResourceInEditor = res;
            var newCoverage = this.restservice.GetCoverageInResource(this.config, this.ResourceInEditor.Key);
            var newSource = VsSonarUtils.GetLinesFromSource(this.restservice.GetSourceForFileResource(this.config, this.ResourceInEditor.Key), "\r\n");
            var newIssues = this.restservice.GetIssuesInResource(this.config, this.ResourceInEditor.Key);

            this.IssuesGridView.Issues.Clear();
            this.IssuesGridView.AllIssues.Clear();
            foreach (var newIssue in newIssues)
            {
                this.IssuesGridView.AllIssues.Add(newIssue);
                this.IssuesGridView.Issues.Add(newIssue);
            }

            this.localEditorCache.UpdateResourceData(this.ResourceInEditor, newCoverage, newIssues, newSource);
            this.OnSelectedViewChanged();
        }

        /// <summary>
        ///     The update colours.
        /// </summary>
        /// <param name="background">
        ///     The background.
        /// </param>
        /// <param name="foreground">
        ///     The foreground.
        /// </param>
        public void UpdateColours(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;

            this.IssuesGridView.UpdateColours(background, foreground);
        }

        private void OnGoToNextIssueCommand()
        {
            this.IssuesGridView.GoToNextIssue();
        }

        private void OnGoToPrevIssueCommand()
        {
            this.IssuesGridView.GoToPrevIssue();
        }

        /// <summary>
        ///     The update services.
        /// </summary>
        /// <param name="restServiceIn">
        ///     The rest service in.
        /// </param>
        /// <param name="vsenvironmenthelperIn">
        ///     The vsenvironmenthelper in.
        /// </param>
        /// <param name="statusBar">
        ///     The status bar.
        /// </param>
        /// <param name="provider">
        ///     The provider.
        /// </param>
        public void UpdateServices(
            ISonarRestService restServiceIn,
            IVsEnvironmentHelper vsenvironmenthelperIn,
            IConfigurationHelper configurationHelper,
            IVSSStatusBar statusBar,
            IServiceProvider provider)
        {
            this.configurationHelper = configurationHelper;
            this.restservice = restServiceIn;
            this.vsenvironmenthelper = vsenvironmenthelperIn;
            this.StatusBar = statusBar;
            this.ServiceProvier = provider;
            this.IssuesGridView.Vsenvironmenthelper = vsenvironmenthelperIn;

            this.IssuesGridView.UpdateVsService(this.vsenvironmenthelper);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The on coverage was modified.
        /// </summary>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected virtual void OnCoverageWasModified(EventArgs e)
        {
            if (this.CoverageWasModified != null)
            {
                this.sonarQubeViewModel.Logger.WriteMessage("Triggering Event: OnCoverageWasModified");
                this.CoverageWasModified(this, e);
            }
        }

        /// <summary>
        ///     The init commanding.
        /// </summary>
        private void InitCommanding()
        {
            this.DisplaySourceDiffCommand = new RelayCommand(this.OnDisplaySourceDiffCommand);
            this.GoToPrevIssueCommand = new RelayCommand(this.OnGoToPrevIssueCommand);
            this.GoToNextIssueCommand = new RelayCommand(this.OnGoToNextIssueCommand);
        }

        /// <summary>
        ///     The on display source diff command.
        /// </summary>
        private void OnDisplaySourceDiffCommand()
        {
            if (this.ResourceInEditor != null && this.DocumentInView != null)
            {
                if (this.AlreadyOpenDiffs.Contains(Path.GetFileName(this.DocumentInView)))
                {
                    return;
                }

                this.AlreadyOpenDiffs.Add(Path.GetFileName(this.DocumentInView));

                try
                {
                    this.vsenvironmenthelper.ShowSourceDiff(this.localEditorCache.GetSourceForResource(this.ResourceInEditor), this.DocumentInView);
                }
                catch (Exception ex)
                {
                    this.sonarQubeViewModel.NotificationManager.ReportException(ex);
                }
            }
        }

        public SortedSet<string> AlreadyOpenDiffs { get; set; }

        #endregion
    }
}