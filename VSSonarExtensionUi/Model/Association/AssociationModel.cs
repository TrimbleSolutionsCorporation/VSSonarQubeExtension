namespace VSSonarExtensionUi.Model.Association
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows;

    using Helpers;
    using PropertyChanged;
    using SonarLocalAnalyser;
    using View.Association;
    using View.Helpers;
    using ViewModel;
    using ViewModel.Association;
    using ViewModel.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Helpers;
    using VSSonarPlugins.Types;
    using ViewModel.Configuration;

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
        /// The association view model
        /// </summary>
        private readonly AssociationViewModel associationViewModel;

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
        /// Initializes a new instance of the <see cref="AssociationModel" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="service">The service.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="translator">The translator.</param>
        /// <param name="pluginManager">The plugin manager.</param>
        /// <param name="model">The model.</param>
        public AssociationModel(
            INotificationManager logger,
            ISonarRestService service,
            IConfigurationHelper configurationHelper,
            ISQKeyTranslator translator,
            IPluginManager pluginManager,
            SonarQubeViewModel model)
        {
            this.keyTranslator = translator;
            this.pluginManager = pluginManager;
            this.model = model;
            this.configurationHelper = configurationHelper;
            this.logger = logger;
            this.sonarService = service;

            // start view model
            this.associationViewModel = new AssociationViewModel(this);
        }

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
        /// Gets or sets the selected project.
        /// </summary>
        /// <value>
        /// The selected project.
        /// </value>
        public Resource SelectedProject { get; set; }

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
                    this.sourceControl = new SourceControlModel(this.pluginManager.SourceCodePlugins, this.OpenSolutionPath);
                }

                return this.sourceControl;
            }
        }

        /// <summary>
        /// Registers the new model in pool.
        /// </summary>
        /// <param name="model">The model.</param>
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

            if (project == null)
            {
                return false;
            }

            if (project.IsBranch && branchProject == null)
            {
                return false;
            }

            this.model.AvailableBranches.Clear();

            if (project.IsBranch && branchProject != null)
            {
                this.AssociatedProject = branchProject;

                foreach (var branch in projectIn.BranchResources)
                {
                    branch.Default = false;
                    this.model.AvailableBranches.Add(branch);
                }

                branchProject.Default = true;
                this.model.IsBranchSelectionEnabled = true;
                this.model.SelectedBranch = branchProject;
            }
            else
            {
                this.AssociatedProject = project;
                this.model.IsBranchSelectionEnabled = false;
                this.model.AvailableBranches.Add(project);
                this.model.SelectedBranch = project;
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
                this.keyTranslator.SetProjectKeyAndBaseDir(this.AssociatedProject.Key, this.OpenSolutionPath);
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
            return this.SourceControl.GetBranch();
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

            var key = this.keyTranslator.TranslatePath(this.vshelper.VsFileItem(fullName, project, null), this.vshelper);
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

            var vsitem = this.vshelper.VsFileItem(fullName, project, null);
            if (vsitem == null)
            {
                this.keyTranslator.SetLookupType(KeyLookUpType.Invalid);
                return null;
            }

            if (this.keyTranslator.GetLookupType() == KeyLookUpType.Invalid)
            {
                foreach (KeyLookUpType type in Enum.GetValues(typeof(KeyLookUpType)))
                {
                    if (type != KeyLookUpType.Invalid)
                    {
                        this.keyTranslator.SetLookupType(type);

                        var key = this.keyTranslator.TranslatePath(vsitem, this.vshelper);
                        if (!string.IsNullOrEmpty(key))
                        {
                            var item = this.ValidateResourceInServer(fullName, key);
                            if (item != null)
                            {
                                return item;
                            }
                        }
                    }
                }

                this.keyTranslator.SetLookupType(KeyLookUpType.Invalid);
                return null;
            }
            else
            {
                var key = this.keyTranslator.TranslatePath(this.vshelper.VsFileItem(fullName, project, null), this.vshelper);
                return this.ValidateResourceInServer(fullName, key);
            }
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
        public void StartAutoAssociation(string solutionName, string solutionPath)
        {
            if (!this.model.IsConnected || (this.OpenSolutionName == solutionName && this.OpenSolutionPath == solutionPath))
            {
                return;
            }

            this.OpenSolutionName = solutionName;
            this.OpenSolutionPath = solutionPath;

            // create a new source control provider for solution
            this.sourceControl = new SourceControlModel(this.pluginManager.SourceCodePlugins, this.OpenSolutionPath);

            if (this.model.IsConnected)
            {
                Resource solResource = this.GetResourceForSolution(solutionName, solutionPath);

                if (solResource != null)
                {
                    foreach (Resource availableProject in this.associationViewModel.AvailableProjects)
                    {
                        if (availableProject.Key.Equals(solResource.Key))
                        {
                            if (availableProject.IsBranch)
                            {
                                var branchName = this.CurrentBranch().Replace("/", "_");
                                foreach (var branch in availableProject.BranchResources)
                                {
                                    if (branch.Name.EndsWith(branchName))
                                    {
                                        availableProject.SolutionRoot = solutionPath;
                                        branch.SolutionRoot = solutionPath;
                                        branch.Default = true;
                                        this.associationViewModel.SelectedProject = availableProject;
                                        this.associationViewModel.SelectedBranchProject = branch;
                                        this.AssignASonarProjectToSolution(availableProject, branch);
                                        this.IsConnectedButNotAssociated = false;
                                        this.IsAssociated = true;
                                        this.CreateConfiguration(Path.Combine(solutionPath, "sonar-project.properties"));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                availableProject.SolutionRoot = solutionPath;
                                this.associationViewModel.SelectedProject = availableProject;
                                this.AssignASonarProjectToSolution(availableProject, null);
                                this.IsConnectedButNotAssociated = false;
                                this.IsAssociated = true;
                                this.CreateConfiguration(Path.Combine(solutionPath, "sonar-project.properties"));
                            }

                            return;
                        }

                        if (availableProject.IsBranch)
                        {
                            foreach (var branch in availableProject.BranchResources)
                            {
                                if (branch.Key.Equals(solResource.Key))
                                {
                                    availableProject.SolutionRoot = solutionPath;
                                    branch.SolutionRoot = solutionPath;
                                    this.associationViewModel.SelectedProject = availableProject;
                                    this.associationViewModel.SelectedBranchProject = branch;
                                    this.AssignASonarProjectToSolution(availableProject, branch);
                                    this.IsConnectedButNotAssociated = false;
                                    this.IsAssociated = true;
                                    this.CreateConfiguration(Path.Combine(solutionPath, "sonar-project.properties"));
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    this.ShowAssociationWindow();
                }
            }
        }


        /// <summary>
        /// Shows the association window.
        /// </summary>
        public void ShowAssociationWindow()
        {
            if (!Thread.CurrentThread.GetApartmentState().Equals(ApartmentState.STA))
            {
                Thread thread = new Thread(() => this.LaunchAssociationWindow(true));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
                return;
            }

            this.LaunchAssociationWindow(false);
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
            this.associationViewModel.AvailableProjects.Clear();
            this.associationViewModel.SelectedProject = null;
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

                foreach (var project in this.associationViewModel.AvailableProjects)
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
                    UserExceptionMessageBox.ShowException("Associated Project does not exist in server, please configure association", ex);
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
            List<Resource> projects = new List<Resource>();

            foreach (var rawitem in projectsRaw)
            {
                var nameWithoutBrachRaw = rawitem.Name.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                bool added = false;

                if (nameWithoutBrachRaw.Length == 2)
                {
                    bool projectFound = false;
                    foreach (var processedItem in projects)
                    {
                        var nameWithoutBrachProcessed = processedItem.Name.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

                        if (nameWithoutBrachRaw[0].Equals(nameWithoutBrachProcessed[0]))
                        {
                            projectFound = true;
                        }
                    }

                    if (!projectFound)
                    {
                        // create master branch
                        var mainProject = new Resource();
                        mainProject.Name = nameWithoutBrachRaw[0];
                        mainProject.Key = nameWithoutBrachRaw[0] + "_Main";
                        mainProject.IsBranch = true;
                        projects.Add(mainProject);
                        added = true;
                    }
                }

                foreach (var processedItem in projects)
                {
                    var nameWithoutBrachProcessed = processedItem.Name.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

                    if (nameWithoutBrachRaw[0].Equals(nameWithoutBrachProcessed[0]) && nameWithoutBrachRaw.Length == 2)
                    {
                        rawitem.BranchName = nameWithoutBrachRaw[1];
                        processedItem.BranchResources.Add(rawitem);
                        added = true;
                    }
                }

                if (!added)
                {
                    rawitem.BranchName = rawitem.Name;
                    projects.Add(rawitem);
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
                            this.associationViewModel.AvailableProjects.Clear();
                            foreach (Resource source in projects.OrderBy(i => i.Name))
                            {
                                this.associationViewModel.AvailableProjects.Add(source);
                            }
                        });
                }
                else
                {
                    this.associationViewModel.AvailableProjects.Clear();
                    foreach (Resource source in projects.OrderBy(i => i.Name))
                    {
                        this.associationViewModel.AvailableProjects.Add(source);
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
        /// Launches the association window.
        /// </summary>
        /// <param name="useDispatcher">if set to <c>true</c> [use dispatcher].</param>
        private void LaunchAssociationWindow(bool useDispatcher)
        {
            if (useDispatcher)
            {
                Application.Current.Dispatcher.Invoke(
                    delegate
                    {
                        this.associationViewModel.UpdateColours(this.model.BackGroundColor, this.model.ForeGroundColor);
                        var window = new AssociationView(this.associationViewModel);
                        window.ShowDialog();
                    });
            }
            else
            {
                this.associationViewModel.UpdateColours(this.model.BackGroundColor, this.model.ForeGroundColor);
                var window = new AssociationView(this.associationViewModel);
                window.ShowDialog();
            }
        }

        /// <summary>
        /// Validates the resource in server.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="key">The key.</param>
        /// <returns>valid resource in server</returns>
        private Resource ValidateResourceInServer(string fullName, string key)
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
                AuthtenticationHelper.AuthToken,
                this.AssociatedProject,
                this.OpenSolutionPath,
                this.SourceControl);

            foreach (IModelBase model in modelPool)
            {
                try
                {
                    model.AssociateWithNewProject(
                        AuthtenticationHelper.AuthToken,
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
    }
}