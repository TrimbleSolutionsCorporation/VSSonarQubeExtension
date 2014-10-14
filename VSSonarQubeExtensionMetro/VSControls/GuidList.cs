// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuidList.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarQubeExtension.VSControls
{
    using System;

    /// <summary>
    /// The guid list.
    /// </summary>
    public static class GuidList
    {
        /// <summary>
        /// The guid vs sonar extension pkg string.
        /// </summary>
        public const string GuidVSSonarExtensionPkgString = "84feee6a-374a-4c3d-a2e6-15e14fd5000e";

        /// <summary>
        /// The guid vs sonar extension cmd set string.
        /// </summary>
        public const string GuidVSSonarExtensionCmdSetString = "05ca2046-1eb1-4813-a91f-a69df9b27698";

        /// <summary>
        /// The guid vs sonar extension cmd set.
        /// </summary>
        public static readonly Guid GuidVSSonarExtensionCmdSet = new Guid(GuidVSSonarExtensionCmdSetString);

        /// <summary>
        /// The guid menu and commands cmd set.
        /// </summary>
        public static readonly Guid GuidMenuAndCommandsCmdSet = new Guid("{19492BCB-32B3-4EC3-8826-D67CD5526653}");

        /// <summary>
        /// The guid show initial toolbar cmd set.
        /// </summary>
        public static readonly Guid GuidShowInitialToolbarCmdSet = new Guid("{91489787-3361-4715-bb3a-76dbd86353dc}");

        /// <summary>
        /// The guid start analysis solution ctx cmd set.
        /// </summary>
        public static readonly Guid GuidStartAnalysisSolutionCTXCmdSet = new Guid("{7FFF1BD4-5BFD-4F5C-9EBC-52AFE79D7BFE}");
    }
}
