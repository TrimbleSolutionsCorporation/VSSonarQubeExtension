// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarQubeViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   The sonar qube view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtensionUi.ViewModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Controls;
    using System.Windows.Documents;

    using ExtensionHelpers;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    using PropertyChanged;

    using VSSonarPlugins;

    /// <summary>
    /// The sonar qube view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class SonarQubeViewModel
    {
        /// <summary>
        /// The analysisPlugin control.
        /// </summary>
        public readonly PluginController PluginControl = new PluginController();
        private readonly Dictionary<int, IMenuCommandPlugin> menuItemPlugins = new Dictionary<int, IMenuCommandPlugin>();

        /// <summary>
        /// The vs helper.
        /// </summary>
        private IVsEnvironmentHelper vsHelper;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeViewModel"/> class.
        /// </summary>
        public SonarQubeViewModel()
        {
            this.ToolsProvidedByPlugins = new ObservableCollection<MenuItem>();
            this.vsHelper = new VsPropertiesHelper();

            this.InitCommands();
            this.InitMenus();
            this.InitViews();
        }

        private void InitViews()
        {
            this.SonarQubeViews = new ObservableCollection<IViewModelBase>();
            this.SonarQubeViews.Add(new ServerViewModel());
            this.SonarQubeViews.Add(new LocalViewModel());
            this.SonarQubeViews.Add(new IssuesSearchViewModel());
        }

        public bool IsRunningInVisualStudio()
        {
            return vsHelper.AreWeRunningInVisualStudio();
        }

        /// <summary>
        /// The init menus.
        /// </summary>
        private void InitMenus()
        {
            if (this.PluginControl == null)
            {
                return;
            }

            var plugins = this.PluginControl.GetMenuItemPlugins();

            if (plugins == null)
            {
                return;
            }

            foreach (var plugin in plugins)
            {
                this.ToolsProvidedByPlugins.Add(new MenuItem { Header = plugin.GetHeader() });
                this.menuItemPlugins.Add(this.menuItemPlugins.Count, plugin);
            }
        }

        /// <summary>
        /// The init commands.
        /// </summary>
        private void InitCommands()
        {
            this.LaunchExtensionPropertiesCommand = new RelayCommand(this.LaunchExtensionProperties);
            this.ToolSwitchCommand = new RelayCommand<string>(this.ExecuteToolWindow);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the launch extension properties command.
        /// </summary>
        public RelayCommand LaunchExtensionPropertiesCommand { get; set; }

        /// <summary>
        /// Gets or sets the tool switch command.
        /// </summary>
        public RelayCommand<string> ToolSwitchCommand { get; set; }

        /// <summary>
        /// Gets the tools provided by plugins.
        /// </summary>
        public ObservableCollection<MenuItem> ToolsProvidedByPlugins { get; set; }

        public ObservableCollection<IViewModelBase> SonarQubeViews { get; set; }

        public IViewModelBase SelectedView { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The execute tool window.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void ExecuteToolWindow(string obj)
        {
        }

        /// <summary>
        /// The launch extension properties.
        /// </summary>
        private void LaunchExtensionProperties()
        {
        }

        #endregion
    }
}