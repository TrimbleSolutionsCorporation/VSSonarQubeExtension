namespace AnalysisPlugin

open VSSonarPlugins.Types
open VSSonarPlugins
open System.IO

type AnalysisPluginVars = 
    static member Key : string = "AnalysisPlugin"

type PluginHelper =
    static member GetKeyWihtBootStrapper(fileItem : VsFileItem, projectKey : string) =
        let filePath = fileItem.FilePath.Replace("\\", "/")
        let solutionPath = fileItem.Project.Solution.SolutionPath.Replace("\\", "/")
        let filerelativePath = filePath.Replace(solutionPath + "/", "")
        let keySplit = projectKey.Split(':').[0]

        let key = 
            if not(fileItem = null) then
                let path = Directory.GetParent(fileItem.Project.ProjectFilePath).ToString().Replace("\\", "/")
                let file = filePath.Replace(path + "/", "")
                projectKey + ":" + fileItem.Project.ProjectName + ":" + file
            else
                let filesplit = filerelativePath.Split('/')
                projectKey + ":" + filesplit.[0] + ":" + filerelativePath.Replace(filesplit.[0] + "/", "")
        key

    static member GetKeyWithoutBootStrapper(fileItem : VsFileItem, projectKey : string, vshelper : IVsEnvironmentHelper) =
            let tounix = vshelper.GetProperFilePathCapitalization(fileItem.FilePath).Replace("\\", "/")
            let driveLetter = tounix.Substring(0, 1)
            let solutionCan = driveLetter + fileItem.Project.Solution.SolutionPath.Replace("\\", "/").Substring(1);
            let fromBaseDir = tounix.Replace(solutionCan + "/", "");
            fileItem.Project.Solution.SonarProject.Key + ":" + fromBaseDir;