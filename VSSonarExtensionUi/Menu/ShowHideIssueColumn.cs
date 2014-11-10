// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShowHideIssueColumn.cs" company="">
//   
// </copyright>
// <summary>
//   The show hide issue column.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.Menu
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    using GalaSoft.MvvmLight.Command;

    using VSSonarExtensionUi.ViewModel.Helpers;

    using VSSonarPlugins;

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
            this.AssociatedCommand = new RelayCommand(this.OnAssociateCommand);
            this.SubItems = new ObservableCollection<IMenuItem>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the associated command.
        /// </summary>
        public ICommand AssociatedCommand { get; set; }

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
                if (column.Equals("Component") || column.Equals("Message") || column.Equals("Line"))
                {
                    continue;
                }

                var subItem = new ShowHideIssueColumn(model, helper, column, gridKey) { IsEnabled = true };
                string visible = helper.ReadOptionFromApplicationData(gridKey, column + "Visible");
                if (visible.Equals("true"))
                {
                    subItem.CommandText = "Hide " + column;
                }
                else
                {
                    subItem.CommandText = "Show " + column;
                }

                menu.SubItems.Add(subItem);
            }

            return menu;
        }

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
                this.helper.WriteOptionInApplicationData(this.gridKey, this.name + "Visible", "true");
            }
            else
            {
                this.CommandText = "Show " + this.name;
                this.helper.WriteOptionInApplicationData(this.gridKey, this.name + "Visible", "false");
            }

            this.model.RestoreUserSettingsInIssuesDataGrid();
        }

        #endregion
    }
}