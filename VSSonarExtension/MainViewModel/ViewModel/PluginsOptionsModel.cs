// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginsOptionsModel.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using ExtensionHelpers;

    using ExtensionTypes;

    using VSSonarPlugins;

    /// <summary>
    ///     The plugins options model.
    /// </summary>
    public partial class PluginsOptionsModel : INotifyPropertyChanged
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
        /// The open solution.
        /// </summary>
        private Resource openSolution;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginsOptionsModel"/> class.
        /// </summary>
        public PluginsOptionsModel()
        {
            this.ControlCommand = new UserSelectControl(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginsOptionsModel"/> class.
        /// </summary>
        /// <param name="vsenvironmenthelper">
        /// The vsenvironmenthelper.
        /// </param>
        public PluginsOptionsModel(IVsEnvironmentHelper vsenvironmenthelper)
        {
            this.ControlCommand = new UserSelectControl(this);
            this.Vsenvironmenthelper = vsenvironmenthelper;
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
        ///     Gets or sets the plugins.
        /// </summary>
        public ReadOnlyCollection<IPlugin> Plugins
        {
            get
            {
                return this.plugins;
            }

            set
            {
                this.plugins = value;
                this.OnPropertyChanged("AvailablePluginsCollection");
                this.OnPropertyChanged("PluginController");
            }
        }

        /// <summary>
        /// Gets or sets the open solution.
        /// </summary>
        public Resource OpenSolution 
        {
            get 
            {
                if (string.IsNullOrEmpty(this.Vsenvironmenthelper.ActiveSolutionName()))
                {
                    this.openSolution = null;
                }

                return this.openSolution;
            }

            set 
            {
                this.openSolution = value;
                this.OnPropertyChanged("OpenSolution");
            }
        }

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
                if (this.Plugins == null)
                {
                    return options;
                }

                foreach (var plugin in this.Plugins)
                {
                    options.Add(plugin.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper)));
                }

                options.Add("Licenses");
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
                        this.GetAvailableLicenses();
                        this.IsLicenseEnable = true;
                        return;
                    }

                    foreach (var plugin in this.plugins)
                    {
                        var configuration = ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper);
                        var pluginOptionsController = plugin.GetPluginControlOptions(configuration, this.OpenSolution);
                        var pluginKey = plugin.GetKey(configuration);

                        if (!pluginKey.Equals(value) || pluginOptionsController == null)
                        {
                            continue;
                        }

                        var optionsInDsk = this.Vsenvironmenthelper.ReadAllOptionsForPluginOptionInApplicationData(pluginKey);
                        pluginOptionsController.SetOptions(optionsInDsk);
                        this.PluginInView = plugin;
                        this.OptionsInView = pluginOptionsController.GetUserControlOptions(this.OpenSolution);
                    }
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
        /// The reset user data.
        /// </summary>
        public void ResetUserData()
        {
            var fileIsMissing = File.Exists(this.Vsenvironmenthelper.UserAppDataConfigurationFile());
            if (this.Plugins == null)
            {
                return;
            }

            foreach (var plugin in this.Plugins)
            {
                if (plugin.GetPluginControlOptions(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper), this.OpenSolution) == null)
                {
                    continue;
                }

                if (!fileIsMissing 
                    || this.Vsenvironmenthelper.ReadAllOptionsForPluginOptionInApplicationData(plugin.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper))).Count == 0)
                {
                    var configuration = ConnectionConfigurationHelpers.GetConnectionConfiguration(this.Vsenvironmenthelper);
                    plugin.GetPluginControlOptions(configuration, this.OpenSolution).ResetDefaults();

                    var pluginKey = plugin.GetKey(configuration);
                    var optionsToSave = plugin.GetPluginControlOptions(configuration, this.OpenSolution).GetOptions();
                    this.Vsenvironmenthelper.WriteAllOptionsForPluginOptionInApplicationData(pluginKey, null, optionsToSave);                    
                }
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
        /// The ok and exit.
        /// </summary>
        public class UserSelectControl : ICommand
        {
            /// <summary>
            /// The model.
            /// </summary>
            private readonly PluginsOptionsModel model;

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
            public UserSelectControl(PluginsOptionsModel model)
            {
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
                    this.SyncOptionsToFile();
                    return;
                }

                if (header.Equals("Save and Exit"))
                {
                    this.SyncOptionsToFile();
                    this.model.SelectedPluginItem = null;
                }                

                if (header.Equals("Cancel and Exit"))
                {
                    var pluginInView = this.model.PluginInView;
                    if (pluginInView != null)
                    {
                        pluginInView.GetPluginControlOptions(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper), this.model.OpenSolution).SetOptions(this.model.Vsenvironmenthelper.ReadAllOptionsForPluginOptionInApplicationData(pluginInView.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper))));
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

                    foreach (var plugin in this.model.Plugins)
                    {
                        if (plugin.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper)).Equals(this.model.SelectedLicense.ProductId))
                        {
                            this.model.GeneratedToken = plugin.GenerateTokenId(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper));
                        }
                    }

                    return;
                }
                
                this.model.OnRequestClose(this, "Exit");
            }

            /// <summary>
            /// The sync options to file.
            /// </summary>
            private void SyncOptionsToFile()
            {
                if (this.model.PluginInView != null)
                {
                    var options = this.model.PluginInView.GetPluginControlOptions(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper), this.model.OpenSolution).GetOptions();
                    var key = this.model.PluginInView.GetKey(ConnectionConfigurationHelpers.GetConnectionConfiguration(this.model.Vsenvironmenthelper));
                    this.model.Vsenvironmenthelper.WriteAllOptionsForPluginOptionInApplicationData(key, this.model.OpenSolution, options);
                }
            }
        }
    }
}