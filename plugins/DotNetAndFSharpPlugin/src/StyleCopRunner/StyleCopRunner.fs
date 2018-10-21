namespace StyleCopRunner

open System.IO
open VSSonarPlugins.Types
open VSSonarPlugins
open System.Text
open System
open System.Reflection
open System.Threading
open System.Collections.Generic
open System.Threading
open System.Diagnostics
open StyleCop

type StyleCopRunner(helper : IConfigurationHelper, notificationManager : INotificationManager) =

    let owner = "AnalysisPlugin"
    let pluginsRunningPath = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")
    let extensionRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")).ToString()
    let CurrentRunDir = Environment.CurrentDirectory
    let mutable asmLoaderProxyObj = null
    let mutable profile : System.Collections.Generic.Dictionary<string, Profile> = null
    let StyleCopPathKey = "StyleCopPath"
    let defaultval = Path.Combine(extensionRunningPath, "externalAnalysers\\StyleCop")
    let settingsContent =
        let assembly = Assembly.GetExecutingAssembly()
        let resourceName = "Settings.StyleCop";

        use stream = assembly.GetManifestResourceStream(resourceName)
        use reader = new StreamReader(stream)
        reader.ReadToEnd()

    let StyleCopPathDefaultValue =         
        if helper <> null then
            try
                let currentDir = helper.ReadSetting(Context.AnalysisGeneral, owner, StyleCopPathKey).Value
                
                notificationManager.ReportMessage(new Message(Id = "StyleCopRunner", Data = currentDir))
                if Directory.Exists(currentDir) then
                    currentDir
                else
                    helper.WriteSetting(Context.AnalysisGeneral, owner, StyleCopPathKey, defaultval, true, false)
                    defaultval
            
            with
            | _ -> helper.WriteSetting(Context.AnalysisGeneral, owner, StyleCopPathKey, defaultval, true, false)
                   defaultval
        else
            defaultval

    let mutable core = 

        let domaininfo = new AppDomainSetup(ApplicationBase = extensionRunningPath, PrivateBinPath = @"plugins")
        domaininfo.ShadowCopyFiles <- "true"
        let adevidence = AppDomain.CurrentDomain.Evidence

        AppDomain.CurrentDomain.add_AssemblyResolve(fun _ args ->
            let name = System.Reflection.AssemblyName(args.Name)

            if name.Name = "System.Windows.Interactivity" then
                ()

            let existingAssembly = 
                System.AppDomain.CurrentDomain.GetAssemblies()
                |> Seq.tryFind(fun a -> System.Reflection.AssemblyName.ReferenceMatchesDefinition(name, a.GetName()))
            match existingAssembly with
            | Some a -> a
            | None -> 
                let path = Path.Combine(extensionRunningPath, name.Name + ".dll")
                if File.Exists(path) then 
                    let inFileAssembly = Assembly.LoadFile(path)
                    inFileAssembly
                else
                    let path = Path.Combine(extensionRunningPath, "externalAnalysers\\StyleCop", name.Name + ".dll")
                    if File.Exists(path) then 
                        let inFileAssembly = Assembly.LoadFile(path)
                        inFileAssembly
                    else
                        null                
        )

        let domain = AppDomain.CreateDomain("StyleCopRunnerDomain", adevidence, domaininfo)

        try

            let typeDom = typeof<ProxyDomain>
            domain.CreateInstanceAndUnwrap(typeDom.Assembly.FullName, typeDom.FullName) |> ignore
            let data = Assembly.GetExecutingAssembly()
            let executingAssembly = data.FullName
            asmLoaderProxyObj <- domain.CreateInstanceAndUnwrap(executingAssembly, typeof<ProxyDomain>.FullName)

            let typesd = asmLoaderProxyObj.GetType()
            let getdiag = typesd.GetMethod("StartStyleCopCore")
            let data : System.Object array = Array.zeroCreate 3
            data.[0] <- (StyleCopPathDefaultValue :> System.Object)
            data.[1] <- (notificationManager :> System.Object)
            data.[2] <- (settingsContent :> System.Object)
            getdiag.Invoke(asmLoaderProxyObj, data) |> ignore
        with
        | ex -> notificationManager.ReportMessage(new Message(Id = "StyleCopRunner", Data = "Cannot Load"))
                notificationManager.ReportException(ex)

        domain

    member this.UnloadDomain() =
        AppDomain.Unload(core)

    member this.UpdateSettings(externlProfileIn : System.Collections.Generic.Dictionary<string, Profile>) =
        profile <- externlProfileIn
        helper.WriteSetting(Context.AnalysisGeneral, owner, StyleCopPathKey, StyleCopPathDefaultValue, true, true)

    member this.RunAnalysis(itemInView : VsFileItem, vsHelper : IConfigurationHelper) =
        try
            let rule = (List.ofSeq (profile.["cs"].GetAllRules())) |> Seq.tryFind(fun c -> c.Repo.Equals("stylecop"))
            match rule with
            | Some rd -> 
                if itemInView.FileName.ToLower().EndsWith(".cs") then
                    let typesd = asmLoaderProxyObj.GetType()
                    let getdiag = typesd.GetMethod("RunStyleCop")

                    let data : System.Object array = Array.zeroCreate 3
                    data.[0] <- (itemInView :> System.Object)
                    data.[1] <- (profile.["cs"] :> System.Object)
                    data.[2] <- (vsHelper :> System.Object)  
                    let issues = (getdiag.Invoke(asmLoaderProxyObj, data) :?> System.Collections.Generic.List<Issue>)
                    issues
                else
                    new System.Collections.Generic.List<Issue>()
            | _ ->
                notificationManager.ReportMessage(new Message(Id = "StyleCopRunner", Data = "StyleCop rules are disable"))
                new System.Collections.Generic.List<Issue>()
        with
        | ex -> notificationManager.ReportMessage(new Message(Id = "StyleCopRunner", Data = "StyleCop disable, no rules enabled"))
                notificationManager.ReportException(ex)
                new System.Collections.Generic.List<Issue>()

