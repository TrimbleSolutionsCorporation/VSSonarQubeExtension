// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionOptionsModel.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
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

namespace VSSonarExtensionUi.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using ExtensionHelpers;

    using ExtensionTypes;

    using VSSonarPlugins;
    using PropertyChanged;


    /// <summary>
    ///     The plugins options model.
    /// </summary>
    [ImplementPropertyChanged]
    public partial class ExtensionOptionsModel
    {
        /// <summary>
        ///     The options in view.
        /// </summary>
        private UserControl optionsInView;

        /// <summary>
        ///     The plugins.
        /// </summary>
        private List<IAnalysisPlugin> plugins;

        /// <summary>
        ///     The selected analysisPlugin item.
        /// </summary>
        private IViewModelBase selectedOption;

        /// <summary>
        /// The project.
        /// </summary>
        private Resource project;

        private SonarQubeViewModel model;

        private PluginController plugincontroller;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSSonarExtension.MainViewModel.ViewModel.ExtensionOptionsModel"/> class.
        /// </summary>
        public ExtensionOptionsModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VSSonarExtension.MainViewModel.ViewModel.ExtensionOptionsModel"/> class.
        /// </summary>
        public ExtensionOptionsModel(SonarQubeViewModel model)
        {
            this.model = model;

            this.InitModels();
        }

        private void InitModels()
        {
            this.AvailableOptions = new ObservableCollection<IViewModelBase>();
            this.GeneralOptions = new GeneralOptionsModel();
            this.PluginManager = new PluginManagerModel(this.plugincontroller, this.SonarConfiguration, this.Vsenvironmenthelper);
            this.LicenseManager = new LicenseViewerViewModel();

            this.AvailableOptions.Add(this.GeneralOptions);
            this.AvailableOptions.Add(this.PluginManager);
            this.AvailableOptions.Add(this.LicenseManager);

            if (this.plugins == null)
            {
                return;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="VSSonarExtension.MainViewModel.ViewModel.ExtensionOptionsModel"/> class.
        /// </summary>
        /// <param name="plugins">
        /// The plugins.
        /// </param>
        public ExtensionOptionsModel(PluginController plugincontroller, SonarQubeViewModel model, IVsEnvironmentHelper vsHelper, ISonarConfiguration conf)
        {
            this.model = model;
            this.plugincontroller = plugincontroller;
            this.plugins = plugincontroller.GetPlugins();
            this.ControlCommand = new UserSelectControl(this, this.plugins);
            this.SonarConfiguration = conf;
            this.Vsenvironmenthelper = vsHelper;

            this.InitModels();
        }

        public UserControl PluginsManagerOptionsFrame { get; set; }

        /// <summary>
        ///     The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The request close.
        /// </summary>
        public event Action<object, object> RequestClose;

        /// <summary>
        /// Gets or sets the general options.
        /// </summary>
        public GeneralOptionsModel GeneralOptions { get; set; }

        /// <summary>
        /// The plugin manager.
        /// </summary>
        public PluginManagerModel PluginManager { get; set; }


        /// <summary>
        ///     Gets or sets the options in view.
        /// </summary>
        public UserControl OptionsInView { get; set; }

        /// <summary>
        ///     Gets the available plugins collection.
        /// </summary>
        public ObservableCollection<IViewModelBase> AvailableOptions { get; set; }

        /// <summary>
        ///     Gets or sets the selected analysisPlugin item.
        /// </summary>
        public IViewModelBase SelectedOption
        {
            get
            {
                return this.selectedOption;
            }

            set
            {
                this.selectedOption = value;
            }
        }

        /// <summary>
        /// Gets or sets the control command.
        /// </summary>
        public UserSelectControl ControlCommand { get; set; }

        /// <summary>
        /// Gets or sets the analysisPlugin in view.
        /// </summary>
        public IAnalysisPlugin AnalysisPluginInView { get; set; }

        /// <summary>
        /// Gets or sets the vsenvironmenthelper.
        /// </summary>
        public IVsEnvironmentHelper Vsenvironmenthelper { get; set; }

        /// <summary>
        /// The project.
        /// </summary>
        public Resource Project
        {
            get
            {
                return this.project;
            }
            set
            {
                this.project = value;
                this.GeneralOptions.Project = value;
            }
        }

        public LicenseViewerViewModel LicenseManager { get; set; }
        public ISonarConfiguration SonarConfiguration { get; set; }

        /// <summary>
        /// The refresh properties in plugins.
        /// </summary>
        public void RefreshPropertiesInPlugins()
        {
            this.UpdatePluginOptions(string.Empty);
        }

        /// <summary>
        /// The refresh general properties.
        /// </summary>
        public void RefreshGeneralProperties()
        {
            this.ResetGeneralOptionsForProject();
        }

        /// <summary>
        /// The reset user data.
        /// </summary>
        public void ResetUserData()
        {
            var fileIsMissing = File.Exists(this.Vsenvironmenthelper.UserAppDataConfigurationFile());
            if (this.plugins == null)
            {
                return;
            }

            foreach (var plugin in this.plugins)
            {
                if (plugin.GetPluginControlOptions(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper)) == null)
                {
                    continue;
                }

                if (!fileIsMissing
                    || this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(plugin.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper))).Count == 0)
                {
                    var configuration = ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper);
                    plugin.GetPluginControlOptions(configuration).ResetDefaults();

                    var pluginKey = plugin.GetKey(configuration);
                    var optionsToSave = plugin.GetPluginControlOptions(configuration).GetOptions();
                    this.Vsenvironmenthelper.WriteAllOptionsForPluginOptionInApplicationData(pluginKey, null, optionsToSave);
                }
            }
        }


        /// <summary>
        /// The sync options to file.
        /// </summary>
        public void SyncOptionsToFile()
        {
            this.model.VsHelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.RunnerExecutableKey, this.GeneralOptions.SonarQubeBinary);
            this.model.VsHelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.JavaExecutableKey, this.GeneralOptions.JavaBinary);
            this.model.VsHelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.LocalAnalysisTimeoutKey, this.GeneralOptions.TimeoutValue.ToString(CultureInfo.InvariantCulture));
            this.model.VsHelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.IsDebugAnalysisOnKey, this.GeneralOptions.DebugIsChecked.ToString());
            this.model.VsHelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.ExcludedPluginsKey, this.GeneralOptions.ExcludedPlugins);

            foreach (var plugin in this.PluginManager.PluginList)
            {
                this.model.VsHelper.WriteOptionInApplicationData(GlobalIds.PluginEnabledControlId, plugin.Name, plugin.Enabled.ToString());
            }

            if (this.Project != null)
            {
                var solId = VsSonarUtils.SolutionGlobPropKey(this.Project.Key);
                this.model.VsHelper.WriteOptionInApplicationData(solId, GlobalIds.SonarSourceKey, this.GeneralOptions.SourceDir);
                this.model.VsHelper.WriteOptionInApplicationData(solId, GlobalIds.SourceEncodingKey, this.GeneralOptions.SourceEncoding);
            }


            if (this.AnalysisPluginInView != null)
            {
                var options = this.AnalysisPluginInView.GetPluginControlOptions(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.VsHelper)).GetOptions();
                var key = this.AnalysisPluginInView.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.VsHelper));
                this.model.VsHelper.WriteAllOptionsForPluginOptionInApplicationData(key, this.Project, options);
            }
        }

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        protected void OnPropertyChanged(string name)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
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
            var handler = this.RequestClose;
            if (handler != null)
            {
                handler(arg1, arg2);
            }
        }

        /// <summary>
        /// The reset general options for project.
        /// </summary>
        private void ResetGeneralOptionsForProject()
        {
            var generalOptionsInDsk = this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(GlobalIds.GlobalPropsId);

            if (generalOptionsInDsk != null)
            {
                this.GeneralOptions.SetGeneralOptions(generalOptionsInDsk);
            }

            if (this.Project == null)
            {
                var solId = VsSonarUtils.SolutionGlobPropKey(this.Project.Key);
                var projectOptionsInDsk = this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(solId);
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
            foreach (var plugin in this.plugins)
            {
                var configuration = ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper);
                var pluginOptionsController = plugin.GetPluginControlOptions(configuration);
                var pluginKey = plugin.GetKey(configuration);

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

                var optionsInDsk = this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(pluginKey);
                this.OptionsInView = pluginOptionsController.GetUserControlOptions(this.Project);
                this.AnalysisPluginInView = plugin;
                pluginOptionsController.SetOptions(optionsInDsk);
            }
        }

        /// <summary>
        /// The ok and exit.
        /// </summary>
        public class UserSelectControl : ICommand
        {
            /// <summary>
            /// The model.
            /// </summary>
            private readonly ExtensionOptionsModel model;

            /// <summary>
            /// The plugins.
            /// </summary>
            private List<IAnalysisPlugin> plugins;

            /// <summary>
            /// Initializes a new instance of the <see cref="VSSonarExtension.MainViewModel.ViewModel.ExtensionOptionsModel.UserSelectControl"/> class.
            /// </summary>
            public UserSelectControl()
            {
                var handler = this.CanExecuteChanged;
                if (handler != null)
                {
                    handler(this, null);
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="VSSonarExtension.MainViewModel.ViewModel.ExtensionOptionsModel.UserSelectControl"/> class.
            /// </summary>
            /// <param name="model">
            /// The model.
            /// </param>
            public UserSelectControl(ExtensionOptionsModel model, List<IAnalysisPlugin> plugins)
            {
                this.plugins = plugins;
                this.model = model;
            }

            /// <summary>
            /// The can execute changed.
            /// </summary>
            public event EventHandler CanExecuteChanged;

            /// <summary>
            /// The can execute.
            /// </summary>
            /// <param name="parameter">
            /// The parameter.
            /// </param>
            /// <returns>
            /// The <see cref="bool"/>.
            /// </returns>
            public bool CanExecute(object parameter)
            {
                return true;
            }

            /// <summary>
            /// The execute.
            /// </summary>
            /// <param name="parameter">
            /// The parameter.
            /// </param>
            public void Execute(object parameter)
            {
                var header = parameter as string;
                if (string.IsNullOrEmpty(header))
                {
                    return;
                }

                if (header.Equals("Apply"))
                {
                    this.model.SyncOptionsToFile();
                    return;
                }

                if (header.Equals("Save and Exit"))
                {
                    this.model.SyncOptionsToFile();
                    this.model.SelectedOption = null;
                }

                if (header.Equals("Cancel and Exit"))
                {
                    var pluginInView = this.model.AnalysisPluginInView;
                    if (pluginInView != null)
                    {
                        pluginInView.GetPluginControlOptions(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper)).SetOptions(this.model.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(pluginInView.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper))));
                    }

                    this.model.SelectedOption = null;
                }

                if (header.Equals("Generate Token"))
                {
                    if (this.model.SelectedLicense == null)
                    {
                        MessageBox.Show("Select a license first");
                        return;
                    }

                    foreach (var plugin in this.plugins)
                    {
                        var key = plugin.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper));
                        if (this.model.SelectedLicense.ProductId.Contains(key))
                        {
                            this.model.GeneratedToken = plugin.GenerateTokenId(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper));
                        }
                    }

                    return;
                }

                this.model.OnRequestClose(this, "Exit");
            }
        }
    }
}