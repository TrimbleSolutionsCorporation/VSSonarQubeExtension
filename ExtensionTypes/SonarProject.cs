// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarProject.cs" company="">
//   
// </copyright>
// <summary>
//   The sonar project.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExtensionTypes
{
    using System.Collections.Generic;

    using PropertyChanged;

    /// <summary>
    /// The sonar project.
    /// </summary>
    [ImplementPropertyChanged]
    public class SonarProject
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarProject"/> class.
        /// </summary>
        public SonarProject()
        {
            this.Profiles = new List<Profile>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the profiles.
        /// </summary>
        public List<Profile> Profiles { get; set; }

        /// <summary>
        /// Gets or sets the qualifier.
        /// </summary>
        public string Qualifier { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        public string Scope { get; set; }

        #endregion
    }
}