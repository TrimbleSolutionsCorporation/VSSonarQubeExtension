// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelEditorCache.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarExtensionUi.Cache
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DifferenceEngine;

    using ExtensionHelpers;

    using ExtensionTypes;

    /// <summary>
    ///     The coverage element.
    /// </summary>
    public enum CoverageElement
    {
        /// <summary>
        ///     The covered.
        /// </summary>
        LineCovered, 

        /// <summary>
        ///     The no t_ covered.
        /// </summary>
        LineNotCovered, 

        /// <summary>
        ///     The partiall y_ covered.
        /// </summary>
        LinePartialCovered
    }

    /// <summary>
    ///     The model editor cache.
    /// </summary>
    public class ModelEditorCache
    {
        #region Static Fields

        /// <summary>
        ///     Gets or sets the coverage DataCache.
        /// </summary>
        private static readonly Dictionary<string, VSSonarExtensionUi.Cache.EditorData> DataCache = new Dictionary<string, VSSonarExtensionUi.Cache.EditorData>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The clear DataCache.
        /// </summary>
        public void ClearData()
        {
            DataCache.Clear();
        }

        /// <summary>
        /// The get coverage DataCache for resource.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <param name="currentSource">
        /// The current source.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public Dictionary<int, CoverageElement> GetCoverageDataForResource(Resource resource, string currentSource)
        {
            var coverageLine = new Dictionary<int, CoverageElement>();

            if (!DataCache.ContainsKey(resource.Key))
            {
                return coverageLine;
            }

            VSSonarExtensionUi.Cache.EditorData element = DataCache[resource.Key];

            ArrayList diffReport = VsSonarUtils.GetSourceDiffFromStrings(element.ServerSource, currentSource, DiffEngineLevel.FastImperfect);

            foreach (var linecov in element.CoverageData)
            {
                int line = VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(linecov.Key, diffReport);

                if (line > 0 && !coverageLine.ContainsKey(line))
                {
                    coverageLine.Add(line, linecov.Value);
                }
            }

            return coverageLine;
        }

        /// <summary>
        /// The get issues for resource.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <param name="currentSource">
        /// The current source.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public List<Issue> GetIssuesForResource(Resource resource, string currentSource)
        {
            var issues = new List<Issue>();

            if (!DataCache.ContainsKey(resource.Key))
            {
                return issues;
            }

            VSSonarExtensionUi.Cache.EditorData element = DataCache[resource.Key];

            if (element.ServerSource == null)
            {
                return element.Issues;
            }

            try
            {
                ArrayList diffReport = VsSonarUtils.GetSourceDiffFromStrings(element.ServerSource, currentSource, DiffEngineLevel.FastImperfect);

                foreach (Issue issue in element.Issues)
                {
                    int line = VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(issue.Line, diffReport);

                    if (line >= 0)
                    {
                        issues.Add(issue);
                    }
                }

                return issues;
            }
            catch (Exception)
            {
                return element.Issues;
            }
        }

        /// <summary>
        /// The get issues for resource.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public List<Issue> GetIssuesForResource(Resource resource)
        {
            var issues = new List<Issue>();

            if (resource == null)
            {
                return issues;
            }

            if (DataCache.ContainsKey(resource.Key) || DataCache.ContainsKey(resource.NonSafeKey))
            {
                VSSonarExtensionUi.Cache.EditorData element = null;

                if (DataCache.ContainsKey(resource.Key))
                {
                    element = DataCache[resource.Key];
                }
                else
                {
                    element = DataCache[resource.NonSafeKey];
                }

                foreach (Issue issue in element.Issues)
                {
                    issues.Add(issue);
                }
            }

            return issues;
        }

        /// <summary>
        /// The is DataCache updated.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsDataUpdated(Resource resource)
        {
            if (!DataCache.ContainsKey(resource.Key) || resource.Date > DataCache[resource.Key].Resource.Date)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The update coverage DataCache.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <param name="coverageData">
        /// The coverage DataCache.
        /// </param>
        /// <param name="issues">
        /// The issues.
        /// </param>
        /// <param name="serverSource">
        /// The server source.
        /// </param>
        public void UpdateResourceData(Resource resource, SourceCoverage coverageData, List<Issue> issues, string serverSource)
        {
            if (!DataCache.ContainsKey(resource.Key))
            {
                DataCache.Add(resource.Key, new VSSonarExtensionUi.Cache.EditorData(resource));
            }

            VSSonarExtensionUi.Cache.EditorData elem = DataCache[resource.Key];
            elem.ServerSource = serverSource;
            this.UpdateCoverageData(elem, coverageData);
            this.UpdateIssuesData(elem, issues);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The get issues.
        /// </summary>
        /// <returns>
        ///     The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        internal List<Issue> GetIssues()
        {
            var issues = new List<Issue>();
            foreach (var editorData in DataCache)
            {
                issues.AddRange(editorData.Value.Issues);
            }

            return issues;
        }

        /// <summary>
        /// The get source for resource.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        internal string GetSourceForResource(Resource resource)
        {
            return DataCache.ContainsKey(resource.Key) ? DataCache[resource.Key].ServerSource : string.Empty;
        }

        /// <summary>
        /// The update issues.
        /// </summary>
        /// <param name="issues">
        /// The issues.
        /// </param>
        internal void UpdateIssues(List<Issue> issues)
        {
            this.ClearData();

            if (issues == null)
            {
                return;
            }

            foreach (Issue issue in issues)
            {
                if (!DataCache.ContainsKey(issue.Component))
                {
                    var resource = new Resource();
                    resource.Key = issue.Component;
                    DataCache.Add(issue.Component, new VSSonarExtensionUi.Cache.EditorData(new Resource()));
                }

                VSSonarExtensionUi.Cache.EditorData elem = DataCache[issue.Component];
                elem.Issues.Add(issue);
            }
        }

        /// <summary>
        /// The update coverage DataCache.
        /// </summary>
        /// <param name="elem">
        /// The elem.
        /// </param>
        /// <param name="coverageData">
        /// The coverage DataCache.
        /// </param>
        private void UpdateCoverageData(VSSonarExtensionUi.Cache.EditorData elem, SourceCoverage coverageData)
        {
            if (coverageData == null)
            {
                return;
            }

            var tempCoverageLine = new Dictionary<int, CoverageElement>();

            foreach (LineCoverage linemeasure in coverageData.LinesHits)
            {
                int line = linemeasure.Id - 1;

                tempCoverageLine.Add(line, linemeasure.Hits > 0 ? CoverageElement.LineCovered : CoverageElement.LineNotCovered);
            }

            foreach (BranchCoverage branchmeasure in coverageData.BranchHits)
            {
                int line = branchmeasure.Id - 1;

                if (tempCoverageLine.ContainsKey(line))
                {
                    if (branchmeasure.CoveredConditions == branchmeasure.TotalConditions)
                    {
                        tempCoverageLine[line] = CoverageElement.LineCovered;
                    }
                    else if (branchmeasure.CoveredConditions == 0)
                    {
                        tempCoverageLine[line] = CoverageElement.LineNotCovered;
                    }
                    else
                    {
                        tempCoverageLine[line] = CoverageElement.LinePartialCovered;
                    }
                }
                else
                {
                    if (branchmeasure.CoveredConditions == branchmeasure.TotalConditions)
                    {
                        tempCoverageLine.Add(line, CoverageElement.LineCovered);
                    }
                    else if (branchmeasure.CoveredConditions == 0)
                    {
                        tempCoverageLine.Add(line, CoverageElement.LineNotCovered);
                    }
                    else
                    {
                        tempCoverageLine.Add(line, CoverageElement.LinePartialCovered);
                    }
                }
            }

            elem.CoverageData = tempCoverageLine;
        }

        /// <summary>
        /// The update issues DataCache.
        /// </summary>
        /// <param name="elem">
        /// The elem.
        /// </param>
        /// <param name="issues">
        /// The issues.
        /// </param>
        private void UpdateIssuesData(VSSonarExtensionUi.Cache.EditorData elem, IEnumerable<Issue> issues)
        {
            elem.Issues.Clear();
            if (issues != null)
            {
                elem.Issues.AddRange(issues);
            }            
        }

        #endregion
    }
}