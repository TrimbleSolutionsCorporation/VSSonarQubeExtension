// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalAnalysisEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   The model expectes this argument to be sent refering the plugin key
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarPlugins
{
    using System;

    /// <summary>
    /// The model expectes this argument to be sent refering the plugin key
    /// </summary>
    public class LocalAnalysisEventArgs : EventArgs
    {
        /// <summary>
        /// The key.
        /// </summary>
        public readonly string Key;

        /// <summary>
        /// The error message.
        /// </summary>
        public readonly string ErrorMessage;

        /// <summary>
        /// The ex.
        /// </summary>
        public readonly Exception Ex;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAnalysisEventArgs"/> class.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="errorMessage">
        /// The error Message.
        /// </param>
        /// <param name="ex">
        /// The ex.
        /// </param>
        public LocalAnalysisEventArgs(string key, string errorMessage, Exception ex)
        {
            this.Key = key;
            this.Ex = ex;
            this.ErrorMessage = errorMessage;
        }
    }
}