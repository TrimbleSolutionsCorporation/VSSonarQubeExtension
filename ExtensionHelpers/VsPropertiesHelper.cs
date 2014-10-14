// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VsPropertiesHelper.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

    using ExtensionTypes;

    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.TextManager.Interop;

    using VSSonarPlugins;
    using Microsoft.VisualStudio.Shell.Interop;


    /// <summary>
    /// The vs properties helper.
    /// </summary>
    public class VsPropertiesHelper : IVsEnvironmentHelper
    {
        /// <summary>
        /// The environment 2.
        /// </summary>
        private readonly DTE2 environment;
        private readonly IServiceProvider provider;


        /// <summary>
        /// Initializes a new instance of the <see cref="VsPropertiesHelper"/> class.
        /// </summary>
        /// <param name="environment">
        /// The environment 2.
        /// </param>
        public VsPropertiesHelper(DTE2 environment, IServiceProvider service)
        {
            this.provider = service;
            this.environment = environment;
            this.ApplicationDataUserSettingsFile =
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\VSSonarExtension\\settings.cfg";
        }

        public VsPropertiesHelper()
        {
            this.ApplicationDataUserSettingsFile =
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\VSSonarExtension\\settings.cfg";
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
            var textManager = (IVsTextManager)Package.GetGlobalService(typeof(SVsTextManager));
            IVsTextView textView;
            textManager.GetActiveView(1, null, out textView);

            var userData = (IVsUserData)textView;
            if (userData == null)
            {
                return null;
            }

            var guidWpfViewHost = DefGuidList.guidIWpfTextViewHost;
            object host;
            userData.GetData(ref guidWpfViewHost, out host);
            return ((IWpfTextViewHost)host).TextView;
        }

        /// <summary>
        /// The get current text in view.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetCurrentTextInView()
        {
            var view = this.GetCurrentView();

            return view == null ? string.Empty : view.TextBuffer.CurrentSnapshot.GetText();
        }

        public bool AreWeRunningInVisualStudio()
        {
            return this.environment != null;
        }

        /// <summary>
        /// The get environment.
        /// </summary>
        /// <returns>
        /// The <see cref="DTE2"/>.
        /// </returns>
        public DTE2 Environment()
        {
            return this.environment;
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

        /// <summary>
        /// The get active project from solution name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ActiveProjectName()
        {
            if (this.environment == null)
            {
                return string.Empty;
            }

            var projects = (Array)this.environment.ActiveSolutionProjects;
            return ((Project)projects.GetValue(0)).Name;
        }

        /// <summary>
        /// The get active project from solution full name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ActiveProjectFileFullPath()
        {
            if (this.environment == null)
            {
                return string.Empty;
            }

            var projects = (Array)this.environment.ActiveSolutionProjects;
            return GetProperFilePathCapitalization(((Project)projects.GetValue(0)).FullName);
        }

        /// <summary>
        /// The get active file full path 1.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ActiveFileFullPath()
        {
            try
            {
                var doc = this.environment.ActiveDocument;
                return doc != null ? GetProperFilePathCapitalization(doc.FullName) : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// The get document language.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string CurrentSelectedDocumentLanguage()
        {
            if (this.environment == null)
            {
                return string.Empty;
            }

            var doc = this.environment.ActiveDocument;

            if (doc == null)
            {
                return string.Empty;
            }

            var textDoc = doc.Object("TextDocument") as TextDocument;
            return textDoc == null ? string.Empty : textDoc.Language.ToLower();
        }

        /// <summary>
        /// The get solution path.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ActiveSolutionPath()
        {
            if (this.environment == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(this.environment.Solution.FullName))
            {
                return string.Empty;
            }

            return Path.GetDirectoryName(GetProperFilePathCapitalization(this.environment.Solution.FullName));
        }

        /// <summary>
        /// The get solution path.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ActiveSolutionName()
        {
            if (this.environment == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(this.environment.Solution.FullName))
            {
                return string.Empty;
            }

            return (this.environment.Solution != null) ? Path.GetFileNameWithoutExtension(this.environment.Solution.FullName) : string.Empty;
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
            if (this.environment == null)
            {
                return string.Empty;
            }

            var props = this.environment.Properties[category, page];
            try
            {
                var data = props.Item(item).Value as string;
                return data;
            }
            catch
            {
                return string.Empty;
            }
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
            if (this.environment == null)
            {
                return;
            }

            var props = this.environment.Properties[category, page];
            props.Item(item).Value = value;
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
            if (this.environment == null)
            {
                return;
            }

            var props = this.environment.Properties[category, page];
            if (props.Item(item) == null)
            {
                this.WriteOption(category, page, item, value);
            }
            else
            {
                var data = this.ReadSavedOption(category, page, item);
                if (string.IsNullOrEmpty(data))
                {
                    this.WriteOption(category, page, item, value);
                }

                if (this.ReadSavedOption(category, page, item).Contains("vera++\\vera++"))
                {
                    this.WriteOption(category, page, item, value);
                }
            }
        }

        /// <summary>
        /// The navigate to resource.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        public void NavigateToResource(string url)
        {
            if (this.environment == null)
            {
                return;
            }

            (new System.Threading.Thread(() => this.environment.ItemOperations.Navigate(url, vsNavigateOptions.vsNavigateOptionsNewWindow))).Start();
        }

        /// <summary>
        /// The open resource in visual studio.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <param name="line">
        /// The line.
        /// </param>
        public void OpenResourceInVisualStudio(string filename, int line)
        {
            if (this.environment == null)
            {
                return;
            }

            var files = this.environment.Solution.FindProjectItem(filename);
            if (files != null)
            {
                (new System.Threading.Thread(() =>
                {
                    try
                    {
                        var masterPath = files.Properties.Item("FullPath").Value;
                        this.environment.ItemOperations.OpenFile(masterPath.ToString());
                        var textSelection = (TextSelection)this.environment.ActiveDocument.Selection;
                        textSelection.GotoLine(line < 1 ? 1 : line);
                        textSelection.SelectLine();
                    }
                    catch (Exception ex)
                    {
                        this.ErroMessage = "Exception Occured: " + filename + " : " + Convert.ToString(line) + " ex: " + ex.Message;
                    }
                })).Start();
            }
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
            if (string.IsNullOrEmpty(filename))
            {
                return null;
            }

            var driveLetter = filename.Substring(0, 1);
            var item = this.environment.Solution.FindProjectItem(filename);

            if (item == null)
            {
                return null;
            }

            var documentName = item.Document.Name;
            var documentPath = driveLetter + GetProperFilePathCapitalization(item.Document.FullName).Substring(1);
            var projectName = item.ContainingProject.Name;
            var projectPath = driveLetter + GetProperFilePathCapitalization(item.ContainingProject.FullName).Substring(1);
            var solutionName = this.ActiveSolutionName();
            var solutionPath = driveLetter + this.ActiveSolutionPath().Substring(1);
            return new VsProjectItem(documentName, documentPath, projectName, projectPath, solutionName, solutionPath);
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

        public void RestartVisualStudio()
        {
            var obj = (IVsShell4)this.provider.GetService(typeof(SVsShell));
            obj.Restart((uint)__VSRESTARTTYPE.RESTART_Normal);
        }

        public IVsOutputWindowPane CustomPane { get; set; }

        public void WriteToVisualStudioOutput(string errorMessage)
        {
            if (this.CustomPane != null)
            {
                this.CustomPane.OutputString(errorMessage + "\r\n");
                this.CustomPane.FlushToTaskList();
            }
        }
    }
}
