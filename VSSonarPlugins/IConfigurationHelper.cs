namespace VSSonarPlugins
{
    using System.Collections.Generic;
    using Types;

    /// <summary>The ConfigurationHelper interface. All settings should be acessed via this interface.</summary>
    public interface IConfigurationHelper
    {
        /// <summary>The write configuration.</summary>
        /// <param name="context">The context.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="SonarQubeProperties"/>.</returns>
        SonarQubeProperties ReadSetting(Context context, string owner, string key);

        /// <summary>The read settings.</summary>
        /// <param name="context">The context.</param>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        IEnumerable<SonarQubeProperties> ReadSettings(Context context, string owner);

        /// <summary>The write setting.</summary>
        /// <param name="prop">The prop.</param>
        /// <param name="sync">The sync.</param>
        /// <param name="skipIfExist">The skip if exist.</param>
        void WriteSetting(SonarQubeProperties prop, bool sync = false, bool skipIfExist = false);

        /// <summary>The sync settings.</summary>
        void SyncSettings();

        /// <summary>The clear non saved settings.</summary>
        void ClearNonSavedSettings();

        /// <summary>The delete settings file.</summary>
        void DeleteSettingsFile();

        /// <summary>
        ///     The get user app data configuration file.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        string UserAppDataConfigurationFile();

        /// <summary>The user log for analysis file.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        string UserLogForAnalysisFile();

        /// <summary>
        /// Gets or sets the application path.
        /// </summary>
        /// <value>
        /// The application path.
        /// </value>
        string ApplicationPath { get; set; }

        /// <summary>The write option in application data.</summary>
        /// <param name="context">The context.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="sync">The sync.</param>
        /// <param name="skipIfExist">The skip if exist.</param>
        void WriteOptionInApplicationData(
        Context context,
        string owner,
        string key,
        string value,
        bool sync = false,
        bool skipIfExist = false);
    }
}