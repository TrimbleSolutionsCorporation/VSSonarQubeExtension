namespace VSSonarPlugins.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class Exclusion
    {
        /// <summary>
        /// Gets or sets the rule regx.
        /// </summary>
        /// <value>
        /// The rule regx.
        /// </value>
        public string RuleRegx { get; set; }

        /// <summary>
        /// Gets or sets the file regx.
        /// </summary>
        /// <value>
        /// The file regx.
        /// </value>
        public string FileRegx { get; set; }
    }
}
