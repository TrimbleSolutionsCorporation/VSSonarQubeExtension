// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PkgCmdIdList.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    /// <summary>
    /// The pkg cmd id list.
    /// </summary>
    public static class PkgCmdIdList
    {
        /// <summary>
        /// The cmdid reviews command.
        /// </summary>
        public const uint CmdidReviewsCommand = 0x145;

        /// <summary>
        /// The cmdid my command.
        /// </summary>
        public const uint ToolBarReportReviews = 0x200;

        /// <summary>
        /// The cmdid run analysis in solution.
        /// </summary>
        public const int CmdidRunAnalysisInSolution = 0x300;

        /// <summary>
        /// The cmdid run analysis in project.
        /// </summary>
        public const int CmdidRunAnalysisInProject = 0x0200;        
    }
}
