// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSonarQubeOptionsViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   The plugins options model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.ViewModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows.Controls;
    using System.Windows.Media;

    using ExtensionHelpers;

    using ExtensionTypes;

    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using VSSonarPlugins;

    /// <summary>
    ///     The plugins options model.
    /// </summary>
    [ImplementPropertyChanged]
    public class VSonarQubeOptionsViewModel : IViewModelBase
    {
        #region Fields

        /// <summary>
        ///     The model.
        /// </summary>
        private readonly SonarQubeViewModel model;

        /// <summary>
        ///     The plugincontroller.
        /// </summary>
        private readonly PluginController plugincontroller;

        /// <summary>
        ///     The plugins.
        /// </summary>
        private readonly List<IAnalysisPlugin> plugins;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VSonarQubeOptionsViewModel"/> class.
        /// </summary>
        public VSonarQubeOptionsViewModel()
        {
            this.ForeGroundColor = Colors.White;
            this.BackGroundColor = Colors.Black;

            this.InitModels();
            this.InitCommanding();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VSonarQubeOptionsViewModel"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public VSonarQubeOptionsViewModel(SonarQubeViewModel model)
        {
            this.model = model;

            this.InitModels();
            this.InitCommanding();

            this.ForeGroundColor = Colors.White;
            this.BackGroundColor = Colors.Black;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VSonarQubeOptionsViewModel"/> class.
        /// </summary>
        /// <param name="plugincontroller">
        /// The plugincontroller.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="vsHelper">
        /// The vs helper.
        /// </param>
        public VSonarQubeOptionsViewModel(PluginController plugincontroller, SonarQubeViewModel model, IVsEnvironmentHelper vsHelper)
        {
            this.model = model;
            this.plugincontroller = plugincontroller;
            this.plugins = plugincontroller.GetPlugins();
            this.Vsenvironmenthelper = vsHelper;

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

        #region Public Properties

        /// <summary>
        ///     Gets or sets the analysisPlugin in view.
        /// </summary>
        public IAnalysisPlugin AnalysisPluginInView { get; set; }

        /// <summary>
        ///     Gets or sets the apply command.
        /// </summary>
        public RelayCommand ApplyCommand { get; set; }

        /// <summary>
        ///     Gets the available plugins collection.
        /// </summary>
        public ObservableCollection<IOptionsViewModelBase> AvailableOptions { get; set; }

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
        public GeneralOptionsModel GeneralOptions { get; set; }

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
        public Resource Project { get; set; }

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
        public SonarConfigurationViewModel SonarConfigurationViewModel { get; set; }

        /// <summary>
        ///     Gets or sets the vsenvironmenthelper.
        /// </summary>
        public IVsEnvironmentHelper Vsenvironmenthelper { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The refresh general properties.
        /// </summary>
        public void RefreshGeneralProperties()
        {
            this.ResetGeneralOptionsForProject();
        }

        /// <summary>
        ///     The refresh properties in plugins.
        /// </summary>
        public void RefreshPropertiesInPlugins()
        {
            this.UpdatePluginOptions(string.Empty);
        }

        /// <summary>
        ///     The reset user data.
        /// </summary>
        public void ResetUserData()
        {
            bool fileIsMissing = File.Exists(this.Vsenvironmenthelper.UserAppDataConfigurationFile());
            if (this.plugins == null)
            {
                return;
            }

            foreach (IAnalysisPlugin plugin in this.plugins)
            {
                if (plugin.GetPluginControlOptions(this.SonarConfigurationViewModel.UserConnectionConfig) == null)
                {
                    continue;
                }

                if (!fileIsMissing
                    || this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(
                        plugin.GetKey(this.SonarConfigurationViewModel.UserConnectionConfig)).Count == 0)
                {
                    plugin.GetPluginControlOptions(this.SonarConfigurationViewModel.UserConnectionConfig).ResetDefaults();

                    string pluginKey = plugin.GetKey(this.SonarConfigurationViewModel.UserConnectionConfig);
                    Dictionary<string, string> optionsToSave =
                        plugin.GetPluginControlOptions(this.SonarConfigurationViewModel.UserConnectionConfig).GetOptions();
                    this.Vsenvironmenthelper.WriteAllOptionsForPluginOptionInApplicationData(pluginKey, null, optionsToSave);
                }
            }
        }

        /// <summary>
        ///     The sync options to file.
        /// </summary>
        public void SyncOptionsToFile()
        {
            this.model.VsHelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.RunnerExecutableKey, 
                this.GeneralOptions.SonarQubeBinary);
            this.model.VsHelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.JavaExecutableKey, this.GeneralOptions.JavaBinary);
            this.model.VsHelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.LocalAnalysisTimeoutKey, 
                this.GeneralOptions.TimeoutValue.ToString(CultureInfo.InvariantCulture));
            this.model.VsHelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.IsDebugAnalysisOnKey, 
                this.GeneralOptions.DebugIsChecked.ToString());
            this.model.VsHelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.ExcludedPluginsKey, 
                this.GeneralOptions.ExcludedPlugins);

            foreach (PluginDescription plugin in this.PluginManager.PluginList)
            {
                this.model.VsHelper.WriteOptionInApplicationData(GlobalIds.PluginEnabledControlId, plugin.Name, plugin.Enabled.ToString());
            }

            if (this.Project != null)
            {
                string solId = VsSonarUtils.SolutionGlobPropKey(this.Project.Key);
                this.model.VsHelper.WriteOptionInApplicationData(solId, GlobalIds.SonarSourceKey, this.GeneralOptions.SourceDir);
                this.model.VsHelper.WriteOptionInApplicationData(solId, GlobalIds.SourceEncodingKey, this.GeneralOptions.SourceEncoding);
            }

            if (this.AnalysisPluginInView != null)
            {
                Dictionary<string, string> options =
                    this.AnalysisPluginInView.GetPluginControlOptions(this.SonarConfigurationViewModel.UserConnectionConfig).GetOptions();
                string key = this.AnalysisPluginInView.GetKey(this.SonarConfigurationViewModel.UserConnectionConfig);
                this.model.VsHelper.WriteAllOptionsForPluginOptionInApplicationData(key, this.Project, options);
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

        #endregion

        #region Methods

        /// <summary>
        /// The update theme.
        /// </summary>
        /// <param name="background">
        /// The background.
        /// </param>
        /// <param name="foreground">
        /// The foreground.
        /// </param>
        internal void UpdateTheme(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;

            foreach (IViewModelBase view in this.AvailableOptions)
            {
                view.UpdateColours(background, foreground);
            }
        }

        /// <summary>
        /// The on request close.
        /// </summary>
        /// <param name="arg1">
        /// The arg 1.
        /// </param>
        /// <param name="arg2">
        /// The arg 2.
        /// </param>
        protected void OnRequestClose(object arg1, object arg2)
        {
            Action<object, object> handler = this.RequestClose;
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

            this.CancelAndExitCommand = new RelayCommand(this.OnCancelAndExitCommand);
        }

        /// <summary>
        ///     The init models.
        /// </summary>
        private void InitModels()
        {
            this.AvailableOptions = new ObservableCollection<IOptionsViewModelBase>();

            this.SonarConfigurationViewModel = new SonarConfigurationViewModel(this, this.model.SonarRestConnector, this.Vsenvironmenthelper);
            this.GeneralOptions = new GeneralOptionsModel(this.Vsenvironmenthelper, this);
            this.PluginManager = new PluginManagerModel(
                this.plugincontroller, 
                this.SonarConfigurationViewModel.UserConnectionConfig, 
                this.Vsenvironmenthelper, 
                this);
            this.LicenseManager = new LicenseViewerViewModel();

            this.AvailableOptions.Add(this.SonarConfigurationViewModel);
            this.AvailableOptions.Add(this.GeneralOptions);
            this.AvailableOptions.Add(this.PluginManager);
            this.AvailableOptions.Add(this.LicenseManager);

            foreach (var availableOption in this.AvailableOptions)
            {
                try
                {
                    availableOption.Exit();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            this.SelectedOption = this.SonarConfigurationViewModel;
        }

        /// <summary>
        ///     The on apply command.
        /// </summary>
        private void OnApplyCommand()
        {
            foreach (IViewModelBase availableOption in this.AvailableOptions)
            {
                var option = availableOption as IOptionsViewModelBase;
                if (option != null)
                {
                    option.Apply();
                }
            }
        }

        /// <summary>
        ///     The on cancel and exit command.
        /// </summary>
        private void OnCancelAndExitCommand()
        {
            foreach (IViewModelBase availableOption in this.AvailableOptions)
            {
                var option = availableOption as IOptionsViewModelBase;
                if (option != null)
                {
                    option.Exit();
                }
            }

            this.OnRequestClose(this, "Exit");
        }

        /// <summary>
        ///     The on save and exit command.
        /// </summary>
        private void OnSaveAndExitCommand()
        {
            foreach (IViewModelBase availableOption in this.AvailableOptions)
            {
                var option = availableOption as IOptionsViewModelBase;
                if (option != null)
                {
                    option.SaveAndClose();
                }
            }

            this.OnRequestClose(this, "Exit");
        }

        /// <summary>
        ///     The reset general options for project.
        /// </summary>
        private void ResetGeneralOptionsForProject()
        {
            Dictionary<string, string> generalOptionsInDsk = this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(GlobalIds.GlobalPropsId);

            if (generalOptionsInDsk != null)
            {
                this.GeneralOptions.SetGeneralOptions(generalOptionsInDsk);
            }

            if (this.Project == null)
            {
                string solId = VsSonarUtils.SolutionGlobPropKey(this.Project.Key);
                Dictionary<string, string> projectOptionsInDsk = this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(solId);
                this.GeneralOptions.SetProjectOptions(projectOptionsInDsk);
            }
        }

        /// <summary>
        /// The update analysisPlugin options.
        /// </summary>
        /// <param name="selectedPlugin">
        /// The selected analysisPlugin.
        /// </param>
        private void UpdatePluginOptions(string selectedPlugin)
        {
            if (this.Project != null)
            {
                this.PluginManager.CanModifyPluginProps = true;
            }
            
            foreach (IAnalysisPlugin plugin in this.plugins)
            {
                IPluginsOptions pluginOptionsController = plugin.GetPluginControlOptions(this.SonarConfigurationViewModel.UserConnectionConfig);
                string pluginKey = plugin.GetKey(this.SonarConfigurationViewModel.UserConnectionConfig);

                if (pluginOptionsController == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(selectedPlugin))
                {
                    if (!pluginKey.Equals(selectedPlugin))
                    {
                        continue;
                    }
                }

                Dictionary<string, string> optionsInDsk = this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(pluginKey);
                this.OptionsInView = pluginOptionsController.GetUserControlOptions(this.Project);
                this.AnalysisPluginInView = plugin;
                pluginOptionsController.SetOptions(optionsInDsk);
            }
        }

        #endregion

        public void EndDataAssociation()
        {
            this.Project = null;
        }
    }
}