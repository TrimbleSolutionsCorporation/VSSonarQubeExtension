namespace NSonarQubeRunner

open System.IO
open VSSonarPlugins.Types
open VSSonarPlugins
open System.Text
open System
open System.Reflection
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.Diagnostics
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.MSBuild
open Microsoft.CodeAnalysis.Text
open System.Collections.Immutable
open System.Threading
open System.Collections.Generic
open System.Threading
open System.Diagnostics
open System.Security
open System.Security.Permissions
open Microsoft.Build.Execution
open Microsoft.Build.Evaluation
open MSBuildHelper
open FSharp.Data
open MSBuildHelper

open Types
open LocalHelpers
open ServerHelpers
open Analysers

type NSonarQubeRunner(helper : IConfigurationHelper, notificationManager : INotificationManager, service : ISonarRestService, vshelper : IVsEnvironmentHelper) =
    let CurrentRunDir = Environment.CurrentDirectory
    let mutable checksRoslyn : System.Collections.Generic.List<DiagnosticAnalyzerType> = new System.Collections.Generic.List<DiagnosticAnalyzerType>()
    let mutable currentKey : string = ""
    let mutable externlProfile : Profile = null
    let mutable solution : ProjectTypes.Solution = null
    let mutable ids = List.empty
    let issuesStyle = new System.Collections.Generic.List<Issue>()
    let mutable cancelationTokens = new System.Collections.Generic.Dictionary<Guid, CancellationTokenSource>()
    let mutable isRunning = false
    let mutable openSolutionPath = ""

    let mutable diagnosticsPerProjectData : System.Collections.Generic.Dictionary<string, ProjectData> = new System.Collections.Generic.Dictionary<string, ProjectData>()
    let mutable builder : DiagnosticAnalyzerType list = List.empty
    let mutable profile : System.Collections.Generic.Dictionary<string, Profile> = null
    let mutable synctype : bool = true
    let mutable additionalDocuments : string list = List.Empty
    let mutable workspace = new AdhocWorkspace()
    
    let getMsbuildProjectData(itemInView : VsFileItem) =
        let projects = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(itemInView.Project.ProjectFilePath)
    
        if projects.Count <> 0 then
            let data = projects.GetEnumerator();
            data.MoveNext() |> ignore
            data.Current
        else
                new Microsoft.Build.Evaluation.Project(itemInView.Project.ProjectFilePath)

    let getDocumentsForProject() = 
        for project in solution.Projects do
            ()
        ()

    member x.astAs<'T when 'T : null> (o:obj) = 
        match o with
        | :? 'T as res -> res
        | _ -> null

    member x.SetDiagnostics(diagnostics : System.Collections.Generic.List<DiagnosticAnalyzerType>) =
        checksRoslyn <- diagnostics

    member x.IsRunning = 
        isRunning

    member x.CancelAnalysis() = 
        try
            for token in cancelationTokens do
                token.Value.Cancel()
        with
        | ex -> ()

    member x.SetCancelationToken(guid : Guid, token : CancellationTokenSource) = 
        cancelationTokens.Add(guid, token)

    member x.RunAnalysis(itemInView : VsFileItem, vsHelper : IConfigurationHelper, notificationManager : INotificationManager, fromSave : bool, guid : Guid) =

        let token = cancelationTokens.[guid]
        let mutable issues = new System.Collections.Generic.List<Issue>()
        isRunning <- true

        if not(token.IsCancellationRequested) then 
        
            let languageoffile =  
                if itemInView.FileName.ToLower().EndsWith(".cs") || itemInView.FileName.ToLower().EndsWith(".vb") then
                    if itemInView.FileName.ToLower().EndsWith(".cs") then
                        "cs"
                    else
                        "vbnet"
                else
                    ""
            if languageoffile <> "" && profile.ContainsKey(languageoffile) && solution <> null then
                let profiletouse = profile.[languageoffile]
                    
                if synctype then
                    issues <- runAnalyserWithMSBuild(
                        itemInView,
                        vsHelper,
                        solution,
                        List.toArray additionalDocuments,
                        ids,
                        builder,
                        profiletouse,
                        synctype,
                        token,
                        fromSave,
                        openSolutionPath,
                        vshelper,
                        notificationManager)
                else
                    let projectKey = Path.GetFileNameWithoutExtension(itemInView.Project.ProjectFilePath.ToLower())
                    let projectdata = diagnosticsPerProjectData.[projectKey]

                    
                    let builderrlprop, idsrlprop = GetRoslynDiagnostics(projectdata.Profile, notificationManager, projectdata.Diagnostics)
                    let mutable idsSet = Set.empty
                    let bd, id, set = CombineIds(idsSet, builderrlprop, idsrlprop, builder, ids)
                    try
                        issues <- runAnalyserWithMSBuild(itemInView, vsHelper, solution, projectdata.AdditionDocuments, id, bd, profiletouse, synctype, token, fromSave, openSolutionPath, vshelper, notificationManager)
                    with
                    | ex -> notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner" , Data = "Please Report : Failed to run analysis in file : " + ex.Message))
                            notificationManager.ReportException(ex)

            
        isRunning <- false
        cancelationTokens.Remove(guid) |> ignore
        issues

        
    member this.UpdateWorkspace(resource : Resource, externlProfileIn : System.Collections.Generic.Dictionary<string, Profile>, notificationManager : INotificationManager, configuration : ISonarConfiguration, vsversion:string) =
        profile <- externlProfileIn

        // read roslyn plugin properties server
        let synctypeout, additionalDocsout, pluginInstalled = GetRoslynPluginProps(configuration, resource, service, notificationManager)
        synctype <- synctypeout

        // read solution diagnostics data
        diagnosticsPerProjectData <- UpdateSolutionDiagnosticList(Path.Combine(resource.SolutionRoot, resource.SolutionName), notificationManager, externlProfileIn)

        if synctype then // enforce profile in server
            // get diagnostics from several places
            let buildersl, idssl = GetSonarLintDiagnostics(externlProfileIn, notificationManager, checksRoslyn)
            let builderrlprop, idsrlprop = GetRoslynDiagnostics(externlProfileIn, notificationManager, List.ofSeq checksRoslyn)
            let buildsol, idssolution = GetSolutionDiagnostics(externlProfileIn, notificationManager, diagnosticsPerProjectData)

            // combine diagnostics so we have single ids
            let mutable idsSet = Set.empty
            builder <- buildersl
            ids <- idssl                
            let bd, id, set = CombineIds(idsSet, builderrlprop, idsrlprop, builder, ids)
            builder <- bd
            ids <- id   
            idsSet <- set             
            let bd, id, set = CombineIds(idsSet, buildsol, idssolution, builder, ids)
            builder <- bd
            ids <- id   
            idsSet <- set

            // get additional documents to be passed to analysis
            additionalDocuments <- additionalDocsout
        else // use local data
            let buildersl, idssl = GetSonarLintDiagnostics(externlProfileIn, notificationManager, checksRoslyn)
            builder <- buildersl
            ids <- idssl 

        notificationManager.ReportMessage(new Message(Id = "RoslynRunner", Data = "Checks Available: " +  " : " + checksRoslyn.Count.ToString()))
        notificationManager.ReportMessage(new Message(Id = "RoslynRunner", Data = "Checks Enabled " + builder.Length.ToString()))

        if resource <> null then
            openSolutionPath <- Path.Combine(resource.SolutionRoot, resource.SolutionName);
            let packagesPath = Path.Combine(resource.SolutionRoot, "Packages");
            solution <- MSBuildHelper.PreProcessSolution("", packagesPath, openSolutionPath, true, false, vsversion);
            Analysers.MSbuildOpenSolution <- Analysers.CurrentWorkspace.OpenSolutionAsync(Path.Combine(resource.SolutionRoot, resource.SolutionName)).Result
            








