// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiffResultSpanStatus.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    /// The diff result span status.
    /// </summary>
    public enum DiffResultSpanStatus
    {
        /// <summary>
        /// The no change.
        /// </summary>
        NoChange, 

        /// <summary>
        /// The replace.
        /// </summary>
        Replace, 

        /// <summary>
        /// The delete source.
        /// </summary>
        DeleteSource, 

        /// <summary>
        /// The add destination.
        /// </summary>
        AddDestination
    }
}