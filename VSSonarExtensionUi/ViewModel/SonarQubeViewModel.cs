// --------------------------------------------------------------------------------------------------------------------
// <copyright file="sonarqubeviewmodel.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
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
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Forms;

    using ExtensionHelpers;

    using ExtensionTypes;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using Security.Windows.Forms;

    using SonarRestService;

    using VSSonarExtensionUi.View;

    using VSSonarPlugins;

    using MenuItem = System.Windows.Controls.MenuItem;
    using MessageBox = System.Windows.Forms.MessageBox;
    using VSSonarExtensionUi.Cache;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;





    /// <summary>
    /// The sonar qube view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class SonarQubeViewModel
    {
        /// <summary>
        /// The analysisPlugin control.
        /// </summary>
        public readonly PluginController PluginControl = new PluginController();
        private readonly Dictionary<int, IMenuCommandPlugin> menuItemPlugins = new Dictionary<int, IMenuCommandPlugin>();

        private object sonarQubeView;

        private Resource associatedProject;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeViewModel"/> class.
        /// </summary>
        public SonarQubeViewModel()
        {
            this.VsHelper = new VsPropertiesHelper();
            this.SonarRestConnector = new SonarRestService(new JsonSonarConnector());

            this.IsExtensionBusy = true;
            this.IsConnected = false;

            this.InitOptionsModel();
            this.InitData();
            this.InitConnection();
            this.InitCommands();
            this.InitMenus();
            this.InitViews();

            this.SetConnectionIcon("pack://application:,,,/VSSonarExtensionUi;component/Icons/no_connection.ico");
            this.ConnectionTooltip = "Not Connected";
        }

        public SonarQubeViewModel(
            ISonarRestService restServiceIn,
            IVsEnvironmentHelper vsenvironmenthelperIn,
            Resource associatedProj,
            IVSSStatusBar statusBar,
            IServiceProvider provider)
        {

            this.IsExtensionBusy = false;
            this.SonarRestConnector = restServiceIn;
            this.VsHelper = vsenvironmenthelperIn;
            this.StatusBar = statusBar;
            this.ServiceProvider = provider;

            this.InitOptionsModel();
            this.InitData();
            this.InitConnection();
            this.InitCommands();
            this.InitMenus();
            this.InitViews();

            this.SetConnectionIcon("pack://application:,,,/VSSonarExtensionUi;component/Icons/no_connection.ico");
            this.ConnectionTooltip = "Not Connected";
        }

        private void InitOptionsModel()
        {
            List<IAnalysisPlugin> plugins = this.PluginControl.GetPlugins();           
            this.ExtensionOptionsData = new ExtensionOptionsModel(this.PluginControl, this, this.VsHelper, this.SonarCubeConfiguration);
            this.ExtensionOptionsData.Vsenvironmenthelper = this.VsHelper;
            this.ExtensionOptionsData.ResetUserData();

            if (plugins != null)
            {
                foreach (IAnalysisPlugin plugin in plugins)
                {
                    ISonarConfiguration configuration = ConnectionConfigurationHelpers.GetConnectionConfiguration(this.VsHelper);
                    IPluginsOptions controloption = plugin.GetPluginControlOptions(configuration);
                    if (controloption == null)
                    {
                        continue;
                    }

                    string pluginKey = plugin.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.VsHelper));
                    Dictionary<string, string> options = this.VsHelper.ReadAllAvailableOptionsInSettings(pluginKey);
                    controloption.SetOptions(options);
                }
            }
        }

        public ExtensionOptionsModel ExtensionOptionsData { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        public IVSSStatusBar StatusBar { get; set; }

        private void InitData()
        {
            this.ToolsProvidedByPlugins = new ObservableCollection<MenuItem>();
            this.AvailableProjects = new ObservableCollection<Resource>();
        }

        private void InitConnection()
        {
            this.ConnectedToServerCommand = new RelayCommand<string>(this.ConnectToSonar);
        }

        private void InitViews()
        {
            this.ServerViewModel = new ServerViewModel(this);
            this.LocalViewModel = new LocalViewModel(this, this.SonarRestConnector, this.VsHelper, this.PluginControl.GetPlugins());
            this.IssuesSearchViewModel = new IssuesSearchViewModel(this, this.VsHelper, this.SonarRestConnector);

            this.SonarQubeViews = new ObservableCollection<IViewModelBase>();
            this.SonarQubeViews.Add(this.ServerViewModel);
            this.SonarQubeViews.Add(this.LocalViewModel);
            this.SonarQubeViews.Add(this.IssuesSearchViewModel);
        }

        public IssuesSearchViewModel IssuesSearchViewModel { get; set; }

        public LocalViewModel LocalViewModel { get; set; }

        public ServerViewModel ServerViewModel { get; set; }

        public bool IsRunningInVisualStudio()
        {
            return this.VsHelper.AreWeRunningInVisualStudio();
        }

        /// <summary>
        /// The init menus.
        /// </summary>
        private void InitMenus()
        {
            if (this.PluginControl == null)
            {
                return;
            }

            var plugins = this.PluginControl.GetMenuItemPlugins();

            if (plugins == null)
            {
                return;
            }

            foreach (var plugin in plugins)
            {
                this.ToolsProvidedByPlugins.Add(new MenuItem { Header = plugin.GetHeader() });
                this.menuItemPlugins.Add(this.menuItemPlugins.Count, plugin);
            }
        }

        /// <summary>
        /// The init commands.
        /// </summary>
        private void InitCommands()
        {
            this.LaunchExtensionPropertiesCommand = new RelayCommand(this.LaunchExtensionProperties);
            this.ToolSwitchCommand = new RelayCommand<string>(this.ExecuteToolWindow);

            this.AssignProjectCommand = new RelayCommand(this.OnAssignProjectCommand);
            this.ClearCacheCommand = new RelayCommand(this.OnClearCacheCommand);
        }

        private void OnClearCacheCommand()
        {
            throw new System.NotImplementedException();
        }


        /// <summary>
        /// The perform solution not open association.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        public void PerformSolutionNotOpenAssociation(Resource resource)
        {
            if (resource != null)
            {
                this.AssociatedProjectKey = resource.Key;
            }
            else
            {
                this.AssociatedProjectKey = string.Empty;
            }

            this.associatedProject = resource;

            this.IssuesSearchViewModel.InitDataAssociation(this.associatedProject, this.SonarCubeConfiguration);


            this.SetConnectionIcon("pack://application:,,,/VSSonarExtensionUi;component/Icons/high_connection.ico");
            this.ConnectionTooltip = "Connected and Associated";
        }

        public bool IsExtensionBusy { get; set; }


        public string AssociatedProjectKey { get; set; }

        private void OnAssignProjectCommand()
        {
            var solutionName = this.VsHelper.ActiveSolutionName();
            var selectedProjectInFilter = this.AssociatedProject;
            if (selectedProjectInFilter == null && string.IsNullOrEmpty(solutionName))
            {
                return;
            }

            if (string.IsNullOrEmpty(solutionName))
            {
                this.PerformSolutionNotOpenAssociation(selectedProjectInFilter);
                return;
            }

            if (this.AssociatedProjectKey == null)
            {
                this.VsHelper.WriteOptionInApplicationData(solutionName, "PROJECTKEY", selectedProjectInFilter.Key);
            }
            else
            {
                var dialogResult = MessageBox.Show("Do you want to save this option to environment, so it will reuse this association when Visual Studio Restart?", "Save Association to Disk", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    this.VsHelper.WriteOptionInApplicationData(solutionName, "PROJECTKEY", selectedProjectInFilter.Key);
                }
            }

            this.AssociateProjectToSolution();
        }

        private void AssociateProjectToSolution()
        {
            Resource newProject = this.AssociateSolutionWithSonarProject();
            if (newProject == null)
            {
                return;
            }

            // startup data
            this.TempDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VSSonarExtension\\" + newProject.Name;

            if (Directory.Exists(this.TempDataFolder))
            {
                Directory.Delete(this.TempDataFolder, true);
            }

            this.ExtensionOptionsData.Project = newProject;
            this.ExtensionOptionsData.RefreshGeneralProperties();
            this.ExtensionOptionsData.RefreshPropertiesInPlugins();
            this.ExtensionOptionsData.SyncOptionsToFile();
            this.AssociatedProject = newProject;
        }

        public Resource AssociatedProject { get; set; }

        public string TempDataFolder { get; set; }

        private Resource AssociateSolutionWithSonarProject()
        {
            string solutionName = this.VsHelper.ActiveSolutionName();

            if (this.SonarCubeConfiguration == null)
            {
                this.ErrorMessage = "User Not Logged In, Extension is Unusable. Configure User Settions in Tools > Options > Sonar";
                return null;
            }

            string sourceKey = this.VsHelper.ReadOptionFromApplicationData(solutionName, "PROJECTKEY");
            if (!string.IsNullOrEmpty(sourceKey))
            {
                try
                {
                    return this.SonarRestConnector.GetResourcesData(this.SonarCubeConfiguration, sourceKey)[0];
                }
                catch (Exception ex)
                {
                    UserExceptionMessageBox.ShowException("Associated Project does not exist in server, please configure association", ex);
                    return null;
                }
            }

            string solutionPath = this.VsHelper.ActiveSolutionPath();
            sourceKey = VsSonarUtils.GetProjectKey(solutionPath);
            this.VsHelper.WriteOptionInApplicationData(solutionName, "PROJECTKEY", sourceKey);

            return string.IsNullOrEmpty(sourceKey) ? null : this.SonarRestConnector.GetResourcesData(this.SonarCubeConfiguration, sourceKey)[0];
        }

        public string ErrorMessage { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the launch extension properties command.
        /// </summary>
        public RelayCommand LaunchExtensionPropertiesCommand { get; set; }

        /// <summary>
        /// Gets or sets the tool switch command.
        /// </summary>
        public RelayCommand<string> ToolSwitchCommand { get; set; }

        public RelayCommand AssignProjectCommand { get; set; }

        public RelayCommand ClearCacheCommand { get; set; }
        

        /// <summary>
        /// Gets the tools provided by plugins.
        /// </summary>
        public ObservableCollection<MenuItem> ToolsProvidedByPlugins { get; set; }

        /// <summary>
        /// Gets or sets the sonar qube views.
        /// </summary>
        public ObservableCollection<IViewModelBase> SonarQubeViews { get; set; }

        public IViewModelBase SelectedView { get; set; }

        public object SonarQubeView
        {
            get
            {
                return this.sonarQubeView;

            }
            set
            {
                this.sonarQubeView = value;
            }
        }

        /// <summary>
        /// The vs helper.
        /// </summary>
        public IVsEnvironmentHelper VsHelper { get; set; }

        public ISonarConfiguration SonarCubeConfiguration { get; set; }

        public ISonarRestService SonarRestConnector { get; set; }

        #endregion

        #region Methods


        /// <summary>
        /// The connect to sonar.
        /// </summary>
        public void ConnectToSonar(string connectionCommand)
        {
            if (connectionCommand.Equals("Disconnect"))
            {
                this.SonarCubeConfiguration = null;
                this.IsConnected = false;
                this.SetConnectionIcon("pack://application:,,,/VSSonarExtensionUi;component/Icons/no_connection.ico");
                this.ConnectionTooltip = "Not Connected";
                this.AssociatedProject = null;
                return;
            }

            bool userCancel = true;
            bool resetServer = false;
            while (userCancel && !this.CreateSonarConnection(resetServer))
            {
                DialogResult result = MessageBox.Show("Cannot Connect, try again?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                userCancel = result == DialogResult.Yes;
                resetServer = true;
            }

            if (userCancel)
            {
                this.IsConnected = false;
                this.IsConnectedButNotAssociated = true;

                this.SetConnectionIcon("pack://application:,,,/VSSonarExtensionUi;component/Icons/medium_connection.ico");
                this.ConnectionTooltip = "Authenticated, but no associated";

                List<Resource> projects = this.SonarRestConnector.GetProjectsList(this.SonarCubeConfiguration);
                if (projects != null && projects.Count > 0)
                {
                    this.OnRefreshProjectList(projects);
                }
            }
        }

        private void SetConnectionIcon(string uri)
        {
            this.ConnectionImage = uri;
        }

        public string ConnectionImage { get; set; }

        private void OnRefreshProjectList(List<Resource> projects)
        {
            this.IsConnected = true;
            this.IsConnectedButNotAssociated = false;

            this.AvailableProjects.Clear();
            this.AvailableProjects = new ObservableCollection<Resource>(projects.OrderBy(i => i.Name));
        }

        public bool IsConnectedButNotAssociated { get; set; }

        public ObservableCollection<Resource> AvailableProjects { get; set; }

        /// <summary>
        /// The create sonar connection.
        /// </summary>
        /// <param name="resetServer">
        /// The reset server.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CreateSonarConnection(bool resetServer)
        {
            if (this.SonarCubeConfiguration == null)
            {
                // find server address
                this.ServerAddress = this.VsHelper.ReadOptionFromApplicationData("QualityEditorPlugin", "ServerAddress");

                if (string.IsNullOrEmpty(this.ServerAddress) || resetServer)
                {
                    this.ServerAddress = PromptUserData.Prompt("Server Address", "Insert Server Address", "http://localhost:9000");
                    if (this.ServerAddress == null)
                    {
                        return false;
                    }

                    this.VsHelper.WriteOptionInApplicationData("QualityEditorPlugin", "ServerAddress", this.ServerAddress);
                }

                using (var dialog = new UserCredentialsDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.SaveChecked)
                        {
                            dialog.ConfirmCredentials(true);
                        }

                        this.SonarCubeConfiguration = new ConnectionConfiguration(this.ServerAddress, dialog.User, dialog.PasswordToString());
                        this.ConnectedToServer = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return this.SonarRestConnector.AuthenticateUser(this.SonarCubeConfiguration);
        }

        public bool ConnectedToServer { get; set; }

        public string ServerAddress { get; set; }

        public RelayCommand<string> ConnectedToServerCommand { get; set; }

        public bool IsConnected { get; set; }

        public string ConnectionTooltip { get; set; }


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
        /// The launch extension properties.
        /// </summary>
        private void LaunchExtensionProperties()
        {
            var window = new ExtensionOptionsWindow(this.ExtensionOptionsData);
            window.ShowDialog();
        }

        #endregion
    }
}