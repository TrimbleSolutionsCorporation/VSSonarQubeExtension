namespace VSSonarPlugins.Types
{
    using System;

    /// <summary>
    /// Action Plan
    /// </summary>
    public class SonarActionPlan
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        public string Project { get; set; }

        /// <summary>
        /// Gets or sets the user login.
        /// </summary>
        /// <value>
        /// The user login.
        /// </value>
        public string UserLogin { get; set; }

        /// <summary>
        /// Gets or sets the dead line.
        /// </summary>
        /// <value>
        /// The dead line.
        /// </value>
        public DateTime DeadLine { get; set; }

        /// <summary>
        /// Gets or sets the total issues.
        /// </summary>
        /// <value>
        /// The total issues.
        /// </value>
        public int TotalIssues { get; set; }

        /// <summary>
        /// Gets or sets the unresolved issues.
        /// </summary>
        /// <value>
        /// The unresolved issues.
        /// </value>
        public int UnresolvedIssues { get; set; }

        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>
        /// The created at.
        /// </value>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at.
        /// </summary>
        /// <value>
        /// The updated at.
        /// </value>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name plus project key.
        /// </summary>
        /// <value>
        /// The name plus project.
        /// </value>
        public string NamePlusProject { get; set; }
    }
}