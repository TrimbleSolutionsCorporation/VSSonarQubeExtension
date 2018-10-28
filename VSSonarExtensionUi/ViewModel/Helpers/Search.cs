namespace VSSonarExtensionUi.ViewModel.Helpers
{
    using System;
    using System.Collections.Generic;
    using VSSonarPlugins.Types;

    using SonarRestService.Types;
    using SonarRestService;

    /// <summary>
    /// search class
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
        /// Gets or sets a value indicating whether [components enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [components enabled]; otherwise, <c>false</c>.
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

        /// <summary>
        /// Gets or sets a value indicating whether [filter by SSCM].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [filter by SSCM]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterBySSCM { get; set; }

        /// <summary>
        /// Gets the authors.
        /// </summary>
        /// <value>
        /// The authors.
        /// </value>
        public string Authors { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether [authors enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [authors enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool AuthorsEnabled { get; internal set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>
        /// The tags.
        /// </value>
        public string Tags { get; set; }

        /// <summary>
        /// Gets a value indicating whether [tag search enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [tag search enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool TagSearchEnabled { get; internal set; }
    }
}
