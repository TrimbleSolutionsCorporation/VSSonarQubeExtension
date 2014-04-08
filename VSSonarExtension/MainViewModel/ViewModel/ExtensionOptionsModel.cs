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

namespace VSSonarExtension.MainViewModel.ViewModel
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

    using VSSonarExtension.MainView;
    using VSSonarExtension.PackageImplementation.PckResources;

    using VSSonarPlugins;

    /// <summary>
    ///     The plugins options model.
    /// </summary>
    public partial class ExtensionOptionsModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     The options in view.
        /// </summary>
        private UserControl optionsInView;

        /// <summary>
        ///     The plugins.
        /// </summary>
        private ReadOnlyCollection<IPlugin> plugins;

        /// <summary>
        ///     The selected plugin item.
        /// </summary>
        private string selectedPluginItem;

        /// <summary>
        /// The project.
        /// </summary>
        private Resource project;

        private ExtensionDataModel model;

        private PluginController plugincontroller;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionOptionsModel"/> class.
        /// </summary>
        public ExtensionOptionsModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionOptionsModel"/> class.
        /// </summary>
        public ExtensionOptionsModel(ExtensionDataModel model)
        {
            this.model = model;
            this.GeneralOptions = new GeneralOptionsModel();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionOptionsModel"/> class.
        /// </summary>
        /// <param name="plugins">
        /// The plugins.
        /// </param>
        public ExtensionOptionsModel(PluginController plugincontroller, ExtensionDataModel model)
        {
            this.model = model;
            this.plugincontroller = plugincontroller;
            this.plugins = plugincontroller.GetPlugins();
            this.ControlCommand = new UserSelectControl(this, this.plugins);
            this.GeneralOptions = new GeneralOptionsModel();
            this.GeneralOptionsFrame = this.GeneralOptions.GetUserControlOptions();
        }

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
        public readonly GeneralOptionsModel GeneralOptions;

        /// <summary>
        /// The general options frame.
        /// </summary>
        private UserControl GeneralOptionsFrame;

        /// <summary>
        ///     Gets or sets the options in view.
        /// </summary>
        public UserControl OptionsInView
        {
            get
            {
                return this.optionsInView;
            }

            set
            {
                this.optionsInView = value;
                this.isLicenseEnable = false;
                this.OnPropertyChanged("IsLicenseEnable");
                this.OnPropertyChanged("OptionsInView");
            }
        }

        /// <summary>
        ///     Gets the available plugins collection.
        /// </summary>
        public ObservableCollection<string> AvailablePluginsCollection
        {
            get
            {
                var options = new ObservableCollection<string>();
                options.Add("General Settings");
                options.Add("Apperance Settings");
                options.Add("Licenses");
                if (this.plugins == null)
                {
                    return options;
                }
                
                foreach (var plugin in this.plugins)
                {
                    options.Add(plugin.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper)));
                }

                return options;
            }
        }

        /// <summary>
        ///     Gets or sets the selected plugin item.
        /// </summary>
        public string SelectedPluginItem
        {
            get
            {
                return this.selectedPluginItem;
            }

            set
            {
                this.selectedPluginItem = value;
                if (value != null)
                {
                    this.OptionsInView = null;
                    this.PluginInView = null;
                    this.OnPropertyChanged("PluginInView");

                    this.IsLicenseEnable = false;
                    if (value.Equals("Licenses"))
                    {
                        this.AvailableLicenses = this.GetLicensesFromServer();
                        this.IsLicenseEnable = true;
                        return;
                    }

                    if (value.Equals("Apperance Settings"))
                    {
                        this.OptionsInView = new IssuesGridSettingsDialog(this.model);
                        return;
                    }

                    if (value.Equals("General Settings"))
                    {
                        this.ResetGeneralOptionsForProject();

                        this.GeneralOptions.PluginList.Clear();
                        foreach (var plugin in this.plugincontroller.GetPlugins())
                        {
                            this.GeneralOptions.PluginList.Add(plugin.GetPluginDescription(this.Vsenvironmenthelper));
                        }

                        foreach (var plugin in this.plugincontroller.GetMenuItemPlugins())
                        {
                            this.GeneralOptions.PluginList.Add(plugin.GetPluginDescription(this.Vsenvironmenthelper));
                        }

                        this.OptionsInView = this.GeneralOptionsFrame;
                        return;
                    }

                    this.UpdatePluginOptions(value);
                }
                else
                {
                    this.OptionsInView = null;
                    this.selectedPluginItem = "No Plugin Selected";
                }

                this.OnPropertyChanged("SelectedPluginItem");
            }
        }

        /// <summary>
        /// Gets or sets the control command.
        /// </summary>
        public UserSelectControl ControlCommand { get; set; }

        /// <summary>
        /// Gets or sets the plugin in view.
        /// </summary>
        public IPlugin PluginInView { get; set; }

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
            this.model.Vsenvironmenthelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.RunnerExecutableKey, this.GeneralOptions.SonarQubeBinary);
            this.model.Vsenvironmenthelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.JavaExecutableKey, this.GeneralOptions.JavaBinary);
            this.model.Vsenvironmenthelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.LocalAnalysisTimeoutKey, this.GeneralOptions.TimeoutValue.ToString(CultureInfo.InvariantCulture));
            this.model.Vsenvironmenthelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.IsDebugAnalysisOnKey, this.GeneralOptions.DebugIsChecked.ToString());
            this.model.Vsenvironmenthelper.WriteOptionInApplicationData(GlobalIds.GlobalPropsId, GlobalIds.ExcludedPluginsKey, this.GeneralOptions.ExcludedPlugins);

            foreach (var plugin in this.GeneralOptions.PluginList)
            {
                this.model.Vsenvironmenthelper.WriteOptionInApplicationData(GlobalIds.PluginEnabledControlId, plugin.Name, plugin.Enabled.ToString());                
            }

            if (this.Project != null)
            {
                var solId = VsSonarUtils.SolutionGlobPropKey(this.Project.Key);
                this.model.Vsenvironmenthelper.WriteOptionInApplicationData(solId, GlobalIds.SonarSourceKey, this.GeneralOptions.SourceDir);
                this.model.Vsenvironmenthelper.WriteOptionInApplicationData(solId, GlobalIds.SourceEncodingKey, this.GeneralOptions.SourceEncoding);
            }


            if (this.PluginInView != null)
            {
                var options = this.PluginInView.GetPluginControlOptions(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper)).GetOptions();
                var key = this.PluginInView.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper));
                this.model.Vsenvironmenthelper.WriteAllOptionsForPluginOptionInApplicationData(key, this.Project, options);
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
            var optionsInDsk = this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(GlobalIds.GlobalPropsId);
            this.GeneralOptions.SetGeneralOptions(optionsInDsk);
            if (this.Project != null)
            {
                var solId = VsSonarUtils.SolutionGlobPropKey(this.Project.Key);
                optionsInDsk = this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(solId);
                this.GeneralOptions.SetProjectOptions(optionsInDsk);
            }
        }

        /// <summary>
        /// The update plugin options.
        /// </summary>
        /// <param name="selectedPlugin">
        /// The selected plugin.
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
                this.PluginInView = plugin;
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
            private ReadOnlyCollection<IPlugin> plugins;

            /// <summary>
            /// Initializes a new instance of the <see cref="UserSelectControl"/> class.
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
            /// Initializes a new instance of the <see cref="UserSelectControl"/> class.
            /// </summary>
            /// <param name="model">
            /// The model.
            /// </param>
            public UserSelectControl(ExtensionOptionsModel model, ReadOnlyCollection<IPlugin> plugins )
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
                    this.model.SelectedPluginItem = null;
                }                

                if (header.Equals("Cancel and Exit"))
                {
                    var pluginInView = this.model.PluginInView;
                    if (pluginInView != null)
                    {
                        pluginInView.GetPluginControlOptions(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper)).SetOptions(this.model.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(pluginInView.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper))));
                    }

                    this.model.SelectedPluginItem = null;
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