// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VsSonarExtensionLogger.cs" company="">
//   
// </copyright>
// <summary>
//   The logger.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.ViewModel.Helpers
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    ///     The logger.
    /// </summary>
    public class VsSonarExtensionLogger
    {
        #region Fields

        /// <summary>
        ///     The model.
        /// </summary>
        private readonly SonarQubeViewModel model;

        /// <summary>
        ///     The user roaming file.
        /// </summary>
        private readonly string userRoamingFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "VSSonarExtension\\debug.log");

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VsSonarExtensionLogger"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public VsSonarExtensionLogger(SonarQubeViewModel model)
        {
            this.model = model;

            if (File.Exists(this.userRoamingFile))
            {
                File.Delete(this.userRoamingFile);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The write exception.
        /// </summary>
        /// <param name="ex">
        /// The ex.
        /// </param>
        public void WriteException(Exception ex)
        {
            if (!this.model.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.ExtensionDebugModeEnabled)
            {
                return;
            }

            try
            {
                using (var writer = new StreamWriter(this.userRoamingFile, true))
                {
                    writer.WriteLineAsync(ex.Message + " : " + ex.StackTrace);
                }
            }
            catch (Exception error)
            {
                Debug.WriteLine(error.Message);
            }
        }

        /// <summary>
        /// The write message.
        /// </summary>
        /// <param name="msg">
        /// The msg.
        /// </param>
        public void WriteMessage(string msg)
        {
            if (!this.model.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.ExtensionDebugModeEnabled)
            {
                return;
            }

            try
            {
                using (var writer = new StreamWriter(this.userRoamingFile, true))
                {
                    writer.WriteLineAsync(msg);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion
    }
}