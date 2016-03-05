// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSonarQubeOptionsViewModel.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2014 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The plugins options model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.ViewModel.Configuration
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    using GalaSoft.MvvmLight.Command;
    using Helpers;

    using Model.Configuration;
    using Model.Helpers;
    using Model.PluginManager;
    using PropertyChanged;

    using SonarLocalAnalyser;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using System.Diagnostics;
    using VSSonarExtensionUi.Association;
    using System.Collections.Generic;


    /// <summary>
    ///     The plugins options model.
    /// </summary>
    [ImplementPropertyChanged]
    public class VSonarQubeOptionsViewModel : IOptionsViewModelBase, IModelBase
    {
        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        /// The rest service
        /// </summary>
        private readonly ISonarRestService restService;

        /// <summary>
        /// The notification manager
        /// </summary>
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// The vsenvironmenthelper
        /// </summary>
        private IVsEnvironmentHelper vsenvironmenthelper;

        /// <summary>
        /// The project
        /// </summary>
        private Resource project;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VSonarQubeOptionsViewModel" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="notifier">The notifier.</param>
        public VSonarQubeOptionsViewModel(
            ISonarRestService service,
            IConfigurationHelper configurationHelper,
            INotificationManager notifier)
        {
            this.notificationManager = notifier;
            this.restService = service;
            this.configurationHelper = configurationHelper;

            this.InitModels();
            this.InitCommanding();

            AssociationModel.RegisterNewModelInPool(this);
            SonarQubeViewModel.RegisterNewViewModelInPool(this);
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The request close.
        /// </summary>
        public event Action<object, object> RequestClose;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the analysisPlugin in view.
        /// </summary>
        public IAnalysisPlugin AnalysisPluginInView { get; set; }

        /// <summary>
        ///     Gets or sets the apply command.
        /// </summary>
        public RelayCommand ApplyCommand { get; set; }

        /// <summary>Gets or sets the reset all changes command.</summary>
        public ICommand ResetAllChangesCommand { get; set; }

        /// <summary>
        /// Gets or sets the available options views.
        /// </summary>
        /// <value>
        /// The available options views.
        /// </value>
        public ObservableCollection<IOptionsViewModelBase> AvailableOptionsViews { get; set; }

        /// <summary>
        /// Gets or sets the available options models.
        /// </summary>
        /// <value>
        /// The available options models.
        /// </value>
        public ObservableCollection<IOptionsModelBase> AvailableOptionsModels { get; set; }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the cancel and exit command.
        /// </summary>
        public RelayCommand CancelAndExitCommand { get; set; }

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the general options.
        /// </summary>
        public AnalysisOptionsViewModel AnalysisOptionsViewModel { get; set; }

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        ///     Gets or sets the license manager.
        /// </summary>
        public LicenseViewerViewModel LicenseManager { get; set; }

        /// <summary>
        ///     Gets or sets the options in view.
        /// </summary>
        public UserControl OptionsInView { get; set; }

        /// <summary>
        /// Gets or sets the plugin manager.
        /// </summary>
        /// <value>
        /// The plugin manager.
        /// </value>
        public PluginManagerModel PluginManager { get; set; }

        /// <summary>
        ///     Gets or sets the plugins manager options frame.
        /// </summary>
        public UserControl PluginsManagerOptionsFrame { get; set; }

        /// <summary>
        /// Gets or sets the save and exit command.
        /// </summary>
        /// <value>
        /// The save and exit command.
        /// </value>
        public RelayCommand SaveAndExitCommand { get; set; }

        /// <summary>
        ///     Gets or sets the selected analysisPlugin item.
        /// </summary>
        public IViewModelBase SelectedOption { get; set; }

        /// <summary>
        ///     Gets or sets the sonar configuration view model.
        /// </summary>
        public GeneralConfigurationViewModel GeneralConfigurationViewModel { get; set; }

        /// <summary>
        /// Gets the roslyn model.
        /// </summary>
        /// <value>
        /// The roslyn model.
        /// </value>
        public RoslynManagerModel RoslynModel { get; private set; }

        /// <summary>
        /// Gets the roslyn view model.
        /// </summary>
        /// <value>
        /// The roslyn view model.
        /// </value>
        internal RoslynManagerViewModel RoslynViewModel { get; private set; }

        /// <summary>
        /// The update services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public void UpdateServices(
            IVsEnvironmentHelper vsenvironmenthelperIn, 
            IVSSStatusBar statusBar, 
            IServiceProvider provider)
        {
            this.vsenvironmenthelper = vsenvironmenthelperIn;
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
        ///     The reset user data.
        /// </summary>
        public void ResetUserData()
        {
            if (this.PluginManager == null)
            {
                return;
            }

            foreach (var plugin in this.PluginManager.AnalysisPlugins)
            {
                plugin.ResetDefaults();
            }

            foreach (var plugin in this.PluginManager.MenuPlugins)
            {
                plugin.ResetDefaults();
            }
        }

        /// <summary>
        ///     The end data association.
        /// </summary>
        public void OnSolutionClosed()
        {
            this.project = null;
        }

        /// <summary>The update colours.</summary>
        /// <param name="background">The background.</param>
        /// <param name="foreground">The foreground.</param>
        public void UpdateColours(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        /// <summary>
        /// The init pugin system.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="plugincontroller">The plugincontroller.</param>
        /// <param name="manager">The manager.</param>
        public void InitPuginSystem(IVsEnvironmentHelper helper, PluginController plugincontroller, INotificationManager manager)
        {
            this.PluginManager = new PluginManagerModel(
                plugincontroller,
                this.configurationHelper,
                manager,
                helper);

            this.LicenseManager = new LicenseViewerViewModel(this.PluginManager, this.configurationHelper);

            this.RoslynModel = new RoslynManagerModel(this.PluginManager.AnalysisPlugins, manager, this.configurationHelper, this.restService);
            this.RoslynViewModel = new RoslynManagerViewModel(this.RoslynModel);

            this.AvailableOptionsViews.Add(this.PluginManager);
            this.AvailableOptionsViews.Add(this.LicenseManager);
            this.AvailableOptionsViews.Add(this.RoslynViewModel);

            this.AvailableOptionsModels.Add(this.PluginManager);
            this.AvailableOptionsModels.Add(this.LicenseManager);
            this.AvailableOptionsModels.Add(this.RoslynModel);

            // sync checks to plugins
            this.RoslynModel.SyncDiagnosticsInPlugins();
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public void OnDisconnect()
        {
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
        /// Associates the with new project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="projectIn">The project in.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="provider">The provider.</param>
        public void AssociateWithNewProject(Resource projectIn, string workingDir, ISourceControlProvider provider, Dictionary<string, Profile> profile)
        {
            this.project = projectIn;

            foreach (var availableOption in this.AvailableOptionsModels)
            {
                availableOption.ReloadDataFromDisk(projectIn);
            }
        }

        #endregion

        #region Methods


        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        /// <param name="availableProjects">The available projects.</param>
        public void OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IIssueTrackerPlugin sourcePlugin)
        {
            if (this.PluginManager != null)
            {
                this.PluginManager.OnConnectToSonar(configuration, availableProjects, sourcePlugin);
            }
                        
            this.RefreshDiagnostics();
        }

        /// <summary>
        /// Refreshes the diagnostics.
        /// </summary>
        public void RefreshDiagnostics()
        {
            if (this.RoslynViewModel == null)
            {
                return;
            }

            this.RoslynViewModel.SyncDiagInView();
        }

        /// <summary>The update theme.</summary>
        /// <param name="background">The background.</param>
        /// <param name="foreground">The foreground.</param>
        internal void UpdateTheme(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;

            foreach (IViewModelBase view in this.AvailableOptionsViews)
            {
                view.UpdateColours(background, foreground);
            }
        }

        /// <summary>The on request close.</summary>
        /// <param name="arg1">The arg 1.</param>
        /// <param name="arg2">The arg 2.</param>
        protected void OnRequestClose(object arg1, object arg2)
        {
            var handler = this.RequestClose;
            if (handler != null)
            {
                handler(arg1, arg2);
            }
        }

        /// <summary>
        ///     The init commanding.
        /// </summary>
        private void InitCommanding()
        {
            this.SaveAndExitCommand = new RelayCommand(this.OnSaveAndExitCommand);

            this.ApplyCommand = new RelayCommand(this.OnApplyCommand);
            this.ResetAllChangesCommand = new RelayCommand(this.OnResetAllChangesCommand);
            this.CancelAndExitCommand = new RelayCommand(this.OnCancelAndExitCommand);
        }

        /// <summary>The on reset all changes command.</summary>
        private void OnResetAllChangesCommand()
        {
            var result = MessageBox.Show(
                "This will delete all saved settings, VS will restart after the reset. Are  you sure? ", 
                "Reset saved settings", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                this.configurationHelper.DeleteSettingsFile();
                this.vsenvironmenthelper.RestartVisualStudio();
            }
        }

        /// <summary>The init models.</summary>
        private void InitModels()
        {
            this.AvailableOptionsViews = new ObservableCollection<IOptionsViewModelBase>();
            this.AvailableOptionsModels = new ObservableCollection<IOptionsModelBase>();

            this.GeneralConfigurationViewModel = new GeneralConfigurationViewModel(
                this, 
                this.restService, 
                this.configurationHelper,
                this.notificationManager);
            this.AnalysisOptionsViewModel = new AnalysisOptionsViewModel(
                this, 
                this.configurationHelper);

            this.AvailableOptionsViews.Add(this.GeneralConfigurationViewModel);
            this.AvailableOptionsViews.Add(this.AnalysisOptionsViewModel);

            this.AvailableOptionsModels.Add(this.GeneralConfigurationViewModel);
            this.AvailableOptionsModels.Add(this.AnalysisOptionsViewModel);

            this.SelectedOption = this.GeneralConfigurationViewModel;
        }

        /// <summary>
        ///     The on apply command.
        /// </summary>
        private void OnApplyCommand()
        {
            this.configurationHelper.SyncSettings();
        }

        /// <summary>
        ///     The on cancel and exit command.
        /// </summary>
        private void OnCancelAndExitCommand()
        {
            this.configurationHelper.ClearNonSavedSettings();
            this.OnRequestClose(this, "Exit");
        }

        /// <summary>
        ///     The on save and exit command.
        /// </summary>
        private void OnSaveAndExitCommand()
        {
            foreach (var option in this.AvailableOptionsModels)
            {
                try
                {
                    option.SaveData();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            this.configurationHelper.SyncSettings();
            this.OnRequestClose(this, "Exit");
        }

        /// <summary>
        /// Establishes a new connection to server.
        /// </summary>
        internal void EstablishANewConnectionToServer()
        {
            this.notificationManager.ResetAndEstablishANewConnectionToServer();
        }

        #endregion
    }
}