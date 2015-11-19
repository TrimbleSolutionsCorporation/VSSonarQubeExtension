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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;

    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using Model.Helpers;
    using PropertyChanged;
    using View.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using UserControl = System.Windows.Controls.UserControl;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    [ImplementPropertyChanged]
    public class PluginManagerModel : IOptionsViewModelBase, IOptionsModelBase, IPluginManager
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
        /// The notification manager
        /// </summary>
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// The vshelper
        /// </summary>
        private readonly IVsEnvironmentHelper vshelper;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        /// The plugins
        /// </summary>
        private readonly IList<IPlugin> plugins = new List<IPlugin>();

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// The source dir
        /// </summary>
        private string sourceDir;

        private IIssueTrackerPlugin issueTrackerPlugin;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManagerModel" /> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="notifyManager">The notify manager.</param>
        /// <param name="helper">The helper.</param>
        public PluginManagerModel(
            IPluginController controller, 
            IConfigurationHelper configurationHelper,
            INotificationManager notifyManager,
            IVsEnvironmentHelper helper)
        {
            this.notificationManager = notifyManager;
            this.Header = "Plugin Manager";
            this.configurationHelper = configurationHelper;
            this.controller = controller;
            this.vshelper = helper;

            this.plugins = new List<IPlugin>();

            this.MenuPlugins = new List<IMenuCommandPlugin>();
            this.AnalysisPlugins = new List<IAnalysisPlugin>();
            this.SourceCodePlugins = new List<ISourceVersionPlugin>();
            this.IssueTrackerPlugins = new List<IIssueTrackerPlugin>();

            this.InitPluginList(helper, null);
            this.InitCommanding();

            SonarQubeViewModel.RegisterNewViewModelInPool(this);
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
        public IList<IAnalysisPlugin> AnalysisPlugins { get; private set; }

        /// <summary>
        /// Gets the issue tracker plugins.
        /// </summary>
        /// <value>
        /// The issue tracker plugins.
        /// </value>
        public IList<IIssueTrackerPlugin> IssueTrackerPlugins { get; private set; }

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
        public IList<IMenuCommandPlugin> MenuPlugins { get; private set; }

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

        /// <summary>
        /// Gets or sets the plugin controller.
        /// </summary>
        /// <value>
        /// The plugin controller.
        /// </value>
        public IPluginControlOption PluginController { get; set; }

        /// <summary>
        /// Gets the source code plugins.
        /// </summary>
        /// <value>
        /// The source code plugins.
        /// </value>
        public IList<ISourceVersionPlugin> SourceCodePlugins { get; private set; }

        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        private Resource Project { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="configIn">The configuration in.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="sourceModel">The source model.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public void AssociateWithNewProject(Resource project, string workingDir, ISourceControlProvider sourceModel, IIssueTrackerPlugin sourcePlugin, IList<Resource> availableProjects)
        {
            // not neccessary
        }

        /// <summary>
        /// Gets the issue tracker plugin.
        /// </summary>
        /// <returns>enabled plugin</returns>
        public IIssueTrackerPlugin GetIssueTrackerPlugin()
        {
            return this.issueTrackerPlugin;
        }

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="associatedProjectIn">The associated project in.</param>
        public void ReloadDataFromDisk(Resource associatedProjectIn)
        {
            this.Project = associatedProjectIn;
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>
        /// returns view model
        /// </returns>
        public object GetViewModel()
        {
            return this;
        }

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
                    var files = this.controller.DeployPlugin(filedialog.FileName);

                    try
                    {
                        if (!this.InitPluginList(this.vshelper, files))
                        {
                            UserExceptionMessageBox.ShowException("Cannot Install Plugin", new Exception("Error Loading Plugin"), this.controller.GetErrorData());
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
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
                plugin.Dispose();
                this.controller.RemovePlugin((IPlugin)plugin, this.plugins);
                this.AnalysisPlugins.Remove(plugin);
                this.PluginList.Remove(this.SelectedPlugin);
                this.plugins.Remove(plugin);
                return;
            }

            var menuPlugin = this.MenuPlugins.SingleOrDefault(s => s.GetPluginDescription().Name.Equals(this.SelectedPlugin.Name));
            if (menuPlugin != null)
            {
                this.controller.RemovePlugin((IPlugin)menuPlugin, this.plugins);
                this.MenuPlugins.Remove(menuPlugin);
                this.plugins.Remove(menuPlugin);
                this.PluginList.Remove(this.SelectedPlugin);
                this.notificationManager.RemoveMenuPlugin(menuPlugin);
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
        /// Called when [disconnect].
        /// </summary>
        public void OnDisconnect()
        {
        }

        /// <summary>
        /// The update services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public void UpdateServices(
            IVsEnvironmentHelper vsenvironmenthelperIn, 
            IVSSStatusBar statusBar, 
            IServiceProvider provider)
        {
            // does not access vs services
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        public void SaveData()
        {
            if (this.associatedProject == null)
            {
                return;
            }

            foreach (var plugin in this.plugins)
            {
                var option = plugin.GetPluginControlOptions(this.associatedProject, AuthtenticationHelper.AuthToken);

                if (option != null)
                {
                    option.SaveDataInUi(this.Project, this.configurationHelper);
                }
            }

            foreach (var pluginDescription in this.pluginList)
            {
                if (pluginDescription.Enabled)
                {
                    this.configurationHelper.WriteOptionInApplicationData(
                        Context.IssueTrackerProps,
                        this.associatedProject.Name,
                        pluginDescription.Name,
                        "true");
                }
                else
                {
                    this.configurationHelper.WriteOptionInApplicationData(
                        Context.IssueTrackerProps,
                        this.associatedProject.Name,
                        pluginDescription.Name,
                        "false");
                }
            }
        }

        /// <summary>
        /// Gets the available model, TODO: needs to be removed after viewmodels are split into models and view models
        /// </summary>
        /// <returns>
        /// returns optinal model
        /// </returns>
        public object GetAvailableModel()
        {
            return null;
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workDir">The work dir.</param>
        /// <param name="provider">The provider.</param>
        public void AssociateWithNewProject(Resource project, string workDir, ISourceControlProvider provider)
        {
            this.sourceDir = workDir;
            this.associatedProject = project;

            foreach (var plugin in this.plugins)
            {
                var plugDesc = plugin.GetPluginDescription();
                try
                {
                    string isEnabled = this.configurationHelper.ReadSetting(Context.IssueTrackerProps, project.Name, plugDesc.Name).Value;
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
                    this.configurationHelper.WriteOptionInApplicationData(Context.IssueTrackerProps, project.Name, plugDesc.Name, "true");
                    plugDesc.Enabled = true;
                }

                try
                {
                    var menuPlugin = plugin as IMenuCommandPlugin;
                    if (menuPlugin != null)
                    {
                        menuPlugin.UpdateConfiguration(AuthtenticationHelper.AuthToken, project, this.vshelper);
                    }

                    plugin.AssociateProject(project, AuthtenticationHelper.AuthToken);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            // select issue tracker plugin to use
            this.SelectIssueTrackerPlugin();
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void OnSolutionClosed()
        {
            this.associatedProject = null;
            this.sourceDir = string.Empty;
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
        /// The init plugin list.
        /// </summary>
        /// <param name="visualStudioHelper">The visual studio helper.</param>
        /// <param name="files">The files.</param>
        /// <returns>true on ok</returns>
        private bool InitPluginList(IVsEnvironmentHelper visualStudioHelper, IEnumerable<string> files)
        {
            bool loaded = false;

            foreach (var plugin in this.controller.LoadPluginsFromPluginFolder(this.notificationManager, this.configurationHelper, visualStudioHelper, files))
            {
                var plugindata = plugin as IAnalysisPlugin;
                if (plugindata != null)
                {
                    this.AnalysisPlugins.Add(plugindata);
                    this.PluginList.Add(plugindata.GetPluginDescription());
                    this.plugins.Add(plugin);
                    loaded = true;
                    continue;                    
                }

                var pluginMenu = plugin as IMenuCommandPlugin;
                if (pluginMenu != null)
                {
                    this.MenuPlugins.Add(pluginMenu);
                    this.PluginList.Add(pluginMenu.GetPluginDescription());
                    this.plugins.Add(plugin);
                    loaded = true;
                    continue;
                }

                var pluginSourceControl = plugin as ISourceVersionPlugin;
                if (pluginSourceControl != null)
                {
                    this.SourceCodePlugins.Add(pluginSourceControl);
                    this.PluginList.Add(pluginSourceControl.GetPluginDescription());
                    this.plugins.Add(plugin);
                    loaded = true;
                    continue;
                }

                var pluginIssueTracker = plugin as IIssueTrackerPlugin;
                if (pluginIssueTracker != null)
                {
                    this.IssueTrackerPlugins.Add(pluginIssueTracker);
                    this.PluginList.Add(pluginIssueTracker.GetPluginDescription());
                    this.plugins.Add(plugin);
                    loaded = true;
                    continue;
                }
            }

            return loaded;
        }

        /// <summary>
        ///     The on selection change command.
        /// </summary>
        private void OnSelectionChangeCommand()
        {
            this.OptionsInView = null;

            if (this.SelectedPlugin == null)
            {
                return;
            }

            this.PluginIsSelected = true;

            foreach (IPlugin plugin in this.plugins)
            {
                var plugDesc = plugin.GetPluginDescription();
                if (plugDesc.Name.Equals(this.SelectedPlugin.Name))
                {
                    try
                    {
                        this.PluginController = plugin.GetPluginControlOptions(this.Project, AuthtenticationHelper.AuthToken);
                        if (this.PluginController != null)
                        {
                            this.OptionsInView = this.PluginController.GetOptionControlUserInterface();
                            this.PluginController.RefreshDataInUi(this.Project, this.configurationHelper);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                    return;
                }
            }
        }

        /// <summary>
        /// Selects the issue tracker plugin.
        /// </summary>
        private void SelectIssueTrackerPlugin()
        {
            int cnt = 0;
            var builder = new StringBuilder();
            foreach (IPlugin plugin in this.IssueTrackerPlugins)
            {
                if (plugin.GetPluginDescription().Enabled)
                {
                    this.issueTrackerPlugin = plugin as IIssueTrackerPlugin;
                    builder.AppendLine("Plugin Enabled: " + plugin.GetPluginDescription().Name);
                    cnt++;
                }
            }

            if (cnt > 1)
            {
                MessageDisplayBox.DisplayMessage("More than on issue tracker plugin is enabled, make sure only one pluing is enabled", builder.ToString());
                this.issueTrackerPlugin = null;
            }
        }

        #endregion
    }
}