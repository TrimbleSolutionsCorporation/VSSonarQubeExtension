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
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    using Analysis;
    using Configuration;
    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using Model.Analysis;
    using Model.Cache;
    using Model.Helpers;
    using Model.PluginManager;
    using PropertyChanged;
    using SonarLocalAnalyser;
    using SonarRestService;
    using View.Configuration;
    using View.Helpers;
    using VSSonarExtensionUi.Association;
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
        /// The new added issues
        /// </summary>
        private readonly Dictionary<string, List<Issue>> newAddedIssues = new Dictionary<string, List<Issue>>();

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
        private readonly ISQKeyTranslator sonarKeyTranslator;

        /// <summary>
        /// The sonar rest connector
        /// </summary>
        private readonly ISonarRestService sonarRestConnector;

        /// <summary>
        /// The source control
        /// </summary>
        private ISourceControlProvider sourceControl;

        /// <summary>
        /// The show flyouts
        /// </summary>
        private bool showRightFlyout;

        /// <summary>
        /// The plugin manager
        /// </summary>
        private IPluginManager pluginManager;

        /// <summary>
        /// The can provision
        /// </summary>
        private bool canProvision;

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
            this.AvailableProjects = new ObservableCollection<Resource>();

            this.notificationManager = new NotifyCationManager(this, vsverionIn);
            this.configurationHelper = new ConfigurationHelper(vsverionIn, this.notificationManager);
            this.sonarKeyTranslator = new SQKeyTranslator(this.notificationManager);
            this.sonarRestConnector = new SonarRestService(new JsonSonarConnector());
            this.VSonarQubeOptionsViewData = new VSonarQubeOptionsViewModel(this.sonarRestConnector, this.configurationHelper, this.notificationManager);
            this.VSonarQubeOptionsViewData.ResetUserData();

            this.CanConnectEnabled = true;
            this.ConnectionTooltip = "Not Connected";
            this.BackGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;
            this.ErrorIsFound = false;
            this.IsExtensionBusy = false;
            this.ShowRightFlyout = false;
            this.InitCommands();
            this.NumberNewIssues = "0";
        }

        /// <summary>
        /// Determines whether this instance [can close solution].
        /// </summary>
        /// <returns></returns>
        public int CanCloseSolution()
        {
            if (this.NewIssuesFound)
            {
                if (QuestionUser.GetInput(
                    "You are about the close solution after technical debt has been added. Are you sure you want to exit."))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }

            return 0;
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
            this.AvailableProjects = new ObservableCollection<Resource>();

            this.configurationHelper = helper;
            this.notificationManager = new NotifyCationManager(this, vsverionIn);
            this.sonarKeyTranslator = new SQKeyTranslator(this.notificationManager);
            this.sonarRestConnector = new SonarRestService(new JsonSonarConnector());
            this.VSonarQubeOptionsViewData = new VSonarQubeOptionsViewModel(this.sonarRestConnector, this.configurationHelper, this.notificationManager);
            this.VSonarQubeOptionsViewData.ResetUserData();

            this.CanConnectEnabled = true;
            this.ConnectionTooltip = "Not Connected";
            this.BackGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;
            this.ErrorIsFound = true;
            this.IsExtensionBusy = false;

            this.InitCommands();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeViewModel" /> class.
        /// </summary>
        /// <param name="vsverionIn">The vsverion in.</param>
        /// <param name="helper">The helper.</param>
        /// <param name="notification">The notification.</param>
        /// <param name="translator">The translator.</param>
        /// <param name="restService">The rest service.</param>
        /// <param name="sourceControl">The source control.</param>
        /// <param name="pluginManager">The plugin manager.</param>
        /// <param name="locaAnalyser">The loca analyser.</param>
        public SonarQubeViewModel(
            string vsverionIn,
            IConfigurationHelper helper,
            INotificationManager notification,
            ISQKeyTranslator translator,
            ISonarRestService restService,
            ISourceControlProvider sourceControl = null,
            IPluginManager pluginManager = null,
            ISonarLocalAnalyser locaAnalyser = null)
        {
            this.vsversion = vsverionIn;
            this.ToolsProvidedByPlugins = new ObservableCollection<MenuItem>();
            this.AvailableProjects = new ObservableCollection<Resource>();

            this.pluginManager = pluginManager;
            this.sourceControl = sourceControl;
            this.configurationHelper = helper;
            this.notificationManager = notification;
            this.sonarKeyTranslator = translator;
            this.sonarRestConnector = restService;
            this.LocaAnalyser = locaAnalyser;
            this.VSonarQubeOptionsViewData =
                new VSonarQubeOptionsViewModel(
                    this.sonarRestConnector,
                    this.configurationHelper,
                    this.notificationManager);

            this.VSonarQubeOptionsViewData.ResetUserData();

            // start association module after all models are started
            this.AssociationModule = new AssociationModel(
                this.notificationManager,
                this.sonarRestConnector,
                this.configurationHelper,
                this.sonarKeyTranslator,
                this.pluginManager,
                this,
                this.LocaAnalyser);

            this.CanConnectEnabled = true;
            this.ConnectionTooltip = "Not Connected";
            this.BackGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;
            this.ErrorIsFound = true;
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

        /// <summary>
        ///     Gets or sets a value indicating whether show flyouts.
        /// </summary>
        public bool ShowRightFlyout
        {
            get
            {
                return this.showRightFlyout;
            }

            set
            {
                this.showRightFlyout = value;
                this.SizeOfFlyout = value ? 250 : 0;
            }
        }

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
                    this.sourceControl = new SourceControlModel(
                        this.pluginManager.SourceCodePlugins,
                        this.AssociationModule.OpenSolutionPath,
                        this.Logger,
                        this.sonarRestConnector);
                }

                return this.sourceControl;
            }
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
        ///     Gets or sets the assign project command.
        /// </summary>
        public RelayCommand AssignProjectCommand { get; set; }

        /// <summary>
        ///     Gets or sets the size of flyout.
        /// </summary>
        public int SizeOfFlyout { get; set; }

        /// <summary>
        /// Gets or sets the error colour.
        /// </summary>
        /// <value>
        /// The error colour.
        /// </value>
        public bool ErrorIsFound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is solution open.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is solution open; otherwise, <c>false</c>.
        /// </value>
        public bool IsSolutionOpen { get; set; }

        /// <summary>
        /// Gets or sets the error message tooltip.
        /// </summary>
        /// <value>
        /// The error message tooltip.
        /// </value>
        public string ErrorMessageTooltip { get; set; }

        /// <summary>
        /// Gets or sets the id of plugin to remove.
        /// </summary>
        /// <value>
        /// The identifier of plugin to remove.
        /// </value>
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
        ///     Gets the close flyout issue search command.
        /// </summary>
        public RelayCommand CloseRightFlyoutCommand { get; private set; }

        /// <summary>
        /// Gets the show new local issues command.
        /// </summary>
        /// <value>
        /// The show new local issues command.
        /// </value>
        public ICommand ShowNewLocalIssuesCommand { get; private set; }

        /// <summary>
        /// Gets the provision project command.
        /// </summary>
        /// <value>
        /// The provision project command.
        /// </value>
        public ICommand ProvisionProjectCommand { get; private set; }

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
        /// Gets or sets the status message association.
        /// </summary>
        /// <value>
        /// The status message association.
        /// </value>
        public string StatusMessageAssociation { get; set; }

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
        /// Gets or sets a value indicating whether this instance can provision.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can provision; otherwise, <c>false</c>.
        /// </value>
        public bool CanProvision
        {
            private set
            {
                this.canProvision = value;

                if (AuthtenticationHelper.AuthToken.SonarVersion < 5.2)
                {
                    this.canProvision = false;
                }                
            }

            get
            {
                return this.canProvision;
            }
        }

        /// <summary>
        /// Gets or sets the number new issues.
        /// </summary>
        /// <value>
        /// The number new issues.
        /// </value>
        [AlsoNotifyFor("NewIssuesFound")]
        public string NumberNewIssues { get; set; }

        /// <summary>
        /// Gets a value indicating whether [new issues found].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [new issues found]; otherwise, <c>false</c>.
        /// </value>
        public bool NewIssuesFound
        {
            get
            {
                return !(string.IsNullOrEmpty(this.NumberNewIssues) || this.NumberNewIssues.Equals("0"));
            }
        }

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

        /// <summary>
        /// Gets the loca analyser.
        /// </summary>
        /// <value>
        /// The loca analyser.
        /// </value>
        public ISonarLocalAnalyser LocaAnalyser { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [analysis from solution running].
        /// </summary>
        /// <value>
        /// <c>true</c> if [analysis from solution running]; otherwise, <c>false</c>.
        /// </value>
        public bool AnalysisFromSolutionRunning { get; private set; }

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


        /// <summary>
        /// Called when [solution open].
        /// </summary>
        /// <param name="solutionName">Name of the solution.</param>
        /// <param name="solutionPath">The solution path.</param>
        /// <param name="fileInView">The file in view.</param>
        public void OnSolutionOpen(string solutionName, string solutionPath, string fileInView, ISourceControlProvider sourceProvider = null)
        {
            this.ErrorIsFound = false;
            this.IsSolutionOpen = true;

            if (sourceProvider == null)
            {
                // create new source control provider
                this.sourceControl = new SourceControlModel(
                    this.pluginManager.SourceCodePlugins,
                    solutionPath,
                    this.Logger,
                    this.sonarRestConnector);
            }
            else
            {
                this.sourceControl = sourceProvider;
            }

            this.AssociationModule.AssociateProjectToSolution(solutionName, solutionPath, this.AvailableProjects, this.SourceControl);


            if (!string.IsNullOrEmpty(fileInView) && File.Exists(fileInView))
            {
                this.RefreshDataForResource(fileInView, File.ReadAllText(fileInView), false);
            }

            this.SetupAssociationMessages();
        }

        /// <summary>
        /// Refreshes the project list.
        /// </summary>
        /// <param name="useDispatcher">if set to <c>true</c> [use dispatcher].</param>
        public void RefreshProjectList(bool useDispatcher)
        {
            List<Resource> projectsRaw = this.sonarRestConnector.GetProjectsList(AuthtenticationHelper.AuthToken);
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
        /// Called when [solution closed].
        /// </summary>
        public void OnSolutionClosed()
        {
            this.IsSolutionOpen = false;

            // clear all messages related with association
            this.StatusMessage = string.Empty;
            this.ConnectionTooltip = "Connected but not associated";
            this.StatusMessageAssociation = "Not associated with any project";
            this.SelectedBranchProject = null;
            this.SelectedProjectInView = null;
            this.SelectedProjectKey = string.Empty;
            this.SelectedProjectName = string.Empty;
            this.SelectedProjectVersion = string.Empty;
            this.NumberNewIssues = "0";

            this.AssociationModule.OnSolutionClosed();
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

                string isEnabled = "true";
                try
                {
                    isEnabled = this.configurationHelper.ReadSetting(
                        Context.GlobalPropsId,
                        plugDesc.Name,
                        GlobalIds.PluginEnabledControlId).Value;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }


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
            this.pluginManager = this.VSonarQubeOptionsViewData.PluginManager;
            this.InitMenus();
            this.InitViewsAndModels();

            // start association module after all models are started
            this.AssociationModule = new AssociationModel(
                this.notificationManager,
                this.sonarRestConnector,
                this.configurationHelper,
                this.sonarKeyTranslator,
                this.VSonarQubeOptionsViewData.PluginManager,
                this,
                this.LocaAnalyser);

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
        /// <param name="showfalseandresolved">if set to <c>true</c> [showfalseandresolved].</param>
        /// <returns>
        /// The<see><cref>List</cref></see>
        /// .
        /// </returns>
        public List<Issue> GetIssuesInEditor(Resource fileResource, string fileContent, out bool showfalseandresolved)
        {
            showfalseandresolved = false;
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

            return view.GetIssuesForResource(fileResource, fileContent, out showfalseandresolved);
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

        public void StartAnalysisWindow(AnalysisTypes mode, bool fromSave)
        {
            if (!this.IsConnected)
            {
                MessageDisplayBox.DisplayMessage("You must first connect to server before you can run analysis", helpurl: "https://github.com/TeklaCorp/VSSonarQubeExtension/wiki");
                return;
            }

            if (this.AnalysisFromSolutionRunning)
            {
                MessageDisplayBox.DisplayMessage("A previous analysis is running, you need to wait for completion.");
                return;
            }


            if (mode.Equals(AnalysisTypes.ANALYSIS))
            {
                this.LocaAnalyser.LocalAnalysisCompleted += this.FullAnalysisHasCompleted;
            }
            else
            {
                this.LocaAnalyser.LocalAnalysisCompleted += this.PreviewAnalysisHasCompleted;
            }

            if (!this.AssociationModule.IsAssociated)
            {
                if (mode.Equals(AnalysisTypes.ANALYSIS))
                {
                    this.LocaAnalyser.AssociateCommandCompeted += this.RunFullAnalysisAfterAssociation;
                }
                else
                {

                    this.LocaAnalyser.AssociateCommandCompeted += this.RunPreviewAnalysisAfterAssociation;
                }

                this.ProvisionProject(false);
            }
            else
            {
                this.LocalViewModel.ErrorsFoundDuringAnalysis = false;
                this.AnalysisFromSolutionRunning = true;
                this.LocalViewModel.RunAnalysis(mode, fromSave);
            }
        }

        public void ResetAndEstablishANewConnectionToServer()
        {
            this.OnDisconnectToSonar();
            this.OnConnectToSonar(true);
        }


        /// <summary>
        /// The connect to sonar.
        /// </summary>
        /// <param name="useDispatcher">if set to <c>true</c> [use dispatcher].</param>
        public void OnConnectToSonar(bool useDispatcher)
        {
            this.ErrorIsFound = false;

            if (AuthtenticationHelper.AuthToken == null)
            {
                this.StatusMessage = "No credentials available Go To: Sonar Menu > Configuration";
                this.IsConnected = false;
                this.ErrorIsFound = true;
                return;
            }

            if (this.VsHelper == null)
            {
                this.StatusMessage = "Extension in unusable, unable to access visual studio services.";
                this.IsConnected = false;
                this.ErrorIsFound = true;
                return;
            }

            this.CanConnectEnabled = false;
            this.IsExtensionBusy = true;

            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.RunWorkerCompleted += delegate
            {
                this.CanConnectEnabled = true;
                this.IsExtensionBusy = false;
            };

            bw.DoWork += delegate
            {
                try
                {
                    this.RefreshProjectList(useDispatcher);                    
                    this.AssociationModule.OnConnectToSonar();
                    this.VSonarQubeOptionsViewData.OnConnectToSonar(AuthtenticationHelper.AuthToken, this.AvailableProjects, this.pluginManager.GetIssueTrackerPlugin());
                    this.ConnectionTooltip = "Authenticated, but not associated";
                    this.StatusMessage = string.Empty;
                    this.IsConnected = true;
                    this.AssociationModule.IsAssociated = false;
                }
                catch (Exception ex)
                {
                    this.notificationManager.ReportMessage(new Message { Id = "SonarQubeViewModel", Data = "Fail To Connect To SonarQube: " + ex.Message });
                    this.notificationManager.ReportException(ex);
                    this.ConnectionTooltip = "No Connection";
                    this.AssociationModule.Disconnect();
                    this.StatusMessage = "Failed to Connect to Sonar, check output log for details";
                    this.IsConnected = false;
                    return;
                }

                // try to associate with open solution
                if (this.IsSolutionOpen)
                {                   
                    try
                    {
                        this.AssociationModule.AssociateProjectToSolution(this.VsHelper.ActiveSolutionName(), this.VsHelper.ActiveSolutionPath(), this.AvailableProjects, this.SourceControl);

                        if (this.AssociationModule.IsAssociated)
                        {
                            this.CanProvision = false;

                            if (this.LocalViewModel != null)
                            {
                                if (this.SelectedViewModel == this.LocalViewModel)
                                {
                                    this.LocalViewModel.FileAnalysisIsEnabled = true;
                                }
                            }
                        }
                        else
                        {
                            this.CanProvision = true;
                        }

                        this.SetupAssociationMessages();
                    }
                    catch (Exception ex)
                    {
                        this.CanProvision = true;
                        this.ErrorIsFound = true;
                        this.ErrorMessageTooltip = ex.Message + "\r\n" + ex.StackTrace;
                        this.StatusMessage = "Failed to Associate with Solution : " + this.VsHelper.ActiveSolutionName() + " " + ex.Message;
                        this.notificationManager.ReportMessage(new Message { Id = "SonarQubeViewModel", Data = "Failed to Associate with Solution : " + ex.Message });
                        this.notificationManager.ReportException(ex);
                    }
                }
            };

            bw.RunWorkerAsync();
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
                    analyser.RefreshDataForResource(this.ResourceInEditor, this.DocumentInView, File.ReadAllText(this.DocumentInView), false);
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
        public void RefreshDataForResource(string fullName, string contentoffile, bool fromSave)
        {
            this.notificationManager.WriteMessage("Refresh Data For File: " + fullName);
            if (string.IsNullOrEmpty(fullName) || this.AssociationModule.AssociatedProject == null)
            {
                return;
            }

            this.DocumentInView = fullName;
            this.ResourceInEditor = null;
            this.ResetIssuesInViews();

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
                    analyser.RefreshDataForResource(this.ResourceInEditor, this.DocumentInView, contentoffile, fromSave);
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
        /// Refreshes the new list of issues.
        /// </summary>
        public void RefreshNewListOfIssues()
        {
            var counterIssues = 0;
            foreach (var file in this.newAddedIssues)
            {
                counterIssues += file.Value.Count;
            }

            this.NumberNewIssues = counterIssues.ToString();
        }

        /// <summary>
        /// Selects the branch from list.
        /// </summary>
        /// <returns>returns current branch</returns>
        public Resource SelectBranchFromList()
        {
            var branch = this.SourceControl.GetBranch().Replace("/", "_");
            Resource masterBranch = null;
            foreach (var branchdata in this.SelectedProjectInView.BranchResources)
            {
                if (branchdata.BranchName.Equals(branch))
                {
                    this.StatusMessageAssociation = "Association Ready. Press associate to confirm.";
                    return branchdata;
                }

                if (branchdata.BranchName.Equals("master"))
                {
                    masterBranch = branchdata;
                }
            }

            if (masterBranch != null)
            {
                this.StatusMessageAssociation = "Using master branch, because current branch does not exist or source control not supported. Press associate to confirm.";
                return masterBranch;
            }

            this.StatusMessageAssociation = "Unable to find branch, please manually choose one from list and confirm.";
            return null;
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

        /// <summary>
        ///     The on disconnect to sonar.
        /// </summary>
        public void OnDisconnectToSonar()
        {
            this.ConnectionTooltip = "Not Connected";
            this.StatusMessage = "Not Connected";
            this.IsConnected = false;
            this.ErrorIsFound = false;
            this.AvailableProjects.Clear();
            this.SelectedProjectInView = null;
            this.SelectedBranchProject = null;

            this.SelectedProjectKey = string.Empty;
            this.SelectedProjectName = string.Empty;
            this.SelectedProjectVersion = string.Empty;
            this.ErrorMessageTooltip = string.Empty;
            this.StatusMessageAssociation = string.Empty;
            this.AssociationModule.Disconnect();
            this.ResetIssuesInViews();
        }


        /// <summary>
        ///     The launch extension properties.
        /// </summary>
        public void LaunchExtensionProperties()
        {
            var window = new VSonarQubeOptionsView(this.VSonarQubeOptionsViewData);
            window.ShowDialog();
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
            this.ProvisionProjectCommand = new RelayCommand(this.OnProvisionProjectCommand);
            this.LaunchExtensionPropertiesCommand = new RelayCommand(this.LaunchExtensionProperties);
            this.ToolSwitchCommand = new RelayCommand<string>(this.ExecutePlugin);

            this.ClearCacheCommand = new RelayCommand(this.OnClearCacheCommand);

            this.ConnectCommand = new RelayCommand(this.OnConnectCommand);
            this.ConnectedToServerCommand = new RelayCommand(this.OnConnectToSonarCommand);
            this.DisconnectToServerCommand = new RelayCommand(this.OnDisconnectToSonar);
            this.AssignProjectCommand = new RelayCommand(this.OnAssignProjectCommand);
            this.CloseRightFlyoutCommand = new RelayCommand(this.OnCloseRightFlyoutCommand);
            this.ShowNewLocalIssuesCommand = new RelayCommand(this.OnShowNewLocalIssuesCommand);
        }

        /// <summary>
        /// Called when [provision project command].
        /// </summary>
        private void OnProvisionProjectCommand()
        {
            this.ProvisionProject(true);
        }

        /// <summary>
        /// Provisions the project.
        /// </summary>
        /// <param name="confirmAnalysis">if set to <c>true</c> [confirm analysis].</param>
        private void ProvisionProject(bool confirmAnalysis)
        {
            var answer = QuestionUser.GetInput("Do you wish to provision a new project? Be sure you cant find it from the available projects and you have provision permissions.");

            if (answer)
            {
                var provisiondata = View.Helpers.ProvisionProject.Prompt(this.AssociationModule.OpenSolutionName, this.sourceControl.GetBranch());

                if (provisiondata != null)
                {
                    var branch = provisiondata.GetBranch();
                    var name = provisiondata.GetName();
                    var key = provisiondata.GetKey();

                    var reply = this.sonarRestConnector.ProvisionProject(AuthtenticationHelper.AuthToken, key, name, branch);

                    if (!string.IsNullOrEmpty(reply))
                    {
                        if (reply.Contains("Could not create Project, key already exists:"))
                        {
                            var answer2 = QuestionUser.GetInput("There is a project with same settings provisioned in server, do you want to use to complete creation?");

                            if (!answer2)
                            {
                                return;
                            }
                        }
                        else
                        {
                            MessageDisplayBox.DisplayMessage("Unable to provision project.", reply, "http://docs.sonarqube.org/display/SONARQUBE51/Provisioning+Projects");
                            return;

                        }
                    }

                    var projectResource = new Resource { Key = key, Name = name, BranchName = branch, Version = "work" };

                    if (!string.IsNullOrEmpty(branch))
                    {
                        projectResource.IsBranch = true;
                        var branchResource = new Resource { Key = key, Name = name, BranchName = branch, Version = "work" };
                        projectResource.BranchResources.Add(branchResource);
                    }

                    this.AvailableProjects.Add(projectResource);

                    this.SelectedProjectInView = projectResource;

                    if (projectResource.IsBranch)
                    {
                        this.SelectedBranchProject = projectResource.BranchResources[0];
                    }

                    this.OnAssignProjectCommand();
                    this.CanProvision = false;

                    if (confirmAnalysis)
                    {
                        MessageDisplayBox.DisplayMessage("Please configure your quality profiles in server and then run a full local analysis.");
                        this.VsHelper.NavigateToResource(AuthtenticationHelper.AuthToken.Hostname + "/project/profile?id=" + key);
                    }
                }
            }
        }

        /// <summary>
        ///     The on close flyout issue search command.
        /// </summary>
        private void OnCloseRightFlyoutCommand()
        {
            this.ShowRightFlyout = false;
            this.SizeOfFlyout = 0;
        }

        /// <summary>
        /// Called when [show new local issues command].
        /// </summary>
        private void OnShowNewLocalIssuesCommand()
        {
            this.LocalViewModel.ShowNewAddedIssuesAndLock();
        }

        /// <summary>
        ///     The init menus.
        /// </summary>
        private void InitMenus()
        {
            foreach (IMenuCommandPlugin plugin in this.pluginManager.MenuPlugins)
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
            this.LocaAnalyser = new SonarLocalAnalyser(this.pluginManager.AnalysisPlugins, this.sonarRestConnector, this.configurationHelper, this.notificationManager, this.VsHelper, this.vsversion);

            this.ServerViewModel = new ServerViewModel(
                this.VsHelper, 
                this.configurationHelper, 
                this.sonarRestConnector,
                this.notificationManager,
                this.sonarKeyTranslator,
                this.LocaAnalyser);

            this.LocalViewModel = new LocalViewModel(
                this.pluginManager.AnalysisPlugins,
                this.sonarRestConnector,
                this.configurationHelper,
                this.notificationManager,
                this.sonarKeyTranslator,
                this.LocaAnalyser,
                this.newAddedIssues);

            this.IssuesSearchModel = new IssuesSearchModel(
                this.configurationHelper,
                this.sonarRestConnector, 
                this.notificationManager, 
                this.sonarKeyTranslator,
                this.LocaAnalyser);
            
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
        ///     The on assign project command.
        /// </summary>
        private void OnAssignProjectCommand()
        {
            this.ErrorIsFound = false;
            if (this.SelectedProjectInView == null)
            {
                this.StatusMessageAssociation = "Please select a project, or if empty please login in the Menu > Configuration in the Sonar Window";
                this.StatusMessage = "Ready";
                return;
            }

            if (this.SelectedProjectInView.IsBranch && this.SelectedBranchProject == null)
            {
                this.StatusMessageAssociation = "Please select a feature branch as the in use branch";
                this.StatusMessage = "Ready";
                return;
            }

            if (this.AssociationModule.AssignASonarProjectToSolution(this.SelectedProjectInView, this.SelectedBranchProject, this.SourceControl))
            {
                this.StatusMessageAssociation = "Associated : " + this.AssociationModule.AssociatedProject.Name;
                this.StatusMessage = "Ready";
                return;
            }

            this.ErrorIsFound = false;
            this.ErrorMessageTooltip = "Could not associate, something failed. Check output log for error messages.";
            this.StatusMessageAssociation = "Error : Not Associated";
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
                this.StatusMessageAssociation = "No project selected, select from above.";
                return;
            }

            this.SelectedProjectName = this.SelectedProjectInView.Name;
            this.SelectedProjectKey = this.SelectedProjectInView.Key;
            this.SelectedProjectVersion = this.SelectedProjectInView.Version;

            if (this.SelectedProjectInView.IsBranch)
            {
                this.SelectedBranchProject = this.SelectBranchFromList();
            }
            else
            {
                this.SelectedBranchProject = null;
                this.StatusMessageAssociation = "Normal project type. Press associate to confirm.";
            }
        }

        /// <summary>
        /// Resets the issues in views.
        /// </summary>
        private void ResetIssuesInViews()
        {
            if (this.LocalViewModel != null)
            {
                this.LocalViewModel.ClearIssues();
                this.LocalViewModel.ResetStats();
            }

            if (this.ServerViewModel != null)
            {
                this.ServerViewModel.ClearIssues();
                this.ServerViewModel.ResetStats();
            }
        }

        /// <summary>
        /// Setups the association messages.
        /// </summary>
        private void SetupAssociationMessages()
        {
            if (!this.AssociationModule.IsAssociated)
            {
                this.CanProvision = true;
                this.SelectedProjectInView = null;
                this.SelectedProjectKey = null;
                this.SelectedProjectName = null;
                this.SelectedProjectVersion = null;
                this.ErrorIsFound = true;
                this.StatusMessage = "Was unable to associate with sonar project, use project association dialog to choose a project or to provision project";
                this.ShowRightFlyout = true;
            }
            else
            {
                this.CanProvision = false;
                this.SelectedProjectKey = this.AssociationModule.AssociatedProject.Key;
                this.SelectedProjectName = this.AssociationModule.AssociatedProject.Name;
                this.SelectedProjectVersion = this.AssociationModule.AssociatedProject.Version;
                this.StatusMessage = "successfully associated with : " + this.AssociationModule.AssociatedProject.Name;
                this.ShowRightFlyout = false;
                this.ErrorIsFound = false;
            }
        }

        /// <summary>
        /// Previews the analysis has completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PreviewAnalysisHasCompleted(object sender, EventArgs e)
        {
            this.AnalysisFromSolutionRunning = false;
            this.LocaAnalyser.LocalAnalysisCompleted -= this.PreviewAnalysisHasCompleted;
            if (this.LocalViewModel.ErrorsFoundDuringAnalysis)
            {
                MessageDisplayBox.DisplayMessage("Analysis as failed, check output log for more information.", helpurl: "https://github.com/TeklaCorp/VSSonarQubeExtension/wiki/Troubleshooting-and-FAQ");
            }

            this.LocalViewModel.ErrorsFoundDuringAnalysis = false;
        }

        /// <summary>
        /// Fulls the analysis has completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FullAnalysisHasCompleted(object sender, EventArgs e)
        {
            this.AnalysisFromSolutionRunning = false;
            this.LocaAnalyser.LocalAnalysisCompleted -= this.FullAnalysisHasCompleted;
            if (this.LocalViewModel.ErrorsFoundDuringAnalysis)
            {
                MessageDisplayBox.DisplayMessage("Analysis as failed, check output log for more information.", helpurl: "https://github.com/TeklaCorp/VSSonarQubeExtension/wiki/Troubleshooting-and-FAQ");
            }
            else
            {
                MessageDisplayBox.DisplayMessage("You may use server analysis mode, to retrieve analysis results.", helpurl: "https://github.com/TeklaCorp/VSSonarQubeExtension/wiki/Server-Analysis");
            }

            this.LocalViewModel.ErrorsFoundDuringAnalysis = false;
        }

        /// <summary>
        /// Runs the preview analysis after association.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RunPreviewAnalysisAfterAssociation(object sender, EventArgs e)
        {
            this.AnalysisFromSolutionRunning = false;
            this.LocaAnalyser.AssociateCommandCompeted -= this.RunPreviewAnalysisAfterAssociation;
            this.LocalViewModel.RunAnalysis(AnalysisTypes.PREVIEW, false);
        }

        /// <summary>
        /// Runs the full analysis after association.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RunFullAnalysisAfterAssociation(object sender, EventArgs e)
        {
            this.LocaAnalyser.AssociateCommandCompeted -= this.RunFullAnalysisAfterAssociation;
            this.LocalViewModel.RunAnalysis(AnalysisTypes.ANALYSIS, false);
        }

        #endregion
    }
}