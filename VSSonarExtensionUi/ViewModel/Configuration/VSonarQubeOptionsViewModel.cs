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
    using System.Linq;
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

        private readonly INotificationManager notifycationManager;

        #endregion

        #region Constructors and Destructors

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
        public VSonarQubeOptionsViewModel(PluginController plugincontroller,
            SonarQubeViewModel model,
            IVsEnvironmentHelper vsHelper,
            IConfigurationHelper configurationHelper,
            INotificationManager notificationManager)
        {
            this.notifycationManager = notificationManager;
            this.model = model;
            this.Vsenvironmenthelper = vsHelper;
            this.ConfigurationHelper = configurationHelper;

            this.InitModels(plugincontroller);
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
        /// <param name="selectedProject"></param>
        public void RefreshPropertiesInView(Resource selectedProject)
        {
            this.Project = selectedProject;
            foreach (var availableOption in this.AvailableOptions)
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
        private void InitModels(PluginController plugincontroller)
        {
            this.AvailableOptions = new ObservableCollection<IOptionsViewModelBase>();

            this.GeneralConfigurationViewModel = new GeneralConfigurationViewModel(this, this.model.SonarRestConnector, this.Vsenvironmenthelper, this.model, this.ConfigurationHelper);
            this.AnalysisOptionsViewModel = new AnalysisOptionsViewModel(this.Vsenvironmenthelper, this, this.ConfigurationHelper);
            this.PluginManager = new PluginManagerModel(
                plugincontroller, 
                this.GeneralConfigurationViewModel.UserConnectionConfig, 
                this.Vsenvironmenthelper, this.ConfigurationHelper, 
                this, this.model, this.notifycationManager);
            this.LicenseManager = new LicenseViewerViewModel(this.PluginManager, this.GeneralConfigurationViewModel.UserConnectionConfig, this.ConfigurationHelper);

            this.AvailableOptions.Add(this.GeneralConfigurationViewModel);
            this.AvailableOptions.Add(this.AnalysisOptionsViewModel);
            this.AvailableOptions.Add(this.PluginManager);
            this.AvailableOptions.Add(this.LicenseManager);
            this.SelectedOption = this.GeneralConfigurationViewModel;
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
            this.ConfigurationHelper.ResetSettings();
            this.OnRequestClose(this, "Exit");
        }

        /// <summary>
        ///     The on save and exit command.
        /// </summary>
        private void OnSaveAndExitCommand()
        {
            foreach (var option in AvailableOptions)
            {
                option.SaveCurrentViewToDisk(this.ConfigurationHelper);
            }

            this.ConfigurationHelper.SyncSettings();
            this.OnRequestClose(this, "Exit");
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