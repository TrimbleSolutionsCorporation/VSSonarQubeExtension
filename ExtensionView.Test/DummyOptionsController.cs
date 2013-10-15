// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyOptionsController.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

using System.ComponentModel;

namespace ExtensionView.Test
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using VSSonarPlugins;

    /// <summary>
    /// The dummy options controller.
    /// </summary>
    public class DummyOptionsController : IPluginsOptions, INotifyPropertyChanged
    {
        /// <summary>
        /// The text box.
        /// </summary>
        private string textBox;

        public DummyOptionsController()
        {
            textBox = "sdsasd";
        }

        private UserControl dummyControl = null;

        /// <summary>
        /// The get user control options.
        /// </summary>
        /// <returns>
        /// The <see cref="UserControl"/>.
        /// </returns>
        public UserControl GetUserControlOptions()
        {
            if (this.dummyControl == null)
            {
                this.dummyControl = new DummyUserControl();
            }

            return this.dummyControl;
        }

        /// <summary>
        /// Gets or sets the text box.
        /// </summary>
        public string TextBox
        {
            get
            {
                return this.textBox;
            }

            set
            {
                this.textBox = value;
                this.OnPropertyChanged("TextBox");
            }
        }

        /// <summary>
        /// The get options.
        /// </summary>
        /// <returns>
        /// The <see>
        ///     <cref>Dictionary</cref>
        /// </see></returns>
        public Dictionary<string, string> GetOptions()
        {
            var options = new Dictionary<string, string>();
            options.Add("TextBox", this.textBox);
            return options;
        }

        /// <summary>
        /// The set options.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void SetOptions(Dictionary<string, string> options)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled()
        {
            throw new NotImplementedException();
        }

        public void ResetDefaults()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}