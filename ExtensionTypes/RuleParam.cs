namespace ExtensionTypes
{
    using System;

    /// <summary>
    /// The rule param.
    /// </summary>
    [Serializable]
    public class RuleParam
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        public int DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the desc.
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key { get; set; }

        #endregion
    }
}