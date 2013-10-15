// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PkgCmdIdList.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.VSControls
{
    /// <summary>
    /// The pkg cmd id list.
    /// </summary>
    public static class PkgCmdIdList
    {
        /// <summary>
        /// The cmdid sonar server violations command.
        /// </summary>
        public const uint CmdidSonarServerViolationsCommand = 0x100;

        /// <summary>
        /// The cmdid local violations from changes command.
        /// </summary>
        public const uint CmdidLocalViolationsFromChangesCommand = 0x110;

        /// <summary>
        /// The cmdid coverage command.
        /// </summary>
        public const uint CmdidCoverageCommand = 0x120;

        /// <summary>
        /// The cmdid source diff command.
        /// </summary>
        public const uint CmdidSourceDiffCommand = 0x130;

        /// <summary>
        /// The cmdid local analysis command.
        /// </summary>
        public const uint CmdidLocalAnalysisCommand = 0x140;

        /// <summary>
        /// The cmdid reviews command.
        /// </summary>
        public const uint CmdidReviewsCommand = 0x145;

        /// <summary>
        /// The cmdid my command.
        /// </summary>
        public const uint ToolBarReportSonarViolations = 0x150;

        /// <summary>
        /// The cmdid my command.
        /// </summary>
        public const uint ToolBarReportLocalViolations = 0x160;

        /// <summary>
        /// The cmdid my command.
        /// </summary>
        public const uint ToolBarReportLocalAddViolations = 0x170;

        /// <summary>
        /// The cmdid my command.
        /// </summary>
        public const uint ToolBarReportCoverage = 0x180;

        /// <summary>
        /// The cmdid my command.
        /// </summary>
        public const uint ToolBarReportSource = 0x190;

        /// <summary>
        /// The cmdid my command.
        /// </summary>
        public const uint ToolBarReportReviews = 0x200;
    }
}
