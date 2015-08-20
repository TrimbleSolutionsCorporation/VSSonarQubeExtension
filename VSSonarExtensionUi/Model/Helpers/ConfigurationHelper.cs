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
        /// The properties
        /// </summary>
        private List<SonarQubeProperties> properties = new List<SonarQubeProperties>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationHelper"/> class. 
        ///     Initializes a new instance of the <see cref="VsPropertiesHelper"/> class.
        /// </summary>
        public ConfigurationHelper(string vsVersion)
        {
            this.ApplicationDataUserSettingsFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                                   + "\\VSSonarExtension\\settings.cfg." + vsVersion;

            this.LogForAnalysis = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                  + "\\VSSonarExtension\\temp\\analysisLog.txt." + vsVersion;

            if (!Directory.Exists(Directory.GetParent(this.ApplicationDataUserSettingsFile).ToString()))
            {
                Directory.CreateDirectory(Directory.GetParent(this.ApplicationDataUserSettingsFile).ToString());
            }

            this.SyncSettings();
        }

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

        public void WriteOptionInApplicationData(
            Context context,
            string owner,
            string key,
            string value, bool sync = false, bool skipIfExist = false)
        {
            this.WriteSetting(new SonarQubeProperties(context, owner, key, value), sync, skipIfExist);
        }

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

        public IEnumerable<SonarQubeProperties> ReadSettings(Context context, string owner)
        {
            if (context.Equals(Context.AnalysisProject))
            {
                return this.tempproperties.Where(property => property.Context.Equals(context) && property.Owner.Equals(owner));
            }

            return this.properties.Where(property => property.Context.Equals(context) && property.Owner.Equals(owner));
        }

        public void WriteSetting(SonarQubeProperties prop, bool sync, bool skipifexist)
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
                catch (Exception)
                {
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

        public void ClearNonSavedSettings()
        {
            using (Stream stream = File.Open(this.ApplicationDataUserSettingsFile, FileMode.Open))
            {
                var bformatter = new BinaryFormatter();
                this.properties = (List<SonarQubeProperties>)bformatter.Deserialize(stream);
            }
        }

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