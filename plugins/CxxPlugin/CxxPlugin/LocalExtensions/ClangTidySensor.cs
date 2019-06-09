namespace CxxPlugin.LocalExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    using SonarRestService;
    using SonarRestService.Types;
    using System.Threading.Tasks;

    /// <summary>
    /// sensor clang tidy
    /// </summary>
    public class ClangTidySensor
    {
        /// <summary>
        /// The environment
        /// </summary>
        private readonly Dictionary<string, string> environment;

        /// <summary>
        /// The regx
        /// </summary>
        private readonly string regx;

        /// <summary>
        /// The regx note
        /// </summary>
        private readonly string regxNote;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configHelper;

        /// <summary>
        /// The logger
        /// </summary>
        private INotificationManager logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClangTidySensor"/> class.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="configHelper">The configuration helper.</param>
        public ClangTidySensor(IVsEnvironmentHelper helper, IConfigurationHelper configHelper, INotificationManager notificationManager)
        {
            this.logger = notificationManager;
            this.configHelper = configHelper;
            this.configHelper.WriteSetting(
                Context.FileAnalysisProperties,
                "CxxPlugin",
                "ClangExecutable",
                @"C:\Program Files\LLVM\bin\clang-tidy.exe",
                true,
                true);

            this.regx = @"(.+|[a-zA-Z]:\\.+):([0-9]+):([0-9]+): ([^:]+): ([^]]+) \[([^]]+)\]";
            this.regxNote = @"(.+|[a-zA-Z]:\\.+):([0-9]+):([0-9]+): ([^:]+): ([^]]+)";
            this.environment = new Dictionary<string, string>();

            // catpure environment to use
            var buildEnvironmentBatFile = @"C:\\Program Files (x86)\\Microsoft Visual Studio 14.0\\VC\\vcvarsall.bat";

            if(helper.VsVersion().Contains("14.0"))
            {
                if(File.Exists("C:\\Program Files (x86)\\Microsoft Visual Studio 14.0\\VC\\vcvarsall.bat"))
                {
                    buildEnvironmentBatFile = "C:\\Program Files (x86)\\Microsoft Visual Studio 14.0\\VC\\vcvarsall.bat";
                }
                else
                {
                    buildEnvironmentBatFile = "C:\\Program Files\\Microsoft Visual Studio 14.0\\VC\\vcvarsall.bat";
                }
            }

            if(helper.VsVersion().Contains("12.0"))
            {
                if(File.Exists("C:\\Program Files (x86)\\Microsoft Visual Studio 12.0\\VC\\vcvarsall.bat"))
                {
                    buildEnvironmentBatFile = "C:\\Program Files (x86)\\Microsoft Visual Studio 12.0\\VC\\vcvarsall.bat";
                }
                else
                {
                    buildEnvironmentBatFile = "C:\\Program Files\\Microsoft Visual Studio 12.0\\VC\\vcvarsall.bat";
                }
            }

            if(helper.VsVersion().Contains("11.0"))
            {
                if(File.Exists("C:\\Program Files (x86)\\Microsoft Visual Studio 11.0\\VC\\vcvarsall.bat"))
                {
                    buildEnvironmentBatFile = "C:\\Program Files (x86)\\Microsoft Visual Studio 11.0\\VC\\vcvarsall.bat";
                }
                else
                {
                    buildEnvironmentBatFile = "C:\\Program Files\\Microsoft Visual Studio 11.0\\VC\\vcvarsall.bat";
                }
            }

            if(File.Exists(buildEnvironmentBatFile))
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = "/c \"" + buildEnvironmentBatFile + "\" && set",
                    WindowStyle = ProcessWindowStyle.Normal,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                };

                var proc = new Process() { StartInfo = startInfo };
                proc.Start();
                var output = proc.StandardOutput.ReadToEnd();
                foreach(var line in Regex.Split(output, "\r\n"))
                {
                    if(string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    var data = line.Split('=');
                    if(data.Length.Equals(2))
                    {
                        this.environment.Add(data[0], data[1]);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the profile.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="profileIn">The profile in.</param>
        /// <param name="vsVersion">The vs version.</param>
        public async Task UpdateProfile(
            Resource project,
            ISonarConfiguration configuration,
            Dictionary<string, Profile> profileIn,
            string vsVersion)
        {
            await Task.Delay(0);
            if(!profileIn.ContainsKey("c++"))
            {
                return;
            }

            var profile = profileIn["c++"];
            var rules = profile.GetAllRules();

            string baseString = "";
            using(var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("CxxPlugin.Resources.clang-tidy"))
            {
                using(StreamReader reader = new StreamReader(resource))
                {
                    baseString = reader.ReadToEnd();
                }
            }

            var clangTidyFile = Path.Combine(project.SolutionRoot, ".clang-tidy");
            var lineEnding = new String[] { "\n" };            
            if(string.IsNullOrEmpty(baseString))
            {
                return;
            }

            var baseConfig = baseString.Split(lineEnding, StringSplitOptions.RemoveEmptyEntries);

            if(File.Exists(clangTidyFile))
            {
                File.Delete(clangTidyFile);
            }

            using(StreamWriter outputFile = new StreamWriter(clangTidyFile))
            {
                foreach(var line in baseConfig)
                {
                    var lineToFile = line;
                    if(line.StartsWith("Checks:          '-clang-diagnostic-*,-clang-analyzer-*'"))
                    {
                        lineToFile = lineToFile.TrimEnd().TrimEnd('\'');
                        foreach(var rule in rules)
                        {
                            if(rule.Repo.Equals("ClangTidy"))
                            {
                                lineToFile += "," + rule.Key;
                            }
                        }

                        lineToFile += "\'";
                    }

                    outputFile.WriteLine(lineToFile);
                }
            }
        }

        /// <summary>
        /// Executes the clang tidy.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="executor">The executor.</param>
        /// <returns>issues</returns>
        public List<Issue> ExecuteClangTidy(
            VsFileItem resource,
            IVSSonarQubeCmdExecutor executor,
            string additionalArgs)
        {
            var tidy = @"clang-tidy";

            try
            {
                tidy = this.configHelper.ReadSetting(
                        Context.FileAnalysisProperties,
                        "CxxPlugin",
                        "ClangExecutable").Value;

                if(!File.Exists(tidy))
                {
                    this.logger.ReportMessage(new Message() { Id = "ClangTidy", Data = "clang-tidy not found in location provided: " + tidy });
                    this.logger.ReportMessage(new Message() { Id = "ClangTidy", Data = "Install LLVM package" });
                    return new List<Issue>();
                }
            }
            catch(Exception ex)
            {
                this.logger.ReportMessage(new Message() { Id = "ClangTidy", Data = "Failed to read clang tidy location : prop : ClangExecutable" });
                this.logger.ReportException(ex);
                return new List<Issue>();
            }

            executor.ExecuteCommand(tidy, resource.FilePath + " -debug " + additionalArgs, this.environment, resource.SonarResource.SolutionRoot);
            var output = executor.GetStdOut().ToArray();
            foreach(var errorData in executor.GetStdError())
            {
                this.logger.ReportMessage(new Message() { Id = "ClangTidy", Data = errorData });
            }
            return ParseClangLog(output, resource);
        }

        /// <summary>
        /// Parses the clang log.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <returns></returns>
        private List<Issue> ParseClangLog(string[] output, VsFileItem resource)
        {
            var fileName = Path.GetFileName(resource.FilePath);
            var folderPath = Path.GetDirectoryName(resource.FilePath);

            var issues = new List<Issue>();

            Issue issue = null;
            int finalLine = -1;
            for(int i = 0; i < output.Length; i++)
            {
                var line = output[i];
                Match m = Regex.Match(line, this.regx);
                if(m.Success)
                {
                    if(issue != null)
                    {
                        if(!issue.LocalPath.ToLower().Contains(fileName))
                        {
                            issue.LocalPath = resource.FilePath;
                            issue.Component = resource.SonarResource.Key;
                            issue.Line = finalLine;
                        }

                        issues.Add(issue);
                        finalLine = -1;
                    }

                    var type = m.Groups[4];
                    var snippet = output[i + 1].Trim();
                    var localPath = m.Groups[1].Value;
                    if(!Path.IsPathRooted(localPath))
                    {
                        localPath = Path.Combine(folderPath, localPath);
                    }

                    issue = new Issue()
                    {
                        LocalPath = localPath,
                        Component = resource.SonarResource.Key,
                        Line = int.Parse(m.Groups[2].Value),
                        Column = int.Parse(m.Groups[3].Value),
                        Message = m.Groups[5].Value + " " + snippet,
                        Rule = "ClangTidy:" + m.Groups[6].Value
                    };

                    continue;
                }

                m = Regex.Match(line, this.regxNote);
                if(m.Success)
                {
                    if(issue != null)
                    {
                        var snippet = output[i + 1].Trim();

                        var path = m.Groups[1].Value;
                        if(Path.IsPathRooted(path))
                        {
                            path = Path.GetFullPath(path);
                        }
                        else
                        {
                            if(issue.Component.EndsWith(Path.GetFileName(path)))
                            {
                                path = Path.GetFullPath(Path.Combine(folderPath, path));
                            }
                        }

                        var explanation = new ExplanationLine()
                        {
                            Path = path,
                            Line = int.Parse(m.Groups[2].Value),
                            Message = m.Groups[5].Value + " " + snippet
                        };

                        issue.Explanation.Add(explanation);

                        if(finalLine != -1)
                        {
                            continue;
                        }

                        if(explanation.Path.ToLower().Contains(fileName.ToLower()))
                        {
                            finalLine = explanation.Line;
                        }
                    }
                }
            }

            if(issue != null)
            {
                if(!issue.LocalPath.ToLower().Contains(fileName))
                {
                    issue.LocalPath = resource.FilePath;
                    issue.Component = resource.SonarResource.Key;
                    issue.Line = finalLine;
                }

                issues.Add(issue);
            }

            return issues;
        }
    }
}
