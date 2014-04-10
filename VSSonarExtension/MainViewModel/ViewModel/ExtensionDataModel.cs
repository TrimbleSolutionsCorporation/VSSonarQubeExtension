// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionDataModel.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
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
namespace VSSonarExtension.MainViewModel.ViewModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Windows;

    using ExtensionHelpers;

    using ExtensionTypes;

    using ExtensionViewModel.Commands;

    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using SonarRestService;

    using VSSonarExtension.MainView;
    using VSSonarExtension.MainViewModel.Cache;
    using VSSonarExtension.MainViewModel.Commands;
    using VSSonarExtension.PackageImplementation;

    using VSSonarPlugins;

    /// <summary>
    ///     The issues list.
    /// </summary>
    public partial class ExtensionDataModel : INotifyPropertyChanged
    {
        #region Static Fields

        /// <summary>
        ///     The plugins options data.
        /// </summary>
        public ExtensionOptionsModel ExtensionOptionsData;

        /// <summary>
        ///     The comments width.
        /// </summary>
        private static readonly GridLength CommentsWidthDefault = new GridLength(250);

        /// <summary>
        ///     The user controls height default.
        /// </summary>
        private static readonly GridLength UserControlsHeightDefault = new GridLength(25);

        /// <summary>
        ///     The user texts height default.
        /// </summary>
        private static readonly GridLength UserTextsHeightDefault = new GridLength(50);

        #endregion

        #region Fields

        /// <summary>
        /// The plugin control.
        /// </summary>
        public readonly PluginController PluginControl = new PluginController();

        /// <summary>
        ///     The local editor cache.
        /// </summary>
        private readonly ModelEditorCache localEditorCache = new ModelEditorCache();

        /// <summary>
        ///     The lock this.
        /// </summary>
        private readonly object lockThis = new object();

        /// <summary>
        ///     The project association data model.
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        ///     The association project.
        /// </summary>
        private string associationProject = string.Empty;

        /// <summary>
        ///     The comment data.
        /// </summary>
        private string commentData = string.Empty;

        /// <summary>
        ///     The enable coverage in editor.
        /// </summary>
        private bool coverageInEditorEnabled;

        /// <summary>
        ///     The diagnostic message.
        /// </summary>
        private string diagnosticMessage;

        /// <summary>
        ///     The disable editor tags.
        /// </summary>
        private bool disableEditorTags;

        /// <summary>
        ///     The error message.
        /// </summary>
        private string errorMessage = string.Empty;

        /// <summary>
        ///     The profile.
        /// </summary>
        private Profile profile;

        /// <summary>
        ///     The resource in editor.
        /// </summary>
        private Resource resourceInEditor;

        /// <summary>
        ///     The rest service.
        /// </summary>
        private ISonarRestService restService;

        /// <summary>
        ///     The selected user.
        /// </summary>
        private User selectedUser = new User();

        /// <summary>
        ///     The sonar info.
        /// </summary>
        private string sonarInfo = string.Empty;

        /// <summary>
        ///     The sonar version.
        /// </summary>
        private double sonarVersion = 3.6;

        /// <summary>
        ///     The stats label.
        /// </summary>
        private string statsLabel = string.Empty;

        /// <summary>
        ///     The vsenvironmenthelper.
        /// </summary>
        private IVsEnvironmentHelper vsenvironmenthelper;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionDataModel" /> class.
        ///     This is here only for blend to recognize the data class
        /// </summary>
        public ExtensionDataModel()
        {
            this.restService = new SonarRestService(new JsonSonarConnector());
            this.UserTextControlsHeight = new GridLength(0);
            this.UserControlsHeight = new GridLength(0);

            this.RestoreUserSettingsInIssuesDataGrid();
            this.RestoreUserFilteringOptions();

            this.AnalysisTrigger = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionDataModel"/> class.
        /// </summary>
        /// <param name="restService">
        /// The rest Service.
        /// </param>
        /// <param name="vsenvironmenthelper">
        /// The vsenvironmenthelper.
        /// </param>
        /// <param name="associatedProj">
        /// The associated Proj.
        /// </param>
        public ExtensionDataModel(ISonarRestService restService, IVsEnvironmentHelper vsenvironmenthelper, Resource associatedProj)
        {
            this.RestService = restService;
            this.Vsenvironmenthelper = vsenvironmenthelper;

            // commands
            this.InitCommanding();
            this.UserTextControlsHeight = new GridLength(0);
            this.UserControlsHeight = new GridLength(0);
            this.RestoreUserSettingsInIssuesDataGrid();
            this.RestoreUserFilteringOptions();
            this.AnalysisTrigger = false;

            ConnectionConfiguration conf = this.UserConfiguration;
            if (conf == null)
            {
                return;
            }

            // start some data
            List<User> usortedList = restService.GetUserList(conf);
            if (usortedList != null && usortedList.Count > 0)
            {
                this.UsersList = new List<User>(usortedList.OrderBy(i => i.Login));
            }

            List<Resource> projects = restService.GetProjectsList(conf);
            if (projects != null && projects.Count > 0)
            {
                this.ProjectResources = new List<Resource>(projects.OrderBy(i => i.Name));
            }

            this.AssociatedProject = associatedProj;
            var plugins = this.PluginControl.GetPlugins();
            if (plugins != null)
            {
                this.ExtensionOptionsData = new ExtensionOptionsModel(this.PluginControl, this);
                this.LocalAnalyserModule = new SonarLocalAnalyser.SonarLocalAnalyser(new List<IPlugin>(plugins), this.RestService, this.vsenvironmenthelper);
                this.LocalAnalyserModule.StdOutEvent += this.UpdateOutputMessagesFromPlugin;
                this.LocalAnalyserModule.LocalAnalysisCompleted += this.UpdateLocalIssues;

                this.ExtensionOptionsData.Vsenvironmenthelper = this.Vsenvironmenthelper;
                this.ExtensionOptionsData.ResetUserData();

                foreach (var plugin in plugins)
                {
                    var configuration = ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper);
                    var controloption = plugin.GetPluginControlOptions(configuration);
                    if (controloption == null)
                    {
                        continue;
                    }

                    var pluginKey = plugin.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper));
                    var options = this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(pluginKey);
                    controloption.SetOptions(options);
                }
            }
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the assign on issue command.
        /// </summary>
        public AssignOnIssueCommand AssignOnIssueCommand { get; set; }

        /// <summary>
        ///     Gets or sets the assign project command.
        /// </summary>
        public AssociateCommand AssignProjectCommand { get; set; }

        /// <summary>
        ///     Gets or sets the project association data model.
        /// </summary>
        public Resource AssociatedProject
        {
            get
            {
                return this.associatedProject;
            }

            set
            {
                if (value != null)
                {
                    this.AssociatedProjectKey = value.Key;
                }
                else
                {
                    this.AssociatedProjectKey = string.Empty;
                }

                this.associatedProject = value;
                this.OnPropertyChanged("AssociatedProject");
                this.OnPropertyChanged("IsSolutionOpen");
            }
        }

        /// <summary>
        ///     Gets or sets the association project.
        /// </summary>
        public string AssociatedProjectKey
        {
            get
            {
                return "Selected Project: " + this.associationProject;
            }

            set
            {
                this.associationProject = value;
                this.OnPropertyChanged("AssociatedProjectKey");
            }
        }

        /// <summary>
        ///     Gets or sets the clear cache command.
        /// </summary>
        public ClearCacheCommand ClearCacheCommand { get; set; }

        /// <summary>
        ///     Gets or sets the user entry data.
        /// </summary>
        public string CommentData
        {
            get
            {
                return this.commentData;
            }

            set
            {
                this.commentData = value;
                this.OnPropertyChanged("CommentData");
            }
        }

        /// <summary>
        ///     Gets or sets the comment on issue.
        /// </summary>
        public CommentOnIssueCommand CommentOnIssueCommand { get; set; }

        /// <summary>
        ///     Gets or sets the comments.
        /// </summary>
        public List<Comment> Comments
        {
            get
            {
                return this.comments;
            }

            set
            {
                this.comments = value;
                if (value != null && value.Count > 0)
                {
                    this.CommentsWidth = CommentsWidthDefault;
                }
                else
                {
                    this.CommentsWidth = new GridLength(0);
                }

                this.OnPropertyChanged("Comments");
            }
        }

        /// <summary>
        ///     Gets or sets the comments width.
        /// </summary>
        public GridLength CommentsWidth
        {
            get
            {
                return this.commentsWidth;
            }

            set
            {
                this.commentsWidth = value;
                this.OnPropertyChanged("CommentsWidth");
            }
        }

        /// <summary>
        ///     Gets or sets the confirm issue command.
        /// </summary>
        public ConfirmIssueCommand ConfirmIssueCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether enable coverage in editor.
        /// </summary>
        public bool CoverageInEditorEnabled
        {
            get
            {
                return this.coverageInEditorEnabled;
            }

            set
            {
                this.coverageInEditorEnabled = value;
                this.RefreshDataForResource(this.DocumentInView);
            }
        }

        /// <summary>
        ///     Gets the custom pane.
        /// </summary>
        public IVsOutputWindowPane CustomPane { get; set; }

        /// <summary>
        ///     Gets or sets the diagnostic message.
        /// </summary>
        public string DiagnosticMessage
        {
            get
            {
                return this.diagnosticMessage;
            }

            set
            {
                this.diagnosticMessage = value;
                this.OnPropertyChanged("DiagnosticMessage");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether issues in view locked.
        /// </summary>
        public bool DisableEditorTags
        {
            get
            {
                return this.disableEditorTags;
            }

            set
            {
                this.disableEditorTags = value;
                this.Vsenvironmenthelper.WriteOptionInApplicationData("SonarOptionsGeneral", "DisableEditorTags", value ? "TRUE" : "FALSE");
                this.OnPropertyChanged("IssuesInEditor");
                this.OnPropertyChanged("DisableEditorTags");
            }
        }

        /// <summary>
        ///     Gets or sets the document in view.
        /// </summary>
        public string DocumentInView { get; set; }

        /// <summary>
        ///     Gets or sets the error message.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }

            set
            {
                this.errorMessage = value;
                this.OnPropertyChanged("ErrorMessage");
            }
        }

        /// <summary>
        ///     Gets or sets the false positive on issue command.
        /// </summary>
        public FalsePositiveOnIssueCommand FalsePositiveOnIssueCommand { get; set; }

        /// <summary>
        ///     Gets or sets the issues in project command.
        /// </summary>
        public GetIssuesCommand GetIssuesCommand { get; set; }

        /// <summary>
        ///     Gets or sets the is assign enabled.
        /// </summary>
        public Visibility IsAssignVisible
        {
            get
            {
                return this.assignVisible;
            }

            set
            {
                this.assignVisible = value;
                this.OnPropertyChanged("IsAssignVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the is commenting enabled.
        /// </summary>
        public Visibility IsCommentingVisible
        {
            get
            {
                return this.commentingVisible;
            }

            set
            {
                this.commentingVisible = value;
                this.OnPropertyChanged("IsCommentingVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the is confirm enable.
        /// </summary>
        public Visibility IsConfirmVisible
        {
            get
            {
                return this.confirmVisible;
            }

            set
            {
                this.confirmVisible = value;
                this.OnPropertyChanged("IsConfirmVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the is false positive enabled.
        /// </summary>
        public Visibility IsFalsePositiveVisible
        {
            get
            {
                return this.falsePositiveVisible;
            }

            set
            {
                this.falsePositiveVisible = value;
                this.OnPropertyChanged("IsFalsePositiveVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the open externally.
        /// </summary>
        public Visibility IsOpenExternallyVisible
        {
            get
            {
                return this.openExternallyVisible;
            }

            set
            {
                this.openExternallyVisible = value;
                this.OnPropertyChanged("IsOpenExternallyVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the is reopen enabled.
        /// </summary>
        public Visibility IsReopenVisible
        {
            get
            {
                return this.reopenVisible;
            }

            set
            {
                this.reopenVisible = value;
                this.OnPropertyChanged("IsReopenVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the is resolve enabled.
        /// </summary>
        public Visibility IsResolveVisible
        {
            get
            {
                return this.resolveVisible;
            }

            set
            {
                this.resolveVisible = value;
                this.OnPropertyChanged("IsResolveVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the is un confirm enable.
        /// </summary>
        public Visibility IsUnconfirmVisible
        {
            get
            {
                return this.unconfirmVisible;
            }

            set
            {
                this.unconfirmVisible = value;
                this.OnPropertyChanged("IsUnconfirmVisible");
            }
        }

        /// <summary>
        ///     Gets the issues.
        /// </summary>
        public List<Issue> Issues
        {
            get
            {
                if (this.IssuesInViewAreLocked)
                {
                    List<Issue> issues = this.ApplyFilterToIssues(this.localEditorCache.GetIssues(), this.AnalysisChangeLines);
                    this.StatsLabel = "Number of Issues: " + issues.Count + " ";
                    return issues;
                }

                if (this.analysisModeText.Equals(AnalysisModes.Server)
                    || (this.analysisModeText.Equals(AnalysisModes.Local) && this.analysisTypeText.Equals(AnalysisTypes.File)))
                {
                    if (!this.AnalysisTrigger)
                    {
                        this.StatsLabel = "Number of Issues: 0 ";
                        return new List<Issue>();
                    }

                    List<Issue> issues = this.ApplyFilterToIssues(
                        this.localEditorCache.GetIssuesForResource(this.ResourceInEditor), 
                        this.AnalysisChangeLines);
                    this.StatsLabel = "Number of Issues: " + issues.Count + " ";
                    return issues;
                }

                if (this.analysisModeText.Equals(AnalysisModes.Local))
                {
                    List<Issue> issues = this.ApplyFilterToIssues(this.localEditorCache.GetIssues(), this.AnalysisChangeLines);
                    this.StatsLabel = "Number of Issues: " + issues.Count + " ";
                    return issues;
                }

                this.StatsLabel = "Number of Issues: 0 ";
                return new List<Issue>();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether issues in view are locked.
        /// </summary>
        public bool IssuesInViewAreLocked { get; set; }

        /// <summary>
        ///     Gets or sets the open in sonar command.
        /// </summary>
        public OpenInSonarCommand OpenInSonarCommand { get; set; }

        /// <summary>
        ///     Gets or sets the open in vs command.
        /// </summary>
        public OpenInVsCommand OpenInVsCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether prevent update of issues list.
        /// </summary>
        public bool PreventUpdateOfIssuesList { get; set; }

        /// <summary>
        ///     Gets the profile.
        /// </summary>
        public Profile Profile
        {
            get
            {
                if (this.profile == null)
                {
                    try
                    {
                        List<Resource> profileResource = this.restService.GetQualityProfile(this.UserConfiguration, this.AssociatedProject.Key);
                        List<Profile> enabledrules = this.restService.GetEnabledRulesInProfile(
                            this.UserConfiguration, 
                            profileResource[0].Lang, 
                            profileResource[0].Metrics[0].Data);
                        this.profile = enabledrules[0];
                    }
                    catch (Exception ex)
                    {
                        this.ErrorMessage = "Cannot retrieve Profile From Server";
                        this.DiagnosticMessage = ex.Message + " " + ex.StackTrace;
                    }
                }

                return this.profile;
            }
        }

        /// <summary>
        ///     Gets or sets the re open issue command.
        /// </summary>
        public ReOpenIssueCommand ReOpenIssueCommand { get; set; }

        /// <summary>
        ///     Gets or sets the false positive on issue command.
        /// </summary>
        public ResolveIssueCommand ResolveIssueCommand { get; set; }

        /// <summary>
        ///     Gets or sets the resource in editor.
        /// </summary>
        public Resource ResourceInEditor
        {
            get
            {
                return this.resourceInEditor;
            }

            set
            {
                this.resourceInEditor = value;
                this.OnPropertyChanged("ResourceInEditor");
            }
        }

        /// <summary>
        ///     Gets or sets the rest service.
        /// </summary>
        public ISonarRestService RestService
        {
            get
            {
                return this.restService;
            }

            set
            {
                this.restService = value;
            }
        }

        /// <summary>
        ///     Gets or sets the selected issue.
        /// </summary>
        public Issue SelectedIssue
        {
            get
            {
                return this.selectedIssue;
            }

            set
            {
                this.selectedIssue = value;
                this.OnPropertyChanged("SelectedIssue");
            }
        }

        /// <summary>
        ///     Gets or sets the issues.
        /// </summary>
        public IList SelectedIssuesInView
        {
            get
            {
                return this.selectedIssuesInView;
            }

            set
            {
                this.selectedIssuesInView = value;

                this.ResetVisibilities();

                if (value.Count > 0)
                {
                    this.UserInputTextBoxVisibility = Visibility.Visible;
                    this.IsCommentingVisible = Visibility.Visible;
                    this.IsOpenExternallyVisible = Visibility.Visible;

                    if (value.Count == 1)
                    {
                        var currentIssue = value[0] as Issue;
                        if (currentIssue != null)
                        {
                            this.Comments = currentIssue.Comments;
                        }
                    }
                    else
                    {
                        this.Comments = null;
                    }

                    this.SetWorkflowVisibility();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the sonar info.
        /// </summary>
        public string SonarInfo
        {
            get
            {
                return this.sonarInfo;
            }

            set
            {
                this.sonarInfo = value;
                this.OnPropertyChanged("SonarInfo");
            }
        }

        /// <summary>
        ///     Gets or sets the sonar version.
        /// </summary>
        public double SonarVersion
        {
            get
            {
                return this.sonarVersion;
            }

            set
            {
                this.sonarVersion = value;
            }
        }

        /// <summary>
        ///     Gets or sets the stats label.
        /// </summary>
        public string StatsLabel
        {
            get
            {
                return this.statsLabel;
            }

            set
            {
                this.statsLabel = value;
                this.OnPropertyChanged("StatsLabel");
            }
        }

        /// <summary>
        ///     Gets or sets the un confirm issue command.
        /// </summary>
        public UnConfirmIssueCommand UnConfirmIssueCommand { get; set; }

        /// <summary>
        ///     Gets a value indicating whether update tags in editor.
        /// </summary>
        public bool UpdateTagsInEditor
        {
            get
            {
                if (this.DisableEditorTags)
                {
                    return false;
                }

                if (this.analysisModeText.Equals(AnalysisModes.Local) && !this.analysisTypeText.Equals(AnalysisTypes.File))
                {
                    return true;
                }

                if (this.IssuesInViewAreLocked)
                {
                    return true;
                }

                return this.analysisTypeText.Equals(AnalysisTypes.File) && this.AnalysisTrigger;
            }
        }

        /// <summary>
        ///     Gets the user configuration.
        /// </summary>
        public ConnectionConfiguration UserConfiguration
        {
            get
            {
                ConnectionConfiguration conf = ConnectionConfigurationHelpers.GetConnectionConfiguration(this.vsenvironmenthelper, this.restService);
                if (conf == null)
                {
                    this.SonarInfo = "Not Logged";
                    this.ErrorMessage = ConnectionConfigurationHelpers.ErrorMessage;
                    return null;
                }

                try
                {
                    this.SonarVersion = this.restService.GetServerInfo(conf);
                }
                catch (Exception ex)
                {
                    this.ErrorMessage = "Cannot Connect To Server, Check Options";
                    this.ErrorMessage = ex.Message + "\r\n" + ex.StackTrace;
                    return null;
                }

                this.SonarInfo = "Logged to Sonar: " + Convert.ToString(this.SonarVersion) + " " + conf.Hostname + " User: " + conf.Username;
                this.ErrorMessage = string.Empty;
                if (this.AssociatedProject != null)
                {
                    this.AssociatedProjectKey = this.AssociatedProject.Key;
                }

                return conf;
            }
        }

        /// <summary>
        ///     Gets or sets the user controls height.
        /// </summary>
        public GridLength UserControlsHeight
        {
            get
            {
                return this.userControlsHeight;
            }

            set
            {
                this.userControlsHeight = value;
                this.OnPropertyChanged("UserControlsHeight");
            }
        }

        /// <summary>
        ///     Gets or sets the user input text box visibility.
        /// </summary>
        public Visibility UserInputTextBoxVisibility
        {
            get
            {
                return this.userInputTextBoxVisibility;
            }

            set
            {
                this.userInputTextBoxVisibility = value;
                this.OnPropertyChanged("UserInputTextBoxVisibility");
            }
        }

        /// <summary>
        ///     Gets or sets the user controls height.
        /// </summary>
        public GridLength UserTextControlsHeight
        {
            get
            {
                return this.userTextControlsHeight;
            }

            set
            {
                this.userTextControlsHeight = value;
                this.OnPropertyChanged("UserTextControlsHeight");
            }
        }

        /// <summary>
        ///     Gets or sets the vs package.
        /// </summary>
        public VsSonarExtensionPackage VSPackage { get; set; }

        /// <summary>
        ///     Gets or sets the vsenvironmenthelper.
        /// </summary>
        public IVsEnvironmentHelper Vsenvironmenthelper
        {
            get
            {
                return this.vsenvironmenthelper;
            }

            set
            {
                this.vsenvironmenthelper = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get issues for resource from list.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static List<Issue> GetIssuesForResourceFromList(Resource resource, IEnumerable<Issue> list)
        {
            return list.Where(issue => issue.Component.Equals(resource.Key)).ToList();
        }

        /// <summary>
        /// The apply filter to issues.
        /// </summary>
        /// <param name="issuesToFilter">
        /// The issues to filter.
        /// </param>
        /// <param name="changeLines">
        /// The change lines.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public List<Issue> ApplyFilterToIssues(List<Issue> issuesToFilter, bool changeLines)
        {
            if (issuesToFilter == null)
            {
                return new List<Issue>();
            }

            return issuesToFilter.Where(issue => !this.ApplyFilter(issue, changeLines)).ToList();
        }

        /// <summary>
        ///     The associate project to solution.
        /// </summary>
        public void AssociateProjectToSolution()
        {
            var newProject = this.AssociateSolutionWithSonarProject();
            if (newProject == null)
            {
                return;
            }

            this.ExtensionOptionsData.Project = newProject;
            this.ExtensionOptionsData.RefreshGeneralProperties();
            this.ExtensionOptionsData.RefreshPropertiesInPlugins();
            this.ExtensionOptionsData.SyncOptionsToFile();
            this.AssociatedProject = newProject;
            this.OnPropertyChanged("IsSolutionOpen");
        }

        /// <summary>
        ///     The validate project key.
        /// </summary>
        /// <returns>
        ///     The System.String.
        /// </returns>
        public Resource AssociateSolutionWithSonarProject()
        {
            IVsEnvironmentHelper vsinter = this.Vsenvironmenthelper;
            ConnectionConfiguration conf = this.UserConfiguration;
            string solutionName = vsinter.ActiveSolutionName();

            if (conf == null)
            {
                this.ErrorMessage = "User Not Logged In, Extension is Unusable. Configure User Settions in Tools > Options > Sonar";
                return null;
            }

            string sourceKey = vsinter.ReadOptionFromApplicationData(solutionName, "PROJECTKEY");
            if (!string.IsNullOrEmpty(sourceKey))
            {
                try
                {
                    return this.RestService.GetResourcesData(conf, sourceKey)[0];
                }
                catch (Exception ex)
                {
                    UserExceptionMessageBox.ShowException("Associated Project does not exist in server, please configure association", ex);
                    return null;
                }                
            }

            string solutionPath = vsinter.ActiveSolutionPath();
            sourceKey = VsSonarUtils.GetProjectKey(solutionPath);
            vsinter.WriteOptionInApplicationData(solutionName, "PROJECTKEY", sourceKey);

            return string.IsNullOrEmpty(sourceKey) ? null : this.RestService.GetResourcesData(conf, sourceKey)[0];
        }

        /// <summary>
        ///     The clear coverage in editor.
        /// </summary>
        public void ClearCoverageInEditor()
        {
            this.localEditorCache.ClearData();
        }

        /// <summary>
        ///     The clear project association.
        /// </summary>
        public void ClearProjectAssociation()
        {
            this.AssociatedProject = null;
            this.ClearCaches();
            this.OnPropertyChanged("IsSolutionOpen");
        }

        /// <summary>
        ///     The display diference to server source.
        /// </summary>
        public void DisplayDiferenceToServerSource()
        {
            if (this.ResourceInEditor != null)
            {
                VsSonarUtils.GetDifferenceReport(this.DocumentInView, this.localEditorCache.GetSourceForResource(this.ResourceInEditor), true);
            }
        }

        /// <summary>
        /// The extension data model update.
        /// </summary>
        /// <param name="restServiceIn">
        /// The rest service in.
        /// </param>
        /// <param name="vsenvironmenthelperIn">
        /// The vsenvironmenthelper in.
        /// </param>
        /// <param name="associatedProj">
        /// The associated proj.
        /// </param>
        public void ExtensionDataModelUpdate(ISonarRestService restServiceIn, IVsEnvironmentHelper vsenvironmenthelperIn, Resource associatedProj)
        {
            this.RestService = restServiceIn;
            this.Vsenvironmenthelper = vsenvironmenthelperIn;
            var plugins = this.PluginControl.GetPlugins();
            if (plugins != null)
            {
                this.ExtensionOptionsData = new ExtensionOptionsModel(this.PluginControl, this);
                this.LocalAnalyserModule = new SonarLocalAnalyser.SonarLocalAnalyser(new List<IPlugin>(plugins), this.RestService, this.Vsenvironmenthelper);
                this.LocalAnalyserModule.StdOutEvent += this.UpdateOutputMessagesFromPlugin;
                this.LocalAnalyserModule.LocalAnalysisCompleted += this.UpdateLocalIssues;

                this.ExtensionOptionsData.Vsenvironmenthelper = this.Vsenvironmenthelper;
                this.ExtensionOptionsData.ResetUserData();

                foreach (var plugin in plugins)
                {
                    var configuration = ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper);
                    var controloption = plugin.GetPluginControlOptions(configuration);
                    if (controloption == null)
                    {
                        continue;
                    }

                    var pluginKey = plugin.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper));
                    var options = this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(pluginKey);
                    controloption.SetOptions(options);
                }
            }

            this.InitCommanding();
            this.UserTextControlsHeight = new GridLength(0);
            this.UserControlsHeight = new GridLength(0);
            this.CommentsWidth = new GridLength(0);
            this.RestoreUserSettingsInIssuesDataGrid();
            this.RestoreUserFilteringOptions();
            this.localEditorCache.ClearData();

            ConnectionConfiguration conf = this.UserConfiguration;
            if (conf == null)
            {
                this.AssociatedProject = null;
                return;
            }

            // start some data
            List<User> usortedList = restServiceIn.GetUserList(conf);
            if (usortedList != null)
            {
                this.UsersList = new List<User>(usortedList.OrderBy(i => i.Login)) { new User() };
            }

            List<Resource> projects = restServiceIn.GetProjectsList(conf);
            if (projects != null)
            {
                this.ProjectResources = new List<Resource>(projects.OrderBy(i => i.Name));
            }

            this.AssociatedProject = associatedProj;
        }

        /// <summary>
        /// Gets the coverage in editor.
        /// </summary>
        /// <param name="currentSourceBuffer">
        /// The current Source Buffer.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public Dictionary<int, CoverageElement> GetCoverageInEditor(string currentSourceBuffer)
        {
            if (this.CoverageInEditorEnabled)
            {
                return this.localEditorCache.GetCoverageDataForResource(this.resourceInEditor, currentSourceBuffer);
            }

            return new Dictionary<int, CoverageElement>();
        }

        /// <summary>
        /// Gets the issues in editor.
        /// </summary>
        /// <param name="sourceReference">
        /// The source Reference.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .I
        /// </returns>
        public List<Issue> GetIssuesInEditor(string sourceReference)
        {
            if (this.DocumentInView == null || this.ResourceInEditor == null)
            {
                return new List<Issue>();
            }

            if (this.analysisModeText.Equals(AnalysisModes.Local))
            {
                return this.ApplyFilterToIssues(this.localEditorCache.GetIssuesForResource(this.ResourceInEditor), this.AnalysisChangeLines);
            }

            return this.ApplyFilterToIssues(
                this.localEditorCache.GetIssuesForResource(this.ResourceInEditor, sourceReference), 
                this.AnalysisChangeLines);
        }

        /// <summary>
        /// The perform solution not open association.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        public void PerformSolutionNotOpenAssociation(Resource resource)
        {
            if (resource != null)
            {
                this.AssociatedProjectKey = resource.Key;
            }
            else
            {
                this.AssociatedProjectKey = string.Empty;
            }

            this.associatedProject = resource;
            this.OnPropertyChanged("AssociatedProject");
        }

        /// <summary>
        /// The refresh data for resource.
        /// </summary>
        /// <param name="fullName">
        /// The full name.
        /// </param>
        public void RefreshDataForResource(string fullName)
        {
            if (string.IsNullOrEmpty(fullName) || this.AssociatedProject == null)
            {
                this.ErrorMessage = "Extension Not Ready";
                return;
            }

            this.DocumentInView = fullName;
            try
            {
                var resourceKey =
                    this.LocalAnalyserModule.GetResourceKey(
                        this.vsenvironmenthelper.VsProjectItem(fullName),
                        this.AssociatedProject,
                        this.UserConfiguration);

                this.ResourceInEditor = this.restService.GetResourcesData(this.UserConfiguration, resourceKey)[0];

                if (this.analysisModeText.Equals(AnalysisModes.Server))
                {
                    this.IssuesInViewAreLocked = false;
                    this.UpdateDataFromServer(this.ResourceInEditor);
                }

                if (this.analysisModeText.Equals(AnalysisModes.Local)
                    && this.analysisTypeText.Equals(AnalysisTypes.File))
                {
                    this.PerformfAnalysis(this.analysisTrigger);
                }

                this.TriggerUpdateSignals();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "Resource in view not supported";
                this.DiagnosticMessage = ex.StackTrace;
            }
        }

        /// <summary>
        ///     The refresh issues.
        /// </summary>
        public void RefreshIssuesInViews()
        {
            this.OnPropertyChanged("Issues");
            this.OnPropertyChanged("IssuesInEditor");
        }

        /// <summary>
        /// The replace all issues in cache.
        /// </summary>
        /// <param name="getAllIssuesByAssignee">
        /// The get all issues by assignee.
        /// </param>
        public void ReplaceAllIssuesInCache(List<Issue> getAllIssuesByAssignee)
        {
            this.localEditorCache.UpdateIssues(getAllIssuesByAssignee);
            this.analysisTrigger = false;
            this.IssuesInViewAreLocked = true;
            this.RefreshIssuesInViews();
            this.OnPropertyChanged("AnalysisTriggerText");
            this.OnPropertyChanged("AnalysisTrigger");
        }

        /// <summary>
        ///     The retrieve issues using current filter.
        /// </summary>
        public void RetrieveIssuesUsingCurrentFilter()
        {
            this.SaveFilterToDisk();

            if (this.sonarVersion < 3.6)
            {
                if (this.DocumentInView != null)
                {
                    this.RefreshDataForResource(this.DocumentInView);
                }
                else
                {
                    this.ReplaceAllIssuesInCache(this.RestService.GetIssuesForProjects(this.UserConfiguration, this.AssociatedProject.Key));
                }

                return;
            }

            string request = "?componentRoots=" + this.AssociatedProject.Key;

            if (this.IsAssigneeChecked)
            {
                request += "&assignees=" + this.AssigneeInFilter.Login;
            }

            if (this.IsReporterChecked)
            {
                request += "&reporters=" + this.ReporterInFilter.Login;
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

            this.ReplaceAllIssuesInCache(this.RestService.GetIssues(this.UserConfiguration, request, this.AssociatedProject.Key));
        }

        /// <summary>
        /// The select a issue from list.
        /// </summary>
        /// <param name="idIn">
        /// The id.
        /// </param>
        public void SelectAIssueFromList(int idIn)
        {
            foreach (Issue issue in this.Issues.Where(issue => issue.Id == idIn))
            {
                this.SelectedIssue = issue;
                return;
            }
        }

        /// <summary>
        /// The select a issue from list.
        /// </summary>
        /// <param name="keyIn">
        /// The key.
        /// </param>
        public void SelectAIssueFromList(Guid keyIn)
        {
            foreach (Issue issue in this.Issues.Where(issue => issue.Key == keyIn))
            {
                this.SelectedIssue = issue;
                return;
            }
        }

        /// <summary>
        ///     The extension data model update.
        /// </summary>
        public void StartupModelData()
        {
            this.InitCommanding();

            try
            {
                this.UserTextControlsHeight = new GridLength(0);
                this.UserControlsHeight = new GridLength(0);
                this.CommentsWidth = new GridLength(0);
                this.RestoreUserSettingsInIssuesDataGrid();
                this.RestoreUserFilteringOptions();
                this.localEditorCache.ClearData();

                ConnectionConfiguration configuration = this.UserConfiguration;
                if (configuration == null)
                {
                    this.AssociatedProject = null;
                    this.UsersList = null;
                    this.ProjectResources = null;
                    this.ErrorMessage = "Extension is not in usable condition, Check User Configuration. Host, UserName and Password needs to be set";
                    return;
                }

                // start some data
                var usortedList = this.RestService.GetUserList(configuration);
                if (usortedList != null)
                {
                    this.UsersList = new List<User>(usortedList.OrderBy(i => i.Login)) { new User() };
                }

                var projects = this.RestService.GetProjectsList(configuration);
                if (projects != null)
                {
                    this.ProjectResources = new List<Resource>(projects.OrderBy(i => i.Name));
                }

                this.AssociateProjectToSolution();
            }
            catch (Exception ex)
            {
                this.AssociatedProject = null;
                this.UsersList = null;
                this.ProjectResources = null;
                UserExceptionMessageBox.ShowException("Extension is not in usable condition, Check User Configuration. Host, UserName and Password needs to be set", ex);
            }
        }

        /// <summary>
        /// The update issues in editor.
        /// </summary>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        public void UpdateIssuesInEditorLocationWithModifiedBuffer(string buffer)
        {
            lock (this.lockThis)
            {
                if (this.Issues == null)
                {
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// The apply filter.
        /// </summary>
        /// <param name="issue">
        /// The issue.
        /// </param>
        /// <param name="newIssuesOnly">
        /// The new Issues Only.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ApplyFilter(Issue issue, bool newIssuesOnly)
        {
            if (this.IsAssigneeChecked && !this.AssigneeInFilter.Login.Equals(issue.Assignee))
            {
                return true;
            }

            if (this.FilterByResolution(issue))
            {
                return true;
            }

            if (this.FilterByStatus(issue))
            {
                return true;
            }

            if (this.FilterBySeverity(issue))
            {
                return true;
            }

            if (newIssuesOnly)
            {
                if (!issue.IsNew)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The filter by resolution.
        /// </summary>
        /// <param name="issue">
        /// The issue.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool FilterByResolution(Issue issue)
        {
            if (string.IsNullOrEmpty(issue.Resolution))
            {
                return false;
            }

            if (issue.Resolution.Equals("FIXED"))
            {
                if (this.IsFixedChecked)
                {
                    return false;
                }
            }

            if (issue.Resolution.Equals("REMOVED"))
            {
                if (this.IsRemovedChecked)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The filter by severity.
        /// </summary>
        /// <param name="issue">
        /// The issue.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool FilterBySeverity(Issue issue)
        {
            if (string.IsNullOrEmpty(issue.Severity))
            {
                return false;
            }

            if (issue.Severity.EndsWith("INFO", true, CultureInfo.InvariantCulture))
            {
                if (this.IsInfoChecked)
                {
                    return false;
                }
            }

            if (issue.Severity.EndsWith("MINOR", true, CultureInfo.InvariantCulture))
            {
                if (this.IsMinorChecked)
                {
                    return false;
                }
            }

            if (issue.Severity.EndsWith("MAJOR", true, CultureInfo.InvariantCulture))
            {
                if (this.IsMajaorChecked)
                {
                    return false;
                }
            }

            if (issue.Severity.EndsWith("CRITICAL", true, CultureInfo.InvariantCulture))
            {
                if (this.IsCriticalChecked)
                {
                    return false;
                }
            }

            if (issue.Severity.EndsWith("BLOCKER", true, CultureInfo.InvariantCulture))
            {
                if (this.IsBlockerChecked)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The filter by status.
        /// </summary>
        /// <param name="issue">
        /// The issue.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool FilterByStatus(Issue issue)
        {
            if (string.IsNullOrEmpty(issue.Status))
            {
                return false;
            }

            if (issue.Status.Equals("CLOSED"))
            {
                if (this.IsStatusClosedChecked)
                {
                    return false;
                }
            }

            if (issue.Status.Equals("OPEN"))
            {
                if (this.IsStatusOpenChecked)
                {
                    return false;
                }
            }

            if (issue.Status.Equals("CONFIRMED"))
            {
                if (this.IsStatusConfirmedChecked)
                {
                    return false;
                }
            }

            if (issue.Status.Equals("REOPENED"))
            {
                if (this.IsStatusReopenedChecked)
                {
                    return false;
                }
            }

            if (issue.Status.Equals("RESOLVED"))
            {
                if (this.IsStatusResolvedChecked)
                {
                    return false;
                }
            }

            return true;
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
            if (this.CommentOnIssueCommand != null)
            {
                return;
            }

            // commands
            this.CommentOnIssueCommand = new CommentOnIssueCommand(this, this.restService);
            this.FalsePositiveOnIssueCommand = new FalsePositiveOnIssueCommand(this, this.restService);
            this.ResolveIssueCommand = new ResolveIssueCommand(this, this.restService);
            this.AssignOnIssueCommand = new AssignOnIssueCommand(this, this.restService);
            this.OpenInSonarCommand = new OpenInSonarCommand(this, this.restService, this.vsenvironmenthelper);
            this.OpenInVsCommand = new OpenInVsCommand(this, this.restService, this.vsenvironmenthelper);
            this.ConfirmIssueCommand = new ConfirmIssueCommand(this, this.restService);
            this.ReOpenIssueCommand = new ReOpenIssueCommand(this, this.restService);
            this.UnConfirmIssueCommand = new UnConfirmIssueCommand(this, this.restService);
            this.GetIssuesCommand = new GetIssuesCommand(this, this.restService);
            this.AssignProjectCommand = new AssociateCommand(this, this.vsenvironmenthelper);
            this.ClearCacheCommand = new ClearCacheCommand(this);
            this.ToolSwitchCommand = new ViewSwitchCommand(this);
        }

        /// <summary>
        ///     The reset visibilities.
        /// </summary>
        private void ResetVisibilities()
        {
            this.UserInputTextBoxVisibility = Visibility.Hidden;
            this.IsAssignVisible = Visibility.Hidden;
            this.IsConfirmVisible = Visibility.Hidden;
            this.IsUnconfirmVisible = Visibility.Hidden;

            this.IsCommentingVisible = Visibility.Hidden;
            this.IsFalsePositiveVisible = Visibility.Hidden;
            this.IsResolveVisible = Visibility.Hidden;
            this.IsReopenVisible = Visibility.Hidden;
            this.IsOpenExternallyVisible = Visibility.Hidden;

            this.UserTextControlsHeight = new GridLength(0);
            this.UserControlsHeight = new GridLength(0);
            this.CommentsWidth = new GridLength(0);
        }

        /// <summary>
        ///     The set multi selection visibility.
        /// </summary>
        private void SetWorkflowVisibility()
        {
            if (this.selectedIssuesInView.OfType<Issue>().Any(issueIn => issueIn.Status == null))
            {
                this.ResetVisibilities();
                return;
            }

            if (this.SonarVersion < 3.6)
            {
                this.VerifyPre36WorkFlow();
            }
            else
            {
                this.VerifyWorkFlow36();
            }
        }

        /// <summary>
        ///     The trigger update signals.
        /// </summary>
        private void TriggerUpdateSignals()
        {
            if (!this.analysisModeText.Equals(AnalysisModes.Local) && !this.IssuesInViewAreLocked)
            {
                this.OnPropertyChanged("Issues");
            }

            this.OnPropertyChanged("CoverageInEditor");
            this.OnPropertyChanged("IssuesInEditor");
        }

        /// <summary>
        /// The update data from server.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        private void UpdateDataFromServer(Resource resource)
        {
            if (this.localEditorCache.IsDataUpdated(resource))
            {
                return;
            }

            SourceCoverage newCoverage = this.restService.GetCoverageInResource(this.UserConfiguration, this.resourceInEditor.Key);
            string newSource =
                VsSonarUtils.GetLinesFromSource(this.restService.GetSourceForFileResource(this.UserConfiguration, this.resourceInEditor.Key), "\r\n");
            List<Issue> newIssues = this.restService.GetIssuesInResource(this.UserConfiguration, this.resourceInEditor.Key);
            this.localEditorCache.UpdateResourceData(this.ResourceInEditor, newCoverage, newIssues, newSource);
        }

        /// <summary>
        ///     The verify pre 36 work flow.
        /// </summary>
        private void VerifyPre36WorkFlow()
        {
            this.IsUnconfirmVisible = Visibility.Hidden;
            this.IsResolveVisible = Visibility.Hidden;
            this.IsFalsePositiveVisible = Visibility.Hidden;
            this.IsAssignVisible = Visibility.Hidden;

            this.UserTextControlsHeight = UserTextsHeightDefault;
            this.UserControlsHeight = UserControlsHeightDefault;
            this.CommentsWidth = CommentsWidthDefault;

            foreach (object issue in this.selectedIssuesInView)
            {
                var issueIn = issue as Issue;
                if (issueIn == null || string.IsNullOrEmpty(issueIn.Status))
                {
                    continue;
                }

                if (issueIn.Status.Equals("OPEN") || issueIn.Status.Equals("REOPENED"))
                {
                    this.IsResolveVisible = Visibility.Visible;
                    this.IsFalsePositiveVisible = Visibility.Visible;
                }

                if (issueIn.Status.Equals("RESOLVED"))
                {
                    this.IsReopenVisible = Visibility.Visible;
                }
            }
        }

        /// <summary>
        ///     The verify work flow 36.
        /// </summary>
        private void VerifyWorkFlow36()
        {
            foreach (object issue in this.selectedIssuesInView)
            {
                var issueIn = issue as Issue;
                if (issueIn == null)
                {
                    continue;
                }

                this.UserTextControlsHeight = UserTextsHeightDefault;
                this.UserControlsHeight = UserControlsHeightDefault;

                if (issueIn.Status.Equals("CONFIRMED"))
                {
                    this.IsUnconfirmVisible = Visibility.Visible;
                    this.IsResolveVisible = Visibility.Visible;
                    this.IsFalsePositiveVisible = Visibility.Visible;
                    this.IsAssignVisible = Visibility.Visible;
                }

                if (issueIn.Status.Equals("OPEN") || issueIn.Status.Equals("REOPENED"))
                {
                    this.IsConfirmVisible = Visibility.Visible;
                    this.IsAssignVisible = Visibility.Visible;
                    this.IsResolveVisible = Visibility.Visible;
                }

                if (issueIn.Status.Equals("RESOLVED"))
                {
                    this.IsReopenVisible = Visibility.Visible;
                }
            }
        }

        #endregion
    }
}