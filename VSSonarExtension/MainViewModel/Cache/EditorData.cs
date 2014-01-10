// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorData.cs" company="">
//   
// </copyright>
// <summary>
//   The editor data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtension.MainViewModel.Cache
{
    using System.Collections.Generic;

    using ExtensionTypes;

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