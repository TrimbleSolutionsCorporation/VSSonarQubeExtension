// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiffListCharData.cs" company="Copyright � 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    /// The diff list_ char data.
    /// </summary>
    public class DiffListCharData : IDiffList
    {
        #region Fields

        /// <summary>
        /// The _char list.
        /// </summary>
        private readonly char[] charList;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffListCharData"/> class.
        /// </summary>
        /// <param name="charData">
        /// The char data.
        /// </param>
        public DiffListCharData(string charData)
        {
            this.charList = charData.ToCharArray();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The count.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int Count()
        {
            return this.charList.Length;
        }

        /// <summary>
        /// The get by index.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <returns>
        /// The <see cref="IComparable"/>.
        /// </returns>
        public IComparable GetByIndex(int index)
        {
            return this.charList[index];
        }

        #endregion
    }
}