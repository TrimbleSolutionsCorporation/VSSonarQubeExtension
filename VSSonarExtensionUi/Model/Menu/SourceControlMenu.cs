﻿namespace VSSonarExtensionUi.Model.Menu
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
    using SonarRestService.Types;
    using SonarRestService;
    using System.Threading;
    using System.Threading.Tasks;


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
        private readonly INotificationManager logger;

        /// <summary>
        /// The tranlator
        /// </summary>
        private readonly ISQKeyTranslator tranlator;

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
            this.logger = manager;
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
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="sourceModel">The source model.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        /// <param name="profile">The profile.</param>
        public async Task AssociateWithNewProject(
            Resource project,
            string workingDir,
            ISourceControlProvider sourceModel,
            Dictionary<string, Profile> profile,
            string visualStudioVersion)
        {
            this.sourceControl = sourceModel;
            this.assignProject = project;
            await Task.Delay(0);
        }

        /// <summary>
        /// Refreshes the menu data for menu that have options that
        /// are context dependent on the selected issues.
        /// </summary>
        public async Task RefreshMenuData()
        {
			// not necessary
			await Task.Delay(0);
		}

        /// <summary>
        /// Cancels the refresh data.
        /// </summary>
        public async Task CancelRefreshData()
        {
			// not necessary
			await Task.Delay(0);
		}

        /// <summary>
        /// The end data association.
        /// </summary>
        public async Task OnSolutionClosed()
        {
            // not needed
            await Task.Delay(0);
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        public async Task OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IList<IIssueTrackerPlugin> sourcePlugin)
        {
            // does nothing
            await Task.Delay(0);
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public async Task OnDisconnect()
        {
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
            return null;
        }

        /// <summary>
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public async Task UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            await Task.Delay(0);
            this.vshelper = vsenvironmenthelperIn;
        }

        /// <summary>
        /// Called when [source control command].
        /// </summary>
        private async void OnSourceControlCommand()
        {
            if (this.assignProject == null)
            {
                MessageDisplayBox.DisplayMessage("Source control only available when a project is associated.");
                return;
            }

            if (this.model.SelectedItems == null || this.model.SelectedItems.Count == 0)
            {
                return;
            }

            try
            {
                if (this.CommandText.Equals("assign to committer"))
                {
                    var users = await this.rest.GetUserList(AuthtenticationHelper.AuthToken);
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

                    var blameLine = await this.sourceControl.GetBlameByLine(translatedPath, issue.Line);

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
                this.logger.ReportMessage(new Message { Id = "SourceControlMenu", Data = "Failed to perform operation: " + ex.Message });
                this.logger.ReportException(ex);
            }
        }

        /// <summary>
        /// Assigns the issue to user.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <param name="issue">The issue.</param>
        private async void AssignIssueToUser(List<User> users, Issue issue)
        {
            if (this.assignProject == null)
            {
                return;
            }

            try
            {
                var translatedPath = this.tranlator.TranslateKey(issue.Component, this.vshelper, this.assignProject.BranchName);

                var blameLine = await this.sourceControl.GetBlameByLine(translatedPath, issue.Line);

                if (blameLine != null)
                {
                    foreach (var user in users)
                    {
                        var emailBlame = blameLine.Email.ToLower().Trim();
                        var emailSq = user.Email.ToLower().Trim();
                        if (emailBlame.Equals(emailSq))
                        {
                            var issues = new List<Issue>();
                            issues.Add(issue);

                            await this.rest.AssignIssuesToUser(
								AuthtenticationHelper.AuthToken,
								issues,
								user,
								"VSSonarQube Extension Auto Assign",
								this.logger,
								new CancellationTokenSource().Token);
                            this.logger.ReportMessage(new Message { Id = "SourceControlMenu : ", Data = "assign issue to: " + blameLine.Author + " ok" });
                            return;
                        }
                    }
                }
                else
                {
                    this.logger.ReportMessage(new Message { Id = "SourceControlMenu : ", Data = "Cannot assign issue: source control information not available" });
                }                
            }
            catch (Exception ex)
            {
                this.logger.ReportMessage(new Message { Id = "SourceControlMenu", Data = "Failed to assign issue" + issue.Message });
                this.logger.ReportException(ex);
                throw;
            }
        }
    }
}
