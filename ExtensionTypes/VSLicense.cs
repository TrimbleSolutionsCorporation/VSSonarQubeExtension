// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CsLicensePair.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the CsLicensePair type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ExtensionTypes
{
    using System;

    using Rhino.Licensing;

    /// <summary>
    ///     The cs License pair.
    /// </summary>
    public class VsLicense
    {
        #region Public Properties
       
        /// <summary>
        /// Gets or sets a value indicating whether is valid.
        /// </summary>
        public string IsValid { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the product id.
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the server id.
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        ///     Gets or sets the License.
        /// </summary>
        public LicenseValidator LicenseData { get; set; }

        /// <summary>
        /// Gets or sets the license txt.
        /// </summary>
        public string LicenseTxt { get; set; }

        /// <summary>
        /// Gets or sets the license id.
        /// </summary>
        public string LicenseId { get; set; }

        /// <summary>
        /// Gets or sets the license name.
        /// </summary>
        public string LicenseName { get; set; }

        /// <summary>
        /// Gets or sets the license type.
        /// </summary>
        public string LicenseType { get; set; }

        /// <summary>
        /// Gets or sets the expire date.
        /// </summary>
        public DateTime ExpireDate { get; set; }

        #endregion
    }
}