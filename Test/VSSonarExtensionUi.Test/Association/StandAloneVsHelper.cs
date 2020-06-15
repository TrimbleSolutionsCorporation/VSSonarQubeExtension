namespace VSSonarExtensionUi.Test.Association
{
    using Microsoft.CodeAnalysis;
    using SonarRestService.Types;
    using System;
    using System.Diagnostics;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>The stand alone vs helper.</summary>
    public class StandAloneVsHelper : IVsEnvironmentHelper
    {
        /// <summary>The write default option.</summary>
        /// <param name="sonarOptions">The sonar options.</param>
        /// <param name="communityOptions">The community options.</param>
        /// <param name="item">The item.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteDefaultOption(string sonarOptions, string communityOptions, string item, string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>The write option.</summary>
        /// <param name="category">The category.</param>
        /// <param name="page">The page.</param>
        /// <param name="item">The item.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteOption(string category, string page, string item, string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>The write to visual studio output.</summary>
        /// <param name="errorMessage">The error message.</param>
        public void WriteToVisualStudioOutput(string errorMessage)
        {
        }

        /// <summary>The navigate to resource.</summary>
        /// <param name="url">The url.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void NavigateToResource(string url)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the unique identifier for project.
        /// </summary>
        /// <param name="projectPath">The project path.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string GetGuidForProject(string projectPath, string solutionFullPath)
        {
            throw new NotImplementedException();
        }

        /// <summary>The open resource in visual studio.</summary>
        /// <param name="workfolder">The workfolder.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="line">The line.</param>
        /// <param name="editorCommandExec">The editor command exec.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void OpenResourceInVisualStudio(
        string workfolder,
        string filename,
        int line,
        string editorCommandExec = "notepad")
        {
            throw new NotImplementedException();
        }

        /// <summary>The open resource in visual studio.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="line">The line.</param>
        /// <param name="editorCommandExec">The editor command exec.</param>
        public void OpenResourceInVisualStudio(string filename, int line, string editorCommandExec = "notepad")
        {
            Process.Start(editorCommandExec, filename);
        }

        /// <summary>The get proper file path capitalization.</summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetProperFilePathCapitalization(string filename)
        {
            throw new NotImplementedException();
        }

        /// <summary>The active project name.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActiveProjectName()
        {
            throw new NotImplementedException();
        }

        /// <summary>The active project file full path.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActiveProjectFileFullPath()
        {
            throw new NotImplementedException();
        }

        /// <summary>The active file full path.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActiveFileFullPath()
        {
            throw new NotImplementedException();
        }

        /// <summary>The current selected document language.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string CurrentSelectedDocumentLanguage()
        {
            throw new NotImplementedException();
        }

        /// <summary>The active solution path.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActiveSolutioRootPath()
        {
            throw new NotImplementedException();
        }

        /// <summary>The active solution name.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActiveSolutionName()
        {
            throw new NotImplementedException();
        }

        /// <summary>The restart visual studio.</summary>
        /// <exception cref="NotImplementedException"></exception>
        public void RestartVisualStudio()
        {
            throw new NotImplementedException();
        }

        /// <summary>The vs project item.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="associatedProject"></param>
        /// <param name="projectResource"></param>
        /// <returns>The <see cref="VsFileItem"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public VsFileItem VsFileItem(string filename, Resource associatedProject, Resource projectResource)
        {
            throw new NotImplementedException();
        }

        public VsFileItem VsFileItem(string fullPath, string projectFullPath, Resource associatedProject, Resource fileResource)
        {
            throw new NotImplementedException();
        }

        public VsProjectItem VsProjectItem(string projectFileName, Resource associatedProject)
        {
            throw new NotImplementedException();
        }

        /// <summary>The get file real path for solution.</summary>
        /// <param name="fileInView">The file in view.</param>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetFileRealPathForSolution(string fileInView)
        {
            throw new NotImplementedException();
        }

        /// <summary>The get current text in view.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetCurrentTextInView()
        {
            throw new NotImplementedException();
        }

        /// <summary>The are we running in visual studio.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool AreWeRunningInVisualStudio()
        {
            return false;
        }

        /// <summary>The show source diff.</summary>
        /// <param name="resourceInEditorTxt">The resource in editor txt.</param>
        /// <param name="documentInViewPath">The document in view path.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void ShowSourceDiff(string resourceInEditorTxt, string documentInViewPath)
        {
            throw new NotImplementedException();
        }

        /// <summary>The read saved option.</summary>
        /// <param name="category">The category.</param>
        /// <param name="page">The page.</param>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ReadSavedOption(string category, string page, string item)
        {
            throw new NotImplementedException();
        }

        /// <summary>The clear diff file.</summary>
        /// <param name="localFileName">The local file name.</param>
        /// <param name="serverFileName">The server file name.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void ClearDiffFile(string localFileName, string serverFileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>The active configuration.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActiveConfiguration()
        {
            throw new NotImplementedException();
        }

        /// <summary>The active platform.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActivePlatform()
        {
            throw new NotImplementedException();
        }


        public VsProjectItem GetProjectByNameInSolution(string projectName, string solutionFullPath)
        {
            throw new NotImplementedException();
        }

        public VsProjectItem GetProjectByGuidInSolution(string projectGuid, string solutionFullPath)
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

        public Solution GetCurrentRoslynSolution()
        {
            throw new NotImplementedException();
        }

        public string VsVersion()
        {
            throw new NotImplementedException();
        }

        public string ActiveSolutionFullName()
        {
            throw new NotImplementedException();
        }

        public string ActiveSolutionFileNameWithExtension()
        {
            throw new NotImplementedException();
        }
    }
}