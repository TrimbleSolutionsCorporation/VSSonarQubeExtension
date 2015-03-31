// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Analyser.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace SonarLocalAnalyser

open System
open System.Runtime.InteropServices
open System.Security.Permissions
open System.Threading
open System.IO
open System.Diagnostics
open System.Collections.Generic
open VSSonarPlugins
open VSSonarPlugins.Types
open VSSonarPlugins.Helpers
open CommonExtensions
open System.Diagnostics
open Microsoft.Build.Utilities
open SonarRestService
open VSSonarQubeCmdExecutor
open FSharp.Collections.ParallelSeq
open System.Reflection

type LanguageType =
   | SingleLang = 0
   | MultiLang = 1

[<ComVisible(false)>]
[<HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)>]
type SonarLocalAnalyser(plugins : System.Collections.Generic.List<IAnalysisPlugin>, restService : ISonarRestService, vsinter : IConfigurationHelper, sconf : ISonarConfiguration) =
    let assemblyRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString()
    let completionEvent = new DelegateEvent<System.EventHandler>()
    let stdOutEvent = new DelegateEvent<System.EventHandler>()
    let jsonReports : System.Collections.Generic.List<String> = new System.Collections.Generic.List<String>()
    let localissues : System.Collections.Generic.List<Issue> = new System.Collections.Generic.List<Issue>()
    let mutable cachedProfiles : Map<string, Profile> = Map.empty
    let syncLock = new System.Object()
    let mutable exec : VSSonarQubeCmdExecutor = new VSSonarQubeCmdExecutor(null, int64(1000 * 60 * 60)) // TODO timeout to be passed as parameter
    let initializationDone = 
        vsinter.WriteSetting(new SonarQubeProperties(Key = GlobalAnalysisIds.ExcludedPluginsKey, Value = "devcockpit,pdfreport,report,scmactivity,views,jira,scmstats", Context = Context.AnalysisGeneral, Owner = OwnersId.AnalysisOwnerId), false, true)
        vsinter.WriteSetting(new SonarQubeProperties(Key = GlobalAnalysisIds.JavaExecutableKey, Value = @"C:\Program Files (x86)\Java\jre7\bin\java.exe", Context = Context.AnalysisGeneral, Owner = OwnersId.AnalysisOwnerId), false, true)
        vsinter.WriteSetting(new SonarQubeProperties(Key = GlobalAnalysisIds.RunnerExecutableKey, Value = Path.Combine(assemblyRunningPath, "SonarRunner", "bin", "sonar-runner.bat"), Context = Context.AnalysisGeneral, Owner = OwnersId.AnalysisOwnerId), false, true)
        vsinter.WriteSetting(new SonarQubeProperties(Key = GlobalAnalysisIds.IsDebugAnalysisOnKey, Value = "false", Context = Context.AnalysisGeneral, Owner = OwnersId.AnalysisOwnerId), false, true)
        vsinter.WriteSetting(new SonarQubeProperties(Key = GlobalAnalysisIds.LocalAnalysisTimeoutKey, Value = "10", Context = Context.AnalysisGeneral, Owner = OwnersId.AnalysisOwnerId), false, true)
        true

    let triggerException(x, msg : string, ex : Exception) = 
        let errorInExecution = new LocalAnalysisEventArgs("LA: ", "INVALID OPTIONS: " + msg, ex)
        completionEvent.Trigger([|x; errorInExecution|])

    let GetAPluginInListThatSupportSingleLanguage(project : Resource, conf : ISonarConfiguration) = 
        (List.ofSeq plugins) |> List.find (fun plugin -> plugin.IsSupported(conf, project)) 

    let GetPluginThatSupportsResource(itemInView : VsProjectItem) =      
        if itemInView = null then        
            raise(new NoFileInViewException())

        let IsSupported(plugin : IAnalysisPlugin, item : VsProjectItem) = 
            let name = plugin.GetPluginDescription().Name
            try
                let prop = vsinter.ReadSetting(Context.AnalysisGeneral, name, GlobalIds.PluginEnabledControlId)
                plugin.IsSupported(item) && prop.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase)
            with
            | ex -> plugin.IsSupported(item)
        try
            (List.ofSeq plugins) |> List.find (fun plugin -> IsSupported(plugin, itemInView)) 
        with
        | ex -> null

    let GetExtensionThatSupportsThisProject(conf : ISonarConfiguration, project : Resource) = 
        let plugin = GetAPluginInListThatSupportSingleLanguage(project, conf)
        if plugin <> null then
            plugin.GetLocalAnalysisExtension(conf)
        else
            null

    let GetExtensionThatSupportsThisFile(vsprojitem : VsProjectItem, conf:ISonarConfiguration) = 
        let plugin = GetPluginThatSupportsResource(vsprojitem)
        if plugin <> null then
            plugin.GetLocalAnalysisExtension(conf)
        else
            null

    let GetQualityProfile(conf:ISonarConfiguration, project:Resource, itemInView:VsProjectItem) =
        if cachedProfiles.ContainsKey(project.Lang) then
            cachedProfiles.[project.Lang]
        else
            
            let profiles = restService.GetQualityProfilesForProject(conf, project.Key)
            let profileResource = restService.GetQualityProfile(conf, project.Key)

            if profileResource.Count > 0 then
                let enabledrules = restService.GetEnabledRulesInProfile(conf, project.Lang, profileResource.[0].Metrics.[0].Data)                            
                restService.GetRulesForProfile(conf, enabledrules.[0])
                cachedProfiles <- cachedProfiles.Add(project.Lang, enabledrules.[0])
                enabledrules.[0]
            else
                if project.Lang = null then
                    let plugin = GetPluginThatSupportsResource(itemInView)
                    project.Lang <- plugin.GetLanguageKey(itemInView)

                let profile = profiles |> Seq.find (fun x -> x.Language = project.Lang)
                if profile <> null then
                    let enabledrules = restService.GetEnabledRulesInProfile(conf, project.Lang, profile.Name)
                    restService.GetRulesForProfile(conf, enabledrules.[0])
                    cachedProfiles <- cachedProfiles.Add(project.Lang, enabledrules.[0])
                    enabledrules.[0]
                else
                    null



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

    let GenerateCommonArgumentsForAnalysis(mode : AnalysisMode, version : double, conf : ISonarConfiguration, project : Resource) =
        let builder = new CommandLineBuilder()
        // mandatory properties
        builder.AppendSwitchIfNotNull("-Dsonar.host.url=", conf.Hostname.Trim())
        if not(String.IsNullOrEmpty(conf.Username)) then
            builder.AppendSwitchIfNotNull("-Dsonar.login=", conf.Username)

        if not(String.IsNullOrEmpty(conf.Password)) then
            builder.AppendSwitchIfNotNull("-Dsonar.password=", conf.Password)

        builder.AppendSwitchIfNotNull("-Dsonar.projectKey=", project.Key)
        builder.AppendSwitchIfNotNull("-Dsonar.projectName=", project.Name)
        builder.AppendSwitchIfNotNull("-Dsonar.projectVersion=", project.Version)

        // optional project properties
        try
            let prop = vsinter.ReadSetting(Context.AnalysisProject, project.Key, "sonar.sourceEncoding")
            builder.AppendSwitchIfNotNull("-Dsonar.sourceEncoding=", prop.Value)
        with
        | ex -> builder.AppendSwitchIfNotNull("-Dsonar.sourceEncoding=", "UTF-8")


        let excludedPlugins = vsinter.ReadSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.ExcludedPluginsKey).Value

        // analysis
        if version >= 4.0 && not(mode.Equals(AnalysisMode.Full)) then
            match mode with
            | AnalysisMode.Incremental -> builder.AppendSwitchIfNotNull("-Dsonar.analysis.mode=", "incremental")
                                          builder.AppendSwitchIfNotNull("-Dsonar.preview.excludePlugins=", excludedPlugins)

            | AnalysisMode.Preview -> builder.AppendSwitchIfNotNull("-Dsonar.analysis.mode=", "preview")
                                      builder.AppendSwitchIfNotNull("-Dsonar.preview.excludePlugins=", excludedPlugins)
            | _ -> ()

        elif version >= 3.4 && not(mode.Equals(AnalysisMode.Full)) then
            match mode with
            | AnalysisMode.Incremental -> raise (new InvalidOperationException("Analysis Method Not Available in this Version of SonarQube"))
            | AnalysisMode.Preview -> builder.AppendSwitchIfNotNull("-Dsonar.dryRun=", "true")
                                      builder.AppendSwitchIfNotNull("-Dsonar.dryRun.excludePlugins=", excludedPlugins)
            | _ -> ()
        elif not(mode.Equals(AnalysisMode.Full)) then
            raise (new InvalidOperationException("Analysis Method Not Available in this Version of SonarQube"))
                   
        builder.AppendSwitch("-X")                    

        builder

    let SetupEnvironment(x, platform : string, configuration : string)=
        let javaexec = vsinter.ReadSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.JavaExecutableKey).Value
        let runnerexec = vsinter.ReadSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.RunnerExecutableKey).Value

        let javahome = GetDoubleParent(x, javaexec)
        let sonarrunnerhome = GetDoubleParent(x, runnerexec)
        let path = GetParent(x, javaexec) + ";" + GetParent(x, runnerexec) + ";" + Environment.GetEnvironmentVariable("PATH")
                   
        if not(Environment.GetEnvironmentVariable("JAVA_HOME") = null) then
            Environment.SetEnvironmentVariable("JAVA_HOME", "")

        if not(Environment.GetEnvironmentVariable("SONAR_RUNNER_HOME") = null) then
            Environment.SetEnvironmentVariable("SONAR_RUNNER_HOME", "")

        if not(Environment.GetEnvironmentVariable("PATH") = null) then
            Environment.SetEnvironmentVariable("PATH", "")

        if not(Environment.GetEnvironmentVariable("Platform") = null) then
            Environment.SetEnvironmentVariable("Platform", "")

        if not(Environment.GetEnvironmentVariable("Configuration") = null) then
            Environment.SetEnvironmentVariable("Configuration", "")

        let cleanPlat = platform.Replace(" ", "")
        let cleanConf = configuration.Replace(" ", "")
        Map.ofList [("PATH", path); ("JAVA_HOME", javahome); ("SONAR_RUNNER_HOME", sonarrunnerhome); ("Platform", cleanPlat); ("Configuration", cleanConf)]


    let monitor = new Object()
    let numberofFilesToAnalyse = ref 0;
      
    member val Abort = false with get, set
    member val ProjectRoot = "" with get, set
    member val Project = null : Resource with get, set
    member val Version = 0.0 with get, set
    member val Conf = null with get, set
    member val ExecutingThread = null with get, set
    member val CurrentAnalysisType = AnalysisMode.File with get, set
    member val RunningLanguageMethod = LanguageType.SingleLang with get, set
    member val ItemInView = null : VsProjectItem with get, set
    member val OnModifyFiles = false with get, set
    member val AnalysisIsRunning = false with get, set

    member x.ProcessExecutionEventComplete(e : EventArgs) =
        let data = e :?> LocalAnalysisEventArgs
        let message = new LocalAnalysisEventArgs("LA: ", data.ErrorMessage, null)
        completionEvent.Trigger([|x; message|])
          
    member x.ProcessOutputPluginData(e : EventArgs) = 
        let data = e :?> LocalAnalysisEventArgs
        let message = new LocalAnalysisEventArgs("LA:", data.ErrorMessage, null)
        stdOutEvent.Trigger([|x; message|])

    member x.ProcessOutputDataReceived(e : DataReceivedEventArgs) =
        try
            if e.Data <> null && not(x.Abort) then
                if e.Data.EndsWith(".json") then
                    let elems = e.Data.Split(' ');
                    jsonReports.Add(elems.[elems.Length-1])

                let isDebugOn = vsinter.ReadSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.IsDebugAnalysisOnKey).Value

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
                        let extension = 
                            if plugin <> null then
                                plugin.GetLocalAnalysisExtension(x.Conf)
                            else
                                null

                        let extension = GetExtensionThatSupportsThisFile(vsprojitem, x.Conf)
                        if extension <> null then
                            let plugin = GetPluginThatSupportsResource(vsprojitem)
                            let project = new Resource()
                            project.Date <- x.Project.Date
                            project.Lang <- x.Project.Lang
                            project.Id <- x.Project.Id
                            project.Key <- x.Project.Key
                            project.Lname <- x.Project.Lname
                            project.Version <- x.Project.Version
                            project.Name <- x.Project.Name
                            project.Qualifier <- x.Project.Qualifier
                            project.Scope <- x.Project.Scope

                            project.Lang <- plugin.GetLanguageKey(vsprojitem)

                            let issues = extension.ExecuteAnalysisOnFile(vsprojitem, GetQualityProfile(x.Conf, project, vsprojitem), project, x.Conf)
                            lock syncLock (
                                fun () -> 
                                    localissues.AddRange(issues)
                                )
            with
            | ex -> ()


    member x.generateCommandArgs(mode : AnalysisMode, version : double, conf : ISonarConfiguration, project : Resource) =
        GenerateCommonArgumentsForAnalysis(mode, version, conf, project).ToString()        
                            
    member x.RunPreviewBuild() =
        try
            jsonReports.Clear()
            let runnerexec = vsinter.ReadSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.RunnerExecutableKey).Value


            let incrementalArgs = x.generateCommandArgs(AnalysisMode.Preview, x.Version, x.Conf, x.Project)
            if not(String.IsNullOrEmpty(x.Conf.Password)) then
                let message = sprintf "[%s] %s %s" x.ProjectRoot runnerexec (incrementalArgs.Replace(x.Conf.Password, "xxxxx"))
                let commandData = new LocalAnalysisEventArgs("CMD: ", message, null)
                stdOutEvent.Trigger([|x; commandData|])               
        
            let errorCode = (exec :> IVSSonarQubeCmdExecutor).ExecuteCommand(runnerexec, incrementalArgs, SetupEnvironment(x,  x.Project.ActivePlatform, x.Project.ActiveConfiguration), x.ProcessOutputDataReceived, x.ProcessOutputDataReceived, x.ProjectRoot)
            if errorCode > 0 then
                let errorInExecution = new LocalAnalysisEventArgs("LA", "Failed To Execute", new Exception("Error Code: " + errorCode.ToString()))
                completionEvent.Trigger([|x; errorInExecution|])               
            else
                completionEvent.Trigger([|x; null|])
        with
        | ex ->
            let errorInExecution = new LocalAnalysisEventArgs("LA", "Exception while executing", ex)
            completionEvent.Trigger([|x; errorInExecution|])

        x.AnalysisIsRunning <- false

    member x.RunFullBuild() =
        try
            jsonReports.Clear()
            localissues.Clear()
            let runnerexec = vsinter.ReadSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.RunnerExecutableKey).Value

            let incrementalArgs = x.generateCommandArgs(AnalysisMode.Full, x.Version, x.Conf, x.Project)

            if not(String.IsNullOrEmpty(x.Conf.Password)) then
                let message = sprintf "[%s] %s %s" x.ProjectRoot runnerexec (incrementalArgs.Replace(x.Conf.Password, "xxxxx"))
                let commandData = new LocalAnalysisEventArgs("TODO SORT KEY", message, null)
                stdOutEvent.Trigger([|x; commandData|])
          
            let errorCode = (exec :> IVSSonarQubeCmdExecutor).ExecuteCommand(runnerexec, incrementalArgs, SetupEnvironment(x,  x.Project.ActivePlatform, x.Project.ActiveConfiguration), x.ProcessOutputDataReceived, x.ProcessOutputDataReceived, x.ProjectRoot)
            if errorCode > 0 then
                let errorInExecution = new LocalAnalysisEventArgs("LA", "Failed To Execute", new Exception("Error Code: " + errorCode.ToString()))
                completionEvent.Trigger([|x; errorInExecution|])               
            else
                completionEvent.Trigger([|x; null|])
        with
        | ex ->
            let errorInExecution = new LocalAnalysisEventArgs("LA", "Exception while executing", ex)
            completionEvent.Trigger([|x; errorInExecution|])

        x.AnalysisIsRunning <- false

    member x.RunIncrementalBuild() = 
        try
            jsonReports.Clear()
            localissues.Clear()
            let runnerexec = vsinter.ReadSetting(Context.AnalysisGeneral, OwnersId.AnalysisOwnerId, GlobalAnalysisIds.RunnerExecutableKey).Value

            let incrementalArgs = x.generateCommandArgs(AnalysisMode.Incremental, x.Version, x.Conf, x.Project)

            if not(String.IsNullOrEmpty(x.Conf.Password)) then
                let message = sprintf "[%s] %s %s" x.ProjectRoot runnerexec (incrementalArgs.Replace(x.Conf.Password, "xxxxx"))
                let commandData = new LocalAnalysisEventArgs("CMD: ", message, null)
                stdOutEvent.Trigger([|x; commandData|])         
         
            let errorCode = (exec :> IVSSonarQubeCmdExecutor).ExecuteCommand(runnerexec, incrementalArgs, SetupEnvironment(x,  x.Project.ActivePlatform, x.Project.ActiveConfiguration), x.ProcessOutputDataReceived, x.ProcessOutputDataReceived, x.ProjectRoot)
            if errorCode > 0 then
                let errorInExecution = new LocalAnalysisEventArgs("LA", "Failed To Execute", new Exception("Error Code: " + errorCode.ToString()))
                completionEvent.Trigger([|x; errorInExecution|])               
            else
                completionEvent.Trigger([|x; null|])
        with
        | ex ->
            let errorInExecution = new LocalAnalysisEventArgs("LA", "Exception while executing", ex)
            completionEvent.Trigger([|x; errorInExecution|])

        x.AnalysisIsRunning <- false

    member x.RunFileAnalysisThread() = 

            let plugin = GetPluginThatSupportsResource(x.ItemInView)
            if plugin = null then
                let errorInExecution = new LocalAnalysisEventArgs("Local Analyser", "Cannot Run Analyis: Not Supported: ", new ResourceNotSupportedException())
                completionEvent.Trigger([|x; errorInExecution|])
                x.AnalysisIsRunning <- false
            else   
                let GetSourceFromServer() = 
                    try
                        let keyOfItem = (x :> ISonarLocalAnalyser).GetResourceKey(x.ItemInView, x.Project, x.Conf, true)
                        let source = restService.GetSourceForFileResource(x.Conf, keyOfItem)
                        VsSonarUtils.GetLinesFromSource(source, "\r\n")
                    with
                    | ex ->
                        try
                            let keyOfItem = (x :> ISonarLocalAnalyser).GetResourceKey(x.ItemInView, x.Project, x.Conf, false)
                            let source = restService.GetSourceForFileResource(x.Conf, keyOfItem)
                            VsSonarUtils.GetLinesFromSource(source, "\r\n")
                        with
                        | ex -> ""
               
                let extension = plugin.GetLocalAnalysisExtension(x.Conf)
                if extension <> null then
                    let profile = GetQualityProfile(x.Conf, x.Project, x.ItemInView)
                    let issues = extension.ExecuteAnalysisOnFile(x.ItemInView, profile, x.Project, x.Conf)

                    lock syncLock (
                        fun () -> 
                            localissues.AddRange(issues)
                        )

                completionEvent.Trigger([|x; null|])
                x.AnalysisIsRunning <- false
                            
    member x.CancelExecution(thread : Thread) =
        if not(obj.ReferenceEquals(exec, null)) then
            (exec :> IVSSonarQubeCmdExecutor).CancelExecution |> ignore
        ()
                   
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

        member x.RunProjectAnalysis(project : VsProjectItem, conf : ISonarConfiguration) =
            ()

        member x.GetIssues(conf : ISonarConfiguration, project : Resource) =
                                        
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
            let _lock = new Object()

            let GetIssuesFromPlugins(plug : IAnalysisPlugin) =
                let extension = plug.GetLocalAnalysisExtension(conf)
                try
                    let AddIssueToLocalIssues(c : Issue) =
                        if extension.IsIssueSupported(c, conf) then
                            lock _lock (fun () -> localissues.Add(c))

                    issues |> PSeq.iter (fun c -> AddIssueToLocalIssues(c))
                with
                | ex -> ()

            List.ofSeq plugins |> Seq.iter (fun m -> GetIssuesFromPlugins(m))

            localissues

        member x.AnalyseFile(itemInView : VsProjectItem, project : Resource, onModifiedLinesOnly : bool, version : double, conf : ISonarConfiguration) =
            x.CurrentAnalysisType <- AnalysisMode.File
            x.Conf <- conf
            x.Version <- version
            x.Abort <- false            
            x.Project <- project
            x.ItemInView <- itemInView
            x.OnModifyFiles <- onModifiedLinesOnly

            if plugins = null then
                raise(new ResourceNotSupportedException())

            if itemInView = null then
                raise(new NoFileInViewException())

            if GetPluginThatSupportsResource(x.ItemInView) = null then
                raise(new ResourceNotSupportedException())
            
            x.AnalysisIsRunning <- true
            localissues.Clear()
            (new Thread(new ThreadStart(x.RunFileAnalysisThread))).Start()

        member x.RunIncrementalAnalysis(project : Resource, version : double, conf : ISonarConfiguration) =
            x.CurrentAnalysisType <- AnalysisMode.Incremental
            x.ProjectRoot <- project.SolutionRoot
            x.Conf <- conf
            x.Version <- version
            x.Abort <- false
            x.Project <- project

            if not(String.IsNullOrEmpty(project.Lang)) then
                x.RunningLanguageMethod <- LanguageType.SingleLang
            else
                x.RunningLanguageMethod <- LanguageType.MultiLang

            localissues.Clear()
            x.AnalysisIsRunning <- true
            (new Thread(new ThreadStart(x.RunIncrementalBuild))).Start()
            
        member x.RunPreviewAnalysis(project : Resource, version : double, conf : ISonarConfiguration) =
            x.CurrentAnalysisType <- AnalysisMode.Preview
            x.ProjectRoot <- project.SolutionRoot
            x.Project <- project
            x.Conf <- conf
            x.Version <- version
            x.Abort <- false

            if not(String.IsNullOrEmpty(project.Lang)) then
                x.RunningLanguageMethod <- LanguageType.SingleLang
            else
                x.RunningLanguageMethod <- LanguageType.MultiLang

            localissues.Clear()
            x.AnalysisIsRunning <- true
            (new Thread(new ThreadStart(x.RunPreviewBuild))).Start()
                                
        member x.RunFullAnalysis(project : Resource,  version : double, conf : ISonarConfiguration) =
            x.CurrentAnalysisType <- AnalysisMode.Full
            x.ProjectRoot <- project.SolutionRoot
            x.Project <- project
            x.Conf <- conf
            x.Version <- version
            x.Abort <- false

            if project.Lang = null then
                x.RunningLanguageMethod <- LanguageType.MultiLang

            localissues.Clear()
            x.AnalysisIsRunning <- true
            (new Thread(new ThreadStart(x.RunFullBuild))).Start()

        member x.StopAllExecution() =
            x.Abort <- true
            x.AnalysisIsRunning <- false
            if x.ExecutingThread = null then
                ()
            else
                if not(obj.ReferenceEquals(exec, null)) then
                    try
                        (exec :> IVSSonarQubeCmdExecutor).CancelExecution |> ignore
                    with
                    | ex -> ()

        member x.IsExecuting() =
            x.AnalysisIsRunning
                
        member x.GetResourceKey(itemInView : VsProjectItem, associatedProject : Resource, conf : ISonarConfiguration, safeIsOn:bool) = 
            if plugins = null then
                raise(new ResourceNotSupportedException())

            if itemInView = null then
                raise(new NoFileInViewException())

            let plugin = GetPluginThatSupportsResource(itemInView)

            plugin.GetResourceKey(itemInView, associatedProject.Key, safeIsOn)

        member x.AssociateWithProject(project : Resource) =

            if project <> null then
                let processLine(line : string) =
                    if line.Contains("=") then
                        let vals = line.Split('=')
                        let key = vals.[0].Trim()
                        let value = vals.[1].Trim()
                        let prop = new SonarQubeProperties(Key = key, Value = value, Context = Context.AnalysisProject, Owner = project.Key)
                        vsinter.WriteSetting(prop, true)
                    ()

                try 
                    let projectFile = vsinter.ReadSetting(Context.AnalysisProject, project.Key, GlobalAnalysisIds.PropertiesFileKey).Value
                    if File.Exists(projectFile) then
                        let lines = File.ReadAllLines(projectFile)
                        lines |> Seq.iter (fun line -> processLine(line))
                with
                | ex -> let projectFile = Path.Combine(project.SolutionRoot, "sonar-project.properties")
                        if File.Exists(projectFile) then
                            vsinter.WriteSetting(new SonarQubeProperties(Key = GlobalAnalysisIds.PropertiesFileKey, Value = "sonar-project.properties", Context = Context.AnalysisProject, Owner = OwnersId.AnalysisOwnerId))
                            let lines = File.ReadAllLines(projectFile)
                            lines |> Seq.iter (fun line -> processLine(line))


