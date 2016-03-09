// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationHelper.cs" company="">
//   
// </copyright>
// <summary>
//   The configuration helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtensionUi.Model.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// The configuration helper.
    /// </summary>
    public class ConfigurationHelper : IConfigurationHelper
    {
        /// <summary>
        /// The tempproperties
        /// </summary>
        private readonly List<SonarQubeProperties> tempproperties = new List<SonarQubeProperties>();

        /// <summary>
        /// The manager
        /// </summary>
        private readonly INotificationManager manager;

        /// <summary>
        /// The properties
        /// </summary>
        private List<SonarQubeProperties> properties = new List<SonarQubeProperties>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationHelper" /> class.
        /// </summary>
        /// <param name="vsversion">The vsversion.</param>
        /// <param name="manager">The manager.</param>
        public ConfigurationHelper(string vsversion, INotificationManager manager)
        {
            this.manager = manager;
            this.ApplicationDataUserSettingsFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                                   + "\\VSSonarExtension\\settings.cfg." + vsversion;

            this.LogForAnalysis = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                  + "\\VSSonarExtension\\temp\\analysisLog.txt." + vsversion;

            this.ApplicationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "VSSonarExtension");

            if (!Directory.Exists(this.ApplicationPath))
            {
                Directory.CreateDirectory(this.ApplicationPath);
            }

            if (!Directory.Exists(Directory.GetParent(this.ApplicationDataUserSettingsFile).ToString()))
            {
                Directory.CreateDirectory(Directory.GetParent(this.ApplicationDataUserSettingsFile).ToString());
            }

            this.SyncSettings();
        }

        public string ApplicationPath { get; set; }

        /// <summary>
        ///     Gets or sets the application data user settings file.
        /// </summary>
        public string ApplicationDataUserSettingsFile { get; set; }

        /// <summary>
        /// Gets or sets the log for analysis.
        /// </summary>
        public string LogForAnalysis { get; set; }

        /// <summary>
        ///     The get user app data configuration file.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string UserLogForAnalysisFile()
        {
            return this.LogForAnalysis;
        }

        /// <summary>
        /// Writes the option in application data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="sync">if set to <c>true</c> [synchronize].</param>
        /// <param name="skipIfExist">if set to <c>true</c> [skip if exist].</param>
        public void WriteOptionInApplicationData(
            Context context,
            string owner,
            string key,
            string value,
            bool sync = false,
            bool skipIfExist = false)
        {
            this.WriteSetting(new SonarQubeProperties(context, owner, key, value), sync, skipIfExist);
        }

        /// <summary>
        /// Reads the setting.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="key">The key.</param>
        /// <returns>Valid property.</returns>
        /// <exception cref="Exception">
        /// Property not found:  + key
        /// or
        /// Property not found:  + key
        /// </exception>
        public SonarQubeProperties ReadSetting(Context context, string owner, string key)
        {
            if (context.Equals(Context.AnalysisProject))
            {
                foreach (var property in this.tempproperties)
                {
                    if (property.Context.Equals(context) && property.Owner.Equals(owner) && property.Key.Equals(key))
                    {
                        return property;
                    }
                }

                throw new Exception("Property not found: " + key);
            }

            foreach (var property in this.properties)
            {
                if (property.Context.Equals(context) && property.Owner.Equals(owner) && property.Key.Equals(key))
                {
                    return property;
                }
            }

            throw new Exception("Property not found: " + key);
        }

        /// <summary>
        /// Reads the settings.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="owner">The owner.</param>
        /// <returns>Returns all properties.</returns>
        public IEnumerable<SonarQubeProperties> ReadSettings(Context context, string owner)
        {
            if (context.Equals(Context.AnalysisProject))
            {
                return this.tempproperties.Where(property => property.Context.Equals(context) && property.Owner.Equals(owner));
            }

            return this.properties.Where(property => property.Context.Equals(context) && property.Owner.Equals(owner));
        }

        /// <summary>
        /// Writes the setting.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <param name="sync">if set to <c>true</c> [synchronize].</param>
        /// <param name="skipifexist">if set to <c>true</c> [skipifexist].</param>
        public void WriteSetting(SonarQubeProperties prop, bool sync, bool skipifexist = false)
        {
            if (prop.Value == null)
            {
                return;
            }

            if (prop.Context.Equals(Context.AnalysisProject))
            {
                foreach (var property in this.tempproperties)
                {
                    if (property.Context.Equals(prop.Context) && property.Owner.Equals(prop.Owner) && property.Key.Equals(prop.Key))
                    {
                        if (skipifexist)
                        {
                            return;
                        }

                        property.Value = prop.Value;
                        return;
                    }
                }

                this.tempproperties.Add(prop);
                return;
            }

            foreach (var property in this.properties)
            {
                if (property.Context.Equals(prop.Context) && property.Owner.Equals(prop.Owner) && property.Key.Equals(prop.Key))
                {
                    if (skipifexist)
                    {
                        return;
                    }

                    property.Value = prop.Value;

                    if (sync)
                    {
                        this.SyncSettings();
                    }

                    return;
                }
            }

            this.properties.Add(prop);

            if (sync)
            {
                this.SyncSettings();
            }
        }

        /// <summary>
        /// Synchronizes the settings.
        /// </summary>
        public void SyncSettings()
        {
            if (this.properties.Count == 0)
            {
                if (!File.Exists(this.ApplicationDataUserSettingsFile))
                {
                    return;
                }

                try
                {
                    using (Stream stream = File.Open(this.ApplicationDataUserSettingsFile, FileMode.Open))
                    {
                        var bformatter = new BinaryFormatter();
                        this.properties = (List<SonarQubeProperties>)bformatter.Deserialize(stream);
                    }
                }
                catch (Exception ex)
                {
                    this.manager.ReportMessage(new Message { Id = "ConfigurationHelper", Data = "Failed to load configuration: " + this.ApplicationDataUserSettingsFile });
                    this.manager.ReportException(ex);
                    File.Delete(this.ApplicationDataUserSettingsFile);
                }
            }
            else
            {
                using (Stream stream = File.Open(this.ApplicationDataUserSettingsFile, FileMode.Create))
                {
                    var bformatter = new BinaryFormatter();

                    bformatter.Serialize(stream, this.properties);
                }
            }
        }

        /// <summary>
        /// Clears the non saved settings.
        /// </summary>
        public void ClearNonSavedSettings()
        {
            using (Stream stream = File.Open(this.ApplicationDataUserSettingsFile, FileMode.Open))
            {
                var bformatter = new BinaryFormatter();
                this.properties = (List<SonarQubeProperties>)bformatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Deletes the settings file.
        /// </summary>
        public void DeleteSettingsFile()
        {
            if (File.Exists(this.ApplicationDataUserSettingsFile))
            {
                File.Delete(this.ApplicationDataUserSettingsFile);
            }
        }

        /// <summary>
        ///     The get user app data configuration file.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string UserAppDataConfigurationFile()
        {
            return this.ApplicationDataUserSettingsFile;
        }
    }
}