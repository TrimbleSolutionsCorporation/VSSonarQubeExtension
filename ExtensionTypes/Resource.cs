// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Resource.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections.Generic;

    /// <summary>
    /// The resource.
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Resource"/> class.
        /// </summary>
        public Resource()
        {
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the l name.
        /// </summary>
        public string Lname { get; set; }

        /// <summary>
        /// Gets or sets the scopre.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the qualifier.
        /// </summary>
        public string Qualifier { get; set; }

        /// <summary>
        /// Gets or sets the lang.
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the Metrics
        /// </summary>
        public List<Metric> Metrics { get; set; }

        /// <summary>
        /// Gets or sets the non safe key.
        /// </summary>
        public string NonSafeKey { get; set; }

        /// <summary>
        /// Gets or sets the active configuration.
        /// </summary>
        public string ActiveConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the active platform.
        /// </summary>
        public string ActivePlatform { get; set; }

        /// <summary>
        /// Gets or sets the solution root.
        /// </summary>
        public string SolutionRoot { get; set; }
    }
}
