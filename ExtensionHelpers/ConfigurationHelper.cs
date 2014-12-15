using ExtensionTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSonarPlugins;

namespace ExtensionHelpers
{
    public class ConfigurationHelper : IConfigurationHelper
    {
        /// <summary>
        ///     Gets or sets the application data user settings file.
        /// </summary>
        public string ApplicationDataUserSettingsFile { get; set; }

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
        ///     Initializes a new instance of the <see cref="VsPropertiesHelper" /> class.
        /// </summary>
        public ConfigurationHelper()
        {
            this.ApplicationDataUserSettingsFile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
                                                   + "\\VSSonarExtension\\settings.cfg";

            this.LogForAnalysis = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
                                                   + "\\VSSonarExtension\\temp\\analysisLog.txt";
        }

        /// <summary>
        /// The get all options for plugin option in application data.
        /// </summary>
        /// <param name="pluginKey">
        /// The plugin key.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public Dictionary<string, string> ReadAllAvailableOptionsInSettings(string pluginKey)
        {
            var options = new Dictionary<string, string>();
            if (!File.Exists(this.ApplicationDataUserSettingsFile))
            {
                return options;
            }

            string[] data = File.ReadAllLines(this.ApplicationDataUserSettingsFile);
            foreach (string s in data.Where(s => s.Contains(pluginKey + "=")))
            {
                try
                {
                    options.Add(s.Split('=')[1].Trim().Split(',')[0], s.Substring(s.IndexOf(',') + 1));
                }
                catch (ArgumentException)
                {
                }
            }

            return options;
        }

        /// <summary>
        /// The set option in application data.
        /// </summary>
        /// <param name="pluginKey">
        /// The plugin key.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public void WriteOptionInApplicationData(string pluginKey, string key, string value)
        {
            if (string.IsNullOrEmpty(pluginKey) || string.IsNullOrEmpty(key))
            {
                return;
            }

            IEnumerable<string> content = new List<string>();
            string contentWrite = string.Empty;
            if (File.Exists(this.ApplicationDataUserSettingsFile))
            {
                content = File.ReadLines(this.ApplicationDataUserSettingsFile);
            }
            else
            {
                Directory.CreateDirectory(Directory.GetParent(this.ApplicationDataUserSettingsFile).ToString());
                using (File.Create(this.ApplicationDataUserSettingsFile))
                {
                }
            }

            contentWrite += pluginKey + "=" + key + "," + value + "\r\n";
            contentWrite = content.Where(line => !line.Contains(pluginKey + "=" + key + ","))
                .Aggregate(contentWrite, (current, line) => current + (line + "\r\n"));

            using (var writer = new StreamWriter(this.ApplicationDataUserSettingsFile))
            {
                writer.Write(contentWrite);
            }
        }

        /// <summary>
        /// The set all options for plugin option in application data.
        /// </summary>
        /// <param name="pluginKey">
        /// The plugin key.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        public void WriteAllOptionsForPluginOptionInApplicationData(string pluginKey, Resource project, Dictionary<string, string> options)
        {
            this.DeleteOptionsForPlugin(pluginKey, project);
            foreach (var option in options)
            {
                this.WriteOptionInApplicationData(pluginKey, option.Key, option.Value);
            }
        }

        /// <summary>
        /// The delete options for plugin.
        /// </summary>
        /// <param name="pluginKey">
        /// The plugin key.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        public void DeleteOptionsForPlugin(string pluginKey, Resource project)
        {
            if (project == null || string.IsNullOrEmpty(pluginKey))
            {
                return;
            }

            IEnumerable<string> content = new List<string>();
            string contentWrite = string.Empty;
            if (File.Exists(this.ApplicationDataUserSettingsFile))
            {
                content = File.ReadLines(this.ApplicationDataUserSettingsFile);
            }
            else
            {
                Directory.CreateDirectory(Directory.GetParent(this.ApplicationDataUserSettingsFile).ToString());
                using (File.Create(this.ApplicationDataUserSettingsFile))
                {
                }
            }

            contentWrite = content.Where(line => !line.StartsWith(pluginKey + "=" + project.Key + "."))
                .Aggregate(contentWrite, (current, line) => current + (line + "\r\n"));

            using (var writer = new StreamWriter(this.ApplicationDataUserSettingsFile))
            {
                writer.Write(contentWrite);
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

        /// <summary>
        /// The get option from application data.
        /// </summary>
        /// <param name="pluginKey">
        /// The plugin Key.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ReadOptionFromApplicationData(string pluginKey, string key)
        {
            string outstring = string.Empty;

            if (!File.Exists(this.ApplicationDataUserSettingsFile) || string.IsNullOrEmpty(pluginKey))
            {
                return string.Empty;
            }

            string[] data = File.ReadAllLines(this.ApplicationDataUserSettingsFile);
            foreach (string s in data.Where(s => s.Contains(pluginKey + "=" + key + ",")))
            {
                outstring = s.Substring(s.IndexOf(',') + 1);
            }

            return outstring;
        }



    }
}
