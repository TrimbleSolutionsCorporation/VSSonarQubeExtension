// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Characteristic.cs" company="">
//   
// </copyright>
// <summary>
//   The sub characteristics.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ExtensionTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     The characteristic.
    /// </summary>
    [Serializable]
    public class Characteristic
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Characteristic"/> class.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public Characteristic(Category key, string name)
        {
            this.Key = key;
            this.Name = name;
            this.Subchars = new List<SubCharacteristics>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the key.
        /// </summary>
        public Category Key { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the subchars.
        /// </summary>
        public List<SubCharacteristics> Subchars { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create sub char.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public void CreateSubChar(SubCategory key, string name)
        {
            if (this.Subchars.Any(subCharacteristicse => subCharacteristicse.Key.Equals(key)))
            {
                return;
            }

            this.Subchars.Add(new SubCharacteristics(key, name));
        }

        /// <summary>
        /// The is sub char present.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsSubCharPresent(SubCategory key)
        {
            return this.Subchars.Any(subCharacteristicse => subCharacteristicse.Key.Equals(key));
        }

        #endregion
    }
}