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

    using GalaSoft.MvvmLight.Command;
    using SonarLocalAnalyser;
    using ViewModel;
    using ViewModel.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using Helpers;
    using Association;

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
        /// The refresh menu items status.
        /// </summary>
        /// <param name="contextMenuItems">
        /// The context menu items.
        /// </param>
        /// <param name="b">
        /// The b.
        /// </param>
        public static void RefreshMenuItemsStatus(ObservableCollection<IMenuItem> contextMenuItems, bool b)
        {
            foreach (IMenuItem contextMenuItem in contextMenuItems)
            {
                if (contextMenuItem is ShowHideIssueColumn)
                {
                }
            }
        }

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
                if (column.Equals("Component") || column.Equals("Message") || column.Equals("Line"))
                {
                    continue;
                }

                var subItem = new ShowHideIssueColumn(model, helper, column, gridKey) { IsEnabled = true, CommandText = "Hide " + column };
                try
                {
                    var value = helper.ReadSetting(Context.UIProperties, gridKey, column + "Visible").Value;
                    if (value.Equals("true"))
                    {
                        subItem.CommandText = "Hide " + column;
                    }
                    else
                    {
                        subItem.CommandText = "Show " + column;
                    }
                }
                catch (Exception)
                {
                    subItem.CommandText = "Hide " + column;
                }

                menu.SubItems.Add(subItem);
            }

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
        public void UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            // menu not accessing services
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="provider">The provider.</param>
        public void AssociateWithNewProject(ISonarConfiguration config, Resource project, string workingDir, ISourceControlProvider provider)
        {
            // menu not accessing services
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void EndDataAssociation()
        {
            // menu not accessing services
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
                prop.Context = Context.UIProperties;
                this.helper.WriteSetting(prop, true);
            }
            else
            {
                this.CommandText = "Show " + this.name;
                var prop = new SonarQubeProperties();
                prop.Owner = this.gridKey;
                prop.Value = "false";
                prop.Key = this.name + "Visible";
                prop.Context = Context.UIProperties;
                this.helper.WriteSetting(prop, true);
            }

            this.model.RestoreUserSettingsInIssuesDataGrid();
        }

        #endregion
    }
}