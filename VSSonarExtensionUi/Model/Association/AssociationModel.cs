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
    using Model.Menu;

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
        /// The local analyser module
        /// </summary>
        private readonly ISonarLocalAnalyser localAnalyserModule;

        /// <summary>
        /// The sourcecontrol
        /// </summary>
        private ISourceControlProvider sourcecontrol;

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
            ISonarLocalAnalyser localAnalyeser)
        {
            this.keyTranslator = translator;
            this.pluginManager = pluginManager;
            this.model = model;
            this.configurationHelper = configurationHelper;
            this.logger = logger;
            this.sonarService = service;
            this.localAnalyserModule = localAnalyeser;

            this.localAnalyserModule.AssociateCommandCompeted += this.LocalAssociationCompleted;
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
        /// Gets or sets a value indicating whether is associated.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is associated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAssociated { get; set; }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        /// <value>
        /// The profile.
        /// </value>
        public Dictionary<string, Profile> Profile { get; private set; }

        /// <summary>
        /// Registers the new model in pool.
        /// </summary>
        /// <param name="model">The </param>
        public static void RegisterNewModelInPool(IModelBase model)
        {
            modelPool.Add(model);
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        public void OnConnectToSonar()
        {
            foreach (IModelBase model in modelPool)
            {
                model.OnConnectToSonar(AuthtenticationHelper.AuthToken, this.model.AvailableProjects);
            }
        }

        /// <summary>
        /// The on assign project command.
        /// </summary>
        /// <param name="projectIn">The project in.</param>
        /// <param name="branchProject">The branch project.</param>
        /// <returns>
        /// Ok if assign.
        /// </returns>
        public bool AssignASonarProjectToSolution(Resource project, Resource branchProject, ISourceControlProvider sourceControl, bool skipRegisterModels = false)
        {
            if (project == null)
            {
                return false;
            }

            if (project.IsBranch)
            {
                if (branchProject == null)
                {
                    return false;
                }

                this.AssociatedProject = branchProject;
            }
            else
            {
                this.AssociatedProject = project;
            }

            this.IsAssociated = true;

            if (!string.IsNullOrEmpty(this.OpenSolutionName))
            {
                this.SaveAssociationToDisk(this.AssociatedProject);
                this.AssociatedProject.SolutionRoot = this.OpenSolutionPath;
                this.AssociatedProject.SolutionName = this.OpenSolutionName;
                if (string.IsNullOrEmpty(this.OpenSolutionPath))
                {
                    this.keyTranslator.SetProjectKeyAndBaseDir(this.AssociatedProject.Key, this.OpenSolutionPath, this.AssociatedProject.BranchName, "");
                }
                else
                {
                    this.keyTranslator.SetProjectKeyAndBaseDir(this.AssociatedProject.Key, this.OpenSolutionPath, this.AssociatedProject.BranchName, Path.Combine(this.OpenSolutionPath, this.OpenSolutionName));
                }
            }

            this.configurationHelper.SyncSettings();
            if (!skipRegisterModels)
            {
                this.InitiateAssociationToSonarProject(sourceControl);
            }
            
            return true;
        }

        private void SaveAssociationToDisk(Resource project)
        {
            try
            {
                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties
                    {
                        Owner = Path.Combine(this.OpenSolutionPath, this.OpenSolutionName),
                        Key = "PROJECTKEY",
                        Value = project.Key,
                        Context = Context.GlobalPropsId
                    });

                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties
                    {
                        Owner = Path.Combine(this.OpenSolutionPath, this.OpenSolutionName),
                        Key = "PROJECTNAME",
                        Value = this.OpenSolutionName,
                        Context = Context.GlobalPropsId
                    });

                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties
                    {
                        Owner = Path.Combine(this.OpenSolutionPath, this.OpenSolutionName),
                        Key = "PROJECTLOCATION",
                        Value = this.OpenSolutionPath,
                        Context = Context.GlobalPropsId
                    });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Currents the branch.
        /// </summary>
        /// <returns>return current branch</returns>
        public string CurrentBranch(ISourceControlProvider sourceControlProvider)
        {
            var branch = sourceControlProvider.GetBranch();
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
        public void OnSolutionClosed()
        {
            this.OpenSolutionName = null;
            this.OpenSolutionPath = null;
            this.AssociatedProject = null;
            this.IsAssociated = false;

            foreach (var item in modelPool)
            {
                item.OnSolutionClosed();
            }
        }

        /// <summary>
        /// Starts the automatic association.
        /// </summary>
        /// <param name="solutionName">Name of the solution.</param>
        /// <param name="solutionPath">The solution path.</param>
        /// <param name="availableProjects">The available projects.</param>
        /// <param name="sourceControl">The source control.</param>
        /// <exception cref="System.Exception">Solution not found in server, please be sure its analysed before</exception>
        public void StartAutoAssociation(
            string solutionName,
            string solutionPath,
            ICollection<Resource> availableProjects,
            ISourceControlProvider sourceControl)
        {
            this.OpenSolutionName = solutionName;
            this.OpenSolutionPath = solutionPath;

            Resource solResource = this.GetResourceForSolution(solutionName, solutionPath, availableProjects, sourceControl);

            if (solResource == null)
            {
                throw new Exception("Solution not found in server, please be sure its analysed before");
            }

            var resource = SearchSolutionResourceInSonarProjects(solResource, CurrentBranch(sourceControl).Replace("/", "_"));

            if (resource != null)
            {
                this.AssignASonarProjectToSolution(resource, resource, sourceControl, true);
            }
        }

        private Resource SearchSolutionResourceInSonarProjects(Resource solResource, string branchName)
        {
            Resource associatedProjectInSonar = null;
            foreach (Resource availableProject in this.model.AvailableProjects)
            {
                if (availableProject.IsBranch && solResource.IsBranch)
                {
                    associatedProjectInSonar = GetProjectInBranchResources(availableProject, solResource, branchName);
                }
                else
                {
                    if (availableProject.Key.Equals(solResource.Key))
                    {
                        associatedProjectInSonar = availableProject;
                    }
                }

                if (associatedProjectInSonar != null)
                {
                    return associatedProjectInSonar;
                }
            }

            return null;
        }

        private Resource GetProjectInBranchResources(Resource projectInSonar, Resource solutionProject, string branchName)
        {            
            Resource masterBranch = null;
            bool isMatch = false;

            foreach (var branch in projectInSonar.BranchResources)
            {
                if (branch.BranchName.ToLower().Equals("master"))
                {
                    masterBranch = branch;
                }

                if (solutionProject.Name.StartsWith(projectInSonar.Name + " ") || solutionProject.Name.Equals(projectInSonar.Name))
                {
                    isMatch = true;
                }

                if (solutionProject.Key.Equals(projectInSonar.Key) && branch.Name.EndsWith(branchName))
                {
                    return branch;
                }
            }

            if (masterBranch != null && isMatch)
            {
                return masterBranch;
            }

            return null;
        }

        /// <summary>
        /// The associate project to solution.
        /// </summary>
        /// <param name="solutionName">The solution Name.</param>
        /// <param name="solutionPath">The solution Path.</param>
        /// <param name="availableProjects">The available projects.</param>
        public void AssociateProjectToSolution(
            string solutionName,
            string solutionPath,
            ICollection<Resource> availableProjects,
            ISourceControlProvider sourceControl)
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

            try
            {
                this.StartAutoAssociation(solution, solutionPath, availableProjects, sourceControl);

                if (this.IsAssociated)
                {
                    this.CreateConfiguration(Path.Combine(solutionPath, "sonar-project.properties"));
                    this.InitiateAssociationToSonarProject(sourceControl);
                    var mainProject = SetExclusionsMenu.GetMainProject(this.AssociatedProject, this.model.AvailableProjects);
                    var exclusions = this.sonarService.GetExclusions(AuthtenticationHelper.AuthToken, mainProject);
                    this.model.LocaAnalyser.UpdateExclusions(exclusions);
                }
            }
            catch (Exception ex)
            {
                this.AssociatedProject = null;
                this.IsAssociated = false;
                this.model.StatusMessageAssociation = "Could not associate to project when solution was open: " + ex.Message;
            }
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            this.AssociatedProject = null;
            this.IsAssociated = false;
            this.keyTranslator.SetLookupType(KeyLookUpType.Invalid);
            foreach (var item in modelPool)
            {
                item.OnDisconnect();
            }
        }

        /// <summary>
        /// Generates a resource name from a given solution, it will bring Associtation View
        /// in case automatic associations is not possible
        /// </summary>
        /// <param name="solutionName">The solution Name.</param>
        /// <param name="solutionPath">The solution Path.</param>
        /// <param name="availableProjects">The available projects.</param>
        /// <param name="sourceControl">The source control.</param>
        /// <returns>
        /// The <see cref="Resource" />.
        /// </returns>
        public Resource GetResourceForSolution(
            string solutionName,
            string solutionPath,
            ICollection<Resource> availableProjects,
            ISourceControlProvider sourceControl)
        {
            try
            {
                var prop = this.configurationHelper.ReadSetting(
                    Context.GlobalPropsId,
                    Path.Combine(solutionPath, solutionName),
                    "PROJECTKEY");

                foreach (var project in availableProjects)
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
                sourceKey = sourceKey + ":" + sourceControl.GetBranch().Replace("/", "_");
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
        private void InitiateAssociationToSonarProject(ISourceControlProvider sourceControl)
        {
            this.sourcecontrol = sourceControl;

            // start local analysis association, and sync profile
            this.localAnalyserModule.AssociateWithProject(this.AssociatedProject, AuthtenticationHelper.AuthToken);
        }

        /// <summary>The update associate command.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void LocalAssociationCompleted(object sender, EventArgs e)
        {

            var listToRemo = new List<IModelBase>();

            this.keyTranslator.SetLookupType(KeyLookUpType.Invalid);
            if (string.IsNullOrEmpty(this.OpenSolutionPath))
            {
                this.keyTranslator.SetProjectKeyAndBaseDir(this.AssociatedProject.Key, this.OpenSolutionPath, this.AssociatedProject.BranchName, "");
            }
            else
            {
                this.keyTranslator.SetProjectKeyAndBaseDir(this.AssociatedProject.Key, this.OpenSolutionPath, this.AssociatedProject.BranchName, Path.Combine(this.OpenSolutionPath, this.OpenSolutionName));
            }

            try
            {
                this.Profile = this.localAnalyserModule.GetProfile(this.AssociatedProject);
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Failed to retrieve Profile : Check Log and report : " + ex.Message, ex);
                return;
            } 

            foreach (IModelBase model in modelPool)
            {
                try
                {
                    model.AssociateWithNewProject(
                        this.AssociatedProject,
                        this.OpenSolutionPath,
                        this.sourcecontrol,
                        this.pluginManager.GetIssueTrackerPlugin(), 
                        this.Profile);
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


            // sync data in plugins
            this.pluginManager.AssociateWithNewProject(
                this.AssociatedProject,
                this.OpenSolutionPath,
                this.sourcecontrol,
                this.Profile);
        }
    }
}