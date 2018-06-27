module Analysers

open System.IO
open VSSonarPlugins.Types
open VSSonarPlugins
open System
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.Diagnostics
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.MSBuild
open Microsoft.CodeAnalysis.Text
open System.Collections.Immutable
open System.Threading
open System.Collections.Generic

open Types

let mutable MSbuildOpenSolution : Solution = null
let CurrentWorkspace = MSBuildWorkspace.Create()

let getLanguage(itemInView : VsFileItem) =
    let extension = Path.GetExtension(itemInView.FilePath.ToLower())
    if  extension.EndsWith(".cs") then
        "C#"
    elif extension.EndsWith(".vb") then
        "VB.NET"
    else
        ""

let issuesFromDiagnostics(result : ImmutableArray<Diagnostic>, itemInView : VsFileItem, externlProfileIn : Profile, repoid : string) =
        
    let mutable issues = Map.empty
    let issuesret = new System.Collections.Generic.List<Issue>()

    for issue in result do
        if issue.Location.SourceTree.FilePath.ToLower().Equals(itemInView.FilePath.ToLower()) then
            let issueNew = new Issue()
            issueNew.Component <- itemInView.SonarResource.Key
            issueNew.Rule <- repoid + ":" + issue.Id
            issueNew.HelpUrl <- issue.Descriptor.HelpLinkUri
            issueNew.Line <- issue.Location.GetLineSpan().StartLinePosition.Line + 1
            issueNew.Message <- issue.GetMessage()
            issueNew.LocalPath <- issue.Location.SourceTree.FilePath
            let rule = externlProfileIn.GetRule(issueNew.Rule)
            if rule <> null then
                issueNew.Severity <- rule.Severity
                issueNew.Effort <- rule.DebtRemFnCoeff
                let key = issueNew.Message + issueNew.Line.ToString()
                if not(issues.ContainsKey(key)) then
                    issues <- issues.Add(key,issueNew)
                    issuesret.Add(issueNew)

    issuesret

let mutable partialSyntaxTrees : System.Collections.Generic.Dictionary<string, SyntaxTree list> = new System.Collections.Generic.Dictionary<string, SyntaxTree list>()
let mutable filepartialSyntaxTrees : System.Collections.Generic.Dictionary<string, SyntaxTree list> = new System.Collections.Generic.Dictionary<string, SyntaxTree list>()

// gets from a compilation all trees that belong to item in view
let GetSyntaxTreeWithReferences(compilation : Compilation, itemInView : VsFileItem) =  
    let mutable syntaxTree : SyntaxTree list = List.Empty
    partialSyntaxTrees <- new System.Collections.Generic.Dictionary<string, SyntaxTree list>()
    filepartialSyntaxTrees <- new System.Collections.Generic.Dictionary<string, SyntaxTree list>()

    for tree in compilation.SyntaxTrees do
        let root = tree.GetRootAsync().Result
        root.DescendantNodes() |> Seq.iter (fun c -> 
                                                if (c :? Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax) then
                                                    let classdata = c :?> Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax
                                                    System.Diagnostics.Debug.WriteLine(classdata.Identifier)
                                                                    
                                                    let containsPartial = classdata.Modifiers |> Seq.tryFind (fun c -> c.Text.Equals("partial")) 
                                                    match containsPartial with
                                                    | Some v -> 
                                                        if partialSyntaxTrees.ContainsKey(classdata.Identifier.ToString()) then
                                                            partialSyntaxTrees.[classdata.Identifier.ToString()] <- partialSyntaxTrees.[classdata.Identifier.ToString()] @ [tree]
                                                        else                                                                        
                                                            partialSyntaxTrees.Add(classdata.Identifier.ToString(), [tree])

                                                        if tree.FilePath.ToLower().Equals(itemInView.FilePath.ToLower()) then
                                                            if filepartialSyntaxTrees.ContainsKey(classdata.Identifier.ToString()) then
                                                                filepartialSyntaxTrees.[classdata.Identifier.ToString()] <- filepartialSyntaxTrees.[classdata.Identifier.ToString()] @ [tree]
                                                            else                                                                        
                                                                filepartialSyntaxTrees.Add(classdata.Identifier.ToString(), [tree])
                                                    | _ -> ()
                                                    )

        if tree.FilePath.ToLower().Equals(itemInView.FilePath.ToLower()) then
            syntaxTree <- [tree]
                                                                                                                         
    if filepartialSyntaxTrees.Count <> 0 then
        syntaxTree <- List.Empty
        for partree in filepartialSyntaxTrees do
            syntaxTree <- syntaxTree @ partialSyntaxTrees.[partree.Key]

    syntaxTree


let runRoslynOnCompilationUnit(compilation : Compilation, ids, builder : DiagnosticAnalyzer list, sonarAdditionalDocument : string [], token : CancellationTokenSource) =
        
    let mutable docs = List.Empty
    for doc in sonarAdditionalDocument do
        docs <- docs @ [new AnalyzerAdditionalFile(doc) :> AdditionalText]

    let optionsWithAdditionalFiles = new AnalyzerOptions((List.toArray docs).ToImmutableArray())

    let options = 
        (if compilation.Language.Equals(LanguageNames.CSharp) then
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            else
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        ).WithSpecificDiagnosticOptions(ids)
        
    let compilationWithOptions = compilation.WithOptions(options)    
    let analyserMain = compilationWithOptions.WithAnalyzers(builder.ToImmutableArray(), optionsWithAdditionalFiles, token.Token)
    analyserMain.GetAnalyzerDiagnosticsAsync().Result

let runRoslynOnCompilationUnitTree(compilation : Compilation, ids, builder : DiagnosticAnalyzer list, sonarAdditionalDocument : string [], token : CancellationTokenSource, itemInView : VsFileItem) =
        
    let mutable docs = List.Empty
    for doc in sonarAdditionalDocument do
        docs <- docs @ [new AnalyzerAdditionalFile(doc) :> AdditionalText]

    let optionsWithAdditionalFiles = new AnalyzerOptions((List.toArray docs).ToImmutableArray())

    let options = 
        (if compilation.Language.Equals(LanguageNames.CSharp) then
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            else
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        ).WithSpecificDiagnosticOptions(ids)
        
    let compilationWithOptions = compilation.WithOptions(options)    
    let analyserMain = compilationWithOptions.WithAnalyzers(builder.ToImmutableArray(), optionsWithAdditionalFiles, token.Token)
    let tree = compilationWithOptions.SyntaxTrees |> Seq.find (fun c -> c.FilePath.ToLower().Equals(itemInView.FilePath.ToLower()))
    let sematincModel = compilation.GetSemanticModel(tree)
    let lenght = tree.Length
    let span = new TextSpan(0, lenght)
    let semanticDiag = analyserMain.GetAnalyzerSemanticDiagnosticsAsync(sematincModel, Nullable span, token.Token).Result
    let syntaxDiag = analyserMain.GetAnalyzerSyntaxDiagnosticsAsync(tree, token.Token).Result
    syntaxDiag, semanticDiag
    


let runAnalyserWithMSBuild(itemInView : VsFileItem,
                           vsHelper : IConfigurationHelper,
                           solution : ProjectTypes.Solution,
                           additionalFiles : string [],
                           ids : KeyValuePair<string, ReportDiagnostic> list,
                           buildertypes : DiagnosticAnalyzerType list,
                           externlProfileIn : Profile,
                           synctype : bool,
                           token : CancellationTokenSource,
                           fromSave : bool,
                           solutionPath : string,
                           vshelper : IVsEnvironmentHelper,
                           notificationManager : INotificationManager) =
           
    let fileLang = getLanguage(itemInView)
    let mutable forceAnalysis = false

    if vshelper.GetCurrentRoslynSolution() <> null then
        notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner" , Data = "Using Visual Studio WorkSpace"))
        MSbuildOpenSolution <- vshelper.GetCurrentRoslynSolution()
    else
        if fromSave then
            notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner" , Data = "Using MSBuild WorkSpace"))    
            // until https://github.com/dotnet/roslyn/issues/7842
            MSbuildOpenSolution <- CurrentWorkspace.OpenSolutionAsync(solutionPath).Result

    let language = 
        if Path.GetExtension(itemInView.FileName).Equals(".cs") then
            LanguageNames.CSharp
        else
            LanguageNames.VisualBasic

    let project = (MSbuildOpenSolution.Projects |> Seq.tryFind (fun c -> c.FilePath.ToLower().Equals(itemInView.Project.ProjectFilePath.ToLower()))).Value

    let isTheSame(doc : Document) =
        Path.GetFullPath(doc.FilePath.ToLower()).Equals(Path.GetFullPath(itemInView.FilePath.ToLower()))
            
    let mutable issues = new System.Collections.Generic.List<Issue>()                                

    let documentFound = (project.Documents |> Seq.tryFind (fun doc -> isTheSame(doc)))
    match documentFound with
    | Some document ->             
        try
            let compilation = document.Project.GetCompilationAsync().Result
            let extension = Path.GetExtension(itemInView.FilePath).ToLower()
            let builder, langids =
                
                let mutable count = 0
                let lang = List.Empty
                let mutable bd = List.Empty
                let mutable idsbd = List.Empty

                for datam in buildertypes do
                    let findType(c:DiagnosticAnalyzer) = 
                        let name1 = c.GetType().FullName
                        let name2 = datam.Diagnostic.GetType().FullName
                        name1.Equals(name2)

                    let isdefinedforlang = datam.Languages |> Seq.tryFind( fun c -> c.Equals(fileLang))
                    match isdefinedforlang with
                    | Some lang -> 
                            let found = bd |> Seq.tryFind( fun c -> findType(c))

                            if found.IsNone then
                                bd <- bd @ [datam.Diagnostic]

                            idsbd <- idsbd @ [ids.[count]]
                    | _ -> ()

                    count <- count + 1
                bd, idsbd

            let syntaxDiag, semanticDiag = runRoslynOnCompilationUnitTree(compilation, ids.ToImmutableDictionary(), builder, additionalFiles, token, itemInView)

            issues.AddRange(issuesFromDiagnostics(syntaxDiag, itemInView, externlProfileIn, "csharpsquid"))
            issues.AddRange(issuesFromDiagnostics(syntaxDiag, itemInView, externlProfileIn, "roslyn-cs"))
            issues.AddRange(issuesFromDiagnostics(syntaxDiag, itemInView, externlProfileIn, "roslyn-vbnet"))
            issues.AddRange(issuesFromDiagnostics(semanticDiag, itemInView, externlProfileIn, "csharpsquid"))
            issues.AddRange(issuesFromDiagnostics(semanticDiag, itemInView, externlProfileIn, "roslyn-cs"))
            issues.AddRange(issuesFromDiagnostics(semanticDiag, itemInView, externlProfileIn, "roslyn-vbnet"))
        with
        | ex -> notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner" , Data = "Roslyn Analysis Failed : " + ex.Message))

    | _ -> notificationManager.ReportMessage(new Message(Id = "NSonarQubeRunner" , Data = "Unable to find document in WorkSpace, ensure a build is done before using the extension."))   
                   
    issues