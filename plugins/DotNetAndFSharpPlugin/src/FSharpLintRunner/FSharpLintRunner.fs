namespace FSharpLintRunner

open System
open System.Resources
open System.Reflection
open System.Globalization
open System.Collections
open System.IO
open System.Text.RegularExpressions
open System.Text

open SonarRestService.Types
open VSSonarPlugins
open VSSonarPlugins.Types

open FSharpLint.Framework
open FSharpLint.Framework.Ast
open FSharpLint.Framework.Configuration
open FSharpLint.Application

[<AllowNullLiteralAttribute>]
type FsLintRule(name : string, value : string) =
    member val Rule : string = name with get
    member val Text : string = value with get

type SonarRules() = 

    let fsLintProfile =
        let resourceManager = ResourceManager("Text" ,Assembly.Load("FSharpLint.Core"))
        let set = resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true)
        let mutable rules = List.Empty
        
        for resoure in set do
            let lem = resoure :?> DictionaryEntry
            try
                if (lem.Key :?> string).StartsWith("Rules") ||
                   (lem.Key :?> string).Equals("LintError")  ||
                   (lem.Key :?> string).Equals("LintSourceError") then
                    let rule = FsLintRule(lem.Key :?> string, lem.Value :?> string)
                    rules <- rules @ [rule]
            with
            | _ -> ()
        rules

    member this.GetRule(txt : string) =
        let verifyIfExists(rule : FsLintRule, txtdata : string) = 
            let pattern = rule.Text
                                .Replace("{0}", "[a-zA-Z0-9]{1,}")
                                .Replace("{1}", "[a-zA-Z0-9]{1,}")
                                .Replace("{2}", "[a-zA-Z0-9]{1,}")
                                .Replace("{3}", "[a-zA-Z0-9]{1,}")
                                .Replace("{4}", "[a-zA-Z0-9]{1,}")
            Regex.Match(txtdata, pattern).Success
        let foundItem = fsLintProfile |> Seq.tryFind (fun c -> verifyIfExists(c, txt))
        match foundItem with | Some v -> v | _ -> null

    member this.ShowRules() =
        fsLintProfile |> Seq.iter (fun c -> printf "%s  = %s\r\n" c.Rule c.Text)
        printf "\r\n Total Rules: %i\r\n" fsLintProfile.Length

type private Argument =
    | ProjectFile of string
    | SingleFile of string
    | Source of string
    | UnexpectedArgument of string

type FsLintRunner(notificationManager : INotificationManager, configuration : FSharpLint.Framework.Configuration.Configuration) =
    let rules = SonarRules()
    let mutable notsupportedlines = List.Empty
    let mutable issues = List.empty
    let mutable filePath = ""


            
    let outputLintResult = function
        | LintResult.Success(_) -> notificationManager.ReportMessage(Message ( Id = "FsLintRunner", Data= "FSharp Link Ok"))
        | LintResult.Failure(error) -> notificationManager.ReportMessage(Message ( Id = "FsLintRunner", Data= "FSharp Link Error: " + error.ToString()))

    let runLintOnFile pathToFile (profile:Profile) =
        
            let reportLintWarning (warning:FSharpLint.Application.LintWarning.Warning) =
                let output = warning.Info + System.Environment.NewLine + LintWarning.getWarningWithLocation warning.Range warning.Input
                let rule = rules.GetRule(warning.Info)
                if isNull(rule) then
                    let issue = Issue(Rule = "fsharplint:" + rule.Rule, Line = warning.Range.StartLine, Component = filePath, Message = warning.Info)
                    let rule = profile.GetRule("fsharplint:" + rule.Rule)
                    issue.Severity <-rule.Severity
                    issue.Status <- IssueStatus.OPEN
                    issues  <- issues @ [issue]  
                else
                    notsupportedlines <- notsupportedlines @ [output]

            let parseInfo =
                {
                    CancellationToken = None
                    ReceivedWarning = Some reportLintWarning
                    Configuration = Some configuration
                }

            lintFile parseInfo pathToFile
        
    member this.ExecuteAnalysis(item : VsFileItem, config : Profile) =
        issues <- List.Empty
        filePath <- item.FilePath
        if File.Exists(filePath) then
            runLintOnFile filePath config (Version(4, 0)) |> outputLintResult
            notificationManager.ReportMessage(Message(Id = "FSharpLintRunner", Data = "Issues Found: " + issues.Length.ToString()))
        else
            notificationManager.ReportMessage(Message(Id = "FSharpLintRunner", Data = "File not found : " + filePath))
        issues


type FSharpLintAnalyser(notificationManager : INotificationManager) =
    let mutable profile : System.Collections.Generic.Dictionary<string, Profile> = null

    let ExecutAnalysis item =
        try
            let lintRunner = FsLintRunner(notificationManager, InputConfigHandler.CreateALintConfiguration(profile))
            lintRunner.ExecuteAnalysis(item, profile.["fs"])
        with
        | ex -> notificationManager.ReportMessage(Message(Id = "FSharpLintRunner", Data = "Failed to run static analysis: " + ex.Message))
                notificationManager.ReportException(ex)
                List.Empty

    member val Issues : Issue List = List.empty with get, set

    member this.RunLint(item : VsFileItem) =
        let extension = Path.GetExtension(item.FilePath).ToLower()
        if extension.Equals(".fs") ||   extension.Equals(".fsi") then
            ExecutAnalysis item
        else
            List.Empty

    member x.UpdateProfile(profileIn : System.Collections.Generic.Dictionary<string, Profile>) =
        profile <- profileIn