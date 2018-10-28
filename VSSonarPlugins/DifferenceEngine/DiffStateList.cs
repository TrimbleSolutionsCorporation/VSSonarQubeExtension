// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiffStateList.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    /// <summary>
    /// The diff state list.
    /// </summary>
    internal class DiffStateList
    {
#if USE_HASH_TABLE
		private Hashtable _table;
#else

        /// <summary>
        /// The _array.
        /// </summary>
        private readonly DiffState[] array;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffStateList"/> class.
        /// </summary>
        /// <param name="destCount">
        /// The dest count.
        /// </param>
        public DiffStateList(int destCount)
        {
#if USE_HASH_TABLE
			_table = new Hashtable(Math.Max(9, destCount/10));
#else
            this.array = new DiffState[destCount];
#endif
        }

        /// <summary>
        /// The get by index.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <returns>
        /// The <see cref="DiffState"/>.
        /// </returns>
        public DiffState GetByIndex(int index)
        {
#if USE_HASH_TABLE
			DiffState retval = (DiffState)_table[index];
			if (retval == null)
			{
				retval = new DiffState();
				_table.Add(index, retval);
			}
#else
            DiffState retval = this.array[index];
            if (retval == null)
            {
                retval = new DiffState();
                this.array[index] = retval;
            }

#endif
            return retval;
        }
    }
}