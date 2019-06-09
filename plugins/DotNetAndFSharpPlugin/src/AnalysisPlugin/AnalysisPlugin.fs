namespace AnalysisPlugin

open System.Collections.Generic
open System.ComponentModel.Composition
open System.IO
open PluginsOptionsController
open System
open System.Reflection
open VSSonarPlugins
open VSSonarPlugins.Types
open SonarRestService
open SonarLocalAnalyser
open SonarRestService.Types

[<Export(typeof<IPlugin>)>]
type public AnalysisPlugin(notificationManager : INotificationManager, configurationHelper : IConfigurationHelper, service : ISonarRestService, vshelper : IVsEnvironmentHelper) = 
    let mutable isAssociating = false

    let mutable pluginCon : PluginsOptionsControl = null
    let desc = new PluginDescription(Name = "AnalysisPlugin",
                                    Description = "Analysis Language Plugin",
                                    SupportedExtensions = "cs,js,xaml,py,html,xml,vb,fs,fsi",
                                    AssemblyPath = Assembly.GetExecutingAssembly().Location,
                                    Version = Assembly.GetExecutingAssembly().GetName().Version.ToString())

    let pluginData = new LocalExtension(configurationHelper, notificationManager, service, vshelper)
    let pluginCon = new PluginsOptionsControl()
    let mutable dllLocations = List.Empty

    new () = new AnalysisPlugin(null, null, null, null)

    interface IDisposable with
        member this.Dispose() = pluginData.UnloadDomains()

    interface IPlugin with
        member this.OnConnectToSonar(path : ISonarConfiguration) =
            async {
                return true
            } |> Async.StartAsTask

        member this.SetDllLocation(path : string) =
            let elem = dllLocations |> List.tryFind (fun c -> c.Equals(path))
            match elem with
            | Some value -> ()
            | _ -> dllLocations <- dllLocations @ [path]

        member this.DllLocations() =
            (new System.Collections.Generic.List<string>(dllLocations) :> IList<string>)

        member this.GetPluginControlOptions(project : Resource, conf : ISonarConfiguration) =
            (pluginCon :> IPluginControlOption)

        member this.GetPluginDescription() =
            desc

        member this.AssociateProject(resource : Resource, configuration : ISonarConfiguration, profile : System.Collections.Generic.Dictionary<string, Profile>, vsversion:string) =
            async {
                if not(isAssociating) then
                    isAssociating <- true
                    try
                        pluginData.AssociatePropject(resource, configuration, profile, vsversion)
                    with
                    | ex -> ()
                    isAssociating <- false
                return true
            } |> Async.StartAsTask
                
        member this.ResetDefaults() =
            ()

    interface IRoslynPlugin with
        member this.SetDiagnostics(diagnostics : System.Collections.Generic.List<DiagnosticAnalyzerType>) =
            pluginData.SetDiagnostics(diagnostics)

    interface IAnalysisPlugin with
        member this.AdditionalCommands(profile : Dictionary<string,Profile>) = 
            new List<IPluginCommand>()
            
        member this.GetLanguageKey(projectItem : VsFileItem) = 
            let extension = Path.GetExtension(projectItem.FileName).ToLower()

            if  extension.EndsWith(".cs") then
                "cs"
            elif extension.EndsWith(".js") then
                "js"
            elif extension.EndsWith(".xaml") then
                "xaml"
            elif extension.EndsWith(".py") then
                "py"
            elif extension.EndsWith(".html") then
                "html"
            elif extension.EndsWith(".xml") then
                "xml"
            elif extension.EndsWith(".vb") then
                "vb"
            elif extension.EndsWith(".fs") then
                "fs"
            elif extension.EndsWith(".fsi") then
                "fs"
            elif extension.EndsWith(".fsx") then
                "fs"
            else
                ""

        member this.LaunchAnalysisOnProject(project : VsProjectItem, conf : ISonarConfiguration) =
            pluginData.RunProjectAnalysis(project, conf, this)

        member this.LaunchAnalysisOnSolution(project : VsSolutionItem, conf : ISonarConfiguration) =
            ()

        member this.GetLocalAnalysisExtension(conf : ISonarConfiguration) =
            (pluginData :> IFileAnalyser)

        member this.GetResourceKey(projectItem : VsFileItem, isBootStrapperOn:bool) =
            if isBootStrapperOn then
                PluginHelper.GetKeyWihtBootStrapper(projectItem, projectItem.Project.Solution.SonarProject.Key)
            else
                PluginHelper.GetKeyWithoutBootStrapper(projectItem, projectItem.Project.Solution.SonarProject.Key, vshelper)

        member this.IsSupported(item : VsFileItem) = 
                item.FileName.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase) ||
                    item.FileName.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase) ||
                    item.FileName.EndsWith(".xaml", StringComparison.InvariantCultureIgnoreCase) ||
                    item.FileName.EndsWith(".py", StringComparison.InvariantCultureIgnoreCase) ||
                    item.FileName.EndsWith(".html", StringComparison.InvariantCultureIgnoreCase) ||
                    item.FileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) ||
                    item.FileName.EndsWith(".vb", StringComparison.InvariantCultureIgnoreCase) ||
                    item.FileName.EndsWith(".fs", StringComparison.InvariantCultureIgnoreCase) ||
                    item.FileName.EndsWith(".fsi", StringComparison.InvariantCultureIgnoreCase) ||
                    item.FileName.EndsWith(".fsx", StringComparison.InvariantCultureIgnoreCase)
                    
        member this.IsProjectSupported(conf : ISonarConfiguration, project : Resource) = 
            if project = null then
                false
            elif project.Lang = null then
                true
            else
                project.Lang.Equals("cs") || project.Lang.Equals("C#") || project.Lang.Equals("CS") ||
                project.Lang.Equals("vbnet") || project.Lang.Equals("VBNET") || project.Lang.Equals("VbNet") ||
                project.Lang.Equals("Py", StringComparison.CurrentCultureIgnoreCase) || 
                project.Lang.Equals("Js") || project.Lang.Equals("JS") || project.Lang.Equals("js") ||
                project.Lang.ToLower().Equals("fs")

