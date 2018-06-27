// --------------------------------------------------------------------------------------------------------------------
// <copyright file="gitplugin.cs" company="Copyright © 2015 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
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
namespace SQGitPlugin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using LibGit2Sharp;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using VSSonarQubeCmdExecutor;

    /// <summary>
    /// The cpp plugin.
    /// </summary>
    [Export(typeof(IPlugin))]
    public class SQGitPlugin : ISourceVersionPlugin
    {
        /// <summary>
        /// The blame cache.
        /// </summary>
        private readonly Dictionary<string, BlameHunkCollection> blameCache = new Dictionary<string, BlameHunkCollection>();

        /// <summary>
        /// The blame cache command line.
        /// </summary>
        private readonly Dictionary<string, GitBlame> blameCacheCommandLine = new Dictionary<string, GitBlame>();

        /// <summary>
        /// The DLL paths.
        /// </summary>
        private readonly List<string> dllPaths = new List<string>();

        /// <summary>
        /// The descrition.
        /// </summary>
        private readonly PluginDescription descrition;

        /// <summary>
        /// The notification manager.
        /// </summary>
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// The executor.
        /// </summary>
        private readonly IVSSonarQubeCmdExecutor executor;

        /// <summary>
        /// The repository.
        /// </summary>
        private Repository repository;

        /// <summary>
        /// The base path.
        /// </summary>
        private string basePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQGitPlugin" /> class.
        /// </summary>
        /// <param name="notificationManager">The notification manager.</param>
        public SQGitPlugin(INotificationManager notificationManager)
        {
            this.executor = new VSSonarQubeCmdExecutor(6000);
            this.UseCommandLine = true;
            this.notificationManager = notificationManager;
            this.descrition = new PluginDescription();
            this.descrition.Description = "Git Source Code Provider";
            this.descrition.Enabled = true;
            this.descrition.Name = "Git Plugin";
            this.descrition.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.descrition.AssemblyPath = Assembly.GetExecutingAssembly().Location;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQGitPlugin"/> class.
        /// </summary>
        public SQGitPlugin(INotificationManager notificationManager, IVSSonarQubeCmdExecutor executor)
        {
            this.executor = executor;
            this.UseCommandLine = true;
            this.notificationManager = notificationManager;
            this.descrition = new PluginDescription();
            this.descrition.Description = "Git Source Code Provider";
            this.descrition.Enabled = true;
            this.descrition.Name = "Git Plugin";
            this.descrition.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.descrition.AssemblyPath = Assembly.GetExecutingAssembly().Location;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use command line].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use command line]; otherwise, <c>false</c>.
        /// </value>
        public bool UseCommandLine { get; set; }

        /// <summary>
        /// Associates the project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        public void AssociateProject(Resource project, ISonarConfiguration configuration, Dictionary<string, Profile> profile, string vsversion)
        {
            // not needed
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // not needed
        }

        /// <summary>
        /// DLLs the locations.
        /// </summary>
        /// <returns>Returns the dlls locations.</returns>
        public IList<string> DllLocations()
        {
            return this.dllPaths;
        }

        /// <summary>
        /// Generates the token identifier.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>Returns nothing.</returns>
        public string GenerateTokenId(ISonarConfiguration configuration)
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the branch.
        /// </summary>
        /// <returns>
        /// Returns the branch.
        /// </returns>
        public string GetBranch()
        {
            if (this.repository == null)
            {
                return string.Empty;
            }

            return this.repository.Head.Name;
        }

        /// <summary>
        /// Determines whether the specified base path is supported.
        /// </summary>
        /// <returns>
        /// Returns if plugin supports solutions.
        /// </returns>
        public bool IsSupported()
        {
            return this.repository != null ? true : false;
        }

        /// <summary>
        /// Gets the blame by line.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="line">The line param.</param>
        /// <returns>
        /// Returns blame info, or null.
        /// </returns>
        public BlameLine GetBlameByLine(string filePath, int line)
        {
            if (this.repository == null || line < 0)
            {
                return null;
            }

            if (this.UseCommandLine)
            {
                try
                {
                    var relativePath = Path.GetFullPath(filePath).Replace(Path.GetFullPath(this.basePath) + "\\", string.Empty);
                    if (this.blameCacheCommandLine.ContainsKey(relativePath))
                    {
                        return this.BlameByLineByCommandLine(this.blameCacheCommandLine[relativePath], line);
                    }
                    else
                    {
                        var blameInfo = this.GenerateBlameForFile(relativePath);
                        this.blameCacheCommandLine.Add(relativePath, blameInfo);
                        return this.BlameByLineByCommandLine(blameInfo, line);
                    }

                }
                catch (Exception ex)
                {
                    this.notificationManager.ReportMessage(new Message { Id = "SQGitPlugin", Data = "Cannot blame using command line: ex " + ex.Message });
                    this.notificationManager.ReportMessage(new Message { Id = "SQGitPlugin", Data = "Make sure Git is installed and available in PATH" });
                    this.notificationManager.ReportException(ex);
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            }
            else
            {
                try
                {
                    var relativePath = Path.GetFullPath(filePath).Replace(Path.GetFullPath(this.basePath) + "\\", string.Empty);

                    if (this.blameCache.ContainsKey(relativePath))
                    {
                        return this.BlameByLine(this.blameCache[relativePath], line - 1);
                    }
                    else
                    {
                        var blameInfo = this.repository.Blame(relativePath);
                        this.blameCache.Add(relativePath, blameInfo);
                        return this.BlameByLine(blameInfo, line - 1);
                    }
                }
                catch (Exception ex)
                {
                    this.notificationManager.ReportMessage(new Message { Id = "SQGitPlugin", Data = "Cannot blame using libsharp2git: ex " + ex.Message });
                    this.notificationManager.ReportException(ex);
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            }
        }

        /// <summary>
        /// Initializes the repository.
        /// </summary>
        /// <param name="basePathIn">The base path in.</param>
        public void InitializeRepository(string basePathIn)
        {
            if (!Directory.Exists(basePathIn))
            {
                return;
            }

            var root = this.GetPathRoot(basePathIn);

            if (!string.IsNullOrEmpty(root))
            {
                this.basePath = basePathIn;

                if (this.repository != null)
                {
                    this.blameCache.Clear();
                    this.repository.Dispose();
                }

                this.repository = new Repository(root);
            }
        }

        /// <summary>
        /// Gets the plugin control options.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>Returns nothing.</returns>
        public IPluginControlOption GetPluginControlOptions(Resource project, ISonarConfiguration configuration)
        {
            return null;
        }

        /// <summary>
        /// Gets the plugin description.
        /// </summary>
        /// <returns>Returns plugin description.</returns>
        public PluginDescription GetPluginDescription()
        {
            return this.descrition;
        }

        /// <summary>
        /// Resets the defaults.
        /// </summary>
        public void ResetDefaults()
        {
            // not needed
        }

        /// <summary>
        /// Sets the DLL location.
        /// </summary>
        /// <param name="path">The path for the file.</param>
        public void SetDllLocation(string path)
        {
            this.dllPaths.Add(path);
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="config">The configuration.</param>
        public void OnConnectToSonar(VSSonarPlugins.Types.ISonarConfiguration config)
        {
        }

        /// <summary>
        /// Determines whether the specified base path is supported.
        /// </summary>
        /// <param name="basePathIn">The base path in.</param>
        /// <returns>
        /// Returns if plugin supports solutions.
        /// </returns>
        private string GetPathRoot(string basePathIn)
        {
            string currePath = basePathIn;

            while (Path.IsPathRooted(Path.GetFullPath(currePath)))
            {
                try
                {
                    using (new Repository(currePath))
                    {
                        return currePath;
                    }
                }
                catch (RepositoryNotFoundException ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                currePath = Directory.GetParent(currePath).ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Blames the by line.
        /// </summary>
        /// <param name="blameInfo">The blame information.</param>
        /// <param name="line">The line param.</param>
        /// <returns>Returns null if not found.</returns>
        private BlameLine BlameByLine(BlameHunkCollection blameInfo, int line)
        {
            var enumerator = blameInfo.GetEnumerator();
            enumerator.MoveNext();
            var prev = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var curr = enumerator.Current;

                Debug.WriteLine(prev.FinalSignature.Name + "    " + prev.FinalSignature.When + " === " + prev.FinalStartLineNumber + " === " + curr.FinalStartLineNumber + "     " + line.ToString());

                if (line >= prev.FinalStartLineNumber && line < curr.FinalStartLineNumber)
                {
                    var blame = new BlameLine();
                    blame.Line = line;
                    blame.Author = prev.FinalSignature.Name;
                    blame.Date = prev.FinalSignature.When.LocalDateTime;
                    blame.Email = prev.FinalSignature.Email;
                    return blame;
                }

                prev = enumerator.Current;
            }

            return null;
        }


        /// <summary>
        /// Generates the blame for file.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>Returns command line blame line.</returns>
        private GitBlame GenerateBlameForFile(string relativePath)
        {
            this.executor.ResetData();
            this.executor.ExecuteCommand("git.exe", "blame --porcelain -M -w " + relativePath, new Dictionary<string, string>(), this.basePath);
            var lines = this.executor.GetStdOut();

            return GitExtensionsFunctions.GenerateBlameForFile(relativePath, lines);
        }

        /// <summary>
        /// Blames the by line by command line.
        /// </summary>
        /// <param name="list">The list param.</param>
        /// <param name="line">The line param.</param>
        /// <returns>
        /// Returns blame line.
        /// </returns>
        private BlameLine BlameByLineByCommandLine(GitBlame list, int line)
        {
            if (line >= list.Lines.Count)
            {
                return null;
            }

            var lineBlame = list.Lines[line - 1];

            foreach (var item in list.Headers)
            {
                if (item.CommitGuid.Equals(lineBlame.CommitGuid))
                {
                    var blame = new BlameLine();
                    blame.Author = item.Author;
                    blame.Email = item.AuthorMail.TrimEnd('>').TrimStart('<').Trim();
                    blame.Date = item.CommitterTime;
                    blame.Line = line;
                    blame.Guid = item.CommitGuid;
                    blame.Summary = item.Summary;
                    return blame;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether [is valid sh a1] [the specified s].
        /// </summary>
        /// <param name="stringData">The string data.</param>
        /// <returns>
        /// If string is valid.
        /// </returns>
        private bool CheckAgaistSHA1(string stringData)
        {
            return Regex.Matches(stringData, "[a-fA-F0-9]{40}").Count != 0;
        }
  
    }
}