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
namespace VSSonarExtensionUi.Model.Helpers
{
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

        /// <summary>
        /// Gets the authentication token.
        /// </summary>
        /// <value>
        /// The authentication token.
        /// </value>
        public static ISonarConfiguration AuthToken { get; private set; }

        /// <summary>
        /// Resets the connection.
        /// </summary>
        public static void ResetConnection()
        {
            AuthToken = null;
        }

        /// <summary>
        /// The get connection configuration.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="address">The address.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        /// The <see cref="ConnectionConfiguration" />.
        /// </returns>
        public static bool EstablishAConnection(ISonarRestService service, string address, string userName, string password)
        {
            if (AuthToken != null)
            {
                return true;
            }

            ErrorMessage = string.Empty;

            var token = new ConnectionConfiguration(address, userName, password, 4.5);
            if (!service.AuthenticateUser(token))
            {
                ErrorMessage = "Authentication Failed, Check User, Password and Hostname";
                return false;
            }

            token.SonarVersion = service.GetServerInfo(token);
            AuthToken = token;

            return true;
        }
    }
}
