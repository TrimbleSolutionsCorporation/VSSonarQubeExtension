namespace VSSonarPlugins
{
    using SonarRestService.Types;
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
        SonarQubeProperties ReadSetting(Types.Context context, string owner, string key);

		/// <summary>
		/// helper function to get string element, with default value
		/// </summary>
		/// <param name="context"></param>
		/// <param name="id"></param>
		/// <param name="key"></param>
		/// <param name="elementToSet"></param>
		/// <param name="defaultValue"></param>
		void ReadInSetting(Context context, string id, string key, out string elementToSet, string defaultValue);
		
		/// <summary>
		/// The write setting.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="owner">The owner.</param>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="sync">The sync.</param>
		/// <param name="skipIfExist">The skip if exist.</param>
		void WriteSetting(Types.Context context, string owner, string key, string value, bool sync = false, bool skipIfExist = false);

        void WriteSetting(SonarQubeProperties prop, bool sync = false, bool skipIfExist = false);

        /// <summary>The sync settings.</summary>
        void SyncSettings();

        /// <summary>The delete settings file.</summary>
        void ResetAllSettings();

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
	}
}