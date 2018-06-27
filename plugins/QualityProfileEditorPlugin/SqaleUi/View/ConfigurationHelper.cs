using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqaleUi.View
{
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    class ConfigurationHelper : IConfigurationHelper
    {
        public string ApplicationPath { get; set; }

        public SonarQubeProperties ReadSetting(Context context, string owner, string key)
        {
            return new SonarQubeProperties { Value = "" };
        }

        public void WriteSetting(SonarQubeProperties prop, bool sync = false, bool skipIfExist = false)
        {
            return;
        }

        public void SyncSettings()
        {
            throw new NotImplementedException();
        }

        public void ClearNonSavedSettings()
        {
            throw new NotImplementedException();
        }

        public void DeleteSettingsFile()
        {
            throw new NotImplementedException();
        }

        public string UserAppDataConfigurationFile()
        {
            throw new NotImplementedException();
        }

        public string UserLogForAnalysisFile()
        {
            throw new NotImplementedException();
        }

        public void WriteSetting(Context context, string owner, string key, string value, bool sync = false, bool skipIfExist = false)
        {
            throw new NotImplementedException();
        }

        public void ResetAllSettings()
        {
            throw new NotImplementedException();
        }
    }
}
