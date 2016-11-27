// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeStatusHandler.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;

    using Association;
    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using View.Helpers;
    using ViewModel.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using System.Linq;

    /// <summary>
    /// The issue handler menu.
    /// </summary>
    internal class AssignTagMenu : IMenuItem
    {
        /// <summary>
        ///     The model.
        /// </summary>
        private readonly IssueGridViewModel model;
       
        /// <summary>
        ///     The rest.
        /// </summary>
        private readonly ISonarRestService rest;

        /// <summary>
        /// The manager
        /// </summary>
        private readonly INotificationManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignTagMenu" /> class.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="notmanager">The notmanager.</param>
        private AssignTagMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager notmanager)
        {
            this.model = model;
            this.rest = rest;
            this.manager = notmanager;
            this.ExecuteCommand = new RelayCommand(this.OnAssociateCommand);
            this.SubItems = new ObservableCollection<IMenuItem>();

            // register menu for data sync
            AssociationModel.RegisterNewModelInPool(this);
        }

        /// <summary>
        ///     Gets or sets the associated command.
        /// </summary>
        public ICommand ExecuteCommand { get; set; }

        /// <summary>
        ///     Gets or sets the command text.
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the sub items.
        /// </summary>
        public ObservableCollection<IMenuItem> SubItems { get; set; }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        public void OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IIssueTrackerPlugin issuePlugin)
        {
            // does nothing
        }

        /// <summary>
        /// Makes the menu.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="notmanager">The notmanager.</param>
        /// <returns>returns menu.</returns>
        public static IMenuItem MakeMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager notmanager)
        {
            var topLel = new AssignTagMenu(rest, model, notmanager) { CommandText = "Tags", IsEnabled = false };
            topLel.SubItems.Add(new AssignTagMenu(rest, model, notmanager) { CommandText = "assign tag", IsEnabled = true });
            topLel.SubItems.Add(new AssignTagMenu(rest, model, notmanager) { CommandText = "remove tags", IsEnabled = true });
            return topLel;
        }

        /// <summary>
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public void UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            // menu not accessing services
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="configIn">The configuration in.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public void AssociateWithNewProject(
            Resource project,
            string workingDir,
            ISourceControlProvider provider,
            Dictionary<string, Profile> profile,
            string visualStudioVersion)
        {
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public void OnDisconnect()
        {
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void OnSolutionClosed()
        {
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
        ///     The on associate command.
        /// </summary>
        private void OnAssociateCommand()
        {
            try
            {
                if (this.CommandText.Equals("assign tag"))
                {
                    string newtag = string.Empty;
                    var tags = this.rest.GetAvailableTags(AuthtenticationHelper.AuthToken);
                    var seletectTag = PromptForTagIssue.Prompt("Choose tag to assign", "Tag selection", tags, out newtag);
                    var tagsToApply = new List<string>();

                    if (!string.IsNullOrEmpty(newtag))
                    {
                        tagsToApply.Add(newtag);
                    }

                    if (!string.IsNullOrEmpty(seletectTag))
                    {
                        tagsToApply.Add(seletectTag);
                    }

                    if (tagsToApply.Count == 0)
                    {
                        return;
                    }

                    using (var bw = new BackgroundWorker { WorkerReportsProgress = false })
                    {
                        bw.RunWorkerCompleted += delegate { Application.Current.Dispatcher.Invoke(delegate { this.manager.EndedWorking(); }); };

                        bw.DoWork += delegate
                            {
                                var issues = this.model.SelectedItems;
                                var issuesToAssign = new List<Issue>();
                                foreach (Issue issue in issues)
                                {
                                    var finalTags = new List<string>();
                                    finalTags.AddRange(tagsToApply);
                                    foreach (var tag in issue.Tags)
                                    {
                                        finalTags.Add(tag);
                                    }

                                    this.manager.ReportMessage(new Message { Id = "AssignTagMenu", Data = "Assign tags:" + this.rest.SetIssueTags(AuthtenticationHelper.AuthToken, issue as Issue, finalTags) });
                                    foreach (var tag in finalTags)
                                    {
                                        if(!issue.Tags.Contains(tag))
                                        {
                                            issue.Tags.Add(tag);
                                        }
                                    }
                                }

                                this.model.RefreshView();
                            };

                        bw.RunWorkerAsync(); 
                    }
                }

                if (this.CommandText.Equals("remove tags"))
                {
                    var issues = this.model.SelectedItems;
                    var listOfUniqueTags = new SortedSet<string>();
                    var uniqueTags = "";
                    foreach (Issue issue in issues)
                    {
                        foreach (var item in issue.Tags)
                        {
                            if (!listOfUniqueTags.Contains(item))
                            {
                                listOfUniqueTags.Add(item);
                                uniqueTags = uniqueTags + item + ",";
                            }
                        }
                    }

                    string newtag = string.Empty;
                    var tags = this.rest.GetAvailableTags(AuthtenticationHelper.AuthToken);
                    var seletectTag = PromptForTagIssue.Prompt("Choose tag to remove", "Tag selection", tags, out newtag, uniqueTags.TrimEnd(','));
                    var tagsToRemove = new List<string>();

                    if (!string.IsNullOrEmpty(newtag))
                    {
                        tagsToRemove.AddRange(newtag.Split(',').ToList());
                    }

                    if (!string.IsNullOrEmpty(seletectTag))
                    {
                        tagsToRemove.Add(seletectTag);
                    }

                    if (tagsToRemove.Count == 0)
                    {
                        return;
                    }

                    using (var bw = new BackgroundWorker { WorkerReportsProgress = false })
                    {
                        bw.RunWorkerCompleted += delegate { Application.Current.Dispatcher.Invoke(delegate { this.manager.EndedWorking(); }); };

                        bw.DoWork += delegate
                        {
                            var issuesToAssign = new List<Issue>();
                            foreach (Issue issue in issues)
                            {
                                var finalTags = new List<string>();

                                foreach (var tag in issue.Tags)
                                {
                                    bool inList = false;
                                    foreach (var item in tagsToRemove)
                                    {
                                        if (tag.Equals(item))
                                        {
                                            inList = true;
                                        }
                                    }

                                    if (!inList)
                                    {
                                        finalTags.Add(tag);
                                    }
                                }

                                this.manager.ReportMessage(new Message { Id = "AssignTagMenu", Data = "Assign tags:" + this.rest.SetIssueTags(AuthtenticationHelper.AuthToken, issue as Issue, finalTags) });
                                issue.Tags.Clear();

                                foreach (var tag in finalTags)
                                {
                                    issue.Tags.Add(tag);
                                }
                            }

                            this.model.RefreshView();
                        };

                        bw.RunWorkerAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Cannot Modify Tags on Issues", ex);
            }
        }
    }
}