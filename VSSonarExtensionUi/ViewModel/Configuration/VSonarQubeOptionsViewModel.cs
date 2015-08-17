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

    using PropertyChanged;
    using Helpers;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using Model.PluginManager;
    using Model.Helpers;
    using Model.Configuration;

    /// <summary>
    ///     The plugins options model.
    /// </summary>
    [ImplementPropertyChanged]
    public class VSonarQubeOptionsViewModel : IOptionsViewModelBase
    {
        #region Constructors and Destructors

        /// <summary>Initializes a new instance of the <see cref="VSonarQubeOptionsViewModel"/> class.</summary>
        /// <param name="model">The model.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        public VSonarQubeOptionsViewModel(
            SonarQubeViewModel model, 
            IConfigurationHelper configurationHelper)
        {
            this.model = model;
            this.ConfigurationHelper = configurationHelper;

            this.InitModels();
            this.InitCommanding();
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The request close.
        /// </summary>
        public event Action<object, object> RequestClose;

        #endregion

        #region Fields

        /// <summary>
        ///     The model.
        /// </summary>
        private readonly SonarQubeViewModel model;

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
        ///     Gets the available plugins collection.
        /// </summary>
        public ObservableCollection<IOptionsViewModelBase> AvailableOptionsViews { get; set; }

        /// <summary>
        ///     Gets the available plugins collection.
        /// </summary>
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
        ///     The plugin manager.
        /// </summary>
        public PluginManagerModel PluginManager { get; set; }

        /// <summary>
        ///     Gets or sets the plugins manager options frame.
        /// </summary>
        public UserControl PluginsManagerOptionsFrame { get; set; }

        /// <summary>
        ///     The project.
        /// </summary>
        private Resource Project { get; set; }

        /// <summary>
        ///     Gets or sets the save and exit command.
        /// </summary>
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
        ///     Gets or sets the vsenvironmenthelper.
        /// </summary>
        public IVsEnvironmentHelper Vsenvironmenthelper { get; set; }

        /// <summary>Gets or sets the configuration helper.</summary>
        public IConfigurationHelper ConfigurationHelper { get; set; }
        public RoslynManagerModel RoslynModel { get; private set; }
        internal RoslynManagerViewModel RoslynViewModel { get; private set; }

        /// <summary>The update services.</summary>
        /// <param name="restServiceIn">The rest service in.</param>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public void UpdateServices(
            ISonarRestService restServiceIn, 
            IVsEnvironmentHelper vsenvironmenthelperIn, 
            IVSSStatusBar statusBar, 
            IServiceProvider provider)
        {
            this.Vsenvironmenthelper = vsenvironmenthelperIn;
        }

        /// <summary>The refresh general properties.</summary>
        /// <param name="selectedProject"></param>
        public void RefreshPropertiesInView(Resource selectedProject)
        {
            this.Project = selectedProject;
            foreach (var availableOption in this.AvailableOptionsModels)
            {
                availableOption.RefreshPropertiesInView(selectedProject);
            }
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
        public void EndDataAssociation()
        {
            this.Project = null;
        }

        /// <summary>The update colours.</summary>
        /// <param name="background">The background.</param>
        /// <param name="foreground">The foreground.</param>
        public void UpdateColours(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        #endregion

        #region Methods

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
                this.ConfigurationHelper.DeleteSettingsFile();
                this.Vsenvironmenthelper.RestartVisualStudio();
            }
        }

        /// <summary>The init models.</summary>
        private void InitModels()
        {
            this.AvailableOptionsViews = new ObservableCollection<IOptionsViewModelBase>();
            this.AvailableOptionsModels = new ObservableCollection<IOptionsModelBase>();

            this.GeneralConfigurationViewModel = new GeneralConfigurationViewModel(
                this, 
                this.model.SonarRestConnector, 
                this.Vsenvironmenthelper, 
                this.model, 
                this.ConfigurationHelper);
            this.AnalysisOptionsViewModel = new AnalysisOptionsViewModel(
                this.Vsenvironmenthelper, 
                this, 
                this.ConfigurationHelper);

            this.AvailableOptionsViews.Add(this.GeneralConfigurationViewModel);
            this.AvailableOptionsViews.Add(this.AnalysisOptionsViewModel);

            this.AvailableOptionsModels.Add(this.GeneralConfigurationViewModel);
            this.AvailableOptionsModels.Add(this.AnalysisOptionsViewModel);

            this.SelectedOption = this.GeneralConfigurationViewModel;
        }

        /// <summary>The init pugin system.</summary>
        /// <param name="helper">The helper.</param>
        public void InitPuginSystem(IVsEnvironmentHelper helper, PluginController plugincontroller, INotificationManager manager)
        {
            this.PluginManager = new PluginManagerModel(
                plugincontroller,
                AuthtenticationHelper.AuthToken,
                this.ConfigurationHelper,
                this,
                this.model,
                manager,
                helper);
            this.LicenseManager = new LicenseViewerViewModel(this.PluginManager, this.ConfigurationHelper);

            this.RoslynModel = new RoslynManagerModel(this.PluginManager.AnalysisPlugins, manager, this.ConfigurationHelper);
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
        ///     The on apply command.
        /// </summary>
        private void OnApplyCommand()
        {
            this.ConfigurationHelper.SyncSettings();
        }

        /// <summary>
        ///     The on cancel and exit command.
        /// </summary>
        private void OnCancelAndExitCommand()
        {
            this.ConfigurationHelper.ClearNonSavedSettings();
            this.OnRequestClose(this, "Exit");
        }

        /// <summary>
        ///     The on save and exit command.
        /// </summary>
        private void OnSaveAndExitCommand()
        {
            foreach (var option in this.AvailableOptionsModels)
            {
                option.SaveCurrentViewToDisk(this.ConfigurationHelper);
            }

            this.ConfigurationHelper.SyncSettings();
            this.OnRequestClose(this, "Exit");
        }

        public object GetAvailableModel()
        {
            return null;
        }

        #endregion

    }
}