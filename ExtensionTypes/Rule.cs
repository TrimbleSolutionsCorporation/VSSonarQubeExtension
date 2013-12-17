// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rule.cs" company="">
//   
// </copyright>
// <summary>
//   The rule.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ExtensionTypes
{
    /// <summary>
    ///     The rule.
    /// </summary>
    public class Rule
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the config key.
        /// </summary>
        public string ConfigKey { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the remediation factor function.
        /// </summary>
        public string RemediationFactorFunction { get; set; }

        /// <summary>
        /// Gets or sets the remediation factor txt.
        /// </summary>
        public string RemediationFactorTxt { get; set; }

        /// <summary>
        /// Gets or sets the remediation factor val.
        /// </summary>
        public string RemediationFactorVal { get; set; }

        /// <summary>
        ///     Gets or sets the repo.
        /// </summary>
        public string Repo { get; set; }

        /// <summary>
        ///     Gets or sets the severity.
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// Gets or sets the sub category.
        /// </summary>
        public string SubCategory { get; set; }

        #endregion
    }
}