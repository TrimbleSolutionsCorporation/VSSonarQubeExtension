// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarQubeViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   The sonar qube view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Media;

    using ExtensionHelpers;

    using ExtensionTypes;

    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using SonarRestService;

    using VSSonarExtensionUi.View;
    using VSSonarExtensionUi.ViewModel.Configuration;

    using VSSonarPlugins;

    /// <summary>
    ///     The sonar qube view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class SonarQubeViewModel : IViewModelBase, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        ///     The analysisPlugin control.
        /// </summary>
        public readonly PluginController PluginControl = new PluginController();

        /// <summary>
        ///     The menu item plugins.
        /// </summary>
        private readonly Dictionary<int, IMenuCommandPlugin> menuItemPlugins = new Dictionary<int, IMenuCommandPlugin>();

        /// <summary>
        ///     The is connected.
        /// </summary>
        private bool isConnected;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SonarQubeViewModel" /> class.
        /// </summary>
        public SonarQubeViewModel()
        {
            this.VsHelper = new VsPropertiesHelper();
            this.SonarRestConnector = new SonarRestService(new JsonSonarConnector());

            this.IsExtensionBusy = false;
            this.IsConnected = false;

            this.InitData();
            this.InitOptionsModel();
            this.InitCommands();
            this.InitMenus();
            this.InitViews();
            this.InitConnection();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets a value indicating whether analysis change lines.
        /// </summary>
        public bool AnalysisChangeLines { get; internal set; }

        /// <summary>
        ///     Gets or sets the assign project command.
        /// </summary>
        public RelayCommand AssignProjectCommand { get; set; }

        /// <summary>
        ///     Gets or sets the associated project.
        /// </summary>
        public Resource AssociatedProject { get; set; }

        /// <summary>
        ///     Gets or sets the available projects.
        /// </summary>
        public ObservableCollection<Resource> AvailableProjects { get; set; }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the busy tool tip.
        /// </summary>
        public string BusyToolTip { get; set; }

        /// <summary>
        ///     Gets or sets the clear cache command.
        /// </summary>
        public RelayCommand ClearCacheCommand { get; set; }

        /// <summary>
        ///     Gets the connect command.
        /// </summary>
        public RelayCommand ConnectCommand { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether connected to server.
        /// </summary>
        public bool ConnectedToServer { get; set; }

        /// <summary>
        ///     Gets or sets the connected to server command.
        /// </summary>
        public RelayCommand ConnectedToServerCommand { get; set; }

        /// <summary>
        ///     Gets or sets the connection tooltip.
        /// </summary>
        public string ConnectionTooltip { get; set; }

        /// <summary>
        ///     Gets or sets the diagnostic message.
        /// </summary>
        public string DiagnosticMessage { get; set; }

        /// <summary>
        ///     Gets or sets the disconnect to server command.
        /// </summary>
        public RelayCommand DisconnectToServerCommand { get; set; }

        /// <summary>
        ///     Gets or sets the document in view.
        /// </summary>
        public string DocumentInView { get; set; }

        /// <summary>
        ///     Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     Gets or sets the extension options data.
        /// </summary>
        public ExtensionOptionsModel ExtensionOptionsData { get; set; }

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is connected.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is connected but not associated.
        /// </summary>
        public bool IsConnectedButNotAssociated { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is extension busy.
        /// </summary>
        public bool IsExtensionBusy { get; set; }

        /// <summary>
        /// Gets or sets the issues.
        /// </summary>
        public List<Issue> Issues { get; set; }

        /// <summary>
        ///     Gets or sets the issues search view model.
        /// </summary>
        public IssuesSearchViewModel IssuesSearchViewModel { get; set; }

        /// <summary>
        ///     Gets or sets the launch extension properties command.
        /// </summary>
        public RelayCommand LaunchExtensionPropertiesCommand { get; set; }

        /// <summary>
        ///     Gets or sets the local view model.
        /// </summary>
        public LocalViewModel LocalViewModel { get; set; }

        /// <summary>
        ///     Gets or sets the open solution name.
        /// </summary>
        public string OpenSolutionName { get; set; }

        /// <summary>
        ///     Gets or sets the open solution path.
        /// </summary>
        public string OpenSolutionPath { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether refresh theme.
        /// </summary>
        [AlsoNotifyFor("BackGroundColor", "ForeGroundColor")]
        public bool RefreshTheme { get; set; }

        /// <summary>
        ///     Gets or sets the resource in editor.
        /// </summary>
        public Resource ResourceInEditor { get; set; }

        /// <summary>
        ///     Gets or sets the selected project.
        /// </summary>
        public Resource SelectedProject { get; set; }

        /// <summary>
        ///     Gets or sets the selected view.
        /// </summary>
        public IViewModelBase SelectedView { get; set; }

        /// <summary>
        ///     Gets or sets the server address.
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        ///     Gets or sets the server view model.
        /// </summary>
        public ServerViewModel ServerViewModel { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets or sets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        ///     Gets or sets the sonar qube view.
        /// </summary>
        public object SonarQubeView { get; set; }

        /// <summary>
        ///     Gets or sets the sonar qube views.
        /// </summary>
        public ObservableCollection<IViewModelBase> SonarQubeViews { get; set; }

        /// <summary>
        ///     Gets or sets the sonar rest connector.
        /// </summary>
        public ISonarRestService SonarRestConnector { get; set; }

        /// <summary>
        ///     Gets the sonar version.
        /// </summary>
        public double SonarVersion { get; internal set; }

        /// <summary>
        ///     Gets or sets the status bar.
        /// </summary>
        public IVSSStatusBar StatusBar { get; set; }

        /// <summary>
        ///     Gets or sets the tool switch command.
        /// </summary>
        public RelayCommand<string> ToolSwitchCommand { get; set; }

        /// <summary>
        ///     Gets the tools provided by plugins.
        /// </summary>
        public ObservableCollection<MenuItem> ToolsProvidedByPlugins { get; set; }

        /// <summary>
        ///     The vs helper.
        /// </summary>
        public IVsEnvironmentHelper VsHelper { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The associate project to solution.
        /// </summary>
        /// <param name="solutionName">
        /// The solution Name.
        /// </param>
        /// <param name="solutionPath">
        /// The solution Path.
        /// </param>
        public void AssociateProjectToSolution(string solutionName, string solutionPath)
        {
            this.OpenSolutionName = solutionName;
            this.OpenSolutionPath = solutionPath;
            if (!this.IsConnected)
            {
                return;
            }

            if (this.IsConnected)
            {
                Resource solResource = this.AssociateSolutionWithSonarProject(solutionName, solutionPath);
                foreach (Resource availableProject in this.AvailableProjects)
                {
                    if (availableProject.Key.Equals(solResource.Key))
                    {
                        this.SelectedProject = availableProject;
                        this.OnAssignProjectCommand();
                        this.IsConnectedButNotAssociated = false;
                        this.VsHelper.WriteOptionInApplicationData(solutionName, "PROJECTKEY", this.SelectedProject.Key);
                        return;
                    }
                }
            }
        }

        /// <summary>
        ///     The clear project association.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void ClearProjectAssociation()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The extension data model update.
        /// </summary>
        /// <param name="restServiceIn">
        /// The rest service in.
        /// </param>
        /// <param name="vsenvironmenthelperIn">
        /// The vsenvironmenthelper in.
        /// </param>
        /// <param name="associatedProj">
        /// The associated proj.
        /// </param>
        /// <param name="statusBar">
        /// The status bar.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public void ExtensionDataModelUpdate(
            ISonarRestService restServiceIn, 
            IVsEnvironmentHelper vsenvironmenthelperIn, 
            IVSSStatusBar statusBar, 
            IServiceProvider provider)
        {
            this.ServiceProvider = provider;
            this.SonarRestConnector = restServiceIn;
            this.VsHelper = vsenvironmenthelperIn;
            this.StatusBar = statusBar;

            foreach (var view in this.SonarQubeViews)
            {
                var md = view as IAnalysisViewModelBase;
                md.UpdateServices(restServiceIn, vsenvironmenthelperIn, statusBar, provider);
            }
        }

        /// <summary>
        /// The get issues in editor.
        /// </summary>
        /// <param name="fileResource">
        /// The file Resource.
        /// </param>
        /// <param name="fileContent">
        /// The file Content.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public List<Issue> GetIssuesInEditor(Resource fileResource, string fileContent)
        {
            if (this.ExtensionOptionsData.SonarConfigurationViewModel.DisableEditorTags)
            {
                return new List<Issue>();
            }

            var view = this.SelectedView as IAnalysisViewModelBase;

            if (view == null)
            {
                return null;
            }

            return view.GetIssuesForResource(fileResource, fileContent);
        }

        /// <summary>
        ///     The is running in visual studio.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool IsRunningInVisualStudio()
        {
            return this.VsHelper.AreWeRunningInVisualStudio();
        }


        /// <summary>
        ///     The connect to sonar.
        /// </summary>
        public void OnConnectToSonar()
        {
            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
            {
                this.CanConnectEnabled = true;
                this.IsExtensionBusy = false;
            };

            bw.DoWork += delegate
                {
                    this.CanConnectEnabled = false;
                    this.IsExtensionBusy = true;
                    this.TryToConnect();
                };

            bw.RunWorkerAsync();
        }

        public bool CanConnectEnabled { get; set; }

        private void TryToConnect()
        {
            try
            {
                if (this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig == null)
                {
                    var window = new ExtensionOptionsWindow(this.ExtensionOptionsData);
                    this.ExtensionOptionsData.SelectedOption = this.ExtensionOptionsData.SonarConfigurationViewModel;
                    window.ShowDialog();
                }

                if (this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig == null)
                {
                    this.IsConnected = false;
                    return;
                }

                this.IsConnected = true;
                this.IsConnectedButNotAssociated = true;

                this.ConnectionTooltip = "Authenticated, but no associated";

                List<Resource> projects = this.SonarRestConnector.GetProjectsList(this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig);
                if (projects != null && projects.Count > 0)
                {
                    this.OnRefreshProjectList(projects);
                }

                this.SonarVersion = this.SonarRestConnector.GetServerInfo(this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                this.ErrorMessage = "Cannot Connect: " + ex.Message;
                this.DiagnosticMessage = "Cannot Connect: " + ex.StackTrace;
                this.ConnectionTooltip = "No Connection";
                this.IsConnected = false;
                this.IsConnectedButNotAssociated = false;
            }
        }

        /// <summary>
        /// The refresh data for resource.
        /// </summary>
        /// <param name="fullName">
        /// The full name.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void RefreshDataForResource(string fullName)
        {
            if (string.IsNullOrEmpty(fullName) || this.AssociatedProject == null)
            {
                this.ErrorMessage = "Extension Not Ready";
                return;
            }

            this.DocumentInView = fullName;
            this.ResourceInEditor = this.CreateAResourceForFileInEditor(fullName);

            if (this.ResourceInEditor == null)
            {
                return;
            }

            var analyser = this.SelectedView as IAnalysisViewModelBase;
            if (analyser != null)
            {
                analyser.RefreshDataForResource(this.ResourceInEditor, this.DocumentInView);
            }            
        }

        /// <summary>
        ///     The startup model data.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void StartupModelData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The update colours.
        /// </summary>
        /// <param name="background">
        /// The background.
        /// </param>
        /// <param name="foreground">
        /// The foreground.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void UpdateColours(Color background, Color foreground)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The update theme.
        /// </summary>
        /// <param name="background">
        /// The background.
        /// </param>
        /// <param name="foreground">
        /// The foreground.
        /// </param>
        public void UpdateTheme(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;

            foreach (IViewModelBase view in this.SonarQubeViews)
            {
                view.UpdateColours(background, foreground);
            }

            this.ExtensionOptionsData.UpdateTheme(background, foreground);

            this.RefreshTheme = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The associate solution with sonar project.
        /// </summary>
        /// <param name="solutionName">
        /// The solution Name.
        /// </param>
        /// <param name="solutionPath">
        /// The solution Path.
        /// </param>
        /// <returns>
        /// The <see cref="Resource"/>.
        /// </returns>
        private Resource AssociateSolutionWithSonarProject(string solutionName, string solutionPath)
        {
            if (this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig == null)
            {
                this.ErrorMessage = "User Not Logged In, Extension is Unusable. Configure User Settions in Tools > Options > Sonar";
                return null;
            }

            string sourceKey = this.VsHelper.ReadOptionFromApplicationData(solutionName, "PROJECTKEY");
            if (!string.IsNullOrEmpty(sourceKey))
            {
                try
                {
                    return
                        this.SonarRestConnector.GetResourcesData(
                            this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig, 
                            sourceKey)[0];
                }
                catch (Exception ex)
                {
                    UserExceptionMessageBox.ShowException("Associated Project does not exist in server, please configure association", ex);
                    return null;
                }
            }

            sourceKey = VsSonarUtils.GetProjectKey(solutionPath);
            return string.IsNullOrEmpty(sourceKey)
                       ? null
                       : this.SonarRestConnector.GetResourcesData(
                           this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig, 
                           sourceKey)[0];
        }

        /// <summary>
        /// The create a resource for file in editor.
        /// </summary>
        /// <param name="fullName">
        /// The full name.
        /// </param>
        /// <returns>
        /// The <see cref="Resource"/>.
        /// </returns>
        private Resource CreateAResourceForFileInEditor(string fullName)
        {
            var toReturn = new Resource();

            try
            {
                string resourceKeySafe = this.LocalViewModel.LocalAnalyserModule.GetResourceKey(
                    this.VsHelper.VsProjectItem(fullName), 
                    this.AssociatedProject, 
                    this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig, 
                    true);
                string resourceKeyNonSafe = this.LocalViewModel.LocalAnalyserModule.GetResourceKey(
                    this.VsHelper.VsProjectItem(fullName), 
                    this.AssociatedProject, 
                    this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig, 
                    false);

                string fileName = Path.GetFileName(fullName);
                toReturn.Name = fileName;
                toReturn.Lname = fileName;
                toReturn.Key = resourceKeySafe;
                toReturn.NonSafeKey = resourceKeyNonSafe;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "No Plugin installed that supports this resource";
                this.DiagnosticMessage = ex.Message + " " + ex.StackTrace;
                return null;
            }

            try
            {
                toReturn =
                    this.SonarRestConnector.GetResourcesData(this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig, toReturn.Key)
                        [0];
            }
            catch (Exception ex)
            {
                try
                {
                    toReturn =
                        this.SonarRestConnector.GetResourcesData(
                            this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig, 
                            toReturn.NonSafeKey)[0];
                }
                catch (Exception ex1)
                {
                    this.ErrorMessage = "Resource not found in Server";
                    this.DiagnosticMessage = ex.Message;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// The execute tool window.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void ExecuteToolWindow(string obj)
        {
        }

        /// <summary>
        ///     The init commands.
        /// </summary>
        private void InitCommands()
        {
            this.LaunchExtensionPropertiesCommand = new RelayCommand(this.LaunchExtensionProperties);
            this.ToolSwitchCommand = new RelayCommand<string>(this.ExecuteToolWindow);

            this.AssignProjectCommand = new RelayCommand(this.OnAssignProjectCommand);
            this.ClearCacheCommand = new RelayCommand(this.OnClearCacheCommand);

            this.ConnectCommand = new RelayCommand(this.OnConnectCommand);
            this.ConnectedToServerCommand = new RelayCommand(this.OnConnectToSonar);
            this.DisconnectToServerCommand = new RelayCommand(this.OnDisconnectToSonar);
        }

        /// <summary>
        ///     The init connection.
        /// </summary>
        private void InitConnection()
        {
            if (this.ExtensionOptionsData.SonarConfigurationViewModel.IsConnectAtStartOn)
            {
                this.OnConnectToSonar();
            }
        }

        /// <summary>
        ///     The init data.
        /// </summary>
        private void InitData()
        {
            this.ToolsProvidedByPlugins = new ObservableCollection<MenuItem>();
            this.AvailableProjects = new ObservableCollection<Resource>();
            this.SonarVersion = 4.5;
            this.CanConnectEnabled = true;
            this.ConnectionTooltip = "Not Connected";
            this.ErrorMessage = "Not Started";
            this.BackGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;
        }

        /// <summary>
        ///     The init menus.
        /// </summary>
        private void InitMenus()
        {
            if (this.PluginControl == null)
            {
                return;
            }

            List<IMenuCommandPlugin> plugins = this.PluginControl.GetMenuItemPlugins();

            if (plugins == null)
            {
                return;
            }

            foreach (IMenuCommandPlugin plugin in plugins)
            {
                this.ToolsProvidedByPlugins.Add(new MenuItem { Header = plugin.GetHeader() });
                this.menuItemPlugins.Add(this.menuItemPlugins.Count, plugin);
            }
        }

        /// <summary>
        ///     The init options model.
        /// </summary>
        private void InitOptionsModel()
        {
            List<IAnalysisPlugin> plugins = this.PluginControl.GetPlugins();
            this.ExtensionOptionsData = new ExtensionOptionsModel(this.PluginControl, this, this.VsHelper);
            this.ExtensionOptionsData.Vsenvironmenthelper = this.VsHelper;
            this.ExtensionOptionsData.ResetUserData();

            if (plugins != null)
            {
                foreach (IAnalysisPlugin plugin in plugins)
                {
                    ISonarConfiguration configuration = this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig;
                    IPluginsOptions controloption = plugin.GetPluginControlOptions(configuration);
                    if (controloption == null)
                    {
                        continue;
                    }

                    string pluginKey = plugin.GetKey(this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig);
                    Dictionary<string, string> options = this.VsHelper.ReadAllAvailableOptionsInSettings(pluginKey);
                    controloption.SetOptions(options);
                }
            }
        }

        /// <summary>
        ///     The init views.
        /// </summary>
        private void InitViews()
        {

            var sdsa = this.VsHelper as VsPropertiesHelper;
            this.ServerViewModel = new ServerViewModel(this, this.VsHelper, this.SonarRestConnector, this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig);
            this.LocalViewModel = new LocalViewModel(this, this.PluginControl.GetPlugins(), this.SonarRestConnector, this.VsHelper);
            this.IssuesSearchViewModel = new IssuesSearchViewModel(this, this.VsHelper, this.SonarRestConnector);

            this.SonarQubeViews = new ObservableCollection<IViewModelBase>();
            this.SonarQubeViews.Add(this.ServerViewModel);
            this.SonarQubeViews.Add(this.LocalViewModel);
            this.SonarQubeViews.Add(this.IssuesSearchViewModel);
        }

        /// <summary>
        ///     The launch extension properties.
        /// </summary>
        private void LaunchExtensionProperties()
        {
            var window = new ExtensionOptionsWindow(this.ExtensionOptionsData);
            window.ShowDialog();
        }

        /// <summary>
        ///     The on assign project command.
        /// </summary>
        private void OnAssignProjectCommand()
        {
            if (this.SelectedProject == null)
            {
                return;
            }

            this.ConnectionTooltip = "Connected and Associated";

            if (!string.IsNullOrEmpty(this.OpenSolutionName))
            {
                this.VsHelper.WriteOptionInApplicationData(this.OpenSolutionName, "PROJECTKEY", this.SelectedProject.Key);
            }

            this.ExtensionOptionsData.Project = this.SelectedProject;
            this.ExtensionOptionsData.RefreshGeneralProperties();
            this.ExtensionOptionsData.RefreshPropertiesInPlugins();
            this.ExtensionOptionsData.SyncOptionsToFile();
            this.AssociatedProject = this.SelectedProject;

            foreach (IViewModelBase sonarQubeView in this.SonarQubeViews)
            {
                var analyser = sonarQubeView as IAnalysisViewModelBase;
                if (analyser != null)
                {
                    analyser.InitDataAssociation(
                        this.AssociatedProject, 
                        this.ExtensionOptionsData.SonarConfigurationViewModel.UserConnectionConfig, 
                        this.OpenSolutionPath);
                }
            }
        }

        /// <summary>
        ///     The on clear cache command.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        private void OnClearCacheCommand()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     The on connect command.
        /// </summary>
        private void OnConnectCommand()
        {
            if (this.IsConnected)
            {
                this.OnConnectToSonar();
            }
            else
            {
                this.OnDisconnectToSonar();
            }
        }

        /// <summary>
        ///     The on disconnect to sonar.
        /// </summary>
        private void OnDisconnectToSonar()
        {
            this.IsConnected = false;
            this.IssuesSearchViewModel.AssociatedProject = null;
            this.LocalViewModel.ClearAssociation();
            this.ConnectionTooltip = "Not Connected";
            this.AssociatedProject = null;
        }

        /// <summary>
        /// The on refresh project list.
        /// </summary>
        /// <param name="projects">
        /// The projects.
        /// </param>
        private void OnRefreshProjectList(List<Resource> projects)
        {
            this.IsConnected = true;
            this.IsConnectedButNotAssociated = false;

            this.AvailableProjects.Clear();
            foreach (Resource source in projects.OrderBy(i => i.Name))
            {
                this.AvailableProjects.Add(source);
            }
        }

        #endregion
    }
}