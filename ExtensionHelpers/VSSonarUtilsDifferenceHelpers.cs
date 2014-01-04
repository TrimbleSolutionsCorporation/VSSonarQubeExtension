// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSSonarUtilsDifferenceHelpers.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DiffCalc;

    using DifferenceEngine;
    using ExtensionTypes;

    /// <summary>
    /// The vs sonar utils.
    /// </summary>
    public static partial class VsSonarUtils
    {
        /// <summary>
        /// The get source diff from strings.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="sourceseparator">
        /// The sourceseparator.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="destinationsepartor">
        /// The destinationsepartor.
        /// </param>
        /// <param name="level">
        /// The level.
        /// </param>
        /// <returns>
        /// The System.Collections.ArrayList.
        /// </returns>
        public static ArrayList GetSourceDiffFromStrings(string source, string destination, DiffEngineLevel level = DiffEngineLevel.SlowPerfect)
        {
            var sourceseparator = "\n";
            if (source.Contains("\r\n"))
            {
                sourceseparator = "\r\n";
            }

            var destinationsepartor = "\n";
            if (destination.Contains("\r\n"))
            {
                destinationsepartor = "\r\n";
            }

            var sLf = new DiffListTextFile(source, sourceseparator);
            var dLf = new DiffListTextFile(destination, destinationsepartor);
            var de = new DiffEngine();
            de.ProcessDiff(sLf, dLf, level);
            return de.DiffReport();
        }

        /// <summary>
        /// The convert issues to local.
        /// </summary>
        /// <param name="issuesIn">
        /// The issues.
        /// </param>
        /// <param name="currentResource">
        /// The current resource.
        /// </param>
        /// <param name="currentBufferData">
        /// The current buffer data.
        /// </param>
        /// <param name="lastReferenceSource">
        /// The last reference source.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static List<Issue> ConvertIssuesToLocal(List<Issue> issuesIn, Resource currentResource, string currentBufferData, string lastReferenceSource)
        {
            var diffReport = GetSourceDiffFromStrings(lastReferenceSource, currentBufferData, DiffEngineLevel.FastImperfect);
            return ConvertOpenIssuesToLocalSource(issuesIn, diffReport, currentResource);
        }

        /// <summary>
        /// The convert violations to local.
        /// </summary>
        /// <param name="issueList">
        /// The issue list.
        /// </param>
        /// <param name="diffReport">
        /// The diff report.
        /// </param>
        /// <param name="currentResource">
        /// The current Resource.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>ObservableCollection</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static List<Issue> ConvertOpenIssuesToLocalSource(List<Issue> issueList, ArrayList diffReport, Resource currentResource)
        {
            var local = new List<Issue>();

            if (issueList != null && issueList.Count > 0)
            {
                foreach (var issue in issueList)
                {
                    if (!issue.Component.EndsWith(currentResource.Lname))
                    {
                        continue;
                    }

                    if (issue.Status != "OPEN" && issue.Status != "REOPENED" && !string.IsNullOrEmpty(issue.Status))
                    {
                        continue;
                    }

                    var copyOfViolation = issue.DeepCopy();
                    var convertedKey = EstimateWhereSonarLineIsUsingSourceDifference(issue.Line, diffReport);
                    
                    if (convertedKey < 0)
                    {
                        continue;
                    }

                    copyOfViolation.Line = convertedKey;
                    local.Add(copyOfViolation);
                }             
            }

            return new List<Issue>(local.OrderBy(i => i.Line));
        }

        /// <summary>
        /// The get difference report.
        /// </summary>
        /// <param name="filePath">
        /// The file Path.
        /// </param>
        /// <param name="sourceInServer">
        /// The in server source.
        /// </param>
        /// <param name="displayWindow">
        /// The display Window.
        /// </param>
        /// <returns>
        /// The <see cref="ArrayList"/>.
        /// </returns>
        public static ArrayList GetDifferenceReport(string filePath, string sourceInServer, bool displayWindow)
        {
            var sLf = new DiffListTextFile(filePath);
            var dLf = new DiffListTextFile(sourceInServer, "\r\n");

            var de = new DiffEngine();
            de.ProcessDiff(dLf, sLf, DiffEngineLevel.SlowPerfect);

            if (!displayWindow)
            {
                return de.DiffReport();
            }

            using (var win = new Results(sLf, dLf, de.DiffReport(), 0))
            {
                win.ShowDialog();
            }

            return de.DiffReport();
        }
    }
}
