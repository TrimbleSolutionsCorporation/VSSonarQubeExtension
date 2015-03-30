// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarQubeProperties.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarPlugins.Types
{
    using System;

    /// <summary>
    ///     The sonar qube properties.
    /// </summary>
    [Serializable]
    public class SonarQubeProperties
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeProperties"/> class.
        /// </summary>
        public SonarQubeProperties()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeProperties"/> class.
        /// </summary>
        /// <param name="prop">
        /// The prop.
        /// </param>
        public SonarQubeProperties(SonarQubeProperties prop)
        {
            this.Key = prop.Key;
            this.Value = prop.Value;
            this.Context = prop.Context;
            this.Owner = prop.Owner;
        }

        public SonarQubeProperties(Context context, string owner, string key, string value)
        {
            this.Key = key;
            this.Value = value;
            this.Context = context;
            this.Owner = owner;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the vera arguments.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Gets or sets the value in server.
        /// </summary>
        public Context Context { get; set; }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        public string Owner { get; set; }

        #endregion
    }
}