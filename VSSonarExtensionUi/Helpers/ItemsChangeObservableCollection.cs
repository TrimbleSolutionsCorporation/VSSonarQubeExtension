// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItemsChangeObservableCollection.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
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
namespace SqaleUi.helpers
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;

    using VSSonarExtensionUi.Helpers;

    /// <summary>
    /// The items change observable collection.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class ItemsChangeObservableCollection<T> : ObservableCollection<T>
        where T : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// The model.
        /// </summary>
        private readonly IDataModel model;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsChangeObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="sqaleGridVm">
        /// The sqale grid vm.
        /// </param>
        public ItemsChangeObservableCollection(IDataModel model)
        {
            this.model = model;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The clear items.
        /// </summary>
        protected override void ClearItems()
        {
            this.UnRegisterPropertyChanged(this);
            base.ClearItems();
        }

        /// <summary>
        /// The on collection changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                this.RegisterPropertyChanged(e.NewItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                this.UnRegisterPropertyChanged(e.OldItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                this.UnRegisterPropertyChanged(e.OldItems);
                this.RegisterPropertyChanged(e.NewItems);
            }

            base.OnCollectionChanged(e);
        }

        /// <summary>
        /// The register property changed.
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        private void RegisterPropertyChanged(IList items)
        {
            foreach (INotifyPropertyChanged item in items)
            {
                if (item != null)
                {
                    item.PropertyChanged += this.item_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// The un register property changed.
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        private void UnRegisterPropertyChanged(IList items)
        {
            foreach (INotifyPropertyChanged item in items)
            {
                if (item != null)
                {
                    item.PropertyChanged -= this.item_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// The item_ property changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            this.model.ProcessChanges(sender, e);
        }

        #endregion
    }
}