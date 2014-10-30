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

    using ExtensionTypes;

    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using SonarRestService;

    using VSSonarExtensionUi.View.Helpers;
    using VSSonarExtensionUi.ViewModel.Helpers;

    using VSSonarPlugins;

    /// <summary>
    ///     The sonar configuration view viewModel.
    /// </summary>
    [ImplementPropertyChanged]
    public class GeneralConfigurationViewModel : IViewModelBase, IOptionsViewModelBase
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
            SonarQubeViewModel sonarQubeViewModel)
        {
            this.Header = "General Settings";
            this.viewModel = viewModel;
            this.restService = restService;
            this.visualStudioHelper = helper;
            this.sonarQubeViewModel = sonarQubeViewModel;

            this.ClearCacheCommand = new RelayCommand(this.OnClearCacheCommand);
            this.TestConnectionCommand = new RelayCommand<object>(this.OnTestAndSavePassword);
            this.ClearCredentials = new RelayCommand(this.OnClearCredentials);

            this.BackGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;

            this.GetCredentials();
            this.ResetDefaults();
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
        ///     Gets or sets the user connection config.
        /// </summary>
        public ISonarConfiguration UserConnectionConfig { get; set; }

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
        ///     The apply.
        /// </summary>
        public void Apply()
        {
            this.visualStudioHelper.WriteOptionInApplicationData("VSSonarQubeConfig", "UserDefinedEditor", this.UserDefinedEditor);
            this.visualStudioHelper.WriteOptionInApplicationData("VSSonarQubeConfig", "DisableEditorTags", this.DisableEditorTags ? "TRUE" : "FALSE");

            this.viewModel.Vsenvironmenthelper.WriteOptionInApplicationData(
                "VSSonarQubeConfig", 
                "AutoConnectAtStart", 
                this.IsConnectAtStartOn ? "true" : "false");
        }

        /// <summary>
        ///     The end data association.
        /// </summary>
        public void EndDataAssociation()
        {
        }

        /// <summary>
        ///     The exit.
        /// </summary>
        public void Exit()
        {
            this.GetCredentials();
            this.ResetDefaults();
        }

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
        public void InitDataAssociation(Resource associatedProject, ISonarConfiguration userConnectionConfig, string workingDir)
        {
        }

        /// <summary>
        /// The on extension debug mode enabled changed.
        /// </summary>
        public void OnExtensionDebugModeEnabledChanged()
        {
            this.visualStudioHelper.WriteOptionInApplicationData(
                "VSSonarQubeConfig", 
                "ExtensionDebugModeEnabled", 
                this.ExtensionDebugModeEnabled ? "TRUE" : "FALSE");
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
        ///     The save and close.
        /// </summary>
        public void SaveAndClose()
        {
            this.Apply();
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
            IVSSStatusBar statusBar, 
            IServiceProvider provider)
        {
            this.restService = restServiceIn;
            this.visualStudioHelper = vsenvironmenthelperIn;
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

            string address = this.viewModel.Vsenvironmenthelper.ReadOptionFromApplicationData("VSSonarQubeConfig", "ServerAddress");

            if (string.IsNullOrEmpty(address))
            {
                return;
            }

            this.UserConnectionConfig = new ConnectionConfiguration(
                address, 
                cm.Username, 
                ConnectionConfiguration.ConvertToUnsecureString(cm.SecurePassword));
            this.UserName = cm.Username;
            this.ServerAddress = address;
            this.Password = ConnectionConfiguration.ConvertToUnsecureString(cm.SecurePassword);
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
            var passwordBox = obj as PasswordBox;
            if (passwordBox != null)
            {
                string password = passwordBox.Password;

                this.UserConnectionConfig = new ConnectionConfiguration(this.ServerAddress.TrimEnd('/'), this.UserName, password);

                try
                {
                    if (this.restService.AuthenticateUser(this.UserConnectionConfig))
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
        ///     The reset defaults.
        /// </summary>
        private void ResetDefaults()
        {
            string isConnectAuto = this.viewModel.Vsenvironmenthelper.ReadOptionFromApplicationData("VSSonarQubeConfig", "AutoConnectAtStart");

            if (string.IsNullOrEmpty(isConnectAuto))
            {
                this.IsConnectAtStartOn = true;
                return;
            }

            this.IsConnectAtStartOn = isConnectAuto.Equals("true");

            string editor = this.viewModel.Vsenvironmenthelper.ReadOptionFromApplicationData("VSSonarQubeConfig", "UserDefinedEditor");
            this.UserDefinedEditor = !string.IsNullOrEmpty(editor) ? editor : "notepad";

            string editorTags = this.visualStudioHelper.ReadOptionFromApplicationData("VSSonarQubeConfig", "DisableEditorTags");
            if (!string.IsNullOrEmpty(editorTags))
            {
                this.DisableEditorTags = editorTags.ToLower().Equals("true");
            }

            string debugmode = this.visualStudioHelper.ReadOptionFromApplicationData("VSSonarQubeConfig", "ExtensionDebugModeEnabled");
            if (!string.IsNullOrEmpty(debugmode))
            {
                this.ExtensionDebugModeEnabled = debugmode.ToLower().Equals("true");
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
            this.viewModel.Vsenvironmenthelper.WriteOptionInApplicationData("VSSonarQubeConfig", "ServerAddress", this.ServerAddress);
            var cm = new Credential
                         {
                             Target = "VSSonarQubeExtension", 
                             PersistanceType = PersistanceType.Enterprise, 
                             Username = userName, 
                             SecurePassword = ConnectionConfiguration.ConvertToSecureString(password)
                         };
            cm.Save();
        }

        #endregion
    }
}