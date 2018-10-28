// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextLine.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// All credit goes to http://www.codeproject.com/Articles/6943/A-Generic-Reusable-Diff-Algorithm-in-C-II#_comments
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------
namespace DifferenceEngine
{
    using System;

    /// <summary>
    ///     The text line.
    /// </summary>
    public class TextLine : IComparable
    {
        #region Fields

        /// <summary>
        ///     The _hash.
        /// </summary>
        private readonly int hash;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TextLine"/> class.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        public TextLine(string str)
        {
            this.Line = str.Replace("\t", "    ");
            this.hash = str.GetHashCode();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the line.
        /// </summary>
        public string Line { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The compare to.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int CompareTo(object obj)
        {
            return this.hash.CompareTo(((TextLine)obj).hash);
        }

        #endregion
    }
}