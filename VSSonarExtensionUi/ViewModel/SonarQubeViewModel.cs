// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarQubeViewModel.cs" company="Copyright © 2015 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2014 [Jorge Costa, Jorge.Costa@tekla.com]
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
namespace VSSonarExtensionUi.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using Analysis;
    using Configuration;
    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using Model.Analysis;
    using Model.Association;
    using Model.Cache;
    using Model.Helpers;
    using Model.PluginManager;
    using PropertyChanged;
    using SonarLocalAnalyser;
    using SonarRestService;
    using View.Configuration;
    using VSSonarPlugins;
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
    public class SonarQubeViewModel
    {
        /// <summary>
        /// The view model pool
        /// </summary>
        private static List<IViewModelBase> viewModelPool = new List<IViewModelBase>();

        /// <summary>
        ///     The analysisPlugin control.
        /// </summary>
        private readonly PluginController pluginController = new PluginController();

        /// <summary>
        ///     The menu item plugins.
        /// </summary>
        private readonly Dictionary<int, IMenuCommandPlugin> menuItemPlugins = new Dictionary<int, IMenuCommandPlugin>();

        /// <summary>
        /// The configuration helper.
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        /// The vs version
        /// </summary>
        private readonly string vsversion;

        /// <summary>
        /// The notification manager
        /// </summary>
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// The sonar key translator
        /// </summary>
        private readonly SQKeyTranslator sonarKeyTranslator;

        /// <summary>
        /// The sonar rest connector
        /// </summary>
        private readonly SonarRestService sonarRestConnector;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeViewModel"/> class.
        /// </summary>
        public SonarQubeViewModel()
        {
            this.notificationManager = new NotifyCationManager(this, "standalone");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeViewModel" /> class. Plugins are initialized in InitModelFromPackageInitialization below
        /// </summary>
        /// <param name="vsverionIn">The vs version.</param>
        public SonarQubeViewModel(string vsverionIn)
        {
            this.vsversion = vsverionIn;
            this.ToolsProvidedByPlugins = new ObservableCollection<MenuItem>();
            this.AvailableBranches = new List<Resource>();

            this.notificationManager = new NotifyCationManager(this, vsverionIn);
            this.configurationHelper = new ConfigurationHelper(vsverionIn, this.notificationManager);
            this.sonarKeyTranslator = new SQKeyTranslator();
            this.sonarRestConnector = new SonarRestService(new JsonSonarConnector());
            this.VSonarQubeOptionsViewData = new VSonarQubeOptionsViewModel(this.sonarRestConnector, this.configurationHelper, this.notificationManager);
            this.VSonarQubeOptionsViewData.ResetUserData();

            this.CanConnectEnabled = true;
            this.ConnectionTooltip = "Not Connected";
            this.BackGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;
            this.IsExtensionBusy = false;
            
            this.InitCommands();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeViewModel" /> class.
        /// </summary>
        /// <param name="vsverionIn">The vsverion in.</param>
        /// <param name="helper">The helper.</param>
        public SonarQubeViewModel(string vsverionIn, IConfigurationHelper helper)
        {
            this.vsversion = vsverionIn;
            this.ToolsProvidedByPlugins = new ObservableCollection<MenuItem>();
            this.AvailableBranches = new List<Resource>();

            this.configurationHelper = helper;
            this.notificationManager = new NotifyCationManager(this, vsverionIn);
            this.sonarKeyTranslator = new SQKeyTranslator();
            this.sonarRestConnector = new SonarRestService(new JsonSonarConnector());
            this.VSonarQubeOptionsViewData = new VSonarQubeOptionsViewModel(this.sonarRestConnector, this.configurationHelper, this.notificationManager);
            this.VSonarQubeOptionsViewData.ResetUserData();

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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is branch selection enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is branch selection enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsBranchSelectionEnabled { get; set; }

        /// <summary>
        /// Gets or sets the available branches.
        /// </summary>
        /// <value>
        /// The available branches.
        /// </value>
        public List<Resource> AvailableBranches { get; set; }

        /// <summary>
        /// Gets or sets the selected branch.
        /// </summary>
        /// <value>
        /// The selected branch.
        /// </value>
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

        /// <summary>
        /// Gets or sets the start association window command.
        /// </summary>
        /// <value>
        /// The start association window command.
        /// </value>
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
        ///     Gets or sets the launch extension properties command.
        /// </summary>
        public RelayCommand LaunchExtensionPropertiesCommand { get; set; }

        /// <summary>
        ///     Gets or sets the local view viewModel.
        /// </summary>
        public LocalViewModel LocalViewModel { get; set; }

        /// <summary>
        ///     Gets or sets the resource in editor.
        /// </summary>
        public Resource ResourceInEditor { get; set; }

        /// <summary>
        ///     Gets or sets the selected view.
        /// </summary>
        public IAnalysisModelBase SelectedModel { get; set; }

        /// <summary>
        /// Gets or sets the selected view.
        /// </summary>
        /// <value>
        /// The selected view.
        /// </value>
        public IViewModelBase SelectedViewModel { get; set; }

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
        public ObservableCollection<IAnalysisModelBase> SonarQubeModels { get; set; }

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

        /// <summary>Gets or sets the extension folder.</summary>
        public string ExtensionFolder { get; set; }

        /// <summary>
        /// Gets the association module.
        /// </summary>
        /// <value>
        /// The association module.
        /// </value>
        public AssociationModel AssociationModule { get; internal set; }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        /// <value>
        /// The status message.
        /// </value>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Gets the issues search model.
        /// </summary>
        /// <value>
        /// The issues search model.
        /// </value>
        public IssuesSearchModel IssuesSearchModel { get; private set; }

        /// <summary>
        /// Gets the sonar qube views.
        /// </summary>
        /// <value>
        /// The sonar qube views.
        /// </value>
        public ObservableCollection<IViewModelBase> SonarQubeViews { get; private set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public INotificationManager Logger
        {
            get
            {
                return this.notificationManager;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Registers the new view model in pool.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public static void RegisterNewViewModelInPool(IViewModelBase viewModel)
        {
            viewModelPool.Add(viewModel);
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

            this.AssociationModule.StartAutoAssociation(solution, solutionPath);
        }

        /// <summary>
        ///     The clear project association.
        /// </summary>
        public void ClearProjectAssociation()
        {
            this.AvailableBranches.Clear();
            this.SelectedBranch = null;
            this.StatusMessage = string.Empty;
            this.ConnectionTooltip = "Connected but not associated";
            this.AssociationModule.EndAssociationAndClearData();
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

        /// <summary>
        /// The extension data viewModel update.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="extensionFolder">The extension Folder.</param>
        public void InitModelFromPackageInitialization(
            IVsEnvironmentHelper vsenvironmenthelperIn, 
            IVSSStatusBar statusBar, 
            IServiceProvider provider, 
            string extensionFolder)
        {
            this.ServiceProvider = provider;
            this.VsHelper = vsenvironmenthelperIn;
            this.StatusBar = statusBar;
            this.ExtensionFolder = extensionFolder;

            // register notificaton manager, since all messages will be show also for initialization
            (this.notificationManager as IModelBase).UpdateServices(this.VsHelper, this.StatusBar, this.ServiceProvider);
            this.VSonarQubeOptionsViewData.InitPuginSystem(vsenvironmenthelperIn, this.pluginController, this.notificationManager);
            this.InitMenus();
            this.InitViewsAndModels();

            // start association module after all models are started
            this.AssociationModule = new AssociationModel(
                this.notificationManager,
                this.sonarRestConnector,
                this.configurationHelper,
                this.sonarKeyTranslator,
                this.VSonarQubeOptionsViewData.PluginManager,
                this);

            this.AssociationModule.UpdateServicesInModels(this.VsHelper, this.StatusBar, this.ServiceProvider);
            this.UpdateTheme(Colors.Black, Colors.White);

            // try to connect to start to sonar if on start is on
            if (this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.IsConnectAtStartOn)
            {
                this.OnConnectToSonar(false);
            }
        }

        /// <summary>
        /// The get issues in editor.
        /// </summary>
        /// <param name="fileResource">The file Resource.</param>
        /// <param name="fileContent">The file Content.</param>
        /// <returns>
        /// The<see><cref>List</cref></see>
        /// .
        /// </returns>
        public List<Issue> GetIssuesInEditor(Resource fileResource, string fileContent)
        {
            this.notificationManager.WriteMessage("Return issues for resource: " + fileResource);
            if (this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.DisableEditorTags)
            {
                this.notificationManager.WriteMessage("Return issues for resource, tags disabled");
                return new List<Issue>();
            }

            IAnalysisModelBase view = this.SelectedModel;

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
        /// Called when [connect to sonar command].
        /// </summary>
        public void OnConnectToSonarCommand()
        {
            this.OnConnectToSonar(true);
        }

        /// <summary>
        ///     The on issues change event.
        /// </summary>
        public void OnIssuesChangeEvent()
        {
            this.SelectedModel.OnAnalysisModeHasChange(EventArgs.Empty);
        }

        /// <summary>
        ///     The on selected view changed.
        /// </summary>
        public void OnSelectedViewModelChanged()
        {
            this.configurationHelper.WriteSetting(
                new SonarQubeProperties
                    {
                        Context = Context.UIProperties, 
                        Key = "SelectedView", 
                        Owner = OwnersId.ApplicationOwnerId, 
                        Value = this.SelectedViewModel.Header
                    }, true);

            foreach (IAnalysisModelBase model in this.SonarQubeModels)
            {
                var modelBase = model as IModelBase;

                if (modelBase.GetViewModel().Equals(this.SelectedViewModel))
                {
                    this.SelectedModel = model;
                }
            }

            this.OnAnalysisModeHasChange(EventArgs.Empty);
            Debug.WriteLine("Name Changed");
        }


        /// <summary>
        /// Called when [selected branch changed].
        /// </summary>
        public void OnSelectedBranchChanged()
        {
            if (this.SelectedBranch != null && this.AssociationModule != null)
            {
                this.AssociationModule.UpdateBranchSelection(this.SelectedBranch);
                this.RefreshDataForResource(this.DocumentInView);
            }
        }

        /// <summary>
        /// Refreshes the data for resource.
        /// </summary>
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

            IAnalysisModelBase analyser = this.SelectedModel;
            if (analyser != null)
            {
                try
                {
                    this.notificationManager.WriteMessage("RefreshDataForResource: Doc in View: " + this.DocumentInView);
                    analyser.RefreshDataForResource(this.ResourceInEditor, this.DocumentInView);
                }
                catch (Exception ex)
                {
                    this.notificationManager.WriteMessage("Cannot find file in server: " + ex.Message);
                    this.notificationManager.WriteException(ex);
                    this.notificationManager.ReportException(ex);
                }
            }
        }

        /// <summary>
        /// The refresh data for resource.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        public void RefreshDataForResource(string fullName)
        {
            this.notificationManager.WriteMessage("Refresh Data For File: " + fullName);
            if (string.IsNullOrEmpty(fullName) || this.AssociationModule.AssociatedProject == null)
            {
                return;
            }

            this.DocumentInView = fullName;
            this.ResourceInEditor = null;
            if (this.LocalViewModel.FileAnalysisIsEnabled)
            {
                this.LocalViewModel.ClearIssues();
                this.LocalViewModel.ResetStats();
            }

            this.ServerViewModel.ClearIssues();
            this.ServerViewModel.ResetStats();

            this.ResourceInEditor = this.AssociationModule.CreateAValidResourceFromServer(fullName, this.AssociationModule.AssociatedProject);
            if (this.ResourceInEditor == null &&
                this.SelectedModel == this.LocalViewModel &&
                this.LocalViewModel.FileAnalysisIsEnabled)
            {
                this.ResourceInEditor = this.AssociationModule.CreateResourcePathFile(fullName, this.AssociationModule.AssociatedProject);
            }
                
            if (this.ResourceInEditor == null)
            {
                this.StatusMessage = "Could not check key type for resource, see VSonarOutput Window for more details and report";
                return;
            }
           
            IAnalysisModelBase analyser = this.SelectedModel;
            if (analyser != null)
            {
                try
                {
                    this.notificationManager.WriteMessage("RefreshDataForResource: Doc in View: " + this.DocumentInView);
                    analyser.RefreshDataForResource(this.ResourceInEditor, this.DocumentInView);
                    this.StatusMessage = this.ResourceInEditor.Key;
                }
                catch (Exception ex)
                {
                    this.StatusMessage = "Cannot find file in server: " + ex.Message;
                    this.notificationManager.WriteMessage("Cannot find file in server: " + ex.Message);
                    this.notificationManager.WriteException(ex);
                    this.notificationManager.ReportException(ex);
                }
            }
        }

        /// <summary>
        /// Projects the has build.
        /// </summary>
        /// <param name="project">The project.</param>
        public void ProjectHasBuild(VsProjectItem project)
        {
            IAnalysisModelBase analyser = this.SelectedModel;
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

            foreach (var view in viewModelPool)
            {
                view.UpdateColours(background, foreground);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get first free id.
        /// </summary>
        /// <returns>
        /// The <see cref="int" />.
        /// </returns>
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
                    KeyValuePair<int, IMenuCommandPlugin> selectMenu = this.InUsePlugin;
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
        /// The closed window.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        public void ClosedWindow(string fullName)
        {
            if (this.ServerViewModel != null)
            {
                this.ServerViewModel.UpdateOpenDiffWindowList(fullName);
            }
        }

        /// <summary>
        /// The get coverage in editor.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The <see cref="Dictionary" />.
        /// </returns>
        public Dictionary<int, CoverageElement> GetCoverageInEditor(string buffer)
        {
            if (this.SelectedModel == this.ServerViewModel)
            {
                return this.ServerViewModel.GetCoverageInEditor(buffer);
            }

            return new Dictionary<int, CoverageElement>();
        }

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

        /// <summary>
        /// Called when [start association window command].
        /// </summary>
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
        private void InitViewsAndModels()
        {
            this.ServerViewModel = new ServerViewModel(
                this.VsHelper, 
                this.configurationHelper, 
                this.sonarRestConnector,
                this.notificationManager,
                this.sonarKeyTranslator);

            this.LocalViewModel = new LocalViewModel(
                this.VSonarQubeOptionsViewData.PluginManager.AnalysisPlugins,
                this.sonarRestConnector,
                this.configurationHelper,
                this.notificationManager,
                this.sonarKeyTranslator);

            this.IssuesSearchModel = new IssuesSearchModel(
                this.configurationHelper,
                this.sonarRestConnector, 
                this.notificationManager, 
                this.sonarKeyTranslator);
            
            this.SonarQubeModels = new ObservableCollection<IAnalysisModelBase>
                                      {
                                          this.ServerViewModel, 
                                          this.LocalViewModel, 
                                          this.IssuesSearchModel
                                      };

            this.SonarQubeViews = new ObservableCollection<IViewModelBase>
                                      {
                                          this.ServerViewModel.GetViewModel() as IViewModelBase,
                                          this.LocalViewModel.GetViewModel() as IViewModelBase,
                                          this.IssuesSearchModel.GetViewModel() as IViewModelBase
                                      };

            try
            {
                string view = this.configurationHelper.ReadSetting(Context.UIProperties, OwnersId.ApplicationOwnerId, "SelectedView").Value;

                foreach (IViewModelBase analysisViewModelBase in
                    this.SonarQubeViews.Where(analysisViewModelBase => analysisViewModelBase.Header.Equals(view)))
                {
                    this.SelectedModel = analysisViewModelBase.GetAvailableModel() as IAnalysisModelBase;
                    this.SelectedViewModel = analysisViewModelBase;
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
        public void LaunchExtensionProperties()
        {
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
            this.ClearProjectAssociation();
            this.AssociationModule.Disconnect();           
            this.ConnectionTooltip = "Not Connected";
        }

        /// <summary>
        /// The connect to sonar.
        /// </summary>
        /// <param name="useDispatcher">if set to <c>true</c> [use dispatcher].</param>
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
        /// The try to connect.
        /// </summary>
        /// <param name="useDispatcher">if set to <c>true</c> [use dispatcher].</param>
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

                this.StatusMessage = string.Empty;

                this.IsConnected = true;
                this.AssociationModule.IsConnectedButNotAssociated = true;
                this.AssociationModule.IsAssociated = false;

                this.ConnectionTooltip = "Authenticated, but no associated";
                this.AssociationModule.RefreshProjectList(useDispatcher);
            }
            catch (Exception ex)
            {
                this.notificationManager.ReportMessage(new Message { Id = "SonarQubeViewModel", Data = "Fail To Connect To SonarQube: " + ex.Message });
                this.notificationManager.ReportException(ex);
                this.ConnectionTooltip = "No Connection";
                this.AssociationModule.Disconnect();
            }
        }

        #endregion
    }
}