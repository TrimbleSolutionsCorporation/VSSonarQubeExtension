// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSLicense.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarPlugins.Types
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