namespace SonarLocalAnalyser

open VSSonarPlugins.Types
open VSSonarPlugins
open System.IO
open System.Text
open SonarRestService.Types
open SonarRestService


[<AllowNullLiteral>]
type SonarModule() = 
    // the name
    member val Name : string =  "" with get, set
    member val ProjectName : string = "" with get, set
    member val BaseDir : string = "" with get, set
    member val Sources : string = "" with get, set
    member val SubModules : SonarModule List = List.Empty with get, set
    member val ParentModule : SonarModule = null with get, set

[<AllowNullLiteral>]
type ISQKeyTranslator =     
  abstract member CreateConfiguration : file:string -> unit
  abstract member GetProjectKey : unit -> string 
  abstract member GetProjectName : unit -> string
  abstract member GetSources : unit -> string
  abstract member GetLookupType : unit -> Types.KeyLookupType
  abstract member SetLookupType : key:Types.KeyLookupType -> unit
  abstract member SetProjectKeyAndBaseDir : key:string * path:string * currentBranch:string * solutionPath:string -> unit
  abstract member SetProjectKey : key:string -> unit
  abstract member GetModules : unit -> List<SonarModule>  
  abstract member TranslateKey : key:string * vshelper:IVsEnvironmentHelper * branch:string -> string
  abstract member TranslatePath : key:VsFileItem * vshelper:IVsEnvironmentHelper * rest:ISonarRestService * configuration:ISonarConfiguration -> Resource
  

[<AllowNullLiteral>]
type SQKeyTranslator(notificationManager : INotificationManager) = 
    let mutable projectKey : string = ""
    let mutable projectName : string = ""
    let mutable projectVersion : string = ""
    let mutable solutionPath : string = ""
    let mutable sonarSources : string = ""
    let mutable projectBaseDir : string = ""
    let mutable currentBranch : string = ""
    let mutable modules : SonarModule List = List.Empty

    let mutable lookupType : Types.KeyLookupType = Types.KeyLookupType.Invalid

    let rec SearchModule(modl : SonarModule, moduleName : string) =
        let mutable modReturn : SonarModule = null

        if modl.Name = moduleName then
            modReturn <- modl
        else
            
            for submod in modl.SubModules do
                if modReturn = null then
                    modReturn <- SearchModule(submod, moduleName)
        modReturn

    let getRelativeDirForModuleAgainstPrevious(currePath : string, moduleName : string) = 
        let mutable modl : SonarModule = null
        for submod in modules do
            if modl = null then
                modl <- SearchModule(submod, moduleName)

        if modl <> null then
            modl.BaseDir
        else
            currePath

    let UpdateModule(moduleName : string, baseDir : string, lines : string [], newModule : SonarModule) = 

        newModule.BaseDir <- baseDir
        let moduleData = lines |> Seq.tryFind (fun c -> c.Contains(moduleName + "sonar.projectName="))
        
        match moduleData with
        | Some value -> newModule.ProjectName <- value.Replace(moduleName + "sonar.projectName=", "").Trim()
        | _ -> ()

        let moduleData = lines |> Seq.tryFind (fun c -> c.Contains(moduleName + "sonar.projectBaseDir="))
        match moduleData with
        | Some value -> newModule.BaseDir <- Path.Combine(baseDir, value.Replace(moduleName + "sonar.projectBaseDir=", "").Trim())
        | _ -> ()

        let moduleData = lines |> Seq.tryFind (fun c -> c.Contains(moduleName + "sonar.sources="))
        match moduleData with
        | Some value -> newModule.Sources <- value.Replace(moduleName + "sonar.sources=", "").Trim()
        | _ -> ()

    let CreateKey(currentModule : SonarModule, fileItem : VsFileItem) = 
        let fileName = Path.GetFileName(fileItem.FilePath)
        let mutable builder = currentModule.Name + ":"

        let mutable parent = currentModule.ParentModule

        while parent <> null do
            builder <- parent.Name + ":" + builder
            parent <- parent.ParentModule

        builder + fileItem.FilePath.Replace(currentModule.BaseDir, "").TrimStart(Path.DirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '/')

    let rec searchFilePathinModules(currentModule : SonarModule, fileItem : VsFileItem) =
        
        let mutable moduleToRetrun : SonarModule = null

        if currentModule.SubModules.Length <> 0 then
            for submod in currentModule.SubModules do
                if moduleToRetrun = null then
                    moduleToRetrun <- searchFilePathinModules(submod, fileItem)
        else
            if currentModule.Sources <> "" then
                let normalizePath(path : string) =
                    Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar).ToLower()

                for source in currentModule.Sources.Split(',') do
                    if moduleToRetrun = null then
                        let BaseDirSRC = normalizePath(Path.Combine(currentModule.BaseDir, source))
                        let fileBaseParent = normalizePath(Directory.GetParent(fileItem.FilePath).ToString())
                        if BaseDirSRC.Equals(fileBaseParent) then
                            moduleToRetrun <- currentModule

        moduleToRetrun

    let rec ProcessModules(file : string, curreModule : SonarModule) =
        let based = Directory.GetParent(file).ToString()
        let lines = File.ReadAllLines(file)

        if curreModule <> null then 
            UpdateModule("", based, lines, curreModule)
        else
            projectBaseDir <- Directory.GetParent(file).ToString().TrimEnd(Path.DirectorySeparatorChar)

            let lineElem = lines |> Seq.tryFind (fun c -> c.Contains("sonar.visualstudio.enable=true"))
            match lineElem with
            | Some value -> lookupType <- Types.KeyLookupType.VSBootStrapper
            | _ -> ()

            let moduleData = lines |> Seq.tryFind (fun c -> c.Trim().StartsWith("sonar.projectKey="))
        
            match moduleData with
            | Some value -> projectKey <- value.Replace("sonar.projectKey=", "").Trim()
            | _ -> ()

            let moduleData = lines |> Seq.tryFind (fun c -> c.Trim().StartsWith("sonar.projectName="))
            match moduleData with
            | Some value -> projectName <- value.Replace("sonar.projectName=", "").Trim()
            | _ -> ()

            let moduleData = lines |> Seq.tryFind (fun c -> c.Trim().StartsWith("sonar.projectVersion="))
            match moduleData with
            | Some value -> projectVersion <- value.Replace("sonar.projectVersion=", "").Trim()
            | _ -> ()

            let moduleData = lines |> Seq.tryFind (fun c -> c.Trim().StartsWith("sonar.sources="))
            match moduleData with
            | Some value -> sonarSources <- value.Replace("sonar.sources=", "").Trim()
            | _ -> ()

        let lineElem = lines |> Seq.tryFind (fun c -> c.Contains("sonar.modules="))

        match lineElem with
        | Some line -> let modulesNames = line.Split('=').[1].Trim().Split(',')
                       if curreModule <> null then
                        curreModule.Sources <- ""

                       lookupType <- Types.KeyLookupType.Module
                       for modl in modulesNames do
                           let newModule = new SonarModule()
                           newModule.Name <- modl

                           if curreModule = null then
                                modules <- modules @ [newModule]
                           else
                                newModule.ParentModule <- curreModule
                                curreModule.SubModules <- curreModule.SubModules @ [newModule]

                           let moduleData = lines |> Seq.tryFind (fun c -> c.Contains(modl + ".sonar."))
                           match moduleData with
                           | Some value -> UpdateModule(modl + ".", based, lines, newModule)
                           | _ -> ProcessModules(Path.Combine(based, modl, "sonar-project.properties"), newModule)

        | _ -> if curreModule = null then
                    let lineElem = lines |> Seq.tryFind (fun c -> c.Contains("sonar.visualstudio.enable=true"))
                    match lineElem with
                    | Some value -> lookupType <- Types.KeyLookupType.VSBootStrapper
                    | _ -> ()
               else
                    UpdateModule("", based, lines, curreModule)

    let GetFlatPath(key : string, branch : string) =
        Path.Combine(projectBaseDir, key.Replace(projectKey + branch, "").Replace('/', Path.DirectorySeparatorChar))

    let GetFlatKey(vshelper : IVsEnvironmentHelper, fileItem : VsFileItem) =
        let tounix = vshelper.GetProperFilePathCapitalization(fileItem.FilePath).Replace("\\", "/")
        let driveLetter = tounix.Substring(0, 1)
        let solutionCan = driveLetter + fileItem.Project.Solution.SolutionRoot.Replace("\\", "/").Substring(1)
        let fromBaseDir = tounix.Replace(solutionCan + "/", "")

        if fileItem.Project.Solution.SonarProject <> null then
            fileItem.Project.Solution.SonarProject.Key + ":" + fromBaseDir
        else
            projectKey + ":" + fromBaseDir

    let GetModulePath (key : string, branch : string) =
        let keyWithoutProjectKey = key.Replace(projectKey + branch, "")
        let allModulesPresentInKey =  keyWithoutProjectKey.Split(':')
        let modulesPresentInKey =  Array.sub allModulesPresentInKey 0 (allModulesPresentInKey.Length-1)

        let mutable currPath = projectBaseDir
        for moduleinKey in modulesPresentInKey do
            currPath <- getRelativeDirForModuleAgainstPrevious(currPath, moduleinKey)
        Path.Combine(currPath, allModulesPresentInKey.[allModulesPresentInKey.Length-1].Replace('/', Path.DirectorySeparatorChar))

    let GetModuleKey(vshelper : IVsEnvironmentHelper, fileItem : VsFileItem) =
        let mutable keyOfResource = ""
        for moduled in modules do
            if keyOfResource = "" then
                let okMod = searchFilePathinModules(moduled, fileItem)
                if okMod <> null then
                    keyOfResource <- 
                        if fileItem.Project.Solution.SonarProject <> null then
                            fileItem.Project.Solution.SonarProject.Key + ":" + CreateKey(okMod, fileItem)
                        else
                            projectKey + ":" + CreateKey(okMod, fileItem)
        keyOfResource

    let GetVSBootStrapperPath(key : string, branch : string, vshelper : IVsEnvironmentHelper) =
        try
            let allModulesPresentInKey =  key.Split(':')
            notificationManager.ReportMessage(new Message(Id = "SQKeyTranslator-VsBootStrapper", Data = "key : " + key + " branch : " + branch))
            let project = vshelper.GetProjectByNameInSolution(allModulesPresentInKey.[allModulesPresentInKey.Length - 2], solutionPath)
            notificationManager.ReportMessage(new Message(Id = "SQKeyTranslator-VsBootStrapper", Data = "project outpath: " + project.OutputPath))
            notificationManager.ReportMessage(new Message(Id = "SQKeyTranslator-VsBootStrapper", Data = "project filepath: " + project.ProjectFilePath))
            let projectfolderpath = Directory.GetParent(project.ProjectFilePath).ToString()
            notificationManager.ReportMessage(new Message(Id = "SQKeyTranslator-VsBootStrapper", Data = "project folder path: " + projectfolderpath))
            let path = Path.Combine(projectfolderpath, allModulesPresentInKey.[allModulesPresentInKey.Length - 1].Replace('/', Path.DirectorySeparatorChar))
            notificationManager.ReportMessage(new Message(Id = "SQKeyTranslator-VsBootStrapper", Data = "translated path: " + path))
            path
        with
        | ex -> notificationManager.ReportMessage(new Message(Id = "SQKeyTranslator-VsBootStrapper", Data = "failed to translate path: " + ex.Message))
                ""

    let GetVSBootStrapperKey(vshelper : IVsEnvironmentHelper, fileItem : VsFileItem) =
        let filePath = fileItem.FilePath.Replace("\\", "/")
        let solutionPath = fileItem.Project.Solution.SolutionRoot.Replace("\\", "/")
        let filerelativePath = filePath.Replace(solutionPath + "/", "")
        let keySplit = projectKey.Split(':').[0]

        notificationManager.ReportMessage(new Message(Id = "SQKeyTranslator-VsBootStrapper", Data = "filepath : " + filePath))
        notificationManager.ReportMessage(new Message(Id = "SQKeyTranslator-VsBootStrapper", Data = "solutionpath : " + solutionPath))
        notificationManager.ReportMessage(new Message(Id = "SQKeyTranslator-VsBootStrapper", Data = "filerelativePath : " + filerelativePath))
        notificationManager.ReportMessage(new Message(Id = "SQKeyTranslator-VsBootStrapper", Data = "projectKey : " + projectKey))

        if not(fileItem = null) then
            let path = Directory.GetParent(fileItem.Project.ProjectFilePath).ToString().Replace("\\", "/")
            let file = filePath.Replace(path + "/", "")
            notificationManager.ReportMessage(new Message(Id = "SQKeyTranslator-VsBootStrapper", Data = "not null translated key : " + projectKey + ":" + fileItem.Project.ProjectName + ":" + file))
            projectKey + ":" + fileItem.Project.ProjectName + ":" + file
        else
            let filesplit = filerelativePath.Split('/')
            notificationManager.ReportMessage(new Message(Id = "SQKeyTranslator-VsBootStrapper", Data = "null translated key : " + projectKey + ":" + filesplit.[0] + ":" + filerelativePath.Replace(filesplit.[0] + "/", "")))
            projectKey + ":" + filesplit.[0] + ":" + filerelativePath.Replace(filesplit.[0] + "/", "")

    let GetMSbuildRunnerPath(key : string, branch : string, vshelper : IVsEnvironmentHelper) = 
        
        let keyWithoutBranch = 
            if branch = null || branch = "" then
                projectKey
            else
                projectKey.Replace(":" + branch, "")

        try
            let keyWithoutProjectKey = key.Replace(keyWithoutBranch + ":", "")
            let allModulesPresentInKey =  keyWithoutProjectKey.Split(':')
            
            let project = Directory.GetParent(vshelper.GetProjectByGuidInSolution(allModulesPresentInKey.[0], solutionPath).ProjectFilePath).ToString()

            if allModulesPresentInKey.Length = 3 then
                Path.Combine(project, allModulesPresentInKey.[2].Replace('/', Path.DirectorySeparatorChar))
            else
                Path.Combine(project, allModulesPresentInKey.[1].Replace('/', Path.DirectorySeparatorChar))
                            
        with
        | ex -> ""

    let GetMSbuildRunnerKey(vshelper : IVsEnvironmentHelper, fileItem : VsFileItem, branch : string) =
        try
            let guid = vshelper.GetGuidForProject(fileItem.Project.ProjectFilePath, solutionPath)
            let tounix = vshelper.GetProperFilePathCapitalization(fileItem.FilePath).Replace("\\", "/")
            let driveLetter = tounix.Substring(0, 1)
            let fromBaseDir =  vshelper.EvaluatedValueForIncludeFile(fileItem.Project.ProjectFilePath, fileItem.FilePath)

            if branch <> "" then
                if fromBaseDir = null || fromBaseDir = "" then
                    ""
                else
                    projectKey.Replace(branch, "") + projectKey.Replace(branch, "") + guid.ToUpper() + ":" + branch + ":" + fromBaseDir.Replace("\\", "/")
            else
                if fromBaseDir = null || fromBaseDir = "" then
                    ""
                else
                    projectKey + ":" + projectKey + ":" + guid.ToUpper() + ":" + fromBaseDir.Replace("\\", "/")
        with
        | ex -> ""

    let GuessLookupTypeFromKey(key : string, branchIn : string, vshelper : IVsEnvironmentHelper) =
        let branch =
            if branchIn = null || branchIn = "" then
                ":"
            elif projectKey.Contains(branchIn) then
                ":"
            else
                ":" + branchIn + ":"

        if lookupType = Types.KeyLookupType.Invalid then
            if File.Exists(GetFlatPath(key, branch)) then
                lookupType <- Types.KeyLookupType.Flat
            elif File.Exists(GetModulePath(key, branch)) then
                lookupType <- Types.KeyLookupType.Module
            elif File.Exists(GetVSBootStrapperPath(key, branch, vshelper)) then
                lookupType <- Types.KeyLookupType.VSBootStrapper
            elif File.Exists(GetMSbuildRunnerPath(key, branchIn, vshelper)) then
                lookupType <- Types.KeyLookupType.ProjectGuid

        lookupType

    let mutable resource : Resource = new Resource()
    let GuessLookupTypeFromPath(vshelper : IVsEnvironmentHelper, fileItem : VsFileItem, rest : ISonarRestService, configuration : ISonarConfiguration) = 
        resource <- new Resource()
        let validateResourceInServer(keyType : Types.KeyLookupType) = 

            let ValidateResourceInServer(key : string) =
                try
                    rest.GetResourcesData(configuration, key).[0]
                with
                | ex -> null

            match keyType with
            | Types.KeyLookupType.Flat -> resource <- ValidateResourceInServer(GetFlatKey(vshelper, fileItem))
                                          resource <> null
            | Types.KeyLookupType.Module -> resource <- ValidateResourceInServer(GetModuleKey(vshelper, fileItem))
                                            resource <> null
            | Types.KeyLookupType.VSBootStrapper -> resource <- ValidateResourceInServer(GetVSBootStrapperKey(vshelper, fileItem))
                                                    resource <> null
            | Types.KeyLookupType.ProjectGuid -> resource <- ValidateResourceInServer(GetMSbuildRunnerKey(vshelper, fileItem, currentBranch))
                                                 resource <> null
            | _ -> lookupType <- Types.KeyLookupType.Invalid
                   false
                  
        let allTags : Types.KeyLookupType seq = unbox (System.Enum.GetValues(typeof<Types.KeyLookupType>))
        let data = allTags |> Seq.tryFind(fun elem -> validateResourceInServer(elem))
        match data with
        | Some elem ->
            lookupType <- elem
            resource.KeyType <- elem
        | _ -> 
            lookupType <- Types.KeyLookupType.Invalid
            // create a dummy resource file to allow local analysis
            resource <- new Resource()
            resource.Key <- GetMSbuildRunnerKey(vshelper, fileItem, currentBranch)
            resource.Qualifier <- "FIL"
            resource.KeyType <- Types.KeyLookupType.ProjectGuid
            resource.FoundInServer <- false

        resource.Lname <- Path.GetFileName(fileItem.FileName)
        resource.Name <- Path.GetFileName(fileItem.FileName)
        resource.SolutionName <- fileItem.Project.Solution.SolutionFileNameWithExtension
        resource.SolutionRoot <- fileItem.Project.Solution.SolutionRoot
        resource.Path <- fileItem.FilePath

        resource

    interface ISQKeyTranslator with
        member this.SetProjectKeyAndBaseDir(key:string, path:string, branch:string, solutionPathIn : string) =
            if branch = null then
                currentBranch <- ""
            else
                currentBranch <- branch

            projectBaseDir <- path
            projectKey <- key
            solutionPath <- solutionPathIn

            ()

        member this.SetProjectKey(key:string) =
            projectKey <- key
            ()

        member this.CreateConfiguration(file : string) = 
            if File.Exists(file) then
                ProcessModules(file, null)
            ()

        member this.GetModules() = 
            modules

        member this.GetLookupType() = 
            lookupType

        member this.SetLookupType(typein : Types.KeyLookupType) = 
            lookupType <- typein
            
        member this.GetProjectKey() =
            projectKey

        member this.GetProjectName() =
            projectName

        member this.GetSources() =
            sonarSources

        member this.TranslatePath(fileItem : VsFileItem, vshelper : IVsEnvironmentHelper, rest : ISonarRestService, configuration : ISonarConfiguration) =
            GuessLookupTypeFromPath(vshelper, fileItem, rest, configuration)

        member this.TranslateKey(key : string, vshelper : IVsEnvironmentHelper, branchIn:string) =

            let branch =
                if branchIn = null || branchIn = "" then
                    ":"
                elif projectKey.Contains(branchIn) then
                    ":"
                else
                    ":" + branchIn + ":"

            match GuessLookupTypeFromKey(key, branchIn, vshelper) with
            | Types.KeyLookupType.Flat -> GetFlatPath(key, branch)
            | Types.KeyLookupType.Module -> GetModulePath(key, branch)
            | Types.KeyLookupType.VSBootStrapper -> GetVSBootStrapperPath(key, branch, vshelper)
            | Types.KeyLookupType.ProjectGuid -> GetMSbuildRunnerPath(key, branchIn, vshelper)
            | _ -> ""
