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
open System.Reflection
open System.ComponentModel
open Microsoft.Win32
open Microsoft.Build.Utilities
open System.Management
open SonarRestService.Types
open SonarRestService

type LanguageType =
   | SingleLang = 0
   | MultiLang = 1

type AnalyserCommandExec(logger : TaskLoggingHelper, timeout : int64) =
    let addEnvironmentVariable (startInfo:ProcessStartInfo) a b = 
        if not(startInfo.EnvironmentVariables.ContainsKey(a)) then
            startInfo.EnvironmentVariables.Add(a, b)

    let output : System.Collections.Generic.List<string> = new System.Collections.Generic.List<string>()
    let error : System.Collections.Generic.List<string> = new System.Collections.Generic.List<string>()

    let KillPrograms(currentProcessName : string) =
        if not(String.IsNullOrEmpty(currentProcessName)) then
            try
                let processId = Process.GetCurrentProcess().Id
                let processes = System.Diagnostics.Process.GetProcessesByName(currentProcessName)

                for proc in processes do
                    if processId <> proc.Id then
                        try
                            Process.GetProcessById(proc.Id).Kill()
                        with
                        | ex -> ()
            with
            | ex -> ()

    let toMap dictionary = 
        (dictionary :> seq<_>)
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq

    member val Logger = logger
    member val stopWatch = Stopwatch.StartNew()
    member val proc : Process  = new Process() with get, set
    member val returncode : ReturnCode = ReturnCode.Ok with get, set
    member val cancelSignal : bool = false with get, set    
    member val Program : string = "" with get, set

    member this.killProcess(pid : int32) : bool =
        let mutable didIkillAnybody = false
        try
            let procs = Process.GetProcesses()
            for proc in procs do
                if this.GetParentProcess(proc.Id) = pid then
                    if this.killProcess(proc.Id) = true then
                        didIkillAnybody <- true

            try
                let myProc = Process.GetProcessById(pid)
                myProc.Kill()
                didIkillAnybody <- true
            with
             | ex -> ()
        with
         | ex -> ()

        didIkillAnybody

    member this.TimerControl() =
        async {
            while not this.cancelSignal do
                if this.stopWatch.ElapsedMilliseconds > timeout then

                    if not(obj.ReferenceEquals(logger, null)) then
                        logger.LogError(sprintf "Expired Timer: %x " this.stopWatch.ElapsedMilliseconds)

                    try
                        if this.killProcess(this.proc.Id) then
                            this.returncode <- ReturnCode.Ok
                        else
                            this.returncode <- ReturnCode.NokAppSpecific
                            //this.proc.Kill()
                    with
                     | ex -> ()

                Thread.Sleep(1000)

            if this.stopWatch.ElapsedMilliseconds > timeout then
                this.returncode <- ReturnCode.Timeout
        }

    member this.ProcessErrorDataReceived(e : DataReceivedEventArgs) =
        this.stopWatch.Restart()
        if not(String.IsNullOrWhiteSpace(e.Data)) then
            error.Add(e.Data)
            System.Diagnostics.Debug.WriteLine("ERROR:" + e.Data)
        ()

    member this.ProcessOutputDataReceived(e : DataReceivedEventArgs) =
        this.stopWatch.Restart()
        if not(String.IsNullOrWhiteSpace(e.Data)) then
            output.Add(e.Data)
            System.Diagnostics.Debug.WriteLine(e.Data)
        ()



    member this.GetParentProcess(Id : int32) = 
        let mutable parentPid = 0
        use mo = new ManagementObject("win32_process.handle='" + Id.ToString() + "'")
        let tmp = mo.Get()
        Convert.ToInt32(mo.["ParentProcessId"])

    member this.CancelExecution() =            
        if this.proc.HasExited = false then
            this.proc.Kill()
        this.cancelSignal <- true
        ReturnCode.Ok

    member this.CancelExecutionAndSpanProcess(processNames : string []) =
            
        if this.proc.HasExited = false then
            processNames |> Array.iter (fun name -> KillPrograms(name))
            if this.proc.HasExited = false then
                this.proc.Kill()

        this.cancelSignal <- true
        ReturnCode.Ok

    member this.ResetData() =
        error.Clear()
        output.Clear()
        ()

    member this.ExecuteCommand(program, args, env, outputHandler, errorHandler, workingDir) =        
        this.Program <- program       
        let startInfo = ProcessStartInfo(FileName = program,
                                            Arguments = args,
                                            WindowStyle = ProcessWindowStyle.Normal,
                                            UseShellExecute = false,
                                            RedirectStandardOutput = true,
                                            RedirectStandardError = true,
                                            RedirectStandardInput = true,
                                            CreateNoWindow = true,
                                            WorkingDirectory = workingDir)
        toMap env |> Map.iter (addEnvironmentVariable startInfo)

        this.proc <- new Process(StartInfo = startInfo,
                                    EnableRaisingEvents = true)
        this.proc.OutputDataReceived.Add(fun c -> outputHandler(c))
        this.proc.ErrorDataReceived.Add(errorHandler)
        let ret = this.proc.Start()

        this.stopWatch.Restart()
        Async.Start(this.TimerControl());

        this.proc.BeginOutputReadLine()
        this.proc.BeginErrorReadLine()
        this.proc.WaitForExit()
        this.cancelSignal <- true
        this.proc.ExitCode


[<ComVisible(false)>]
[<HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)>]
type SonarLocalAnalyser(plugins : System.Collections.Generic.IList<IAnalysisPlugin>, restService : ISonarRestService, vsinter : IConfigurationHelper, notificationManager : INotificationManager, vsinterface : IVsEnvironmentHelper, vsversion : string) =
    let assemblyRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString()
    let stdOutEvent = new DelegateEvent<System.EventHandler>()
    let mutable profileUpdated = false
    let mutable profileCannotBeRetrived = false
    let jsonReports : System.Collections.Generic.List<String> = new System.Collections.Generic.List<String>()
    let cachedProfiles : System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, Profile>> =
                    let data = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, Profile>>()
                    data
    let syncLock = new System.Object()
    let syncLockProfiles = new System.Object()
    let mutable exec : AnalyserCommandExec = new AnalyserCommandExec(null, int64(1000 * 60 * 60)) // TODO timeout to be passed as parameter
    let mutable sonarConfig : ISonarConfiguration = null

    let GetPluginThatSupportsResource(itemInView : VsFileItem) =      
        if itemInView = null then        
            raise(new NoFileInViewException())

        let IsSupported(plugin : IAnalysisPlugin, item : VsFileItem) = 
            let name = plugin.GetPluginDescription().Name
            try
                let prop = vsinter.ReadSetting(Context.GlobalPropsId, name, GlobalIds.PluginEnabledControlId)
                if prop <> null then
                    plugin.IsSupported(item) && prop.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase)
                else
                    plugin.IsSupported(item)
            with
            | ex -> plugin.IsSupported(item)
        try
            (List.ofSeq plugins) |> List.find (fun plugin -> IsSupported(plugin, itemInView)) 
        with
        | ex -> null
      
    let GetQualityProfiles(conf:ISonarConfiguration, project:Resource) =
        if cachedProfiles.ContainsKey(project.Name) then
            notificationManager.ReportMessage(new Message(Id = "Analyser", Data = "Use Cached Version for: " + project.Name))
            profileUpdated <- true
        else
            let profiles = restService.GetQualityProfilesForProject(conf, project)
            for profile in profiles do
                notificationManager.ReportMessage(new Message(Id = "Analyser", Data = "updating profile: " + profile.Name + " : " + profile.Language))
                lock syncLockProfiles (
                    fun () -> 
                        try
                            System.Diagnostics.Debug.WriteLine("Get Profile: " + profile.Name + " : " + profile.Language)
                            restService.GetRulesForProfile(conf, profile, false)
                            if cachedProfiles.ContainsKey(project.Name) then
                                cachedProfiles.[project.Name].Add(profile.Language, profile)
                            else
                                let entry = new System.Collections.Generic.Dictionary<string, Profile>()
                                cachedProfiles.Add(project.Name, entry)
                                cachedProfiles.[project.Name].Add(profile.Language, profile)
                        with
                        | ex -> System.Diagnostics.Debug.WriteLine("cannot add profile: " + ex.Message + " : " + ex.StackTrace)
                    )

        true

    member val Exclusions : System.Collections.Generic.IList<Exclusion> = null with get, set

    member x.GetFileToBeAnalysedFromSonarLog(line : string) =
        let separator = [| "Populating index from" |]
        let mutable fileName = line.Split(separator, StringSplitOptions.RemoveEmptyEntries).[1]

        if fileName.Contains("abs=") then 
            let separator = [| "abs=" |]
            let absfileName = fileName.Split(separator, StringSplitOptions.RemoveEmptyEntries).[1].Trim()
            fileName <- absfileName.TrimEnd(']')

        if fileName.Contains("moduleKey=") || fileName.Contains("oduleKey=") then
            let separator = [| "basedir=" |]
            let baseDirElemts = fileName.Split(separator, StringSplitOptions.RemoveEmptyEntries)
            let baseDir = baseDirElemts.[1].Trim().TrimEnd(']')
            let separator = [| "relative=" |]
            let relativePath = baseDirElemts.[0].Split(separator, StringSplitOptions.RemoveEmptyEntries).[1].Trim().TrimEnd(',')
            fileName <- Path.Combine(baseDir, relativePath)

        fileName    

    member x.RunFileAnalysisOnFile(itemInAnalysis:VsFileItem,
                                   project:Resource,
                                   fromSave:bool,
                                   sonarConfig:ISonarConfiguration,
                                   keyTranslator:ISQKeyTranslator,
                                   vsInter:IVsEnvironmentHelper) = 
        let localissues : System.Collections.Generic.List<Issue> = new System.Collections.Generic.List<Issue>()
        let plugin = GetPluginThatSupportsResource(itemInAnalysis)
        if plugin = null then
            notificationManager.ReportMessage(new Message(Id = "Analyser", Data = "Failed to analyse file : " + itemInAnalysis.FilePath + " : No plugin supports this file"))
            raise (ResourceNotSupportedException())
            
        let extension = plugin.GetLocalAnalysisExtension(sonarConfig)
        if extension = null then
            notificationManager.ReportMessage(new Message(Id = "Analyser", Data = "Failed to analyse file : " + itemInAnalysis.FilePath + " : No available LocalAnalysisExtension "))
            raise (ResourceNotSupportedException())

        if not(cachedProfiles.ContainsKey(project.Name)) then
            notificationManager.ReportMessage(new Message(Id = "Analyser", Data = "Will Associate the Project Before running anlysis"))
            (x :> ISonarLocalAnalyser).AssociateWithProject(project, sonarConfig).RunSynchronously()
            notificationManager.ReportMessage(new Message(Id = "Analyser", Data = "Association Completed"))

        let pluginKey = plugin.GetLanguageKey(itemInAnalysis)
        if not(cachedProfiles.[project.Name].ContainsKey(pluginKey) || (pluginKey.Equals("c++") && cachedProfiles.[project.Name].ContainsKey("cxx"))) then
            notificationManager.ReportMessage(new Message(Id = "Analyser", Data = "No plugin supports the language key : " + pluginKey))
            raise (ResourceNotSupportedException())

        let profile =
            if cachedProfiles.[project.Name].ContainsKey("cxx") then
                cachedProfiles.[project.Name].["cxx"]
            else
                cachedProfiles.[project.Name].[pluginKey]

        let issues = extension.ExecuteAnalysisOnFile(itemInAnalysis, project, sonarConfig, fromSave, profile)
        lock syncLock (
            fun () ->
                let ProcessIssue(issue:Issue, key:string) = 
                    issue.Component <- key
                    if x.Exclusions = null then
                        localissues.Add(issue)
                    else
                        let mutable excluded = false
                        for exclusion in x.Exclusions do
                            if issue.Component.Contains(exclusion.FileRegx) && issue.Rule.Equals(exclusion.RuleRegx) then
                                excluded <- true

                            if issue.Component.Contains(exclusion.FileRegx) && exclusion.RuleRegx.Equals("*") then
                                excluded <- true

                        if not(excluded) then
                            localissues.Add(issue)

                let keydata = keyTranslator.TranslatePath(itemInAnalysis, vsInter, restService, sonarConfig)
                issues |> Seq.iter (fun issue -> ProcessIssue(issue, keydata.Key))
            )

        localissues

    member x.CancelExecution(thread : Thread) =
        if not(obj.ReferenceEquals(exec, null)) then
            exec.CancelExecution |> ignore
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

        member x.UpdateExclusions(exclusions : System.Collections.Generic.IList<Exclusion>) =
            x.Exclusions <- exclusions
            ()

        member x.GetProfile(project : Resource) = 
            cachedProfiles.[project.Name]

        member x.AnalyseFile(itemInView : VsFileItem,
                             project : Resource,
                             conf : ISonarConfiguration,
                             sqTranslator : ISQKeyTranslator,
                             vsInter : IVsEnvironmentHelper,
                             fromSave : bool) =

            if not(profileUpdated) then
                notificationManager.ReportMessage(new Message(Id = "Analyser", Data = "Will Associate the Project Before running anlysis"))
                (x :> ISonarLocalAnalyser).AssociateWithProject(project, conf).RunSynchronously()
                notificationManager.ReportMessage(new Message(Id = "Analyser", Data = "Association Completed"))
                
            if plugins = null then
                raise(new ResourceNotSupportedException())

            if itemInView = null then
                raise(new NoFileInViewException())

            if GetPluginThatSupportsResource(itemInView) = null then
                raise(new ResourceNotSupportedException())

            async {
                if File.Exists(Path.Combine(project.SolutionRoot, ".sonar", "sonar-report.json")) then
                    File.Delete(Path.Combine(project.SolutionRoot, ".sonar", "sonar-report.json"))
                                            
                return x.RunFileAnalysisOnFile(itemInView, project, fromSave, conf, sqTranslator, vsInter)                    
            } |> Async.StartAsTask
            
        member x.GetResourceKey(itemInView : VsFileItem, safeIsOn:bool) = 
            if plugins = null then
                raise(new ResourceNotSupportedException())

            if itemInView = null then
                raise(new NoFileInViewException())

            let plugin = GetPluginThatSupportsResource(itemInView)

            if plugin = null then
                raise(new ResourceNotSupportedException())

            plugin.GetResourceKey(itemInView, safeIsOn)

        member x.OnDisconect() =
            cachedProfiles.Clear()
            profileUpdated <- false
            profileCannotBeRetrived <- false

        member x.AssociateWithProject(project : Resource, conf:ISonarConfiguration) =
            if project = null && conf = null then
                raise (NullReferenceException("Project or Conf Cant be Null"))

            sonarConfig <- conf
            let processLine(line : string) =
                if line.Contains("=") then
                    let vals = line.Split('=')
                    let key = vals.[0].Trim()
                    let value = vals.[1].Trim()
                    let prop = new SonarQubeProperties(Key = key, Value = value, Context = Context.AnalysisProject.ToString(), Owner = project.Key)
                    vsinter.WriteSetting(prop, true)
                ()

            if not(String.IsNullOrEmpty(project.SolutionRoot)) then
                try 
                    let setting = vsinter.ReadSetting(Context.AnalysisGeneral, project.Key, GlobalAnalysisIds.PropertiesFileKey)
                    if setting <> null then
                        let projectFile = setting.Value
                        if File.Exists(projectFile) then
                            let lines = File.ReadAllLines(projectFile)
                            lines |> Seq.iter (fun line -> processLine(line))
                    else
                        let projectFile = Path.Combine(project.SolutionRoot, "sonar-project.properties")
                        if File.Exists(projectFile) then
                            vsinter.WriteSetting(new SonarQubeProperties(Key = GlobalAnalysisIds.PropertiesFileKey, Value = "sonar-project.properties", Context = Context.AnalysisGeneral.ToString(), Owner = OwnersId.AnalysisOwnerId))
                            let lines = File.ReadAllLines(projectFile)
                            lines |> Seq.iter (fun line -> processLine(line))
                with
                | ex -> let projectFile = Path.Combine(project.SolutionRoot, "sonar-project.properties")
                        if File.Exists(projectFile) then
                            vsinter.WriteSetting(new SonarQubeProperties(Key = GlobalAnalysisIds.PropertiesFileKey, Value = "sonar-project.properties", Context = Context.AnalysisGeneral.ToString(), Owner = OwnersId.AnalysisOwnerId))
                            let lines = File.ReadAllLines(projectFile)
                            lines |> Seq.iter (fun line -> processLine(line))

            try
                notificationManager.ReportMessage(new Message(Id = "Analyser", Data = "Start Update profile"))
                async {
                    return GetQualityProfiles(conf, project)
                } |> Async.StartAsTask
                
            with
            | ex ->
                notificationManager.ReportMessage(new Message(Id = "Analyser", Data = "Failed to Update profile"))
                notificationManager.ReportException(ex)
                profileUpdated <- true
                profileCannotBeRetrived <- true
                raise(ex)
   
        member x.GetRuleForKey(key : string, project : Resource) = 
            if cachedProfiles.ContainsKey(project.Name) then
                let profileData = cachedProfiles.[project.Name]

                let ruled = profileData |> Seq.tryFind (fun c -> c.Value.GetRule(key) <> null)
                match ruled with
                | Some(c) ->  c.Value.GetRule(key)
                | _ -> null

            else
                null