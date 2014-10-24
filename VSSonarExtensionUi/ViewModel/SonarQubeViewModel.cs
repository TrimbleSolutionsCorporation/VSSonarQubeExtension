// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarQubeViewModel.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Windows.Media;

    using ExtensionHelpers;

    using ExtensionTypes;

    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using SonarRestService;

    using VSSonarExtensionUi.View.Configuration;
    using VSSonarExtensionUi.View.Helpers;
    using VSSonarExtensionUi.ViewModel.Analysis;
    using VSSonarExtensionUi.ViewModel.Configuration;
    using VSSonarExtensionUi.ViewModel.Helpers;

    using VSSonarPlugins;

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

        #region Public Events

        /// <summary>
        ///     The analysis mode has change.
        /// </summary>
        public event ChangedEventHandler AnalysisModeHasChange;

        public event ChangedEventHandler PluginRequest;

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

        /// <summary>
        /// Gets or sets the in use plugin.
        /// </summary>
        public KeyValuePair<int, IMenuCommandPlugin> InUsePlugin { get; set; }

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
        public void ClearProjectAssociation()
        {
            this.OpenSolutionName = null;
            this.OpenSolutionPath = null;
            this.IsConnectedButNotAssociated = this.IsConnected;

            foreach (IAnalysisViewModelBase sonarQubeView in this.SonarQubeViews)
            {
                sonarQubeView.EndDataAssociation();
            }

            foreach (IOptionsViewModelBase option in this.VSonarQubeOptionsViewData.AvailableOptions)
            {
                option.EndDataAssociation();
            }

            this.VSonarQubeOptionsViewData.EndDataAssociation();
        }

        /// <summary>
        /// The extension data viewModel update.
        /// </summary>
        /// <param name="restServiceIn">
        /// The rest service in.
        /// </param>
        /// <param name="vsenvironmenthelperIn">
        /// The vsenvironmenthelper in.
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

            foreach (IAnalysisViewModelBase view in this.SonarQubeViews)
            {
                IAnalysisViewModelBase md = view;
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
        /// The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public List<Issue> GetIssuesInEditor(Resource fileResource, string fileContent)
        {
            if (this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.DisableEditorTags)
            {
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
            this.VsHelper.WriteOptionInApplicationData("VSSonarQubeConfig", "SelectedView", this.SelectedView.Header);
            this.OnAnalysisModeHasChange(EventArgs.Empty);
            Debug.WriteLine("Name Changed");
        }

        /// <summary>
        /// The refresh data for resource.
        /// </summary>
        /// <param name="fullName">
        /// The full name.
        /// </param>
        public void RefreshDataForResource(string fullName)
        {
            if (string.IsNullOrEmpty(fullName) || this.AssociatedProject == null)
            {
                if (this.IsConnected)
                {
                    this.ErrorMessage = "Not Connected";
                }
                else
                {
                    this.ErrorMessage = "Not Associated";
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
                analyser.RefreshDataForResource(this.ResourceInEditor, this.DocumentInView);
            }
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
        public void UpdateColours(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
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

            foreach (IAnalysisViewModelBase view in this.SonarQubeViews)
            {
                view.UpdateColours(background, foreground);
            }

            this.VSonarQubeOptionsViewData.UpdateTheme(background, foreground);

            this.RefreshTheme = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected virtual void OnAnalysisModeHasChange(EventArgs e)
        {
            if (this.AnalysisModeHasChange != null)
            {
                this.AnalysisModeHasChange(this, e);
            }
        }

        protected virtual void OnPluginRequest(EventArgs e)
        {
            if (this.PluginRequest != null)
            {
                this.PluginRequest(this, e);
            }
        }

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
            if (this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig == null)
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
                            this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig, 
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
                           this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig, 
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
            var keyTypes = new string[3];

            try
            {


                keyTypes[1] = this.LocalViewModel.LocalAnalyserModule.GetResourceKey(
                    this.VsHelper.VsProjectItem(fullName), 
                    this.AssociatedProject, 
                    this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig, 
                    true);
                keyTypes[2] = this.LocalViewModel.LocalAnalyserModule.GetResourceKey(
                    this.VsHelper.VsProjectItem(fullName), 
                    this.AssociatedProject, 
                    this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig, 
                    false);

                var tounix = this.VsHelper.GetProperFilePathCapitalization(fullName).Replace("\\", "/");
                string driveLetter = tounix.Substring(0, 1);
                var solutionCan = driveLetter + this.OpenSolutionPath.Replace("\\", "/").Substring(1);
                var fromBaseDir = tounix.Replace(solutionCan + "/", string.Empty);
                keyTypes[0] = this.AssociatedProject.Key + ":" + fromBaseDir;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "No Plugin installed that supports this resource";
                this.DiagnosticMessage = ex.Message + " " + ex.StackTrace;
                return null;
            }

            foreach (var key in keyTypes)
            {
                try
                {
                    var toReturn = this.SonarRestConnector.GetResourcesData(this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig, key)[0];
                    var fileName = Path.GetFileName(fullName);
                    toReturn.Name = fileName;
                    toReturn.Lname = fileName;
                    this.ErrorMessage = "Resource Found";
                    return toReturn;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            this.ErrorMessage = "Resource not found in Server";

            return null;
        }

        /// <summary>
        /// The execute plugin.
        /// </summary>
        /// <param name="header">
        /// The data.
        /// </param>
        public void ExecutePlugin(string header)
        {
            foreach (var plugin in this.menuItemPlugins)
            {
                if (plugin.Value.GetHeader().Equals(header))
                {
                    var isEnabled =
                        this.VsHelper.ReadOptionFromApplicationData(
                            GlobalIds.PluginEnabledControlId,
                            plugin.Value.GetPluginDescription(this.VsHelper).Name);

                    if (string.IsNullOrEmpty(isEnabled)
                        || isEnabled.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (this.IsRunningInVisualStudio())
                        {
                            this.InUsePlugin = plugin;
                            this.OnPluginRequest(EventArgs.Empty);
                        }
                        else
                        {
                            var window = new Window();
                            window.Content =
                                plugin.Value.GetUserControl(
                                    this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig,
                                    this.AssociatedProject,
                                    this.VsHelper);

                            window.ShowDialog();
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            plugin.Value.GetPluginDescription(this.VsHelper).Name + " is disabled");
                    }
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
        ///     The init options viewModel.
        /// </summary>
        private void InitOptionsModel()
        {
            List<IAnalysisPlugin> plugins = this.PluginControl.GetPlugins();
            this.VSonarQubeOptionsViewData = new VSonarQubeOptionsViewModel(this.PluginControl, this, this.VsHelper)
                                                 {
                                                     Vsenvironmenthelper =
                                                         this.VsHelper
                                                 };
            this.VSonarQubeOptionsViewData.ResetUserData();

            if (plugins != null)
            {
                foreach (IAnalysisPlugin plugin in plugins)
                {
                    ISonarConfiguration configuration = this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig;
                    IPluginsOptions controloption = plugin.GetPluginControlOptions(configuration);
                    if (controloption == null)
                    {
                        continue;
                    }

                    string pluginKey = plugin.GetKey(this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig);
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
            this.ServerViewModel = new ServerViewModel(
                this, 
                this.VsHelper, 
                this.SonarRestConnector, 
                this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig);
            this.LocalViewModel = new LocalViewModel(this, this.PluginControl.GetPlugins(), this.SonarRestConnector, this.VsHelper);
            this.IssuesSearchViewModel = new IssuesSearchViewModel(this, this.VsHelper, this.SonarRestConnector);

            this.SonarQubeViews = new ObservableCollection<IAnalysisViewModelBase>
                                      {
                                          this.ServerViewModel, 
                                          this.LocalViewModel, 
                                          this.IssuesSearchViewModel
                                      };

            var view = this.VsHelper.ReadOptionFromApplicationData("VSSonarQubeConfig", "SelectedView");

            foreach (var analysisViewModelBase in this.SonarQubeViews.Where(analysisViewModelBase => analysisViewModelBase.Header.Equals(view)))
            {
                this.SelectedView = analysisViewModelBase;
            }
        }

        /// <summary>
        ///     The launch extension properties.
        /// </summary>
        private void LaunchExtensionProperties()
        {
            var window = new ExtensionOptionsWindow(this.VSonarQubeOptionsViewData);
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
                this.VsHelper.WriteOptionInApplicationData(this.OpenSolutionName, "PROJECTLOCATION", this.OpenSolutionPath);
            }

            this.VSonarQubeOptionsViewData.Project = this.SelectedProject;
            this.VSonarQubeOptionsViewData.RefreshGeneralProperties();
            this.VSonarQubeOptionsViewData.RefreshPropertiesInPlugins();
            this.VSonarQubeOptionsViewData.SyncOptionsToFile();
            this.AssociatedProject = this.SelectedProject;

            if (string.IsNullOrEmpty(this.OpenSolutionPath))
            {
                this.OpenSolutionPath = this.VsHelper.ReadOptionFromApplicationData(this.AssociatedProject.Key, "PROJECTLOCATION");
            }            

            foreach (
                IAnalysisViewModelBase analyser in
                    (from IViewModelBase sonarQubeView in this.SonarQubeViews select sonarQubeView).OfType<IAnalysisViewModelBase>())
            {
                analyser.InitDataAssociation(
                    this.AssociatedProject, 
                    this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig, 
                    this.OpenSolutionPath);
            }

            foreach (
                IOptionsViewModelBase option in
                    (from IViewModelBase sonarQubeView in this.VSonarQubeOptionsViewData.AvailableOptions select sonarQubeView)
                        .OfType<IOptionsViewModelBase>())
            {
                option.InitDataAssociation(
                    this.AssociatedProject, 
                    this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig, 
                    this.OpenSolutionPath);
            }

            this.ErrorMessage = string.Empty;
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
        private void OnRefreshProjectList(IEnumerable<Resource> projects)
        {
            this.IsConnected = true;
            this.IsConnectedButNotAssociated = false;

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
                if (this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig == null)
                {
                    var window = new ExtensionOptionsWindow(this.VSonarQubeOptionsViewData);
                    this.VSonarQubeOptionsViewData.SelectedOption = this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel;
                    window.ShowDialog();
                }

                if (this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig == null)
                {
                    this.IsConnected = false;
                    return;
                }

                this.IsConnected = true;
                this.IsConnectedButNotAssociated = true;

                this.ConnectionTooltip = "Authenticated, but no associated";

                List<Resource> projects =
                    this.SonarRestConnector.GetProjectsList(this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig);
                if (projects != null && projects.Count > 0)
                {
                    this.OnRefreshProjectList(projects);
                }

                this.SonarVersion =
                    this.SonarRestConnector.GetServerInfo(this.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserConnectionConfig);
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
            }
        }

        #endregion
    }
}