// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiffListBinaryFile.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.IO;

    /// <summary>
    /// The diff list_ binary file.
    /// </summary>
    public class DiffListBinaryFile : IDiffList
    {
        #region Fields

        /// <summary>
        /// The _byte list.
        /// </summary>
        private readonly byte[] byteList;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffListBinaryFile"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        public DiffListBinaryFile(string fileName)
        {
            FileStream fs = null;
            BinaryReader br = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                var len = (int)fs.Length;
                br = new BinaryReader(fs);
                this.byteList = br.ReadBytes(len);
            }
            finally
            {
                if (br != null)
                {
                    br.Close();
                }

                if (fs != null)
                {
                    fs.Close();
                }
            }
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
            return this.byteList.Length;
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
            return this.byteList[index];
        }

        #endregion
    }
}