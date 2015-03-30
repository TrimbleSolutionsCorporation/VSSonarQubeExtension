// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionConfigurationHelpers.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarExtensionUi.Helpers
{  
    using SonarRestService;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// The connection configuration helpers.
    /// </summary>
    public static class AuthtenticationHelper
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public static string ErrorMessage { get; set; }

        public static ISonarConfiguration AuthToken { get; private set; }

        public static void ResetConnection() {
            AuthToken = null;
        }

        /// <summary>
        /// The get connection configuration.
        /// </summary>
        /// <param name="properties">
        /// The properties.
        /// </param>
        /// <param name="service">
        /// The service.
        /// </param>
        /// <returns>
        /// The <see cref="ConnectionConfiguration"/>.
        /// </returns>
        public static bool EstablishAConnection(ISonarRestService service, string address, string userName, string password)
        {
            if (AuthToken != null)
            {
                return true;
            }

            ErrorMessage = string.Empty;

            AuthToken = new ConnectionConfiguration(address, userName, password);
            if (!service.AuthenticateUser(AuthToken))
            {
                ErrorMessage = "Authentication Failed, Check User, Password and Hostname";
                return false;
            }

            return true;
        }
    }
}
