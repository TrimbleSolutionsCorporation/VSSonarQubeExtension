namespace VSSonarExtensionUi.Model.Association
{
    using SonarLocalAnalyser;
    using System;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using ViewModel.Helpers;
    using System.IO;
    using View.Helpers;
    using System.Diagnostics;
    using VSSonarPlugins.Helpers;
    using Helpers;
    using ViewModel.Association;
    using System.Collections.Generic;
    using System.Windows;
    using System.Linq;
    using ViewModel;
    using View.Association;
    using PropertyChanged;
    using System.Threading;

    [ImplementPropertyChanged]
    public class AssociationModel
    {
        private readonly ISQKeyTranslator keyTranslator;
        private readonly IVsSonarExtensionLogger logger;
        private readonly IConfigurationHelper configurationHelper;
        private readonly AssociationViewModel associationViewModel;
        private readonly SonarQubeViewModel model;
        private readonly ISourceControlProvider sourceControl;

        public AssociationModel(ISQKeyTranslator translator,
            IVsSonarExtensionLogger logger,
            ISonarRestService service,
            IConfigurationHelper configurationHelper,
            ISourceControlProvider sourceControl,
            SonarQubeViewModel model)
        {
            this.configurationHelper = configurationHelper;
            this.keyTranslator = translator;
            this.logger = logger;
            this.SonarService = service;
            this.model = model;
            this.sourceControl = sourceControl;

            // start view model
            this.associationViewModel = new AssociationViewModel(this);
        }

        public IVsEnvironmentHelper VsHelper { get; set; }

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

        public void OnAssociatedProjectChanged()
        {
        }


        public Resource SelectedProject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is associated.
        /// </summary>
        public bool IsAssociated { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is connected but not associated.
        /// </summary>
        public bool IsConnectedButNotAssociated { get; set; }
        public IVSSStatusBar StatusBar { get; private set; }
        public IServiceProvider Provider { get; private set; }
        public ISonarRestService SonarService { get; private set; }

        internal void CreateConfiguration(string pathForPropertiesFile)
        {
            this.keyTranslator.CreateConfiguration(pathForPropertiesFile);
        }

        /// <summary>
        ///     The on assign project command.
        /// </summary>
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
                        Owner = Path.Combine(this.VsHelper.ActiveSolutionPath(), this.VsHelper.ActiveSolutionName() + ".sln"),
                        Key = "PROJECTKEY",
                        Value = project.Key,
                        Context = Context.GlobalPropsId
                    });

                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties
                    {
                        Owner = Path.Combine(this.VsHelper.ActiveSolutionPath(), this.VsHelper.ActiveSolutionName() + ".sln"),
                        Key = "PROJECTNAME",
                        Value = this.OpenSolutionName,
                        Context = Context.GlobalPropsId
                    });

                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties
                    {
                        Owner = Path.Combine(this.VsHelper.ActiveSolutionPath(), this.VsHelper.ActiveSolutionName() + ".sln"),
                        Key = "PROJECTLOCATION",
                        Value = this.OpenSolutionPath,
                        Context = Context.GlobalPropsId
                    });

                this.AssociatedProject.SolutionRoot = this.OpenSolutionPath;
                this.keyTranslator.SetProjectKeyAndBaseDir(this.AssociatedProject.Key, this.OpenSolutionPath);
            }

            this.configurationHelper.SyncSettings();
            this.model.SyncSettings();
            return true;
        }

        public string CurrentBranch()
        {
            return this.sourceControl.GetBranch(this.OpenSolutionPath);
        }

        public Resource CreateResourcePathFile(string fullName, Resource project)
        {
            if (this.VsHelper == null)
            {
                return null;
            }

            var key = this.keyTranslator.TranslatePath(this.VsHelper.VsFileItem(fullName, project, null), this.VsHelper);
            var localRes = new Resource();
            localRes.Key = key;
            localRes.Scope = "FIL";
            string fileName = Path.GetFileName(fullName);
            localRes.Name = fileName;
            localRes.Lname = fileName;
            return localRes;
        }

        /// <summary>The create a resource for file in editor.</summary>
        /// <param name="fullName">The full name.</param>
        /// <returns>The <see cref="Resource"/>.</returns>
        public Resource CreateAValidResourceFromServer(string fullName, Resource project)
        {
            if (this.VsHelper == null)
            {
                return null;
            }

            var vsitem = this.VsHelper.VsFileItem(fullName, project, null);
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

                        var key = this.keyTranslator.TranslatePath(vsitem, this.VsHelper);
                        if (!string.IsNullOrEmpty(key))
                        {
                            var item = ValidateResourceInServer(fullName, key);
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
                var key = this.keyTranslator.TranslatePath(this.VsHelper.VsFileItem(fullName, project, null), this.VsHelper);
                return ValidateResourceInServer(fullName, key);
            }
        }

        private Resource ValidateResourceInServer(string fullName, string key)
        {
            try
            {
                this.logger.WriteMessage(this.AssociatedProject.Key  + " = Lookup Resource: " + fullName + " with Key: " +  key);
                Resource toReturn =
                    this.SonarService.GetResourcesData(
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
                    return this.SonarService.GetResourcesData(AuthtenticationHelper.AuthToken, prop.Value)[0];
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
                return string.IsNullOrEmpty(sourceKey) ? null : this.SonarService.GetResourcesData(AuthtenticationHelper.AuthToken, sourceKey)[0];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                sourceKey = sourceKey + ":" + this.sourceControl.GetBranch(solutionPath).Replace("/", "_");
                try
                {
                    return string.IsNullOrEmpty(sourceKey) ? null : this.SonarService.GetResourcesData(AuthtenticationHelper.AuthToken, sourceKey)[0];
                }
                catch (Exception exceptionBranch)
                {
                    Debug.WriteLine(exceptionBranch.Message);
                    return null;
                }
            }
        }

        internal void StartAutoAssociation(string solutionName, string solutionPath)
        {

            if (!this.model.IsConnected || (this.OpenSolutionName == solutionName && this.OpenSolutionPath == solutionPath))
            {
                return;
            }

            this.OpenSolutionName = solutionName;
            this.OpenSolutionPath = solutionPath;

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

        internal void ClearProjectAssociation()
        {
            this.OpenSolutionName = null;
            this.OpenSolutionPath = null;
            this.IsConnectedButNotAssociated = this.model.IsConnected;
            this.IsAssociated = !this.model.IsConnected;
        }

        internal void Disconnect()
        {
            this.model.IsConnected = false;
            this.OpenSolutionName = string.Empty;
            this.OpenSolutionPath = string.Empty;
            this.associationViewModel.AvailableProjects.Clear();
            this.associationViewModel.SelectedProject = null;
            this.AssociatedProject = null;
            this.keyTranslator.SetLookupType(KeyLookUpType.Invalid);
        }

        internal void ShowAssociationWindow()
        {
            if (!Thread.CurrentThread.GetApartmentState().Equals(ApartmentState.STA))
            {
                Thread thread = new Thread(() => LaunchAssociationWindow(true));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
                return;
            }

            LaunchAssociationWindow(false);
        }

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

        internal void RefreshProjectList(bool useDispatcher)
        {
            List<Resource> projectsRaw = this.SonarService.GetProjectsList(AuthtenticationHelper.AuthToken);
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

        internal void UpdateServices(ISonarRestService restServiceIn, IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            this.VsHelper = vsenvironmenthelperIn;
            this.StatusBar = statusBar;
            this.Provider = provider;
            this.SonarService = restServiceIn;
        }
    }
}
