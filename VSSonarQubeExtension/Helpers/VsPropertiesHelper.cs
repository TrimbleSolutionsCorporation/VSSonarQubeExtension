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
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;

    using EnvDTE;

    using EnvDTE80;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.TextManager.Interop;

    using SonarRestService.Types;

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
            this.TempDataFolder = Path.GetTempPath();
            this.provider = service;
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
                if (this.textViewHost == null)
                {
                    var textManager = (IVsTextManager)Package.GetGlobalService(typeof(SVsTextManager));
                    textManager.GetActiveView(1, null, out var textView);

                    var data = textView as IVsUserData;
                    if (data != null)
                    {
                        var guid = DefGuidList.guidIWpfTextViewHost;
                        var hr = data.GetData(ref guid, out var obj);
                        if ((hr == VSConstants.S_OK) && obj != null && obj is IWpfTextViewHost)
                        {
                            this.textViewHost = obj as IWpfTextViewHost;
                        }
                    }
                }

                return this.textViewHost;
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
            var textDoc = doc.Object("TextDocument") as TextDocument;

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
                var doc = this.environment.ActiveDocument;
                return doc != null ? this.GetProperFilePathCapitalization(doc.FullName) : string.Empty;
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
            if (this.environment == null)
            {
                return string.Empty;
            }

            var projects = (Array)this.environment.ActiveSolutionProjects;
            return this.GetProperFilePathCapitalization(((Project)projects.GetValue(0)).FullName);
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
            if (this.environment == null)
            {
                return string.Empty;
            }

            var projects = (Array)this.environment.ActiveSolutionProjects;
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
            if (this.environment == null)
            {
                return string.Empty;
            }

            return this.environment.Solution.FullName;
        }

        public string ActiveSolutionFileNameWithExtension()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (this.environment == null)
            {
                return string.Empty;
            }

            return Path.GetFileName(this.environment.Solution.FullName);
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
            if (this.environment == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(this.environment.Solution.FullName))
            {
                return string.Empty;
            }

            return Path.GetDirectoryName(this.GetProperFilePathCapitalization(this.environment.Solution.FullName));
        }

        /// <summary>
        ///     The are we running in visual studio.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool AreWeRunningInVisualStudio()
        {
            return this.environment != null;
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
        ///     The get environment.
        /// </summary>
        /// <returns>
        ///     The <see cref="DTE2" />.
        /// </returns>
        public DTE2 Environment()
        {
            return this.environment;
        }

        /// <summary>
        ///     The get current text in view.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string GetCurrentTextInView()
        {
            var view = this.GetCurrentView();
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
            var viewHost = this.TextViewHost;
            if (viewHost != null)
            {
                return this.TextViewHost.TextView;
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
            var solutionPath = this.ActiveSolutioRootPath();
            var driveLetter = solutionPath.Substring(0, 1);
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
            var fileInfo = new FileInfo(filename);
            var dirInfo = fileInfo.Directory;
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
            if (this.environment == null)
            {
                if (!string.IsNullOrEmpty(workfolder) && !Path.IsPathRooted(filename))
                {
                    if (!File.Exists(Path.Combine(workfolder, filename)))
                    {
                        var filesData = new List<string>();
                        this.RecursePath(filename, workfolder, filesData);

                        foreach (var file in filesData)
                        {
                            var file1 = file;
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
                    this.environment.ItemOperations.OpenFile(filename);
                    var textSelection = (TextSelection)this.environment.ActiveDocument.Selection;
                    textSelection.GotoLine(line < 1 ? 1 : line);
                    textSelection.SelectLine();
                }
                catch (Exception ex)
                {
                    this.ErroMessage = "Exception Occured: " + filename + " : " + Convert.ToString(line) + " ex: " + ex.Message;
                }

                return;
            }

            var files = this.environment.Solution.FindProjectItem(filename);
            if (files != null)
            {
                (new Thread(
                    () =>
                        {
                            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                            try
                            {
                                var masterPath = files.Properties.Item("FullPath").Value;
                                this.environment.ItemOperations.OpenFile(masterPath.ToString());
                                var textSelection = (TextSelection)this.environment.ActiveDocument.Selection;
                                textSelection.GotoLine(line < 1 ? 1 : line);
                                textSelection.SelectLine();
                            }
                            catch
                            {
                                try
                                {
                                    if (File.Exists(filename))
                                    {
                                        this.environment.ItemOperations.OpenFile(filename);
                                        var textSelection = (TextSelection)this.environment.ActiveDocument.Selection;
                                        textSelection.GotoLine(line < 1 ? 1 : line);
                                        textSelection.SelectLine();
                                    }
                                    else
                                    {
                                        this.ErroMessage = "Cannot Open File: " + filename;
                                    }
                                }
                                catch (Exception ex1)
                                {
                                    this.ErroMessage = "Exception Occured: " + filename + " : " + Convert.ToString(line) + " ex: " + ex1.Message;
                                }
                            }
                        })).Start();
            }
            else
            {
                try
                {
                    this.environment.ItemOperations.OpenFile(filename);
                }
                catch (Exception ex)
                {
                    this.ErroMessage = "Exception Occured: " + filename + " : " + Convert.ToString(line) + " ex: " + ex.Message;
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
            if (this.environment == null)
            {
                (new Thread(() => Process.Start(editorCommandExec, filename))).Start();
                return;
            }

            var files = this.environment.Solution.FindProjectItem(filename);
            if (files != null)
            {
                (new Thread(
                    () =>
                    {
                        Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                        try
                        {
                            var masterPath = files.Properties.Item("FullPath").Value;
                            this.environment.ItemOperations.OpenFile(masterPath.ToString());
                            var textSelection = (TextSelection)this.environment.ActiveDocument.Selection;
                            textSelection.GotoLine(line < 1 ? 1 : line);
                            textSelection.SelectLine();
                        }
                        catch
                        {
                            try
                            {
                                if (File.Exists(filename))
                                {
                                    this.environment.ItemOperations.OpenFile(filename);
                                    var textSelection = (TextSelection)this.environment.ActiveDocument.Selection;
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
                    this.environment.ItemOperations.OpenFile(filename);
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
        ///     The restart visual studio.
        /// </summary>
        public void RestartVisualStudio()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var obj = (IVsShell4)this.provider.GetService(typeof(SVsShell));
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
            var diff = (IVsDifferenceService)this.provider.GetService(typeof(SVsDifferenceService));
            if (diff == null)
            {
                throw new NullReferenceException($"couldnt get IVsDifferenceService");
            }

            if (!Directory.Exists(this.TempDataFolder))
            {
                Directory.CreateDirectory(this.TempDataFolder);
            }

            var tempfile = Path.Combine(this.TempDataFolder, "server." + Path.GetFileName(documentInViewPath));
            var tempfile2 = Path.Combine(this.TempDataFolder, "local." + Path.GetFileName(documentInViewPath));

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
            var tempfile = Path.Combine(this.TempDataFolder, localFileName);
            if (File.Exists(tempfile))
            {
                File.Delete(tempfile);
            }

            var tempfile2 = Path.Combine(this.TempDataFolder, serverFileName);
            if (File.Exists(tempfile2))
            {
                File.Delete(tempfile2);
            }
        }

        /// <summary>The vs project item.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="associatedProject">The associated project.</param>
        /// <returns>The <see cref="VsProjectItem"/>.</returns>
        public async Task<VsProjectItem> VsProjectItem(string filename, Resource associatedProject)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (string.IsNullOrEmpty(filename))
            {
                return null;
            }

            var driveLetter = filename.Substring(0, 1);
            var projectName = Path.GetFileNameWithoutExtension(filename);
            var projectPath = driveLetter + this.GetProperFilePathCapitalization(filename).Substring(1);
            var solutionName = this.ActiveSolutionFileNameWithExtension();
            var solutionPath = driveLetter + this.ActiveSolutioRootPath().Substring(1);
            var itemToReturn =
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
        public async Task<VsFileItem> VsFileItem(string filename, Resource associatedProject, Resource fileResource)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
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

                var documentName = Path.GetFileName(filename);
                var documentPath = driveLetter + this.GetProperFilePathCapitalization(filename).Substring(1);
                var projectName = item.ContainingProject.Name;
                var projectPath = driveLetter + this.GetProperFilePathCapitalization(item.ContainingProject.FullName).Substring(1);
                var solutionPath = driveLetter + this.ActiveSolutioRootPath().Substring(1);
                var itemToReturn = new VsFileItem
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
                                    SolutionFileNameWithExtension = this.ActiveSolutionFileNameWithExtension(),
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
        public async Task<VsFileItem> VsFileItem(string fullPath, string projectFullPath, Resource associatedProject, Resource fileResource)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (string.IsNullOrEmpty(fullPath))
            {
                return null;
            }

            var driveLetter = fullPath.Substring(0, 1);
            var documentName = Path.GetFileNameWithoutExtension(fullPath);
            var documentPath = driveLetter + this.GetProperFilePathCapitalization(fullPath).Substring(1);
            var projectName = Path.GetFileNameWithoutExtension(projectFullPath);
            var projectPath = driveLetter + this.GetProperFilePathCapitalization(projectFullPath).Substring(1);
            var solutionPath = driveLetter + this.ActiveSolutioRootPath().Substring(1);
            var itemToReturn = new VsFileItem
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
                                SolutionFileNameWithExtension = this.ActiveSolutionFileNameWithExtension(),
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
            if (this.environment == null)
            {
                return;
            }

            var props = this.environment.Properties[category, page];
            props.Item(item).Value = value;
        }

        /// <summary>
        /// The write to visual studio output.
        /// </summary>
        /// <param name="errorMessage">
        /// The error message.
        /// </param>
#pragma warning disable VSTHRD100 // Avoid async void methods
        public async void WriteToVisualStudioOutput(string errorMessage)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (this.CustomPane != null)
            {
                this.CustomPane.OutputStringThreadSafe(errorMessage + "\r\n");
                this.CustomPane.FlushToTaskList();
            }
        }

        /// <summary>
        /// The active configuration.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.String" />.
        /// </returns>
        public async Task<string> ActiveConfiguration()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (this.environment == null)
            {
                return string.Empty;
            }

            try
            {
                var solutionConfiguration2 = (EnvDTE80.SolutionConfiguration2)this.environment.Solution.SolutionBuild.ActiveConfiguration;
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
        public async Task<string> ActivePlatform()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (this.environment == null)
            {
                return string.Empty;
            }

            try
            {
                var solutionConfiguration2 = (EnvDTE80.SolutionConfiguration2)this.environment.Solution.SolutionBuild.ActiveConfiguration;
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
            var solutiondata = MSBuildHelper.CreateSolutionData(solutionPath);
            foreach (var project in solutiondata.Projects)
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
            var solutiondata = MSBuildHelper.CreateSolutionData(solutionPath);
            foreach (var project in solutiondata.Projects)
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
            var solutiondata = MSBuildHelper.CreateSolutionData(solutionPath);
            foreach (var project in solutiondata.Projects)
            {
                var fullPathOne = Path.GetFullPath(project.Value.Path).ToLower();
                var fullPathTwo = Path.GetFullPath(projectPath).ToLower();
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
            this.resourceinview = fullName;
        }

        /// <summary>
        /// Gets the current document in view.
        /// </summary>
        /// <returns>returns current document in view</returns>
        public string GetCurrentDocumentInView()
        {
            return this.resourceinview;
        }

        /// <summary>
        /// Does the i have admin rights.
        /// </summary>
        /// <returns>
        /// true if admin
        /// </returns>
        public bool DoIHaveAdminRights()
        {
            var principal = WindowsPrincipal.Current;
            var isadmin = principal.Claims.Any((c) => c.Value == "S-1-5-32-544");
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
            var proToRet = new VsProjectItem
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
            var parentDirInfo = dirInfo.Parent;
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
                var dir = new DirectoryInfo(path);
                paths.AddRange(from info in dir.GetFiles() where info.FullName.EndsWith(fileName) select info.FullName);

                foreach (var info in dir.GetDirectories().Where(info => !info.Name.StartsWith(".")))
                {
                    this.RecursePath(fileName, info.FullName, paths);
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
            return this.environment.Version;
        }
    }
}
