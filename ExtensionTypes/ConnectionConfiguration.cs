// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionConfiguration.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
namespace ExtensionTypes
{
    /// <summary>
    /// The connection configuration.
    /// </summary>
    public class ConnectionConfiguration
    {
        /// <summary>
        /// The hostname.
        /// </summary>
        private readonly string hostname;

        /// <summary>
        /// The username.
        /// </summary>
        private readonly string username;

        /// <summary>
        /// The password.
        /// </summary>
        private readonly string password;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionConfiguration"/> class.
        /// </summary>
        public ConnectionConfiguration()
        {
            this.password = string.Empty;
            this.hostname = string.Empty;
            this.username = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionConfiguration"/> class.
        /// </summary>
        /// <param name="hostname">
        /// The hostname.
        /// </param>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        public ConnectionConfiguration(string hostname, string username, string password)
        {
            this.password = password;
            this.hostname = hostname;
            this.username = username;
        }

        /// <summary>
        /// Gets the hostname.
        /// </summary>
        public string Hostname
        {
            get { return this.hostname; }
        }

        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username
        {
            get { return this.username; }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password
        {
            get { return this.password; }
        }
    }
}
