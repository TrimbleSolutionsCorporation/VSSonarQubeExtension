// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxExternalSensor.cs" company="Copyright © 2014 jmecsoftware">
//   Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
// </copyright>
// <summary>
//   The cxx-lint sensor
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CxxPlugin.LocalExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Microsoft.Win32;

    using SonarRestService;
    using SonarRestService.Types;

    using VSSonarPlugins;

    /// <summary>
    ///     The CxxLintSensor sensor.
    /// </summary>
    public class CxxLintSensor : ASensor
    {
        /// <summary>
        /// prop
        /// </summary>
        public const string SKey = "cxx";

        /// <summary>
        /// Key
        /// </summary>
        public const string LinterProp = "CxxLinter";
        private readonly string javaBin = Path.Combine(CxxLintSensor.GetJavaInstallationPath(), "bin", "java.exe");
        private string pathForSettings;
        private readonly IVsEnvironmentHelper vshelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CxxLintSensor" /> class.
        /// </summary>
        /// <param name="notificationManager">The notification Manager.</param>
        /// <param name="configurationHelper">The configuration Helper.</param>
        /// <param name="sonarRestService">The sonar Rest Service.</param>
        /// <param name="vsHelper">vs helper</param>
        public CxxLintSensor(
            INotificationManager notificationManager,
            IConfigurationHelper configurationHelper,
            ISonarRestService sonarRestService,
            IVsEnvironmentHelper vsHelper)
            : base(SKey, true, notificationManager, configurationHelper, sonarRestService)
        {
            this.vshelper = vsHelper;
        }

        /// <summary>
        /// Gets the solution data.
        /// </summary>
        /// <value>
        /// The solution data.
        /// </value>
        public ProjectTypes.Solution SolutionData { get; private set; }

        /// <summary>
        /// file casing
        /// </summary>
        /// <param name="filePath">filepath</param>
        /// <returns>fix path</returns>
        public static string FixFilePathCasing(string filePath)
        {
            var fullFilePath = Path.GetFullPath(filePath);

            var fixedPath = string.Empty;
            foreach (var token in fullFilePath.Split('\\'))
            {
                // first token should be drive token
                if (fixedPath == string.Empty)
                {
                    // fix drive casing
                    var drive = string.Concat(token, "\\");
                    drive = DriveInfo.GetDrives()
                        .First(driveInfo => driveInfo.Name.Equals(drive, StringComparison.OrdinalIgnoreCase)).Name;

                    fixedPath = drive;
                }
                else
                {
                    fixedPath = Directory.GetFileSystemEntries(fixedPath, token).First();
                }
            }

            return fixedPath;
        }

        /// <summary>The get violations.</summary>
        /// <param name="lines">The lines.</param>
        /// <returns>The VSSonarPlugin.SonarInterface.ResponseMappings.Violations.ViolationsResponse.</returns>
        public override List<Issue> GetViolations(List<string> lines)
        {
            var violations = new List<Issue>();

            if (lines == null || lines.Count == 0)
            {
                return violations;
            }

            foreach (var line in lines)
            {
                try
                {
                    var start = 0;
                    var file = GetStringUntilFirstChar(ref start, line, '(');

                    start++;
                    var linenumber = Convert.ToInt32(GetStringUntilFirstChar(ref start, line, ')'));

                    start += 2;
                    var msg = line.Substring(start).Trim();

                    var elemsts = line.Split(' ');
                    var id = elemsts[elemsts.Length - 1].TrimEnd(']').TrimStart('[');

                    var entry = new Issue
                    {
                        Line = linenumber,
                        Message = msg.Replace(id, string.Empty).TrimEnd(']').TrimEnd('[').Trim(),
                        Rule = this.RepositoryKey + ":" + id,
                        Component = file
                    };

                    violations.Add(entry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error: " + ex.Message);
                }
            }

            return violations;
        }

        /// <summary>
        ///     The get environment.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public override Dictionary<string, string> GetEnvironment()
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Updates the profile.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="profileIn">The profile in.</param>
        /// <param name="vsVersion">vs version</param>
        public override void UpdateProfile(
            Resource project,
            ISonarConfiguration configuration,
            Dictionary<string, Profile> profileIn,
            string vsVersion)
        {
            var dataPath = Path.Combine(project.SolutionRoot, project.SolutionName);
            var packagesPath = Path.Combine(project.SolutionRoot, "Packages");
            this.SolutionData = MSBuildHelper.PreProcessSolution(string.Empty, packagesPath, dataPath, true, false, vsVersion);

            var compileDb = new StringBuilder();
            compileDb.AppendLine("[");
            foreach (var projectd in this.SolutionData.Projects)
            {
                foreach (var unit in projectd.Value.CompileUnits)
                {
                    compileDb.AppendLine("  {");
                    var message = string.Format("    \"directory\": \"{0}\",", unit.Directory.Replace("\\", "/"));
                    compileDb.AppendLine(message);
                    message = string.Format("    \"command\": \"{0}\",", unit.ClangCommand.Replace("\"", "\\\"").Replace("\\", "/"));
                    compileDb.AppendLine(message);
                    message = string.Format("    \"file\": \"{0}\"", unit.File.Replace("\\", "/"));
                    compileDb.AppendLine(message);
                    compileDb.AppendLine("  },");
                }

                foreach (var item in projectd.Value.NugetReferences)
                {
                    Debug.WriteLine("nuget: " + item);
                }
            }

            // dump clang compilation db
            var fileContent = compileDb.ToString().Trim().TrimEnd(',') + "\r\n]\r\n";
            File.WriteAllText(Path.Combine(project.SolutionRoot, "compile_commands.json"), fileContent);

            this.pathForSettings = Path.Combine(project.SolutionRoot, ".sonarqube", "cxx-lint");
            if (Directory.Exists(this.pathForSettings))
            {
                Directory.Delete(this.pathForSettings, true);
            }

            Directory.CreateDirectory(this.pathForSettings);

            var rules = string.Empty;
            if (profileIn.ContainsKey("c++"))
            {
                var profile = profileIn["c++"];
                rules = this.GetRules(profile);
            }

            if (profileIn.ContainsKey("cxx"))
            {
                var profile = profileIn["cxx"];
                rules = this.GetRules(profile);
            }

            foreach (var projectdata in this.SolutionData.Projects)
            {
                Directory.CreateDirectory(this.pathForSettings);

                try
                {
                    var fileSettings = Path.Combine(this.pathForSettings, projectdata.Key + ".json");
                    var platformToolSet = "  \"platformToolset\": \"" + projectdata.Value.PlatformToolset + "\",\r\n";
                    var platform = "  \"platform\": \"" + projectdata.Value.Platform + "\",\r\n";
                    var projectFile = "  \"projectFile\": \"" + projectdata.Value.Path + "\",\r\n";
                    var includeFolders = this.GetIncludeSettings(projectdata);
                    var defines = this.GetDefinesSettings(projectdata);
                    var additionalOptions = this.GetAdditionalOptions(projectdata);
                    if (File.Exists(fileSettings))
                    {
                        File.Delete(fileSettings);
                    }

                    File.WriteAllText(fileSettings, "{\r\n");

                    File.AppendAllText(fileSettings, platformToolSet);
                    File.AppendAllText(fileSettings, platform);
                    File.AppendAllText(fileSettings, projectFile);
                    File.AppendAllText(fileSettings, includeFolders);
                    File.AppendAllText(fileSettings, defines);
                    File.AppendAllText(fileSettings, additionalOptions);
                    if (!string.IsNullOrEmpty(rules))
                    {
                        File.AppendAllText(fileSettings, rules);
                    }

                    File.AppendAllText(fileSettings, "}\r\n");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        ///     The get command.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public override string GetCommand()
        {
            return this.javaBin;
        }

        /// <summary>
        /// The get arguments.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        public override string GetArguments(string filePath)
        {
            var jsonFileData = this.GetJsonFilePropArgument(filePath);
            return "-jar " + CxxConfiguration.CxxSettings.LinterProp + " " + jsonFileData + " -f " + filePath;
        }

        private static string GetStringUntilFirstChar(ref int start, string line, char charCheck)
        {
            var data = string.Empty;

            for (var i = start; i < line.Length; i++)
            {
                start = i;
                if (line[i].Equals(charCheck))
                {
                    break;
                }

                data += line[i];
            }

            return data;
        }

        private static string GetJavaInstallationPath()
        {
            var environmentPath = Environment.GetEnvironmentVariable("JAVA_HOME");

            if (string.IsNullOrEmpty(environmentPath))
            {
                const string JAVA_KEY = "SOFTWARE\\JavaSoft\\Java Runtime Environment\\";

                var localKey32 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                var localKey64 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);

                using (var rk32 = localKey32.OpenSubKey(JAVA_KEY))
                {
                    if (rk32 != null)
                    {
                        var currentVersion = rk32.GetValue("CurrentVersion").ToString();
                        using (var key32 = rk32.OpenSubKey(currentVersion))
                        {
                            return key32.GetValue("JavaHome").ToString();
                        }
                    }
                }

                using (var rk64 = localKey64.OpenSubKey(JAVA_KEY))
                {
                    if (rk64 != null)
                    {
                        var currentVersion = rk64.GetValue("CurrentVersion").ToString();
                        using (var key64 = rk64.OpenSubKey(currentVersion))
                        {
                            return key64.GetValue("JavaHome").ToString();
                        }
                    }
                }
            }
            else
            {
                return environmentPath;
            }

            return string.Empty;
        }

        private static string GetProperDirectoryCapitalization(DirectoryInfo dirInfo)
        {
            var parentDirInfo = dirInfo.Parent;
            return null == parentDirInfo
                       ? dirInfo.Name
                       : Path.Combine(GetProperDirectoryCapitalization(parentDirInfo), parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
        }

        private string GetRules(Profile profileIn)
        {
            var rulesjson = new StringBuilder();
            var finalbuilder = new StringBuilder();
            rulesjson.AppendLine("  \"rules\": [");
            foreach (var rule in profileIn.GetAllRules())
            {
                if (rule.Repo.Equals("cxx"))
                {
                    rulesjson.AppendLine("    {");
                    rulesjson.AppendLine("      \"ruleId\": \"" + rule.Key + "\",");
                    rulesjson.AppendLine("      \"templateKeyId\": \"" + rule.TemplateKey + "\",");

                    if (rule.Params.Count > 0)
                    {
                        rulesjson.AppendLine("      \"status\": \"Enabled\",");
                        rulesjson.AppendLine("      \"properties\": {");
                        for (var i = 0; i < rule.Params.Count; i++)
                        {
                            if (i != rule.Params.Count - 1)
                            {
                                if (string.IsNullOrEmpty(rule.Params[i].Value))
                                {
                                    rulesjson.AppendLine("        \"" + rule.Params[i].Key + "\": \"" + rule.Params[i].DefaultValue + "\",");
                                }
                                else
                                {
                                    rulesjson.AppendLine("        \"" + rule.Params[i].Key + "\": \"" + rule.Params[i].Value + "\",");
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(rule.Params[i].Value))
                                {
                                    rulesjson.AppendLine("        \"" + rule.Params[i].Key + "\": \"" + rule.Params[i].DefaultValue + "\"");
                                }
                                else
                                {
                                    rulesjson.AppendLine("        \"" + rule.Params[i].Key + "\": \"" + rule.Params[i].Value + "\"");
                                }
                            }
                        }

                        rulesjson.AppendLine("      }");
                    }
                    else
                    {
                        rulesjson.AppendLine("      \"status\": \"Enabled\"");
                    }

                    rulesjson.AppendLine("    },");
                }
            }

            var data = rulesjson.ToString().TrimEnd();
            finalbuilder.Append(data.TrimEnd(',') + "\r\n");
            finalbuilder.AppendLine("  ]");
            return finalbuilder.ToString();
        }

        private string GetAdditionalOptions(KeyValuePair<Guid, ProjectTypes.Project> projectdata)
        {
            var builder = new StringBuilder();
            var finalbuilder = new StringBuilder();
            builder.AppendLine("  \"additionalOptions\": [");
            foreach (var option in projectdata.Value.AdditionalOptions)
            {
                builder.AppendLine("    \"" + option + "\",");
            }

            if (projectdata.Value.AdditionalOptions.Count > 0)
            {
                var data = builder.ToString().TrimEnd();
                finalbuilder.Append(data.TrimEnd(',') + "\r\n");
            }
            else
            {
                finalbuilder.Append(builder.ToString());
            }

            finalbuilder.AppendLine("  ],");
            return finalbuilder.ToString();
        }

        private string GetIncludeSettings(KeyValuePair<Guid, ProjectTypes.Project> projectdata)
        {
            var builder = new StringBuilder();
            var finalbuilder = new StringBuilder();
            builder.AppendLine("  \"includes\": [");

            foreach (var systemInclude in projectdata.Value.SystemIncludeDirs)
            {
                try
                {
                    builder.AppendLine("    \"" + GetProperDirectoryCapitalization(new DirectoryInfo(systemInclude)).Replace("\\", "/") + "\",");
                }
                catch (Exception ex)
                {
                    builder.AppendLine("    \"" + systemInclude.Replace("\\", "/") + "\",");
                    System.Diagnostics.Debug.WriteLine("Failed to get Casing: " + ex.Message + " : " + systemInclude);
                }
            }

            foreach (var include in projectdata.Value.AdditionalIncludeDirectories)
            {
                if (Path.IsPathRooted(include))
                {
                    var fullPath = Path.GetFullPath(include);

                    try
                    {
                        builder.AppendLine("    \"" + GetProperDirectoryCapitalization(new DirectoryInfo(fullPath)).Replace("\\", "/") + "\",");
                    }
                    catch (Exception ex)
                    {
                        builder.AppendLine("    \"" + include.Replace("\\", "/") + "\",");
                        System.Diagnostics.Debug.WriteLine("Failed to get Casing: " + ex.Message + " : " + fullPath);
                    }
                }
                else
                {
                    var projectBasePath = Path.GetDirectoryName(projectdata.Value.Path).ToString();
                    var fullPath = Path.GetFullPath(Path.Combine(projectBasePath, include));

                    try
                    {
                        builder.AppendLine("    \"" + GetProperDirectoryCapitalization(new DirectoryInfo(fullPath)).Replace("\\", "/") + "\",");
                    }
                    catch (Exception ex)
                    {
                        builder.AppendLine("    \"" + fullPath.Replace("\\", "/") + "\",");
                        System.Diagnostics.Debug.WriteLine("Failed to get Casing: " + ex.Message);
                        System.Diagnostics.Debug.WriteLine("Failed to get Casing: " + ex.Message + " : " + fullPath);
                    }
                }
            }

            if (projectdata.Value.AdditionalIncludeDirectories.Count > 0)
            {
                var data = builder.ToString().TrimEnd();
                finalbuilder.Append(data.TrimEnd(',') + "\r\n");
            }
            else
            {
                finalbuilder.Append(builder.ToString());
            }

            finalbuilder.AppendLine("  ],");

            return finalbuilder.ToString();
        }

        private string GetDefinesSettings(KeyValuePair<Guid, ProjectTypes.Project> projectdata)
        {
            var builder = new StringBuilder();
            var finalbuilder = new StringBuilder();
            builder.AppendLine("  \"defines\": [");
            foreach (var define in projectdata.Value.Defines)
            {
                builder.AppendLine("    \"" + define + "\",");
            }

            if (projectdata.Value.Defines.Count > 0)
            {
                var data = builder.ToString().TrimEnd();
                finalbuilder.Append(data.TrimEnd(',') + "\r\n");
            }
            else
            {
                finalbuilder.Append(builder.ToString());
            }

            finalbuilder.AppendLine("  ],");
            return finalbuilder.ToString();
        }

        private string GetJsonFilePropArgument(string filePath)
        {
            if (this.SolutionData == null)
            {
                return string.Empty;
            }

            foreach (var project in this.SolutionData.Projects)
            {
                var projectPath = Directory.GetParent(Path.GetFullPath(project.Value.Path)).ToString().ToLower();
                var fileAbsPath = Path.GetFullPath(filePath).ToLower();

                if (fileAbsPath.Contains(projectPath))
                {
                    return "-s " + Path.GetFullPath(Path.Combine(this.pathForSettings, project.Key + ".json")).Replace("\\", "/");
                }
            }

            return string.Empty;
        }
    }
}
