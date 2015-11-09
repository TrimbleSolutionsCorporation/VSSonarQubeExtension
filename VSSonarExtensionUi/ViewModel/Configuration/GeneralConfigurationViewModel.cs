// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneralConfigurationViewModel.cs" company="">
//
// </copyright>
// <summary>
//   The sonar configuration view viewModel.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.ViewModel.Configuration
{
    using System;
    using System.Diagnostics;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    using CredentialManagement;
    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using Model.Helpers;
    using PropertyChanged;
    using SonarLocalAnalyser;
    using View.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using VSSonarExtensionUi.Association;

    /// <summary>
    /// The sonar configuration view viewModel.
    /// </summary>
    [ImplementPropertyChanged]
    public class GeneralConfigurationViewModel : IOptionsViewModelBase, IOptionsModelBase
    {
        #region Fields

        /// <summary>
        ///     The viewModel.
        /// </summary>
        private readonly VSonarQubeOptionsViewModel viewModel;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        ///     The rest service.
        /// </summary>
        private readonly ISonarRestService restService;

        /// <summary>
        /// The notification manager
        /// </summary>
        private readonly INotificationManager notificationManager;

        /// <summary>
        ///     The visual studio helper.
        /// </summary>
        private IVsEnvironmentHelper visualStudioHelper;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// The source dir
        /// </summary>
        private string sourceDir;

        #endregion Fields

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralConfigurationViewModel" /> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="restService">The rest service.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="notificationManager">The notification manager.</param>
        public GeneralConfigurationViewModel(
            VSonarQubeOptionsViewModel viewModel,
            ISonarRestService restService,
            IConfigurationHelper configurationHelper,
            INotificationManager notificationManager)
        {
            this.Header = "General Settings";
            this.UserName = string.Empty;
            this.Password = string.Empty;
            this.viewModel = viewModel;
            this.restService = restService;
            this.configurationHelper = configurationHelper;
            this.notificationManager = notificationManager;

            this.ClearCacheCommand = new RelayCommand(this.OnClearCacheCommand);
            this.TestConnectionCommand = new RelayCommand<object>(this.OnTestAndSavePassword);
            this.ClearCredentials = new RelayCommand(this.OnClearCredentials);

            this.BackGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;

            this.GetCredentials();
            this.ReloadDataFromDisk(null);

            // register model
            AssociationModel.RegisterNewModelInPool(this);
            SonarQubeViewModel.RegisterNewViewModelInPool(this);
        }

        #endregion Constructors and Destructors

        #region Public Events

        /// <summary>
        ///     The analysis mode has change.
        /// </summary>
        public event ChangedEventHandler ConfigurationHasChanged;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the on clear cache command.
        /// </summary>
        public ICommand ClearCacheCommand { get; set; }

        /// <summary>
        ///     Gets or sets the clear credentials.
        /// </summary>
        public ICommand ClearCredentials { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether disable editor tags.
        /// </summary>
        public bool DisableEditorTags { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether extension debug mode enabled.
        /// </summary>
        public bool ExtensionDebugModeEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is connect at start on.
        /// </summary>
        public bool IsConnectAtStartOn { get; set; }

        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets the server address.
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        ///     Gets or sets the status message.
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        ///     Gets or sets the test connection command.
        /// </summary>
        public RelayCommand<object> TestConnectionCommand { get; set; }

        /// <summary>
        ///     Gets or sets the user defined editor.
        /// </summary>
        public string UserDefinedEditor { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        /// <summary>
        /// The on is connect at star on changed.
        /// </summary>
        public void OnIsConnectAtStarOnChanged()
        {
            this.configurationHelper.WriteSetting(
                new SonarQubeProperties
                {
                    Context = Context.GlobalPropsId,
                    Key = "AutoConnectAtStart",
                    Owner = OwnersId.ApplicationOwnerId,
                    Value = this.IsConnectAtStartOn ? "true" : "false"
                });
        }

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="associatedProjectIn">The associated project in.</param>
        public void ReloadDataFromDisk(Resource associatedProjectIn)
        {
            try
            {
                string isConnectAuto = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.IsConnectAtStartOn).Value;
                this.IsConnectAtStartOn = isConnectAuto.Equals("true");
            }
            catch (Exception ex)
            {
                this.notificationManager.ReportMessage(
                    new Message { Id = "GeneralConfigurationViewModel", Data = "ReloadDataFromDisk Failed Read : " + GlobalIds.IsConnectAtStartOn + " : " + ex.Message });
                this.notificationManager.ReportException(ex);
                this.IsConnectAtStartOn = true;
                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.IsConnectAtStartOn, this.IsConnectAtStartOn ? "true" : "false"));
            }

            try
            {
                string editor = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.UserDefinedEditor).Value;
                this.UserDefinedEditor = editor;
            }
            catch (Exception ex)
            {
                this.notificationManager.ReportMessage(
                    new Message { Id = "GeneralConfigurationViewModel", Data = "ReloadDataFromDisk Failed Read : " + GlobalIds.UserDefinedEditor + " : " + ex.Message });
                this.notificationManager.ReportException(ex);
                this.UserDefinedEditor = "notepad";
                this.configurationHelper.WriteSetting(new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.UserDefinedEditor, "notepad"));
            }

            try
            {
                string editorTags = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.DisableEditorTags).Value;
                this.DisableEditorTags = editorTags.ToLower().Equals("true");
            }
            catch (Exception ex)
            {
                this.notificationManager.ReportMessage(
                    new Message { Id = "GeneralConfigurationViewModel", Data = "ReloadDataFromDisk Failed Read : " + GlobalIds.DisableEditorTags + " : " + ex.Message });
                this.notificationManager.ReportException(ex);
                this.DisableEditorTags = false;
                this.configurationHelper.WriteSetting(new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.DisableEditorTags, "false"));
            }

            try
            {
                string debugmode =
                    this.configurationHelper.ReadSetting(
                        Context.GlobalPropsId,
                        OwnersId.ApplicationOwnerId,
                        GlobalIds.ExtensionDebugModeEnabled).Value;
                this.ExtensionDebugModeEnabled = debugmode.ToLower().Equals("true");
            }
            catch (Exception ex)
            {
                this.notificationManager.ReportMessage(
                    new Message { Id = "GeneralConfigurationViewModel", Data = "ReloadDataFromDisk Failed : " + GlobalIds.ExtensionDebugModeEnabled + " : " + ex.Message });
                this.notificationManager.ReportException(ex);
                this.ExtensionDebugModeEnabled = false;
                this.configurationHelper.WriteSetting(
                    new SonarQubeProperties(
                        Context.GlobalPropsId,
                        OwnersId.ApplicationOwnerId,
                        GlobalIds.ExtensionDebugModeEnabled,
                        "false"));
            }
        }

        /// <summary>
        /// The on extension debug mode enabled changed.
        /// </summary>
        public void OnExtensionDebugModeEnabledChanged()
        {
            var prp = new SonarQubeProperties
            {
                Context = Context.GlobalPropsId,
                Owner = OwnersId.ApplicationOwnerId,
                Key = "ExtensionDebugModeEnabled",
                Value = this.ExtensionDebugModeEnabled ? "TRUE" : "FALSE"
            };

            this.configurationHelper.WriteSetting(prp);
        }

        /// <summary>
        ///     The on selected view changed.
        /// </summary>
        public void OnSelectedViewChanged()
        {
            this.OnAnalysisModeHasChange(EventArgs.Empty);
            Debug.WriteLine("Name Changed");
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
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public void UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            this.visualStudioHelper = vsenvironmenthelperIn;
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public void AssociateWithNewProject(Resource project, string workingDir, ISourceControlProvider provider, IIssueTrackerPlugin sourcePlugin)
        {
            this.sourceDir = workingDir;
            this.associatedProject = project;
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>
        /// returns view model
        /// </returns>
        public object GetViewModel()
        {
            return this;
        }

        /// <summary>
        /// Gets the available model, TODO: needs to be removed after viewmodels are split into models and view models
        /// </summary>
        /// <returns>
        /// returns optinal model
        /// </returns>
        public object GetAvailableModel()
        {
            return null;
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void OnSolutionClosed()
        {
            this.sourceDir = string.Empty;
            this.associatedProject = null;
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        public void SaveData()
        {
            this.configurationHelper.WriteSetting(
                new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, "ServerAddress", this.ServerAddress));
            this.configurationHelper.WriteSetting(
                new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.IsConnectAtStartOn, this.IsConnectAtStartOn ? "true" : "false"));
            this.configurationHelper.WriteSetting(
                new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.UserDefinedEditor, this.UserDefinedEditor));
            this.configurationHelper.WriteSetting(
                new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.ExtensionDebugModeEnabled, this.ExtensionDebugModeEnabled ? "true" : "false"));
            this.configurationHelper.WriteSetting(
                new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.DisableEditorTags, this.DisableEditorTags ? "true" : "false"));
        }


        /// <summary>
        /// Called when [server address changed].
        /// </summary>
        public void OnServerAddressChanged()
        {
            if (this.ServerAddress.EndsWith("/"))
            {
                this.ServerAddress = this.ServerAddress.Trim('/');
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        /// <summary>
        /// The on changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected virtual void OnAnalysisModeHasChange(EventArgs e)
        {
            if (this.ConfigurationHasChanged != null)
            {
                this.ConfigurationHasChanged(this, e);
            }
        }

        /// <summary>
        ///     The get credentials.
        /// </summary>
        private void GetCredentials()
        {
            using (var cm = new Credential { Target = "VSSonarQubeExtension" })
            {
                if (!cm.Exists())
                {
                    return;
                }

                cm.Load();

                try
                {
                    string address = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, "ServerAddress").Value;
                    string bootatstart = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.IsConnectAtStartOn).Value;

                    if (address != null &&
                        bootatstart.Equals("true") &&
                        AuthtenticationHelper.EstablishAConnection(this.restService, address, cm.Username, ConnectionConfiguration.ConvertToUnsecureString(cm.SecurePassword)))
                    {
                        this.UserName = cm.Username;
                        this.ServerAddress = address;
                        this.Password = ConnectionConfiguration.ConvertToUnsecureString(cm.SecurePassword);
                    }
                }
                catch (Exception ex)
                {
                    this.notificationManager.ReportMessage(new Message { Id = "GeneralConfigurationViewModel", Data = "Failed To Connect To Server: " + ex.Message });
                    this.notificationManager.ReportException(ex);
                }
            }
        }

        /// <summary>
        ///     The on clear cache command.
        /// </summary>
        private void OnClearCacheCommand()
        {
            this.notificationManager.ClearCache();
        }

        /// <summary>
        ///     The on clear credentials.
        /// </summary>
        private void OnClearCredentials()
        {
            var cm = new Credential { Target = "VSSonarQubeExtension", };
            cm.Delete();
            this.ServerAddress = string.Empty;
            this.Password = string.Empty;
            this.UserName = string.Empty;
        }

        /// <summary>
        /// The on test and save password.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnTestAndSavePassword(object obj)
        {
            if (string.IsNullOrEmpty(this.ServerAddress))
            {
                this.StatusMessage = "Failed: Address not set";
                return;
            }

            var passwordBox = obj as PasswordBox;
            if (passwordBox != null)
            {
                try
                {
                    string password = passwordBox.Password;
                    AuthtenticationHelper.ResetConnection();

                    if (AuthtenticationHelper.EstablishAConnection(this.restService, this.ServerAddress.TrimEnd('/'), this.UserName, password))
                    {
                        this.StatusMessage = "Authenticated";
                        this.SetCredentials(this.UserName, password);
                    }
                    else
                    {
                        this.StatusMessage = "Wrong Credentials";
                    }
                }
                catch (Exception ex)
                {
                    UserExceptionMessageBox.ShowException("Cannot Authenticate", ex);
                }
            }
        }

        /// <summary>
        /// The set credentials.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        private void SetCredentials(string userName, string password)
        {
            this.configurationHelper.WriteSetting(new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, "ServerAddress", this.ServerAddress));
            var cm = new Credential
            {
                Target = "VSSonarQubeExtension",
                PersistanceType = PersistanceType.Enterprise,
                Username = userName,
                SecurePassword = ConnectionConfiguration.ConvertToSecureString(password)
            };
            cm.Save();
        }

        #endregion Methods
    }
}