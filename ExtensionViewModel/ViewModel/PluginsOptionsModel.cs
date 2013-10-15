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

namespace ExtensionViewModel.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Windows.Controls;
    using System.Windows.Input;
    using ExtensionHelpers;
    using VSSonarPlugins;

    /// <summary>
    ///     The plugins options model.
    /// </summary>
    public class PluginsOptionsModel : INotifyPropertyChanged
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
        ///     The user control width.
        /// </summary>
        private double userControlWidth;

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
        ///     Gets or sets the user control width.
        /// </summary>
        public double UserControlWidth
        {
            get
            {
                return this.userControlWidth;
            }

            set
            {
                this.userControlWidth = value;
                this.OnPropertyChanged("UserControlWidth");
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
                    options.Add(plugin.GetKey());
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
                    this.PluginInView = null;
                    this.OptionsInView = null;
                    foreach (var plugin in this.plugins)
                    {
                        if (!plugin.GetKey().Equals(value) || plugin.GetUsePluginControlOptions() == null)
                        {
                            continue;
                        }

                        this.PluginInView = plugin;
                        plugin.GetUsePluginControlOptions().SetOptions(this.Vsenvironmenthelper.ReadAllOptionsForPluginOptionInApplicationData(plugin.GetKey()));
                        this.OptionsInView = plugin.GetUsePluginControlOptions().GetUserControlOptions();                        
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
                if (plugin.GetUsePluginControlOptions() == null)
                {
                    continue;
                }

                if (!fileIsMissing || this.Vsenvironmenthelper.ReadAllOptionsForPluginOptionInApplicationData(plugin.GetKey()).Count == 0)
                {
                    plugin.GetUsePluginControlOptions().ResetDefaults();
                    this.Vsenvironmenthelper.WriteAllOptionsForPluginOptionInApplicationData(plugin.GetKey(), plugin.GetUsePluginControlOptions().GetOptions());                    
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
                }                

                if (header.Equals("Cancel and Exit"))
                {
                    var pluginInView = this.model.PluginInView;
                    if (pluginInView != null)
                    {
                        pluginInView.GetUsePluginControlOptions().SetOptions(this.model.Vsenvironmenthelper.ReadAllOptionsForPluginOptionInApplicationData(pluginInView.GetKey()));
                    }
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
                    var options = this.model.PluginInView.GetUsePluginControlOptions().GetOptions();
                    foreach (var option in options)
                    {
                        var key = this.model.PluginInView.GetKey();
                        this.model.Vsenvironmenthelper.WriteOptionInApplicationData(key, option.Key, option.Value);
                    }
                }
            }
        }
    }
}