// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorData.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2014 [Jorge Costa, Jorge.Costa@tekla.com]
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

namespace VSSonarExtensionUi.Cache
{
    using System.Collections.Generic;
    using VSSonarPlugins.Types;

    

    /// <summary>
    /// The editor data.
    /// </summary>
    public class EditorData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorData"/> class.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        public EditorData(Resource resource)
        {
            this.CoverageData = new Dictionary<int, CoverageElement>();
            this.Issues = new List<Issue>();
            this.Resource = resource;
        }

        #region Public Properties

        /// <summary>
        /// Gets or sets the coverage data.
        /// </summary>
        public Dictionary<int, CoverageElement> CoverageData { get; set; }

        /// <summary>
        /// Gets or sets the issues.
        /// </summary>
        public List<Issue> Issues { get; set; }

        /// <summary>
        /// Gets or sets the server source.
        /// </summary>
        public string ServerSource { get; set; }

        /// <summary>
        /// Gets the resource.
        /// </summary>
        public Resource Resource { get; private set; }

        #endregion
    }
}