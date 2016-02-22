using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSonarPlugins.Types;

namespace VSSonarExtensionUi.ViewModel.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Search
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [componenets enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [componenets enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool ComponenetsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the components.
        /// </summary>
        /// <value>
        /// The components.
        /// </value>
        public List<Resource> Components { get; set; }

        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        /// <value>
        /// The users.
        /// </value>
        public List<User> Assignees { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [assignees enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [assignees enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool AssigneesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the reporters.
        /// </summary>
        /// <value>
        /// The reporters.
        /// </value>
        public List<User> Reporters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [reporters enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [reporters enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool ReportersEnabled { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public List<IssueStatus> Status { get; set; }

        /// <summary>
        /// Gets or sets the severities.
        /// </summary>
        /// <value>
        /// The severities.
        /// </value>
        public List<Severity> Severities { get; set; }

        /// <summary>
        /// Gets or sets the resolutions.
        /// </summary>
        /// <value>
        /// The resolutions.
        /// </value>
        public List<Resolution> Resolutions { get; set; }

        /// <summary>
        /// Gets or sets the before date.
        /// </summary>
        /// <value>
        /// The before date.
        /// </value>
        public DateTime BeforeDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [before date enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [before date enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool BeforeDateEnabled { get; set; }

        /// <summary>
        /// Gets or sets the since date.
        /// </summary>
        /// <value>
        /// The since date.
        /// </value>
        public DateTime SinceDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [since date enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [since date enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool SinceDateEnabled { get; set; }
    }
}
