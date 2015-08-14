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
    using View.Configuration;
    using Analysis;
    using Configuration;
    using Helpers;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using SonarLocalAnalyser;
    using Model.Helpers;
    using Model.Association;
    using Model.PluginManager;
    using Model.Cache;

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
        public SonarQubeViewModel()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SonarQubeViewModel" /> class. Plugins are initialized in InitModelFromPackageInitialization below
        /// </summary>
        public SonarQubeViewModel(string vsVersion)
        {
            this.ToolsProvidedByPlugins = new ObservableCollection<MenuItem>();
            this.AvailableBranches = new List<Resource>();

            this.configurationHelper = new ConfigurationHelper(vsVersion);
            this.SonarKeyTranslator = new SQKeyTranslator();
            this.SonarRestConnector = new SonarRestService(new JsonSonarConnector());
            this.Logger = new VsSonarExtensionLogger(this, vsVersion);
            this.VSonarQubeOptionsViewData = new VSonarQubeOptionsViewModel(this, this.configurationHelper);
            this.VSonarQubeOptionsViewData.ResetUserData();
            this.SourceControlProvider = new SourceControlModel();

            this.AssociationModule = new AssociationModel(this.SonarKeyTranslator, this.Logger, this.SonarRestConnector, this.configurationHelper, this.SourceControlProvider, this);

            this.SonarVersion = 4.5;
            this.CanConnectEnabled = true;
            this.ConnectionTooltip = "Not Connected";
            this.BackGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;
            this.IsExtensionBusy = false;
            
            this.InitCommands();
        }

        public SonarQubeViewModel(string vsVersion, IConfigurationHelper helper)
        {
            this.ToolsProvidedByPlugins = new ObservableCollection<MenuItem>();
            this.AvailableBranches = new List<Resource>();

            this.configurationHelper = helper;
            this.SonarKeyTranslator = new SQKeyTranslator();
            this.SonarRestConnector = new SonarRestService(new JsonSonarConnector());
            this.Logger = new VsSonarExtensionLogger(this, vsVersion);
            this.VSonarQubeOptionsViewData = new VSonarQubeOptionsViewModel(this, this.configurationHelper);
            this.VSonarQubeOptionsViewData.ResetUserData();
            this.SourceControlProvider = new SourceControlModel();

            this.AssociationModule = new AssociationModel(this.SonarKeyTranslator, this.Logger, this.SonarRestConnector, this.configurationHelper, this.SourceControlProvider, this);

            this.SonarVersion = 4.5;
            this.CanConnectEnabled = true;
            this.ConnectionTooltip = "Not Connected";
            this.BackGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;
            this.IsExtensionBusy = false;

            this.InitCommands();
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
        ///     Gets or sets a value indicating whether is connected.
        /// </summary>
        public bool IsConnected { get; set; }

        public bool IsBranchSelectionEnabled { get; set; }
        
        public List<Resource> AvailableBranches { get; set; }

        public Resource SelectedBranch { get; set; }

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
        ///     Gets or sets the disconnect to server command.
        /// </summary>
        public RelayCommand DisconnectToServerCommand { get; set; }

        public RelayCommand StartAssociationWindowCommand { get; set; }

        /// <summary>
        ///     Gets or sets the document in view.
        /// </summary>
        public string DocumentInView { get; set; }

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
        ///     Gets or sets a value indicating whether refresh theme.
        /// </summary>
        [AlsoNotifyFor("BackGroundColor", "ForeGroundColor")]
        public bool RefreshTheme { get; set; }

        /// <summary>
        ///     Gets or sets the resource in editor.
        /// </summary>
        public Resource ResourceInEditor { get; set; }

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
        ///     Gets or sets the connection tooltip.
        /// </summary>
        public string ConnectionTooltip { get; set; }

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
            var solution = solutionName;

            if (string.IsNullOrEmpty(solution))
            {
                return;
            }

            if (!solution.ToLower().EndsWith(".sln"))
            {
                solution += ".sln";
            }

            this.AssociationModule.StartAutoAssociation(solution, solutionPath);
        }

        /// <summary>
        ///     The clear project association.
        /// </summary>
        public void ClearProjectAssociation()
        {
            this.AssociationModule.ClearProjectAssociation();

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
                    Context.GlobalPropsId, 
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
                                                     this.AssociationModule.AssociatedProject, 
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
        public void InitModelFromPackageInitialization(
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
            this.NotificationManager = new NotifyCationManager(this, this.VsHelper);

            this.VSonarQubeOptionsViewData.InitPuginSystem(vsenvironmenthelperIn, this.PluginControl, this.NotificationManager);

            this.InitMenus();
            this.InitViews();
            this.SourceControlProvider.UpdatePlugins(this.VSonarQubeOptionsViewData.PluginManager.SourceCodePlugins);

            if (this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.IsConnectAtStartOn)
            {
                this.OnConnectToSonar(false);
            }

            this.UpdateTheme(Colors.Black, Colors.White);

            foreach (IAnalysisViewModelBase view in this.SonarQubeViews)
            {
                view.UpdateServices(restServiceIn, vsenvironmenthelperIn, this.configurationHelper, statusBar, provider);
            }

            this.AssociationModule.UpdateServices(restServiceIn, vsenvironmenthelperIn, statusBar, provider);
            this.VSonarQubeOptionsViewData.UpdateServices(restServiceIn, vsenvironmenthelperIn, statusBar, provider);
            foreach (IOptionsModelBase view in this.VSonarQubeOptionsViewData.AvailableOptionsModels)
            {
                view.UpdateServices(restServiceIn, vsenvironmenthelperIn, this.configurationHelper, statusBar, provider);
            }
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

        public void OnConnectToSonarCommand()
        {
            OnConnectToSonar(true);
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


        public void OnSelectedBranchChanged()
        {
            if (this.SelectedBranch != null)
            {
                this.AssociationModule.AssociatedProject = this.SelectedBranch;
                this.RefreshDataForResource(this.DocumentInView);
            }
        }

        internal void SyncSettings()
        {
            this.VSonarQubeOptionsViewData.RefreshPropertiesInView(this.AssociationModule.AssociatedProject);

            if (this.SonarQubeViews != null)
            {
                foreach (var view in this.SonarQubeViews)
                {
                    view.InitDataAssociation(this.AssociationModule.AssociatedProject, AuthtenticationHelper.AuthToken, this.AssociationModule.OpenSolutionPath);
                }
            }
        }

        public void RefreshDataForResource()
        {
            if (this.ServerViewModel != null)
            {
                this.ServerViewModel.ResetStats();
            }

            if (this.LocalViewModel != null)
            {
                this.LocalViewModel.ResetStats();
            }

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
                    this.NotificationManager.ReportException(ex);
                }
            }
        }

        /// <summary>The refresh data for resource.</summary>
        /// <param name="fullName">The full name.</param>
        public void RefreshDataForResource(string fullName)
        {
            this.Logger.WriteMessage("Refresh Data For File: " + fullName);
            if (string.IsNullOrEmpty(fullName) || this.AssociationModule.AssociatedProject == null)
            {
                return;
            }

            this.DocumentInView = fullName;
            this.ResourceInEditor = null;
            if (this.LocalViewModel.FileAnalysisIsEnabled)
            {
                this.LocalViewModel.IssuesGridView.AllIssues.Clear();
                this.LocalViewModel.IssuesGridView.Issues.Clear();
                this.LocalViewModel.ResetStats();
            }

            this.ServerViewModel.IssuesGridView.AllIssues.Clear();
            this.ServerViewModel.IssuesGridView.Issues.Clear();
            this.ServerViewModel.ResetStats();

            this.ResourceInEditor = this.AssociationModule.CreateAValidResourceFromServer(fullName, this.AssociationModule.AssociatedProject);
            if (this.ResourceInEditor == null &&
                this.SelectedView == this.LocalViewModel &&
                this.LocalViewModel.FileAnalysisIsEnabled)
            {
                this.ResourceInEditor = this.AssociationModule.CreateResourcePathFile(fullName, this.AssociationModule.AssociatedProject);
            }
                
            if (this.ResourceInEditor == null)
            {
                this.StatusMessage = "Could not check key type for resource, see VSonarOutput Window for more details and report";
                return;
            }
           
            IAnalysisViewModelBase analyser = this.SelectedView;
            if (analyser != null)
            {
                try
                {
                    this.Logger.WriteMessage("RefreshDataForResource: Doc in View: " + this.DocumentInView);
                    analyser.RefreshDataForResource(this.ResourceInEditor, this.DocumentInView);
                    this.StatusMessage = this.ResourceInEditor.Key;
                }
                catch (Exception ex)
                {
                    this.StatusMessage = "Cannot find file in server: " + ex.Message;
                    this.Logger.WriteMessage("Cannot find file in server: " + ex.Message);
                    this.Logger.WriteException(ex);
                    this.NotificationManager.ReportException(ex);
                }
            }
        }

        public void ProjectHasBuild(VsProjectItem project)
        {
            IAnalysisViewModelBase analyser = this.SelectedView;
            if (analyser != null)
            {
                analyser.TriggerAProjectAnalysis(project);
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

        /// <summary>
        ///     The init commands.
        /// </summary>
        private void InitCommands()
        {
            this.LaunchExtensionPropertiesCommand = new RelayCommand(this.LaunchExtensionProperties);
            this.ToolSwitchCommand = new RelayCommand<string>(this.ExecutePlugin);

            this.ClearCacheCommand = new RelayCommand(this.OnClearCacheCommand);

            this.ConnectCommand = new RelayCommand(this.OnConnectCommand);
            this.ConnectedToServerCommand = new RelayCommand(this.OnConnectToSonarCommand);
            this.DisconnectToServerCommand = new RelayCommand(this.OnDisconnectToSonar);
            this.StartAssociationWindowCommand = new RelayCommand(this.OnStartAssociationWindowCommand);
        }

        private void OnStartAssociationWindowCommand()
        {
            this.AssociationModule.ShowAssociationWindow();
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
            this.VSonarQubeOptionsViewData.RefreshPropertiesInView(this.AssociationModule.AssociatedProject);
            var window = new VSonarQubeOptionsView(this.VSonarQubeOptionsViewData);
            window.ShowDialog();
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
                this.OnConnectToSonar(true);
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
            this.AvailableBranches.Clear();
            this.SelectedBranch = null;
            this.AssociationModule.Disconnect();

            foreach (IAnalysisViewModelBase view in this.SonarQubeViews)
            {
                view.EndDataAssociation();
            }
                     
            this.LocalViewModel.IssuesGridView.AllIssues.Clear();
            this.LocalViewModel.IssuesGridView.Issues.Clear();
            this.ServerViewModel.IssuesGridView.AllIssues.Clear();
            this.ServerViewModel.IssuesGridView.Issues.Clear();
            this.IssuesSearchViewModel.IssuesGridView.Issues.Clear();
            this.IssuesSearchViewModel.IssuesGridView.AllIssues.Clear();

            this.ConnectionTooltip = "Not Connected";
            this.StatusMessage = "";
        }

        /// <summary>
        ///     The connect to sonar.
        /// </summary>
        private void OnConnectToSonar(bool useDispatcher)
        {
            if (this.VsHelper == null)
            {
                return;
            }

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
                this.TryToConnect(useDispatcher);
                this.AssociateProjectToSolution(this.VsHelper.ActiveSolutionName(), this.VsHelper.ActiveSolutionPath());
            };

            bw.RunWorkerAsync();
        }

        /// <summary>
        ///     The try to connect.
        /// </summary>
        private void TryToConnect(bool useDispatcher)
        {
            try
            {
                if (AuthtenticationHelper.AuthToken == null)
                {
                    this.StatusMessage = "No credentials available Go To: Menu > Configuration in this window";
                    this.IsConnected = false;
                    return;
                }

                this.StatusMessage = "";

                this.IsConnected = true;
                this.AssociationModule.IsConnectedButNotAssociated = true;
                this.AssociationModule.IsAssociated = false;

                this.ConnectionTooltip = "Authenticated, but no associated";
                this.AssociationModule.RefreshProjectList(useDispatcher);

                this.SonarVersion =
                    this.SonarRestConnector.GetServerInfo(AuthtenticationHelper.AuthToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                this.NotificationManager.ReportException(ex);
                this.ConnectionTooltip = "No Connection";
                this.AssociationModule.Disconnect();
            }
        }

        #endregion

        /// <summary>The configuration helper.</summary>
        private readonly IConfigurationHelper configurationHelper;

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

        public NotifyCationManager NotificationManager { get; internal set; }
        public AssociationModel AssociationModule { get; internal set; }
        public ISQKeyTranslator SonarKeyTranslator { get; internal set; }
        internal ISourceControlProvider SourceControlProvider { get; private set; }
        public string StatusMessage { get; set; }
    }
}