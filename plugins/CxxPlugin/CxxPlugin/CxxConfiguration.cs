namespace CxxPlugin
{
    using System;
    using System.IO;

    using Newtonsoft.Json;

    /// <summary>
    /// Css Configuration
    /// </summary>
    public static class CxxConfiguration
    {
        /// <summary>
        /// Settings folder
        /// </summary>
        public static readonly string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".vssonarextension");

        /// <summary>
        /// downalod tools folder
        /// </summary>
        public static readonly string TempDownloadLockFile = Path.Combine(SettingsFolder, "downloadlock");

        /// <summary>
        /// wrapper folder
        /// </summary>
        public static readonly string WrapperFolder = Path.Combine(SettingsFolder, "Wrapper");

        /// <summary>
        /// settings file
        /// </summary>
        public static readonly string SettingsFile = Path.Combine(SettingsFolder, "cxx-settings.json");

        /// <summary>
        /// gets Settings
        /// </summary>
        public static CxxSettings CxxSettings => CreateSettings();

        /// <summary>
        /// settings        
        /// </summary>
        /// <param name="newSettings">settings data</param>
        public static void SyncSettings(CxxSettings newSettings)
        {
            File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(newSettings, Formatting.Indented));
        }

        /// <summary>
        /// create settings helper
        /// </summary>
        /// <param name="cxxVersion">version</param>
        /// <returns>cxx settings</returns>
        public static CxxSettings CreateSettingsElement(string cxxVersion)
        {
            // create file with default settings
            return new CxxSettings
            {
                CxxVersion = cxxVersion,
                VeraEnvironment = string.Empty,
                VeraExecutable = Path.Combine(WrapperFolder, cxxVersion, "Tools", "vera", "bin", "vera++.exe"),
                VeraArguments = "-nodup -showrules",
                RatsEnvironment = string.Empty,
                RatsExecutable = Path.Combine(WrapperFolder, cxxVersion, "Tools", "rats", "rats.exe"),
                RatsArguments = "--xml",
                PcLintEnvironment = string.Empty,
                PcLintExecutable = string.Empty,
                PcLintArguments = string.Empty,
                LinterProp = Path.Combine(SettingsFolder, "cxx-lint-0.9.5-SNAPSHOT.jar"),
                CustomEnvironment = string.Empty,
                CustomExecutable = Path.Combine(WrapperFolder, cxxVersion, "Tools", "Python27", "python.exe"),
                CustomArguments = Path.Combine(WrapperFolder, cxxVersion, "Tools", "CppLint", "cpplint_mod.py"),
                CustomSensorKey = "cpplint",
                CppCheckEnvironment = string.Empty,
                CppCheckExecutable = Path.Combine(WrapperFolder, cxxVersion, "Tools", "Cppcheck", "cppcheck.exe"),
                CppCheckArguments = "--inline-suppr --enable=all --xml -D__cplusplus -DNT",
                ClangExecutable = @"C:\Program Files\LLVM\bin\clang-tidy.exe"
            };
        }

        /// <summary>
        /// Create settings
        /// </summary>
        /// <returns>cxx settings</returns>
        private static CxxSettings CreateSettings()
        {
            var cxxVersion = "3.2.2";
            try
            {
                CxxSettings settings;
                if (!Directory.Exists(Directory.GetParent(SettingsFile).FullName))
                {
                    Directory.CreateDirectory(Directory.GetParent(SettingsFile).FullName);
                }

                if (!File.Exists(SettingsFile))
                {
                    settings = CreateSettingsElement(cxxVersion);

                    File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
                }
                else
                {
                    var txt = File.ReadAllText(SettingsFile);
                    settings = JsonConvert.DeserializeObject<CxxSettings>(txt);
                }

                return settings;
            }
            catch (Exception)
            {
                return CreateSettingsElement(cxxVersion);
            }
        }

    }
}
