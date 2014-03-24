// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarQubeProperties.cs" company="">
//   
// </copyright>
// <summary>
//   The sonar qube properties.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarPlugins
{
    /// <summary>
    ///     The sonar qube properties.
    /// </summary>
    public class SonarQubeProperties
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeProperties"/> class.
        /// </summary>
        public SonarQubeProperties()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeProperties"/> class.
        /// </summary>
        /// <param name="prop">
        /// The prop.
        /// </param>
        public SonarQubeProperties(SonarQubeProperties prop)
        {
            this.Key = prop.Key;
            this.Value = prop.Value;
            this.ValueInServer = prop.ValueInServer;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the vera arguments.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Gets or sets the value in server.
        /// </summary>
        public string ValueInServer { get; set; }

        #endregion
    }
}