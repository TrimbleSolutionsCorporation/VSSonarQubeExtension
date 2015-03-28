// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginManagerModel.cs" company="">
//   
// </copyright>
// <summary>
//   The dummy options controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.ViewModel.Configuration
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;

    using ExtensionTypes;

    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using SonarRestService;

    using VSSonarExtensionUi.View.Helpers;
    using VSSonarExtensionUi.ViewModel.Helpers;

    using VSSonarPlugins;

    using Application = System.Windows.Application;
    using UserControl = System.Windows.Controls.UserControl;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    [ImplementPropertyChanged]
    public class PluginManagerModel : IViewModelBase, IOptionsViewModelBase
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
        ///     The viewModel.
        /// </summary>
        private readonly VSonarQubeOptionsViewModel viewModel;
        private readonly SonarQubeViewModel sqmodel;
        private readonly ISonarConfiguration sonarConf;

        /// <summary>
        ///     The visual studio helper.
        /// </summary>
        private IVsEnvironmentHelper visualStudioHelper;

        private IConfigurationHelper configurationHelper;
        private readonly INotificationManager notificationManager;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManagerModel"/> class.
        /// </summary>
        /// <param name="controller">
        /// The controller.
        /// </param>
        /// <param name="conf">
        /// The conf.
        /// </param>
        /// <param name="visualStudioHelper">
        /// The visual Studio Helper.
        /// </param>
        /// <param name="viewModel">
        /// The viewModel.
        /// </param>
        public PluginManagerModel(
            IPluginController controller, 
            ISonarConfiguration conf, 
            IVsEnvironmentHelper visualStudioHelper, 
            IConfigurationHelper configurationHelper,
            VSonarQubeOptionsViewModel viewModel, 
            SonarQubeViewModel mainModel,
            INotificationManager notifyManager)
        {
            this.notificationManager = notifyManager;
            this.Header = "Plugin Manager";
            this.visualStudioHelper = visualStudioHelper;
            this.configurationHelper = configurationHelper;
            this.viewModel = viewModel;
            this.sqmodel = mainModel;
            this.sonarConf = conf;
            this.controller = controller;

            this.MenuPlugins = new List<IMenuCommandPlugin>();
            this.AnalysisPlugins = new List<IAnalysisPlugin>();

            this.InitPluginList();
            this.InitCommanding();
        }

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
        ///     Gets the analysis plugins.
        /// </summary>
        public List<IAnalysisPlugin> AnalysisPlugins { get; private set; }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether changes are required.
        /// </summary>
        public bool ChangesAreRequired { get; set; }

        /// <summary>
        ///     Gets or sets the excluded plugins.
        /// </summary>
        public string ExcludedPlugins { get; set; }

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        ///     Gets or sets the install new plugin.
        /// </summary>
        public RelayCommand InstallNewPlugin { get; set; }

        /// <summary>
        ///     Gets the menu plugins.
        /// </summary>
        public List<IMenuCommandPlugin> MenuPlugins { get; private set; }

        /// <summary>
        ///     Gets or sets the options in view.
        /// </summary>
        public UserControl OptionsInView { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether plugin is selected.
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

        /// <summary>
        ///     Gets or sets the remove plugin.
        /// </summary>
        public RelayCommand RemovePlugin { get; set; }

        /// <summary>
        ///     Gets or sets the selected plugin.
        /// </summary>
        public PluginDescription SelectedPlugin { get; set; }

        /// <summary>
        ///     Gets or sets the selection changed command.
        /// </summary>
        public ICommand SelectionChangedCommand { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="associatedProject">
        /// The associated project.
        /// </param>
        /// <param name="userConnectionConfig">
        /// The user connection config.
        /// </param>
        /// <param name="workingDir">
        /// The working dir.
        /// </param>
        public void RefreshPropertiesInView(Resource associatedProject)
        {
            this.Project = associatedProject;
        }

        private Resource Project { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether install new plugin.
        /// </summary>
        public void OnInstallNewPlugin()
        {
            var filedialog = new OpenFileDialog { Filter = @"VSSonar Plugin|*.VSQ" };
            DialogResult result = filedialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (!File.Exists(filedialog.FileName))
                {
                    UserExceptionMessageBox.ShowException("Error Choosing File, File Does not exits", null);
                }
                else
                {
                    IPlugin plugin = this.controller.IstallNewPlugin(filedialog.FileName, this.sonarConf);

                    if (plugin != null)
                    {
                        try
                        {
                            IMenuCommandPlugin menuPlugin = (IMenuCommandPlugin)plugin;
                            foreach (var pluginI in this.MenuPlugins)
                            {
                                if (pluginI.GetPluginDescription().Name.Equals(menuPlugin.GetPluginDescription().Name))
                                {
                                    return;
                                }
                            }
                            this.MenuPlugins.Add(menuPlugin);
                            this.sqmodel.AddANewMenu(menuPlugin);
                            var pluginDesc = menuPlugin.GetPluginDescription();
                            pluginDesc.Enabled = true;
                            this.PluginList.Add(pluginDesc);
                        }
                        catch(Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            try
                            {
                                IAnalysisPlugin analysisPlugin = (IAnalysisPlugin)plugin;
                                foreach (var pluginI in this.AnalysisPlugins)
                                {
                                    if (pluginI.GetPluginDescription().Name.Equals(analysisPlugin.GetPluginDescription().Name))
                                    {
                                        return;
                                    }
                                }
                                this.AnalysisPlugins.Add(analysisPlugin);
                                var pluginDesc = analysisPlugin.GetPluginDescription();
                                pluginDesc.Enabled = true;
                                this.PluginList.Add(pluginDesc);
                            }
                            catch (Exception ex1)
                            {
                                Debug.WriteLine(ex1.Message);
                            }
                        }
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
        ///     Gets or sets a value indicating whether remove plugin.
        /// </summary>
        public void OnRemovePlugin()
        {
            var plugin = this.AnalysisPlugins.SingleOrDefault(s => s.GetPluginDescription().Name.Equals(this.SelectedPlugin.Name));
            if (plugin != null)
            {
                this.controller.RemovePlugin((IPlugin)plugin);
                this.AnalysisPlugins.Remove(plugin);
                this.PluginList.Remove(this.SelectedPlugin);
                return;
            }

            var menuPlugin = this.MenuPlugins.SingleOrDefault(s => s.GetPluginDescription().Name.Equals(this.SelectedPlugin.Name));
            if (menuPlugin != null)
            {
                this.controller.RemovePlugin((IPlugin)plugin);
                this.MenuPlugins.Remove(menuPlugin);
                this.PluginList.Remove(this.SelectedPlugin);
                this.sqmodel.RemoveMenuPlugin(menuPlugin);
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

            foreach (var plugin in this.MenuPlugins)
            {
                plugin.UpdateTheme(this.BackGroundColor, this.ForeGroundColor);
            }
        }

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
            IConfigurationHelper configurationHelper,
            IVSSStatusBar statusBar, 
            IServiceProvider provider)
        {
            this.configurationHelper = configurationHelper;
            this.visualStudioHelper = vsenvironmenthelperIn;
        }

        public void SaveCurrentViewToDisk(IConfigurationHelper configurationHelper)
        {
            if (this.PluginController != null)
            {
                this.PluginController.SaveDataInUi(this.Project, this.configurationHelper);
            }

            foreach (var pluginDescription in this.pluginList)
            {
                if (pluginDescription.Enabled)
                {
                    this.configurationHelper.WriteOptionInApplicationData(
                        Context.AnalysisGeneral,
                        pluginDescription.Name,
                        GlobalIds.PluginEnabledControlId,
                        "true");
                }
                else
                {
                    this.configurationHelper.WriteOptionInApplicationData(Context.AnalysisGeneral, pluginDescription.Name, GlobalIds.PluginEnabledControlId, "false");
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The init commanding.
        /// </summary>
        private void InitCommanding()
        {
            this.InstallNewPlugin = new RelayCommand(this.OnInstallNewPlugin);
            this.RemovePlugin = new RelayCommand(this.OnRemovePlugin);

            this.SelectionChangedCommand = new RelayCommand(this.OnSelectionChangeCommand);
        }

        /// <summary>
        ///     The init plugin list.
        /// </summary>
        private void InitPluginList()
        {
            foreach (var plugin in this.controller.LoadPluginsFromPluginFolder(this.notificationManager, this.configurationHelper))
            {
                var plugDesc = plugin.GetPluginDescription();
                try
                {
                    string isEnabled = this.configurationHelper.ReadSetting(Context.AnalysisGeneral, plugDesc.Name, GlobalIds.PluginEnabledControlId).Value;
                    if (isEnabled.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        plugDesc.Enabled = true;
                    }
                    else
                    {
                        plugDesc.Enabled = false;
                    }
                }
                catch (Exception)
                {
                    this.configurationHelper.WriteOptionInApplicationData(Context.AnalysisGeneral, plugDesc.Name, GlobalIds.PluginEnabledControlId, "true");
                    plugDesc.Enabled = true;
                }

                try
                {
                    var plugindata = (IAnalysisPlugin)plugin;
                    this.AnalysisPlugins.Add(plugindata);
                    this.PluginList.Add(plugDesc);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    try
                    {
                        var plugindata = (IMenuCommandPlugin)plugin;
                        this.MenuPlugins.Add(plugindata);
                        this.PluginList.Add(plugDesc);
                    }
                    catch (Exception ex1)
                    {
                        Debug.WriteLine(ex1.Message);
                    }
                }                
            }
        }

        /// <summary>
        ///     The on selection change command.
        /// </summary>
        private void OnSelectionChangeCommand()
        {
            if (this.SelectedPlugin != null)
            {
                this.PluginIsSelected = true;

                foreach (IAnalysisPlugin plugin in this.AnalysisPlugins)
                {
                    var plugDesc = plugin.GetPluginDescription();
                    if (plugDesc.Name.Equals(this.SelectedPlugin.Name))
                    {
                        try
                        {
                            this.PluginController = plugin.GetPluginControlOptions(this.Project, this.sonarConf);
                            this.OptionsInView = this.PluginController.GetOptionControlUserInterface();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }

                        this.PluginController.RefreshDataInUi(this.Project, this.configurationHelper);
                        return;
                    }
                }
            }
            else
            {
                this.PluginIsSelected = false;
                this.OptionsInView = null;
            }
        }

        public IPluginControlOption PluginController { get; set; }

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

            return present;
        }

        #endregion
    }
}