namespace VSSonarExtensionUi.Association
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows;

    using PropertyChanged;
    using SonarLocalAnalyser;
    using View.Helpers;
    using ViewModel;
    using VSSonarPlugins;
    using VSSonarPlugins.Helpers;
    using VSSonarPlugins.Types;
    using System.Collections.ObjectModel;
    using VSSonarExtensionUi.Model.Helpers;
    using VSSonarExtensionUi.ViewModel.Configuration;
    
    /// <summary>
    /// Generates associations with sonar projects
    /// </summary>
    [ImplementPropertyChanged]
    public class AssociationModel
    {
        /// <summary>
        /// The model pool
        /// </summary>
        private static List<IModelBase> modelPool = new List<IModelBase>();

        /// <summary>
        /// The logger
        /// </summary>
        private readonly INotificationManager logger;

        /// <summary>
        /// The plugin manager
        /// </summary>
        private readonly IPluginManager pluginManager;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        /// The model
        /// </summary>
        private readonly SonarQubeViewModel model;

        /// <summary>
        /// The key translator
        /// </summary>
        private readonly ISQKeyTranslator keyTranslator;

        /// <summary>
        /// The sonar service
        /// </summary>
        private readonly ISonarRestService sonarService;

        /// <summary>
        /// The vshelper
        /// </summary>
        private IVsEnvironmentHelper vshelper;

        /// <summary>
        /// The source control
        /// </summary>
        private ISourceControlProvider sourceControl;

        /// <summary>
        /// The current branch name
        /// </summary>
        private string currentBranchName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssociationModel" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="service">The service.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="translator">The translator.</param>
        /// <param name="pluginManager">The plugin manager.</param>
        /// <param name="model">The </param>
        public AssociationModel(
            INotificationManager logger,
            ISonarRestService service,
            IConfigurationHelper configurationHelper,
            ISQKeyTranslator translator,
            IPluginManager pluginManager,
            SonarQubeViewModel model,
            ISourceControlProvider control = null)
        {
            this.keyTranslator = translator;
            this.pluginManager = pluginManager;
            this.model = model;
            this.configurationHelper = configurationHelper;
            this.logger = logger;
            this.sonarService = service;
            this.sourceControl = control;

            this.AvailableProjects = new ObservableCollection<Resource>();
        }

        /// <summary>
        ///     Gets or sets the available projects.
        /// </summary>
        public ObservableCollection<Resource> AvailableProjects { get; set; }

        /// <summary>
        ///     Gets or sets the selected project.
        /// </summary>
        public Resource SelectedProjectInView { get; set; }

        /// <summary>
        /// Gets or sets the selected branch project.
        /// </summary>
        /// <value>
        /// The selected branch project.
        /// </value>
        public Resource SelectedBranchProject { get; set; }

        /// <summary>
        ///     Gets or sets the open solution path.
        /// </summary>
        public string OpenSolutionPath { get; set; }

        /// <summary>
        ///     Gets or sets the open solution name.
        /// </summary>
        public string OpenSolutionName { get; set; }

        /// <summary>
        ///     Gets or sets the associated project.
        /// </summary>
        public Resource AssociatedProject { get; set; }

        /// <summary>
        /// Gets or sets the name of the selected project.
        /// </summary>
        /// <value>
        /// The name of the selected project.
        /// </value>
        public string SelectedProjectName { get; set; }

        /// <summary>
        /// Gets or sets the selected project key.
        /// </summary>
        /// <value>
        /// The selected project key.
        /// </value>
        public string SelectedProjectKey { get; set; }

        /// <summary>
        /// Gets or sets the selected project version.
        /// </summary>
        /// <value>
        /// The selected project version.
        /// </value>
        public string SelectedProjectVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is associated.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is associated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAssociated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is connected but not associated.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is connected but not associated; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnectedButNotAssociated { get; set; }

        /// <summary>
        /// Gets the source control.
        /// </summary>
        /// <value>
        /// The source control.
        /// </value>
        public ISourceControlProvider SourceControl
        {
            get
            {
                if (this.sourceControl == null)
                {
                    // create a new source control provider for solution
                    this.sourceControl = new SourceControlModel(this.pluginManager.SourceCodePlugins, this.OpenSolutionPath, this.logger);
                }

                return this.sourceControl;
            }
        }

        /// <summary>
        /// Registers the new model in pool.
        /// </summary>
        /// <param name="model">The </param>
        public static void RegisterNewModelInPool(IModelBase model)
        {
            modelPool.Add(model);
        }

        /// <summary>
        /// The on assign project command.
        /// </summary>
        /// <param name="projectIn">The project in.</param>
        /// <param name="branchProject">The branch project.</param>
        /// <returns>
        /// Ok if assign.
        /// </returns>
        public bool AssignASonarProjectToSolution(Resource projectIn, Resource branchProject)
        {
            var project = projectIn;
            this.currentBranchName = string.Empty;

            if (project == null)
            {
                return false;
            }

            if (project.IsBranch && branchProject == null)
            {
                return false;
            }

            if (project.IsBranch && branchProject != null)
            {
                this.AssociatedProject = branchProject;

                foreach (var branch in projectIn.BranchResources)
                {
                    branch.Default = false;
                }

                branchProject.Default = true;
                this.currentBranchName = branchProject.BranchName;
            }
            else
            {
                this.AssociatedProject = project;
            }

            this.model.ConnectionTooltip = "Connected and Associated";
            this.IsAssociated = true;

            if (!string.IsNullOrEmpty(this.OpenSolutionName))
            {
                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties
                    {
                        Owner = Path.Combine(this.vshelper.ActiveSolutionPath(), this.vshelper.ActiveSolutionName() + ".sln"),
                        Key = "PROJECTKEY",
                        Value = project.Key,
                        Context = Context.GlobalPropsId
                    });

                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties
                    {
                        Owner = Path.Combine(this.vshelper.ActiveSolutionPath(), this.vshelper.ActiveSolutionName() + ".sln"),
                        Key = "PROJECTNAME",
                        Value = this.OpenSolutionName,
                        Context = Context.GlobalPropsId
                    });

                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties
                    {
                        Owner = Path.Combine(this.vshelper.ActiveSolutionPath(), this.vshelper.ActiveSolutionName() + ".sln"),
                        Key = "PROJECTLOCATION",
                        Value = this.OpenSolutionPath,
                        Context = Context.GlobalPropsId
                    });

                this.AssociatedProject.SolutionRoot = this.OpenSolutionPath;
                this.keyTranslator.SetProjectKeyAndBaseDir(this.AssociatedProject.Key, this.OpenSolutionPath, this.currentBranchName);
            }

            this.configurationHelper.SyncSettings();
            this.UpdateAssociationData();
            return true;
        }

        /// <summary>
        /// Currents the branch.
        /// </summary>
        /// <returns>return current branch</returns>
        public string CurrentBranch()
        {
            var branch = this.SourceControl.GetBranch();
            if (string.IsNullOrEmpty(branch))
            {
                return "master";
            }

            return branch;
        }

        /// <summary>
        /// Creates the resource path file.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="project">The project.</param>
        /// <returns>retruns resource for given path</returns>
        public Resource CreateResourcePathFile(string fullName, Resource project)
        {
            if (this.vshelper == null)
            {
                return null;
            }

            var key = this.keyTranslator.TranslatePath(this.vshelper.VsFileItem(fullName, project, null), this.vshelper, this.sonarService, AuthtenticationHelper.AuthToken);
            var localRes = new Resource();
            localRes.Key = key;
            localRes.Scope = "FIL";
            string fileName = Path.GetFileName(fullName);
            localRes.Name = fileName;
            localRes.Lname = fileName;
            return localRes;
        }

        /// <summary>
        /// The create a resource for file in editor.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="project">The project.</param>
        /// <returns>
        /// The <see cref="Resource" />.
        /// </returns>
        public Resource CreateAValidResourceFromServer(string fullName, Resource project)
        {
            if (this.vshelper == null)
            {
                return null;
            }

            var key = this.keyTranslator.TranslatePath(this.vshelper.VsFileItem(fullName, project, null), this.vshelper, this.sonarService, AuthtenticationHelper.AuthToken);
            return this.CreateResourceForKey(fullName, key);
        }

        /// <summary>
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public void UpdateServicesInModels(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            this.vshelper = vsenvironmenthelperIn;
            foreach (IModelBase model in modelPool)
            {
                model.UpdateServices(vsenvironmenthelperIn, statusBar, provider);
            }
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void EndAssociationAndClearData()
        {
            this.OpenSolutionName = null;
            this.OpenSolutionPath = null;
            this.SelectedProjectInView = null;
            this.AssociatedProject = null;
            this.IsConnectedButNotAssociated = this.model.IsConnected;
            this.IsAssociated = !this.model.IsConnected;

            foreach (var item in modelPool)
            {
                item.EndDataAssociation();
            }
        }

        /// <summary>
        /// Starts the automatic association.
        /// </summary>
        /// <param name="solutionName">Name of the solution.</param>
        /// <param name="solutionPath">The solution path.</param>
        /// <returns>if association window should be open or closed</returns>
        public void StartAutoAssociation(string solutionName, string solutionPath)
        {
            if (!this.model.IsConnected)
            {
                this.model.StatusMessageAssociation = "Not connected, no association possible.";
                this.model.ShowRightFlyout = true;
                return;
            }

            if (!this.model.IsConnected || (this.OpenSolutionName == solutionName && this.OpenSolutionPath == solutionPath))
            {
                this.model.ShowRightFlyout = false;
                return;
            }

            this.OpenSolutionName = solutionName;
            this.OpenSolutionPath = solutionPath;

            // create a new source control provider for solution
            this.sourceControl = new SourceControlModel(this.pluginManager.SourceCodePlugins, this.OpenSolutionPath, this.logger);

            Resource solResource = this.GetResourceForSolution(solutionName, solutionPath);

            if (solResource != null)
            {
                foreach (Resource availableProject in this.AvailableProjects)
                {
                    if (availableProject.Key.Equals(solResource.Key))
                    {
                        if (availableProject.IsBranch)
                        {
                            var branchName = this.CurrentBranch().Replace("/", "_");
                            Resource masterBranch = null;

                            foreach (var branch in availableProject.BranchResources)
                            {
                                if (branch.BranchName.ToLower().Equals("master"))
                                {
                                    masterBranch = branch;
                                }

                                if (branch.Name.EndsWith(branchName))
                                {
                                    availableProject.SolutionRoot = solutionPath;
                                    branch.SolutionRoot = solutionPath;
                                    branch.Default = true;
                                    this.AssociatedProject = availableProject;
                                    this.SelectedBranchProject = branch;
                                    this.AssignASonarProjectToSolution(availableProject, branch);
                                    this.IsConnectedButNotAssociated = false;
                                    this.IsAssociated = true;
                                    this.CreateConfiguration(Path.Combine(solutionPath, "sonar-project.properties"));
                                    this.SelectedProjectInView = this.AssociatedProject;
                                    this.model.StatusMessageAssociation = "Associated with: " + this.AssociatedProject.Name;
                                    this.model.ShowRightFlyout = false;
                                    return;
                                }
                            }

                            if (masterBranch != null)
                            {
                                this.AssociatedProject = masterBranch;
                                this.SelectedProjectInView = this.AssociatedProject;
                                availableProject.SolutionRoot = solutionPath;
                                this.model.StatusMessageAssociation = "Associated with master branch because local branch not found in server.";
                                this.model.ShowRightFlyout = false;
                                this.IsConnectedButNotAssociated = false;
                                this.IsAssociated = true;
                                return;
                            }

                            this.model.StatusMessageAssociation = "No automatic association possible, no local or master branch detected in server.";
                            this.model.ShowRightFlyout = true;
                            return;
                        }
                        else
                        {
                            availableProject.SolutionRoot = solutionPath;
                            this.AssociatedProject = availableProject;
                            this.AssignASonarProjectToSolution(availableProject, null);
                            this.IsConnectedButNotAssociated = false;
                            this.IsAssociated = true;
                            this.CreateConfiguration(Path.Combine(solutionPath, "sonar-project.properties"));
                            this.SelectedProjectInView = this.AssociatedProject;
                            this.model.StatusMessageAssociation = "Associated with: " + this.AssociatedProject.Name;
                            this.model.ShowRightFlyout = false;
                            return;
                        }
                    }

                    if (availableProject.IsBranch)
                    {
                        foreach (var branch in availableProject.BranchResources)
                        {
                            if (branch.Key.Equals(solResource.Key))
                            {
                                availableProject.SolutionRoot = solutionPath;
                                branch.SolutionRoot = solutionPath;
                                this.AssociatedProject = availableProject;
                                this.SelectedBranchProject = branch;
                                this.AssignASonarProjectToSolution(availableProject, branch);
                                this.IsConnectedButNotAssociated = false;
                                this.IsAssociated = true;
                                this.CreateConfiguration(Path.Combine(solutionPath, "sonar-project.properties"));
                                this.SelectedProjectInView = this.AssociatedProject;
                                this.model.StatusMessageAssociation = "Associated with: " + this.AssociatedProject.Name;
                                this.model.ShowRightFlyout = false;
                                return;
                            }
                        }
                    }
                }
            }

            this.model.StatusMessageAssociation = "No automatic association possible, project found in server that corresponds to this solution.";
            this.model.ShowRightFlyout = true;
        }

        /// <summary>The associate project to solution.</summary>
        /// <param name="solutionName">The solution Name.</param>
        /// <param name="solutionPath">The solution Path.</param>
        public void AssociateProjectToSolution(string solutionName, string solutionPath)
        {
            var solution = solutionName;

            if (string.IsNullOrEmpty(solution))
            {
                return;
            }

            if (!solution.ToLower().EndsWith(".sln"))
            {
                solution += ".sln";
            }

            this.StartAutoAssociation(solution, solutionPath);
            
            if (this.IsAssociated)
            {
                this.UpdateAssociationData();
            }
        }

        /// <summary>
        /// Updates the branch selection.
        /// </summary>
        /// <param name="selectedBranch">The selected branch.</param>
        public void UpdateBranchSelection(Resource selectedBranch)
        {
            this.keyTranslator.SetProjectKey(selectedBranch.Key);
            this.AssociatedProject = selectedBranch;
            this.UpdateAssociationData();
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            this.model.IsConnected = false;
            this.OpenSolutionName = string.Empty;
            this.OpenSolutionPath = string.Empty;
            if (this.SelectedProjectInView != null)
            {
                this.SelectedProjectInView.Name = "";
                this.SelectedProjectInView.Key = "";
                this.SelectedProjectInView.BranchName = "";
                this.SelectedProjectInView.BranchResources.Clear();
            }

            if (this.AvailableProjects != null)
            {
                this.AvailableProjects.Clear();
            }

            this.SelectedBranchProject = null;
            this.SelectedProjectInView = null;
            this.AssociatedProject = null;
            this.keyTranslator.SetLookupType(KeyLookUpType.Invalid);
        }

        /// <summary>Generates a resource name from a given solution, it will bring Associtation View
        ///  in case automatic associations is not possible</summary>
        /// <param name="solutionName">The solution Name.</param>
        /// <param name="solutionPath">The solution Path.</param>
        /// <returns>The <see cref="Resource"/>.</returns>
        public Resource GetResourceForSolution(string solutionName, string solutionPath)
        {
            if (AuthtenticationHelper.AuthToken == null)
            {
                return null;
            }

            try
            {
                var prop = this.configurationHelper.ReadSetting(
                    Context.GlobalPropsId,
                    Path.Combine(solutionPath, solutionName),
                    "PROJECTKEY");

                foreach (var project in this.AvailableProjects)
                {
                    if (project.Key.Equals(prop.Value))
                    {
                        return project;
                    }
                }

                try
                {
                    return this.sonarService.GetResourcesData(AuthtenticationHelper.AuthToken, prop.Value)[0];
                }
                catch (Exception ex)
                {
                    this.model.StatusMessageAssociation = "Associated Project does not exist in server, please configure association";
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            var sourceKey = VsSonarUtils.GetProjectKey(solutionPath);
            try
            {
                return string.IsNullOrEmpty(sourceKey) ? null : this.sonarService.GetResourcesData(AuthtenticationHelper.AuthToken, sourceKey)[0];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                sourceKey = sourceKey + ":" + this.SourceControl.GetBranch().Replace("/", "_");
                try
                {
                    return string.IsNullOrEmpty(sourceKey) ? null : this.sonarService.GetResourcesData(AuthtenticationHelper.AuthToken, sourceKey)[0];
                }
                catch (Exception exceptionBranch)
                {
                    Debug.WriteLine(exceptionBranch.Message);
                    return null;
                }
            }
        }

        /// <summary>
        /// Refreshes the project list.
        /// </summary>
        /// <param name="useDispatcher">if set to <c>true</c> [use dispatcher].</param>
        public void RefreshProjectList(bool useDispatcher)
        {
            List<Resource> projectsRaw = this.sonarService.GetProjectsList(AuthtenticationHelper.AuthToken);
            SortedDictionary<string, Resource> projects = new SortedDictionary<string, Resource>();

            foreach (var rawitem in projectsRaw)
            {
                if (rawitem.IsBranch)
                {
                    var nameWithoutBrachRaw = rawitem.Name.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    var keyDic = nameWithoutBrachRaw[0] + "_Main";
                    if (!projects.ContainsKey(keyDic))
                    {
                        // create main project holder
                        var mainProject = new Resource();
                        mainProject.Name = nameWithoutBrachRaw[0];
                        mainProject.Key = keyDic;
                        mainProject.IsBranch = true;
                        projects.Add(keyDic, mainProject);
                    }

                    var element = projects[keyDic];
                    element.BranchResources.Add(rawitem);
                }
                else
                {
                    projects.Add(rawitem.Key, rawitem);
                }
            }

            if (projects != null && projects.Count > 0)
            {
                this.model.IsConnected = true;
                this.IsConnectedButNotAssociated = false;
                this.IsAssociated = false;

                if (useDispatcher)
                {
                    Application.Current.Dispatcher.Invoke(
                        delegate
                        {
                            this.AvailableProjects.Clear();
                            foreach (var source in projects)
                            {
                                this.AvailableProjects.Add(source.Value);
                            }
                        });
                }
                else
                {
                    this.AvailableProjects.Clear();
                    foreach (var source in projects)
                    {
                        this.AvailableProjects.Add(source.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Creates the configuration.
        /// </summary>
        /// <param name="pathForPropertiesFile">The path for properties file.</param>
        private void CreateConfiguration(string pathForPropertiesFile)
        {
            this.keyTranslator.CreateConfiguration(pathForPropertiesFile);
        }

        /// <summary>
        /// Validates the resource in server.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="key">The key.</param>
        /// <returns>valid resource in server</returns>
        private Resource CreateResourceForKey(string fullName, string key)
        {
            try
            {
                this.logger.WriteMessage(this.AssociatedProject.Key + " = Lookup Resource: " + fullName + " with Key: " + key);
                Resource toReturn =
                    this.sonarService.GetResourcesData(
                        AuthtenticationHelper.AuthToken,
                        key)[0];
                string fileName = Path.GetFileName(fullName);
                toReturn.Name = fileName;
                toReturn.Lname = fileName;
                this.logger.WriteMessage("Resource found: " + toReturn.Key);
                return toReturn;
            }
            catch (Exception ex)
            {
                this.logger.WriteMessage("Resource not found for: " + fullName);
                this.logger.WriteException(ex);
            }

            this.logger.WriteMessage("Resource not found in Server");

            return null;
        }

        /// <summary>
        /// Updates the services.
        /// </summary>
        private void UpdateAssociationData()
        {
            var listToRemo = new List<IModelBase>();

            // associate
            this.pluginManager.AssociateWithNewProject(
                this.AssociatedProject,
                this.OpenSolutionPath,
                this.SourceControl);

            this.keyTranslator.SetLookupType(KeyLookUpType.Invalid);
            this.keyTranslator.SetProjectKeyAndBaseDir(this.AssociatedProject.Key, this.OpenSolutionPath, this.currentBranchName);

            foreach (IModelBase model in modelPool)
            {
                try
                {
                    model.AssociateWithNewProject(
                        this.AssociatedProject,
                        this.OpenSolutionPath,
                        this.SourceControl,
                        this.pluginManager.GetIssueTrackerPlugin());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    listToRemo.Add(model);
                }
            }

            foreach (var item in listToRemo)
            {
                modelPool.Remove(item);
            }
        }

        public Resource SelectBranchFromList(Resource resource)
        {
            var branch = this.SourceControl.GetBranch().Replace("/", "_");
            Resource masterBranch = null;
            foreach (var branchdata in this.SelectedProjectInView.BranchResources)
            {
                if (branchdata.BranchName.Equals(branch))
                {
                    this.model.StatusMessageAssociation = "Association Ready. Press associate to confirm.";
                    return branchdata;
                }

                if (branchdata.BranchName.Equals("master"))
                {
                    masterBranch = branchdata;
                }
            }

            if (masterBranch != null)
            {
                this.model.StatusMessageAssociation = "Using master branch, because current branch does not exist or source control not supported. Press associate to confirm.";
                return masterBranch;
            }

            this.model.StatusMessageAssociation = "Unable to find branch, please manually choose one from list and confirm.";
            return null;
        }

        /// <summary>
        /// Called when [selected project changed].
        /// </summary>
        private void OnSelectedProjectInViewChanged()
        {
            if (this.SelectedProjectInView == null)
            {
                this.SelectedProjectName = "";
                this.SelectedProjectKey = "";
                this.SelectedProjectVersion = "";
                this.model.StatusMessageAssociation = "No project selected, select from above.";
                return;
            }

            this.SelectedProjectName = this.SelectedProjectInView.Name;
            this.SelectedProjectKey = this.SelectedProjectInView.Key;
            this.SelectedProjectVersion = this.SelectedProjectInView.Version;

            if (this.SelectedProjectInView.IsBranch)
            {
                this.SelectedBranchProject = this.SelectBranchFromList(this.SelectedProjectInView);
            }
            else
            {
                this.SelectedBranchProject = null;
                this.model.StatusMessageAssociation = "Normal project type. Press associate to confirm.";
            }
        }
    }
}