namespace RoslynAdapter

open System.Xml
open System.Xml.Linq
open System
open System.Text
open System.IO
open FSharp.Data
open System.Reflection
open Microsoft.CodeAnalysis.CodeFixes
open Microsoft.CodeAnalysis.Diagnostics
open Microsoft.CodeAnalysis
open ExtensionTypes
open ZeroMQ
open System.Threading
open System.Diagnostics

type RoslynAdapter() =     
    let AddDllAnalysisToManifest(assemblyToAdd:string, manifestPath:string) =
        let lines = File.ReadAllLines(manifestPath)
        let mutable linesout = List.Empty
        let added = false
        for line in lines do
            linesout <- linesout @ [line]
            if not(added) && line.Trim().StartsWith("<Asset ") then
                let data = sprintf """    <Asset d:Source="File" Path="diagnostics\%s" d:VsixSubPath="diagnostics" Type="Microsoft.VisualStudio.Analyzer" />""" (Path.GetFileName(assemblyToAdd))
                linesout <- linesout @ [data]

        File.WriteAllLines(manifestPath, linesout)

    let AddDllCodeFixToManifest(assemblyToAdd:string, manifestPath:string) =
        let lines = File.ReadAllLines(manifestPath)
        let mutable linesout = List.Empty
        let added = false
        for line in lines do
            linesout <- linesout @ [line]
            if not(added) && line.Trim().StartsWith("<Asset ") then
                let data = sprintf """    <Asset d:Source="File" Path="diagnostics\%s" d:VsixSubPath="diagnostics" Type="Microsoft.VisualStudio.MefComponent" />""" (Path.GetFileName(assemblyToAdd))
                linesout <- linesout @ [data]

        File.WriteAllLines(manifestPath, linesout)
    
    member val Messages : SubscriberElem List = List.Empty with get, set
    member val EndBroadcaster : bool = false with get, set
    member val BroadcastPeriod : int = 3000 with get, set

    member x.StartPublisher() =
        use context = ZmqContext.Create()
        use publisher = context.CreateSocket(SocketType.PUB)
        let currentProcess = Process.GetCurrentProcess()
        let mutable id = currentProcess.Id
        if id < 5000 then
            id <- id + 5000

        publisher.Bind(sprintf "tcp://*:%i" id)

        while not(x.EndBroadcaster) do

            let getParamsData(c : SubscriberElem) = 
                let mutable retData = ""
                for elem in c.GetParams do
                    retData <- retData + ";" + elem.Key + "=" + elem.DefaultValue.Replace("\"", "")
                retData
                          
            let publishData(m : SubscriberElem) = 
                let value = sprintf "%s;%b;%s" m.Id m.Status (getParamsData(m))
                publisher.Send(value, Encoding.Unicode) |> ignore

            Thread.Sleep(x.BroadcastPeriod)
            x.Messages |> List.iter (fun x -> publishData(x))

    member x.CreateASubscriberWithMessages(messages : SubscriberElem List) =
        x.Messages <- messages
        (new Thread(new ThreadStart(x.StartPublisher))).Start()

    member x.UpdateSubscriberMessages(messages : SubscriberElem List) =
        x.Messages <- messages
        
    member x.GetCodeFixesForDiagnostics(ruleId : string, codeFixes : CodeFixProvider List) = 
        let codeFixesOut = System.Collections.Generic.List<CodeFixProvider>()

        let AddToListIfIdAreFound(fix :  CodeFixProvider, outvar:System.Collections.Generic.List<CodeFixProvider>) = 
            let data = List.ofSeq (fix.GetFixableDiagnosticIds()) |> List.tryFind (fun ele -> ele.Equals(ruleId))
            match data with
            | None -> ()
            | Some value -> outvar.Add(fix)
           
        codeFixes |> List.iter (fun c -> AddToListIfIdAreFound(c, codeFixesOut))

        codeFixesOut
        
    member x.GetSubscriberData(ids : DiagnosticDescriptor List, profile : Profile) =
        let mutable subs : SubscriberElem List = List.Empty

        for id in ids do
            let data = SubscriberElem(id.Id)
            let ret = List.ofSeq profile.Rules |> List.tryFind (fun elem -> elem.Key.Equals(id.Id))
            match ret with
            | None -> data.Status <- false
            | Some(c) -> data.Status <- true
                         for para in c.Params do
                            data.AddParam(para)
            
            subs <- subs @ [data]
            
        subs        

    member x.GetDiagnosticsFromAssembly(assemblyToAdd:string, path:string) =
        let listOfDiagnosticsId : string List = List.Empty

        let domaininfo = new AppDomainSetup(ApplicationBase = path, PrivateBinPath = @"vs2015\SQAnalyzer")
        let adevidence = AppDomain.CurrentDomain.Evidence
        AppDomain.CurrentDomain.add_AssemblyResolve(fun _ args ->
            let name = System.Reflection.AssemblyName(args.Name)
            let existingAssembly = 
                System.AppDomain.CurrentDomain.GetAssemblies()
                |> Seq.tryFind(fun a -> System.Reflection.AssemblyName.ReferenceMatchesDefinition(name, a.GetName()))
            match existingAssembly with
            | Some a -> a
            | None -> null
        )

        let domain = AppDomain.CreateDomain("AllowUnloadingOfAssembliesDomain", adevidence, domaininfo)
        let typeDom = typeof<ProxyDomain>
        domain.CreateInstanceAndUnwrap(typeDom.Assembly.FullName, typeDom.FullName) |> ignore
        let data = Assembly.GetExecutingAssembly()
        let executingAssembly = data.FullName
        let asmLoaderProxy = domain.CreateInstanceAndUnwrap(executingAssembly, typeof<ProxyDomain>.FullName)
        
        let typesd = asmLoaderProxy.GetType()
        let getdiag = typesd.GetMethod("GetDiagnosticPresentInAssembly")
        let data : System.Object array = Array.zeroCreate 1
        data.[0] <- (Path.Combine(path,assemblyToAdd) :> System.Object)
        let ret = (getdiag.Invoke(asmLoaderProxy, data) :?> DiagnosticDescriptor List)
        AppDomain.Unload(domain)
        ret

    
    member x.GetCodeFixFromAssembly(assemblyToAdd:string) =
        let listOfDiagnostics = List.Empty

        let domaininfo = new AppDomainSetup(ApplicationBase = Environment.CurrentDirectory)
        let adevidence = AppDomain.CurrentDomain.Evidence
        let domain = AppDomain.CreateDomain("AllowUnloadingOfAssembliesDomain", adevidence, domaininfo)
        let typeDom = typeof<ProxyDomain>
        domain.CreateInstanceAndUnwrap(typeDom.Assembly.FullName, typeDom.FullName) |> ignore
        let asmLoaderProxy = domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof<ProxyDomain>.FullName)
        
        let getdiag = asmLoaderProxy.GetType().GetMethod("GetCodeFixPresentInAssembly")
        let data : System.Object array = Array.zeroCreate 1
        data.[0] <- (assemblyToAdd :> System.Object)
        let ret = (getdiag.Invoke(asmLoaderProxy, data) :?> CodeFixProvider List)

        AppDomain.Unload(domain)
        ret
          
    member x.GetDiagnosticElementFromManifest(manifestPath:string) =
        let mutable diagnostics : DiagnosticDescriptor List = List.Empty
        if File.Exists(manifestPath) then
            let manifest = SourceManifest.Parse(File.ReadAllText(manifestPath))
            for asset in manifest.Assets do
                if asset.Type.Equals("Microsoft.VisualStudio.Analyzer") || asset.Type.Equals("Microsoft.VisualStudio.MefComponent") then
                    try
                        let diag = x.GetDiagnosticsFromAssembly(asset.Path, Directory.GetParent(manifestPath).ToString())
                        for elem in diag do
                            diagnostics <- diagnostics @ [elem]
                    with
                    | ex -> ()

        diagnostics
                
    member x.AddDiagnosticElementToManifest(assemblyToAdd:string, manifestPath:string) =
        if not(File.Exists(assemblyToAdd)) then
            PatchSourceManifestRetCode.FileNotFound
        else 
            let manifest = SourceManifest.Parse(File.ReadAllText(manifestPath))
            let elem = seq manifest.Assets |> Seq.tryFind (fun elem -> elem.Path.EndsWith(Path.GetFileName(assemblyToAdd)))
            match elem with 
                | Some x -> PatchSourceManifestRetCode.AlreadyAdded
                | None -> 
                    let domaininfo = new AppDomainSetup(ApplicationBase = Environment.CurrentDirectory)
                    let adevidence = AppDomain.CurrentDomain.Evidence
                    let domain = AppDomain.CreateDomain("AllowUnloadingOfAssembliesDomain", adevidence, domaininfo)
                    let typeDom = typeof<ProxyDomain>
                    domain.CreateInstanceAndUnwrap(typeDom.Assembly.FullName, typeDom.FullName) |> ignore
                    let asmLoaderProxy = domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof<ProxyDomain>.FullName)
                    
                    
                    let isdiagPresent = asmLoaderProxy.GetType().GetMethod("IsDiagnosticPresentInAssembly")
                    let iscodeFixPresent = asmLoaderProxy.GetType().GetMethod("IsCodeFixPresentInAssembly")

                    let data : System.Object array = Array.zeroCreate 1
                    data.[0] <- (assemblyToAdd :> System.Object)
                    let ret = (isdiagPresent.Invoke(asmLoaderProxy, data) :?> bool)
                    let mutable addedAnalysis = false
                    match ret with 
                        | true -> AddDllAnalysisToManifest(assemblyToAdd, manifestPath)
                                  addedAnalysis <- true
                        | false -> ()

                    let ret = (iscodeFixPresent.Invoke(asmLoaderProxy, data) :?> bool)
                    let mutable addedMef = false
                    AppDomain.Unload(domain)
                    match ret with 
                        | true -> AddDllCodeFixToManifest(assemblyToAdd, manifestPath)
                                  addedMef <- true
                        | false -> ()

                    if addedAnalysis && addedMef then
                        PatchSourceManifestRetCode.AddedMefAndAnalysis
                    elif addedAnalysis then
                        PatchSourceManifestRetCode.OnlyAnalysis
                    elif addedAnalysis then
                        PatchSourceManifestRetCode.OnlyMef
                    else
                        PatchSourceManifestRetCode.NotFoundDiagnosticOrCodeFixers
                        