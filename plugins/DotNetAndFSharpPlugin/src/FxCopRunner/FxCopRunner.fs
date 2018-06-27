namespace FxCopRunner

open System
open System.IO
open VSSonarPlugins
open VSSonarPlugins.Types
open Microsoft.Build.Utilities
open System.Text.RegularExpressions
open System.ComponentModel
open FSharp.Data
open System.Reflection
open System.Xml
open System.Xml.Linq

type Rules = XmlProvider<"""
 <rules>
  <rule key="CustomRuleTemplate">
    <cardinality>MULTIPLE</cardinality>
    <priority>MAJOR</priority>
    <name><![CDATA[Template for custom FxCop rules]]></name>
    <description>
      <![CDATA[<p></p>]]>
    </description>
  </rule>
  <rule key="dasdas">
    <configKey>CA1000</configKey>
    <cardinality>MULTIPLE</cardinality>
    <priority>MAJOR</priority>
    <name><![CDATA[Template for custom FxCop rules]]></name>
    <description>
      <![CDATA[<p></p>]]>
    </description>
  </rule>  
</rules> """>

type FxCopRunner(executor : IVSSonarQubeCmdExecutor, vsinter : IConfigurationHelper, vshelper : IVsEnvironmentHelper, notifier : INotificationManager) =
    let mutable ProjectLookupRunning = Set.empty
    let mutable profile : System.Collections.Generic.Dictionary<string, Profile> = null
    let mutable ProjectIssues : Map<string, System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Issue>>> = Map.empty

    let rulesData = 
        let assembly = Assembly.GetExecutingAssembly()
        let resourceName = "rules.xml"

        use stream = assembly.GetManifestResourceStream(resourceName)
        use reader = new StreamReader(stream)
        let result = reader.ReadToEnd()
        Rules.Parse(result)

    let defaultFxCopLocation = 
        if vsinter <> null then
            try
                let defPro = new SonarQubeProperties(Key = GlobalAnalysisIds.FxCopPathKey,
                                                     Value = @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Team Tools\Static Analysis Tools\FxCop\FxCopCmd.exe",
                                                     Context = Context.AnalysisGeneral,
                                                     Owner = "AnalysisPlugin")

                vsinter.WriteSetting(defPro, false, true)
                let currentValue = vsinter.ReadSetting(Context.AnalysisGeneral, "AnalysisPlugin", GlobalAnalysisIds.FxCopPathKey).Value
                if File.Exists(currentValue) then
                    currentValue
                else
                    @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Team Tools\Static Analysis Tools\FxCop\FxCopCmd.exe"
            with
            | _ -> @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Team Tools\Static Analysis Tools\FxCop\FxCopCmd.exe"
        else
            @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Team Tools\Static Analysis Tools\FxCop\FxCopCmd.exe"



    let runFxCop(project : VsProjectItem, localAnalyser : IAnalysisPlugin) =
        let rule = (List.ofSeq (profile.["cs"].GetAllRules())) |> Seq.tryFind(fun c -> c.Repo.Equals("fxcop"))
        match rule with
        | Some rd -> 

            let issues = new System.Collections.Generic.List<Issue>()
            let parentFolder = Directory.GetParent(project.Solution.SolutionPath).ToString()
        
            let fxCopCommand = 
                try
                    vsinter.ReadSetting(Context.AnalysisProject, project.Solution.SonarProject.Key, "sonar.cs.fxcop.fxCopCmdPath").Value
                with
                | ex -> defaultFxCopLocation

            let fxCopDirectories =
                try
                    vsinter.ReadSetting(Context.AnalysisProject, project.Solution.SonarProject.Key, "sonar.cs.fxcop.directories").Value
                with
                | ex -> ""

            let bootStrapper =
                try
                    vsinter.ReadSetting(Context.AnalysisProject, project.Solution.SonarProject.Key, "sonar.visualstudio.enable").Value.ToLower().Equals("true")
                with
                | ex -> false

            let arguments = 
                let builder = new CommandLineBuilder()

                if fxCopDirectories <> "" then
                    for dir in fxCopDirectories.Split(',') do
                        if Path.IsPathRooted(dir) then
                            builder.AppendSwitch("/d:" + dir)
                        else
                            builder.AppendSwitch("/d:" + Path.GetFullPath(Path.Combine(parentFolder, dir)))


                builder.AppendSwitch("/f:" + project.OutputPath)
                builder.AppendSwitch("/console")
                builder.AppendSwitch("/searchgac")
                builder.ToString()

            notifier.ReportMessage(new Message(Id = "FxCopRunner", Data = fxCopCommand + " " + arguments))

            try
                let data = executor.ExecuteCommand(fxCopCommand, arguments)

                // [Location not stored in Pdb] : warning  : CA1062 : Microsoft.Design : In externally visible method 'VectorConversions.ToVector3(this Point)', validate parameter 'point' before using it.
                // d:\folder\BoxConversions.cs(19,1) : warning  : CA1062 : Microsoft.Design : In externally visible method 'BoxConversions.ToAxisAlignedBox3(this AABB)', validate parameter 'axisAlignedBoundingBox' before using it.
                // d:\Development\SonarQube\VssonarExtension\VSSonarQubeExtension\ObjDrop\vs13\Release\VSSonarExtensionUi\View\Helpers\view\helpers\UserExceptionMessageBox.xaml(32,1) : warning  : CA1033 : Microsoft.Design : Make 'UserExceptionMessageBox' sealed (a breaking change if this class has previously shipped), implement the method non-explicitly, or implement a new method that exposes the functionality of 'IComponentConnector.Connect(int, object)' and is visible to derived classes.
                // Project : warning : CA0067 : Directory 'd:\folder\bin' was not found.
                for line in data do
                    let elems = Regex.Split(line, " : ")
                    if elems.Length > 4 then
                        try
                            let issue = new Issue()
                            let lineandfile = elems.[0].Split('(')

                            if elems.[0].Equals("[Location not stored in Pdb]") || elems.[0].Equals("Project") then
                                issue.Component <- elems.[0]
                                issue.Line <- 0
                                issue.LocalPath <- elems.[0]
                            else
                                let item = vshelper.VsFileItem(elems.[0].Split('(').[0], project.ProjectFilePath, project.Solution.SonarProject, null)
                                issue.LocalPath <- item.FilePath
                                issue.Component <- localAnalyser.GetResourceKey(item, bootStrapper)
                                issue.Line <- Convert.ToInt32(elems.[0].Split('(').[1].Split(',').[0])

                            let element =  rulesData.Rules |> Seq.tryFind (fun c -> match c.ConfigKey with
                                                                                    | Some value -> value.Equals(elems.[2])
                                                                                    | _ -> false)
                            match element with
                            | Some data -> issue.Rule <- data.Key
                            | _ -> issue.Rule <- elems.[2]
                            issue.Message <- elems.[4]
                            issues.Add(issue)
                        with
                        | ex -> notifier.ReportMessage(new Message(Id = "FxCopRunner", Data = "FxCop Failed to Parse Line: " + line))
                                notifier.ReportException(ex)
            with
            | ex -> notifier.ReportMessage(new Message(Id = "FxCopRunner", Data = "FxCop Failed: " + fxCopCommand + " " + arguments))
                    notifier.ReportException(ex)

            notifier.ReportMessage(new Message(Id = "FxCopRunner", Data = "FxCop Done: Found " + issues.Count.ToString() + " : " + project.OutputPath))
            issues
        | _ ->
            notifier.ReportMessage(new Message(Id = "FxCopRunner", Data = "FxCop disable, no rules enabled"))
            new System.Collections.Generic.List<Issue>()

    let getLanguageKey(extension : string) =
        if  extension.EndsWith(".cs") then
            "cs"
        elif extension.EndsWith(".xaml") then
            "xaml"
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

    let updateIssueWithProfile(itemInView : VsFileItem, issue : Issue, suffix : string) =
        try
            let extension = Path.GetExtension(itemInView.FilePath).ToLower()
            let rule = profile.[getLanguageKey(extension)].GetRule(suffix + issue.Rule)
            if rule <> null then
                issue.Rule <- suffix + issue.Rule
                issue.Severity <- rule.Severity
                issue.Effort <- rule.DebtRemFnCoeff
                issue.LocalPath <- itemInView.FilePath
                issue
            else
                null
        with
        | _ -> null

    member x.GetIssuesForFile(itemInView : VsFileItem) =
        let issues = new System.Collections.Generic.List<Issue>()

        let suffix =
            if itemInView.FileName.EndsWith(".fs") || itemInView.FileName.EndsWith(".fsi") then
                "fxcop-fs:"
            else
                "fxcop:"

        for project in ProjectIssues do
            if project.Value.ContainsKey(itemInView.FilePath.ToLower()) then
                for issue in project.Value.[itemInView.FilePath.ToLower()] do
                    let issuenew = updateIssueWithProfile(itemInView, issue, suffix)
                    if issuenew <> null then
                        issues.Add(issuenew)

        if ProjectIssues.ContainsKey(itemInView.Project.ProjectName) then
            try
                if ProjectIssues.[itemInView.Project.ProjectName].ContainsKey("[Location not stored in Pdb]".ToLower()) then
                    for issue in ProjectIssues.[itemInView.Project.ProjectName].["[Location not stored in Pdb]".ToLower()] do
                        let issuenew = updateIssueWithProfile(itemInView, issue, suffix)
                        if issuenew <> null then
                            issues.Add(issuenew)

            with
            | ex -> ()

            try
                if ProjectIssues.[itemInView.Project.ProjectName].ContainsKey("Project".ToLower()) then
                    for issue in ProjectIssues.[itemInView.Project.ProjectName].["Project".ToLower()] do
                        let issuenew = updateIssueWithProfile(itemInView, issue, suffix)
                        if issuenew <> null then
                            issues.Add(issuenew)

            with
            | ex -> ()

        issues


    member x.RunAnalysisOnProject(project : VsProjectItem, localAnalyser : IAnalysisPlugin) =
        let found = ProjectLookupRunning |> Set.contains(project.ProjectName)
        if not(found) then
            ProjectLookupRunning <- ProjectLookupRunning.Add(project.ProjectName)
            if ProjectIssues.ContainsKey(project.ProjectName) then
                ProjectIssues <- ProjectIssues.Remove(project.ProjectName)

            let backgroundWorker = new BackgroundWorker(WorkerReportsProgress = false)
            backgroundWorker
                .RunWorkerCompleted
                .Add(fun c -> ProjectLookupRunning <- ProjectLookupRunning.Remove(project.ProjectName))

            backgroundWorker
                .DoWork
                .Add(fun c ->
                        for issue in runFxCop(project, localAnalyser) do
                            if ProjectIssues.ContainsKey(project.ProjectName) then
                                let issuesPerProject = ProjectIssues.[project.ProjectName]
                                if issuesPerProject.ContainsKey(issue.LocalPath.ToLower()) then
                                    let issuesPerFile = issuesPerProject.[issue.LocalPath.ToLower()]
                                    issuesPerFile.Add(issue)
                                else
                                    let data = new System.Collections.Generic.List<Issue>()
                                    data.Add(issue)
                                    ProjectIssues.[project.ProjectName].Add(issue.LocalPath.ToLower(), data) |> ignore
                            else
                                let data = new System.Collections.Generic.List<Issue>()
                                let mutable issuesPerFile = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Issue>>()
                                data.Add(issue)
                                issuesPerFile.Add(issue.LocalPath.ToLower(), data)
                                ProjectIssues <- ProjectIssues.Add(project.ProjectName, issuesPerFile)
                    )

            backgroundWorker.RunWorkerAsync()

    member x.UpdateProfile(profileIn : System.Collections.Generic.Dictionary<string, Profile>) =
        profile <- profileIn