// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSonarQubeOptionsViewModel.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2014 [Jorge Costa, Jorge.Costa@tekla.com]
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

    using SonarRestService;

    using VSSonarExtensionUi.ViewModel.Helpers;

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
            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;

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

            this.ForeGroundColor = Colors.Black;
            this.BackGroundColor = Colors.White;
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
        public VSonarQubeOptionsViewModel(PluginController plugincontroller, SonarQubeViewModel model, IVsEnvironmentHelper vsHelper, IConfigurationHelper configurationHelper)
        {
            this.model = model;
            this.plugincontroller = plugincontroller;
            this.plugins = plugincontroller.GetPlugins();
            this.Vsenvironmenthelper = vsHelper;
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
        public GeneralConfigurationViewModel GeneralConfigurationViewModel { get; set; }

        /// <summary>
        ///     Gets or sets the vsenvironmenthelper.
        /// </summary>
        public IVsEnvironmentHelper Vsenvironmenthelper { get; set; }

        public IConfigurationHelper ConfigurationHelper { get; set; }

        #endregion

        #region Public Methods and Operators

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
            this.Vsenvironmenthelper = vsenvironmenthelperIn;
        }

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
            bool fileIsMissing = File.Exists(this.ConfigurationHelper.UserAppDataConfigurationFile());
            if (this.plugins == null)
            {
                return;
            }

            foreach (IAnalysisPlugin plugin in this.plugins)
            {
                if (plugin.GetPluginControlOptions(this.GeneralConfigurationViewModel.UserConnectionConfig) == null)
                {
                    continue;
                }

                if (!fileIsMissing
                    || this.ConfigurationHelper.ReadAllAvailableOptionsInSettings(
                        plugin.GetKey(this.GeneralConfigurationViewModel.UserConnectionConfig)).Count == 0)
                {
                    plugin.GetPluginControlOptions(this.GeneralConfigurationViewModel.UserConnectionConfig).ResetDefaults();

                    string pluginKey = plugin.GetKey(this.GeneralConfigurationViewModel.UserConnectionConfig);
                    Dictionary<string, string> optionsToSave =
                        plugin.GetPluginControlOptions(this.GeneralConfigurationViewModel.UserConnectionConfig).GetOptions();
                    this.ConfigurationHelper.WriteAllOptionsForPluginOptionInApplicationData(pluginKey, null, optionsToSave);
                }
            }
        }

        /// <summary>
        ///     The sync options to file.
        /// </summary>
        public void SyncOptionsToFile()
        {
            this.ConfigurationHelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.RunnerExecutableKey, 
                this.AnalysisOptionsViewModel.SonarQubeBinary);
            this.ConfigurationHelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.JavaExecutableKey, this.AnalysisOptionsViewModel.JavaBinary);
            this.ConfigurationHelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.LocalAnalysisTimeoutKey, 
                this.AnalysisOptionsViewModel.TimeoutValue.ToString(CultureInfo.InvariantCulture));
            this.ConfigurationHelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.IsDebugAnalysisOnKey, 
                this.AnalysisOptionsViewModel.DebugIsChecked.ToString());
            this.ConfigurationHelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.ExcludedPluginsKey, 
                this.AnalysisOptionsViewModel.ExcludedPlugins);

            foreach (PluginDescription plugin in this.PluginManager.PluginList)
            {
                this.ConfigurationHelper.WriteOptionInApplicationData(GlobalIds.PluginEnabledControlId, plugin.Name, plugin.Enabled.ToString());
            }

            if (this.Project != null)
            {
                string solId = VsSonarUtils.SolutionGlobPropKey(this.Project.Key);
                this.ConfigurationHelper.WriteOptionInApplicationData(solId, GlobalIds.SonarSourceKey, this.AnalysisOptionsViewModel.SourceDir);
                this.ConfigurationHelper.WriteOptionInApplicationData(solId, GlobalIds.SourceEncodingKey, this.AnalysisOptionsViewModel.SourceEncoding);
            }

            if (this.AnalysisPluginInView != null)
            {
                Dictionary<string, string> options =
                    this.AnalysisPluginInView.GetPluginControlOptions(this.GeneralConfigurationViewModel.UserConnectionConfig).GetOptions();
                string key = this.AnalysisPluginInView.GetKey(this.GeneralConfigurationViewModel.UserConnectionConfig);
                this.ConfigurationHelper.WriteAllOptionsForPluginOptionInApplicationData(key, this.Project, options);
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

            this.GeneralConfigurationViewModel = new GeneralConfigurationViewModel(this, this.model.SonarRestConnector, this.Vsenvironmenthelper, this.model, this.ConfigurationHelper);
            this.AnalysisOptionsViewModel = new AnalysisOptionsViewModel(this.Vsenvironmenthelper, this, this.ConfigurationHelper);
            this.PluginManager = new PluginManagerModel(
                this.plugincontroller, 
                this.GeneralConfigurationViewModel.UserConnectionConfig, 
                this.Vsenvironmenthelper, this.ConfigurationHelper, 
                this);
            this.LicenseManager = new LicenseViewerViewModel();

            this.AvailableOptions.Add(this.GeneralConfigurationViewModel);
            this.AvailableOptions.Add(this.AnalysisOptionsViewModel);
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

            this.SelectedOption = this.GeneralConfigurationViewModel;
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
            Dictionary<string, string> generalOptionsInDsk = this.ConfigurationHelper.ReadAllAvailableOptionsInSettings(GlobalIds.GlobalPropsId);

            if (generalOptionsInDsk != null)
            {
                this.AnalysisOptionsViewModel.SetGeneralOptions(generalOptionsInDsk);
            }

            if (this.Project != null)
            {
                string solId = VsSonarUtils.SolutionGlobPropKey(this.Project.Key);
                Dictionary<string, string> projectOptionsInDsk = this.ConfigurationHelper.ReadAllAvailableOptionsInSettings(solId);
                this.AnalysisOptionsViewModel.SetProjectOptions(projectOptionsInDsk);
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
                IPluginsOptions pluginOptionsController = plugin.GetPluginControlOptions(this.GeneralConfigurationViewModel.UserConnectionConfig);
                string pluginKey = plugin.GetKey(this.GeneralConfigurationViewModel.UserConnectionConfig);

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

                Dictionary<string, string> optionsInDsk = this.ConfigurationHelper.ReadAllAvailableOptionsInSettings(pluginKey);
                this.OptionsInView = pluginOptionsController.GetUserControlOptions(this.Project);
                this.AnalysisPluginInView = plugin;
                pluginOptionsController.SetOptions(optionsInDsk);
            }
        }

        #endregion

        /// <summary>
        /// The end data association.
        /// </summary>
        public void EndDataAssociation()
        {
            this.Project = null;
        }
    }
}