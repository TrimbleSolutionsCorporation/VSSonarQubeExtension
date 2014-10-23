// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Characteristic.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace ExtensionTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     The characteristic.
    /// </summary>
    [Serializable]
    public class Characteristic
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Characteristic"/> class.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public Characteristic(Category key, string name)
        {
            this.Key = key;
            this.Name = name;
            this.Subchars = new List<SubCharacteristics>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the key.
        /// </summary>
        public Category Key { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the subchars.
        /// </summary>
        public List<SubCharacteristics> Subchars { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create sub char.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public void CreateSubChar(SubCategory key, string name)
        {
            if (this.Subchars.Any(subCharacteristicse => subCharacteristicse.Key.Equals(key)))
            {
                return;
            }

            this.Subchars.Add(new SubCharacteristics(key, name));
        }

        /// <summary>
        /// The is sub char present.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsSubCharPresent(SubCategory key)
        {
            return this.Subchars.Any(subCharacteristicse => subCharacteristicse.Key.Equals(key));
        }

        #endregion
    }
}