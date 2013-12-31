// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelEditorCache.cs" company="">
//   
// </copyright>
// <summary>
//   The coverage element.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExtensionViewModel.Cache
{
    using System.Collections.Generic;

    using DifferenceEngine;

    using ExtensionHelpers;

    using ExtensionTypes;

    /// <summary>
    /// The coverage element.
    /// </summary>
    public enum CoverageElement
    {
        /// <summary>
        /// The covered.
        /// </summary>
        LineCovered, 

        /// <summary>
        /// The no t_ covered.
        /// </summary>
        LineNotCovered, 

        /// <summary>
        /// The partiall y_ covered.
        /// </summary>
        LinePartialCovered
    }

    /// <summary>
    /// The model editor cache.
    /// </summary>
    public class ModelEditorCache
    {
        /// <summary>
        /// Gets or sets the coverage data.
        /// </summary>
        private static Dictionary<string, EditorData> data = new Dictionary<string, EditorData>();

        #region Public Properties

        #endregion

        /// <summary>
        /// The update coverage data.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <param name="coverageData">
        /// The coverage data.
        /// </param>
        /// <param name="issues">
        /// The issues.
        /// </param>
        /// <param name="serverSource">
        /// The server source.
        /// </param>
        public void UpdateResourceData(Resource resource, SourceCoverage coverageData, List<Issue> issues, string serverSource)
        {
            if (!data.ContainsKey(resource.Key))
            {
                data.Add(resource.Key, new EditorData(resource));
            }

            var elem = data[resource.Key];
            elem.ServerSource = serverSource;
            this.UpdateCoverageData(elem, coverageData);
            this.UpdateIssuesData(elem, issues);
        }

        /// <summary>
        /// The get coverage data for resource.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <param name="currentSource">
        /// The current source.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary"/>.
        /// </returns>
        public Dictionary<int, CoverageElement> GetCoverageDataForResource(Resource resource, string currentSource)
        {
            var coverageLine = new Dictionary<int, CoverageElement>();

            if (!data.ContainsKey(resource.Key))
            {
                return coverageLine;
            }

            var element = data[resource.Key];

            var diffReport = VsSonarUtils.GetSourceDiffFromStrings(
                element.ServerSource, 
                "\r\n",
                currentSource, 
                "\r\n", 
                DiffEngineLevel.FastImperfect);

            foreach (var linecov in element.CoverageData)
            {
                var line = VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(linecov.Key, diffReport);

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
        /// The <see>
        ///     <cref>List</cref>
        /// </see>
        ///     .
        /// </returns>
        public List<Issue> GetIssuesForResource(Resource resource, string currentSource)
        {
            var issues = new List<Issue>();

            if (!data.ContainsKey(resource.Key))
            {
                return issues;
            }

            var element = data[resource.Key];

            if (element.ServerSource == null)
            {
                return element.Issues;
            }

            var diffReport = VsSonarUtils.GetSourceDiffFromStrings(
                element.ServerSource,
                "\r\n",
                currentSource,
                "\r\n",
                DiffEngineLevel.FastImperfect);

            foreach (var issue in element.Issues)
            {
                var line = VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(issue.Line, diffReport);

                if (line >= 0)
                {
                    issues.Add(issue);
                }
            }

            return issues;
        }

        public List<Issue> GetIssuesForResource(Resource resource)
        {
            var issues = new List<Issue>();

            if (resource == null)
            {
                return issues;
            }

            if (!data.ContainsKey(resource.Key))
            {
                return issues;
            }

            var element = data[resource.Key];

            foreach (var issue in element.Issues)
            {
                issues.Add(issue);
            }

            return issues;
        }

        /// <summary>
        /// The clear data.
        /// </summary>
        public void ClearData()
        {
            data.Clear();
        }

        /// <summary>
        /// The is data updated.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsDataUpdated(Resource resource)
        {
            if (!data.ContainsKey(resource.Key) || resource.Date > data[resource.Key].Resource.Date)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The update coverage data.
        /// </summary>
        /// <param name="elem">
        /// The elem.
        /// </param>
        /// <param name="coverageData">
        /// The coverage data.
        /// </param>
        private void UpdateCoverageData(EditorData elem, SourceCoverage coverageData)
        {
            if (coverageData == null)
            {
                return;
            }

            var tempCoverageLine = new Dictionary<int, CoverageElement>();

            foreach (var linemeasure in coverageData.LinesHits)
            {
                var line = linemeasure.Id - 1;

                tempCoverageLine.Add(line, linemeasure.Hits > 0 ? CoverageElement.LineCovered : CoverageElement.LineNotCovered);
            }

            foreach (var branchmeasure in coverageData.BranchHits)
            {
                var line = branchmeasure.Id - 1;

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
        /// The update issues data.
        /// </summary>
        /// <param name="elem">
        /// The elem.
        /// </param>
        /// <param name="issues">
        /// The issues.
        /// </param>
        private void UpdateIssuesData(EditorData elem, IEnumerable<Issue> issues)
        {
            elem.Issues.Clear();
            elem.Issues.AddRange(issues);
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
            return data.ContainsKey(resource.Key) ? data[resource.Key].ServerSource : string.Empty;
        }

        internal List<Issue> GetIssues()
        {
            var issues = new List<Issue>();
            foreach (var editorData in data)
            {
                issues.AddRange(editorData.Value.Issues);                
            }

            return issues;
        }

        internal void UpdateIssues(List<Issue> issues)
        {
            this.ClearData();

            if (issues == null)
            {
                return;
            }

            foreach (var issue in issues)
            {
                if (!data.ContainsKey(issue.Component))
                {
                    var resource = new Resource();
                    resource.Key = issue.Component;
                    data.Add(issue.Component, new EditorData(new Resource()));
                }

                var elem = data[issue.Component];
                elem.Issues.Add(issue);
            }
        }
    }
}