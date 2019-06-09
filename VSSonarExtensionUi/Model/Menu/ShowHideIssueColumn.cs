// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShowHideIssueColumn.cs" company="">
//   
// </copyright>
// <summary>
//   The show hide issue column.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.Model.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    using Association;    
    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using ViewModel.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using SonarRestService.Types;
    using SonarRestService;
    using System.Threading.Tasks;

    /// <summary>
    ///     The show hide issue column.
    /// </summary>
    internal class ShowHideIssueColumn : IMenuItem
    {
        #region Fields

        /// <summary>
        /// The grid key.
        /// </summary>
        private readonly string gridKey;

        /// <summary>
        ///     The helper.
        /// </summary>
        private readonly IConfigurationHelper helper;

        /// <summary>
        ///     The model.
        /// </summary>
        private readonly IssueGridViewModel model;

        /// <summary>
        ///     The name.
        /// </summary>
        private readonly string name;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowHideIssueColumn"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="gridKey">
        /// The grid key.
        /// </param>
        public ShowHideIssueColumn(IssueGridViewModel model, IConfigurationHelper helper, string name, string gridKey)
        {
            this.gridKey = gridKey;
            this.helper = helper;
            this.name = name;
            this.model = model;
            this.CommandText = "Columns";
            this.IsEnabled = true;
            this.ExecuteCommand = new RelayCommand(this.OnAssociateCommand);
            this.SubItems = new ObservableCollection<IMenuItem>();

            AssociationModel.RegisterNewModelInPool(this);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the associated command.
        /// </summary>
        public ICommand ExecuteCommand { get; set; }

        /// <summary>
        ///     Gets or sets the command text.
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the sub items.
        /// </summary>
        public ObservableCollection<IMenuItem> SubItems { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The make menu.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="columns">
        /// The columns.
        /// </param>
        /// <param name="gridKey">
        /// The grid key.
        /// </param>
        /// <returns>
        /// The <see cref="IMenuItem"/>.
        /// </returns>
        public static IMenuItem MakeMenu(IssueGridViewModel model, IConfigurationHelper helper, List<string> columns, string gridKey)
        {
            var menu = new ShowHideIssueColumn(model, helper, string.Empty, gridKey) { CommandText = "Columns", IsEnabled = false };

            foreach (string column in columns)
            {
                if (column.Equals("Component") || column.Equals("Message") || column.Equals("Line") || column.Equals("LocalPath") || column.Equals("Comments"))
                {
                    continue;
                }

                var subItem = new ShowHideIssueColumn(model, helper, column, gridKey) { IsEnabled = true, CommandText = "Hide " + column };
                var value = helper.ReadSetting(Context.UIProperties, gridKey, column + "Visible");
                if (value == null)
                {
                    subItem.CommandText = "Hide " + column;
                    menu.SubItems.Add(subItem);
                    continue;
                }

                if (value.Value.ToLower().Equals("true"))
                {
                    subItem.CommandText = "Hide " + column;
                }
                else
                {
                    subItem.CommandText = "Show " + column;
                }

                menu.SubItems.Add(subItem);
            }

            var subItemreset = new ShowHideIssueColumn(model, helper, "Reset View", gridKey) { IsEnabled = true, CommandText = "Reset View" };
            menu.SubItems.Add(subItemreset);

            return menu;
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
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public async Task UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            // menu not accessing services
            await Task.Delay(0);
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        public async Task OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IList<IIssueTrackerPlugin> sourcePlugin)
        {
            // does nothing
            await Task.Delay(0);
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public async Task AssociateWithNewProject(
            Resource project,
            string workingDir,
            ISourceControlProvider provider,
            Dictionary<string, Profile> profile,
            string visualStudioVersion)
        {
            // menu not accessing services
            await Task.Delay(0);
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public async Task OnSolutionClosed()
        {
            // menu not accessing services
            await Task.Delay(0);
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public async Task OnDisconnect()
        {
            await Task.Delay(0);
        }

        /// <summary>
        /// Refreshes the menu data for menu that have options that
        /// are context dependent on the selected issues.
        /// </summary>
        public async Task RefreshMenuData()
        {
			// not necessary
			await Task.Delay(0);
		}

        /// <summary>
        /// Cancels the refresh data.
        /// </summary>
        public async Task CancelRefreshData()
        {
			// not necessay
			await Task.Delay(0);
		}

        #endregion

        #region Methods

        /// <summary>
        ///     The on associate command.
        /// </summary>
        private void OnAssociateCommand()
        {
            if (this.CommandText.Equals("Show " + this.name))
            {
                this.CommandText = "Hide " + this.name;
                var prop = new SonarQubeProperties();
                prop.Owner = this.gridKey;
                prop.Value = "true";
                prop.Key = this.name + "Visible";
                prop.Context = Context.UIProperties.ToString();
                this.helper.WriteSetting(prop, true);
                this.model.RestoreUserSettingsInIssuesDataGrid();
                return;
            }

            if (this.CommandText.Equals("Hide " + this.name))
            {
                this.CommandText = "Show " + this.name;
                var prop = new SonarQubeProperties();
                prop.Owner = this.gridKey;
                prop.Value = "false";
                prop.Key = this.name + "Visible";
                prop.Context = Context.UIProperties.ToString();
                this.helper.WriteSetting(prop, false);
                this.model.RestoreUserSettingsInIssuesDataGrid();
                return;
            }

            if (this.CommandText.Equals("Reset View"))
            {
                this.model.ResetColumnsView();
                return;
            }            
        }

        #endregion
    }
}