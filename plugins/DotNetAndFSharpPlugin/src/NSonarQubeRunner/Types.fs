module Types

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

type AnalyzerAdditionalFile(path : string) =
    inherit AdditionalText()

    override this.Path : string = path

    override this.GetText(cancellationToken : CancellationToken) =
        SourceText.From(File.ReadAllText(path))

type RuleSet = XmlProvider<"""
<RuleSet Name="Rules for ClassLibrary2" Description="Code analysis rules for ClassLibrary2.csproj." ToolsVersion="14.0">
  <Rules AnalyzerId="Microsoft.Analyzers.ManagedCodeAnalysis" RuleNamespace="Microsoft.Rules.Managed">
    <Rule Id="CA1001" Action="Warning" />
    <Rule Id="CA1009" Action="Warning" />
  </Rules>
  <Rules AnalyzerId="StyleCop.Analyzers" RuleNamespace="StyleCop.Analyzers">
    <Rule Id="SA1305" Action="Warning" />
    <Rule Id="SA1634" Action="None" />
  </Rules>
</RuleSet>
""" >

type ProjectFile = XmlProvider<""" 
<Project ToolsVersion="14.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="System.Xml" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Properties\AssemblyInfo.cs" />
  </ItemGroup>
  <ItemGroup>
    <None Include="ClassLibrary2.data" />
    <None Include="ClassLibrary2.ruleset" />
    <AdditionalFiles Include="stylecop.json" />
    <AdditionalFiles Include="stylecop2.json" />    
  </ItemGroup>
  <ItemGroup>
    <Analyzer Include="..\packages\StyleCop.Analyzers.1.0.0-rc2\analyzers\dotnet\cs\StyleCop.Analyzers.CodeFixes.dll" />
    <Analyzer Include="..\packages\StyleCop.Analyzers.1.0.0-rc2\analyzers\dotnet\cs\StyleCop.Analyzers.dll" />
  </ItemGroup>
  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
</Project> """>

[<AllowNullLiteral>]
type RulesetData() =
    member val AnalyzerId : string = "" with get, set 
    member val RuleNamespace : string = "" with get, set
    member val DisabledRulesInRuleSet : string list = list.Empty with get, set

type ProjectData() =
    member val AdditionDocuments : string [] = [||] with get, set 
    member val Diagnostics : DiagnosticAnalyzerType list = list.Empty with get, set
    member val Path : string = "" with get, set
    member val RuleSet : RulesetData = null with get, set
    member val Profile : System.Collections.Generic.Dictionary<string, Profile> = new System.Collections.Generic.Dictionary<string, Profile>() with get, set


let LoadDiagnosticsFromPath(path : string) = 
    
    let runningPath = Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")).ToString()

    AppDomain.CurrentDomain.add_AssemblyResolve(fun _ args ->
            
        let name = System.Reflection.AssemblyName(args.Name)
        
        let path = Path.Combine(runningPath, name.Name + ".dll")

        if name.Name = "System.Windows.Interactivity" || name.Name = "FSharp.Core.resources" || name.Name.EndsWith(".resources") then
            null
        else
            printf "Request to load %s %s\n\r" args.Name (path)
            
            let existingAssembly = 
                System.AppDomain.CurrentDomain.GetAssemblies()
                |> Seq.tryFind(fun a -> System.Reflection.AssemblyName.ReferenceMatchesDefinition(name, a.GetName()))
            match existingAssembly with
            | Some a -> a
            | None -> 
                let path = Path.Combine(runningPath, name.Name + ".dll")
                if File.Exists(path) then 
                    let inFileAssembly = Assembly.LoadFile(path)
                    inFileAssembly
                else
                    let folder = Path.GetDirectoryName(path)
                    let path = Path.Combine(folder, name.Name + ".dll")
                    if File.Exists(path) then
                        let inFileAssembly = Assembly.LoadFile(path)
                        inFileAssembly
                    else
                        null
    )

    let assembly = Assembly.LoadFrom(path)

    let mutable analyzers = List.Empty

    try
        for elem in assembly.GetTypes() do
            if elem.IsSubclassOf(typeof<DiagnosticAnalyzer>) && not(elem.IsAbstract) then
                try
                    let diag = Activator.CreateInstance(elem) :?> DiagnosticAnalyzer
                    let attributes = elem.GetCustomAttributes()

                    let attribute = Attribute.GetCustomAttribute(elem, typeof<DiagnosticAnalyzerAttribute>) :?> DiagnosticAnalyzerAttribute
                    analyzers <- analyzers @ [new DiagnosticAnalyzerType(Diagnostic = diag, Languages = attribute.Languages)]
                with
                | ex -> ()
    with
    | ex -> 
        let ex = ex :?> ReflectionTypeLoadException
        for t in ex.LoaderExceptions do
            System.Diagnostics.Debug.WriteLine("Failed to loaded : "  + (t.ToString()) + "\r\n")
                
    printf "[RoslynRunner] Loaded %i diagnostic analyzers from %s\n\r" analyzers.Length (path)
    analyzers 