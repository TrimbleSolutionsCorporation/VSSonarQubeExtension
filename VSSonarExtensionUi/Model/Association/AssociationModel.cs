﻿namespace VSSonarExtensionUi.Association
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using PropertyChanged;

    using SonarLocalAnalyser;

    using SonarRestService;
    using SonarRestService.Types;

    using View.Helpers;

    using ViewModel;

    using VSSonarExtensionUi.Model.Helpers;
    using VSSonarExtensionUi.Model.Menu;
    using VSSonarExtensionUi.ViewModel.Configuration;

    using VSSonarPlugins;
    using VSSonarPlugins.Helpers;
    using VSSonarPlugins.Types;

    /// <summary>
    /// Generates associations with sonar projects
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class AssociationModel
    {
        /// <summary>
        /// The model pool
        /// </summary>
        private static readonly List<IModelBase> modelPool = new List<IModelBase>();

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
        /// The vs version
        /// </summary>
        private readonly string vsVersion;

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
            ISonarLocalAnalyser localAnalyeser,
            string vsVersion)
        {
            this.vsVersion = vsVersion;
            this.keyTranslator = translator;
            this.pluginManager = pluginManager;
            this.model = model;
            this.configurationHelper = configurationHelper;
            this.logger = logger;
            this.sonarService = service;
            this.localAnalyserModule = localAnalyeser;
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
        public async Task OnConnectToSonar()
        {
            foreach (var model in modelPool)
            {
                try
                {
                    await model.OnConnectToSonar(AuthtenticationHelper.AuthToken, this.model.AvailableProjects, this.pluginManager.IssueTrackerPlugins);
                }
                catch (Exception ex)
                {
                    this.logger.ReportMessage(new Message { Id = "Association Model", Data = "Exception for model while connecting : " + model.ToString() });
                    this.logger.ReportException(ex);
                }
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
        public async Task<bool> AssignASonarProjectToSolution(Resource project, Resource branchProject, ISourceControlProvider sourceControl)
        {
            if (project == null ||
               (this.OpenSolutionName == null || this.OpenSolutionPath == null) ||
               (project.IsBranch && branchProject == null))
            {
                return false;
            }

            if (project.IsBranch)
            {
                this.AssociatedProject = branchProject;
            }
            else
            {
                this.AssociatedProject = project;
            }

            this.SaveAssociationToDisk(this.AssociatedProject);
            this.AssociatedProject.SolutionRoot = this.OpenSolutionPath;
            this.AssociatedProject.SolutionName = this.OpenSolutionName;
            this.keyTranslator.SetProjectKeyAndBaseDir(this.AssociatedProject.Key, this.OpenSolutionPath, this.AssociatedProject.BranchName, Path.Combine(this.OpenSolutionPath, this.OpenSolutionName));
            this.configurationHelper.SyncSettings();
            // start local analysis association, and sync profile
            await this.localAnalyserModule.AssociateWithProject(this.AssociatedProject, AuthtenticationHelper.AuthToken);
            this.LocalAssociationCompleted();
            this.IsAssociated = true;
            return true;
        }

        /// <summary>
        /// The associate project to solution.
        /// </summary>
        /// <param name="solutionName">The solution Name.</param>
        /// <param name="solutionPath">The solution Path.</param>
        /// <param name="availableProjects">The available projects.</param>
        public async Task AssociateProjectToSolution(
            string solutionName,
            string solutionPath,
            ICollection<Resource> availableProjects,
            ISourceControlProvider sourceControl)
        {
            try
            {
                this.OpenSolutionName = solutionName;
                this.OpenSolutionPath = solutionPath;

                var solResource = this.GetResourceForSolution(solutionName, solutionPath, availableProjects, sourceControl);

                if (solResource == null)
                {
                    throw new Exception("Solution not found in server, please be sure its analysed before");
                }

                var resource = this.SearchSolutionResourceInSonarProjects(solResource, this.CurrentBranch(sourceControl).Replace("/", "_"));

                if (resource == null)
                {
                    throw new Exception("Solution not found in server, please be sure its analysed before");
                }

                resource.SolutionName = solutionName;
                resource.SolutionRoot = solutionPath;
                await this.AssignASonarProjectToSolution(resource, resource, sourceControl);
                this.CreateConfiguration(Path.Combine(solutionPath, "sonar-project.properties"));
                // start local analysis association, and sync profile
                await this.localAnalyserModule.AssociateWithProject(this.AssociatedProject, AuthtenticationHelper.AuthToken);
                this.LocalAssociationCompleted();
                var mainProject = SetExclusionsMenu.GetMainProject(this.AssociatedProject, this.model.AvailableProjects);
                var exclusions = this.sonarService.GetExclusions(AuthtenticationHelper.AuthToken, mainProject);
                this.model.LocaAnalyser.UpdateExclusions(exclusions);
            }
            catch (Exception ex)
            {
                this.AssociatedProject = null;
                this.IsAssociated = false;
                this.model.StatusMessageAssociation = "Could not associate to project when solution was open: " + ex.Message;
            }
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
                        Context = Context.GlobalPropsId.ToString()
                    });

                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties
                    {
                        Owner = Path.Combine(this.OpenSolutionPath, this.OpenSolutionName),
                        Key = "PROJECTNAME",
                        Value = OpenSolutionName,
                        Context = Context.GlobalPropsId.ToString()
                    });

                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties
                    {
                        Owner = Path.Combine(this.OpenSolutionPath, this.OpenSolutionName),
                        Key = "PROJECTLOCATION",
                        Value = OpenSolutionPath,
                        Context = Context.GlobalPropsId.ToString()
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
        private Resource CreateResourcePathFile(string fullName, Resource project)
        {
            if (this.vshelper == null)
            {
                return null;
            }

            // key empty because translator failed, new file
            var localRes = new Resource
            {
                Key = "",
                Scope = "FIL"
            };
            var fileName = Path.GetFileName(fullName);
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
        public async Task<Resource> CreateAValidResourceFromServer(string fullName, Resource project)
        {
            if (this.vshelper == null)
            {
                return null;
            }

            try
            {
                var data = await this.vshelper.VsFileItem(fullName, project, null);
                return this.keyTranslator.TranslatePath(data, this.vshelper, this.sonarService, AuthtenticationHelper.AuthToken);
            }
            catch (Exception ex)
            {
                this.logger.WriteMessageToLog("Please report this error: Resource should be always created. " + ex.Message);
                return null;
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
            foreach (var model in modelPool)
            {
                model.UpdateServices(vsenvironmenthelperIn, statusBar, provider);
            }
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public async Task OnSolutionClosed()
        {
            this.OpenSolutionName = null;
            this.OpenSolutionPath = null;
            this.AssociatedProject = null;
            this.IsAssociated = false;

            foreach (var item in modelPool)
            {
                await item.OnSolutionClosed();
            }
        }

        private Resource SearchSolutionResourceInSonarProjects(Resource solResource, string branchName)
        {
            Resource associatedProjectInSonar = null;
            foreach (var availableProject in this.model.AvailableProjects)
            {
                if (availableProject.IsBranch && solResource.IsBranch)
                {
                    associatedProjectInSonar = this.GetProjectInBranchResources(availableProject, solResource, branchName);
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
            var isMatch = false;

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
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            this.AssociatedProject = null;
            this.IsAssociated = false;
            this.keyTranslator.SetLookupType(KeyLookupType.Invalid);
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
            if (AuthtenticationHelper.AuthToken == null)
            {
                return null;
            }

            var prop = this.configurationHelper.ReadSetting(
                Context.GlobalPropsId,
                Path.Combine(solutionPath, solutionName),
                "PROJECTKEY");

            if (prop != null)
            {
                var project = availableProjects.FirstOrDefault(x => x.Key.Equals(prop.Value));
                if (project != null)
                {
                    return project;
                }

                try
                {
                    return this.sonarService.GetResourcesData(AuthtenticationHelper.AuthToken, prop.Value)[0];
                }
                catch (Exception ex)
                {
                    this.model.StatusMessageAssociation = "Associated Project does not exist in server, please configure association: " + ex.Message;
                    return null;
                }
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

        /// <summary>The update associate command.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void LocalAssociationCompleted()
        {
            if (!this.model.IsConnected)
            {
                return;
            }

            var listToRemo = new List<IModelBase>();

            this.keyTranslator.SetLookupType(KeyLookupType.Invalid);
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

            // sync data in plugins
            this.pluginManager.AssociateWithNewProject(
                this.AssociatedProject,
                this.OpenSolutionPath,
                null,
                this.Profile,
                this.vsVersion);

            foreach (var model in modelPool)
            {
                try
                {
                    model.AssociateWithNewProject(
                        this.AssociatedProject,
                        this.OpenSolutionPath,
                        null,
                        this.Profile,
                        this.vsVersion);
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
