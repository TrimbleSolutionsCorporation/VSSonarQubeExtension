// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISonarConfiguration.cs" company="">
//   
// </copyright>
// <summary>
//   The SonarConnector interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExtensionTypes
{
    /// <summary>
    /// The SonarConnector interface.
    /// </summary>
    public interface ISonarConfiguration
    {
        /// <summary>
        /// Gets the hostname.
        /// </summary>
        string Hostname { get; }

        /// <summary>
        /// Gets the username.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        string Password { get; }
    }
}