// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionOptionsModel.cs" company="">
//   
// </copyright>
// <summary>
//   The plugins options model.
// </summary>
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

    using VSSonarPlugins;

    /// <summary>
    ///     The plugins options model.
    /// </summary>
    public partial class ExtensionOptionsModel : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        ///     Gets or sets the general options.
        /// </summary>
        public readonly GeneralOptionsModel GeneralOptions;

        /// <summary>
        ///     The plugin manager.
        /// </summary>
        public readonly PluginManagerModel PluginManager;

        /// <summary>
        ///     The general options frame.
        /// </summary>
        private readonly UserControl GeneralOptionsFrame;

        /// <summary>
        /// The model.
        /// </summary>
        private readonly ExtensionDataModel model;

        /// <summary>
        /// The plugincontroller.
        /// </summary>
        private readonly PluginController plugincontroller;

        /// <summary>
        ///     The plugins.
        /// </summary>
        private readonly List<IAnalysisPlugin> plugins;

        /// <summary>
        ///     The options in view.
        /// </summary>
        private UserControl optionsInView;

        /// <summary>
        ///     The project.
        /// </summary>
        private Resource project;

        /// <summary>
        ///     The selected analysisPlugin item.
        /// </summary>
        private string selectedPluginItem;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionOptionsModel" /> class.
        /// </summary>
        public ExtensionOptionsModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionOptionsModel"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public ExtensionOptionsModel(ExtensionDataModel model)
        {
            this.model = model;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionOptionsModel"/> class.
        /// </summary>
        /// <param name="plugincontroller">
        /// The plugincontroller.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="conf">
        /// The conf.
        /// </param>
        public ExtensionOptionsModel(PluginController plugincontroller, ExtensionDataModel model, IServiceProvider provider, ISonarConfiguration conf)
        {
            this.model = model;
            this.plugincontroller = plugincontroller;
            this.plugins = plugincontroller.GetPlugins();
            this.ControlCommand = new UserSelectControl(this, this.plugins);
            this.GeneralOptions = new GeneralOptionsModel();
            this.GeneralOptionsFrame = this.GeneralOptions.GetUserControlOptions();
            this.PluginManager = new PluginManagerModel(plugincontroller, provider, conf);
            this.PluginsManagerOptionsFrame = this.PluginManager.GetUserControlOptions();
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

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
        ///     Gets the available plugins collection.
        /// </summary>
        public ObservableCollection<string> AvailablePluginsCollection
        {
            get
            {
                var options = new ObservableCollection<string>();
                options.Add("General Settings");
                options.Add("Apperance Settings");
                options.Add("Plugin Manager");
                options.Add("Licenses");
                if (this.plugins == null)
                {
                    return options;
                }

                foreach (IAnalysisPlugin plugin in this.plugins)
                {
                    options.Add(plugin.GetKey(null));
                }

                return options;
            }
        }

        /// <summary>
        ///     Gets or sets the control command.
        /// </summary>
        public UserSelectControl ControlCommand { get; set; }

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
        /// Gets or sets the plugins manager options frame.
        /// </summary>
        public UserControl PluginsManagerOptionsFrame { get; set; }

        /// <summary>
        ///     The project.
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
        ///     Gets or sets the selected analysisPlugin item.
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
                    this.AnalysisPluginInView = null;
                    this.OnPropertyChanged("AnalysisPluginInView");

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
                        this.OptionsInView = this.GeneralOptionsFrame;
                        return;
                    }

                    if (value.Equals("Plugin Manager"))
                    {
                        this.PluginManager.PluginList.Clear();
                        foreach (IAnalysisPlugin plugin in this.plugincontroller.GetPlugins())
                        {
                            this.PluginManager.PluginList.Add(plugin.GetPluginDescription(this.Vsenvironmenthelper));
                        }

                        foreach (IMenuCommandPlugin plugin in this.plugincontroller.GetMenuItemPlugins())
                        {
                            this.PluginManager.PluginList.Add(plugin.GetPluginDescription(this.Vsenvironmenthelper));
                        }

                        this.OptionsInView = this.PluginsManagerOptionsFrame;
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
                if (plugin.GetPluginControlOptions(null) == null)
                {
                    continue;
                }

                if (!fileIsMissing || this.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(plugin.GetKey(null)).Count == 0)
                {
                    ISonarConfiguration configuration = null;
                    plugin.GetPluginControlOptions(configuration).ResetDefaults();

                    string pluginKey = plugin.GetKey(configuration);
                    Dictionary<string, string> optionsToSave = plugin.GetPluginControlOptions(configuration).GetOptions();
                    this.Vsenvironmenthelper.WriteAllOptionsForPluginOptionInApplicationData(pluginKey, null, optionsToSave);
                }
            }
        }

        /// <summary>
        ///     The sync options to file.
        /// </summary>
        public void SyncOptionsToFile()
        {
            this.model.Vsenvironmenthelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.RunnerExecutableKey, 
                this.GeneralOptions.SonarQubeBinary);
            this.model.Vsenvironmenthelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.JavaExecutableKey, 
                this.GeneralOptions.JavaBinary);
            this.model.Vsenvironmenthelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.LocalAnalysisTimeoutKey, 
                this.GeneralOptions.TimeoutValue.ToString(CultureInfo.InvariantCulture));
            this.model.Vsenvironmenthelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.IsDebugAnalysisOnKey, 
                this.GeneralOptions.DebugIsChecked.ToString());
            this.model.Vsenvironmenthelper.WriteOptionInApplicationData(
                GlobalIds.GlobalPropsId, 
                GlobalIds.ExcludedPluginsKey, 
                this.GeneralOptions.ExcludedPlugins);

            foreach (PluginDescription plugin in this.PluginManager.PluginList)
            {
                this.model.Vsenvironmenthelper.WriteOptionInApplicationData(GlobalIds.PluginEnabledControlId, plugin.Name, plugin.Enabled.ToString());
            }

            if (this.Project != null)
            {
                string solId = VsSonarUtils.SolutionGlobPropKey(this.Project.Key);
                this.model.Vsenvironmenthelper.WriteOptionInApplicationData(solId, GlobalIds.SonarSourceKey, this.GeneralOptions.SourceDir);
                this.model.Vsenvironmenthelper.WriteOptionInApplicationData(solId, GlobalIds.SourceEncodingKey, this.GeneralOptions.SourceEncoding);
            }

            if (this.AnalysisPluginInView != null)
            {
                Dictionary<string, string> options = this.AnalysisPluginInView.GetPluginControlOptions(null).GetOptions();
                string key = this.AnalysisPluginInView.GetKey(null);
                this.model.Vsenvironmenthelper.WriteAllOptionsForPluginOptionInApplicationData(key, this.Project, options);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
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
            Action<object, object> handler = this.RequestClose;
            if (handler != null)
            {
                handler(arg1, arg2);
            }
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
            foreach (IAnalysisPlugin plugin in this.plugins)
            {
                ISonarConfiguration configuration = null;
                IPluginsOptions pluginOptionsController = plugin.GetPluginControlOptions(configuration);
                string pluginKey = plugin.GetKey(configuration);

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

        /// <summary>
        ///     The ok and exit.
        /// </summary>
        public class UserSelectControl : ICommand
        {
            #region Fields

            /// <summary>
            ///     The model.
            /// </summary>
            private readonly ExtensionOptionsModel model;

            /// <summary>
            ///     The plugins.
            /// </summary>
            private readonly List<IAnalysisPlugin> plugins;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="UserSelectControl" /> class.
            /// </summary>
            public UserSelectControl()
            {
                EventHandler handler = this.CanExecuteChanged;
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
            /// <param name="plugins">
            /// The plugins.
            /// </param>
            public UserSelectControl(ExtensionOptionsModel model, List<IAnalysisPlugin> plugins)
            {
                this.plugins = plugins;
                this.model = model;
            }

            #endregion

            #region Public Events

            /// <summary>
            ///     The can execute changed.
            /// </summary>
            public event EventHandler CanExecuteChanged;

            #endregion

            #region Public Methods and Operators

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
                    IAnalysisPlugin pluginInView = this.model.AnalysisPluginInView;
                    if (pluginInView != null)
                    {
                        pluginInView.GetPluginControlOptions(null)
                            .SetOptions(this.model.Vsenvironmenthelper.ReadAllAvailableOptionsInSettings(pluginInView.GetKey(null)));
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

                    foreach (IAnalysisPlugin plugin in this.plugins)
                    {
                        string key = plugin.GetKey(null);
                        if (this.model.SelectedLicense.ProductId.Contains(key))
                        {
                            this.model.GeneratedToken = plugin.GenerateTokenId(null);
                        }
                    }

                    return;
                }

                this.model.OnRequestClose(this, "Exit");
            }

            #endregion
        }
    }
}