// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectDuplicationTrackerModel.cs" company="Copyright © 2014 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
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

namespace ProjectDuplicationTracker
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using VSSonarPlugins.Types;

    using ProjectDuplicationTracker.Annotations;


    using VSSonarPlugins;
    using System.Windows.Media;
    using PropertyChanged;

    /// <summary>
    /// The project duplication tracker model.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class ProjectDuplicationTrackerModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The conf.
        /// </summary>
        private ISonarConfiguration conf;

        /// <summary>
        /// The service.
        /// </summary>
        private readonly ISonarRestService service;

        /// <summary>
        /// The vshelper.
        /// </summary>
        private IVsEnvironmentHelper vshelper;

        /// <summary>
        /// The selected first project.
        /// </summary>
        private string selectedFirstProject;

        /// <summary>
        /// The selected second project.
        /// </summary>
        private string selectedSecondProject;

        /// <summary>
        /// The sonar host.
        /// </summary>
        private string sonarHost;

        /// <summary>
        /// The sonar login.
        /// </summary>
        private string sonarLogin;

        /// <summary>
        /// The project resources.
        /// </summary>
        private List<SonarProject> projectResources;

        /// <summary>
        /// The cross project resources.
        /// </summary>
        private List<SonarProject> crossProjectResources;

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }


        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        /// The status message.
        /// </summary>
        private string statusMessage;

        /// <summary>
        /// The user is logged in.
        /// </summary>
        private bool userIsLoggedIn;

        /// <summary>
        /// The duplicated data.
        /// </summary>
        private List<DuplicationData> duplicatedData;

        /// <summary>
        /// The duplicated resources.
        /// </summary>
        private ObservableCollection<string> duplicatedResources;

        /// <summary>
        /// The selected resource in view.
        /// </summary>
        private string selectedResourceInView;

        /// <summary>
        /// The duplicated groups.
        /// </summary>
        private List<DuplicatedGroup> duplicatedGroups;

        /// <summary>
        /// The duplicated blocks.
        /// </summary>
        private List<DuplicatedBlock> duplicatedBlocks;

        /// <summary>
        /// The duplicated groups txt.
        /// </summary>
        private List<string> duplicatedGroupsTxt;

        /// <summary>
        /// The selected group in view.
        /// </summary>
        private string selectedGroupInView;

        /// <summary>
        /// The selected block in view.
        /// </summary>
        private DuplicatedBlock selectedBlockInView;

        /// <summary>
        /// The second project resources list.
        /// </summary>
        private List<string> secondProjectResourcesList;

        /// <summary>
        /// The project resources list.
        /// </summary>
        private List<string> projectResourcesList;

        /// <summary>
        /// The is idle.
        /// </summary>
        private bool isIdle;

        /// <summary>
        /// The source code txt.
        /// </summary>
        private string sourceCodeTxt;


        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectDuplicationTrackerModel"/> class.
        /// </summary>
        public ProjectDuplicationTrackerModel()
        {
            this.UserIsLoggedIn = false;
            this.conf = new ConnectionConfiguration("http://localhost", "admin", "admin", 4.5);
            this.vshelper = null;
            this.Login();
            this.IsIdle = true;
            this.service = null;

            this.UpdateColours(Colors.White, Colors.Black);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectDuplicationTrackerModel"/> class.
        /// </summary>
        /// <param name="conf">
        /// The conf.
        /// </param>
        /// <param name="vshelper">
        /// The vshelper.
        /// </param>
        public ProjectDuplicationTrackerModel(ISonarConfiguration conf, IVsEnvironmentHelper vshelper, ISonarRestService service)
        {
            this.service = service;
            this.vshelper = vshelper;
            this.UserIsLoggedIn = false;
            this.conf = conf;
            this.Login();
            this.IsIdle = true;

            this.UpdateColours(Colors.White, Colors.Black);
        }

        /// <summary>
        /// The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the selected first project.
        /// </summary>
        public string SelectedFirstProject
        {
            get
            {
                return this.selectedFirstProject;
            }

            set
            {
                this.selectedFirstProject = value;
                this.FirstProject = this.GetSonarProjectProjectList(value);
                this.SetDuplicationDataForSingleProject(this.FirstProject);
            }
        }

        /// <summary>
        /// Gets or sets the duplicated resources.
        /// </summary>
        public ObservableCollection<string> DuplicatedResources
        {
            get
            {
                return this.duplicatedResources;
            }

            set
            {
                this.duplicatedResources = null;
                this.duplicatedResources = value;
                this.OnPropertyChanged();
            }
        } 

        /// <summary>
        /// Gets or sets project resource list
        /// </summary>
        public List<DuplicationData> DuplicatedData
        {
            get
            {
                return this.duplicatedData;
            }

            set
            {
                this.duplicatedData = value;
                var data = new ObservableCollection<string>();
                foreach (var dup in this.duplicatedData)
                {
                    data.Add(dup.Resource.Key);                    
                }

                this.DuplicatedResources = data;                
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// Gets or sets project resource list
        /// </summary>
        public List<DuplicatedBlock> DuplicatedBlocks
        {
            get
            {
                return this.duplicatedBlocks;
            }

            set
            {
                this.duplicatedBlocks = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// Gets or sets project resource list
        /// </summary>
        public List<string> DuplicatedGroupsTxt
        {
            get
            {
                return this.duplicatedGroupsTxt;
            }

            set
            {
                this.duplicatedGroupsTxt = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets project resource list
        /// </summary>
        public List<DuplicatedGroup> DuplicatedGroups
        {
            get
            {
                return this.duplicatedGroups;
            }

            set
            {
                this.duplicatedGroups = value;
                var stringOfGroups = new List<string>();
                for (var i = 0; i < this.duplicatedGroups.Count; i++)
                {
                    stringOfGroups.Add(i.ToString(CultureInfo.InvariantCulture));
                }

                this.DuplicatedGroupsTxt = stringOfGroups;

                if (this.duplicatedGroups.Count > 0)
                {
                        this.SelectedGroupInView = "0";
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets project resource list
        /// </summary>
        public List<string> ProjectResourcesList
        {
            get
            {
                if (this.projectResourcesList != null && this.projectResourcesList.Count > 0)
                {
                    return new List<string>(this.projectResourcesList.OrderBy(i => i));
                }

                return this.projectResourcesList;
            }

            set
            {
                this.projectResourcesList = value;
                this.OnPropertyChanged();
            }
        }

        public List<string> SecondProjectResourcesList
        {
            get
            {
                return this.secondProjectResourcesList;
            }

            set
            {
                this.secondProjectResourcesList = value;
                this.OnPropertyChanged();
            }
        }
        


        /// <summary>
        /// Gets or sets the selected first project.
        /// </summary>
        public string SelectedSecondProject
        {
            get
            {
                return this.selectedSecondProject;
            }

            set
            {
                this.selectedSecondProject = value;

                if (string.IsNullOrEmpty(value))
                {
                    this.FirstProject = this.GetSonarProjectProjectList(this.SelectedFirstProject);
                    this.SetDuplicationDataForSingleProject(this.FirstProject);
                }
                else
                {
                    this.SecondProject = this.GetSonarProjectProjectList(value);
                    this.SetCrossDuplicationData(this.FirstProject, this.SecondProject);
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the sonar host.
        /// </summary>
        public string SonarHost
        {
            get
            {
                return this.sonarHost;
            }

            set
            {
                this.sonarHost = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the sonar login.
        /// </summary>
        public string SonarLogin
        {
            get
            {
                return this.sonarLogin;
            }

            set
            {
                this.sonarLogin = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the users list.
        /// </summary>
        public List<SonarProject> ProjectResources
        {
            get
            {
                return this.projectResources;
            }

            set
            {
                this.projectResources = value;
                this.ProjectResourcesList = this.projectResources.Select(projectResource => projectResource.Project.Lname + "   -    " + projectResource.Project.Lang).ToList();
                this.OnPropertyChanged("ProjectResourcesList");
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the cross project resources.
        /// </summary>
        public List<SonarProject> CrossProjectResources
        {
            get
            {
                return this.crossProjectResources;
            }

            set
            {
                this.crossProjectResources = value;
                this.CrossProjectResourcesList = this.crossProjectResources.Select(projectResource => projectResource.Project.Lname + "   -    " + projectResource.Project.Lang).ToList();
                this.CrossProjectResourcesList.Add(string.Empty);
                this.OnPropertyChanged("CrossProjectResourcesList");
            }
        }

        /// <summary>
        /// Gets or sets the cross project resources list.
        /// </summary>
        public List<string> CrossProjectResourcesList { get; set; }

        /// <summary>
        /// The set password.
        /// </summary>
        public void Login()
        {
            if (this.conf == null)
            {
                this.StatusMessage = "Not Logged";
                return;
            }

            try
            {
                this.SonarVersion = this.service.GetServerInfo(conf);
            }
            catch (Exception ex)
            {
                this.StatusMessage = "Cannot Connect To Server, Check Login: ";
                this.StatusMessage += ex.Message + "\r\n" + ex.StackTrace;
                return;
            }

            var projects = this.service.GetProjectsList(this.conf);
            var tmpProjectResources = new List<SonarProject>();
            foreach (var project in projects)
            {
                tmpProjectResources.Add(new SonarProject(project, this.service, this.conf));
            }

            this.ProjectResources = tmpProjectResources;

            this.StatusMessage = "Logged to Sonar: " + Convert.ToString(this.SonarVersion) + " " + this.conf.Hostname + " User: " + this.conf.Username;
            this.UserIsLoggedIn = true;
        }

        /// <summary>
        /// The update configuration.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="vsenvironmentHelper">
        /// The vs environment helper.
        /// </param>
        public void UpdateConfiguration(ISonarConfiguration configuration, Resource project, IVsEnvironmentHelper vsenvironmentHelper)
        {
            this.conf = configuration;
            if (this.conf != null)
            {
                this.Login();
            }

            this.SelectMainResource(project);
            this.vshelper = vsenvironmentHelper;
        }

        /// <summary>
        /// The select main resource.
        /// </summary>
        /// <param name="project">
        /// The project.
        /// </param>
        public void SelectMainResource(Resource project)
        {
            if (project == null)
            {
                return;
            }

            this.SelectedFirstProject = project.Lname + "   -    " + project.Lang;
        }

        /// <summary>
        /// The get projects with matching duplications.
        /// </summary>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private List<SonarProject> GetProjectsWithMatchingDuplications(SonarProject project)
        {
            var crossProjectData = new List<SonarProject>();

            foreach (var dup in this.DuplicatedData)
            {
                foreach (var group in dup.DuplicatedGroups)
                {
                    foreach (var block in group.DuplicatedBlocks)
                    {
                        if (project.IsFileFoundInFiles(block.Resource.Key) == null)
                        {
                            foreach (var projectResource in this.ProjectResources)
                            {
                                if (projectResource.IsFileFoundInFiles(block.Resource.Key) != null)
                                {
                                    bool found = false;
                                    foreach (var crossproject in crossProjectData)
                                    {
                                        if (crossproject.Project.Key.Equals(projectResource.Project.Key))
                                        {
                                            found = true;
                                        }
                                    }

                                    if (!found)
                                    {
                                        crossProjectData.Add(projectResource);                                
                                    }                                
                                }
                            }
                        } 

                    }                    
                }                
            }

            return crossProjectData;
        }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string StatusMessage
        {
            get
            {
                return this.statusMessage;
            }

            set
            {
                this.statusMessage = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// Gets or sets the sonar version.
        /// </summary>
        public double SonarVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user is logged in.
        /// </summary>
        public bool UserIsLoggedIn
        {
            get
            {
                return this.userIsLoggedIn;
            }

            set
            {
                this.userIsLoggedIn = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the selected resource in view.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public string SelectedResourceInView
        {
            get
            {
                return this.selectedResourceInView;
            }

            set
            {
                this.selectedResourceInView = value;
                if (value != null)
                {
                    this.DuplicatedGroups = this.GetDuplicateGroupsFromResource(value);
                }

                this.OnPropertyChanged();
            }
        }

        private List<DuplicatedGroup> GetDuplicateGroupsFromResource(string value)
        {
            foreach (var data in this.DuplicatedData)
            {
                if (data.Resource.Key.Equals(value))
                {
                    return data.DuplicatedGroups;
                }                
            }

            return null;
        }

        public string SelectedGroupInView
        {
            get
            {
                return this.selectedGroupInView;
            }

            set
            {
                this.selectedGroupInView = value;
                if (value != null)
                {
                    var i = int.Parse(value);
                    this.DuplicatedBlocks = this.DuplicatedGroups[i].DuplicatedBlocks;
                }

                this.OnPropertyChanged();
            }
        }

        public DuplicatedBlock SelectedBlockInView
        {
            get
            {
                return this.selectedBlockInView;
            }

            set
            {
                this.selectedBlockInView = value;
                if (value == null)
                {
                    return;
                }

                this.SourceCodeTxt = this.GetSourceForDuplicatedBlock(value);
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the updating data.
        /// </summary>
        public bool IsIdle
        {
            get
            {
                return this.isIdle;
            }

            set
            {
                this.isIdle = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("IsBusy");
            }
        }

        public bool IsBusy
        {
            get
            {
                return !this.IsIdle;
            }
        }

        public string SourceCodeTxt
        {
            get
            {
                return this.sourceCodeTxt;
            }

            set
            {
                this.sourceCodeTxt = value;
                this.OnPropertyChanged();
            }
        }

        public SonarProject FirstProject { get; set; }
        public SonarProject SecondProject { get; set; }

        /// <summary>
        /// The update colours.
        /// </summary>
        /// <param name="background">
        /// The background.
        /// </param>
        /// <param name="foreground">
        /// The foreground.
        /// </param>
        public void UpdateColours(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// The set cross duplication data.
        /// </summary>
        /// <param name="mainSonarProject">
        /// The main sonar project.
        /// </param>
        /// <param name="secondProject">
        /// The second project.
        /// </param>
        private void SetCrossDuplicationData(SonarProject mainSonarProject, SonarProject secondProject)
        {
            var bw = new BackgroundWorker();

            bw.DoWork += delegate
            {
                this.IsIdle = false;
                this.DuplicatedData = mainSonarProject.GetDuplicatedBlocks(secondProject);
            };

            bw.RunWorkerCompleted += delegate
            {
                this.OnPropertyChanged();
                this.IsIdle = true;
            };

            bw.RunWorkerAsync();
        }

        /// <summary>
        /// The set duplication data for single project.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        private void SetDuplicationDataForSingleProject(SonarProject project)
        {
            var bw = new BackgroundWorker();

            bw.DoWork += delegate
            {
                this.IsIdle = false;
                if (project == null)
                {
                    return;
                }

                this.DuplicatedData = project.GetDuplicatedData();
                this.CrossProjectResources = this.GetProjectsWithMatchingDuplications(project);
            };

            bw.RunWorkerCompleted += delegate
            {
                this.OnPropertyChanged();
                this.IsIdle = true;
            };

            bw.RunWorkerAsync();
        }

        /// <summary>
        /// The get sonar project project list.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="SonarProject"/>.
        /// </returns>
        private SonarProject GetSonarProjectProjectList(string value)
        {
            return this.projectResources.FirstOrDefault(variable => (variable.Project.Lname + "   -    " + variable.Project.Lang).Equals(value));
        }

        /// <summary>
        /// The get source for duplicated block.
        /// </summary>
        /// <param name="block">
        /// The block.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetSourceForDuplicatedBlock(DuplicatedBlock block)
        {
            if (string.IsNullOrEmpty(this.SelectedSecondProject))
            {
                var sourcedata = this.FirstProject.GetSourceCodeLines(block.Resource.Key, block.Startline, block.Lenght);
                if (!string.IsNullOrEmpty(sourcedata))
                {
                    return sourcedata;
                }

                return this.FindFileInAllProject(block.Resource.Key, this.ProjectResources).GetSourceCodeLines(block.Resource.Key, block.Startline, block.Lenght);
            }

            var source = this.FirstProject.GetSourceCodeLines(block.Resource.Key, block.Startline, block.Lenght);
            if (!string.IsNullOrEmpty(source))
            {
                return source;
            }

            return this.FindFileInAllProject(block.Resource.Key, this.CrossProjectResources).GetSourceCodeLines(block.Resource.Key, block.Startline, block.Lenght);
        }

        /// <summary>
        /// The find file in all project.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="projects">
        /// The projects.
        /// </param>
        /// <returns>
        /// The <see cref="SonarProject"/>.
        /// </returns>
        private SonarProject FindFileInAllProject(string key, IEnumerable<SonarProject> projects)
        {
            return projects.FirstOrDefault(project => project.IsFileFoundInFiles(key) != null);
        }
    }
}
