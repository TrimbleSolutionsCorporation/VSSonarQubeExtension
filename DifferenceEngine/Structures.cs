// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Structures.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Diagnostics;

    /// <summary>
    /// The diff status.
    /// </summary>
    internal enum DiffStatus
    {
        /// <summary>
        /// The matched.
        /// </summary>
        Matched = 1,

        /// <summary>
        /// The no match.
        /// </summary>
        NoMatch = -1,

        /// <summary>
        /// The unknown.
        /// </summary>
        Unknown = -2
    }

    /// <summary>
    /// The DiffList interface.
    /// </summary>
    public interface IDiffList
    {
        #region Public Methods and Operators

        /// <summary>
        /// The count.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        int Count();

        /// <summary>
        /// The get by index.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <returns>
        /// The <see cref="IComparable"/>.
        /// </returns>
        IComparable GetByIndex(int index);

        #endregion
    }

    /// <summary>
    /// The diff state.
    /// </summary>
    internal class DiffState
    {
        #region Constants

        /// <summary>
        /// The ba d_ index.
        /// </summary>
        private const int BadIndex = -1;

        #endregion

        #region Fields

        /// <summary>
        /// The _length.
        /// </summary>
        private int length;

        /// <summary>
        /// The _start index.
        /// </summary>
        private int startIndex;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffState"/> class.
        /// </summary>
        public DiffState()
        {
            this.SetToUnkown();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the end index.
        /// </summary>
        public int EndIndex
        {
            get
            {
                return (this.startIndex + this.length) - 1;
            }
        }

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int Length
        {
            get
            {
                int len;
                if (this.length > 0)
                {
                    len = this.length;
                }
                else
                {
                    if (this.length == 0)
                    {
                        len = 1;
                    }
                    else
                    {
                        len = 0;
                    }
                }

                return len;
            }
        }

        /// <summary>
        /// Gets the start index.
        /// </summary>
        public int StartIndex
        {
            get
            {
                return this.startIndex;
            }
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public DiffStatus Status
        {
            get
            {
                DiffStatus stat;
                if (this.length > 0)
                {
                    stat = DiffStatus.Matched;
                }
                else
                {
                    switch (this.length)
                    {
                        case -1:
                            stat = DiffStatus.NoMatch;
                            break;
                        default:
                            Debug.Assert(this.length == -2, "Invalid status: _length < -2");
                            stat = DiffStatus.Unknown;
                            break;
                    }
                }

                return stat;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The has valid length.
        /// </summary>
        /// <param name="newStart">
        /// The new start.
        /// </param>
        /// <param name="newEnd">
        /// The new end.
        /// </param>
        /// <param name="maxPossibleDestLength">
        /// The max possible dest length.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool HasValidLength(int newStart, int newEnd, int maxPossibleDestLength)
        {
            if (this.length > 0)
            {
                // have unlocked match
                if ((maxPossibleDestLength < this.length)
                    || ((this.startIndex < newStart) || (this.EndIndex > newEnd)))
                {
                    this.SetToUnkown();
                }
            }

            return this.length != (int)DiffStatus.Unknown;
        }

        /// <summary>
        /// The set match.
        /// </summary>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="lengthIn">
        /// The length.
        /// </param>
        public void SetMatch(int start, int lengthIn)
        {
            Debug.Assert(lengthIn > 0, "Length must be greater than zero");
            Debug.Assert(start >= 0, "Start must be greater than or equal to zero");
            this.startIndex = start;
            this.length = lengthIn;
        }

        /// <summary>
        /// The set no match.
        /// </summary>
        public void SetNoMatch()
        {
            this.startIndex = BadIndex;
            this.length = (int)DiffStatus.NoMatch;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The set to unkown.
        /// </summary>
        protected void SetToUnkown()
        {
            this.startIndex = BadIndex;
            this.length = (int)DiffStatus.Unknown;
        }

        #endregion
    }
}