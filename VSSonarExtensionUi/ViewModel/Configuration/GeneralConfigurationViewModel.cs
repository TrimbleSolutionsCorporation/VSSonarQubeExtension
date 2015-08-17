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

    using PropertyChanged;

    using View.Helpers;
    using Helpers;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using Model.Helpers;

    /// <summary>
    ///     The sonar configuration view viewModel.
    /// </summary>
    [ImplementPropertyChanged]
    public class GeneralConfigurationViewModel : IOptionsViewModelBase, IOptionsModelBase
    {
        #region Fields

        /// <summary>
        ///     The sq view model.
        /// </summary>
        private readonly SonarQubeViewModel sonarQubeViewModel;

        /// <summary>
        ///     The viewModel.
        /// </summary>
        private readonly VSonarQubeOptionsViewModel viewModel;

        /// <summary>
        ///     The rest service.
        /// </summary>
        private ISonarRestService restService;

        /// <summary>
        ///     The visual studio helper.
        /// </summary>
        private IVsEnvironmentHelper visualStudioHelper;

        private IConfigurationHelper configurationHelper;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralConfigurationViewModel"/> class.
        /// </summary>
        /// <param name="viewModel">
        /// The view model.
        /// </param>
        /// <param name="restService">
        /// The rest service.
        /// </param>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="sonarQubeViewModel">
        /// The sonar qube view model.
        /// </param>
        public GeneralConfigurationViewModel(
            VSonarQubeOptionsViewModel viewModel, 
            ISonarRestService restService, 
            IVsEnvironmentHelper helper, 
            SonarQubeViewModel sonarQubeViewModel,
            IConfigurationHelper configurationHelper)
        {
            this.Header = "General Settings";
            this.UserName = "";
            this.Password = "";
            this.viewModel = viewModel;
            this.restService = restService;
            this.configurationHelper = configurationHelper;
            this.visualStudioHelper = helper;
            this.sonarQubeViewModel = sonarQubeViewModel;

            this.ClearCacheCommand = new RelayCommand(this.OnClearCacheCommand);
            this.TestConnectionCommand = new RelayCommand<object>(this.OnTestAndSavePassword);
            this.ClearCredentials = new RelayCommand(this.OnClearCredentials);

            this.BackGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;

            this.GetCredentials();
            this.RefreshPropertiesInView(null);
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The analysis mode has change.
        /// </summary>
        public event ChangedEventHandler ConfigurationHasChanged;

        #endregion

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
        ///     Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="associatedProject">
        /// The associated project.
        /// </param>
        /// <param name="userConnectionConfig">
        /// The user connection config.
        /// </param>
        /// <param name="workingDir">
        /// The working dir.
        /// </param>
        public void RefreshPropertiesInView(Resource associatedProject)
        {
            try
            {
                string isConnectAuto = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.IsConnectAtStartOn).Value;
                this.IsConnectAtStartOn = isConnectAuto.Equals("true");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                this.IsConnectAtStartOn = true;
                this.configurationHelper.WriteSetting(new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.IsConnectAtStartOn, this.IsConnectAtStartOn ? "true" : "false"));
            }

            try
            {
                string editor = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.UserDefinedEditor).Value;
                this.UserDefinedEditor = editor;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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
                Debug.WriteLine(ex.Message);
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
                Debug.WriteLine(ex.Message);
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
        /// The update services.
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
        public void UpdateServices(
            ISonarRestService restServiceIn, 
            IVsEnvironmentHelper vsenvironmenthelperIn, 
            IConfigurationHelper  configurationHelper,
            IVSSStatusBar statusBar, 
            IServiceProvider provider)
        {
            this.restService = restServiceIn;
            this.visualStudioHelper = vsenvironmenthelperIn;
            this.configurationHelper = configurationHelper;
        }

        public void SaveCurrentViewToDisk(IConfigurationHelper configurationHelper)
        {
            this.configurationHelper.WriteSetting(new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, "ServerAddress", this.ServerAddress));
            this.configurationHelper.WriteSetting(new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.IsConnectAtStartOn, this.IsConnectAtStartOn ? "true" : "false"));
            this.configurationHelper.WriteSetting(new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.UserDefinedEditor, this.UserDefinedEditor));
            this.configurationHelper.WriteSetting(new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.ExtensionDebugModeEnabled, this.ExtensionDebugModeEnabled ? "true" : "false"));
            this.configurationHelper.WriteSetting(new SonarQubeProperties(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.DisableEditorTags, this.DisableEditorTags ? "true" : "false"));
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
            var cm = new Credential { Target = "VSSonarQubeExtension", };

            if (!cm.Exists())
            {
                return;
            }

            cm.Load();

            try
            {
                string address = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, "ServerAddress").Value;
                string bootatstart = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.IsConnectAtStartOn).Value;

                if (address != null && bootatstart.Equals("true"))
                {
                    if (AuthtenticationHelper.EstablishAConnection(this.restService, address, cm.Username, ConnectionConfiguration.ConvertToUnsecureString(cm.SecurePassword)))
                    {
                        this.UserName = cm.Username;
                        this.ServerAddress = address;
                        this.Password = ConnectionConfiguration.ConvertToUnsecureString(cm.SecurePassword); 
                    }
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
            this.sonarQubeViewModel.ServerViewModel.ClearCache();
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
            if (String.IsNullOrEmpty(this.ServerAddress))
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

        public object GetAvailableModel()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}