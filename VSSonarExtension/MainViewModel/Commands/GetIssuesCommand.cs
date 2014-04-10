// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetIssuesCommand.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.MainViewModel.Commands
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;

    using SonarRestService;

    using VSSonarExtension.MainViewModel.ViewModel;

    /// <summary>
    /// The view options command.
    /// </summary>
    public class GetIssuesCommand : ICommand
    {
        /// <summary>
        /// The user entry data.
        /// </summary>
        private readonly ExtensionDataModel model;

        /// <summary>
        /// The service.
        /// </summary>
        private readonly ISonarRestService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetIssuesCommand"/> class.
        /// </summary>
        public GetIssuesCommand()
        {
            var handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetIssuesCommand"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="service">
        /// The service.
        /// </param>
        public GetIssuesCommand(ExtensionDataModel model, ISonarRestService service)
        {
            this.model = model;
            this.service = service;
        }

        /// <summary>
        /// The can execute changed.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// The can execute.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        public void Execute(object parameter)
        {
            var header = parameter as string;
            if (string.IsNullOrEmpty(header))
            {
                this.model.ErrorMessage = "Wrong data in command from Visual Studio";
                return;
            }

            if (!this.ValidateConfiguration())
            {
                return;
            }

            if (header.Equals("All Issues"))
            {
                this.GetAllIssues();
                return;
            }

            if (header.Equals("All Issues Since Last Analysis"))
            {
                this.GetAllIssuesSinceLastAnalysis();
                return;
            }

            if (header.Equals("My Issues In Project"))
            {
                this.GetAllIssuesInProject();
                return;
            }

            if (header.Equals("All My Issues"))
            {
                this.GetAllMyIssues();
                return;
            }

            if (header.Equals("Update Issues"))
            {
                this.GetIssuesUsingCurrentFilter();
                return;
            }

            if (header.Equals("Apply Filter"))
            {
                this.model.SaveFilterToDisk();
                this.model.RefreshIssuesInViews();
            }
        }

        /// <summary>
        /// The get issues using current filter.
        /// </summary>
        private void GetIssuesUsingCurrentFilter()
        {
            this.model.QueryForIssuesIsRunning = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate { this.model.QueryForIssuesIsRunning = false; };

            bw.DoWork += delegate { this.model.RetrieveIssuesUsingCurrentFilter(); };

            bw.RunWorkerAsync();
        }

        /// <summary>
        /// The get all my issues.
        /// </summary>
        private void GetAllMyIssues()
        {
            this.model.QueryForIssuesIsRunning = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate { this.model.QueryForIssuesIsRunning = false; };
            bw.RunWorkerAsync();
        }

        /// <summary>
        /// The get all issues in project.
        /// </summary>
        private void GetAllIssuesInProject()
        {
            this.model.QueryForIssuesIsRunning = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate { this.model.QueryForIssuesIsRunning = false; };

            bw.DoWork +=
                delegate
                    {
                        this.model.ReplaceAllIssuesInCache(
                            this.service.GetIssuesByAssigneeInProject(
                                this.model.UserConfiguration,
                                this.model.AssociatedProject.Key,
                                this.model.UserConfiguration.Username));
                    };

            bw.RunWorkerAsync();
        }

        /// <summary>
        /// The get all issues since last analysis.
        /// </summary>
        private void GetAllIssuesSinceLastAnalysis()
        {
            this.model.QueryForIssuesIsRunning = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate { this.model.QueryForIssuesIsRunning = false; };

            bw.DoWork +=
                delegate
                    {
                        this.model.ReplaceAllIssuesInCache(
                            this.service.GetIssuesForProjectsCreatedAfterDate(
                                this.model.UserConfiguration,
                                this.model.AssociatedProject.Key,
                                this.model.AssociatedProject.Date));
                    };

            bw.RunWorkerAsync();
        }

        /// <summary>
        /// The get all issues.
        /// </summary>
        private void GetAllIssues()
        {
            this.model.QueryForIssuesIsRunning = true;
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate { this.model.QueryForIssuesIsRunning = false; };

            bw.DoWork +=
                delegate
                    {
                        this.model.ReplaceAllIssuesInCache(this.service.GetIssuesForProjects(this.model.UserConfiguration, this.model.AssociatedProject.Key));
                    };

            bw.RunWorkerAsync();
        }

        /// <summary>
        /// The validate configuration.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ValidateConfiguration()
        {
            if (this.service == null || this.model.UserConfiguration == null || this.model.AssociatedProject == null)
            {
                if (this.model.UserConfiguration == null)
                {
                    this.model.ErrorMessage = "Authentication Failed, check Sonar Options";
                }

                if (this.model.AssociatedProject == null)
                {
                    this.model.ErrorMessage = "No project selected, please select a project from List";
                }

                return false;
            }

            return true;
        }
    }
}
