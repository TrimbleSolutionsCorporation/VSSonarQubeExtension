module LocalHelpers

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

open SonarRestService
open SonarRestService.Types
open Types

let GetRoslynDiagnostics(externlProfileIn : System.Collections.Generic.Dictionary<string, Profile>, notificationManager : INotificationManager, checksRoslyn : DiagnosticAnalyzerType list) =
    let mutable builder = List.empty
    let mutable ids = List.empty
    for check in checksRoslyn do
            let mutable checkadded = false
            for diagnostic in check.Diagnostic.SupportedDiagnostics do
                if not(checkadded) then                      
                    for lang in check.Languages do
                        let language, repo = 
                            if lang.Equals("C#") then
                                "cs", "roslyn-cs"
                            else
                                "vbnet", "roslyn-vbnet"
                            
                        let id = repo + ":" + diagnostic.Id
                        try
                            let profile = externlProfileIn.[language]
                            let rule = profile.GetRule(id)
                            if rule <> null then
                                checkadded <- true
                                builder <- builder @ [check]
                                ids <- ids @ [new System.Collections.Generic.KeyValuePair<string, ReportDiagnostic>(diagnostic.Id, ReportDiagnostic.Warn)]

                                if rule.Params.Count <> 0 then
                                    let fields = check.Diagnostic.GetType().GetProperties()
                                    for field in fields do
                                        let attributes = field.GetCustomAttributes().ToImmutableArray()
                                        if attributes.Length = 1 &&
                                            attributes.[0].TypeId.ToString().EndsWith("Common.RuleParameterAttribute") then
                                            try
                                                let typeOfField = field.PropertyType
                                                let typeOfFiledName = field.PropertyType.Name
                                                if typeOfFiledName.Equals("IImmutableSet`1") then
                                                    let elems = rule.Params.[0].Value.Replace("\"", "").Split(',').ToImmutableHashSet()
                                                    field.SetValue(check.Diagnostic, elems)
                                                else
                                                    let changedValue = Convert.ChangeType(rule.Params.[0].Value.Replace("\"", ""), typeOfField)
                                                    field.SetValue(check.Diagnostic, changedValue)

                                                let value = field.GetValue(check.Diagnostic)
                                                notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner", Data = "Applied Rule Parameter csharpsquid:" + diagnostic.Id + " = " + rule.Params.[0].Value))
                                                System.Diagnostics.Debug.WriteLine("Applied Rule Parameter csharpsquid:" + diagnostic.Id + " = " + rule.Params.[0].Value)
                                            with
                                            | ex -> 
                                                System.Diagnostics.Debug.WriteLine("Applied Rule Parameter csharpsquid:" + diagnostic.Id + " = " + rule.Params.[0].Value)
                                                notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner", Data = "Failed Applied Rule Parameter csharpsquid:" + diagnostic.Id + " = " + rule.Params.[0].Value))
                                    ()
                        with
                        | ex -> System.Diagnostics.Debug.WriteLine("Cannot Add Check Failed: " + id + " : " +  ex.Message)
                                notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner", Data = "Check Rule Could Not Be Used " + id + " : " + ex.Message))

    builder, ids

let CombineIds(idsSetIn : Set<string>, builderdata : DiagnosticAnalyzerType list, idsdata : KeyValuePair<string, ReportDiagnostic> list, builder : DiagnosticAnalyzerType list, ids : KeyValuePair<string, ReportDiagnostic> list) =
    let mutable builder = builder
    let mutable ids = ids
    let mutable idsSet = idsSetIn
    let mutable count = 0

    for id in idsdata do
                
        if not(idsSet.Contains(id.Key)) then
            builder <- builder @ [builderdata.[count]]
            ids <- ids @ [id]
            idsSet <- idsSet.Add(id.Key)
                
        count <- count + 1

    builder, ids, idsSet
    
let GetSolutionDiagnostics(externlProfileIn  : System.Collections.Generic.Dictionary<string, Profile>, notificationManager : INotificationManager, diagnosticsPerProjectData : System.Collections.Generic.Dictionary<string, ProjectData>) =
    let mutable builder = List.empty
    let mutable ids = List.empty
    let mutable idsSet = Set.empty
    
    for project in diagnosticsPerProjectData do
        try
            let builderdata, idsdata = GetRoslynDiagnostics(externlProfileIn, notificationManager, project.Value.Diagnostics)
            
            let bd, id, setid = CombineIds(idsSet, builderdata, idsdata, builder, ids)
            idsSet <- setid
            builder <- bd
            ids <- id

        with
        | ex -> ()
    
    builder, ids

let UpdateSolutionDiagnosticList(solutionPath : string, notificationManager : INotificationManager, externlProfileIn : System.Collections.Generic.Dictionary<string, Profile>) =
    let solution = MSBuildHelper.CreateSolutionData(solutionPath)
    let mutable diagnosticsPerProject : System.Collections.Generic.Dictionary<string, ProjectData> = new System.Collections.Generic.Dictionary<string, ProjectData>()

    for projectdata in solution.Projects do
        try
            let mutable addionalFilesInPojectFile = [||]
            let mutable rulesetdata : RulesetData = null
            let mutable analysersFilesInProject = [||]
            let isDir = Directory.Exists(projectdata.Value.Path)

            if not(isDir) && File.Exists(projectdata.Value.Path) then
                let lines = File.ReadAllText(projectdata.Value.Path)
                let projectparse = ProjectFile.Parse(lines)
                for itemgroup in projectparse.ItemGroups do
                    try
                        for additionafile in itemgroup.AdditionalFiles do
                            addionalFilesInPojectFile <- Array.append addionalFilesInPojectFile [|additionafile.Include|]
                    with
                    | _ -> ()

                    try
                        for additionafile in itemgroup.Nones do
                            if additionafile.Include.EndsWith(".ruleset") then
                                let ruleset =
                                    if Path.IsPathRooted(additionafile.Include) then 
                                        additionafile.Include
                                    else 
                                        Path.Combine(Directory.GetParent(projectdata.Value.Path).ToString(), additionafile.Include)

                                if File.Exists(ruleset) && rulesetdata = null then
                                    rulesetdata <- new RulesetData()
                                    let rulesetparse = RuleSet.Parse(ruleset)
                                    for rule in rulesetparse.Rules do
                                        rulesetdata.AnalyzerId <- rule.AnalyzerId
                                        rulesetdata.RuleNamespace <- rule.RuleNamespace
                                        for id in rule.Rules do
                                            if id.Action = "None" then
                                                rulesetdata.DisabledRulesInRuleSet <- rulesetdata.DisabledRulesInRuleSet @ [id.Id]
                                                               
                    with
                    | _ -> ()

                    try
                        for additionafile in itemgroup.Analyzers do
                            analysersFilesInProject <- Array.append analysersFilesInProject [|additionafile.Include|]
                    with
                    | _ -> ()

                // create list of diagnostics in project
                let projectData = new ProjectData()
                projectData.RuleSet <- rulesetdata
                projectData.Path <- projectdata.Value.Path
                projectData.AdditionDocuments <- addionalFilesInPojectFile

                for diag in analysersFilesInProject do

                    let diagPath =
                        if Path.IsPathRooted(diag) then
                            diag
                        else
                            Path.GetFullPath(Path.Combine(Directory.GetParent(projectdata.Value.Path).ToString(), diag))

                    if File.Exists(diagPath) then
                        let diagnostics = LoadDiagnosticsFromPath(diagPath)
                        if diagnostics.Length <> 0 then
                            projectData.Diagnostics <- projectData.Diagnostics @ diagnostics

                // ensure local rules is set
                if projectData.RuleSet = null then
                    let ruleset = new RulesetData()
                    ruleset.AnalyzerId <- Path.GetFileNameWithoutExtension(projectdata.Value.Path)
                    ruleset.RuleNamespace <- ""
                    for diaganal in projectData.Diagnostics do
                        for dia in diaganal.Diagnostic.SupportedDiagnostics do
                            if not(dia.IsEnabledByDefault) then
                                ruleset.DisabledRulesInRuleSet <- ruleset.DisabledRulesInRuleSet @ [dia.Id]
                    projectData.RuleSet <- ruleset

                let langkey =
                    let extension = Path.GetExtension(projectdata.Value.Path).ToLower()
                    if extension.Equals(".csproj") then
                        "cs"
                    else
                        if Path.GetFileNameWithoutExtension(projectdata.Value.Path).ToLower().Equals(".vbproj") then
                            "vb"
                        else
                            ""

                if langkey <> "" && externlProfileIn.ContainsKey(langkey) then
                    // create a profile for the project
                    let profiledata = new System.Collections.Generic.Dictionary<string, Profile>()
                    let newprofile = new Profile(null, null)
                    newprofile.Language <- langkey

                    let repokey = "roslyn-" + langkey
                
                    let profile = externlProfileIn.[langkey]

                    for diag in projectData.Diagnostics do
                        for diagtype in diag.Diagnostic.SupportedDiagnostics do
                            let isDisabled = projectData.RuleSet.DisabledRulesInRuleSet |> Seq.tryFind (fun c -> c.Equals(diagtype.Id))
                            match isDisabled with
                            | Some m -> ()
                            | _ -> 
                                let rule = profile.GetRule(repokey + ":" + diagtype.Id)
                                if rule <> null then
                                    newprofile.AddRule(rule)

                    profiledata.Add(langkey, newprofile)
                    projectData.Profile <- profiledata

                diagnosticsPerProject.Add(Path.GetFileNameWithoutExtension(projectdata.Value.Path.ToLower()), projectData)
        with
        | ex -> notificationManager.ReportMessage(new Message(Id = "RoslynRunner", Data = "Failed to process solution project : " + projectdata.Value.Path))
                notificationManager.ReportException(ex)

    diagnosticsPerProject