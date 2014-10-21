// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarConfigurationViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   The sonar configuration view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtensionUi.ViewModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Windows.Controls;
    using System.Windows.Media;

    using CredentialManagement;

    using ExtensionTypes;

    using GalaSoft.MvvmLight.Command;

    using SonarRestService;

    using VSSonarExtensionUi.View;

    using VSSonarPlugins;

    /// <summary>
    /// The sonar configuration view model.
    /// </summary>
    public class SonarConfigurationViewModel : IViewModelBase, IOptionsViewModelBase, INotifyPropertyChanged
    {
        #region Fields

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The model.
        /// </summary>
        private readonly ExtensionOptionsModel model;

        /// <summary>
        /// The rest service.
        /// </summary>
        private readonly ISonarRestService restService;

        private readonly IVsEnvironmentHelper vsHelper;

        public void OnDisableEditorTagsChanged()
        {
            this.vsHelper.WriteOptionInApplicationData("SonarOptionsGeneral", "DisableEditorTags", this.DisableEditorTags ? "TRUE" : "FALSE");
        }

        /// <summary>
        /// Gets or sets a value indicating whether disable editor tags.
        /// </summary>
        public bool DisableEditorTags { get; set; }

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarConfigurationViewModel"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="restService">
        /// The rest service.
        /// </param>
        public SonarConfigurationViewModel(ExtensionOptionsModel model, ISonarRestService restService, IVsEnvironmentHelper vsHelper)
        {
            this.Header = "Connection";
            this.model = model;
            this.restService = restService;
            this.vsHelper = vsHelper;

            this.TestConnectionCommand = new RelayCommand<object>(this.OnTestAndSavePassword);

            this.BackGroundColor = Colors.Black;
            this.ForeGroundColor = Colors.White;

            this.GetCredentials();
            this.ResetDefaults();
        }

        /// <summary>
        /// The reset defaults.
        /// </summary>
        private void ResetDefaults()
        {
            var isConnectAuto = this.model.Vsenvironmenthelper.ReadOptionFromApplicationData("VSSonarQubeConfig", "AutoConnectAtStart");

            if (string.IsNullOrEmpty(isConnectAuto))
            {
                this.IsConnectAtStartOn = true;
                return;
            }

            this.IsConnectAtStartOn = isConnectAuto.Equals("true");
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        public void RefreshDataForResource(Resource fullName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is connect at start on.
        /// </summary>
        public bool IsConnectAtStartOn { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the server address.
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Gets or sets the test connection command.
        /// </summary>
        public RelayCommand<object> TestConnectionCommand { get; set; }

        /// <summary>
        /// Gets or sets the user connection config.
        /// </summary>
        public ISonarConfiguration UserConnectionConfig { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The apply.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Apply()
        {
            if (this.IsConnectAtStartOn)
            {
                this.model.Vsenvironmenthelper.WriteOptionInApplicationData("VSSonarQubeConfig", "AutoConnectAtStart", "true");
            }
            else
            {
                this.model.Vsenvironmenthelper.WriteOptionInApplicationData("VSSonarQubeConfig", "AutoConnectAtStart", "false");
            }
        }

        /// <summary>
        /// The exit.
        /// </summary>
        public void Exit()
        {
            this.GetCredentials();
            this.ResetDefaults();
        }

        /// <summary>
        /// The save and close.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
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

        #endregion

        #region Methods

        /// <summary>
        /// The get credentials.
        /// </summary>
        private void GetCredentials()
        {
            var cm = new Credential { Target = "VSSonarQubeExtension", };

            if (!cm.Exists())
            {
                return;
            }

            cm.Load();

            string address = this.model.Vsenvironmenthelper.ReadOptionFromApplicationData("VSSonarQubeConfig", "ServerAddress");

            if (string.IsNullOrEmpty(address))
            {
                return;
            }

            this.UserConnectionConfig = new ConnectionConfiguration(address, cm.Username, ConnectionConfiguration.ConvertToUnsecureString(cm.SecurePassword));
            this.UserName = cm.Username;
            this.ServerAddress = address;
            this.Password = ConnectionConfiguration.ConvertToUnsecureString(cm.SecurePassword);
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
            string password = passwordBox.Password;

            this.UserConnectionConfig = new ConnectionConfiguration(
                this.ServerAddress, 
                this.UserName, 
                password);

            try
            {
                if (this.restService.AuthenticateUser(this.UserConnectionConfig))
                {
                    this.StatusMessage = "Authenticated";
                    this.SetCredentials(this.UserName, password);
                }
                else
                {
                    UserExceptionMessageBox.ShowException("Cannot Authenticate", null);
                }
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Cannot Authenticate", ex);
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
            this.model.Vsenvironmenthelper.WriteOptionInApplicationData("VSSonarQubeConfig", "ServerAddress", this.ServerAddress);
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