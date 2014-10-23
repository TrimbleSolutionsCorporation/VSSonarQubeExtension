// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncObservableCollection.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2014 [Jorge Costa, Jorge.Costa@tekla.com]
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

namespace VSSonarExtensionUi.Helpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// The async observable collection.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// The model.
        /// </summary>
        private readonly IDataModel model;

        #region Fields

        /// <summary>
        /// The _synchronization context.
        /// </summary>
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public AsyncObservableCollection(IDataModel model)
        {
            this.model = model;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        public AsyncObservableCollection(IEnumerable<T> list)
            : base(list)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on collection changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (SynchronizationContext.Current == this.synchronizationContext)
            {
                // Execute the CollectionChanged event on the current thread
                this.RaiseCollectionChanged(e);
            }
            else
            {
                try
                {
                    // Raises the CollectionChanged event on the creator thread
                    this.synchronizationContext.Send(this.RaiseCollectionChanged, e);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Message: " + ex.Message);
                }
            }

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
        }

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (SynchronizationContext.Current == this.synchronizationContext)
            {
                // Execute the PropertyChanged event on the current thread
                this.RaisePropertyChanged(e);
            }
            else
            {
                // Raises the PropertyChanged event on the creator thread
                this.synchronizationContext.Send(this.RaisePropertyChanged, e);
            }
        }

        /// <summary>
        /// The clear items.
        /// </summary>
        protected override void ClearItems()
        {
            this.UnRegisterPropertyChanged(this);
            base.ClearItems();
        }

        /// <summary>
        /// The raise collection changed.
        /// </summary>
        /// <param name="param">
        /// The param.
        /// </param>
        private void RaiseCollectionChanged(object param)
        {
            // We are in the creator thread, call the base implementation directly
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
        }

        /// <summary>
        /// The raise property changed.
        /// </summary>
        /// <param name="param">
        /// The param.
        /// </param>
        private void RaisePropertyChanged(object param)
        {
            // We are in the creator thread, call the base implementation directly
            base.OnPropertyChanged((PropertyChangedEventArgs)param);
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
                    item.PropertyChanged += this.ItemPropertyChanged;
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
                    item.PropertyChanged -= this.ItemPropertyChanged;
                }
            }
        }

        /// <summary>
        /// The item property changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {          
            this.model.ProcessChanges(sender, e);
        }

        #endregion
    }
}