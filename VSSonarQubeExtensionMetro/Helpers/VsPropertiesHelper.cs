// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VsPropertiesHelper.cs" company="">
//   
// </copyright>
// <summary>
//   The vs properties helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarQubeExtension.Helpers
{
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.TextManager.Interop;
    using SonarRestService.Types;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Text;
    using System.Windows;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using Process = System.Diagnostics.Process;
    using Thread = System.Threading.Thread;

    /// <summary>
    ///     The vs properties helper.
    /// </summary>
    public class VsPropertiesHelper : IVsEnvironmentHelper
    {
        /// <summary>
        ///     The environment 2.
        /// </summary>
        private readonly DTE2 environment;

        /// <summary>
        ///     The provider.
        /// </summary>
        private readonly IServiceProvider provider;

        /// <summary>
        /// The text view host.
        /// </summary>
        private IWpfTextViewHost textViewHost;

        /// <summary>
        /// The resourceinview
        /// </summary>
        private string resourceinview;

        /// <summary>
        /// Initializes a new instance of the <see cref="VsPropertiesHelper"/> class.
        /// </summary>
        /// <param name="environment">
        /// The environment 2.
        /// </param>
        /// <param name="service">
        /// The service.
        /// </param>
        public VsPropertiesHelper(DTE2 environment, IServiceProvider service)
        {
            TempDataFolder = Path.GetTempPath();
            provider = service;
            this.environment = environment;
        }

        /// <summary>
        ///     Gets or sets the custom pane.
        /// </summary>
        public IVsOutputWindowPane CustomPane { get; set; }

        /// <summary>
        ///     Gets or sets the erro message.
        /// </summary>
        public string ErroMessage { get; set; }

        /// <summary>
        ///     Gets or sets the temp data folder.
        /// </summary>
        public string TempDataFolder { get; set; }

        /// <summary>
        /// Gets the text view host.
        /// </summary>
        public IWpfTextViewHost TextViewHost
        {
            get
            {
                if (textViewHost == null)
                {
                    IVsTextManager textManager = (IVsTextManager)Package.GetGlobalService(typeof(SVsTextManager));
                    textManager.GetActiveView(1, null, out IVsTextView textView);

                    IVsUserData data = textView as IVsUserData;
                    if (data != null)
                    {
                        Guid guid = DefGuidList.guidIWpfTextViewHost;
                        int hr = data.GetData(ref guid, out object obj);
                        if ((hr == VSConstants.S_OK) && obj != null && obj is IWpfTextViewHost)
                        {
                            textViewHost = obj as IWpfTextViewHost;
                        }
                    }
                }

                return textViewHost;
            }
        }

        /// <summary>
        /// The get document language.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string GetDocumentLanguage(Document doc)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            TextDocument textDoc = doc.Object("TextDocument") as TextDocument;

            if (textDoc == null)
            {
                return string.Empty;
            }

            return textDoc.Language.ToLower();
        }

        /// <summary>
        /// The solution path.
        /// </summary>
        /// <param name="applicationObject">
        /// The application object.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string SolutionPath(DTE2 applicationObject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return applicationObject != null ? Path.GetDirectoryName(applicationObject.Solution.FullName) : string.Empty;
        }

        /// <summary>
        ///     The get active file full path 1.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string ActiveFileFullPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                Document doc = environment.ActiveDocument;
                return doc != null ? GetProperFilePathCapitalization(doc.FullName) : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     The get active project from solution full name.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string ActiveProjectFileFullPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (environment == null)
            {
                return string.Empty;
            }

            Array projects = (Array)environment.ActiveSolutionProjects;
            return GetProperFilePathCapitalization(((Project)projects.GetValue(0)).FullName);
        }

        /// <summary>
        ///     The get active project from solution name.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string ActiveProjectName()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (environment == null)
            {
                return string.Empty;
            }

            Array projects = (Array)environment.ActiveSolutionProjects;
            return ((Project)projects.GetValue(0)).Name;
        }

        /// <summary>
        ///     The get solution path.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string ActiveSolutionFullName()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (environment == null)
            {
                return string.Empty;
            }

            return environment.Solution.FullName;
        }

        public string ActiveSolutionFileNameWithExtension()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (environment == null)
            {
                return string.Empty;
            }

            return Path.GetFileName(environment.Solution.FullName);
        }

        /// <summary>
        ///     The get solution path.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string ActiveSolutioRootPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (environment == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(environment.Solution.FullName))
            {
                return string.Empty;
            }

            return Path.GetDirectoryName(GetProperFilePathCapitalization(environment.Solution.FullName));
        }

        /// <summary>
        ///     The are we running in visual studio.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool AreWeRunningInVisualStudio()
        {
            return environment != null;
        }

        /// <summary>
        ///     The get document language.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string CurrentSelectedDocumentLanguage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (environment == null)
            {
                return string.Empty;
            }

            Document doc = environment.ActiveDocument;

            if (doc == null)
            {
                return string.Empty;
            }

            TextDocument textDoc = doc.Object("TextDocument") as TextDocument;
            return textDoc == null ? string.Empty : textDoc.Language.ToLower();
        }

        /// <summary>
        ///     The get environment.
        /// </summary>
        /// <returns>
        ///     The <see cref="DTE2" />.
        /// </returns>
        public DTE2 Environment()
        {
            return environment;
        }

        /// <summary>
        ///     The get current text in view.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string GetCurrentTextInView()
        {
            IWpfTextView view = GetCurrentView();
            return view == null ? string.Empty : view.TextBuffer.CurrentSnapshot.GetText();
        }

        /// <summary>
        ///     The get current view.
        /// </summary>
        /// <returns>
        ///     The <see cref="IWpfTextView" />.
        /// </returns>
        public IWpfTextView GetCurrentView()
        {
            IWpfTextViewHost viewHost = TextViewHost;
            if (viewHost != null)
            {
                return TextViewHost.TextView;
            }

            return null;
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
            ThreadHelper.ThrowIfNotOnUIThread();
            string solutionPath = ActiveSolutioRootPath();
            string driveLetter = solutionPath.Substring(0, 1);
            return driveLetter + fileInView.Substring(1);
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
        public string GetProperFilePathCapitalization(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            DirectoryInfo dirInfo = fileInfo.Directory;
            return dirInfo != null ? Path.Combine(GetProperDirectoryCapitalization(dirInfo), dirInfo.GetFiles(fileInfo.Name)[0].Name) : string.Empty;
        }

        /// <summary>
        /// The navigate to resource.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        public void NavigateToResource(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Win32Exception)
            {
                Process.Start("IExplore.exe", url);
            }
        }

        /// <summary>
        /// The open resource in visual studio.
        /// </summary>
        /// <param name="workfolder">
        /// The workfolder.
        /// </param>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <param name="line">
        /// The line.
        /// </param>
        /// <param name="editorCommandExec">
        /// The editor Command Exec.
        /// </param>
        public void OpenResourceInVisualStudio(string workfolder, string filename, int line, string editorCommandExec = "notepad")
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (environment == null)
            {
                if (!string.IsNullOrEmpty(workfolder) && !Path.IsPathRooted(filename))
                {
                    if (!File.Exists(Path.Combine(workfolder, filename)))
                    {
                        List<string> filesData = new List<string>();
                        RecursePath(filename, workfolder, filesData);

                        foreach (string file in filesData)
                        {
                            string file1 = file;
                            (new Thread(() => Process.Start(editorCommandExec, file1))).Start();
                        }
                    }
                    else
                    {
                        (new Thread(() => Process.Start(editorCommandExec, Path.Combine(workfolder, filename)))).Start();
                    }
                }

                return;
            }

            if (File.Exists(filename))
            {
                try
                {
                    environment.ItemOperations.OpenFile(filename);
                    TextSelection textSelection = (TextSelection)environment.ActiveDocument.Selection;
                    textSelection.GotoLine(line < 1 ? 1 : line);
                    textSelection.SelectLine();
                }
                catch (Exception ex)
                {
                    ErroMessage = "Exception Occured: " + filename + " : " + Convert.ToString(line) + " ex: " + ex.Message;
                }

                return;
            }

            ProjectItem files = environment.Solution.FindProjectItem(filename);
            if (files != null)
            {
                (new Thread(
                    () =>
                        {
                            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                            try
                            {
                                object masterPath = files.Properties.Item("FullPath").Value;
                                environment.ItemOperations.OpenFile(masterPath.ToString());
                                TextSelection textSelection = (TextSelection)environment.ActiveDocument.Selection;
                                textSelection.GotoLine(line < 1 ? 1 : line);
                                textSelection.SelectLine();
                            }
                            catch
                            {
                                try
                                {
                                    if (File.Exists(filename))
                                    {
                                        environment.ItemOperations.OpenFile(filename);
                                        TextSelection textSelection = (TextSelection)environment.ActiveDocument.Selection;
                                        textSelection.GotoLine(line < 1 ? 1 : line);
                                        textSelection.SelectLine();
                                    }
                                    else
                                    {
                                        ErroMessage = "Cannot Open File: " + filename;
                                    }
                                }
                                catch (Exception ex1)
                                {
                                    ErroMessage = "Exception Occured: " + filename + " : " + Convert.ToString(line) + " ex: " + ex1.Message;
                                }
                            }
                        })).Start();
            }
            else
            {
                try
                {
                    environment.ItemOperations.OpenFile(filename);
                }
                catch (Exception ex)
                {
                    ErroMessage = "Exception Occured: " + filename + " : " + Convert.ToString(line) + " ex: " + ex.Message;
                }
            }
        }

        /// <summary>
        /// The open resource in visual studio.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="line">The line.</param>
        /// <param name="editorCommandExec">The editor command exec.</param>
        public void OpenResourceInVisualStudio(string filename, int line, string editorCommandExec = "notepad")
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (environment == null)
            {
                (new Thread(() => Process.Start(editorCommandExec, filename))).Start();
                return;
            }

            ProjectItem files = environment.Solution.FindProjectItem(filename);
            if (files != null)
            {
                (new Thread(
                    () =>
                    {
                        Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                        try
                        {
                            object masterPath = files.Properties.Item("FullPath").Value;
                            environment.ItemOperations.OpenFile(masterPath.ToString());
                            TextSelection textSelection = (TextSelection)environment.ActiveDocument.Selection;
                            textSelection.GotoLine(line < 1 ? 1 : line);
                            textSelection.SelectLine();
                        }
                        catch
                        {
                            try
                            {
                                if (File.Exists(filename))
                                {
                                    environment.ItemOperations.OpenFile(filename);
                                    TextSelection textSelection = (TextSelection)environment.ActiveDocument.Selection;
                                    textSelection.GotoLine(line < 1 ? 1 : line);
                                    textSelection.SelectLine();
                                }
                                else
                                {
                                    MessageBox.Show("Cannot Open File: " + filename);
                                }
                            }
                            catch (Exception ex1)
                            {
                                MessageBox.Show("Cannot Open File: " + ex1.Message);
                            }
                        }
                    })).Start();
            }
            else
            {
                try
                {
                    environment.ItemOperations.OpenFile(filename);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cannot Open File: " + ex.Message);
                }
            }
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
            ThreadHelper.ThrowIfNotOnUIThread();
            if (environment == null)
            {
                return string.Empty;
            }

            Properties props = environment.Properties[category, page];
            try
            {
                string data = props.Item(item).Value as string;
                return data;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     The restart visual studio.
        /// </summary>
        public void RestartVisualStudio()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsShell4 obj = (IVsShell4)provider.GetService(typeof(SVsShell));
            if (obj == null)
            {
                throw new NullReferenceException($"couldnt get IVsShell4");
            }

            obj.Restart((uint)__VSRESTARTTYPE.RESTART_Normal);
        }

        /// <summary>
        /// The show source diff.
        /// </summary>
        /// <param name="resourceInEditor">
        /// The resource in editor.
        /// </param>
        /// <param name="documentInViewPath">
        /// The document in view path.
        /// </param>
        public void ShowSourceDiff(string resourceInEditor, string documentInViewPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsDifferenceService diff = (IVsDifferenceService)provider.GetService(typeof(SVsDifferenceService));
            if (diff == null)
            {
                throw new NullReferenceException($"couldnt get IVsDifferenceService");
            }

            if (!Directory.Exists(TempDataFolder))
            {
                Directory.CreateDirectory(TempDataFolder);
            }

            string tempfile = Path.Combine(TempDataFolder, "server." + Path.GetFileName(documentInViewPath));
            string tempfile2 = Path.Combine(TempDataFolder, "local." + Path.GetFileName(documentInViewPath));

            File.WriteAllText(tempfile, resourceInEditor);
            File.WriteAllText(tempfile2, File.ReadAllText(documentInViewPath));

            diff.OpenComparisonWindow(tempfile, tempfile2);
        }

        /// <summary>
        /// The clear diff file.
        /// </summary>
        /// <param name="localFileName">The local file name.</param>
        /// <param name="serverFileName">The server file name.</param>
        public void ClearDiffFile(string localFileName, string serverFileName)
        {
            string tempfile = Path.Combine(TempDataFolder, localFileName);
            if (File.Exists(tempfile))
            {
                File.Delete(tempfile);
            }

            string tempfile2 = Path.Combine(TempDataFolder, serverFileName);
            if (File.Exists(tempfile2))
            {
                File.Delete(tempfile2);
            }
        }

        /// <summary>The vs project item.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="associatedProject">The associated project.</param>
        /// <returns>The <see cref="VsProjectItem"/>.</returns>
        public VsProjectItem VsProjectItem(string filename, Resource associatedProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (string.IsNullOrEmpty(filename))
            {
                return null;
            }

            string driveLetter = filename.Substring(0, 1);
            string projectName = Path.GetFileNameWithoutExtension(filename);
            string projectPath = driveLetter + GetProperFilePathCapitalization(filename).Substring(1);
            string solutionName = ActiveSolutionFileNameWithExtension();
            string solutionPath = driveLetter + ActiveSolutioRootPath().Substring(1);
            VsProjectItem itemToReturn =
                new VsProjectItem
                {
                    ProjectName = projectName,
                    ProjectFilePath = projectPath,
                    Solution =
                        new VsSolutionItem
                        {
                            SolutionFileNameWithExtension = solutionName,
                            SolutionRoot = solutionPath,
                            SonarProject = associatedProject
                        }
                };

            return itemToReturn;
        }

        /// <summary>The vs file item.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="associatedProject">The associated project.</param>
        /// <param name="fileResource">The file resource.</param>
        /// <returns>The <see cref="VsFileItem"/>.</returns>
        public VsFileItem VsFileItem(string filename, Resource associatedProject, Resource fileResource)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                if (string.IsNullOrEmpty(filename))
                {
                    return null;
                }

                string driveLetter = filename.Substring(0, 1);
                ProjectItem item = environment.Solution.FindProjectItem(filename);

                if (item == null)
                {
                    return null;
                }

                string documentName = Path.GetFileName(filename);
                string documentPath = driveLetter + GetProperFilePathCapitalization(filename).Substring(1);
                string projectName = item.ContainingProject.Name;
                string projectPath = driveLetter + GetProperFilePathCapitalization(item.ContainingProject.FullName).Substring(1);
                string solutionPath = driveLetter + ActiveSolutioRootPath().Substring(1);
                VsFileItem itemToReturn = new VsFileItem
                {
                    FileName = documentName,
                    FilePath = documentPath,
                    SonarResource = fileResource,
                    Project =
                        new VsProjectItem
                        {
                            ProjectName = projectName,
                            ProjectFilePath = projectPath,
                            Solution =
                                new VsSolutionItem
                                {
                                    SolutionFileNameWithExtension = ActiveSolutionFileNameWithExtension(),
                                    SolutionRoot = solutionPath,
                                    SonarProject = associatedProject
                                }
                        }
                };

                return itemToReturn;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>The vs file item.</summary>
        /// <param name="fullPath">The full Path.</param>
        /// <param name="projectFullPath">The project Full Path.</param>
        /// <param name="associatedProject">The associated project.</param>
        /// <param name="fileResource">The file resource.</param>
        /// <returns>The <see cref="VsFileItem"/>.</returns>
        public VsFileItem VsFileItem(string fullPath, string projectFullPath, Resource associatedProject, Resource fileResource)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (string.IsNullOrEmpty(fullPath))
            {
                return null;
            }

            string driveLetter = fullPath.Substring(0, 1);
            string documentName = Path.GetFileNameWithoutExtension(fullPath);
            string documentPath = driveLetter + GetProperFilePathCapitalization(fullPath).Substring(1);
            string projectName = Path.GetFileNameWithoutExtension(projectFullPath);
            string projectPath = driveLetter + GetProperFilePathCapitalization(projectFullPath).Substring(1);
            string solutionPath = driveLetter + ActiveSolutioRootPath().Substring(1);
            VsFileItem itemToReturn = new VsFileItem
            {
                FileName = documentName,
                FilePath = documentPath,
                SonarResource = fileResource,
                Project =
                    new VsProjectItem
                    {
                        ProjectName = projectName,
                        ProjectFilePath = projectPath,
                        Solution =
                            new VsSolutionItem
                            {
                                SolutionFileNameWithExtension = ActiveSolutionFileNameWithExtension(),
                                SolutionRoot = solutionPath,
                                SonarProject = associatedProject
                            }
                    }
            };

            return itemToReturn;
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
            ThreadHelper.ThrowIfNotOnUIThread();
            if (environment == null)
            {
                return;
            }

            Properties props = environment.Properties[category, page];
            if (props.Item(item) == null)
            {
                WriteOption(category, page, item, value);
            }
            else
            {
                string data = ReadSavedOption(category, page, item);
                if (string.IsNullOrEmpty(data))
                {
                    WriteOption(category, page, item, value);
                }

                if (ReadSavedOption(category, page, item).Contains("vera++\\vera++"))
                {
                    WriteOption(category, page, item, value);
                }
            }
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
            ThreadHelper.ThrowIfNotOnUIThread();
            if (environment == null)
            {
                return;
            }

            Properties props = environment.Properties[category, page];
            props.Item(item).Value = value;
        }

        /// <summary>
        /// The write to visual studio output.
        /// </summary>
        /// <param name="errorMessage">
        /// The error message.
        /// </param>
        public void WriteToVisualStudioOutput(string errorMessage)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (CustomPane != null)
            {
                CustomPane.OutputString(errorMessage + "\r\n");
                CustomPane.FlushToTaskList();
            }
        }

        /// <summary>
        /// The active configuration.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.String" />.
        /// </returns>
        public string ActiveConfiguration()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (environment == null)
            {
                return string.Empty;
            }

            try
            {
                SolutionConfiguration2 solutionConfiguration2 = (EnvDTE80.SolutionConfiguration2)environment.Solution.SolutionBuild.ActiveConfiguration;
                return solutionConfiguration2.Name;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// The active platform.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.String" />.
        /// </returns>
        public string ActivePlatform()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (environment == null)
            {
                return string.Empty;
            }

            try
            {
                SolutionConfiguration2 solutionConfiguration2 = (EnvDTE80.SolutionConfiguration2)environment.Solution.SolutionBuild.ActiveConfiguration;
                return solutionConfiguration2.PlatformName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the project by name in solution.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="solutionPath">The solution path.</param>
        /// <returns>visual studio project item</returns>
        public VsProjectItem GetProjectByNameInSolution(string name, string solutionPath)
        {
            ProjectTypes.Solution solutiondata = MSBuildHelper.CreateSolutionData(solutionPath);
            foreach (KeyValuePair<Guid, ProjectTypes.Project> project in solutiondata.Projects)
            {
                if (project.Value.Name.ToLower().Equals(name.ToLower()))
                {
                    return CreateVsProjectItem(project.Value);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the project by unique identifier in solution.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="solutionPath">The solution path.</param>
        /// <returns>returns guid for project</returns>
        public VsProjectItem GetProjectByGuidInSolution(string guid, string solutionPath)
        {
            ProjectTypes.Solution solutiondata = MSBuildHelper.CreateSolutionData(solutionPath);
            foreach (KeyValuePair<Guid, ProjectTypes.Project> project in solutiondata.Projects)
            {
                if (project.Value.Guid.ToString().ToLower().Equals(guid.ToLower()))
                {
                    return CreateVsProjectItem(project.Value);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the project guid from path.
        /// </summary>
        /// <param name="projectPath">The project path.</param>
        /// <param name="solutionPath">The solution path.</param>
        /// <returns>
        /// project unique identifier
        /// </returns>
        public string GetGuidForProject(string projectPath, string solutionPath)
        {
            ProjectTypes.Solution solutiondata = MSBuildHelper.CreateSolutionData(solutionPath);
            foreach (KeyValuePair<Guid, ProjectTypes.Project> project in solutiondata.Projects)
            {
                string fullPathOne = Path.GetFullPath(project.Value.Path).ToLower();
                string fullPathTwo = Path.GetFullPath(projectPath).ToLower();
                if (fullPathOne.Equals(fullPathTwo))
                {
                    return project.Value.Guid.ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Evaluated the value for include file.
        /// </summary>
        /// <param name="msbuildProjectFile">The msbuild project file.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>
        /// returns evaluated include
        /// </returns>
        public string EvaluatedValueForIncludeFile(string msbuildProjectFile, string filePath)
        {
            return MSBuildHelper.GetProjectFilePathForFile(msbuildProjectFile, filePath);
        }

        /// <summary>
        /// Sets the current document in view.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        public void SetCurrentDocumentInView(string fullName)
        {
            resourceinview = fullName;
        }

        /// <summary>
        /// Gets the current document in view.
        /// </summary>
        /// <returns>returns current document in view</returns>
        public string GetCurrentDocumentInView()
        {
            return resourceinview;
        }

        /// <summary>
        /// Does the i have admin rights.
        /// </summary>
        /// <returns>
        /// true if admin
        /// </returns>
        public bool DoIHaveAdminRights()
        {
            System.Security.Claims.ClaimsPrincipal principal = WindowsPrincipal.Current;
            bool isadmin = principal.Claims.Any((c) => c.Value == "S-1-5-32-544");
            return isadmin;
        }

        /// <summary>
        /// Gets the current roslyn solution.
        /// </summary>
        /// <returns>
        /// returns current roslyn solution
        /// </returns>
        public Microsoft.CodeAnalysis.Solution GetCurrentRoslynSolution()
        {
            return WorspaceProvider.ProvideCurrentRoslynSolution();
        }

        /// <summary>
        /// Creates the vs project item.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns>return vs project it</returns>
        private static VsProjectItem CreateVsProjectItem(ProjectTypes.Project project)
        {
            VsProjectItem proToRet = new VsProjectItem
            {
                ProjectName = project.Name,
                ProjectFilePath = project.Path
            };
            return proToRet;
        }

        /// <summary>
        /// The get long path name.
        /// </summary>
        /// <param name="shortPath">
        /// The short path.
        /// </param>
        /// <param name="sb">
        /// The sb.
        /// </param>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <returns>
        /// The <see cref="uint"/>.
        /// </returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint GetLongPathName(string shortPath, StringBuilder sb, int buffer);

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
            DirectoryInfo parentDirInfo = dirInfo.Parent;
            return null == parentDirInfo
                       ? dirInfo.Name
                       : Path.Combine(GetProperDirectoryCapitalization(parentDirInfo), parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
        }

        /// <summary>
        /// The recurse path.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <param name="paths">
        /// The paths.
        /// </param>
        private void RecursePath(string fileName, string path, List<string> paths)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                paths.AddRange(from info in dir.GetFiles() where info.FullName.EndsWith(fileName) select info.FullName);

                foreach (DirectoryInfo info in dir.GetDirectories().Where(info => !info.Name.StartsWith(".")))
                {
                    RecursePath(fileName, info.FullName, paths);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Vses the version.
        /// </summary>
        /// <returns></returns>
        public string VsVersion()
        {
            return environment.Version;
        }
    }
}
