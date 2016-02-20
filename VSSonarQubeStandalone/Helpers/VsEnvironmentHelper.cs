using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSonarPlugins;
using VSSonarPlugins.Types;

namespace VSSonarQubeStandalone.Helpers
{
    class VsEnvironmentHelper : IVsEnvironmentHelper
    {
        public string ActiveConfiguration()
        {
            throw new NotImplementedException();
        }

        public string ActiveFileFullPath()
        {
            throw new NotImplementedException();
        }

        public string ActivePlatform()
        {
            throw new NotImplementedException();
        }

        public string ActiveProjectFileFullPath()
        {
            throw new NotImplementedException();
        }

        public string ActiveProjectName()
        {
            throw new NotImplementedException();
        }

        public string ActiveSolutionName()
        {
            throw new NotImplementedException();
        }

        public string ActiveSolutionPath()
        {
            throw new NotImplementedException();
        }

        public bool AreWeRunningInVisualStudio()
        {
            throw new NotImplementedException();
        }

        public void ClearDiffFile(string localFileName, string serverFileName)
        {
            throw new NotImplementedException();
        }

        public string CurrentSelectedDocumentLanguage()
        {
            throw new NotImplementedException();
        }

        public bool DoIHaveAdminRights()
        {
            throw new NotImplementedException();
        }

        public string EvaluatedValueForIncludeFile(string msbuildProjectFile, string filePath)
        {
            throw new NotImplementedException();
        }

        public string GetCurrentDocumentInView()
        {
            throw new NotImplementedException();
        }

        public string GetFileRealPathForSolution(string fileInView)
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

        public VsProjectItem GetProjectByNameInSolution(string projectName, string solutionPath)
        {
            throw new NotImplementedException();
        }

        public string GetProperFilePathCapitalization(string filename)
        {
            throw new NotImplementedException();
        }

        public void NavigateToResource(string url)
        {
            Process.Start(url);
        }

        public void OpenResourceInVisualStudio(string filename, int line, string editorCommandExec = "notepad")
        {
            throw new NotImplementedException();
        }

        public void OpenResourceInVisualStudio(string workfolder, string filename, int line, string editorCommandExec = "notepad")
        {
            throw new NotImplementedException();
        }

        public string ReadSavedOption(string category, string page, string item)
        {
            throw new NotImplementedException();
        }

        public void RestartVisualStudio()
        {
            throw new NotImplementedException();
        }

        public void SetCurrentDocumentInView(string fullName)
        {
            throw new NotImplementedException();
        }

        public void ShowSourceDiff(string resourceInEditorTxt, string documentInViewPath)
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

        public VsProjectItem VsProjectItem(string projectFileName, Resource associatedProject)
        {
            throw new NotImplementedException();
        }

        public void WriteDefaultOption(string sonarOptions, string communityOptions, string item, string value)
        {
            throw new NotImplementedException();
        }

        public void WriteOption(string category, string page, string item, string value)
        {
            throw new NotImplementedException();
        }

        public void WriteToVisualStudioOutput(string errorMessage)
        {
        }
    }
}
