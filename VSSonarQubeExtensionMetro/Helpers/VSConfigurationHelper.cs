// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VsConfigurationHelper.cs" company="Copyright © 2016 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarQubeExtension.Helpers
{
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// visual studio settings helper
    /// </summary>
    /// <seealso cref="VSSonarPlugins.IConfigurationHelper" />
    public class VsConfigurationHelper : IConfigurationHelper
    {
        /// <summary>
        /// The base key registry version
        /// </summary>
        private readonly string baseKeyRegistryVersion;

        /// <summary>
        /// The base key registry
        /// </summary>
        private readonly string baseKeyRegistry;

        /// <summary>
        /// The vs version
        /// </summary>
        private readonly string vsversion;

        /// <summary>
        /// Initializes a new instance of the <see cref="VsConfigurationHelper" /> class.
        /// </summary>
        /// <param name="vsVersion">The vs version.</param>
        public VsConfigurationHelper(string vsVersion)
        {
            this.vsversion = vsVersion;
            this.ApplicationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "VSSonarExtension");

            this.baseKeyRegistry = "SOFTWARE\\VSSonarExtension";
            this.baseKeyRegistryVersion = this.baseKeyRegistry + "\\" + this.vsversion;
            this.GetBaseRegistry(false);
        }

        /// <summary>
        /// Gets or sets the application path.
        /// </summary>
        /// <value>
        /// The application path.
        /// </value>
        public string ApplicationPath { get; set; }

        /// <summary>
        /// The delete settings file.
        /// </summary>
        public void ResetAllSettings()
        {
            RegistryKey rk = Registry.CurrentUser;
            RegistryKey sk1 = rk.OpenSubKey(this.baseKeyRegistry, true);
            if (sk1 != null)
            {
                sk1.DeleteSubKeyTree(this.vsversion);
            }            
        }

        /// <summary>
        /// The write configuration.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="key">The key.</param>
        /// <returns>
        /// The <see cref="T:VSSonarPlugins.Types.SonarQubeProperties" />.
        /// </returns>
        public SonarQubeProperties ReadSetting(Context context, string owner, string key)
        {
            var baseKey = this.GetBaseRegistry(false);
            var data = baseKey.GetValue(this.GetRegistryKey(new SonarQubeProperties { Context = context, Key = key, Owner = owner }));

            if (data == null)
            {
                return null;
            }

            var bformatter = new BinaryFormatter();
            using (var ms = new MemoryStream((byte [])data))
            {
                return (SonarQubeProperties)bformatter.Deserialize(ms);
            }           
        }

        /// <summary>
        /// The sync settings.
        /// </summary>
        public void SyncSettings()
        {
        }

        /// <summary>
        /// The get user app data configuration file.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.String" />.
        /// </returns>
        public string UserAppDataConfigurationFile()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VSSonarExtension\\settings.cfg." + this.vsversion;
        }

        /// <summary>
        /// The user log for analysis file.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.String" />.
        /// </returns>
        public string UserLogForAnalysisFile()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                      + "\\VSSonarExtension\\temp\\analysisLog.txt." + this.vsversion;
        }

        /// <summary>
        /// The write option in application data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="sync">The sync.</param>
        /// <param name="skipIfExist">The skip if exist.</param>
        public void WriteOptionInApplicationData(Context context, string owner, string key, string value, bool sync = false, bool skipIfExist = false)
        {
            this.WriteSetting(new SonarQubeProperties(context, owner, key, value), sync, skipIfExist);
        }

        /// <summary>
        /// The write setting.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="sync">The sync.</param>
        /// <param name="skipIfExist">The skip if exist.</param>
        public void WriteSetting(SonarQubeProperties prop, bool sync = false, bool skipIfExist = false)
        {
            var bformatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bformatter.Serialize(ms, prop);
            var baseRk = this.GetBaseRegistry(true);
            baseRk.SetValue(this.GetRegistryKey(prop), ms.GetBuffer());
        }

        /// <summary>
        /// Gets the registry key.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <returns></returns>
        private string GetRegistryKey(SonarQubeProperties prop)
        {
            return prop.Context + "_" + prop.Key; 
        }

        /// <summary>
        /// Gets the base registry.
        /// </summary>
        /// <param name="enableWrite">if set to <c>true</c> [enable write].</param>
        /// <returns></returns>
        private RegistryKey GetBaseRegistry(bool enableWrite)
        {
            RegistryKey rk = Registry.CurrentUser;
            RegistryKey sk1 = rk.OpenSubKey(this.baseKeyRegistryVersion, enableWrite);
            if (sk1 == null)
            {
                RegistryKey sk2 = rk.CreateSubKey(this.baseKeyRegistryVersion);
                sk2.SetValue("CreationDate", DateTime.Now.ToLongDateString());
            }

            return sk1;
        }

        public void WriteSetting(Context context, string owner, string key, string value, bool sync = false, bool skipIfExist = false)
        {
            this.WriteSetting(new SonarQubeProperties { Context = context, Owner = owner, Key = key, Value = value }, sync, skipIfExist);
        }
    }
}
