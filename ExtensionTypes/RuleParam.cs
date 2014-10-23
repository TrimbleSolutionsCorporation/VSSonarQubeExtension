// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleParam.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

    /// <summary>
    /// The rule param.
    /// </summary>
    [Serializable]
    public class RuleParam
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        public int DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the desc.
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key { get; set; }

        #endregion
    }
}