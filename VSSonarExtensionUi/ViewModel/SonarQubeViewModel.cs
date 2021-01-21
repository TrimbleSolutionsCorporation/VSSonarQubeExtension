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
    using SonarRestService.Types;
    using SonarRestServiceImpl;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
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
    [AddINotifyPropertyChangedInterface]
    public class SonarQubeViewModel
    {
        /// <summary>
        /// The view model pool
        /// </summary>
        private static readonly List<IViewModelBase> viewModelPool = new List<IViewModelBase>();

        /// <summary>
        ///     The analysisPlugin control.
        /// </summary>
        private readonly PluginController pluginController = new PluginController();

        /// <summary>
        ///     The menu item plugins.
        /// </summary>
        private readonly Dictionary<int, IMenuCommandPlugin> menuItemPlugins = new Dictionary<int, IMenuCommandPlugin>();

        /// The new added issues
        /// <summary>
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
            notificationManager = new NotifyCationManager(this, "standalone");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeViewModel" /> class. Plugins are initialized in InitModelFromPackageInitialization below
        /// </summary>
        /// <param name="vsverionIn">The vs version.</param>
        public SonarQubeViewModel(string vsverionIn, IConfigurationHelper configurationHelperIn)
        {
            vsversion = vsverionIn;
            ToolsProvidedByPlugins = new ObservableCollection<MenuItem>();
            AvailableProjects = new ObservableCollection<Resource>();

            notificationManager = new NotifyCationManager(this, vsverionIn);
            if (configurationHelperIn != null)
            {
                configurationHelper = configurationHelperIn;
            }
            else
            {
                configurationHelper = new ConfigurationHelper(vsverionIn, notificationManager);
            }

            sonarKeyTranslator = new SQKeyTranslator(notificationManager);
            sonarRestConnector = new SonarService(new JsonSonarConnector());
            VSonarQubeOptionsViewData = new VSonarQubeOptionsViewModel(sonarRestConnector, configurationHelper, notificationManager);
            VSonarQubeOptionsViewData.ResetUserData();

            ConnectionTooltip = "Not Connected";
            BackGroundColor = Colors.White;
            ForeGroundColor = Colors.Black;
            ErrorIsFound = false;
            IsExtensionBusy = false;
            ShowRightFlyout = false;
            InitCommands();
            NumberNewIssues = "0";
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
        /// <param name="locaAnalyser">The local analyser.</param>
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
            vsversion = vsverionIn;
            ToolsProvidedByPlugins = new ObservableCollection<MenuItem>();
            AvailableProjects = new ObservableCollection<Resource>();

            this.pluginManager = pluginManager;
            this.sourceControl = sourceControl;
            configurationHelper = helper;
            notificationManager = notification;
            sonarKeyTranslator = translator;
            sonarRestConnector = restService;
            LocaAnalyser = locaAnalyser;
            VSonarQubeOptionsViewData =
                new VSonarQubeOptionsViewModel(
                    sonarRestConnector,
                    configurationHelper,
                    notificationManager);

            VSonarQubeOptionsViewData.ResetUserData();

            // start association module after all models are started
            AssociationModule = new AssociationModel(
                notificationManager,
                sonarRestConnector,
                configurationHelper,
                sonarKeyTranslator,
                this.pluginManager,
                this,
                LocaAnalyser,
                vsversion);

            ConnectionTooltip = "Not Connected";
            BackGroundColor = Colors.White;
            ForeGroundColor = Colors.Black;
            ErrorIsFound = true;
            IsExtensionBusy = false;

            InitCommands();
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
            get => showRightFlyout;

            set
            {
                showRightFlyout = value;
                SizeOfFlyout = value ? 250 : 0;
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
                if (sourceControl == null)
                {
                    // create a new source control provider for solution
                    sourceControl = new SourceControlModel(
                        pluginManager.SourceCodePlugins,
                        AssociationModule.OpenSolutionPath,
                        Logger,
                        sonarRestConnector);
                }

                return sourceControl;
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
        /// Gets or sets a value indicating whether [error is found].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [error is found]; otherwise, <c>false</c>.
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
        /// Gets the sonarqube views.
        /// </summary>
        /// <value>
        /// The sonarqube views.
        /// </value>
        public ObservableCollection<IViewModelBase> SonarQubeViews { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance can provision.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can provision; otherwise, <c>false</c>.
        /// </value>
        public bool CanProvision
        {
            get => canProvision;

            private set
            {
                canProvision = value;

                if (AuthtenticationHelper.AuthToken != null && AuthtenticationHelper.AuthToken.SonarVersion < 5.2)
                {
                    canProvision = false;
                }
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
        public bool NewIssuesFound => !(string.IsNullOrEmpty(NumberNewIssues) || NumberNewIssues.Equals("0"));

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public INotificationManager Logger => notificationManager;

        /// <summary>
        /// Gets the local analyses.
        /// </summary>
        /// <value>
        /// The local analyzer.
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
        /// Determines whether this instance [can close solution].
        /// </summary>
        /// <returns>returns 0 if can close</returns>
        public int CanCloseSolution()
        {
            if (NewIssuesFound)
            {
                if (QuestionUser.GetInput(
                    "You are about the close solution after new issues have been introduced. Are you sure you want to exit."))
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
        /// Called when [solution open].
        /// </summary>
        /// <param name="solutionName">Name of the solution.</param>
        /// <param name="solutionPath">The solution path.</param>
        /// <param name="fileInView">The file in view.</param>
        /// <param name="sourceProvider">The source provider.</param>
        public async Task OnSolutionOpen(string solutionName, string solutionPath, string fileInView, ISourceControlProvider sourceProvider = null)
        {
            if (IsSolutionOpen)
            {
                return;
            }

            ErrorIsFound = false;
            IsSolutionOpen = true;

            if (sourceProvider == null)
            {
                // create new source control provider
                sourceControl = new SourceControlModel(
                    pluginManager.SourceCodePlugins,
                    solutionPath,
                    Logger,
                    sonarRestConnector);
            }
            else
            {
                sourceControl = sourceProvider;
            }

            await AssociationModule.AssociateProjectToSolution(solutionName, solutionPath, AvailableProjects, SourceControl);

            if (!string.IsNullOrEmpty(fileInView) && File.Exists(fileInView))
            {
                try
                {
                    string slnCnt = File.ReadAllText(fileInView);
                    await RefreshDataForResource(fileInView, slnCnt, false);
                }
                catch (OutOfMemoryException)
                {
                }
            }

            SetupAssociationMessages();
        }

        /// <summary>
        /// Refreshes the project list.
        /// </summary>
        /// <param name="useDispatcher">if set to <c>true</c> [use dispatcher].</param>
        public void RefreshProjectList(bool useDispatcher)
        {
            List<Resource> projectsRaw = sonarRestConnector.GetProjectsList(AuthtenticationHelper.AuthToken);
            SortedDictionary<string, Resource> projects = new SortedDictionary<string, Resource>();

            foreach (Resource rawitem in projectsRaw)
            {
                if (rawitem.IsBranch)
                {
                    string[] nameWithoutBrachRaw = rawitem.Name.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    string keyDic = nameWithoutBrachRaw[0] + "_Main";
                    if (!projects.ContainsKey(keyDic))
                    {
                        // create main project holder
                        Resource mainProject = new Resource
                        {
                            Name = nameWithoutBrachRaw[0],
                            Key = keyDic,
                            IsBranch = true
                        };
                        projects.Add(keyDic, mainProject);
                    }

                    Resource element = projects[keyDic];
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
                            AvailableProjects.Clear();
                            foreach (KeyValuePair<string, Resource> source in projects)
                            {
                                AvailableProjects.Add(source.Value);
                            }
                        });
                }
                else
                {
                    AvailableProjects.Clear();
                    foreach (KeyValuePair<string, Resource> source in projects)
                    {
                        AvailableProjects.Add(source.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Called when [solution closed].
        /// </summary>
        public async Task OnSolutionClosed()
        {
            IsSolutionOpen = false;

            // clear all messages related with association
            StatusMessage = string.Empty;
            ConnectionTooltip = "Connected but not associated";
            StatusMessageAssociation = "Not associated with any project";
            SelectedBranchProject = null;
            SelectedProjectInView = null;
            SelectedProjectKey = string.Empty;
            SelectedProjectName = string.Empty;
            SelectedProjectVersion = string.Empty;
            NumberNewIssues = "0";

            await AssociationModule.OnSolutionClosed();
        }

        /// <summary>The execute plugin.</summary>
        /// <param name="header">The data.</param>
        public void ExecutePlugin(string header)
        {
            foreach (KeyValuePair<int, IMenuCommandPlugin> plugin in menuItemPlugins)
            {
                PluginDescription plugDesc = plugin.Value.GetPluginDescription();
                if (!plugDesc.Name.Equals(header))
                {
                    continue;
                }

                string isEnabled = "true";
                try
                {
                    isEnabled = configurationHelper.ReadSetting(
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
                    if (IsRunningInVisualStudio())
                    {
                        InUsePlugin = plugin;
                        OnPluginRequest(EventArgs.Empty);
                    }
                    else
                    {
                        Window window = new Window
                        {
                            Content =
                                                 plugin.Value.GetUserControl(
                                                     AuthtenticationHelper.AuthToken,
                                                     AssociationModule.AssociatedProject,
                                                     VsHelper)
                        };

                        plugin.Value.UpdateTheme(BackGroundColor, ForeGroundColor);
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
        /// <param name="vsenvironmenthelperIn">The vs environment helper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="extensionFolder">The extension Folder.</param>
        public async Task InitModelFromPackageInitialization(
            IVsEnvironmentHelper vsenvironmenthelperIn,
            IVSSStatusBar statusBar,
            IServiceProvider provider,
            string extensionFolder)
        {
            ServiceProvider = provider;
            VsHelper = vsenvironmenthelperIn;
            StatusBar = statusBar;
            ExtensionFolder = extensionFolder;

            // register notification manager, since all messages will be show also for initialization
            await (notificationManager as IModelBase).UpdateServices(VsHelper, StatusBar, ServiceProvider);
            VSonarQubeOptionsViewData.InitPuginSystem(vsenvironmenthelperIn, pluginController, notificationManager);
            pluginManager = VSonarQubeOptionsViewData.PluginManager;
            InitMenus();
            InitViewsAndModels();

            // start association module after all models are started
            AssociationModule = new AssociationModel(
                notificationManager,
                sonarRestConnector,
                configurationHelper,
                sonarKeyTranslator,
                VSonarQubeOptionsViewData.PluginManager,
                this,
                LocaAnalyser,
                vsversion);

            AssociationModule.UpdateServicesInModels(VsHelper, StatusBar, ServiceProvider);
            UpdateTheme(Colors.Black, Colors.White);

            // try to connect to start to sonar if on start is on
            if (VSonarQubeOptionsViewData.GeneralConfigurationViewModel.IsConnectAtStartOn)
            {
                await OnConnectToSonar(false);
            }
        }

        /// <summary>
        /// The get issues in editor.
        /// </summary>
        /// <param name="fileResource">The file Resource.</param>
        /// <param name="fileContent">The file Content.</param>
        /// <param name="showfalseandresolved">if set to <c>true</c> [show false and resolved].</param>
        /// <returns>
        /// The<see><cref>List</cref></see>
        /// .
        /// </returns>
        public async Task<Tuple<List<Issue>, bool>> GetIssuesInEditor(Resource fileResource, string fileContent)
        {
            notificationManager.WriteMessageToLog("Return issues for resource: " + fileResource);
            if (VSonarQubeOptionsViewData.GeneralConfigurationViewModel.DisableEditorTags)
            {
                notificationManager.WriteMessageToLog("Return issues for resource, tags disabled");
                return new Tuple<List<Issue>, bool>(new List<Issue>(), false);
            }

            IAnalysisModelBase view = SelectedModel;

            if (view == null)
            {
                return null;
            }

            Tuple<List<Issue>, bool> issuesData = await view.GetIssuesForResource(fileResource, fileContent, true);
            return new Tuple<List<Issue>, bool>(issuesData.Item1, issuesData.Item2);
        }

        /// <summary>
        ///     The is running in visual studio.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool IsRunningInVisualStudio()
        {
            return VsHelper.AreWeRunningInVisualStudio();
        }

        /// <summary>
        /// Called when [connect to sonar command].
        /// </summary>
        public async void OnConnectToSonarCommand()
        {
            await OnConnectToSonar(true);
        }

        /// <summary>
        /// Starts the analysis window.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="fromSave">if set to <c>true</c> [from save].</param>
        public void StartAnalysisWindow(AnalysisTypes mode, bool fromSave)
        {
            if (!IsConnected)
            {
                MessageDisplayBox.DisplayMessage("You must first connect to server before you can run analysis", helpurl: "https://github.com/TeklaCorp/VSSonarQubeExtension/wiki");
                return;
            }

            if (AnalysisFromSolutionRunning)
            {
                MessageDisplayBox.DisplayMessage("A previous analysis is running, you need to wait for completion.");
                return;
            }

            if (!AssociationModule.IsAssociated)
            {
                ProvisionProject(false);
            }
            else
            {
                LocalViewModel.ErrorsFoundDuringAnalysis = false;
                AnalysisFromSolutionRunning = true;
                LocalViewModel.RunAnalysis(mode, fromSave);
            }
        }

        /// <summary>
        /// Resets the and establish a new connection to server.
        /// </summary>
        public async void ResetAndEstablishANewConnectionToServer()
        {
            OnDisconnectToSonar();
            await OnConnectToSonar(true);
        }

        /// <summary>
        /// The connect to sonar.
        /// </summary>
        /// <param name="useDispatcher">if set to <c>true</c> [use dispatcher].</param>
        public async Task OnConnectToSonar(bool useDispatcher)
        {
            ErrorIsFound = false;

            if (AuthtenticationHelper.AuthToken == null)
            {
                StatusMessage = "No credentials available Go To: Sonar Menu > Configuration";
                IsConnected = false;
                ErrorIsFound = true;
                return;
            }

            if (VsHelper == null)
            {
                StatusMessage = "Extension in unusable, unable to access visual studio services.";
                IsConnected = false;
                ErrorIsFound = true;
                return;
            }

            IsExtensionBusy = true;

            try
            {
                RefreshProjectList(useDispatcher);
                await AssociationModule.OnConnectToSonar();
                await VSonarQubeOptionsViewData.OnConnectToSonar(AuthtenticationHelper.AuthToken, AvailableProjects, pluginManager.IssueTrackerPlugins);
                ConnectionTooltip = "Authenticated, but not associated";
                StatusMessage = string.Empty;
                IsConnected = true;
                AssociationModule.IsAssociated = false;
            }
            catch (Exception ex)
            {
                notificationManager.ReportMessage(new Message { Id = "SonarQubeViewModel", Data = "Fail To Connect To SonarQube: " + ex.Message });
                notificationManager.ReportException(ex);
                ConnectionTooltip = "No Connection";
                AssociationModule.Disconnect();
                StatusMessage = "Failed to Connect to Sonar, check output log for details";
                IsConnected = false;
                return;
            }

            // try to associate with open solution
            if (!IsSolutionOpen)
            {
                return;
            }

            try
            {
                await AssociationModule.AssociateProjectToSolution(VsHelper.ActiveSolutionFileNameWithExtension(), VsHelper.ActiveSolutioRootPath(), AvailableProjects, SourceControl);

                if (AssociationModule.IsAssociated)
                {
                    CanProvision = false;
                }
                else
                {
                    CanProvision = true;
                }

                SetupAssociationMessages();
            }
            catch (Exception ex)
            {
                CanProvision = true;
                ErrorIsFound = true;
                ErrorMessageTooltip = ex.Message + "\r\n" + ex.StackTrace;
                StatusMessage = "Failed to Associate with Solution : " + VsHelper.ActiveSolutionFileNameWithExtension() + " " + ex.Message;
                notificationManager.ReportMessage(new Message { Id = "SonarQubeViewModel", Data = "Failed to Associate with Solution : " + ex.Message });
                notificationManager.ReportException(ex);
            }
        }

        /// <summary>
        ///     The on issues change event.
        /// </summary>
        public void OnIssuesChangeEvent()
        {
            SelectedModel.RefreshIssuesEditor(EventArgs.Empty);
        }

        /// <summary>
        ///     The on selected view changed.
        /// </summary>
        public void OnSelectedViewModelChanged()
        {
            configurationHelper.WriteSetting(
                new SonarQubeProperties
                {
                    Context = Context.UIProperties.ToString(),
                    Key = "SelectedView",
                    Owner = OwnersId.ApplicationOwnerId,
                    Value = SelectedViewModel.Header
                }, true);

            foreach (IAnalysisModelBase model in SonarQubeModels)
            {
                IModelBase modelBase = model as IModelBase;

                if (modelBase.GetViewModel().Equals(SelectedViewModel))
                {
                    SelectedModel = model;
                }
            }

            OnAnalysisModeHasChange(EventArgs.Empty);
            Debug.WriteLine("Name Changed");
        }

        /// <summary>
        /// Refreshes the data for resource.
        /// </summary>
        public void RefreshDataForResource()
        {
            if (ServerViewModel != null)
            {
                ServerViewModel.ResetStats();
            }

            if (LocalViewModel != null)
            {
                LocalViewModel.ResetStats();
            }

            if (ResourceInEditor == null)
            {
                return;
            }

            IAnalysisModelBase analyser = SelectedModel;
            if (analyser != null)
            {
                try
                {
                    notificationManager.WriteMessageToLog("RefreshDataForResource: Doc in View: " + DocumentInView);
                    analyser.RefreshDataForResource(ResourceInEditor, DocumentInView, File.ReadAllText(DocumentInView), false);
                }
                catch (Exception ex)
                {
                    notificationManager.WriteMessageToLog("Cannot find file in server: " + ex.Message);
                    notificationManager.WriteExceptionToLog(ex);
                    notificationManager.ReportException(ex);
                }
            }
        }

        /// <summary>
        /// The refresh data for resource.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="contentoffile">The content of file.</param>
        /// <param name="fromSave">if set to <c>true</c> [from save].</param>
        public async Task RefreshDataForResource(string fullName, string contentoffile, bool fromSave)
        {
            notificationManager.WriteMessageToLog("Refresh Data For File: " + fullName);
            if (string.IsNullOrEmpty(fullName) || AssociationModule.AssociatedProject == null)
            {
                return;
            }

            DocumentInView = fullName;
            ResourceInEditor = null;
            ResetIssuesInViews();

            ResourceInEditor = await AssociationModule.CreateAValidResourceFromServer(fullName, AssociationModule.AssociatedProject);

            if (ResourceInEditor == null)
            {
                StatusMessage = "Could not check key type for resource, see VSonarOutput Window for more details and report";
                return;
            }

            if (!ResourceInEditor.Path.ToLower().Contains(ResourceInEditor.SolutionRoot.ToLower()))
            {
                return;
            }

            IAnalysisModelBase analyser = SelectedModel;
            if (analyser != null)
            {
                try
                {
                    notificationManager.WriteMessageToLog("RefreshDataForResource: Doc in View: " + DocumentInView);
                    await Task.Run(() =>
                    {
                        analyser.RefreshDataForResource(ResourceInEditor, DocumentInView, contentoffile, fromSave);
                    });

                    StatusMessage = ResourceInEditor.Key;
                }
                catch (Exception ex)
                {
                    StatusMessage = "Cannot find file in server: " + ex.Message;
                    notificationManager.WriteMessageToLog("Cannot find file in server: " + ex.Message);
                    notificationManager.WriteExceptionToLog(ex);
                    notificationManager.ReportException(ex);
                }
            }
        }

        /// <summary>
        /// Projects the has build.
        /// </summary>
        /// <param name="project">The project.</param>
        public void ProjectHasBuild(VsProjectItem project)
        {
            IAnalysisModelBase analyser = SelectedModel;
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
            BackGroundColor = background;
            ForeGroundColor = foreground;
        }

        /// <summary>The update theme.</summary>
        /// <param name="background">The background.</param>
        /// <param name="foreground">The foreground.</param>
        public void UpdateTheme(Color background, Color foreground)
        {
            BackGroundColor = background;
            ForeGroundColor = foreground;

            foreach (IViewModelBase view in viewModelPool)
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
            foreach (KeyValuePair<int, IMenuCommandPlugin> menu in menuItemPlugins)
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
            PluginDescription plugDesc = toAdd.GetPluginDescription();

            foreach (MenuItem menu in ToolsProvidedByPlugins)
            {
                if (menu.Header.Equals(plugDesc.Name))
                {
                    RemoveMenuPlugin(toAdd);
                    ToolsProvidedByPlugins.Add(new MenuItem { Header = plugDesc.Name });
                    menuItemPlugins.Add(GetFirstFreeId(), toAdd);
                    return;
                }
            }

            ToolsProvidedByPlugins.Add(new MenuItem { Header = plugDesc.Name });
            menuItemPlugins.Add(GetFirstFreeId(), toAdd);
        }

        /// <summary>The remove menu plugin.</summary>
        /// <param name="toRemove">The to remove.</param>
        public void RemoveMenuPlugin(IMenuCommandPlugin toRemove)
        {
            PluginDescription plugDescRemove = toRemove.GetPluginDescription();
            foreach (MenuItem menu in ToolsProvidedByPlugins)
            {
                if (menu.Header.Equals(plugDescRemove.Name))
                {
                    ToolsProvidedByPlugins.Remove(menu);
                    KeyValuePair<int, IMenuCommandPlugin> selectMenu = InUsePlugin;
                    foreach (KeyValuePair<int, IMenuCommandPlugin> menuCheck in menuItemPlugins)
                    {
                        if (menuCheck.Value.GetPluginDescription().Name.Equals(plugDescRemove.Name))
                        {
                            selectMenu = menuCheck;
                        }
                    }

                    if (IsRunningInVisualStudio())
                    {
                        IdOfPluginToRemove = selectMenu.Key;
                        OnRemovePluginRequest(EventArgs.Empty);
                    }

                    menuItemPlugins.Remove(selectMenu.Key);
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
            if (ServerViewModel != null)
            {
                ServerViewModel.UpdateOpenDiffWindowList(fullName);
            }
        }

        /// <summary>
        /// Refreshes the new list of issues.
        /// </summary>
        public void RefreshNewListOfIssues()
        {
            int counterIssues = 0;
            foreach (KeyValuePair<string, List<Issue>> file in newAddedIssues)
            {
                counterIssues += file.Value.Count;
            }

            NumberNewIssues = counterIssues.ToString();
        }

        /// <summary>
        /// Selects the branch from list.
        /// </summary>
        /// <returns>returns current branch</returns>
        public Resource SelectBranchFromList()
        {
            string branch = SourceControl.GetBranch().Replace("/", "_");
            Resource masterBranch = null;
            foreach (Resource branchdata in SelectedProjectInView.BranchResources)
            {
                if (branchdata.BranchName.Equals(branch))
                {
                    StatusMessageAssociation = "Association Ready. Press associate to confirm.";
                    return branchdata;
                }

                if (branchdata.BranchName.Equals("master"))
                {
                    masterBranch = branchdata;
                }
            }

            if (masterBranch != null)
            {
                StatusMessageAssociation = "Using master branch, because current branch does not exist or source control not supported. Press associate to confirm.";
                return masterBranch;
            }

            StatusMessageAssociation = "Unable to find branch, please manually choose one from list and confirm.";
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
            if (SelectedModel == ServerViewModel)
            {
                return ServerViewModel.GetCoverageInEditor(buffer);
            }

            return new Dictionary<int, CoverageElement>();
        }

        /// <summary>
        ///     The on disconnect to sonar.
        /// </summary>
        public void OnDisconnectToSonar()
        {
            ConnectionTooltip = "Not Connected";
            StatusMessage = "Not Connected";
            IsConnected = false;
            ErrorIsFound = false;
            AvailableProjects.Clear();
            SelectedProjectInView = null;
            SelectedBranchProject = null;

            SelectedProjectKey = string.Empty;
            SelectedProjectName = string.Empty;
            SelectedProjectVersion = string.Empty;
            ErrorMessageTooltip = string.Empty;
            StatusMessageAssociation = string.Empty;
            AssociationModule.Disconnect();
            ResetIssuesInViews();
        }

        /// <summary>
        ///     The launch extension properties.
        /// </summary>
        public void LaunchExtensionProperties()
        {
            VSonarQubeOptionsView window = new VSonarQubeOptionsView(VSonarQubeOptionsViewData);
            window.ShowDialog();
        }

        /// <summary>The on changed.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnAnalysisModeHasChange(EventArgs e)
        {
            if (AnalysisModeHasChange != null)
            {
                AnalysisModeHasChange(this, e);
            }
        }

        /// <summary>The on plugin request.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnPluginRequest(EventArgs e)
        {
            if (PluginRequest != null)
            {
                PluginRequest(this, e);
            }
        }

        /// <summary>The on remove plugin request.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnRemovePluginRequest(EventArgs e)
        {
            if (RemovePluginRequest != null)
            {
                RemovePluginRequest(this, e);
            }
        }

        /// <summary>
        ///     The init commands.
        /// </summary>
        private void InitCommands()
        {
            ProvisionProjectCommand = new RelayCommand(OnProvisionProjectCommand);
            LaunchExtensionPropertiesCommand = new RelayCommand(LaunchExtensionProperties);
            ToolSwitchCommand = new RelayCommand<string>(ExecutePlugin);

            ClearCacheCommand = new RelayCommand(OnClearCacheCommand);

            ConnectCommand = new RelayCommand(OnConnectCommand);
            ConnectedToServerCommand = new RelayCommand(OnConnectToSonarCommand);
            DisconnectToServerCommand = new RelayCommand(OnDisconnectToSonar);
            AssignProjectCommand = new RelayCommand(OnAssignProjectCommand);
            CloseRightFlyoutCommand = new RelayCommand(OnCloseRightFlyoutCommand);
            ShowNewLocalIssuesCommand = new RelayCommand(OnShowNewLocalIssuesCommand);
        }

        /// <summary>
        /// Called when [provision project command].
        /// </summary>
        private void OnProvisionProjectCommand()
        {
            ProvisionProject(true);
        }

        /// <summary>
        /// Provisions the project.
        /// </summary>
        /// <param name="confirmAnalysis">if set to <c>true</c> [confirm analysis].</param>
        private void ProvisionProject(bool confirmAnalysis)
        {
            bool answer = QuestionUser.GetInput("Do you wish to provision a new project? Be sure you cant find it from the available projects and you have provision permissions.");

            if (answer)
            {
                ProvisionProject provisiondata = View.Helpers.ProvisionProject.Prompt(AssociationModule.OpenSolutionName, sourceControl.GetBranch());

                if (provisiondata != null)
                {
                    string branch = provisiondata.GetBranch();
                    string name = provisiondata.GetName();
                    string key = provisiondata.GetKey();

                    string reply = sonarRestConnector.ProvisionProject(AuthtenticationHelper.AuthToken, key, name, branch);

                    if (!string.IsNullOrEmpty(reply))
                    {
                        if (reply.Contains("Could not create Project, key already exists:"))
                        {
                            bool answer2 = QuestionUser.GetInput("There is a project with same settings provisioned in server, do you want to use to complete creation?");

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

                    Resource projectResource = new Resource { Key = key, Name = name, BranchName = branch, Version = "work" };

                    if (!string.IsNullOrEmpty(branch))
                    {
                        projectResource.IsBranch = true;
                        Resource branchResource = new Resource { Key = key, Name = name, BranchName = branch, Version = "work" };
                        projectResource.BranchResources.Add(branchResource);
                    }

                    AvailableProjects.Add(projectResource);

                    SelectedProjectInView = projectResource;

                    if (projectResource.IsBranch)
                    {
                        SelectedBranchProject = projectResource.BranchResources[0];
                    }

                    OnAssignProjectCommand();
                    CanProvision = false;

                    if (confirmAnalysis)
                    {
                        MessageDisplayBox.DisplayMessage("Please configure your quality profiles in server and then run a full local analysis.");
                        VsHelper.NavigateToResource(AuthtenticationHelper.AuthToken.Hostname + "/project/profile?id=" + key);
                    }
                }
            }
        }

        /// <summary>
        ///     The on close flyout issue search command.
        /// </summary>
        private void OnCloseRightFlyoutCommand()
        {
            ShowRightFlyout = false;
            SizeOfFlyout = 0;
        }

        /// <summary>
        /// Called when [show new local issues command].
        /// </summary>
        private void OnShowNewLocalIssuesCommand()
        {
            LocalViewModel.ShowNewAddedIssuesAndLock();
        }

        /// <summary>
        ///     The init menus.
        /// </summary>
        private void InitMenus()
        {
            foreach (IMenuCommandPlugin plugin in pluginManager.MenuPlugins)
            {
                ToolsProvidedByPlugins.Add(new MenuItem { Header = plugin.GetPluginDescription().Name });
                menuItemPlugins.Add(menuItemPlugins.Count, plugin);
            }
        }

        /// <summary>
        ///     The init views.
        /// </summary>
        private void InitViewsAndModels()
        {
            LocaAnalyser = new SonarLocalAnalyser(pluginManager.AnalysisPlugins, sonarRestConnector, configurationHelper, notificationManager, VsHelper, vsversion);

            ServerViewModel = new ServerViewModel(
                VsHelper,
                configurationHelper,
                sonarRestConnector,
                notificationManager,
                sonarKeyTranslator,
                LocaAnalyser,
                pluginManager.IssueTrackerPlugins);

            LocalViewModel = new LocalViewModel(
                pluginManager.AnalysisPlugins,
                sonarRestConnector,
                configurationHelper,
                notificationManager,
                sonarKeyTranslator,
                LocaAnalyser,
                newAddedIssues);

            IssuesSearchModel = new IssuesSearchModel(
                configurationHelper,
                sonarRestConnector,
                notificationManager,
                sonarKeyTranslator,
                LocaAnalyser,
                pluginManager.IssueTrackerPlugins);

            SonarQubeModels = new ObservableCollection<IAnalysisModelBase>
                                      {
                                          LocalViewModel,
                                          IssuesSearchModel,
                                          ServerViewModel
                                      };

            SonarQubeViews = new ObservableCollection<IViewModelBase>
                                      {
                                          ServerViewModel.GetViewModel() as IViewModelBase,
                                          LocalViewModel.GetViewModel() as IViewModelBase,
                                          IssuesSearchModel.GetViewModel() as IViewModelBase
                                      };

            SonarQubeProperties viewValue = configurationHelper.ReadSetting(Context.UIProperties, OwnersId.ApplicationOwnerId, "SelectedView");
            if (viewValue == null)
            {
                return;
            }

            foreach (IViewModelBase analysisViewModelBase in
                SonarQubeViews.Where(analysisViewModelBase => analysisViewModelBase.Header.Equals(viewValue.Value)))
            {
                SelectedModel = analysisViewModelBase.GetAvailableModel() as IAnalysisModelBase;
                SelectedViewModel = analysisViewModelBase;
            }
        }

        /// <summary>
        ///     The on clear cache command.
        /// </summary>
        private void OnClearCacheCommand()
        {
            ServerViewModel.ClearCache();
        }

        /// <summary>
        ///     The on connect command.
        /// </summary>
        private async void OnConnectCommand()
        {
            if (IsConnected)
            {
                await OnConnectToSonar(true);
            }
            else
            {
                OnDisconnectToSonar();
            }
        }

        /// <summary>
        ///     The on assign project command.
        /// </summary>
        private async void OnAssignProjectCommand()
        {
            ErrorIsFound = false;
            if (SelectedProjectInView == null)
            {
                StatusMessageAssociation = "Please select a project, or if empty please login in the Menu > Configuration in the Sonar Window";
                StatusMessage = "Ready";
                return;
            }

            if (SelectedProjectInView.IsBranch && SelectedBranchProject == null)
            {
                StatusMessageAssociation = "Please select a feature branch as the in use branch";
                StatusMessage = "Ready";
                return;
            }

            bool isAssociated = await AssociationModule.AssignASonarProjectToSolution(SelectedProjectInView, SelectedBranchProject, SourceControl);
            if (isAssociated)
            {
                StatusMessageAssociation = "Associated : " + AssociationModule.AssociatedProject.Name;
                StatusMessage = "Ready";
                return;
            }

            ErrorIsFound = false;
            ErrorMessageTooltip = "Could not associate, something failed. Check output log for error messages.";
            StatusMessageAssociation = "Error : Not Associated. Solution needs to be open.";
        }

        /// <summary>
        /// Called when [selected project changed].
        /// </summary>
        private void OnSelectedProjectInViewChanged()
        {
            if (SelectedProjectInView == null)
            {
                SelectedProjectName = string.Empty;
                SelectedProjectKey = string.Empty;
                SelectedProjectVersion = string.Empty;
                StatusMessageAssociation = "No project selected, select from above.";
                return;
            }

            SelectedProjectName = SelectedProjectInView.Name;
            SelectedProjectKey = SelectedProjectInView.Key;
            SelectedProjectVersion = SelectedProjectInView.Version;

            if (SelectedProjectInView.IsBranch)
            {
                SelectedBranchProject = SelectBranchFromList();
            }
            else
            {
                SelectedBranchProject = null;
                StatusMessageAssociation = "Normal project type. Press associate to confirm.";
            }
        }

        /// <summary>
        /// Resets the issues in views.
        /// </summary>
        private void ResetIssuesInViews()
        {
            if (LocalViewModel != null)
            {
                LocalViewModel.ClearIssues();
                LocalViewModel.ResetStats();
            }

            if (ServerViewModel != null)
            {
                ServerViewModel.ClearIssues();
                ServerViewModel.ResetStats();
            }
        }

        /// <summary>
        /// Setups the association messages.
        /// </summary>
        private void SetupAssociationMessages()
        {
            if (!AssociationModule.IsAssociated)
            {
                CanProvision = true;
                SelectedProjectInView = null;
                SelectedProjectKey = null;
                SelectedProjectName = null;
                SelectedProjectVersion = null;
                ErrorIsFound = true;
                StatusMessage = "Was unable to associate with sonar project, use project association dialog to choose a project or to provision project";
                ShowRightFlyout = true;
            }
            else
            {
                CanProvision = false;
                SelectedProjectKey = AssociationModule.AssociatedProject.Key;
                SelectedProjectName = AssociationModule.AssociatedProject.Name;
                SelectedProjectVersion = AssociationModule.AssociatedProject.Version;
                StatusMessage = "successfully associated with : " + AssociationModule.AssociatedProject.Name;
                ShowRightFlyout = false;
                ErrorIsFound = false;
            }
        }

        /// <summary>
        /// Fulls the analysis has completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FullAnalysisHasCompleted(object sender, EventArgs e)
        {
            AnalysisFromSolutionRunning = false;
            LocaAnalyser.LocalAnalysisCompleted -= FullAnalysisHasCompleted;
            if (LocalViewModel.ErrorsFoundDuringAnalysis)
            {
                MessageDisplayBox.DisplayMessage(
                    "Analysis as failed, check output log for more information.",
                    helpurl: "https://github.com/TeklaCorp/VSSonarQubeExtension/wiki/Troubleshooting-and-FAQ");
            }
            else
            {
                MessageDisplayBox.DisplayMessage(
                    "You may use server analysis mode, to retrieve analysis results.",
                    helpurl: "https://github.com/TeklaCorp/VSSonarQubeExtension/wiki/Server-Analysis");
            }

            LocalViewModel.ErrorsFoundDuringAnalysis = false;
        }

        #endregion
    }
}