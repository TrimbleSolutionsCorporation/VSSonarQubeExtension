// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarQubeViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   The changed event handler.
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
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using SonarRestService;

    using VSSonarExtensionUi.Cache;
    using VSSonarExtensionUi.Helpers;
    using VSSonarExtensionUi.View.Configuration;
    using VSSonarExtensionUi.View.Helpers;
    using VSSonarExtensionUi.ViewModel.Analysis;
    using VSSonarExtensionUi.ViewModel.Configuration;
    using VSSonarExtensionUi.ViewModel.Helpers;

    using VSSonarPlugins;
    using VSSonarPlugins.Helpers;
    using VSSonarPlugins.Types;

    /// <summary>
    ///     The changed event handler.
    /// </summary>
    /// <param name="sender">
    ///     The sender.
    /// </param>
    /// <param name="e">
    ///     The e.
    /// </param>
    public delegate void ChangedEventHandler(object sender, EventArgs e);

    /// <summary>
    ///     The sonar qube view viewModel.
    /// </summary>
    [ImplementPropertyChanged]
    public class SonarQubeViewModel : IViewModelBase
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

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SonarQubeViewModel" /> class.
        /// </summary>
        public SonarQubeViewModel()
        {
            this.configurationHelper = new ConfigurationHelper();
            this.SonarRestConnector = new SonarRestService(new JsonSonarConnector());
            this.Logger = new VsSonarExtensionLogger(this);
            this.VsHelper = new StandAloneVsHelper();
            this.notificationManager = new NotifyCationManager(this);

            this.IsExtensionBusy = false;
            this.IsConnected = false;

            this.InitData();
            this.InitOptionsModel();
            this.InitCommands();
            this.InitMenus();
            this.InitViews();
            this.InitConnection();

            this.UpdateTheme(Colors.Black, Colors.White);
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The analysis mode has change.
        /// </summary>
        public event ChangedEventHandler AnalysisModeHasChange;

        /// <summary>
        ///     The plugin request.
        /// </summary>
        public event ChangedEventHandler PluginRequest;

        /// <summary>The remove plugin request.</summary>
        public event ChangedEventHandler RemovePluginRequest;

        #endregion

        #region Public Properties

        /// <summary>Gets or sets the id of plugin to remove.</summary>
        public int IdOfPluginToRemove { get; set; }

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
        ///     Gets or sets a value indicating whether can connect enabled.
        /// </summary>
        public bool CanConnectEnabled { get; set; }

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
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        ///     Gets or sets the in use plugin.
        /// </summary>
        public KeyValuePair<int, IMenuCommandPlugin> InUsePlugin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is associated.
        /// </summary>
        public bool IsAssociated { get; set; }

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
        ///     Gets or sets the issues.
        /// </summary>
        public List<Issue> Issues { get; set; }

        /// <summary>
        ///     Gets or sets the issues search view viewModel.
        /// </summary>
        public IssuesSearchViewModel IssuesSearchViewModel { get; set; }

        /// <summary>
        ///     Gets or sets the launch extension properties command.
        /// </summary>
        public RelayCommand LaunchExtensionPropertiesCommand { get; set; }

        /// <summary>
        ///     Gets or sets the local view viewModel.
        /// </summary>
        public LocalViewModel LocalViewModel { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        public VsSonarExtensionLogger Logger { get; set; }

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
        public IAnalysisViewModelBase SelectedView { get; set; }

        /// <summary>
        ///     Gets or sets the server address.
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        ///     Gets or sets the server view viewModel.
        /// </summary>
        public ServerViewModel ServerViewModel { get; set; }

        /// <summary>
        ///     Gets or sets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        ///     Gets or sets the sonar qube views.
        /// </summary>
        public ObservableCollection<IAnalysisViewModelBase> SonarQubeViews { get; set; }

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
        ///     Gets or sets the tools provided by plugins.
        /// </summary>
        public ObservableCollection<MenuItem> ToolsProvidedByPlugins { get; set; }

        /// <summary>
        ///     Gets or sets the extension options data.
        /// </summary>
        public VSonarQubeOptionsViewModel VSonarQubeOptionsViewData { get; set; }

        /// <summary>
        ///     Gets or sets the vs helper.
        /// </summary>
        public IVsEnvironmentHelper VsHelper { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>The associate project to solution.</summary>
        /// <param name="solutionName">The solution Name.</param>
        /// <param name="solutionPath">The solution Path.</param>
        public void AssociateProjectToSolution(string solutionName, string solutionPath)
        {
            if (!solutionName.ToLower().EndsWith(".sln"))
            {
                solutionName += ".sln";
            }

            this.OpenSolutionName = solutionName;
            this.OpenSolutionPath = solutionPath;
            if (!this.IsConnected)
            {
                return;
            }

            if (this.IsConnected)
            {
                Resource solResource = this.AssociateSolutionWithSonarProject(solutionName, solutionPath);
                if(solResource != null)
                {
                    foreach (Resource availableProject in this.AvailableProjects)
                    {
                        if (availableProject.Key.Equals(solResource.Key))
                        {
                            this.SelectedProject = availableProject;
                            this.OnAssignProjectCommand();
                            this.IsConnectedButNotAssociated = false;
                            this.IsAssociated = true;
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     The clear project association.
        /// </summary>
        public void ClearProjectAssociation()
        {
            this.OpenSolutionName = null;
            this.OpenSolutionPath = null;
            this.IsConnectedButNotAssociated = this.IsConnected;
            this.IsAssociated = !this.IsConnected;

            foreach (IAnalysisViewModelBase sonarQubeView in this.SonarQubeViews)
            {
                sonarQubeView.EndDataAssociation();
            }

            this.VSonarQubeOptionsViewData.EndDataAssociation();
        }

        /// <summary>The execute plugin.</summary>
        /// <param name="header">The data.</param>
        public void ExecutePlugin(string header)
        {
            foreach (var plugin in this.menuItemPlugins)
            {
                var plugDesc = plugin.Value.GetPluginDescription();
                if (!plugDesc.Name.Equals(header))
                {
                    continue;
                }

                string isEnabled = this.configurationHelper.ReadSetting(
                    Context.AnalysisGeneral, 
                    plugDesc.Name, 
                    GlobalIds.PluginEnabledControlId).Value;

                if (string.IsNullOrEmpty(isEnabled) || isEnabled.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (this.IsRunningInVisualStudio())
                    {
                        this.InUsePlugin = plugin;
                        this.OnPluginRequest(EventArgs.Empty);
                    }
                    else
                    {
                        var window = new Window
                                         {
                                             Content =
                                                 plugin.Value.GetUserControl(
                                                     AuthtenticationHelper.AuthToken, 
                                                     this.AssociatedProject, 
                                                     this.VsHelper)
                                         };

                        plugin.Value.UpdateTheme(this.BackGroundColor, this.ForeGroundColor);
                        window.ShowDialog();
                    }
                }
                else
                {
                    MessageBox.Show(plugin.Value.GetPluginDescription().Name + " is disabled");
                }
            }
        }

        /// <summary>The extension data viewModel update.</summary>
        /// <param name="restServiceIn">The rest service in.</param>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="extensionFolder">The extension Folder.</param>
        public void ExtensionDataModelUpdate(
            ISonarRestService restServiceIn, 
            IVsEnvironmentHelper vsenvironmenthelperIn, 
            IVSSStatusBar statusBar, 
            IServiceProvider provider, 
            string extensionFolder)
        {
            this.ServiceProvider = provider;
            this.SonarRestConnector = restServiceIn;
            this.VsHelper = vsenvironmenthelperIn;
            this.StatusBar = statusBar;
            this.ExtensionFolder = extensionFolder;

            foreach (IAnalysisViewModelBase view in this.SonarQubeViews)
            {
                view.UpdateServices(restServiceIn, vsenvironmenthelperIn, this.configurationHelper, statusBar, provider);
            }

            this.VSonarQubeOptionsViewData.UpdateServices(restServiceIn, vsenvironmenthelperIn, statusBar, provider);
            foreach (IOptionsViewModelBase view in this.VSonarQubeOptionsViewData.AvailableOptions)
            {
                view.UpdateServices(restServiceIn, vsenvironmenthelperIn, this.configurationHelper, statusBar, provider);
            }

            // this.Diagnostics = this.roslynAdapter.GetDiagnosticElementFromManifest(Path.Combine(this.ExtensionFolder, "extension.vsixmanifest"));
            // this.DiagnosticsFound = "Sonar Rules Found: " + this.Diagnostics.Length;
        }

        /// <summary>Gets or sets the extension folder.</summary>
        public string ExtensionFolder { get; set; }

        /// <summary>The get issues in editor.</summary>
        /// <param name="fileResource">The file Resource.</param>
        /// <param name="fileContent">The file Content.</param>
        /// <returns>The<see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .</returns>
        public List<Issue> GetIssuesInEditor(Resource fileResource, string fileContent)
        {
            this.Logger.WriteMessage("Return issues for resource: " + fileResource);
            if (this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.DisableEditorTags)
            {
                this.Logger.WriteMessage("Return issues for resource, tags disabled");
                return new List<Issue>();
            }

            IAnalysisViewModelBase view = this.SelectedView;

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
                    if (!string.IsNullOrEmpty(this.OpenSolutionName))
                    {
                        this.AssociateProjectToSolution(this.OpenSolutionName, this.OpenSolutionPath);
                    }
                };

            bw.RunWorkerAsync();
        }

        /// <summary>
        ///     The on issues change event.
        /// </summary>
        public void OnIssuesChangeEvent()
        {
            this.SelectedView.OnAnalysisModeHasChange(EventArgs.Empty);
        }

        /// <summary>
        ///     The on selected view changed.
        /// </summary>
        public void OnSelectedViewChanged()
        {
            this.configurationHelper.WriteSetting(
                new SonarQubeProperties
                    {
                        Context = Context.UIProperties, 
                        Key = "SelectedView", 
                        Owner = OwnersId.ApplicationOwnerId, 
                        Value = this.SelectedView.Header
                    }, true);

            this.OnAnalysisModeHasChange(EventArgs.Empty);
            Debug.WriteLine("Name Changed");
        }

        /// <summary>The refresh data for resource.</summary>
        /// <param name="fullName">The full name.</param>
        public void RefreshDataForResource(string fullName)
        {
            this.Logger.WriteMessage("Refresh Data For File: " + fullName);
            if (string.IsNullOrEmpty(fullName) || this.AssociatedProject == null)
            {
                if (this.IsConnected && this.AssociatedProject == null)
                {
                    this.ErrorMessage = "Online : Not Associated";
                }

                if (!this.IsConnected)
                {
                    this.ErrorMessage = "Offline";
                }

                this.DiagnosticMessage = string.Empty;
                return;
            }

            this.DocumentInView = fullName;
            this.ResourceInEditor = this.CreateAResourceForFileInEditor(fullName);

            if (this.ResourceInEditor == null)
            {
                return;
            }

            IAnalysisViewModelBase analyser = this.SelectedView;
            if (analyser != null)
            {
                try
                {
                    this.Logger.WriteMessage("RefreshDataForResource: Doc in View: " + this.DocumentInView);
                    analyser.RefreshDataForResource(this.ResourceInEditor, this.DocumentInView);
                }
                catch (Exception ex)
                {
                    this.Logger.WriteMessage("Cannot find file in server: " + ex.Message);
                    this.Logger.WriteException(ex);
                    this.ErrorMessage = "Cannot Find File in Server";
                }
            }
        }

        /// <summary>The update colours.</summary>
        /// <param name="background">The background.</param>
        /// <param name="foreground">The foreground.</param>
        public void UpdateColours(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        /// <summary>The update theme.</summary>
        /// <param name="background">The background.</param>
        /// <param name="foreground">The foreground.</param>
        public void UpdateTheme(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;

            foreach (IAnalysisViewModelBase view in this.SonarQubeViews)
            {
                view.UpdateColours(background, foreground);
            }

            this.VSonarQubeOptionsViewData.UpdateTheme(background, foreground);

            this.RefreshTheme = true;

            if (this.PluginControl == null)
            {
                return;
            }

            this.VSonarQubeOptionsViewData.PluginManager.UpdateColours(background, foreground);
        }

        #endregion

        #region Methods

        /// <summary>The on changed.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnAnalysisModeHasChange(EventArgs e)
        {
            if (this.AnalysisModeHasChange != null)
            {
                this.AnalysisModeHasChange(this, e);
            }
        }

        /// <summary>The on plugin request.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnPluginRequest(EventArgs e)
        {
            if (this.PluginRequest != null)
            {
                this.PluginRequest(this, e);
            }
        }

        /// <summary>The on remove plugin request.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnRemovePluginRequest(EventArgs e)
        {
            if (this.RemovePluginRequest != null)
            {
                this.RemovePluginRequest(this, e);
            }
        }

        /// <summary>The get first free id.</summary>
        /// <returns>The <see cref="int"/>.</returns>
        public int GetFirstFreeId()
        {
            int i = 0;
            foreach (var menu in this.menuItemPlugins)
            {
                if (menu.Key != i)
                {
                    return i;
                }

                i++;
            }

            return i;
        }

        /// <summary>The add a new menu.</summary>
        /// <param name="toAdd">The to add.</param>
        public void AddANewMenu(IMenuCommandPlugin toAdd)
        {
            var plugDesc = toAdd.GetPluginDescription();

            foreach (var menu in this.ToolsProvidedByPlugins)
            {
                if (menu.Header.Equals(plugDesc.Name))
                {
                    this.RemoveMenuPlugin(toAdd);
                    this.ToolsProvidedByPlugins.Add(new MenuItem { Header = plugDesc.Name });
                    this.menuItemPlugins.Add(this.GetFirstFreeId(), toAdd);
                    return;
                }
            }

            this.ToolsProvidedByPlugins.Add(new MenuItem { Header = plugDesc.Name });
            this.menuItemPlugins.Add(this.GetFirstFreeId(), toAdd);
        }

        /// <summary>The remove menu plugin.</summary>
        /// <param name="toRemove">The to remove.</param>
        public void RemoveMenuPlugin(IMenuCommandPlugin toRemove)
        {
            var plugDescRemove = toRemove.GetPluginDescription();
            foreach (var menu in this.ToolsProvidedByPlugins)
            {
                if (menu.Header.Equals(plugDescRemove.Name))
                {
                    this.ToolsProvidedByPlugins.Remove(menu);
                    KeyValuePair<int, IMenuCommandPlugin> selectMenu = InUsePlugin;
                    foreach (var menuCheck in this.menuItemPlugins)
                    {
                        if (menuCheck.Value.GetPluginDescription().Name.Equals(plugDescRemove.Name))
                        {
                            selectMenu = menuCheck;
                        }
                    }

                    if (this.IsRunningInVisualStudio())
                    {
                        this.IdOfPluginToRemove = selectMenu.Key;
                        this.OnRemovePluginRequest(EventArgs.Empty);
                    }

                    this.menuItemPlugins.Remove(selectMenu.Key);
                    return;
                }
            }
        }

        /// <summary>The associate solution with sonar project.</summary>
        /// <param name="solutionName">The solution Name.</param>
        /// <param name="solutionPath">The solution Path.</param>
        /// <returns>The <see cref="Resource"/>.</returns>
        private Resource AssociateSolutionWithSonarProject(string solutionName, string solutionPath)
        {
            if (AuthtenticationHelper.AuthToken == null)
            {
                this.ErrorMessage = "User Not Logged In, Extension is Unusable. Configure User Settions in Tools > Options > Sonar";
                return null;
            }

            try
            {
                var prop = this.configurationHelper.ReadSetting(
                    Context.GlobalPropsId, 
                    Path.Combine(solutionPath, solutionName), 
                    "PROJECTKEY");

                try
                {
                    return
                        this.SonarRestConnector.GetResourcesData(
                            AuthtenticationHelper.AuthToken, 
                            prop.Value)[0];
                }
                catch (Exception ex)
                {
                    UserExceptionMessageBox.ShowException("Associated Project does not exist in server, please configure association", ex);
                    return null;
                }
            }
            catch (Exception ex)
            {
            }

            var sourceKey = VsSonarUtils.GetProjectKey(solutionPath);
            return string.IsNullOrEmpty(sourceKey)
                       ? null
                       : this.SonarRestConnector.GetResourcesData(
                           AuthtenticationHelper.AuthToken, 
                           sourceKey)[0];
        }

        /// <summary>The create a resource for file in editor.</summary>
        /// <param name="fullName">The full name.</param>
        /// <returns>The <see cref="Resource"/>.</returns>
        private Resource CreateAResourceForFileInEditor(string fullName)
        {
            var keyTypes = new string[3];

            try
            {
                keyTypes[1] = this.LocalViewModel.LocalAnalyserModule.GetResourceKey(
                    this.VsHelper.VsProjectItem(fullName), 
                    this.AssociatedProject, 
                    AuthtenticationHelper.AuthToken, 
                    true);
                keyTypes[2] = this.LocalViewModel.LocalAnalyserModule.GetResourceKey(
                    this.VsHelper.VsProjectItem(fullName), 
                    this.AssociatedProject, 
                    AuthtenticationHelper.AuthToken, 
                    false);

                string tounix = this.VsHelper.GetProperFilePathCapitalization(fullName).Replace("\\", "/");
                string driveLetter = tounix.Substring(0, 1);
                string solutionCan = driveLetter + this.OpenSolutionPath.Replace("\\", "/").Substring(1);
                string fromBaseDir = tounix.Replace(solutionCan + "/", string.Empty);
                keyTypes[0] = this.AssociatedProject.Key + ":" + fromBaseDir;
            }
            catch (Exception ex)
            {
                this.Logger.WriteException(ex);
                this.ErrorMessage = "No Plugin installed that supports this resource";
                this.DiagnosticMessage = ex.Message + " " + ex.StackTrace;
                return null;
            }

            foreach (string key in keyTypes)
            {
                try
                {
                    this.Logger.WriteMessage("try key: " + key);
                    Resource toReturn =
                        this.SonarRestConnector.GetResourcesData(
                            AuthtenticationHelper.AuthToken, 
                            key)[0];
                    string fileName = Path.GetFileName(fullName);
                    toReturn.Name = fileName;
                    toReturn.Lname = fileName;
                    this.ErrorMessage = "Resource Found";
                    this.Logger.WriteMessage("Resource found: " + toReturn.Key);
                    return toReturn;
                }
                catch (Exception ex)
                {
                    this.Logger.WriteException(ex);
                }
            }

            this.Logger.WriteMessage("Resource not found in Server");
            this.ErrorMessage = "Resource not found in Server";

            return null;
        }

        /// <summary>
        ///     The init commands.
        /// </summary>
        private void InitCommands()
        {
            this.LaunchExtensionPropertiesCommand = new RelayCommand(this.LaunchExtensionProperties);
            this.ToolSwitchCommand = new RelayCommand<string>(this.ExecutePlugin);

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
            if (this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.IsConnectAtStartOn)
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
            foreach (IMenuCommandPlugin plugin in this.VSonarQubeOptionsViewData.PluginManager.MenuPlugins)
            {
                this.ToolsProvidedByPlugins.Add(new MenuItem { Header = plugin.GetPluginDescription().Name });
                this.menuItemPlugins.Add(this.menuItemPlugins.Count, plugin);
            }
        }

        /// <summary>
        ///     The init options viewModel.
        /// </summary>
        private void InitOptionsModel()
        {
            this.VSonarQubeOptionsViewData = new VSonarQubeOptionsViewModel(this.PluginControl, this, this.VsHelper, this.configurationHelper, this.notificationManager)
                                                 {
                                                     Vsenvironmenthelper =
                                                         this.VsHelper
                                                 };

            this.VSonarQubeOptionsViewData.ResetUserData();
        }

        /// <summary>
        ///     The init views.
        /// </summary>
        private void InitViews()
        {
            this.ServerViewModel = new ServerViewModel(
                this, 
                this.VsHelper, 
                this.configurationHelper, 
                this.SonarRestConnector, 
                AuthtenticationHelper.AuthToken);
            this.LocalViewModel = new LocalViewModel(this, this.VSonarQubeOptionsViewData.PluginManager.AnalysisPlugins, this.SonarRestConnector, this.VsHelper, this.configurationHelper, AuthtenticationHelper.AuthToken);
            this.IssuesSearchViewModel = new IssuesSearchViewModel(this, this.VsHelper, this.SonarRestConnector, this.configurationHelper);

            this.SonarQubeViews = new ObservableCollection<IAnalysisViewModelBase>
                                      {
                                          this.ServerViewModel, 
                                          this.LocalViewModel, 
                                          this.IssuesSearchViewModel
                                      };

            try
            {
                string view = this.configurationHelper.ReadSetting(Context.UIProperties, OwnersId.ApplicationOwnerId, "SelectedView").Value;

                foreach (IAnalysisViewModelBase analysisViewModelBase in
                    this.SonarQubeViews.Where(analysisViewModelBase => analysisViewModelBase.Header.Equals(view)))
                {
                    this.SelectedView = analysisViewModelBase;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        ///     The launch extension properties.
        /// </summary>
        private void LaunchExtensionProperties()
        {
            this.VSonarQubeOptionsViewData.RefreshPropertiesInView(this.AssociatedProject);
            var window = new VSonarQubeOptionsView(this.VSonarQubeOptionsViewData);
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
                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties
                        {
                            Owner = OwnersId.ApplicationOwnerId, 
                            Key = "PROJECTKEY", 
                            Value = this.SelectedProject.Key, 
                            Context = Context.GlobalPropsId
                        });

                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties
                        {
                            Owner = OwnersId.ApplicationOwnerId, 
                            Key = "PROJECTLOCATION", 
                            Value = this.OpenSolutionPath, 
                            Context = Context.GlobalPropsId
                        });
                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties
                        {
                            Owner = OwnersId.ApplicationOwnerId, 
                            Key = "PROJECTNAME", 
                            Value = this.OpenSolutionName, 
                            Context = Context.GlobalPropsId
                        });
            }

            this.AssociatedProject = this.SelectedProject;

            if (string.IsNullOrEmpty(this.OpenSolutionPath))
            {
                try
                {
                    var option = this.configurationHelper.ReadSetting(
                        Context.GlobalPropsId, 
                        this.SelectedProject.Key, 
                        "PROJECTLOCATION");
                    this.OpenSolutionPath = option.Value;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                try
                {
                    var option = this.configurationHelper.ReadSetting(
                        Context.GlobalPropsId, 
                        this.SelectedProject.Key, 
                        "PROJECTNAME");
                    this.OpenSolutionName = option.Value;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            this.ErrorMessage = "Associated";
            this.IsAssociated = true;

            this.configurationHelper.SyncSettings();
            this.VSonarQubeOptionsViewData.RefreshPropertiesInView(this.SelectedProject);
            foreach (var view in this.SonarQubeViews)
            {
                view.InitDataAssociation(this.AssociatedProject, AuthtenticationHelper.AuthToken, this.OpenSolutionPath);
            }
        }

        /// <summary>
        ///     The on clear cache command.
        /// </summary>
        private void OnClearCacheCommand()
        {
            this.ServerViewModel.ClearCache();
        }

        /// <summary>
        ///     The on connect command.
        /// </summary>
        private void OnConnectCommand()
        {
            if (this.IsConnected)
            {
                this.OnConnectToSonar();
                this.ErrorMessage = "Online";
            }
            else
            {
                this.OnDisconnectToSonar();
                this.ErrorMessage = "Offline";
            }
        }

        /// <summary>
        ///     The on disconnect to sonar.
        /// </summary>
        private void OnDisconnectToSonar()
        {
            this.IsConnected = false;
            foreach (IAnalysisViewModelBase view in this.SonarQubeViews)
            {
                view.EndDataAssociation();
            }


            if (this.VsHelper == null || !this.VsHelper.AreWeRunningInVisualStudio())
            {
                this.OpenSolutionPath = string.Empty;
                this.OpenSolutionName = string.Empty;
            }

            this.AvailableProjects.Clear();
            this.SelectedProject = null;

            this.ConnectionTooltip = "Not Connected";
            this.AssociatedProject = null;
        }

        /// <summary>The on refresh project list.</summary>
        /// <param name="projects">The projects.</param>
        private void OnRefreshProjectList(IEnumerable<Resource> projects)
        {
            this.IsConnected = true;
            this.IsConnectedButNotAssociated = false;
            this.IsAssociated = false;

            Application.Current.Dispatcher.Invoke(
                delegate
                    {
                        this.AvailableProjects.Clear();
                        foreach (Resource source in projects.OrderBy(i => i.Name))
                        {
                            this.AvailableProjects.Add(source);
                        }
                    });
        }

        /// <summary>
        ///     The try to connect.
        /// </summary>
        private void TryToConnect()
        {
            try
            {
                if (AuthtenticationHelper.AuthToken == null)
                {
                    var window = new VSonarQubeOptionsView(this.VSonarQubeOptionsViewData);
                    this.VSonarQubeOptionsViewData.SelectedOption = this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel;
                    window.ShowDialog();
                }

                if (AuthtenticationHelper.AuthToken == null)
                {
                    this.IsConnected = false;
                    return;
                }

                this.IsConnected = true;
                this.IsConnectedButNotAssociated = true;
                this.IsAssociated = false;

                this.ConnectionTooltip = "Authenticated, but no associated";

                List<Resource> projects =
                    this.SonarRestConnector.GetProjectsList(AuthtenticationHelper.AuthToken);
                if (projects != null && projects.Count > 0)
                {
                    this.OnRefreshProjectList(projects);
                }

                this.SonarVersion =
                    this.SonarRestConnector.GetServerInfo(AuthtenticationHelper.AuthToken);
                this.ErrorMessage = "Online";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                this.ErrorMessage = "Cannot Connect: Check Credentials";
                this.DiagnosticMessage = "Cannot Connect: " + ex.Message + " \n " + ex.StackTrace;
                this.ConnectionTooltip = "No Connection";
                this.IsConnected = false;
                this.IsConnectedButNotAssociated = false;
                this.IsAssociated = false;
            }
        }

        #endregion

        /// <summary>The configuration helper.</summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>The notification manager.</summary>
        private readonly INotificationManager notificationManager;

        /// <summary>The closed window.</summary>
        /// <param name="fullName">The full name.</param>
        public void ClosedWindow(string fullName)
        {
            if (this.ServerViewModel != null)
            {
                this.ServerViewModel.UpdateOpenDiffWindowList(fullName);
            }
        }

        /// <summary>The get coverage in editor.</summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The <see cref="Dictionary"/>.</returns>
        public Dictionary<int, CoverageElement> GetCoverageInEditor(string buffer)
        {
            if (this.SelectedView == this.ServerViewModel)
            {
                return this.ServerViewModel.GetCoverageInEditor(buffer);
            }

            return new Dictionary<int, CoverageElement>();
        }
    }
}