namespace VSSonarExtensionUi.Model.Menu
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using Helpers;

    using ViewModel.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using GalaSoft.MvvmLight.Command;
    using Association;
    using System.Collections.Generic;
    using SonarLocalAnalyser;
    using View.Helpers;


    /// <summary>
    /// Source Control Related Actions
    /// </summary>
    public class SourceControlMenu : IMenuItem
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
        /// The source control
        /// </summary>
        private ISourceControlProvider sourceControl;

        /// <summary>
        /// The user conf
        /// </summary>
        private ISonarConfiguration userConf;

        /// <summary>
        /// The vshelper
        /// </summary>
        private IVsEnvironmentHelper vshelper;

        /// <summary>
        /// The assign project
        /// </summary>
        private Resource assignProject;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceControlMenu" /> class.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="translator">The translator.</param>
        public SourceControlMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager manager, ISQKeyTranslator translator)
        {
            this.rest = rest;
            this.model = model;
            this.manager = manager;
            this.tranlator = translator;

            this.ExecuteCommand = new RelayCommand(this.OnSourceControlCommand);
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
        public static IMenuItem MakeMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager manager, ISQKeyTranslator translator)
        {
            var topLel = new SourceControlMenu(rest, model, manager, translator) { CommandText = "Source Control", IsEnabled = false };
            topLel.SubItems.Add(new SourceControlMenu(rest, model, manager, translator) { CommandText = "assign to committer", IsEnabled = true });
            topLel.SubItems.Add(new SourceControlMenu(rest, model, manager, translator) { CommandText = "show commit message", IsEnabled = true });
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
        public void AssociateWithNewProject(ISonarConfiguration config, Resource project, string workingDir, ISourceControlProvider sourceModel, IIssueTrackerPlugin sourcePlugin)
        {
            this.sourceControl = sourceModel;
            this.userConf = config;
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
        /// The end data association.
        /// </summary>
        public void EndDataAssociation()
        {
            // not needed
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
        private void OnSourceControlCommand()
        {
            if (this.model.SelectedItems == null || this.model.SelectedItems.Count == 0)
            {
                return;
            }

            try
            {
                if (this.CommandText.Equals("assign to committer"))
                {
                    var users = this.rest.GetUserList(this.userConf);
                    var issues = this.model.SelectedItems;

                    foreach (var issue in issues)
                    {
                        this.AssignIssueToUser(users, issue as Issue);
                    }

                    return;
                }

                if (this.CommandText.Equals("show commit message"))
                {
                    var issue = this.model.SelectedItems[0] as Issue;

                    var translatedPath = this.tranlator.TranslateKey(issue.Component, this.vshelper, this.assignProject.BranchName);

                    var blameLine = this.sourceControl.GetBlameByLine(translatedPath, issue.Line);

                    if (blameLine != null)
                    {
                        string message = string.Format("Author:\t\t {0}\r\nEmail:\t\t {1}\r\nCommit Date:\t {2}\r\nHash:\t\t {3}", blameLine.Author, blameLine.Email, blameLine.Date, blameLine.Guid);
                        string summary = blameLine.Summary;

                        MessageDisplayBox.DisplayMessage(message, summary);
                    }                                       
                }
            }
            catch (Exception ex)
            {
                this.manager.ReportMessage(new Message { Id = "SourceControlMenu", Data = "Failed to perform operation: " + ex.Message });
                this.manager.ReportException(ex);
                throw;
            }
        }

        /// <summary>
        /// Assigns the issue to user.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <param name="issue">The issue.</param>
        private void AssignIssueToUser(List<User> users, Issue issue)
        {
            try
            {
                var translatedPath = this.tranlator.TranslateKey(issue.Component, this.vshelper, this.assignProject.BranchName);

                var blameLine = this.sourceControl.GetBlameByLine(translatedPath, issue.Line);

                if (blameLine != null)
                {
                    foreach (var user in users)
                    {
                        if (user.Email.Equals(blameLine.Email.ToLower()))
                        {
                            var issues = new List<Issue>();
                            issues.Add(issue);

                            this.rest.AssignIssuesToUser(this.userConf, issues, user, "VSSonarQube Extension Auto Assign");
                            this.manager.ReportMessage(new Message { Id = "SourceControlMenu : ", Data = "assign issue to: " + blameLine.Author + " ok" });
                            return;
                        }
                    }
                }
                else
                {
                    this.manager.ReportMessage(new Message { Id = "SourceControlMenu : ", Data = "Cannot assign issue: source control information not available" });
                }                
            }
            catch (Exception ex)
            {
                this.manager.ReportMessage(new Message { Id = "SourceControlMenu", Data = "Failed to assign issue" + issue.Message });
                this.manager.ReportException(ex);
                throw;
            }
        }
    }
}
