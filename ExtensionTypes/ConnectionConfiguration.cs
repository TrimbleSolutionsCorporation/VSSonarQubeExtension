// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionConfiguration.cs" company="">
//   
// </copyright>
// <summary>
//   The connection configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ExtensionTypes
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