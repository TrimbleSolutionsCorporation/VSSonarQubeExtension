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
open System.Collections.Immutable
open System.Threading
open System.Collections.Generic
open System.Threading
open System.Diagnostics

open SonarRestService
open SonarRestService.Types

type ProxyDomain() =
    let mutable checksRoslyn = List.Empty
    let mutable currentKey : string = ""
    let mutable externlProfile : Profile = null
    let mutable notificationManager : INotificationManager = null
    let issuesStyle = new System.Collections.Generic.List<Issue>()
    let extensionRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")).ToString()

    member x.CreateDiagnosticPresentInAssembly(path:string, notificationManager : INotificationManager) =         
        try
            let b = File.ReadAllBytes(path)
            let assembly = Assembly.LoadFrom(path)
            let types2 = assembly.GetExportedTypes()
            
            for typedata in types2 do
                try
                    let data = Activator.CreateInstance(typedata)
                    checksRoslyn <- checksRoslyn @ [data :?> Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer]
                    notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner", Data = "Check " + typedata.ToString() + " Loaded"))
                    System.Diagnostics.Debug.WriteLine("Check : " + typedata.ToString() + " Loaded")
                with
                | ex -> Debug.WriteLine(ex.Message)
                        notificationManager.ReportException(ex)
                        notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner", Data = "Check " + typedata.ToString() + " Could Not Be Instantited"))
        with
        | ex -> ()

        System.Diagnostics.Debug.WriteLine("Checks : " + checksRoslyn.Length.ToString())
        notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner", Data = "Loaded " + checksRoslyn.Length.ToString() + " Checks"))


    member x.RunAnalysis(itemInView : VsFileItem, externlProfileIn : Profile, vsHelper : IConfigurationHelper, notificationManager : INotificationManager) = 
        let issues = new System.Collections.Generic.List<Issue>()
        let syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(itemInView.FilePath, Encoding.UTF8))
        let builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>()
        let mutable checksAvailalbe = ""
        let mutable ids = List.Empty

        for check in checksRoslyn do
            let field = check.GetType().GetFields(BindingFlags.Static ||| BindingFlags.Public ||| BindingFlags.NonPublic)

            try
                for diagnostic in check.SupportedDiagnostics do
                    let id = "csharpsquid:" + diagnostic.Id
                    checksAvailalbe <- checksAvailalbe + " - " + id
                    let rule = externlProfileIn.GetRule(id)
                    if rule <> null then
                        if diagnostic.IsEnabledByDefault then
                            builder.Add(check)
                        else
                            builder.Add(check)

                        ids <- ids @ [new KeyValuePair<string, ReportDiagnostic>(diagnostic.Id, ReportDiagnostic.Warn)]

                        if rule.Params.Count <> 0 then
                            let fields = check.GetType().GetProperties()
                            for field in fields do
                                let attributes = field.GetCustomAttributes().ToImmutableArray()
                                if attributes.Length = 1 &&
                                    attributes.[0].TypeId.ToString().EndsWith("Common.RuleParameterAttribute") then
                                    try
                                        let typeOfField = field.PropertyType
                                        let typeOfFiledName = field.PropertyType.Name
                                        if typeOfFiledName.Equals("IImmutableSet`1") then
                                            let elems = rule.Params.[0].Value.Replace("\"", "").Split(',').ToImmutableHashSet()
                                            field.SetValue(check, elems)
                                        else
                                            let changedValue = Convert.ChangeType(rule.Params.[0].Value.Replace("\"", ""), typeOfField)
                                            field.SetValue(check, changedValue)

                                        let value = field.GetValue(check)
                                        notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner", Data = "Applied Rule Parameter csharpsquid:" + diagnostic.Id + " = " + rule.Params.[0].Value))
                                        System.Diagnostics.Debug.WriteLine("Applied Rule Parameter csharpsquid:" + diagnostic.Id + " = " + rule.Params.[0].Value)
                                    with
                                    | ex -> 
                                        System.Diagnostics.Debug.WriteLine("Applied Rule Parameter csharpsquid:" + diagnostic.Id + " = " + rule.Params.[0].Value)
                                        notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner", Data = "Failed Applied Rule Parameter csharpsquid:" + diagnostic.Id + " = " + rule.Params.[0].Value))

                            ()
            with
            | ex -> System.Diagnostics.Debug.WriteLine("Cannot Add Check Failed: " + check.ToString() + " : " +  ex.Message)
                    notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner", Data = "Check Rule Could Not Be Used " + check.ToString() + " : " + ex.Message))

        notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner", Data = "Checks Availalbe: " +  " : " + checksRoslyn.Length.ToString() + " : " + checksAvailalbe))
        notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner", Data = "Checks Enabled " + builder.Count.ToString()))

        let issues = new System.Collections.Generic.List<Issue>()

        if builder.Count <> 0 then
            let analysisUnit = CSharpCompilation.Create("NSonarQubeRunner.dll", ImmutableArray.Create(syntaxTree))
            let mutable options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            options <- options.WithSpecificDiagnosticOptions(ids)
            let analysisUnitWithOptions = analysisUnit.WithOptions(options)
            let analyserMain = analysisUnitWithOptions.WithAnalyzers(builder.ToImmutable(), null, (new CancellationTokenSource()).Token)

            let result = analyserMain.GetAnalyzerDiagnosticsAsync().Result

            for issue in result do
            
                let issueNew = new Issue()
                issueNew.Component <- itemInView.SonarResource.Key
                issueNew.Rule <- "csharpsquid:" + issue.Id
                issueNew.Line <- issue.Location.GetLineSpan().StartLinePosition.Line + 1
                issueNew.Message <- issue.GetMessage()
                let rule = externlProfileIn.GetRule(issueNew.Rule)
                if rule <> null then
                    issueNew.Severity <- rule.Severity
                    issueNew.Effort <- rule.DebtRemFnCoeff
                    issues.Add(issueNew)

        issues
