// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenResourceMenu.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections;
    using System.Linq;
    /// <summary>
    /// The issue handler menu.
    /// </summary>
    internal class PlanMenu : IMenuItem
    {
        /// <summary>
        /// The model.
        /// </summary>
        private readonly IssueGridViewModel model;

        /// <summary>
        /// The rest.
        /// </summary>
        private readonly ISonarRestService rest;

        /// <summary>
        /// The manager
        /// </summary>
        private readonly INotificationManager manager;

        /// <summary>
        /// The parent
        /// </summary>
        private readonly PlanMenu parent;

        /// <summary>
        /// The vs helper.
        /// </summary>
        private IVsEnvironmentHelper visualStudioHelper;

        /// <summary>
        /// The source directory
        /// </summary>
        private string sourceDir;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// The provider
        /// </summary>
        private ISourceControlProvider provider;

        /// <summary>
        /// The available plans
        /// </summary>
        private ObservableCollection<SonarActionPlan> availablePlans;
        private IEnumerable<Resource> availableProjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanMenu" /> class.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="registerPool">if set to <c>true</c> [register pool].</param>
        private PlanMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager manager, PlanMenu parent = null, bool registerPool = true)
        {
            this.parent = parent;
            this.manager = manager;
            this.model = model;
            this.rest = rest;

            this.ExecuteCommand = new RelayCommand(this.OnPlanCommand);
            this.SubItems = new ObservableCollection<IMenuItem>();

            if (registerPool)
            {
                AssociationModel.RegisterNewModelInPool(this);
            }            
        }

        /// <summary>
        /// Gets or sets the associated command.
        /// </summary>
        public ICommand ExecuteCommand { get; set; }

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the sub items.
        /// </summary>
        public ObservableCollection<IMenuItem> SubItems { get; set; }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        /// <value>
        /// The profile.
        /// </value>
        public Dictionary<string, Profile> Profile { get; private set; }

        /// <summary>
        /// The make menu.
        /// </summary>
        /// <param name="rest">The rest.</param>
        /// <param name="model">The model.</param>
        /// <param name="manager">The manager.</param>
        /// <returns>
        /// The <see cref="IMenuItem" />.
        /// </returns>
        public static IMenuItem MakeMenu(ISonarRestService rest, IssueGridViewModel model, INotificationManager manager)
        {
            var topLel = new PlanMenu(rest, model, manager) { CommandText = "Plan", IsEnabled = false };

            topLel.SubItems.Add(new PlanMenu(rest, model, manager) { CommandText = "Add to Existent Plan", IsEnabled = false });
            topLel.SubItems.Add(new PlanMenu(rest, model, manager) { CommandText = "Unplan", IsEnabled = true });
            topLel.SubItems.Add(new PlanMenu(rest, model, manager, topLel) { CommandText = "Associate to new plan", IsEnabled = true });
            return topLel;
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
            // does nothing
            this.availableProjects = availableProjectsIn;
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public void OnDisconnect()
        {
            // not necessary
        }

        /// <summary>
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The visual studio environment helper.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="providerIn">The provider in.</param>
        public void UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider providerIn)
        {
            this.visualStudioHelper = vsenvironmenthelperIn;
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="providerIn">The provider in.</param>
        /// <param name="sourcePluginIn">The source plugin in.</param>
        /// <param name="profile">The profile.</param>
        public void AssociateWithNewProject(Resource project, string workingDir, ISourceControlProvider providerIn, Dictionary<string, Profile> profile)
        {
            this.Profile = profile;
            this.sourceDir = workingDir;
            this.associatedProject = project;
            this.provider = providerIn;
            this.availableProjects = availableProjects;
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
        /// The end data association.
        /// </summary>
        public void OnSolutionClosed()
        {
            this.associatedProject = null;
            this.sourceDir = string.Empty;
        }

        /// <summary>
        /// The on associate command.
        /// </summary>
        private void OnPlanCommand()
        {
            try
            {
                if (this.CommandText.Equals("Associate to new plan"))
                {
                    var plan = PromptUserForNewPlan.Prompt(this.availablePlans.ToList(), this.availableProjects.ToList<Resource>(), this.associatedProject);

                    if (plan == null)
                    {
                        return;
                    }

                    this.AssociateToNewPlan(plan, this.model.SelectedItems);

                    return;
                }

                if (this.CommandText.Equals("Unplan"))
                {
                    this.UnPlanIssues();

                    foreach (var issue in this.model.SelectedItems)
                    {
                        (issue as Issue).ActionPlanName = string.Empty;
                        (issue as Issue).ActionPlan = string.Empty;
                    }

                    return;
                }

                if (!this.CommandText.Equals("Unplan") && !this.CommandText.Equals("Associate to new plan"))
                { 
                    foreach (var plan in this.availablePlans)
                    {
                        if (plan.NamePlusProject.Equals(this.CommandText))
                        {
                            this.AttachToExistentPlan(plan);
                            foreach (var issue in this.model.SelectedItems)
                            {
                                (issue as Issue).ActionPlanName = plan.Name;
                                (issue as Issue).ActionPlan = plan.Key;
                            }
                        }
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Cannot Perform Operation in Plan: " + ex.Message + " please check vs output log for detailed information", ex);
            }
        }

        /// <summary>
        /// Updates the action plans.
        /// </summary>
        /// <param name="availableActionPlans">The available action plans.</param>
        public void UpdateActionPlans(ObservableCollection<SonarActionPlan> availableActionPlans)
        {
            this.ReloadPlanData(this, availableActionPlans);
        }

        /// <summary>
        /// Remove issues from plan.
        /// </summary>
        private void UnPlanIssues()
        {
            using (var bw = new BackgroundWorker { WorkerReportsProgress = false })
            {
                bw.RunWorkerCompleted += delegate { Application.Current.Dispatcher.Invoke(delegate { this.manager.EndedWorking(); }); };

                bw.DoWork += delegate
                {
                    this.manager.StartedWorking("Unplan");
                    var replies = this.rest.UnPlanIssues(AuthtenticationHelper.AuthToken, this.model.SelectedItems);
                    foreach (var itemreply in replies)
                    {
                        this.manager.ReportMessage(new Message { Data = "Unplan Operation Result: " + itemreply.Key + " : " + itemreply.Value });
                    }

                    this.model.RefreshView();
                };

                bw.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Attaches to existent plan.
        /// </summary>
        /// <param name="plan">The plan.</param>
        private void AttachToExistentPlan(SonarActionPlan plan)
        {
            using (var bw = new BackgroundWorker { WorkerReportsProgress = false })
            {
                bw.RunWorkerCompleted += delegate { Application.Current.Dispatcher.Invoke(delegate { this.manager.EndedWorking(); }); };

                bw.DoWork += delegate
                {
                    this.manager.StartedWorking("Attach to plan");

                    var replies = this.rest.PlanIssues(AuthtenticationHelper.AuthToken, this.model.SelectedItems, plan.Key.ToString());
                    foreach (var itemreply in replies)
                    {
                        this.manager.ReportMessage(new Message { Data = "Plan Operation Result: " + itemreply.Key + " : " + itemreply.Value });
                    }

                    this.model.RefreshView();
                };

                bw.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Associates to new plan.
        /// </summary>
        /// <param name="newPlan">The new plan.</param>
        /// <param name="selectedItems">The selected items.</param>
        private void AssociateToNewPlan(SonarActionPlan newPlan, IList selectedItems)
        {
            using (var bw = new BackgroundWorker { WorkerReportsProgress = false })
            {
                bw.RunWorkerCompleted += delegate
                {
                    this.ReloadPlanData(this.parent, newPlan);
                    Application.Current.Dispatcher.Invoke(delegate { this.manager.EndedWorking(); });
                };

                bw.DoWork += delegate
                {
                    this.manager.StartedWorking("Associate With New Plan");

                    try
                    {
                        var plan = this.rest.CreateNewPlan(AuthtenticationHelper.AuthToken, newPlan.Project, newPlan);
                        var replies = this.rest.PlanIssues(AuthtenticationHelper.AuthToken, selectedItems, plan.Key.ToString());

                        foreach (Issue issue in selectedItems)
                        {
                            issue.ActionPlanName = plan.Name;
                            issue.ActionPlan = plan.Key;
                        }

                        foreach (var itemreply in replies)
                        {
                            this.manager.ReportMessage(new Message { Data = "Plan Operation Result: " + itemreply.Key + " : " + itemreply.Value });
                        }

                        this.model.RefreshView();
                    }
                    catch (Exception ex)
                    {
                        this.manager.ReportMessage(new Message { Data = "Plan Operation Failed" + ex.Message });
                        this.manager.ReportException(ex);
                    }
                };

                bw.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Reloads the plan data.
        /// </summary>
        /// <param name="parentMenu">The parent menu.</param>
        /// <param name="plan">The plan.</param>
        private void ReloadPlanData(PlanMenu parentMenu, SonarActionPlan plan)
        {
            foreach (var item in parentMenu.SubItems)
            {
                if (item.CommandText.Equals("Add to Existent Plan"))
                {
                    Application.Current.Dispatcher.Invoke(
                        delegate
                        {
                            foreach (PlanMenu availablePlan in item.SubItems)
                            {
                                if (availablePlan.CommandText.Equals(plan.Project))
                                {
                                    var menu = new PlanMenu(this.rest, this.model, this.manager, null, false) { CommandText = plan.NamePlusProject, IsEnabled = true };
                                    menu.AssociateWithNewProject(this.associatedProject, this.sourceDir, this.provider, this.Profile);
                                    availablePlan.SubItems.Add(menu);
                                }
                            }
                        });
                }
            }
        }


        /// <summary>
        /// Reloads the plan data.
        /// </summary>
        /// <param name="parentMenu">The parent menu.</param>
        /// <param name="plans">The plans.</param>
        private void ReloadPlanData(PlanMenu parentMenu, ObservableCollection<SonarActionPlan> plans)
        {
            this.availablePlans = plans;

            foreach (PlanMenu item in parentMenu.SubItems)
            {
                item.availablePlans = plans;

                if (item.CommandText.Equals("Add to Existent Plan"))
                {
                    Application.Current.Dispatcher.Invoke(
                        delegate
                        {
                            // clear data
                            foreach (var data in item.SubItems)
                            {
                                data.SubItems.Clear();
                            }

                            item.SubItems.Clear();

                            var plansPerProject = new Dictionary<string, List<SonarActionPlan>>();

                            foreach (SonarActionPlan plan in this.availablePlans)
                            {
                                if (!plansPerProject.ContainsKey(plan.Project))
                                {
                                    var listofplans = new List<SonarActionPlan>();
                                    listofplans.Add(plan);
                                    plansPerProject.Add(plan.Project, listofplans);
                                }
                                else
                                {
                                    plansPerProject[plan.Project].Add(plan);
                                }
                            }


                            foreach (var planPerProject in plansPerProject)
                            {
                                var menu = new PlanMenu(this.rest, this.model, this.manager, null, false) { CommandText = planPerProject.Key, IsEnabled = false };
                                menu.AssociateWithNewProject(this.associatedProject, this.sourceDir, this.provider, this.Profile);
                                foreach (var plan in planPerProject.Value)
                                {
                                    var submenu = new PlanMenu(this.rest, this.model, this.manager, null, false) { CommandText = plan.NamePlusProject, IsEnabled = true };
                                    submenu.AssociateWithNewProject(this.associatedProject, this.sourceDir, this.provider, this.Profile);
                                    submenu.UpdateActionPlans(plans);
                                    menu.SubItems.Add(submenu);
                                }

                                item.SubItems.Add(menu);
                            }                            
                        });
                }
            }
        }
    }
}