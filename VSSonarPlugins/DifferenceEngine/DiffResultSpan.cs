// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiffResultSpan.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    /// The diff result span.
    /// </summary>
    public class DiffResultSpan : IComparable
    {
        #region Constants

        /// <summary>
        /// The ba d_ index.
        /// </summary>
        private const int BadIndex = -1;

        #endregion

        #region Fields

        /// <summary>
        /// The _dest index.
        /// </summary>
        private readonly int destIndex;

        /// <summary>
        /// The _source index.
        /// </summary>
        private readonly int sourceIndex;

        /// <summary>
        /// The _status.
        /// </summary>
        private readonly DiffResultSpanStatus status;

        /// <summary>
        /// The _length.
        /// </summary>
        private int length;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffResultSpan"/> class.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="destIndex">
        /// The dest index.
        /// </param>
        /// <param name="sourceIndex">
        /// The source index.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        protected DiffResultSpan(DiffResultSpanStatus status, int destIndex, int sourceIndex, int length)
        {
            this.status = status;
            this.destIndex = destIndex;
            this.sourceIndex = sourceIndex;
            this.length = length;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the dest index.
        /// </summary>
        public int DestIndex
        {
            get
            {
                return this.destIndex;
            }
        }

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int Length
        {
            get
            {
                return this.length;
            }
        }

        /// <summary>
        /// Gets the source index.
        /// </summary>
        public int SourceIndex
        {
            get
            {
                return this.sourceIndex;
            }
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public DiffResultSpanStatus Status
        {
            get
            {
                return this.status;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create add destination.
        /// </summary>
        /// <param name="destIndex">
        /// The dest index.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// The <see cref="DiffResultSpan"/>.
        /// </returns>
        public static DiffResultSpan CreateAddDestination(int destIndex, int length)
        {
            return new DiffResultSpan(DiffResultSpanStatus.AddDestination, destIndex, BadIndex, length);
        }

        /// <summary>
        /// The create delete source.
        /// </summary>
        /// <param name="sourceIndex">
        /// The source index.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// The <see cref="DiffResultSpan"/>.
        /// </returns>
        public static DiffResultSpan CreateDeleteSource(int sourceIndex, int length)
        {
            return new DiffResultSpan(DiffResultSpanStatus.DeleteSource, BadIndex, sourceIndex, length);
        }

        /// <summary>
        /// The create no change.
        /// </summary>
        /// <param name="destIndex">
        /// The dest index.
        /// </param>
        /// <param name="sourceIndex">
        /// The source index.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// The <see cref="DiffResultSpan"/>.
        /// </returns>
        public static DiffResultSpan CreateNoChange(int destIndex, int sourceIndex, int length)
        {
            return new DiffResultSpan(DiffResultSpanStatus.NoChange, destIndex, sourceIndex, length);
        }

        /// <summary>
        /// The create replace.
        /// </summary>
        /// <param name="destIndex">
        /// The dest index.
        /// </param>
        /// <param name="sourceIndex">
        /// The source index.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// The <see cref="DiffResultSpan"/>.
        /// </returns>
        public static DiffResultSpan CreateReplace(int destIndex, int sourceIndex, int length)
        {
            return new DiffResultSpan(DiffResultSpanStatus.Replace, destIndex, sourceIndex, length);
        }

        /// <summary>
        /// The add length.
        /// </summary>
        /// <param name="i">
        /// The i.
        /// </param>
        public void AddLength(int i)
        {
            this.length += i;
        }

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
            return this.destIndex.CompareTo(((DiffResultSpan)obj).destIndex);
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                "{0} (Dest: {1},Source: {2}) {3}", 
                this.status, 
                this.destIndex, 
                this.sourceIndex, 
                this.length);
        }

        #endregion
    }
}