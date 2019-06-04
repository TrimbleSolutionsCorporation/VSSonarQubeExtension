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
	using Helpers;
	using Model.Helpers;
	using PropertyChanged;
	using SonarLocalAnalyser;
	using View.Helpers;
	using VSSonarPlugins;
	using VSSonarPlugins.Types;
	using VSSonarExtensionUi.Association;
	using System.Collections.Generic;

	using SonarRestService.Types;
	using SonarRestService;
	using System.Threading.Tasks;

	/// <summary>
	/// The sonar configuration view viewModel.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public class GeneralConfigurationViewModel : IOptionsViewModelBase, IOptionsModelBase
	{
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
			this.StatusMessage = "";
			this.UserName = string.Empty;
			this.Password = string.Empty;
			this.viewModel = viewModel;
			this.restService = restService;
			this.configurationHelper = configurationHelper;
			this.notificationManager = notificationManager;

			this.GetTokenCommand = new VsRelayCommand(this.OnGetTokenCommand);
			this.ClearCacheCommand = new VsRelayCommand(this.OnClearCacheCommand);
			this.TestConnectionCommand = new VsRelayCommand<object>(this.OnTestAndSavePassword);
			this.ClearCredentials = new VsRelayCommand(this.OnClearCredentials);

			this.ConnectToServerCommand = new VsRelayCommand<object>(this.OnConnectToServerCommand);



			this.BackGroundColor = Colors.White;
			this.ForeGroundColor = Colors.Black;

			this.GetCredentials();
			Task.Run(() => this.ReloadDataFromDisk(null));

			// register model
			AssociationModel.RegisterNewModelInPool(this);
			SonarQubeViewModel.RegisterNewViewModelInPool(this);
		}

		/// <summary>
		///     The analysis mode has change.
		/// </summary>
		public event ChangedEventHandler ConfigurationHasChanged;

		/// <summary>
		///     Gets or sets the back ground color.
		/// </summary>
		public Color BackGroundColor { get; set; }

		/// <summary>
		///     Gets or sets the on clear cache command.
		/// </summary>
		public ICommand ClearCacheCommand { get; set; }

		/// <summary>
		/// Gets or sets the get token command.
		/// </summary>
		/// <value>
		/// The get token command.
		/// </value>
		public ICommand GetTokenCommand { get; set; }

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
		public ICommand TestConnectionCommand { get; set; }

		/// <summary>
		/// Gets or sets the connect to server command.
		/// </summary>
		/// <value>
		/// The connect to server command.
		/// </value>
		public ICommand ConnectToServerCommand { get; set; }

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

		/// <summary>
		/// User Login
		/// </summary>
		public string UserLogin { get; set; }

		/// <summary>
		/// Called when [connect to sonar].
		/// </summary>
		/// <param name="configuration">sonar configuration</param>
		public void OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IList<IIssueTrackerPlugin> plugin)
		{
			// does nothing
		}

		/// <summary>
		/// The on is connect at star on changed.
		/// </summary>
		public void OnIsConnectAtStarOnChanged()
		{
			this.configurationHelper.WriteSetting(
				new SonarQubeProperties
				{
					Context = Context.GlobalPropsId.ToString(),
					Key = "AutoConnectAtStart",
					Owner = OwnersId.ApplicationOwnerId,
					Value = this.IsConnectAtStartOn ? "true" : "false"
				});
		}

		/// <summary>
		/// The init data association.
		/// </summary>
		/// <param name="associatedProjectIn">The associated project in.</param>
		public async Task ReloadDataFromDisk(Resource associatedProjectIn)
		{
			await Task.Delay(1);
			var outData = string.Empty;
			this.configurationHelper.ReadInSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.IsConnectAtStartOn, out outData, "true");
			this.IsConnectAtStartOn = bool.Parse(outData);

			this.configurationHelper.ReadInSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.UserDefinedEditor, out outData, "notepad");
			this.UserDefinedEditor = outData;

			this.configurationHelper.ReadInSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.DisableEditorTags, out outData, "false");
			this.DisableEditorTags = bool.Parse(outData);

			this.configurationHelper.ReadInSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.ExtensionDebugModeEnabled, out outData, "false");
			this.ExtensionDebugModeEnabled = bool.Parse(outData);

			// dump configuration to file, when first save
			this.SaveData();
		}

		/// <summary>
		/// The on extension debug mode enabled changed.
		/// </summary>
		public void OnExtensionDebugModeEnabledChanged()
		{
			var prp = new SonarQubeProperties
			{
				Context = Context.GlobalPropsId.ToString(),
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
		/// Called when [disconnect].
		/// </summary>
		public void OnDisconnect()
		{
			// not needed
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
		/// <param name="project">The project.</param>
		/// <param name="workingDir">The working dir.</param>
		/// <param name="provider">The provider.</param>
		/// <param name="profile">The profile.</param>
		public void AssociateWithNewProject(
			Resource project,
			string workingDir,
			ISourceControlProvider provider,
			Dictionary<string, Profile> profile,
			string visualStudioVersion)
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
				new SonarQubeProperties(Context.GlobalPropsId.ToString(), OwnersId.ApplicationOwnerId, GlobalIds.ServerAdress, this.ServerAddress));
			this.configurationHelper.WriteSetting(
				new SonarQubeProperties(Context.GlobalPropsId.ToString(), OwnersId.ApplicationOwnerId, GlobalIds.IsConnectAtStartOn, this.IsConnectAtStartOn ? "true" : "false"));
			this.configurationHelper.WriteSetting(
				new SonarQubeProperties(Context.GlobalPropsId.ToString(), OwnersId.ApplicationOwnerId, GlobalIds.UserDefinedEditor, this.UserDefinedEditor));
			this.configurationHelper.WriteSetting(
				new SonarQubeProperties(Context.GlobalPropsId.ToString(), OwnersId.ApplicationOwnerId, GlobalIds.ExtensionDebugModeEnabled, this.ExtensionDebugModeEnabled ? "true" : "false"));
			this.configurationHelper.WriteSetting(
				new SonarQubeProperties(Context.GlobalPropsId.ToString(), OwnersId.ApplicationOwnerId, GlobalIds.DisableEditorTags, this.DisableEditorTags ? "true" : "false"));
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
				string address = "http://localhost:9000";

				var serverValue = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.ServerAdress);

				if (serverValue != null)
				{
					address = serverValue.Value;
				}

				var userLoginValue = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.UserLogin);
				this.UserLogin = string.Empty;
				if (userLoginValue != null)
				{
					this.UserLogin = userLoginValue.Value;
				}

				string bootatstart = "false";
				var bootatstartValue = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.IsConnectAtStartOn);
				if (bootatstartValue != null)
				{
					bootatstart = bootatstartValue.Value;
				}

				this.UserName = cm.Username;
				this.Password = ConnectionConfiguration.ConvertToUnsecureString(cm.SecurePassword);

				this.ServerAddress = address;

				if (address != null)
				{
					AuthtenticationHelper.EstablishAConnection(this.restService, address, cm.Username, ConnectionConfiguration.ConvertToUnsecureString(cm.SecurePassword));
				}
			}
		}

		/// <summary>
		///     The on clear cache command.
		/// </summary>
		private void OnClearCacheCommand(object data)
		{
			this.notificationManager.ClearCache();
		}

		/// <summary>
		///     The on clear credentials.
		/// </summary>
		private void OnClearCredentials(object data)
		{
			using (var cm = new Credential { Target = "VSSonarQubeExtension", })
			{
				cm.Delete();
				this.ServerAddress = string.Empty;
				this.Password = string.Empty;
				this.UserName = string.Empty;
			}
		}

		/// <summary>
		/// Called when [connect to server command].
		/// </summary>
		/// <param name="obj">The object.</param>
		private void OnConnectToServerCommand(object obj)
		{
			if (!this.StatusMessage.Equals("Authenticated"))
			{
				this.StatusMessage = "Press test connection first";
				return;
			}

			this.viewModel.EstablishANewConnectionToServer();
			this.StatusMessage = "Connected";
		}

		private void OnGetTokenCommand(object obj)
		{
			if (string.IsNullOrEmpty(this.ServerAddress))
			{
				MessageDisplayBox.DisplayMessage("Address not defined, please set adress before trying to generate token");
				return;
			}

			this.visualStudioHelper.NavigateToResource(this.ServerAddress + "/account/security");
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

			if (!this.ServerAddress.StartsWith("http"))
			{
				this.StatusMessage = "Address is malformed, it must start with http or https";
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
						this.SaveUserConfigurationData(this.UserName, password);
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
		private void SaveUserConfigurationData(string userName, string password)
		{
			this.configurationHelper.WriteSetting(new SonarQubeProperties(Context.GlobalPropsId.ToString(), OwnersId.ApplicationOwnerId, GlobalIds.ServerAdress, this.ServerAddress));
			this.configurationHelper.WriteSetting(new SonarQubeProperties(Context.GlobalPropsId.ToString(), OwnersId.ApplicationOwnerId, GlobalIds.UserLogin, this.UserLogin));
			using (var cm = new Credential
			{
				Target = "VSSonarQubeExtension",
				PersistanceType = PersistanceType.Enterprise,
				Username = userName,
				SecurePassword = ConnectionConfiguration.ConvertToSecureString(password)
			})
			{
				cm.Save();
			}
		}
	}
}