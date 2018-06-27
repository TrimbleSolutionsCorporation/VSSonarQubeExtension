// --------------------------------------------------------------------------------------------------------------------
// <copyright file="sqaleeditorcontrolviewmodel.cs" company="Copyright © 2014 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
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

namespace SqaleUi.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;

    using VSSonarPlugins.Types;

    using MahApps.Metro.Controls;

    using PropertyChanged;

    using Security.Windows.Forms;

    using SonarRestService;

    using SqaleManager;

    using SqaleUi.Menus;
    using SqaleUi.View;

    using VSSonarPlugins;

    using MessageBox = System.Windows.Forms.MessageBox;

    /// <summary>
    /// The sqale editor control view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class SqaleEditorControlViewModel
    {
        #region Fields

        /// <summary>
        ///     The selected tab.
        /// </summary>
        private SqaleGridVm selectedTab;
        private ISonarConfiguration configuration;
        private ISonarRestService service;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the SqaleEditorControlViewModel class.
        /// </summary>
        public SqaleEditorControlViewModel()
        {
            this.Tabs = new ObservableCollection<SqaleGridVm>();
            this.Tabs.CollectionChanged += TabsHaveChanged;

            this.RestService = new SonarRestService(new JsonSonarConnector());
            this.Project = null;
            this.ConnectedToServer = false;

            this.InitCommanding();

            this.StatusMessage = "OffLine -> Press Icon on the right to connect to Server";
            this.BackGroundColor = Colors.Black;
            this.ForeGroundColor = Colors.White;
        }

        /// <summary>
        ///     Initializes a new instance of the SqaleEditorControlViewModel class.
        /// </summary>
        public SqaleEditorControlViewModel(IConfigurationHelper helper, ISonarRestService service, ISonarConfiguration configuration)
        {
            this.service = service;
            this.configuration = configuration;
            this.Tabs = new ObservableCollection<SqaleGridVm>();
            this.Tabs.CollectionChanged += TabsHaveChanged;

            this.RestService = new SonarRestService(new JsonSonarConnector());
            this.VsHelper = helper;
            this.Project = null;
            this.ConnectedToServer = false;

            this.InitCommanding();

            this.StatusMessage = "OffLine -> Press Icon on the right to connect to Server";
            this.BackGroundColor = Colors.Black;
            this.ForeGroundColor = Colors.White;
        }

        public ISonarRestService RestService { get; set; }

        public SqaleEditorControlViewModel(ISonarConfiguration configuration, Resource project, IConfigurationHelper vshelper)
        {
            this.Tabs = new ObservableCollection<SqaleGridVm>();
            this.InitCommanding();

            this.Configuration = configuration;
            this.Project = project;
            this.VsHelper = vshelper;
            this.ConnectedToServer = false;
        }

        private void InitCommanding()
        {
            this.CanExecuteNewProjectCommand = true;
            this.CanExecuteOpenProjectCommand = true;
            this.CanExecuteSaveProjectCommand = false;
            this.CanExecuteSaveAsProjectCommand = false;
            this.CanExecuteCloseProjectCommand = false;

            this.NewProjectCommand = new RelayCommand<object>(this.ExecuteNewProjectCommand);
            this.OpenProjectCommand = new RelayCommand<object>(this.ExecuteOpenProjectCommand);
            this.SaveProjectCommand = new RelayCommand<object>(this.ExecuteSaveProjectCommand);
            this.SaveAsProjectCommand = new RelayCommand<object>(this.ExecuteSaveAsProjectCommand);
            this.CloseProjectCommand = new RelayCommand<object>(this.ExecuteCloseProjectCommand);

            this.CreateWorkAreaCommand = new RelayCommand<object>(item => this.CreateNewWorkArea(false, !this.Tabs[0].ConnectedToSonarServer));
            this.DeleteWorkAreaCommand = new RelayCommand<object>(this.RemoveCurrentSelectedTab);

            this.ConnectCommand = new RelayCommand<object>(this.OnConnectCommand);
            this.ConnectedToServerCommand = new RelayCommand<object>(this.OnConnectedToServerCommand);
            this.DisconnectToServerCommand = new RelayCommand<object>(this.OnDisconnectToServerCommand);
            this.SelectedNewServerCommand = new RelayCommand<object>(this.OnSelectedNewServerCommand);


            this.OpenVsWindow = new RelayCommand<object>(this.OnOpenVsWindow);
        }

        private void OnSelectedNewServerCommand(object data)
        {
            this.ServerAddress = PromptUserData.Prompt("Server Address", "Insert Server Address", "http://localhost:9000");
            if (!string.IsNullOrEmpty(this.ServerAddress))
            {
                this.VsHelper.WriteSetting(Context.MenuPluginProperties, "QualityEditorPlugin", "ServerAddress", this.ServerAddress);
            }
        }

        private void OnDisconnectToServerCommand(object data)
        {
            this.Configuration = null;
            this.IsConnected = false;
            this.StatusMessage = "OffLine -> Press Icon on the right to connect to Server";
        }

        private void OnConnectedToServerCommand(object data)
        {
            if (!this.CreateSonarConnection(false))
            {
                MessageBox.Show("Cannot Authenticate against: " + this.ServerAddress + " check you settings or change server address");
                this.Configuration = null;
                this.IsConnected = false;
                return;
            }

            if (this.ServerAddress == null)
            {
                MessageBox.Show("Server address not Set");
                return;
            }

            this.IsConnected = true;
            this.StatusMessage = "OnLine";
        }

        private void OnConnectCommand(object data)
        {
            if (this.IsConnected)
            {
                this.OnConnectedToServerCommand(null);
                foreach (var tab in this.Tabs)
                {
                    tab.IsConnected = this.IsConnected;
                    tab.Configuration = this.Configuration;
                }
            }
            else
            {                
                this.OnDisconnectToServerCommand(null);
            }
        }

        public bool IsConnected { get; set; }

        public ICommand ConnectCommand { get; set; }

        public ICommand SelectedNewServerCommand { get; set; }

        public ICommand DisconnectToServerCommand { get; set; }

        public ICommand ConnectedToServerCommand { get; set; }

        private void OnOpenVsWindow(object data)
        {
            var modelProject = new ProjectViewModel(this.RestService, this.Configuration);
            ProjectViewer projectSelector = new ProjectViewer(modelProject);
            projectSelector.ShowDialog();

            var window = new MetroWindow();
            window.Height = 600;
            window.Width = 800;
            window.Title = "Project Quality Editor";
            var model = new SqaleGridVmVs(modelProject.SelectedProject, this.RestService, this.Configuration);
            model.UpdateColors(this.BackGroundColor, this.ForeGroundColor);
            var grid = new SqaleGridVs(model);
            window.Content = grid;
            window.ShowDialog();
        }

        #endregion

        #region Public Properties

        public string StatusMessage { get; set; }

        /// <summary>
        ///     Gets a value indicating whether can execute new project command.
        /// </summary>
        public bool CanExecuteCloseProjectCommand { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether can execute new project command.
        /// </summary>
        public bool CanExecuteNewProjectCommand { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether can execute new project command.
        /// </summary>
        public bool CanExecuteOpenProjectCommand { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether can execute save project command.
        /// </summary>
        public bool CanExecuteSaveProjectCommand { get; set; }

        /// <summary>
        ///     Gets the close project command.
        /// </summary>
        public ICommand CloseProjectCommand { get; private set; }

        public ICommand OpenVsWindow { get; private set; }
        /// <summary>
        /// Gets or sets the create work area command.
        /// </summary>
        public ICommand CreateWorkAreaCommand { get; set; }

        /// <summary>
        /// Gets or sets the delete work area command.
        /// </summary>
        public object DeleteWorkAreaCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether is add tab enabled.
        /// </summary>
        public bool IsAddTabEnabled { get; set; }

        /// <summary>
        /// Gets the is remove tab enabled.
        /// </summary>
        public bool IsRemoveTabEnabled { get; set; }

        public ICommand EstablishConnectionCommand { get; private set; }

        /// <summary>
        ///     Gets the new project command.
        /// </summary>
        public ICommand NewProjectCommand { get; private set; }

        /// <summary>
        ///     Gets the open project command.
        /// </summary>
        public ICommand OpenProjectCommand { get; private set; }

        /// <summary>
        /// Gets or sets the save project command.
        /// </summary>
        public ICommand SaveProjectCommand { get; set; }

        /// <summary>
        /// Gets or sets the selected tab.
        /// </summary>
        public SqaleGridVm SelectedTab
        {
            get
            {
                return this.selectedTab;
            }

            set
            {
                if (value != null)
                {
                    value.RefreshView();
                }

                this.selectedTab = value;
            }
        }

        /// <summary>
        /// Gets or sets the tabs.
        /// </summary>
        [AlsoNotifyFor("IsAddTabEnabled")]
        public ObservableCollection<SqaleGridVm> Tabs { get; set; }

        public ICommand SaveAsProjectCommand { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create new work area.
        /// </summary>
        /// <param name="contextMenu"></param>
        /// <param name="showContextMenu">
        ///     The show context menu.
        /// </param>
        /// <returns>
        /// The <see cref="SqaleGridVm"/>.
        /// </returns>
        public SqaleGridVm CreateNewWorkArea(bool showContextMenu, bool allowServerConnection = true)
        {
            var newWorkArea = new SqaleGridVm(this, new SqaleManager(this.service, this.configuration), this.service, this.configuration)
                                  {
                                      Header = "Non Saved Data: " + this.Tabs.Count, 
                                      ShowContextMenu = showContextMenu, 
                                      CanSendToProject = true, 
                                      CanSendToWorkAreaCommand = true,
                                      CanImportFromSonarServer = allowServerConnection,
                                      IsDirty = false
                                  };
            newWorkArea.CanSendToWorkAreaCommand = false;
            this.Tabs.Add(newWorkArea);
            this.SelectedTab = newWorkArea;
            return newWorkArea;
        }

        /// <summary>
        /// The create work area with data set.
        /// </summary>
        /// <param name="profileRules">
        /// The profile rules.
        /// </param>
        public void CreateWorkAreaWithDataSet(ObservableCollection<Rule> profileRules)
        {
            var newWorkAread = new SqaleGridVm(this, new SqaleManager(this.service, this.configuration), this.service, this.configuration) { Header = "Non Saved Data: " + this.Tabs.Count };
            newWorkAread.CanSendToWorkAreaCommand = false;
            foreach (Rule profileRule in profileRules)
            {
                newWorkAread.ProfileRules.Add(new Rule());
            }

            this.Tabs.Add(newWorkAread);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The execute close project command.
        /// </summary>
        private void ExecuteCloseProjectCommand(object data)
        {

            this.ExecuteSaveAsProjectCommand(null);
            this.Tabs.Remove(this.Tabs[0]);

            this.CanExecuteNewProjectCommand = true;
            this.CanExecuteOpenProjectCommand = true;

            this.CanExecuteSaveProjectCommand = false;
            this.CanExecuteSaveAsProjectCommand = false;
            this.CanExecuteCloseProjectCommand = false;
        }

        /// <summary>
        ///     The execute new project command.
        /// </summary>
        private void ExecuteNewProjectCommand(object data)
        {
            this.CreateNewProject(string.Empty, this.Configuration, this.Project, this.VsHelper);
            this.CanExecuteNewProjectCommand = false;
            this.CanExecuteOpenProjectCommand = false;

            this.CanExecuteSaveProjectCommand = true;
            this.CanExecuteSaveAsProjectCommand = true;
            this.CanExecuteCloseProjectCommand = true;
        }

        /// <summary>
        /// The connect to sonar.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public void ConnectToSonar(ISqaleGridVm model)
        {
            bool userCancel = true;
            bool resetServer = false;
            while (userCancel && !this.CreateSonarConnection(resetServer))
            {
                DialogResult result = MessageBox.Show("Cannot Connect, try again?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                userCancel = result == DialogResult.Yes;
                resetServer = true;
            }

            model.Configuration = this.Configuration;
        }

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
            if (this.Configuration == null)
            {
                // find server address
                try
                {
                    this.ServerAddress = this.VsHelper.ReadSetting(Context.MenuPluginProperties, "QualityEditorPlugin", "ServerAddress").Value;
                }
                catch (Exception)
                {
                }

                if (string.IsNullOrEmpty(this.ServerAddress) || resetServer)
                {
                    this.ServerAddress = PromptUserData.Prompt("Server Address", "Insert Server Address", "http://localhost:9000");
                    if (this.ServerAddress == null)
                    {
                        return false;
                    }

                    this.VsHelper.WriteSetting(Context.MenuPluginProperties, "QualityEditorPlugin", "ServerAddress", this.ServerAddress);
                }

                using (var dialog = new UserCredentialsDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.SaveChecked)
                        {
                            dialog.ConfirmCredentials(true);
                        }

                        this.Configuration = new ConnectionConfiguration(this.ServerAddress, dialog.User, dialog.PasswordToString(), 4.5);
                        this.ConnectedToServer = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return this.RestService.AuthenticateUser(this.Configuration);
        }

        public bool ConnectedToServer { get; set; }

        public string ServerAddress { get; set; }

        public bool CanExecuteSaveAsProjectCommand { get; set; }

        /// <summary>
        ///     The execute open project command.
        /// </summary>
        private void ExecuteOpenProjectCommand(object data)
        {
            // Do something 
            var filedialog = new OpenFileDialog { Filter = @"Project Model|*.xml" };

            DialogResult result = filedialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            try
            {
                this.CreateNewProject(filedialog.FileName, this.Configuration, this.Project, this.VsHelper);

                this.CanExecuteNewProjectCommand = false;
                this.CanExecuteOpenProjectCommand = false;
                this.CanExecuteSaveAsProjectCommand = true;
                this.CanExecuteSaveProjectCommand = true;
                this.CanExecuteCloseProjectCommand = true;

                this.Tabs[0].ProjectFile = filedialog.FileName;
            }
            catch (Exception ex)
            {
                this.Tabs.Clear();
                MessageBox.Show("Cannot Open Project: " + ex.Message);
            }

        }

        public void CreateNewProject(string fileName, ISonarConfiguration configuration, Resource resource, IConfigurationHelper vshelper)
        {
            var project = new SqaleGridVm(this, new SqaleManager(this.service, this.configuration), this.service, this.configuration)
                              {
                                  Header = "Project",
                                  ShowContextMenu = true,
                                  CanSendToProject = false,
                                  CanSendToWorkAreaCommand = true,
                                  Configuration = configuration,
                                  Solution = resource,
                                  VShelper = vshelper
                              };

            this.Tabs.Add(project);
            this.SelectedTab = this.Tabs[0];

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            if (!File.Exists(fileName))
            {
                return;
            }

            var model = project.SqaleManager.ImportSqaleProjectFromFile(fileName);
            foreach (var rule in model.GetProfile().GetAllRules())
            {
                project.ProfileRules.Add(rule);
            }

            var importViewerModel = new ImportLogViewModel();
            importViewerModel.ImportLog = project.SqaleManager.GetImportLog();
            if (importViewerModel.ImportLog.Count > 0)
            {
                var importLogWindow = new ImportLogView(importViewerModel);
                importLogWindow.Show();
            }
        }

        /// <summary>
        /// The execute save project command.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        private void ExecuteSaveProjectCommand(object data)
        {
            if (string.IsNullOrEmpty(this.Tabs[0].ProjectFile))
            {
                this.ExecuteSaveAsProjectCommand(null);
                return;
            }

            var modelToExport = this.Tabs[0].SqaleManager.CreateModelFromRules(this.Tabs[0].ProfileRules);
            this.Tabs[0].SqaleManager.SaveSqaleModelAsXmlProject(modelToExport, this.Tabs[0].ProjectFile);
        }

        private void ExecuteSaveAsProjectCommand(object data)
        {
            var saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "xml files (*.xml)|*.xml";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var modelToExport = this.Tabs[0].SqaleManager.CreateModelFromRules(this.Tabs[0].ProfileRules);
                    this.Tabs[0].SqaleManager.SaveSqaleModelAsXmlProject(modelToExport, saveFileDialog.FileName);
                    this.Tabs[0].ProjectFile = saveFileDialog.FileName;
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Cannot Save: " + ex.Message);
                }


            }            
        }

        /// <summary>
        /// The remove current selected tab.
        /// </summary>
        private void RemoveCurrentSelectedTab(object data)
        {
            if (this.SelectedTab.Header.Equals("Project"))
            {
                return;
            }

            if (!this.SelectedTab.IsDirty)
            {
                this.Tabs.Remove(this.SelectedTab);
                this.SelectedTab = this.Tabs[0];
                this.Tabs[0].RefreshMenus();
                return;
            }

            DialogResult result = MessageBox.Show("Changes will be lost, proceed?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Tabs.Remove(this.SelectedTab);
                this.SelectedTab = this.Tabs[0];
            }
        }

        #endregion

        public IConfigurationHelper VsHelper { get; set; }

        public Resource Project { get; set; }

        public ISonarConfiguration Configuration { get; set; }

        public void UpdateConfiguration(ISonarConfiguration configuration, Resource project, IConfigurationHelper vshelper)
        {
            this.Configuration = configuration;
            this.Project = project;
            this.VsHelper = vshelper;
        }

        private void TabsHaveChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.Tabs.Count == 0)
            {
                this.IsAddTabEnabled = false;
                this.IsRemoveTabEnabled = false;
            }
            else if (this.Tabs.Count == 1)
            {
                this.IsAddTabEnabled = true;
                this.IsRemoveTabEnabled = false;
            }
            else
            {
                this.IsAddTabEnabled = true;
                this.IsRemoveTabEnabled = true;
            }

        }

        public Color BackGroundColor { get; set; }

        public Color ForeGroundColor { get; set; }
    }
}