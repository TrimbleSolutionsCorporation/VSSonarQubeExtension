// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VsHelper.cs" company="Copyright © 2014 jmecsoftware">
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

namespace ExtensionHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    using EnvDTE;
    using EnvDTE80;

    using VSSonarPlugins.Types;

    using Microsoft.VisualStudio.Text.Editor;

    using VSSonarPlugins;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// The vs properties helper.
    /// </summary>
    public class VsHelper : IVsEnvironmentHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VsPropertiesHelper"/> class.
        /// </summary>
        /// <param name="environment">
        /// The environment 2.
        /// </param>
        public VsHelper()
        {
            this.ApplicationDataUserSettingsFile =
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\VSSonarExtension\\QualityEditor\\settings.cfg";
        }

        /// <summary>
        /// Gets or sets the erro message.
        /// </summary>
        public string ErroMessage { get; set; }

        /// <summary>
        /// Gets or sets the application data user settings file.
        /// </summary>
        public string ApplicationDataUserSettingsFile { get; set; }

        /// <summary>
        /// The get windows physical path.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetWindowsPhysicalPathOld(string path)
        {
            var builder = new StringBuilder(255);

            // names with long extension can cause the short name to be actually larger than
            // the long name.
            GetShortPathName(path, builder, builder.Capacity);

            path = builder.ToString();

            var result = GetLongPathName(path, builder, builder.Capacity);

            if (result > 0 && result < builder.Capacity)
            {
                builder[0] = char.ToLower(builder[0]);
                return builder.ToString(0, (int)result);
            }

            if (result <= 0)
            {
                return null;
            }

            builder = new StringBuilder((int)result);
            result = GetLongPathName(path, builder, builder.Capacity);
            builder[0] = char.ToLower(builder[0]);
            return builder.ToString(0, (int)result);
        }

        /// <summary>
        /// The get current view.
        /// </summary>
        /// <returns>
        /// The <see cref="IWpfTextView"/>.
        /// </returns>
        public IWpfTextView GetCurrentView()
        {
            return null;
        }

        /// <summary>
        /// The get current text in view.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetCurrentTextInView()
        {
            return string.Empty;
        }

        public bool AreWeRunningInVisualStudio()
        {
            throw new NotImplementedException();
        }

        public void ShowSourceDiff(string resourceInEditorTxt, string documentInViewPath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get environment.
        /// </summary>
        /// <returns>
        /// The <see cref="DTE2"/>.
        /// </returns>
        public DTE2 Environment()
        {
            return null;
        }

        public VsProjectItem VsProjectItem(string projectFileName, Resource associatedProject)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get file real path for solution.
        /// </summary>
        /// <param name="fileInView">
        /// The file in view.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetFileRealPathForSolution(string fileInView)
        {
            var solutionPath = this.ActiveSolutionPath();
            var driveLetter = solutionPath.Substring(0, 1);
            return driveLetter + fileInView.Substring(1);
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
            var contentWrite = string.Empty;
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
            contentWrite = content.Where(line => !line.Contains(pluginKey + "=" + key + ",")).Aggregate(contentWrite, (current, line) => current + (line + "\r\n"));

            using (var writer = new StreamWriter(this.ApplicationDataUserSettingsFile))
            {
                writer.Write(contentWrite);
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
            var contentWrite = string.Empty;
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

            contentWrite = content.Where(line => !line.StartsWith(pluginKey + "=" + project.Key + ".")).Aggregate(contentWrite, (current, line) => current + (line + "\r\n"));

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

        public void OpenResourceInVisualStudio(string workfolder, string filename, int line, string editorCommandExec = "notepad")
        {
        }

        public void OpenResourceInVisualStudio(string filename, int line, string editorCommandExec = "notepad")
        {
        }

        /// <summary>
        /// The get active project from solution name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ActiveProjectName()
        {
            return string.Empty;
        }

        /// <summary>
        /// The get active project from solution full name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ActiveProjectFileFullPath()
        {
            return string.Empty;
        }

        /// <summary>
        /// The get active file full path 1.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ActiveFileFullPath()
        {
            return string.Empty;
        }

        /// <summary>
        /// The get document language.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string CurrentSelectedDocumentLanguage()
        {
            return string.Empty;
        }

        /// <summary>
        /// The get solution path.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ActiveSolutionPath()
        {
            return string.Empty;
        }

        /// <summary>
        /// The get solution path.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ActiveSolutionName()
        {
            return string.Empty;
        }

        /// <summary>
        /// The get saved option.
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ReadSavedOption(string category, string page, string item)
        {
            return string.Empty;
        }

        public void ClearDiffFile(string localFileName, string serverFileName)
        {
            throw new NotImplementedException();
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
            var outstring = string.Empty;

            if (!File.Exists(this.ApplicationDataUserSettingsFile) || string.IsNullOrEmpty(pluginKey))
            {
                return string.Empty;
            }

            var data = File.ReadAllLines(this.ApplicationDataUserSettingsFile);
            foreach (var s in data.Where(s => s.Contains(pluginKey + "=" + key + ",")))
            {
                outstring = s.Substring(s.IndexOf(',') + 1);
            }

            return outstring;
        }

        /// <summary>
        /// The get all options for plugin option in application data.
        /// </summary>
        /// <param name="pluginKey">
        /// The plugin key.
        /// </param>
        /// <returns>
        /// The <see>
        ///     <cref>Dictionary</cref>
        /// </see>
        ///     .
        /// </returns>
        public Dictionary<string, string> ReadAllAvailableOptionsInSettings(string pluginKey)
        {
            var options = new Dictionary<string, string>();
            if (!File.Exists(this.ApplicationDataUserSettingsFile))
            {
                return options;
            }

            var data = File.ReadAllLines(this.ApplicationDataUserSettingsFile);
            foreach (var s in data.Where(s => s.Contains(pluginKey + "=")))
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
        /// The get user app data configuration file.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string UserAppDataConfigurationFile()
        {
            return this.ApplicationDataUserSettingsFile;
        }

        public void WriteToVisualStudioOutput(string errorMessage)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set option.
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public void WriteOption(string category, string page, string item, string value)
        {
        }

        public void RestartVisualStudio()
        {
            throw new NotImplementedException();
        }

        public VsFileItem VsFileItem(string filename, Resource associatedProject, Resource fileResource)
        {
            throw new NotImplementedException();
        }

        public VsFileItem VsFileItem(string fullPath, string projectFullPath, Resource associatedProject, Resource fileResource)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The set default option.
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public void WriteDefaultOption(string category, string page, string item, string value)
        {
            return;
        }

        /// <summary>
        /// The navigate to resource.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        public void NavigateToResource(string url)
        {
        }

        /// <summary>
        /// The get vs project item.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="VSSonarPlugins.VsProjectItem"/>.
        /// </returns>
        public VsProjectItem VsProjectItem(string filename)
        {
            return null;
        }

        /// <summary>
        /// The get proper directory capitalization.
        /// </summary>
        /// <param name="dirInfo">
        /// The dir info.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetProperDirectoryCapitalization(DirectoryInfo dirInfo)
        {
            var parentDirInfo = dirInfo.Parent;
            return null == parentDirInfo ? dirInfo.Name : Path.Combine(GetProperDirectoryCapitalization(parentDirInfo), parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
        }

        /// <summary>
        /// The get proper file path capitalization.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetProperFilePathCapitalization(string filename)
        {
            var fileInfo = new FileInfo(filename);
            var dirInfo = fileInfo.Directory;
            return dirInfo != null ? Path.Combine(GetProperDirectoryCapitalization(dirInfo), dirInfo.GetFiles(fileInfo.Name)[0].Name) : string.Empty;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint GetLongPathName(string shortPath, StringBuilder sb, int buffer);

        [DllImport("kernel32.dll")]
        private static extern uint GetShortPathName(string longpath, StringBuilder sb, int buffer);


        string IVsEnvironmentHelper.GetProperFilePathCapitalization(string filename)
        {
            return string.Empty;
        }

        public string ActiveConfiguration()
        {
            return "";
        }

        public string ActivePlatform()
        {
            return "";
        }


        public VsProjectItem GetProjectByNameInSolution(string projectName)
        {
            throw new NotImplementedException();
        }

        public VsProjectItem GetProjectByNameInSolution(string projectName, string solutionPath)
        {
            throw new NotImplementedException();
        }

        public string GetGuidForProject(string projectPath, string solutionPath)
        {
            throw new NotImplementedException();
        }

        public VsProjectItem GetProjectByGuidInSolution(string projectGuid, string solutionPath)
        {
            throw new NotImplementedException();
        }

        public string EvaluatedValueForIncludeFile(string msbuildProjectFile, string filePath)
        {
            throw new NotImplementedException();
        }

        public void SetCurrentDocumentInView(string fullName)
        {
            throw new NotImplementedException();
        }

        public string GetCurrentDocumentInView()
        {
            throw new NotImplementedException();
        }

        public bool DoIHaveAdminRights()
        {
            throw new NotImplementedException();
        }

        public Microsoft.CodeAnalysis.Solution GetCurrentRoslynSolution()
        {
            throw new NotImplementedException();
        }

        public string VsVersion()
        {
            throw new NotImplementedException();
        }
    }
}
