// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionConfigurationHelpers.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
namespace ExtensionHelpers
{
    using ExtensionTypes;

    using SonarRestService;

    /// <summary>
    /// The connection configuration helpers.
    /// </summary>
    public class ConnectionConfigurationHelpers
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public static string ErrorMessage { get; set; }

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
        public static ConnectionConfiguration GetConnectionConfiguration(IVsEnvironmentHelper properties, ISonarRestService service)
        {
            ErrorMessage = string.Empty;

            var userName = "admin";
            var userPassword = "admin";
            var hostname = "http://localhost:9000";
            if (properties != null)
            {
                userName = properties.ReadSavedOption("Sonar Options", "General", "SonarUserName");
                userPassword = properties.ReadSavedOption("Sonar Options", "General", "SonarUserPassword");
                hostname = properties.ReadSavedOption("Sonar Options", "General", "SonarHost");
            }

            if (string.IsNullOrEmpty(hostname))
            {
                ErrorMessage = "User Configuration is Invalid, Check Tools > Options > Sonar Options";
                return null;
            }

            var userConf = new ConnectionConfiguration(hostname, userName, userPassword);
            if (!service.AuthenticateUser(userConf))
            {
                ErrorMessage = "Authentication Failed, Check User, Password and Hostname";
                return null;
            }

            return userConf;
        }

        /// <summary>
        /// The get connection configuration.
        /// </summary>
        /// <param name="properties">
        /// The properties.
        /// </param>
        /// <returns>
        /// The <see cref="ConnectionConfiguration"/>.
        /// </returns>
        public static ConnectionConfiguration GetConnectionConfiguration(IVsEnvironmentHelper properties)
        {
            ErrorMessage = string.Empty;

            var userName = "admin";
            var userPassword = "admin";
            var hostname = "http://localhost:9000";
            if (properties != null)
            {
                userName = properties.ReadSavedOption("Sonar Options", "General", "SonarUserName");
                userPassword = properties.ReadSavedOption("Sonar Options", "General", "SonarUserPassword");
                hostname = properties.ReadSavedOption("Sonar Options", "General", "SonarHost");
            }

            if (string.IsNullOrEmpty(hostname))
            {
                ErrorMessage = "User Configuration is Invalid, Check Tools > Options > Sonar Options";
                return null;
            }

            var userConf = new ConnectionConfiguration(hostname, userName, userPassword);
            return userConf;
        }
    }
}
