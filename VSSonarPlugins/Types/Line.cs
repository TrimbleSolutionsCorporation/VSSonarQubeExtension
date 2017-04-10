// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Line.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
using System;

namespace VSSonarPlugins.Types
{
    /// <summary>
    /// The source.
    /// </summary>
    public class Line
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is executable line.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is executable line; otherwise, <c>false</c>.
        /// </value>
        public bool IsExecutableLine { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the val.
        /// </summary>
        public string Val { get; set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the SCM revision.
        /// </summary>
        /// <value>
        /// The SCM revision.
        /// </value>
        public string ScmRevision { get; set; }

        /// <summary>
        /// Gets or sets the SCM author.
        /// </summary>
        /// <value>
        /// The SCM author.
        /// </value>
        public string ScmAuthor { get; set; }

        /// <summary>
        /// Gets or sets the SCM date.
        /// </summary>
        /// <value>
        /// The SCM date.
        /// </value>
        public DateTime ScmDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Line"/> is duplicated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if duplicated; otherwise, <c>false</c>.
        /// </value>
        public bool Duplicated { get; set; }

        /// <summary>
        /// Gets or sets the ut lines.
        /// </summary>
        /// <value>
        /// The ut lines.
        /// </value>
        public int LineHits { get; set; }

        /// <summary>
        /// Gets or sets the conditions.
        /// </summary>
        /// <value>
        /// The conditions.
        /// </value>
        public int Conditions { get; set; }

        /// <summary>
        /// Gets or sets the covered conditions.
        /// </summary>
        /// <value>
        /// The covered conditions.
        /// </value>
        public int CoveredConditions { get; set; }
    }
}
