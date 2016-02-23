namespace VSSonarExtensionUi.Model.Menu
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Windows.Input;

    using Helpers;

    using ViewModel.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using GalaSoft.MvvmLight.Command;
    using Association;
    using SonarLocalAnalyser;
    using View.Helpers;
    using System.Linq;

    /// <summary>
    /// Source Control Related Actions
    /// </summary>
    public class SetSqaleMenu : IMenuItem
    {
        /// <summary>
        /// The rest service
        /// </summary>
        private readonly ISonarRestService rest;

        /// <summary>
        /// The model service
        /// </summary>
        private readonly IssueGridViewModel model;

        /// <summary>
        /// The manager
        /// </summary>
        private readonly INotificationManager manager;

        /// <summary>
        /// The tranlator
        /// </summary>
        private readonly ISQKeyTranslator tranlator;

        /// <summary>
        /// The analyser
        /// </summary>
        private readonly ISonarLocalAnalyser analyser;

        /// <summary>
        /// The source control
        /// </summary>
        private ISourceControlProvider sourceControl;

        /// <summary>
        /// The vshelper
        /// </summary>
        private IVsEnvironmentHelper vshelper;

        /// <summary>
        /// The assign project
        /// </summary>
        private Resource assignProject;

        /// <summary>
        /// The available projects
        /// </summary>
        private IList<Resource> availableProjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceControlMenu" /> class.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="translator">The translator.</param>
        public SetSqaleMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager manager, ISQKeyTranslator translator, ISonarLocalAnalyser analyser)
        {
            this.analyser = analyser;
            this.rest = rest;
            this.model = model;
            this.manager = manager;
            this.tranlator = translator;

            this.ExecuteCommand = new RelayCommand(this.OnSetExclusionsMenuCommand);
            this.SubItems = new ObservableCollection<IMenuItem>();

            // register menu for data sync
            AssociationModel.RegisterNewModelInPool(this);
        }

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// Gets or sets the associated command.
        /// </summary>
        public ICommand ExecuteCommand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the sub items.
        /// </summary>
        public ObservableCollection<IMenuItem> SubItems { get; set; }

        /// <summary>
        /// The make menu.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="translator">The translator.</param>
        /// <returns>
        /// The <see cref="IMenuItem" />.
        /// </returns>
        public static IMenuItem MakeMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager manager, ISQKeyTranslator translator, ISonarLocalAnalyser analyser)
        {
            var topLel = new SetSqaleMenu(rest, model, manager, translator, analyser) { CommandText = "Sqale", IsEnabled = false };
            
            topLel.SubItems.Add(new SetSqaleMenu(rest, model, manager, translator, analyser) { CommandText = "adjust debt", IsEnabled = true });
            return topLel;
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="sourceModel">The source model.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public void AssociateWithNewProject(Resource project, string workingDir, ISourceControlProvider sourceModel, Dictionary<string, Profile> profile)
        {
            this.sourceControl = sourceModel;
            this.assignProject = project;
        }

        /// <summary>
        /// Refreshes the menu data for menu that have options that
        /// are context dependent on the selected issues.
        /// </summary>
        public void RefreshMenuData()
        {
            // not necessary
        }

        /// <summary>
        /// Cancels the refresh data.
        /// </summary>
        public void CancelRefreshData()
        {
            // not necessary
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        /// <param name="availableProjectsIn">The available projects in.</param>
        public void OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjectsIn, IIssueTrackerPlugin sourcePlugin)
        {
            this.availableProjects = availableProjectsIn.ToList();
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void OnSolutionClosed()
        {
            // not needed
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public void OnDisconnect()
        {
            this.availableProjects = null;
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>
        /// returns view model
        /// </returns>
        public object GetViewModel()
        {
            return null;
        }

        /// <summary>
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public void UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            this.vshelper = vsenvironmenthelperIn;
        }

        /// <summary>
        /// Called when [source control command].
        /// </summary>
        private void OnSetExclusionsMenuCommand()
        {
            if (this.assignProject == null)
            {
                MessageDisplayBox.DisplayMessage("Technical debt can only be adjusted if project is associated");
                return;
            }

            if (this.model.SelectedItems == null || this.model.SelectedItems.Count == 0)
            {
                return;
            }

            try
            {
                Resource projectToUse = SetExclusionsMenu.GetMainProject(this.assignProject, this.availableProjects);

                if (this.CommandText.Equals("adjust debt"))
                {
                    this.AdjustDebtForRule();
                }
            }
            catch (Exception ex)
            {
                this.manager.ReportMessage(new Message { Id = "SetSqaleMenu", Data = "Failed to perform operation: " + ex.Message });
                this.manager.ReportException(ex);
            }
        }

        /// <summary>
        /// Adjusts the debt for rule.
        /// </summary>
        private void AdjustDebtForRule()
        {
            var minutes = PromptUserData.Prompt("debt in minutes", "new value in minutes");
            var min = 0;

            if (string.IsNullOrEmpty(minutes) || !int.TryParse(minutes, out min))
            {
                return;
            }

            foreach (var item in this.model.SelectedItems)
            {
                var issue = item as Issue;
                var rule = this.analyser.GetRuleForKey(issue.Rule, this.assignProject);

                if (rule == null)
                {
                    continue;
                }

                var dic = new Dictionary<string, string> { { "debt_remediation_fn_type", rule.RemediationFunction.ToString() } };

                if (rule.RemediationFunction.Equals(RemediationFunction.LINEAR) || rule.RemediationFunction.Equals(RemediationFunction.LINEAR_OFFSET))
                {
                    dic.Add("debt_remediation_fy_coeff", minutes + "min");
                }

                if (rule.RemediationFunction.Equals(RemediationFunction.CONSTANT_ISSUE) || rule.RemediationFunction.Equals(RemediationFunction.LINEAR_OFFSET))
                {
                    if (rule.RemediationFunction.Equals(RemediationFunction.LINEAR_OFFSET))
                    {
                        minutes = PromptUserData.Prompt("Adjust a new offset for the rule", "new value in minutes");
                        if (string.IsNullOrEmpty(minutes) || !int.TryParse(minutes, out min))
                        {
                            return;
                        }
                    }

                    dic.Add("debt_remediation_fn_offset", minutes + "min");
                }

                List<string> reply = this.rest.UpdateRule(AuthtenticationHelper.AuthToken, rule.Key, dic);
                if (reply != null && reply.Count != 0)
                {
                    this.manager.ReportMessage(new Message { Id = "SetSqaleMenu", Data = "Failed to change debt value for: " + issue.Rule + " : do you have the correct permissions"});
                }
                else
                {
                    this.manager.ReportMessage(new Message { Id = "SetSqaleMenu", Data = "New value for debt was taken into use" });
                }
            }
        }
    }
}
