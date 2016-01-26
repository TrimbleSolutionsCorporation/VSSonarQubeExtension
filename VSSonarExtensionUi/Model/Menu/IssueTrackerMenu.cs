// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssueTrackerMenu.cs" company="Copyright © 2015 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarExtensionUi.Model.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Input;

    using Association;
    using GalaSoft.MvvmLight.Command;
    using Helpers;

    using SonarLocalAnalyser;
    using View.Helpers;
    using ViewModel.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// Issue tracker menus.
    /// </summary>
    public class IssueTrackerMenu : IMenuItem
    {
        /// <summary>
        /// The comment message for issue
        /// </summary>
        private const string CommentMessageForIssue = "[VSSonarQubeExtension] Attached to issue: ";

        /// <summary>
        /// The parent
        /// </summary>
        private readonly IssueTrackerMenu parent;

        /// <summary>
        /// The defect cache
        /// </summary>
        private readonly Dictionary<Guid, Defect> issueDefectCache = new Dictionary<Guid, Defect>();

        /// <summary>
        /// The defect cache
        /// </summary>
        private readonly Dictionary<string, Defect> defectCache = new Dictionary<string, Defect>();

        /// <summary>
        /// The manager
        /// </summary>
        private readonly INotificationManager manager;

        /// <summary>
        /// The model
        /// </summary>
        private readonly IssueGridViewModel model;

        /// <summary>
        /// The rest
        /// </summary>
        private readonly ISonarRestService rest;

        /// <summary>
        /// The translator
        /// </summary>
        private readonly ISQKeyTranslator translator;

        /// <summary>
        /// The source dir
        /// </summary>
        private string sourceDir;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// The configuration
        /// </summary>
        private ISonarConfiguration config;

        /// <summary>
        /// The source plugin
        /// </summary>
        private IIssueTrackerPlugin issueTrackerPlugin;

        /// <summary>
        /// The source model
        /// </summary>
        private ISourceControlProvider sourceModel;

        /// <summary>
        /// The vshelper
        /// </summary>
        private IVsEnvironmentHelper vshelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueTrackerMenu" /> class.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="translator">The translator.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="registerPool">if set to <c>true</c> [register pool].</param>
        private IssueTrackerMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager manager, ISQKeyTranslator translator, IssueTrackerMenu parent = null, bool registerPool = true)
        {
            this.translator = translator;
            this.manager = manager;
            this.model = model;
            this.rest = rest;
            this.parent = parent;

            this.ExecuteCommand = new RelayCommand(this.OnAttachToIssueTracker);
            this.SubItems = new ObservableCollection<IMenuItem>();

            if (registerPool)
            {
                AssociationModel.RegisterNewModelInPool(this);
            }
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
            var topLel = new IssueTrackerMenu(rest, model, manager, translator) { CommandText = "Issue Tracker", IsEnabled = false };
            topLel.SubItems.Add(new IssueTrackerMenu(rest, model, manager, translator) { CommandText = "new issue and attach", IsEnabled = true });
            topLel.SubItems.Add(new IssueTrackerMenu(rest, model, manager, translator) { CommandText = "attach to existent", IsEnabled = true });
            return topLel;
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="configIn">The configuration in.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="sourceModelIn">The source model in.</param>
        /// <param name="sourcePluginIn">The source plugin in.</param>
        public void AssociateWithNewProject(Resource project, string workingDir, ISourceControlProvider sourceModelIn, IIssueTrackerPlugin sourcePlugin, IList<Resource> availableProjects, Dictionary<string, Profile> profile)
        {
            this.sourceDir = workingDir;
            this.associatedProject = project;
            this.issueTrackerPlugin = sourcePlugin;
            this.sourceModel = sourceModelIn;
            this.config = AuthtenticationHelper.AuthToken;
        }


        /// <summary>
        /// The end data association.
        /// </summary>
        public void OnSolutionClosed()
        {
            this.associatedProject = null;
            this.sourceDir = string.Empty;
            this.config = null;
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
        /// Called when [disconnect].
        /// </summary>
        public void OnDisconnect()
        {
        }

        /// <summary>
        /// Cancels the refresh data.
        /// </summary>
        public void CancelRefreshData()
        {
            // not used
        }

        /// <summary>
        /// Refreshes the menu data for menu that have options that
        /// are context dependent on the selected issues.
        /// </summary>
        public void RefreshMenuData()
        {
            // not used
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
        /// Called when [attache to issue tracker].
        /// </summary>
        private void OnAttachToIssueTracker()
        {
            if (this.model.SelectedItems == null || this.model.SelectedItems.Count == 0)
            {
                return;
            }  

            try
            {
                if (this.CommandText.Equals("new issue and attach"))
                {
                    var issues = this.model.SelectedItems.Cast<Issue>().ToList();
                    string id = string.Empty;
                    var replydata = this.issueTrackerPlugin.AttachToNewDefect(issues, out id);

                    // update issues
                    foreach (var issue in issues)
                    {
                        issue.IssueTrackerId = id;
                    }

                    if (string.IsNullOrEmpty(replydata))
                    {
                        this.manager.ReportMessage(new Message { Id = "IssueTrackerMenu", Data = "Failed to create defect" });
                        return;
                    }

                    var builder = new StringBuilder();
                    builder.AppendLine(CommentMessageForIssue + id);
                    builder.AppendLine(replydata);
                    this.rest.CommentOnIssues(this.config, issues, builder.ToString());
                }

                if (this.CommandText.Equals("attach to existent"))
                {
                    var id = PromptUserData.Prompt("Enter Item Id", "Issue Tracker Id");
                    if (string.IsNullOrEmpty(id))
                    {
                        return;
                    }

                    try
                    {
                        var issues = this.model.SelectedItems.Cast<Issue>().ToList();
                        var replydata = this.issueTrackerPlugin.AttachToExistentDefect(issues, id);

                        if (string.IsNullOrEmpty(replydata))
                        {
                            this.manager.ReportMessage(new Message { Id = "IssueTrackerMenu", Data = "Failed to attach to defect" });
                            return;
                        }

                        // update issues
                        foreach (var issue in issues)
                        {
                            issue.IssueTrackerId = id;
                        }

                        var builder = new StringBuilder();
                        builder.AppendLine(CommentMessageForIssue + id);
                        builder.AppendLine(replydata);
                        this.rest.CommentOnIssues(this.config, issues, builder.ToString());
                    }
                    catch (Exception ex)
                    {
                        this.manager.ReportMessage(new Message { Id = "IssueTrackerMenu", Data = "Failed to attach to defect: " + ex.Message });
                        this.manager.ReportException(ex);
                        return;
                    }
                }

                if (this.CommandText.Equals("show info"))
                {
                    var id = this.parent.CommandText;
                    if (!this.parent.parent.defectCache.ContainsKey(id))
                    {
                        try
                        {
                            var defect = this.issueTrackerPlugin.GetDefect(id);
                            this.parent.parent.defectCache.Add(id, defect);
                        }
                        catch (Exception ex)
                        {
                            this.manager.ReportMessage(new Message { Id = "IssueTrackerMenu", Data = "Failed to get info for defect: " + ex.Message });
                            this.manager.ReportException(ex);
                            return;
                        }
                    }

                    var item = this.parent.parent.defectCache[id];
                    string message = string.Format("Summary:\t\t {0}\r\nStatus:\t\t {1}\r\nId:\t {2}\r\n", item.Summary, item.Status, item.Id);
                    MessageDisplayBox.DisplayMessage(message, string.Empty);
                }
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Cannot Perform Operation in Plan: " + ex.Message + " please check vs output log for detailed information", ex);
            }
        }

        /// <summary>
        /// Generates the identifier from message.
        /// </summary>
        /// <param name="htmlText">The HTML text.</param>
        /// <returns>returns id from a message.</returns>
        private string GenerateIdFromMessage(string htmlText)
        {
            foreach (Match item in Regex.Matches(htmlText, "\\d+"))
            {
                return item.Value;
            }

            return string.Empty;
        }
    }
}
