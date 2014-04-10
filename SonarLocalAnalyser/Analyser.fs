namespace SonarLocalAnalyser

open System
open System.Runtime.InteropServices
open System.Security.Permissions
open System.Threading
open System.IO
open System.Diagnostics
open VSSonarPlugins
open ExtensionTypes
open CommonExtensions
open MSBuild.Tekla.Tasks.Executor
open System.Diagnostics
open Microsoft.Build.Utilities
open SonarRestService
open ExtensionHelpers

type LanguageType =
   | SingleLang = 0
   | MultiLang = 1

[<ComVisible(false)>]
[<HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)>]
type SonarLocalAnalyser(plugins : System.Collections.Generic.List<IPlugin>, restService : ISonarRestService, vsinter : IVsEnvironmentHelper) =
    let completionEvent = new DelegateEvent<System.EventHandler>()
    let stdOutEvent = new DelegateEvent<System.EventHandler>()
    let jsonReports : System.Collections.Generic.List<String> = new System.Collections.Generic.List<String>()
    let localissues : System.Collections.Generic.List<Issue> = new System.Collections.Generic.List<Issue>()
    let syncLock = new System.Object()
    let mutable exec : CommandExecutor = new CommandExecutor(null, int64(1000 * 60 * 60)) // TODO timeout to be passed as parameter

    let triggerException(x, msg : string, ex : Exception) = 
        let errorInExecution = new LocalAnalysisEventArgs("LA: ", "INVALID OPTIONS: " + msg, ex)
        completionEvent.Trigger([|x; errorInExecution|])

    let GetDoubleParent(x, path : string) =
        if String.IsNullOrEmpty(path) then
            triggerException(x, "Path not set in options", new Exception("Path Give in Options was not defined, be sure configuration is correctly set"))
            ""
        else
            let data = Directory.GetParent(Directory.GetParent(path).ToString()).ToString()
            if not(Directory.Exists(data)) then
                triggerException(x, "DirectoryNotFound For Required Variable: " + data, new Exception("Directory Not Foud"))
            data

    let GetParent(x, path : string) =
        if String.IsNullOrEmpty(path) then
            triggerException(x, "Path not set in options", new Exception("Path Give in Options was not defined, be sure configuration is correctly set"))
            ""
        else
            let data = Directory.GetParent(path).ToString()
            if not(Directory.Exists(data)) then
                triggerException(x, "DirectoryNotFound For Required Variable: " + data, new Exception("Directory Not Foud"))
            data

    let GenerateCommonArgumentsForAnalysis(mode : AnalysisMode, version : double, conf : ConnectionConfiguration, plugin : IPlugin, project : Resource) =
        let builder = new CommandLineBuilder()
        // mandatory properties
        builder.AppendSwitchIfNotNull("-Dsonar.host.url=", conf.Hostname)
        if not(String.IsNullOrEmpty(conf.Username)) then
            builder.AppendSwitchIfNotNull("-Dsonar.login=", conf.Username)

        if not(String.IsNullOrEmpty(conf.Password)) then
            builder.AppendSwitchIfNotNull("-Dsonar.password=", conf.Password)

        builder.AppendSwitchIfNotNull("-Dsonar.projectKey=", project.Key)
        builder.AppendSwitchIfNotNull("-Dsonar.projectName=", project.Name)
        builder.AppendSwitchIfNotNull("-Dsonar.projectVersion=", project.Version)

        // mandatory project properties
        let solId = VsSonarUtils.SolutionGlobPropKey(project.Key)
        let optionsInDsk = vsinter.ReadAllAvailableOptionsInSettings(solId)
        builder.AppendSwitchIfNotNull("-Dsonar.sources=", optionsInDsk.[GlobalIds.SonarSourceKey])

        // optional project properties
        builder.AppendSwitchIfNotNull("-Dsonar.sourceEncoding=", optionsInDsk.[GlobalIds.SourceEncodingKey])
        
        // analysis
        if version >= 4.0 && not(mode.Equals(AnalysisMode.Full)) then            
            match mode with
            | AnalysisMode.Incremental -> builder.AppendSwitchIfNotNull("-Dsonar.analysis.mode=", "incremental")
                                          builder.AppendSwitchIfNotNull("-Dsonar.preview.excludePlugins=", vsinter.ReadOptionFromApplicationData(GlobalIds.GlobalPropsId, GlobalIds.ExcludedPluginsKey))
            | AnalysisMode.Preview -> builder.AppendSwitchIfNotNull("-Dsonar.analysis.mode=", "preview")
                                      builder.AppendSwitchIfNotNull("-Dsonar.preview.excludePlugins=", vsinter.ReadOptionFromApplicationData(GlobalIds.GlobalPropsId, GlobalIds.ExcludedPluginsKey))
            | _ -> ()

        elif version >= 3.4 && not(mode.Equals(AnalysisMode.Full)) then
            match mode with
            | AnalysisMode.Incremental -> raise (new InvalidOperationException("Analysis Method Not Available in this Version of SonarQube"))
            | AnalysisMode.Preview -> builder.AppendSwitchIfNotNull("-Dsonar.dryRun=", "true")
                                      builder.AppendSwitchIfNotNull("-Dsonar.dryRun.excludePlugins=", vsinter.ReadOptionFromApplicationData(GlobalIds.GlobalPropsId, GlobalIds.ExcludedPluginsKey))
            | _ -> ()
        elif not(mode.Equals(AnalysisMode.Full)) then
            raise (new InvalidOperationException("Analysis Method Not Available in this Version of SonarQube"))
                   
        builder.AppendSwitch("-X")                    

        builder

    let SetupEnvironment(x)= 
        let javaexec = vsinter.ReadOptionFromApplicationData(GlobalIds.GlobalPropsId, GlobalIds.JavaExecutableKey)
        let runnerexec = vsinter.ReadOptionFromApplicationData(GlobalIds.GlobalPropsId, GlobalIds.RunnerExecutableKey)
        let javahome = GetDoubleParent(x, javaexec)
        let sonarrunnerhome = GetDoubleParent(x, runnerexec)            
        let path = GetParent(x, javaexec) + ";" + GetParent(x, runnerexec) + ";" + Environment.GetEnvironmentVariable("PATH")
                   
        if not(Environment.GetEnvironmentVariable("JAVA_HOME") = null) then
            Environment.SetEnvironmentVariable("JAVA_HOME", "")

        if not(Environment.GetEnvironmentVariable("SONAR_RUNNER_HOME") = null) then
            Environment.SetEnvironmentVariable("SONAR_RUNNER_HOME", "")

        if not(Environment.GetEnvironmentVariable("PATH") = null) then
            Environment.SetEnvironmentVariable("PATH", "")

        Map.ofList [("PATH", path); ("JAVA_HOME", javahome); ("SONAR_RUNNER_HOME", sonarrunnerhome)]

    let GetAPluginInListThatSupportSingleLanguage(res : Resource, conf : ConnectionConfiguration) =              
        (List.ofSeq plugins) |> List.find (fun plugin -> plugin.IsSupported(conf, res)) 

    let GetPluginThatSupportsResource(itemInView : VsProjectItem) =      
        if itemInView = null then        
            raise(new NoFileInViewException())

        let IsSupported(plugin : IPlugin, item : VsProjectItem) = 
            let isEnabled = vsinter.ReadOptionFromApplicationData(GlobalIds.PluginEnabledControlId, plugin.GetPluginDescription(vsinter).Name)
            if String.IsNullOrEmpty(isEnabled) then
                plugin.IsSupported(item)
            else
                plugin.IsSupported(item) && isEnabled.Equals("true", StringComparison.CurrentCultureIgnoreCase)

        try
            (List.ofSeq plugins) |> List.find (fun plugin -> IsSupported(plugin, itemInView)) 
        with
        | ex -> null

    let monitor = new Object()
    let numberofFilesToAnalyse = ref 0;
      
    member val QaProfile = null : Profile with get, set
    member val Abort = false with get, set
    member val ProjectRoot = "" with get, set
    member val Project = null : Resource with get, set
    member val Version = 0.0 with get, set
    member val Conf = null with get, set
    member val ExecutingThread = null with get, set
    member val AnalysisPlugin = null : IPlugin with get, set
    member val AnalysisLocalExtension = null : ILocalAnalyserExtension with get, set
    member val CurrentAnalysisType = AnalysisMode.File with get, set
    member val RunningLanguageMethod = LanguageType.SingleLang with get, set 

    member x.ProcessExecutionEventComplete(e : EventArgs) =
        let data = e :?> LocalAnalysisEventArgs
        let message = new LocalAnalysisEventArgs("LA: ", data.ErrorMessage, null)
        completionEvent.Trigger([|x; message|])
             
    member x.ProcessOutputPluginData(e : EventArgs) = 
        let data = e :?> LocalAnalysisEventArgs
        let message = new LocalAnalysisEventArgs("LA:", data.ErrorMessage, null)
        stdOutEvent.Trigger([|x; message|])

    member x.ProcessOutputDataReceived(e : DataReceivedEventArgs) =
        if e.Data <> null && not(x.Abort) then
            if e.Data.EndsWith(".json") then
                let elems = e.Data.Split(' ');
                jsonReports.Add(elems.[elems.Length-1])

            let isDebugOn = vsinter.ReadOptionFromApplicationData(GlobalIds.GlobalPropsId, GlobalIds.IsDebugAnalysisOnKey)
        
            if e.Data.Contains(" DEBUG - ") then
                if isDebugOn.Equals("True") then
                    let message = new LocalAnalysisEventArgs("LA:", e.Data, null)
                    stdOutEvent.Trigger([|x; message|])
            else
                let message = new LocalAnalysisEventArgs("LA:", e.Data, null)
                stdOutEvent.Trigger([|x; message|])

            if e.Data.Contains("DEBUG - Populating index from") then
                let message = new LocalAnalysisEventArgs("LA:", e.Data, null)
                stdOutEvent.Trigger([|x; message|])
                let separator = [| "Populating index from" |]
                let mutable fileName = e.Data.Split(separator, StringSplitOptions.RemoveEmptyEntries).[1]

                if fileName.Contains("abs=") then 
                    let separator = [| "abs=" |]
                    let absfileName = fileName.Split(separator, StringSplitOptions.RemoveEmptyEntries).[1].Trim()
                    fileName <- absfileName.TrimEnd(']')

                if File.Exists(fileName) then
                    let vsprojitem = new VsProjectItem(Path.GetFileName(fileName), fileName, "", "", "", x.ProjectRoot)
                    let plugin = GetPluginThatSupportsResource(vsprojitem)

                    if plugin <> null then
                        let extension = GetPluginThatSupportsResource(vsprojitem).GetLocalAnalysisExtension(x.Conf, x.Project)
                        if extension <> null then
                            let issues = extension.ExecuteAnalysisOnFile(vsprojitem, x.QaProfile, x.Project)
                            lock syncLock (
                                fun () -> 
                                    localissues.AddRange(issues)
                                )


    member x.generateCommandArgsForMultiLangProject(mode : AnalysisMode, version : double, conf : ConnectionConfiguration, project : Resource) =
        let builder = GenerateCommonArgumentsForAnalysis(mode, version, conf, null, project)

        let AddProperties(extension : ILocalAnalyserExtension) = 
            if extension <> null then
                let argsforplugin = extension.GetLocalAnalysisParamenters()
                for prop in argsforplugin do
                    builder.AppendSwitchIfNotNull("-D" + prop.Key + "=", prop.Value)

        plugins |> Seq.iter (fun plugin -> AddProperties(plugin.GetLocalAnalysisExtension(conf, x.Project)))
        builder.ToString()

    member x.generateCommandArgsForSingleLangProject(mode : AnalysisMode, version : double, conf : ConnectionConfiguration, plugin : IPlugin, project : Resource) =
        let builder = GenerateCommonArgumentsForAnalysis(mode, version, conf, plugin, project)
        if x.AnalysisLocalExtension <> null then
            let argsforplugin = x.AnalysisLocalExtension.GetLocalAnalysisParamenters()
            for prop in argsforplugin do
                builder.AppendSwitchIfNotNull("-D" + prop.Key + "=", prop.Value)

        builder.AppendSwitchIfNotNull("-Dsonar.language=", project.Lang)        
        builder.ToString()
                            
    member x.RunPreviewBuild() =
        try
            jsonReports.Clear()
            localissues.Clear()
            let executable = vsinter.ReadOptionFromApplicationData(GlobalIds.GlobalPropsId, GlobalIds.RunnerExecutableKey)

            let GetArgs = 
                if x.RunningLanguageMethod = LanguageType.SingleLang then
                    x.generateCommandArgsForSingleLangProject(AnalysisMode.Preview, x.Version, x.Conf, x.AnalysisPlugin, x.Project)
                else
                    x.generateCommandArgsForMultiLangProject(AnalysisMode.Preview, x.Version, x.Conf, x.Project)

            let incrementalArgs = GetArgs
            if not(String.IsNullOrEmpty(x.Conf.Password)) then
                let message = sprintf "[%s] %s %s" x.ProjectRoot executable (incrementalArgs.Replace(x.Conf.Password, "xxxxx"))
                let commandData = new LocalAnalysisEventArgs("CMD: ", message, null)
                stdOutEvent.Trigger([|x; commandData|])               
        
            let errorCode = (exec :> ICommandExecutor).ExecuteCommand(executable, incrementalArgs, SetupEnvironment x, x.ProcessOutputDataReceived, x.ProcessOutputDataReceived, x.ProjectRoot)
            if errorCode > 0 then
                let errorInExecution = new LocalAnalysisEventArgs("LA", "Failed To Execute", new Exception("Error Code: " + errorCode.ToString()))
                completionEvent.Trigger([|x; errorInExecution|])               
            else
                completionEvent.Trigger([|x; null|])
        with
        | ex ->
            let errorInExecution = new LocalAnalysisEventArgs("LA", "Exception while executing", ex)
            completionEvent.Trigger([|x; errorInExecution|])

    member x.RunFullBuild() =
        try
            jsonReports.Clear()
            localissues.Clear()
            let executable = vsinter.ReadOptionFromApplicationData(GlobalIds.GlobalPropsId, GlobalIds.RunnerExecutableKey)

            let GetArgs = 
                if x.RunningLanguageMethod = LanguageType.SingleLang then
                    x.generateCommandArgsForSingleLangProject(AnalysisMode.Full, x.Version, x.Conf, x.AnalysisPlugin, x.Project)
                else
                    x.generateCommandArgsForMultiLangProject(AnalysisMode.Full, x.Version, x.Conf, x.Project)

            let incrementalArgs = GetArgs

            if not(String.IsNullOrEmpty(x.Conf.Password)) then
                let message = sprintf "[%s] %s %s" x.ProjectRoot executable (incrementalArgs.Replace(x.Conf.Password, "xxxxx"))
                let commandData = new LocalAnalysisEventArgs("TODO SORT KEY", message, null)
                stdOutEvent.Trigger([|x; commandData|])
          
            let errorCode = (exec :> ICommandExecutor).ExecuteCommand(executable, incrementalArgs, SetupEnvironment x, x.ProcessOutputDataReceived, x.ProcessOutputDataReceived, x.ProjectRoot)
            if errorCode > 0 then
                let errorInExecution = new LocalAnalysisEventArgs("LA", "Failed To Execute", new Exception("Error Code: " + errorCode.ToString()))
                completionEvent.Trigger([|x; errorInExecution|])               
            else
                completionEvent.Trigger([|x; null|])
        with
        | ex ->
            let errorInExecution = new LocalAnalysisEventArgs("LA", "Exception while executing", ex)
            completionEvent.Trigger([|x; errorInExecution|])

    member x.RunIncrementalBuild() =       
        try
            jsonReports.Clear()
            localissues.Clear()
            let executable = vsinter.ReadOptionFromApplicationData(GlobalIds.GlobalPropsId, GlobalIds.RunnerExecutableKey)
            
            let GetArgs = 
                if x.RunningLanguageMethod = LanguageType.SingleLang then
                    x.generateCommandArgsForSingleLangProject(AnalysisMode.Incremental, x.Version, x.Conf, x.AnalysisPlugin, x.Project)
                else
                    x.generateCommandArgsForMultiLangProject(AnalysisMode.Incremental, x.Version, x.Conf, x.Project)

            let incrementalArgs = GetArgs

            if not(String.IsNullOrEmpty(x.Conf.Password)) then
                let message = sprintf "[%s] %s %s" x.ProjectRoot executable (incrementalArgs.Replace(x.Conf.Password, "xxxxx"))
                let commandData = new LocalAnalysisEventArgs("CMD: ", message, null)
                stdOutEvent.Trigger([|x; commandData|])         
         
            let errorCode = (exec :> ICommandExecutor).ExecuteCommand(executable, incrementalArgs, SetupEnvironment x, x.ProcessOutputDataReceived, x.ProcessOutputDataReceived, x.ProjectRoot)
            if errorCode > 0 then
                let errorInExecution = new LocalAnalysisEventArgs("LA", "Failed To Execute", new Exception("Error Code: " + errorCode.ToString()))
                completionEvent.Trigger([|x; errorInExecution|])               
            else
                completionEvent.Trigger([|x; null|])
        with
        | ex ->
            let errorInExecution = new LocalAnalysisEventArgs("LA", "Exception while executing", ex)
            completionEvent.Trigger([|x; errorInExecution|])
                            
    member x.CancelExecution(thread : Thread) =
        if not(obj.ReferenceEquals(exec, null)) then
            (exec :> ICommandExecutor).CancelExecution |> ignore
        ()

    member x.GetPluginThatRunsAnalysisOnSingleLanguageProject(associatedProject : Resource, conf : ConnectionConfiguration) = 
        if plugins = null || plugins.Count = 0 then
            raise(new NoPluginInstalledException())
        else                
            if String.IsNullOrEmpty(associatedProject.Lang) then
                raise(new MultiLanguageExceptionNotSupported())
            else
                GetAPluginInListThatSupportSingleLanguage(associatedProject, conf)

    member x.GetPluginThatRunFileAnalysis(itemInView : VsProjectItem, conf : ConnectionConfiguration) = 
        if plugins = null || plugins.Count = 0 then
            raise(new NoPluginInstalledException())
        else                
            if itemInView = null then
                raise(new NoFileInViewException())
            else
                GetPluginThatSupportsResource(itemInView)
                   
    member x.IsMultiLanguageAnalysis(res : Resource) =

        if plugins = null then
            raise(new NoPluginInstalledException())
        elif res = null then
            raise(new ProjectNotAssociatedException())
        elif String.IsNullOrEmpty(res.Lang) then
            true
        else
            false
           
    interface ISonarLocalAnalyser with
        [<CLIEvent>]
        member x.LocalAnalysisCompleted = completionEvent.Publish

        [<CLIEvent>]
        member x.StdOutEvent = stdOutEvent.Publish

        member x.GetIssues(conf : ConnectionConfiguration) = 
            if x.CurrentAnalysisType = AnalysisMode.File && x.AnalysisLocalExtension <> null then
                x.AnalysisLocalExtension.GetIssues()
            else

                let issues = new System.Collections.Generic.List<Issue>()

                let ParseReport(report : string) =
                    if File.Exists(report) then
                        try
                            issues.AddRange(restService.ParseReportOfIssues(report))
                        with
                        | ex -> try
                                    issues.AddRange(restService.ParseDryRunReportOfIssues(report));
                                with 
                                | ex -> try
                                            issues.AddRange(restService.ParseReportOfIssuesOld(report));
                                        with
                                        | ex -> ()

                List.ofSeq jsonReports |> Seq.iter (fun m -> ParseReport(m))

                if x.AnalysisLocalExtension <> null then
                    localissues.AddRange(x.AnalysisLocalExtension.GetSupportedIssues(issues))
                else
                    let GetIssuesFromPlugins(plug : IPlugin) = 
                        try
                            localissues.AddRange(plug.GetLocalAnalysisExtension(conf, x.Project).GetSupportedIssues(issues))
                        with
                        | ex -> ()                        

                    List.ofSeq plugins |> Seq.iter (fun m -> GetIssuesFromPlugins(m))

                localissues

        member x.AnalyseFile(itemInView : VsProjectItem, project : Resource, profile : Profile, onModifiedLinesOnly : bool, version : double, conf : ConnectionConfiguration) =
            x.CurrentAnalysisType <- AnalysisMode.File
            x.AnalysisLocalExtension <- GetPluginThatSupportsResource(itemInView).GetLocalAnalysisExtension(conf, project)

            let keyOfItem = (x :> ISonarLocalAnalyser).GetResourceKey(itemInView, project, conf)

            let GetSourceFromServer = 
                try
                    let source = restService.GetSourceForFileResource(conf, keyOfItem)
                    VsSonarUtils.GetLinesFromSource(source, "\r\n")
                with
                | ex -> String.Empty

            x.AnalysisLocalExtension.StdOutEvent.Add(x.ProcessOutputPluginData)
            x.AnalysisLocalExtension.LocalAnalysisCompleted.Add(x.ProcessExecutionEventComplete)
            x.ExecutingThread <- x.AnalysisLocalExtension.GetFileAnalyserThread(itemInView, project.Key, profile, GetSourceFromServer, onModifiedLinesOnly)            
            x.ExecutingThread.Start()

        member x.RunIncrementalAnalysis(projectRoot : string, project : Resource, profile : Profile, version : double, conf : ConnectionConfiguration) =
            x.CurrentAnalysisType <- AnalysisMode.Incremental
            x.ProjectRoot <- projectRoot
            x.Conf <- conf
            x.Version <- version
            x.Abort <- false
            x.QaProfile <- profile
            x.Project <- project

            if not(String.IsNullOrEmpty(project.Lang)) then
                x.RunningLanguageMethod <- LanguageType.SingleLang
                x.AnalysisPlugin <- x.GetPluginThatRunsAnalysisOnSingleLanguageProject(project, conf)
                x.AnalysisLocalExtension <- x.AnalysisPlugin.GetLocalAnalysisExtension(conf, project)
                
                (new Thread(new ThreadStart(x.RunIncrementalBuild))).Start()
            else
                x.AnalysisPlugin <- null
                x.AnalysisLocalExtension <- null
                x.RunningLanguageMethod <- LanguageType.MultiLang
                (new Thread(new ThreadStart(x.RunIncrementalBuild))).Start()

            
        member x.RunPreviewAnalysis(projectRoot : string, project : Resource, profile : Profile,  version : double, conf : ConnectionConfiguration) =
            x.CurrentAnalysisType <- AnalysisMode.Preview
            x.ProjectRoot <- projectRoot
            x.Project <- project
            x.Conf <- conf
            x.Version <- version
            x.QaProfile <- profile
            x.Abort <- false

            if not(String.IsNullOrEmpty(project.Lang)) then
                x.AnalysisPlugin <- x.GetPluginThatRunsAnalysisOnSingleLanguageProject(project, conf)
                x.AnalysisLocalExtension <- x.AnalysisPlugin.GetLocalAnalysisExtension(conf, project)
                (new Thread(new ThreadStart(x.RunPreviewBuild))).Start()
            else
                x.AnalysisPlugin <- null
                x.AnalysisLocalExtension <- null
                x.RunningLanguageMethod <- LanguageType.MultiLang
                (new Thread(new ThreadStart(x.RunPreviewBuild))).Start()
                                
        member x.RunFullAnalysis(projectRoot : string, project : Resource,  version : double, conf : ConnectionConfiguration) =
            x.CurrentAnalysisType <- AnalysisMode.Full
            x.ProjectRoot <- projectRoot
            x.Project <- project
            x.Conf <- conf
            x.Version <- version
            x.Abort <- false

            if not(String.IsNullOrEmpty(project.Lang)) then
                x.AnalysisPlugin <- x.GetPluginThatRunsAnalysisOnSingleLanguageProject(project, conf)
                x.AnalysisLocalExtension <- x.AnalysisPlugin.GetLocalAnalysisExtension(conf, project)
                let thread = new Thread(new ThreadStart(x.RunFullBuild))
                thread.Start()
            else
                new Thread(new ThreadStart(x.RunFullBuild)) |> ignore

        member x.StopAllExecution() =
            x.Abort <- true
            if x.ExecutingThread = null || not(x.ExecutingThread.IsAlive) then
                ()
            else
                if not(obj.ReferenceEquals(exec, null)) then
                    try
                        (exec :> ICommandExecutor).CancelExecution |> ignore
                    with
                    | ex -> ()

        member x.IsExecuting() =
            if x.ExecutingThread = null || not(x.ExecutingThread.IsAlive) then
                false
            else
                true
                
        member x.GetResourceKey(itemInView : VsProjectItem, associatedProject : Resource, conf : ConnectionConfiguration) = 
            if plugins = null then
                raise(new ResourceNotSupportedException())

            if itemInView = null then
                raise(new NoFileInViewException())

            GetPluginThatSupportsResource(itemInView).GetResourceKey(itemInView, associatedProject.Key)