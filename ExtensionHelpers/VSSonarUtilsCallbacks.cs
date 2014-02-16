// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSSonarUtilsCallbacks.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
namespace ExtensionHelpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using DifferenceEngine;

    using ExtensionTypes;

    /// <summary>
    /// The vs sonar utils.
    /// </summary>
    public static partial class VsSonarUtils
    {
        /// <summary>
        /// The get added violations from source difference.
        /// </summary>
        /// <param name="issuesIn">
        /// The issues In.
        /// </param>
        /// <param name="diffReport">
        /// The diff report.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>ObservableCollection</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static List<Issue> GetIssuesInModifiedLinesOnly(List<Issue> issuesIn, ArrayList diffReport)
        {
            var addedViolations = new List<Issue>();

            if (diffReport != null)
            {
                foreach (DiffResultSpan change in diffReport)
                {
                    if (change.Status == DiffResultSpanStatus.Replace)
                    {
                        addedViolations.AddRange(issuesIn.Where(issue => change.DestIndex == issue.Line));
                    }

                    if (change.Status != DiffResultSpanStatus.AddDestination)
                    {
                        continue;
                    }

                    for (var i = change.DestIndex; i < change.DestIndex + change.Length; i++)
                    {
                        addedViolations.AddRange(issuesIn.Where(issue => i == issue.Line));
                    }
                }
            }

            return addedViolations;
        }

        /// <summary>
        /// The estimate where violation is using source differce.
        /// </summary>
        /// <param name="sonarLine">
        /// The sonar Line.
        /// </param>
        /// <param name="currentDiffReport">
        /// The current Diff Report.
        /// </param>
        /// <returns>
        /// Returns -1 if violation line is removed or modified locally. &gt; 0 otherwise
        /// </returns>
        public static int EstimateWhereSonarLineIsUsingSourceDifference(int sonarLine, ArrayList currentDiffReport)
        {
            var line = sonarLine;

            try
            {
                var differenceSpan = GetChangeForLine(line, currentDiffReport);
                if (differenceSpan == null)
                {
                    return -1;
                }

                int j = 0;
                for (int i = differenceSpan.SourceIndex; i < differenceSpan.SourceIndex + differenceSpan.Length; i++)
                {
                    if (line - 1 == i)
                    {
                        return differenceSpan.DestIndex + j + 1;
                    }
                    ++j;
                }

                line = -1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                line = -1;
            }

            return line;
        }

        /// <summary>
        /// The get change for line.
        /// </summary>
        /// <param name="line">
        /// The violation.
        /// </param>
        /// <param name="rep">
        /// The rep.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public static DiffResultSpan GetChangeForLine(int line, ArrayList rep)
        {
            var fixLine = line - 1;

            if (rep != null && line >= 0)
            {
                foreach (DiffResultSpan currentdiff in rep)
                {
                    if (fixLine >= currentdiff.SourceIndex && fixLine < currentdiff.SourceIndex + currentdiff.Length)
                    {
                        if (currentdiff.Status != DiffResultSpanStatus.DeleteSource && currentdiff.Status != DiffResultSpanStatus.Replace)
                        {
                            return currentdiff;
                        }
                    }
                }
            }

            return null;
        }
    }
}
