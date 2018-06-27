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
open System.Xml

type ProxyDomain() =
    let mutable checksRoslyn = List.Empty
    let mutable core : StyleCopCore = null
    let mutable currentKey : string = ""
    let mutable externlProfile : Profile = null
    let mutable notificationManager : INotificationManager = null
    let issuesStyle = new System.Collections.Generic.List<Issue>()
    let extensionRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")).ToString()
    let mutable settingsLines : string = ""

    let CoreViolationEncountered(e : ViolationEventArgs) =
        
        let newissue = new Issue()
        newissue.Component <- currentKey
        newissue.Line <- e.Violation.Line
        newissue.Rule <- "stylecop:" + e.Violation.Rule.Name
        newissue.Message <- e.Violation.Message
        
        let rule = externlProfile.GetRule(newissue.Rule)
        if rule <> null then
            newissue.Severity <- rule.Severity
            newissue.Effort <- rule.DebtRemFnCoeff
            issuesStyle.Add(newissue)

        ()

    member x.StartStyleCopCore(path:string, notificationManagerIn : INotificationManager, settings : string) =               
        notificationManager <- notificationManagerIn    
        settingsLines <- settings
        core <- new StyleCopCore()
        let paths = new System.Collections.Generic.List<string>()
        paths.Add(path)
        core.Initialize(paths, false)
        notificationManager.ReportMessage(new Message(Id = "StyleCopRunner", Data = "Parsers: " +  core.Parsers.Count.ToString()))
        System.Diagnostics.Debug.WriteLine("StyleCopRunner : Parsers: " +  core.Parsers.Count.ToString());
        core.ViolationEncountered.Add(CoreViolationEncountered)


    member x.RunStyleCop(itemInView : VsFileItem, externlProfileIn : Profile, vsHelper : IConfigurationHelper) = 
        issuesStyle.Clear()
        let settingsFile = Path.Combine(Directory.GetParent(itemInView.Project.ProjectFilePath).ToString(), "Settings.StyleCop")
        let mutable backup = false
        if settingsLines <> "" then
            if File.Exists(settingsFile) then
                File.Copy(settingsFile, settingsFile + ".bck")
                backup <- true

            File.WriteAllText(settingsFile, settingsLines)

        try
            externlProfile <- externlProfileIn
            currentKey <- itemInView.SonarResource.Key
            let codeProject = new CodeProject(itemInView.Project.ProjectFilePath.GetHashCode(), itemInView.Project.ProjectFilePath, new StyleCop.Configuration("DEBUG;TRACE".Split(';')))

            core.Environment.AddSourceCode(codeProject, itemInView.FilePath, null) |> ignore
            let projects = new System.Collections.Generic.List<CodeProject>()

            notificationManager.ReportMessage(new Message(Id = "StyleCopRunner", Data = "Analyse: " +  itemInView.Project.ProjectFilePath))

            projects.Add(codeProject)
            core.Analyze(projects)
            notificationManager.ReportMessage(new Message(Id = "StyleCopRunner", Data = "Issues Found: " +  issuesStyle.Count.ToString()))
        with
        | _ -> ()

        if settingsLines <> "" then
            if backup then
                File.Copy(settingsFile + ".bck", settingsFile, true)
                File.Delete(settingsFile + ".bck")
            else
                File.Delete(settingsFile)

        issuesStyle
