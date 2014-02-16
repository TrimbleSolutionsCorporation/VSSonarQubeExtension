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

namespace VSSonarExtension.Test.TestView
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Controls;

    using ExtensionTypes;

    using VSSonarPlugins;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    public class DummyOptionsController : IPluginsOptions, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// The dummy control.
        /// </summary>
        private UserControl dummyControl;

        /// <summary>
        ///     The text box.
        /// </summary>
        private string textBox;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DummyOptionsController" /> class.
        /// </summary>
        public DummyOptionsController()
        {
            this.textBox = "sdsasd";
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the text box.
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

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The get options.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        /// </returns>
        public Dictionary<string, string> GetOptions()
        {
            var options = new Dictionary<string, string>();
            options.Add("TextBox", this.textBox);
            return options;
        }

        /// <summary>
        ///     The get user control options.
        /// </summary>
        /// <returns>
        ///     The <see cref="UserControl" />.
        /// </returns>
        public UserControl GetUserControlOptions()
        {
            if (this.dummyControl == null)
            {
                this.dummyControl = new UserControl();
            }

            return this.dummyControl;
        }

        /// <summary>
        /// The get user control options.
        /// </summary>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <returns>
        /// The <see cref="UserControl"/>.
        /// </returns>
        public UserControl GetUserControlOptions(Resource project)
        {
            return null;
        }

        /// <summary>
        /// The is enabled.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsEnabled()
        {
            return true;
        }

        /// <summary>
        /// The reset defaults.
        /// </summary>
        public void ResetDefaults()
        {
        }

        /// <summary>
        /// The set options.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        public void SetOptions(Dictionary<string, string> options)
        {
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
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}