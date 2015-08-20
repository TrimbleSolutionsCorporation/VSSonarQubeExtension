// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionConfiguration.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    ///     The connection configuration.
    /// </summary>
    [Serializable]
    public class ConnectionConfiguration : ISonarConfiguration
    {
        #region Fields

        /// <summary>
        ///     The hostname.
        /// </summary>
        private readonly string hostname;

        /// <summary>
        ///     The password.
        /// </summary>
        private readonly string password;

        /// <summary>
        ///     The username.
        /// </summary>
        private readonly string username;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionConfiguration" /> class.
        /// </summary>
        public ConnectionConfiguration()
        {
            this.hostname = string.Empty;
            this.username = string.Empty;
            this.IsOk = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionConfiguration" /> class.
        /// </summary>
        /// <param name="hostname">The hostname.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="sonarVersion">The sonar version.</param>
        public ConnectionConfiguration(string hostname, string username, string password, double sonarVersion)
        {
            this.hostname = hostname;
            this.password = password;
            this.username = username;
            this.SonarVersion = sonarVersion;

            this.IsOk = true;
            if (password == null)
            {
                this.password = string.Empty;
            }
            if (username == null)
            {
                this.username = string.Empty;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the hostname.
        /// </summary>
        public string Hostname
        {
            get
            {
                return this.hostname;
            }
        }

        /// <summary>
        ///     Gets the password.
        /// </summary>
        public string Password
        {
            get
            {
                return this.password;
            }
        }

        /// <summary>
        ///     Gets the username.
        /// </summary>
        public string Username
        {
            get
            {
                return this.username;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is ok.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is ok; otherwise, <c>false</c>.
        /// </value>
        public bool IsOk { get; set; }

        /// <summary>
        /// Gets the sonar version.
        /// </summary>
        /// <value>
        /// The sonar version.
        /// </value>
        public double SonarVersion { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The convert to secure string.
        /// </summary>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <returns>
        /// The <see cref="SecureString"/>.
        /// </returns>
        /// <exception>
        ///     <cref>ArgumentNullException</cref>
        /// </exception>
        public static SecureString ConvertToSecureString(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            var secure = new SecureString();
            foreach (char c in password)
            {
                secure.AppendChar(c);
            }

            return secure;
        }

        /// <summary>
        /// The convert to unsecure string.
        /// </summary>
        /// <param name="securePassword">
        /// The secure password.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception>
        ///     <cref>ArgumentNullException</cref>
        /// </exception>
        public static string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
            {
                throw new ArgumentNullException("securePassword");
            }

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        #endregion
    }
}