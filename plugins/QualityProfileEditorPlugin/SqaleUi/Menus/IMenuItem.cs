// --------------------------------------------------------------------------------------------------------------------
// <copyright file="imenuitem.cs" company="Copyright © 2014 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
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

namespace SqaleUi.Menus
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    using SqaleUi.View;
    using SqaleUi.ViewModel;

    /// <summary>
    ///     The MenuItem interface.
    /// </summary>
    public interface IMenuItem
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the associated command.
        /// </summary>
        ICommand AssociatedCommand { get; set; }

        /// <summary>
        ///     Gets or sets the command text.
        /// </summary>
        string CommandText { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the sub items.
        /// </summary>
        ObservableCollection<IMenuItem> SubItems { get; set; }

        #endregion
    }

    /// <summary>
    /// The create tag menu item.
    /// </summary>
    internal class CreateTagMenuItem : IMenuItem
    {
        #region Fields

        /// <summary>
        /// The model.
        /// </summary>
        private readonly ISqaleGridVm model;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTagMenuItem"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public CreateTagMenuItem(ISqaleGridVm model)
        {
            this.model = model;
            this.CommandText = "Change Tags";
            this.IsEnabled = true;
            this.AssociatedCommand = new RelayCommand<object>(this.OnAssociateCommand);

            this.SubItems = new ObservableCollection<IMenuItem>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the associated command.
        /// </summary>
        public ICommand AssociatedCommand { get; set; }

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the sub items.
        /// </summary>
        public ObservableCollection<IMenuItem> SubItems { get; set; }

        /// <summary>
        /// Gets or sets the tag model.
        /// </summary>
        public TagEditorViewModel TagModel { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The make menu.
        /// </summary>
        /// <param name="sqaleGridVm">
        /// The sqale grid vm.
        /// </param>
        /// <returns>
        /// The <see cref="IMenuItem"/>.
        /// </returns>
        public static IMenuItem MakeMenu(ISqaleGridVm sqaleGridVm)
        {
            return new CreateTagMenuItem(sqaleGridVm);
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
                if (contextMenuItem is CreateTagMenuItem)
                {
                    contextMenuItem.IsEnabled = b;

                    if (((CreateTagMenuItem)contextMenuItem).TagModel != null)
                    {
                        ((CreateTagMenuItem)contextMenuItem).TagModel.RefreshTagsInRule();
                    }
                }
            }
        }

        /// </param>
        public static void RefreshMenuItemsStatus(ObservableCollection<IMenuItem> contextMenuItems, bool b, ISonarConfiguration conf, ISonarRestService rest, ISqaleGridVm model)
        {
            foreach (IMenuItem contextMenuItem in contextMenuItems)
            {
                if (contextMenuItem is CreateTagMenuItem)
                {
                    contextMenuItem.IsEnabled = b;

                    if (((CreateTagMenuItem)contextMenuItem).TagModel != null)
                    {
                        ((CreateTagMenuItem)contextMenuItem).TagModel.RefreshTagsInRule();
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on associate command.
        /// </summary>
        public void OnAssociateCommand(object data)
        {
            if (this.TagModel == null)
            {
                this.TagModel = new TagEditorViewModel(this.model.Configuration, this.model.RestService, this.model);
            }

            var window = new TagEditorView(this.TagModel);
            window.Show();
        }

        #endregion
    }
}