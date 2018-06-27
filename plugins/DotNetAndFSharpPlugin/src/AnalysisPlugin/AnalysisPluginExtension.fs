namespace AnalysisPlugin

open System
open System.Collections.Generic
open System.Runtime.InteropServices
open System.Security.Permissions
open System.Threading
open System.IO
open System.Diagnostics
open VSSonarPlugins
open VSSonarPlugins.Types
open CommonExtensions
open System.Diagnostics
open PluginsOptionsController
open Microsoft.Build.Utilities
open NSonarQubeRunner
open StyleCopRunner
open FxCopRunner
open Microsoft.CodeAnalysis.Diagnostics
open FSharpLintRunner

type AnalysisMode =
   | File = 0
   | Incremental = 1
   | Preview = 2
   | Full = 3

[<ComVisible(false)>]
[<HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)>]
[<AllowNullLiteral>]
type LocalExtension(helper : IConfigurationHelper, notificationManager : INotificationManager, service : ISonarRestService, vshelper : IVsEnvironmentHelper, executor : IVSSonarQubeCmdExecutor) =
    let completionEvent = new DelegateEvent<System.EventHandler>()
    let stdOutEvent = new DelegateEvent<System.EventHandler>()
    let stdErrEvent = new DelegateEvent<System.EventHandler>()
    let monitor = new Object()
    let jsonReports : System.Collections.Generic.List<String> = new System.Collections.Generic.List<String>()
    let mutable allOutData : string list = []
    let fxCopRunner = new FxCopRunner(executor, helper, vshelper, notificationManager)
    let mutable nsqanalyser = new NSonarQubeRunner(helper, notificationManager, service, vshelper)
    let mutable stylecopanalyser = new StyleCopRunner(helper, notificationManager)
    let mutable fsharpAnalyser = new FSharpLintAnalyser(notificationManager)
    let mutable nmbofAnalisedFiles = 0
    let mutable prevFromSave = false
    let mutable exOu = null
    let mutable tries = 5

    let triggerException(x, msg : string, ex : Exception) = 
        let errorInExecution = new LocalAnalysisExceptionEventArgs(AnalysisPluginVars.Key + " " + AnalysisPluginVars.Key + msg, ex)
        completionEvent.Trigger([|x; errorInExecution|])

    let GetDoubleParent(x, path : string) =
        if String.IsNullOrEmpty(path) then
            triggerException(x, "Path not set in options", new Exception("Path Give in Options was not defined, be sure configuration is correctly set"))
            ""
        else
            let data = Directory.GetParent(Directory.GetParent(path).ToString()).ToString()
            if not(Directory.Exists(data)) then
                triggerException(x, "DirectoryNotFound For Required Variable: " + data, new Exception("Directory Not Found"))
            data

    let GetParent(x, path : string) =
        if String.IsNullOrEmpty(path) then
            triggerException(x, "Path not set in options", new Exception("Path Give in Options was not defined, be sure configuration is correctly set"))
            ""
        else
            let data = Directory.GetParent(path).ToString()
            if not(Directory.Exists(data)) then
                triggerException(x, "DirectoryNotFound For Required Variable: " + data, new Exception("Directory Not Found"))
            data


    member val ProjectRoot = "" with get, set

    member x.SetDiagnostics(diagnostics : System.Collections.Generic.List<DiagnosticAnalyzerType>) =
        nsqanalyser.SetDiagnostics(diagnostics)
    
    member x.ProcessErrorDataReceived(e : DataReceivedEventArgs) =
        if e.Data <> null then
            allOutData <- allOutData @ [e.Data]
            let message = new LocalAnalysisStdoutMessage(AnalysisPluginVars.Key + " " + e.Data)
            stdErrEvent.Trigger([|x; message|])

    member x.ProcessOutputDataReceived(e : DataReceivedEventArgs) =
        if e.Data <> null then
            allOutData <- allOutData @ [e.Data]
            let message = new LocalAnalysisStdoutMessage(AnalysisPluginVars.Key + " " + e.Data)
            stdOutEvent.Trigger([|x; message|])

            if e.Data.EndsWith(".json") then
                let elems = e.Data.Split(' ');
                jsonReports.Add(elems.[elems.Length-1])
                                       

    

    member x.AssociatePropject(project : Resource, conf : ISonarConfiguration, externlProfile : System.Collections.Generic.Dictionary<string, Profile>, vsversion:string) = 
        tries <- 5

        let rec update() =
            try
                nsqanalyser.UpdateWorkspace(project, externlProfile, notificationManager, conf, vsversion)
            with
            | ex -> exOu <- ex
                    tries <- tries - 1
                    update()
        update()

        if tries = 0 then
            notificationManager.ReportMessage(new Message(Id = "AnalysisPlugin", Data = "Failed to Refresh Roslyn Workspace" + exOu.Message ))
            notificationManager.ReportException(exOu)

        try
            stylecopanalyser.UpdateSettings(externlProfile)
        with
        | ex -> notificationManager.ReportMessage(new Message(Id = "AnalysisPlugin", Data = "Failed to Refresh StyleCop Workspace: " + ex.Message ))
                notificationManager.ReportException(ex)

        try
            fxCopRunner.UpdateProfile(externlProfile)
        with
        | ex -> notificationManager.ReportMessage(new Message(Id = "AnalysisPlugin", Data = "Failed to Refresh FxCop Workspace: " + ex.Message ))
                notificationManager.ReportException(ex)

        try
            fsharpAnalyser.UpdateProfile(externlProfile)
        with
        | ex -> notificationManager.ReportMessage(new Message(Id = "AnalysisPlugin", Data = "Failed to Refresh FSharpLint Workspace: " + ex.Message ))
                notificationManager.ReportException(ex)

    member x.RunProjectAnalysis(project : VsProjectItem, conf : ISonarConfiguration, plugin : IAnalysisPlugin) =
        fxCopRunner.RunAnalysisOnProject(project, plugin)

    member x.UnloadDomains() =
        stylecopanalyser.UnloadDomain()

    interface IFileAnalyser with

        member x.ExecuteAnalysisOnFile(itemInView : VsFileItem, project : Resource, conf : ISonarConfiguration, fromSave : bool) =

            nmbofAnalisedFiles <- nmbofAnalisedFiles + 1

            if fromSave && nsqanalyser.IsRunning then
                nsqanalyser.CancelAnalysis()

            let guid = Guid.NewGuid()
            nsqanalyser.SetCancelationToken(guid, new CancellationTokenSource())

            let issues = lock monitor (fun () -> 
                let stopWatch = System.Diagnostics.Stopwatch.StartNew()
                notificationManager.ReportMessage(new Message(Id = "AnalysisPlugin", Data = "Start Analysis On File: " + itemInView.FileName ))
                let issues = nsqanalyser.RunAnalysis(itemInView, helper, notificationManager, fromSave, guid)
                notificationManager.ReportMessage(new Message(Id = "AnalysisPlugin", Data = "Current  issue count [roslyn] : " + issues.Count.ToString() + " : in " + stopWatch.Elapsed.TotalMilliseconds.ToString()))
                issues.AddRange(stylecopanalyser.RunAnalysis(itemInView, helper))
                notificationManager.ReportMessage(new Message(Id = "AnalysisPlugin", Data = "Current  issue count [stylecop] : " + issues.Count.ToString() + " : in " + stopWatch.Elapsed.TotalMilliseconds.ToString() ))
                notificationManager.ReportMessage(new Message(Id = "AnalysisPlugin", Data = "Start [fsharplint] : " + stopWatch.Elapsed.TotalMilliseconds.ToString()))
                issues.AddRange(fsharpAnalyser.RunLint(itemInView))
                notificationManager.ReportMessage(new Message(Id = "AnalysisPlugin", Data = "Current  issue count [fsharplint] : " + issues.Count.ToString() + " : in " + stopWatch.Elapsed.TotalMilliseconds.ToString() ))
                // get available project issues
                issues.AddRange(fxCopRunner.GetIssuesForFile(itemInView))
                notificationManager.ReportMessage(new Message(Id = "AnalysisPlugin", Data = "Current  issue count [fxcop] : " + issues.Count.ToString() + " : in " + stopWatch.Elapsed.TotalMilliseconds.ToString() ))
                stopWatch.Stop()
                notificationManager.ReportMessage(new Message(Id = "AnalysisPlugin", Data = "Done in : " + stopWatch.Elapsed.TotalMilliseconds.ToString() + " ms" ))
                issues
            )

            issues

        member x.IsIssueSupported(issue : Issue, conf : ISonarConfiguration) =
            if issue.Component.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase) ||
                issue.Component.EndsWith(".xaml", StringComparison.InvariantCultureIgnoreCase) then
                true
            else
                false

