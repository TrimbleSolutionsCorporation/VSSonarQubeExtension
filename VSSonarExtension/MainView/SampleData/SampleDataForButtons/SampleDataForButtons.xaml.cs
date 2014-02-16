// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SampleDataForButtons.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarExtension.MainView.SampleData.SampleDataForButtons
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;

    // To significantly reduce the sample data footprint in your production application, you can set
    // the DISABLE_SAMPLE_DATA conditional compilation constant and disable sample data at runtime.
#if DISABLE_SAMPLE_DATA
    internal class SampleDataForButtons { }
#else

    /// <summary>
    /// The sample data for buttons.
    /// </summary>
    public class SampleDataForButtons : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// The _ collection.
        /// </summary>
        private readonly ItemCollection collection = new ItemCollection();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleDataForButtons"/> class.
        /// </summary>
        public SampleDataForButtons()
        {
            try
            {
                var resourceUri = new Uri(
                    "/ExtensionView;component/SampleData/SampleDataForButtons/SampleDataForButtons.xaml", 
                    UriKind.RelativeOrAbsolute);
                Application.LoadComponent(this, resourceUri);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        #endregion

        #region Public Events

        /// <summary>
        /// The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the collection.
        /// </summary>
        public ItemCollection Collection
        {
            get
            {
                return this.collection;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

    /// <summary>
    ///     The item collection.
    /// </summary>
    public class ItemCollection : ObservableCollection<Item>
    {
    }

#endif
}