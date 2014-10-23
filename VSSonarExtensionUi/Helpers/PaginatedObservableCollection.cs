// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PaginatedObservableCollection.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    /// <summary>
    /// The paginated observable collection.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class PaginatedObservableCollection<T> : ObservableCollection<T>
    {
        #region Fields

        /// <summary>
        /// The original collection.
        /// </summary>
        private readonly List<T> originalCollection;

        /// <summary>
        /// The _current page index.
        /// </summary>
        private int _currentPageIndex;

        /// <summary>
        /// The _item count per page.
        /// </summary>
        private int _itemCountPerPage;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="collecton">
        /// The collecton.
        /// </param>
        public PaginatedObservableCollection(IEnumerable<T> collecton)
        {
            this._currentPageIndex = 0;
            this._itemCountPerPage = 1;
            this.originalCollection = new List<T>(collecton);
            this.RecalculateThePageItems();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="itemsPerPage">
        /// The items per page.
        /// </param>
        public PaginatedObservableCollection(int itemsPerPage)
        {
            this._currentPageIndex = 0;
            this._itemCountPerPage = itemsPerPage;
            this.originalCollection = new List<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedObservableCollection{T}"/> class.
        /// </summary>
        public PaginatedObservableCollection()
        {
            this._currentPageIndex = 0;
            this._itemCountPerPage = 1;
            this.originalCollection = new List<T>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        public int CurrentPage
        {
            get
            {
                return this._currentPageIndex;
            }

            set
            {
                if (value >= 0)
                {
                    this._currentPageIndex = value;
                    this.RecalculateThePageItems();
                    this.OnPropertyChanged(new PropertyChangedEventArgs("CurrentPage"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize
        {
            get
            {
                return this._itemCountPerPage;
            }

            set
            {
                if (value >= 0)
                {
                    this._itemCountPerPage = value;
                    this.RecalculateThePageItems();
                    this.OnPropertyChanged(new PropertyChangedEventArgs("PageSize"));
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The insert item.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        protected override void InsertItem(int index, T item)
        {
            int startIndex = this._currentPageIndex * this._itemCountPerPage;
            int endIndex = startIndex + this._itemCountPerPage;

            // Check if the Index is with in the current Page then add to the collection as bellow. And add to the originalCollection also
            if ((index >= startIndex) && (index < endIndex))
            {
                base.InsertItem(index - startIndex, item);

                if (this.Count > this._itemCountPerPage)
                {
                    base.RemoveItem(endIndex);
                }
            }

            if (index >= this.Count)
            {
                this.originalCollection.Add(item);
            }
            else
            {
                this.originalCollection.Insert(index, item);
            }
        }

        /// <summary>
        /// The remove item.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        protected override void RemoveItem(int index)
        {
            int startIndex = this._currentPageIndex * this._itemCountPerPage;
            int endIndex = startIndex + this._itemCountPerPage;

            // Check if the Index is with in the current Page range then remove from the collection as bellow. And remove from the originalCollection also
            if ((index >= startIndex) && (index < endIndex))
            {
                this.RemoveAt(index - startIndex);

                if (this.Count <= this._itemCountPerPage)
                {
                    base.InsertItem(endIndex - 1, this.originalCollection[index + 1]);
                }
            }

            this.originalCollection.RemoveAt(index);
        }

        /// <summary>
        /// The recalculate the page items.
        /// </summary>
        private void RecalculateThePageItems()
        {
            this.Clear();

            int startIndex = this._currentPageIndex * this._itemCountPerPage;

            for (int i = startIndex; i < startIndex + this._itemCountPerPage; i++)
            {
                if (this.originalCollection.Count > i)
                {
                    base.InsertItem(i - startIndex, this.originalCollection[i]);
                }
            }
        }

        #endregion
    }
}