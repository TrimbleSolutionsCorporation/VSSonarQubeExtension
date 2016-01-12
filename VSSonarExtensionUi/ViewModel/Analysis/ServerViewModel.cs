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
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Input;
    using System.Windows.Media;

    using Association;
    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using Model.Analysis;
    using Model.Cache;
    using Model.Helpers;
    using Model.Menu;

    using PropertyChanged;
    using SonarLocalAnalyser;
    using VSSonarPlugins;
    using VSSonarPlugins.Helpers;
    using VSSonarPlugins.Types;
    
    

    /// <summary>
    ///     The server view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class ServerViewModel : IAnalysisModelBase, IViewModelBase, IModelBase
    {
        /// <summary>
        ///     The local editor cache.
        /// </summary>
        private readonly ModelEditorCache localEditorCache = new ModelEditorCache();

        /// <summary>
        ///     The restservice.
        /// </summary>
        private readonly ISonarRestService restservice;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        /// The notification man
        /// </summary>
        private readonly INotificationManager notificationMan;

        /// <summary>
        ///     The vsenvironmenthelper.
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
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// The analyser
        /// </summary>
        private readonly ISonarLocalAnalyser analyser;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerViewModel" /> class.
        /// </summary>
        /// <param name="vsenvironmenthelper">The vsenvironmenthelper.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="restservice">The restservice.</param>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="translator">The translator.</param>
        /// <param name="analyser">The analyser.</param>
        public ServerViewModel(
            IVsEnvironmentHelper vsenvironmenthelper,
            IConfigurationHelper configurationHelper,
            ISonarRestService restservice,
            INotificationManager notificationManager,
            ISQKeyTranslator translator,
            ISonarLocalAnalyser analyser)
        {
            this.analyser = analyser;
            this.notificationMan = notificationManager;
            this.vsenvironmenthelper = vsenvironmenthelper;
            this.configurationHelper = configurationHelper;
            this.restservice = restservice;

            this.Header = "Server Analysis";
            this.AvailableActionPlans = new ObservableCollection<SonarActionPlan>();
            this.AlreadyOpenDiffs = new SortedSet<string>();
            this.IssuesGridView = new IssueGridViewModel("ServerView", true, this.configurationHelper, this.restservice, this.notificationMan, translator);
            this.IssuesGridView.ContextMenuItems = this.CreateRowContextMenu(restservice, translator);
            this.IssuesGridView.ShowContextMenu = true;
            this.IssuesGridView.ShowLeftFlyoutEvent += this.ShowHideLeftFlyout;
            this.InitCommanding();

            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
            this.SizeOfFlyout = 0;
            // register model
            AssociationModel.RegisterNewModelInPool(this);
        }

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
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether coverage in editor enabled.
        /// </summary>
        public bool CoverageInEditorEnabled { get; set; }

        /// <summary>
        ///     Gets the display source diff command.
        /// </summary>
        public ICommand DisplaySourceDiffCommand { get; private set; }

        /// <summary>
        ///     Gets the close flyout issue search command.
        /// </summary>
        public RelayCommand CloseLeftFlyoutCommand { get; private set; }

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
        /// Gets or sets the available gates.
        /// </summary>
        /// <value>
        /// The available gates.
        /// </value>
        public ObservableCollection<SonarActionPlan> AvailableActionPlans { get; set; }

        /// <summary>
        /// Gets or sets the already open diffs.
        /// </summary>
        /// <value>
        /// The already open diffs.
        /// </value>
        public SortedSet<string> AlreadyOpenDiffs { get; set; }

        /// <summary>
        /// Gets a value indicating whether [show left fly out].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show left fly out]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowLeftFlyOut { get; set; }

        /// <summary>
        /// Gets or sets the size of flyout.
        /// </summary>
        /// <value>
        /// The size of flyout.
        /// </value>
        public int SizeOfFlyout { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Updates the open difference window list.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        public void UpdateOpenDiffWindowList(string fullName)
        {
            string[] delimiters = { "vs." };

            var files = fullName.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            if (files.Count() != 2)
            {
                return;
            }

            if (this.AlreadyOpenDiffs.Contains(files[1].Trim().Replace("local.", string.Empty)))
            {
                this.AlreadyOpenDiffs.Remove(files[1].Trim().Replace("local.", string.Empty));
                this.vsenvironmenthelper.ClearDiffFile(files[0].Trim(), files[1].Trim());
            }
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
        /// Clears the issues.
        /// </summary>
        public void ClearIssues()
        {
            this.IssuesGridView.ResetStatistics();
            this.IssuesGridView.AllIssues.Clear();
            this.IssuesGridView.Issues.Clear();
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
        ///     The clear cache.
        /// </summary>
        public void ClearCache()
        {
            this.localEditorCache.ClearData();
        }

        /// <summary>
        /// The trigger a project analysis.
        /// </summary>
        /// <param name="project">The project.</param>
        public void TriggerAProjectAnalysis(VsProjectItem project)
        {
            // server analysis does not trigger project
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
        ///     The end data association.
        /// </summary>
        public void OnSolutionClosed()
        {
            this.ClearIssues();
            this.IsRunningInVisualStudio = false;
            this.associatedProject = null;
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public void OnDisconnect()
        {
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
                this.notificationMan.WriteMessage("Requested Coverage - Coverage enabled");
                return this.localEditorCache.GetCoverageDataForResource(this.ResourceInEditor, currentSourceBuffer);
            }

            return new Dictionary<int, CoverageElement>();
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
            shownfalseandresolved = false;

            if (this.DocumentInView == null || this.ResourceInEditor == null)
            {
                return new List<Issue>();
            }

            this.IssuesGridView.RefreshStatistics();
            var issuesWithModifiedData = this.localEditorCache.GetIssuesForResource(this.ResourceInEditor, fileContent);
            return issuesWithModifiedData.Where(issue => this.IssuesGridView.IsNotFiltered(issue)).ToList();
        }

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="userConnectionConfig">The user connection config.</param>
        /// <param name="associatedProjectIn">The associated project in.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public void AssociateWithNewProject(
            Resource associatedProjectIn,
            string workingDir,
            ISourceControlProvider provider,
            IIssueTrackerPlugin sourcePlugin,
            IList<Resource> availableProjects,
            Dictionary<string, Profile> profile)
        {
            if (associatedProjectIn == null || string.IsNullOrEmpty(associatedProjectIn.SolutionRoot))
            {
                return;
            }

            this.associatedProject = associatedProjectIn;
            if (this.vsenvironmenthelper != null)
            {
                this.IsRunningInVisualStudio = this.vsenvironmenthelper.AreWeRunningInVisualStudio();
            }
            else
            {
                this.IsRunningInVisualStudio = false;
            }

            List<SonarActionPlan> usortedListofPlan = this.restservice.GetAvailableActionPlan(AuthtenticationHelper.AuthToken, this.associatedProject.Key);
            if (usortedListofPlan != null && usortedListofPlan.Count > 0)
            {
                this.AvailableActionPlans = new ObservableCollection<SonarActionPlan>(usortedListofPlan.OrderBy(i => i.Name));
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
                this.notificationMan.WriteMessage("Trigger issues update");
                this.IssuesReadyForCollecting(this, e);
                if (this.CoverageWasModified != null)
                {
                    this.notificationMan.WriteMessage("Trigger Coverage Update");
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
            this.notificationMan.WriteMessage("CoverageInEditorEnabled Changed");
        }

        /// <summary>
        ///     The on selected view changed.
        /// </summary>
        public void OnSelectedViewChanged()
        {
            this.OnAnalysisModeHasChange(EventArgs.Empty);
            this.notificationMan.WriteMessage("OnSelectedViewChanged Changed");
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
        public void RefreshDataForResource(Resource res, string documentInView, string content)
        {
            this.DocumentInView = documentInView;
            this.ResourceInEditor = res;
            var newCoverage = this.restservice.GetCoverageInResource(AuthtenticationHelper.AuthToken, this.ResourceInEditor.Key);
            var newSource = VsSonarUtils.GetLinesFromSource(this.restservice.GetSourceForFileResource(AuthtenticationHelper.AuthToken, this.ResourceInEditor.Key), "\r\n");
            var newIssues = this.restservice.GetIssuesInResource(AuthtenticationHelper.AuthToken, this.ResourceInEditor.Key);

            this.IssuesGridView.UpdateIssues(newIssues, this.AvailableActionPlans);
            this.localEditorCache.UpdateResourceData(this.ResourceInEditor, newCoverage, newIssues, newSource);
            this.OnSelectedViewChanged();
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

        /// <summary>
        /// The on coverage was modified.
        /// </summary>
        /// <param name="e">The e.</param>
        protected virtual void OnCoverageWasModified(EventArgs e)
        {
            if (this.CoverageWasModified != null)
            {
                this.notificationMan.WriteMessage("Triggering Event: OnCoverageWasModified");
                this.CoverageWasModified(this, e);
            }
        }

        /// <summary>
        /// Called when [show flyouts changed].
        /// </summary>
        public void OnShowFlyoutsChanged()
        {
            this.SizeOfFlyout = this.ShowLeftFlyOut ? 250 : 0;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The init commanding.
        /// </summary>
        private void InitCommanding()
        {
            this.DisplaySourceDiffCommand = new RelayCommand(this.OnDisplaySourceDiffCommand);
            this.CloseLeftFlyoutCommand = new RelayCommand(this.OnCloseFlyoutIssueSearchCommand);
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
                    this.notificationMan.ReportException(ex);
                }
            }
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
        ///     The on close flyout issue search command.
        /// </summary>
        private void OnCloseFlyoutIssueSearchCommand()
        {
            this.ShowLeftFlyOut = false;
            this.IssuesGridView.ShowLeftFlyOut = false;
            this.SizeOfFlyout = 0;
        }

        /// <summary>
        /// The create row context menu.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="translator">The translator.</param>
        /// <returns>
        /// The
        /// <see><cref>ObservableCollection</cref></see>
        /// .
        /// </returns>
        private ObservableCollection<IMenuItem> CreateRowContextMenu(ISonarRestService service, ISQKeyTranslator translator)
        {
            var menu = new ObservableCollection<IMenuItem>
                           {
                               ChangeStatusMenu.MakeMenu(service, this.IssuesGridView, this.notificationMan),
                               OpenResourceMenu.MakeMenu(service, this.IssuesGridView),
                               PlanMenu.MakeMenu(service, this.IssuesGridView, this.notificationMan),
                               SourceControlMenu.MakeMenu(service, this.IssuesGridView, this.notificationMan, translator),
                               IssueTrackerMenu.MakeMenu(service, this.IssuesGridView, this.notificationMan, translator),
                               AssignMenu.MakeMenu(service, this.IssuesGridView, this.notificationMan),
                               SetExclusionsMenu.MakeMenu(service, this.IssuesGridView, this.notificationMan, translator, this.analyser),
                               SetSqaleMenu.MakeMenu(service, this.IssuesGridView, this.notificationMan, translator, this.analyser)
                           };

            return menu;
        }


        #endregion
    }
}