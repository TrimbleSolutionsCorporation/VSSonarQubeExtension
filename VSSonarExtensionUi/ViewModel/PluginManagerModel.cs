// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneralOptionsModel.cs" company="">
//   
// </copyright>
// <summary>
//   The dummy options controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;

    using ExtensionTypes;

    using VSSonarPlugins;

    using UserControl = System.Windows.Controls.UserControl;
    using VSSonarExtensionUi.View;
    using PropertyChanged;
    using GalaSoft.MvvmLight.Command;
    using System.Windows;
    using System.Collections.Generic;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    [ImplementPropertyChanged]
    public class PluginManagerModel : IViewModelBase
    {
        #region Fields

        /// <summary>
        ///     The controller.
        /// </summary>
        private readonly IPluginController controller;

        /// <summary>
        ///     The plugin list.
        /// </summary>
        private readonly ObservableCollection<PluginDescription> pluginList = new ObservableCollection<PluginDescription>();

        private readonly IVsEnvironmentHelper visualStudioHelper;

        /// <summary>
        ///     The selected plugin.
        /// </summary>
        private PluginDescription selectedPlugin;

        /// <summary>
        ///     The dummy control.
        /// </summary>
        private UserControl userControl;

        private List<IAnalysisPlugin> plugins;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManagerModel"/> class.
        /// </summary>
        public PluginManagerModel()
        {
            this.Header = "Plugin Manager";

            this.InitCommanding();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManagerModel"/> class.
        /// </summary>
        /// <param name="controller">
        /// The controller.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="conf">
        /// The conf.
        /// </param>
        public PluginManagerModel(IPluginController controller, ISonarConfiguration conf, IVsEnvironmentHelper visualStudioHelper)
        {
            this.visualStudioHelper = visualStudioHelper;
            this.Header = "Plugin Manager";
            this.Conf = conf;
            this.controller = controller;
            

            this.InitCommanding();
            this.InitPluginList();
        }

        private void InitPluginList()
        {
            this.AnalysisPlugins = controller.GetPlugins();
            this.MenuPlugins = controller.GetMenuItemPlugins();

            foreach (var plugin in this.AnalysisPlugins)
            {
                this.PluginList.Add(plugin.GetPluginDescription(this.visualStudioHelper));
            }

            foreach (var plugin in this.MenuPlugins)
            {
                this.PluginList.Add(plugin.GetPluginDescription(this.visualStudioHelper));
            }
        }

        private void InitCommanding()
        {
            this.InstallNewPlugin = new RelayCommand(this.OnInstallNewPlugin);
            this.RestartVs = new RelayCommand(this.OnInstallNewPlugin, () => this.ChangesAreRequired);
            this.RemovePlugin = new RelayCommand(this.OnInstallNewPlugin, () => this.PluginIsSelected);
            this.PluginSettingsCommand = new RelayCommand(this.OnOnPluginSettingsCommand, () => this.PluginIsSelected);
        }

        private void OnOnPluginSettingsCommand()
        {
            var window = new Window();
            foreach (var plugin in this.AnalysisPlugins)
            {
                if (plugin.GetPluginDescription(this.visualStudioHelper).Name.Equals(this.SelectedPlugin.Name))
                {
                    window.Content = plugin.GetPluginControlOptions(this.Conf);
                    window.ShowDialog();
                    return;
                }
            }
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the assembly directory.
        /// </summary>
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether changes are required.
        /// </summary>
        public bool ChangesAreRequired { get; set; }


        /// <summary>
        /// Gets or sets the conf.
        /// </summary>
        public ISonarConfiguration Conf { get; set; }

        public RelayCommand InstallNewPlugin { get; set; }

        public RelayCommand RestartVs { get; set; }

        public RelayCommand RemovePlugin { get; set; }

        public RelayCommand PluginSettingsCommand { get; set; }

        /// <summary>
        ///     Gets or sets the excluded plugins.
        /// </summary>
        public string ExcludedPlugins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether install new plugin.
        /// </summary>
        public void OnInstallNewPlugin()
        {
            var filedialog = new OpenFileDialog { Filter = @"VSSonar Plugin|*.VSQ" };
            var result = filedialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (!File.Exists(filedialog.FileName))
                {
                    System.Windows.MessageBox.Show(@"Error Choosing File, File Does not exits");
                }
                else
                {
                    PluginDescription plugin = this.controller.IstallNewPlugin(filedialog.FileName, this.Conf);

                    if (plugin != null)
                    {
                        if (!this.UpdatePluginInListOfPlugins(plugin, "Plugin Will Be Updated in Next Restart"))
                        {
                            this.PluginList.Add(plugin);
                        }

                        this.ChangesAreRequired = true;
                    }
                    else
                    {
                        UserExceptionMessageBox.ShowException(
                            "Cannot Install Plugin", 
                            new Exception("Error Loading Plugin"), 
                            this.controller.GetErrorData());
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether plugin is selected.
        /// </summary>
        public bool PluginIsSelected { get; set; }

        /// <summary>
        ///     Gets the plugin list.
        /// </summary>
        public ObservableCollection<PluginDescription> PluginList
        {
            get
            {
                return this.pluginList;
            }
        }

        public string Header { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether remove plugin.
        /// </summary>
        public void OnRemovePlugin()
        {
                if (this.controller.RemovePlugin(this.Conf, this.SelectedPlugin))
                {
                    this.UpdatePluginInListOfPlugins(this.SelectedPlugin, "Plugin Will Be Removed In Next Restart");
                    this.ChangesAreRequired = true;
                }

                this.OnPropertyChanged("RemovePlugin");
        }

        /// <summary>
        /// Gets or sets a value indicating whether restart vs.
        /// </summary>
        public void OnRestartVs()
        {
            if (this.visualStudioHelper.AreWeRunningInVisualStudio())
            {
                this.visualStudioHelper.RestartVisualStudio();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the selected plugin.
        /// </summary>
        public PluginDescription SelectedPlugin
        {
            get
            {
                return this.selectedPlugin;
            }

            set
            {
                this.selectedPlugin = value;
                this.PluginIsSelected = this.selectedPlugin != null;

                this.OnPropertyChanged("SelectedPlugin");
            }
        }

        public List<IAnalysisPlugin> AnalysisPlugins { get; private set; }
        public List<IMenuCommandPlugin> MenuPlugins { get; private set; }

        #endregion

        #region Public Methods and Operators

        #endregion

        #region Methods

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// The update plugin in list of plugins.
        /// </summary>
        /// <param name="plugin">
        /// The plugin.
        /// </param>
        /// <param name="pluginWillBeRemovedInNextRestart">
        /// The plugin will be removed in next restart.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool UpdatePluginInListOfPlugins(PluginDescription plugin, string pluginWillBeRemovedInNextRestart)
        {
            bool present = false;
            var newList = new ObservableCollection<PluginDescription>();
            foreach (PluginDescription pluginDescription in this.PluginList)
            {
                if (pluginDescription.Name.Equals(plugin.Name))
                {
                    var updatedDescription = new PluginDescription
                                                 {
                                                     Name = pluginDescription.Name, 
                                                     Enabled = pluginDescription.Enabled, 
                                                     Status = pluginWillBeRemovedInNextRestart, 
                                                     SupportedExtensions = pluginDescription.SupportedExtensions, 
                                                     Version = pluginDescription.Version
                                                 };

                    newList.Add(updatedDescription);
                    present = true;
                }
                else
                {
                    newList.Add(pluginDescription);
                }
            }

            this.PluginList.Clear();
            foreach (PluginDescription pluginDescription in newList)
            {
                this.PluginList.Add(pluginDescription);
            }

            this.OnPropertyChanged("PluginList");
            return present;
        }

        #endregion
    }
}