namespace ExtensionTypes
{
    using System;

    /// <summary>
    /// The sub characteristics.
    /// </summary>
    [Serializable]
    public class SubCharacteristics
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SubCharacteristics"/> class.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public SubCharacteristics(SubCategory key, string name)
        {
            this.Key = key;
            this.Name = name;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public SubCategory Key { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        #endregion
    }
}