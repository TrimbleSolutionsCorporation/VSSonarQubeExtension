namespace NSonarQubeRunner.Test

open NUnit.Framework
open Foq
open NSonarQubeRunner
open VSSonarPlugins
open VSSonarPlugins.Types
open System.IO
open FSharp.Data
open System.Xml.Linq
open System.Reflection
open Microsoft.CodeAnalysis.Diagnostics
open Microsoft.CodeAnalysis.CSharp
open System
open System.Threading

type NQubeRunnerTest() =

    
    let extensionRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")).ToString()
    let rootProject = Path.Combine(extensionRunningPath, "sample")
    let mockHttpReq = Mock<IConfigurationHelper>().Create()
    let mockNotifier = Mock<INotificationManager>().Create()
    let vshelper = Mock<IVsEnvironmentHelper>().Create()

    let rest =
        Mock<ISonarRestService>()
            .Setup(fun x -> <@ x.GetInstalledPlugins(any()) @>).Returns(
                new System.Collections.Generic.Dictionary<string, string>())
            .Create()
    let conf = Mock<ISonarConfiguration>().Create()

    let mutable profileComplete : Profile = new Profile(rest, conf)

    let profile = 
        let profiles = new System.Collections.Generic.Dictionary<string, Profile>()
        let profile = new Profile(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let rule = new Rule(ConfigKey = "csharpsquid:S104" )
        rule.Params.Add(new RuleParam(Key = "maximumFileLocThreshold", Value = "200" ))
        profile.AddRule(rule)
        let rule = new Rule(ConfigKey = "csharpsquid:S2333" )
        profile.AddRule(rule)
        let rule = new Rule(ConfigKey = "csharpsquid:S2357" )
        profile.AddRule(rule)
        let rule = new Rule(ConfigKey = "csharpsquid:S2933" )
        profile.AddRule(rule)
        let rule = new Rule(ConfigKey = "csharpsquid:S2930" )
        profile.AddRule(rule)
        let rule = new Rule(ConfigKey = "csharpsquid:S125" )
        profile.AddRule(rule)
        let rule = new Rule(ConfigKey = "csharpsquid:S1172" )
        profile.AddRule(rule)        
        let rule = new Rule(ConfigKey = "csharpsquid:S1172" )
        profile.AddRule(rule)                        
        let rule = new Rule(ConfigKey = "csharpsquid:S1144" )
        profile.AddRule(rule) 
        let rule = new Rule(ConfigKey = "csharpsquid:S109" )
        rule.Params.Add(new RuleParam(Key = "exceptions", Value = "0,1,0x0,0x00,.0,.1,0.0,1.0" ))
        profile.AddRule(rule)
        profiles.Add("cs", profile)
        profiles

    let diagnostics = 
        AppDomain.CurrentDomain.add_AssemblyResolve(fun _ args ->
            
            let name = System.Reflection.AssemblyName(args.Name)
            let path = Path.Combine(extensionRunningPath, name.Name + ".dll")
            

            if name.Name = "System.Windows.Interactivity" || name.Name = "FSharp.Core.resources" then
                null
            else
                System.Diagnostics.Debug.WriteLine("Try to find: "  + path)
            
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
                        null           
        )

        let assembly = Assembly.LoadFrom(@"C:\ProgramData\VSSonarExtension\Diagnostics\InternalDiagnostics\csharp\5.0-SNAPSHOT\SonarAnalyzer.CSharp.dll")

        let diagnosticAnalyzerType = typeof<DiagnosticAnalyzer>
        let analyzers = new System.Collections.Generic.List<DiagnosticAnalyzerType>()

        try
            for elem in assembly.GetTypes() do
                if elem.IsSubclassOf(diagnosticAnalyzerType) && not(elem.IsAbstract) then
                    try
                        let diag = Activator.CreateInstance(elem) :?> DiagnosticAnalyzer   
                        let diagt = new DiagnosticAnalyzerType()
                        diagt.Languages <- [|"C#"|]
                        diagt.Diagnostic <- diag
                        analyzers.Add(diagt)
                        let rule = new Rule(ConfigKey = "csharpsquid:" + diag.SupportedDiagnostics.[0].Id )
                        profileComplete.AddRule(rule)

                    with
                    | ex -> ()
        with
        | ex -> 
            let ex = ex :?> ReflectionTypeLoadException
            for t in ex.Types do
                System.Diagnostics.Debug.WriteLine("try to load: t: " + t.ToString())
                printf "%s" (ex.Types.Length.ToString())
                
        analyzers


    [<Test>]
    member test.``defined workspace reports issues`` () =
        let project = new NSonarQubeRunner(mockHttpReq, mockNotifier, rest, vshelper)
        let resource = new Resource(SolutionRoot = rootProject, SolutionName = "solution.sln")
        project.SetDiagnostics(diagnostics)
        project.UpdateWorkspace(resource, profile, mockNotifier, conf, "14.0")
        let item = new VsFileItem(FilePath = Path.Combine(rootProject, "sourcefile.cs"))
        item.Project <- new VsProjectItem()
        item.FilePath <- Path.Combine(rootProject, "sourcefile.cs")
        item.FileName <- "PluginsOptionsControl.cs"
        item.Project.ProjectFilePath <- Path.Combine(rootProject, "project.csproj")
        

        item.SonarResource <- new Resource(Key = "sdadsa")        
        let guid = Guid.NewGuid()
        project.SetCancelationToken(guid, new CancellationTokenSource())
        let issues = project.RunAnalysis(item, mockHttpReq, mockNotifier, true, guid)
        Assert.That(issues.Count, Is.EqualTo(1))
        Assert.That(issues.[0].Rule, Is.EqualTo("csharpsquid:S2357"))


    [<Test>]
    member test.``New file reports issues`` () =
        let project = new NSonarQubeRunner(mockHttpReq, mockNotifier, rest, vshelper)
        let resource = new Resource(SolutionRoot = rootProject, SolutionName = "solution.sln")
        project.SetDiagnostics(diagnostics)
        project.UpdateWorkspace(resource, profile, mockNotifier, conf, "14.0")

        let item = new VsFileItem(FilePath = Path.Combine(rootProject, "sourcefile.cs"))
        item.Project <- new VsProjectItem()
        item.FilePath <- Path.Combine(rootProject, "sourcefile.cs")
        item.FileName <- "PluginsOptionsControl.cs"
        item.Project.ProjectFilePath <- Path.Combine(rootProject, "project.csproj")        
        item.SonarResource <- new Resource(Key = "sdadsa")
        let guid = Guid.NewGuid()
        project.SetCancelationToken(guid, new CancellationTokenSource())             
        let issues = project.RunAnalysis(item, mockHttpReq, mockNotifier, true, guid)
        Assert.That(issues.Count, Is.EqualTo(1))
        Assert.That(issues.[0].Rule, Is.EqualTo("csharpsquid:S2357"))

        let item = new VsFileItem(FilePath = Path.Combine(rootProject,  "sourcefile.mod.cs"), FileName = "sourcefile.mod.cs")
        item.Project <- new VsProjectItem()
        item.Project.ProjectFilePath <- Path.Combine(rootProject, "project.csproj")
        item.SonarResource <- new Resource(Key = "keyofproject")
        let issues = project.RunAnalysis(item, mockHttpReq, mockNotifier, true, guid)
        Assert.That(issues.Count, Is.EqualTo(2))



    [<Test>]
    member test.``test unused private members`` () =
        let project = new NSonarQubeRunner(mockHttpReq, mockNotifier, rest, vshelper)
        let resource = new Resource(SolutionRoot = rootProject, SolutionName = "solution.sln")
        project.SetDiagnostics(diagnostics)
        project.UpdateWorkspace(resource, profile, mockNotifier, conf, "14.0")
        let item = new VsFileItem(FilePath = Path.Combine(rootProject, "Class2.cs"))
        item.Project <- new VsProjectItem()
        item.Project.Solution <- new VsSolutionItem()
        item.FilePath <- Path.Combine(rootProject, "Class2.cs")
        item.FileName <- "Class2.cs"
        item.Project.ProjectFilePath <- Path.Combine(rootProject, "project.csproj")
        item.Project.Solution.SolutionPath <- Path.Combine(rootProject, "solution.sln")
        

        item.SonarResource <- new Resource(Key = "sdadsa")        
        let guid = Guid.NewGuid()
        project.SetCancelationToken(guid, new CancellationTokenSource())
        let issues = project.RunAnalysis(item, mockHttpReq, mockNotifier, true, guid)
        let issues = project.RunAnalysis(item, mockHttpReq, mockNotifier, false, guid)
        Assert.That(issues.Count, Is.EqualTo(1))
        Assert.That(issues.[0].Rule, Is.EqualTo("csharpsquid:S1144"))

    [<Test>]
    member test.``real data test`` () =
        let project = new NSonarQubeRunner(mockHttpReq, mockNotifier, rest, vshelper)
        let resource = new Resource(SolutionRoot = @"E:\Development\SonarQube\VssonarExtension\VSSonarQubeExtension", SolutionName = "VSSonarQubeExtension2015.sln")
        project.SetDiagnostics(diagnostics)

        let profiles = new System.Collections.Generic.Dictionary<string, Profile>()
        profiles.Add("cs", profileComplete)
        project.UpdateWorkspace(resource, profiles, mockNotifier, conf, "14.0")
        let item = new VsFileItem(FilePath = @"E:\Development\SonarQube\VssonarExtension\VSSonarQubeExtension\VSSonarExtensionUi\ViewModel\Analysis\LocalViewModel.cs")
        item.Project <- new VsProjectItem()
        item.FilePath <- @"E:\Development\SonarQube\VssonarExtension\VSSonarQubeExtension\VSSonarExtensionUi\ViewModel\Analysis\LocalViewModel.cs"
        item.FileName <- "WorkspaceFeature.cs"
        item.Project.ProjectFilePath <- @"E:\Development\SonarQube\VssonarExtension\VSSonarQubeExtension\VSSonarExtensionUi\VSSonarExtensionUi.csproj"       
        item.Project.Solution <- new VsSolutionItem()
        item.Project.Solution.SolutionPath <- @"E:\Development\SonarQube\VssonarExtension\VSSonarQubeExtension\VSSonarQubeExtension2015.sln"

        item.SonarResource <- new Resource(Key = "sdadsa")        
        let guid = Guid.NewGuid()
        project.SetCancelationToken(guid, new CancellationTokenSource())
        let issues = project.RunAnalysis(item, mockHttpReq, mockNotifier, true, guid)
        let issues = project.RunAnalysis(item, mockHttpReq, mockNotifier, false, guid)
        Assert.That(issues.Count, Is.EqualTo(5))
        Assert.That(issues.[0].Rule, Is.EqualTo("csharpsquid:S125"))
        Assert.That(issues.[1].Rule, Is.EqualTo("csharpsquid:S125"))
        Assert.That(issues.[2].Rule, Is.EqualTo("csharpsquid:S125"))
        Assert.That(issues.[3].Rule, Is.EqualTo("csharpsquid:S1172"))
        Assert.That(issues.[4].Rule, Is.EqualTo("csharpsquid:S2933"))
 
        let issues = project.RunAnalysis(item, mockHttpReq, mockNotifier, false, guid)
        Assert.That(issues.Count, Is.EqualTo(5))
        Assert.That(issues.[0].Rule, Is.EqualTo("csharpsquid:S125"))
        Assert.That(issues.[1].Rule, Is.EqualTo("csharpsquid:S125"))
        Assert.That(issues.[2].Rule, Is.EqualTo("csharpsquid:S125"))
        Assert.That(issues.[3].Rule, Is.EqualTo("csharpsquid:S1172"))
        Assert.That(issues.[4].Rule, Is.EqualTo("csharpsquid:S2933"))       