namespace VSSonarPlugins.Types
{
    /// <summary>
    /// CoverageReport helper
    /// </summary>
    public class CoverageReport
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the resource.
        /// </summary>
        /// <value>
        /// The resource.
        /// </value>
        public Resource resource { get; set; }

        /// <summary>
        /// Gets or sets the uncovered conditons.
        /// </summary>
        /// <value>
        /// The uncovered conditons.
        /// </value>
        public long LinesOfCode { get; set; }

        /// <summary>
        /// Gets or sets the uncovered lines.
        /// </summary>
        /// <value>
        /// The uncovered lines.
        /// </value>
        public long NewLines { get; set; }

        /// <summary>
        /// Gets or sets the new coverage.
        /// </summary>
        /// <value>
        /// The new coverage.
        /// </value>
        public decimal NewCoverage { get; set; }

        /// <summary>
        /// Gets or sets the coverage.
        /// </summary>
        /// <value>
        /// The coverage.
        /// </value>
        public decimal Coverage { get; set; }
    }
}
