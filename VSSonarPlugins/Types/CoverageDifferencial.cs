using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSonarPlugins.Types
{
    public class CoverageDifferencial
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
        public int UncoveredConditons { get; set; }

        /// <summary>
        /// Gets or sets the uncovered lines.
        /// </summary>
        /// <value>
        /// The uncovered lines.
        /// </value>
        public int UncoveredLines { get; set; }

        /// <summary>
        /// Gets or sets the new coverage.
        /// </summary>
        /// <value>
        /// The new coverage.
        /// </value>
        public decimal NewCoverage { get; set; }
    }
}
