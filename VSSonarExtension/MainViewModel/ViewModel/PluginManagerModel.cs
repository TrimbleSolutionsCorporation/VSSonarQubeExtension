// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneralOptionsModel.cs" company="">
//   
// </copyright>
// <summary>
//   The dummy options controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtension.MainViewModel.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;

    using ExtensionTypes;

    using Microsoft.VisualStudio.Shell.Interop;

    using VSSonarExtension.MainView;

    using VSSonarPlugins;

    using UserControl = System.Windows.Controls.UserControl;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    public class PluginManagerModel : INotifyPropertyChanged
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

        /// <summary>
        /// The provider.
        /// </summary>
        private readonly IServiceProvider provider;

        /// <summary>
        /// The changes are required.
        /// </summary>
        private bool changesAreRequired;

        /// <summary>
        /// The plugin is selected.
        /// </summary>
        private bool pluginIsSelected;

        /// <summary>
        ///     The selected plugin.
        /// </summary>
        private PluginDescription selectedPlugin;

        /// <summary>
        ///     The dummy control.
        /// </summary>
        private UserControl userControl;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManagerModel"/> class.
        /// </summary>
        public PluginManagerModel()
        {
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
        public PluginManagerModel(IPluginController controller, IServiceProvider provider, ISonarConfiguration conf)
        {
            this.Conf = conf;
            this.provider = provider;
            this.controller = controller;
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
        public bool ChangesAreRequired
        {
            get
            {
                return this.changesAreRequired;
            }

            set
            {
                this.changesAreRequired = value;
                this.OnPropertyChanged("ChangesAreRequired");
            }
        }

        /// <summary>
        /// Gets or sets the conf.
        /// </summary>
        public ISonarConfiguration Conf { get; set; }

        /// <summary>
        ///     Gets or sets the excluded plugins.
        /// </summary>
        public string ExcludedPlugins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether install new plugin.
        /// </summary>
        public bool InstallNewPlugin
        {
            get
            {
                return true;
            }

            set
            {
                var filedialog = new OpenFileDialog { Filter = @"VSSonar Plugin|*.VSQ" };
                var result = filedialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    if (!File.Exists(filedialog.FileName))
                    {
                        MessageBox.Show(@"Error Choosing File, File Does not exits");
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

                this.OnPropertyChanged("InstallNewPlugin");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether plugin is selected.
        /// </summary>
        public bool PluginIsSelected
        {
            get
            {
                return this.pluginIsSelected;
            }

            set
            {
                this.pluginIsSelected = value;
                this.OnPropertyChanged("PluginIsSelected");
            }
        }

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

        /// <summary>
        ///     Gets or sets a value indicating whether remove plugin.
        /// </summary>
        public bool RemovePlugin
        {
            get
            {
                return true;
            }

            set
            {
                if (this.controller.RemovePlugin(this.Conf, this.SelectedPlugin))
                {
                    this.UpdatePluginInListOfPlugins(this.SelectedPlugin, "Plugin Will Be Removed In Next Restart");
                    this.ChangesAreRequired = true;
                }

                this.OnPropertyChanged("RemovePlugin");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether restart vs.
        /// </summary>
        public bool RestartVs
        {
            get
            {
                return true;
            }

            set
            {
                var obj = (IVsShell4)this.provider.GetService(typeof(SVsShell));
                obj.Restart((uint)__VSRESTARTTYPE.RESTART_Normal);
                this.OnPropertyChanged("RestartVS");
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

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The get user control options.
        /// </summary>
        /// <returns>
        ///     The <see cref="UserControl" />.
        /// </returns>
        public UserControl GetUserControlOptions()
        {
            return this.userControl ?? (this.userControl = new PluginManagerControl(this));
        }

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