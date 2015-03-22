namespace RoslynAdapter

open System.Xml
open System.Xml.Linq
open System
open System.Text
open System.IO
open FSharp.Data
open System.Reflection
open Microsoft.CodeAnalysis.CodeFixes
open Microsoft.CodeAnalysis.Diagnostics
open Microsoft.CodeAnalysis
open ExtensionTypes
open ZeroMQ
open System.Threading

type SourceManifest = XmlProvider<"""<?xml version="1.0" encoding="utf-8"?>
<PackageManifest Version="3.0.0" xmlns="http://schemas.microsoft.com/developer/vsx-schema/2011" xmlns:d="http://schemas.microsoft.com/developer/vsx-schema-design/2011">
  <Metadata>
    <Identity Id="VSSonarExtension2015-6FC40A4F-2B69-4DFB-98D0-08D50056643E" Version="3.0.0" Language="en-US" Publisher="Tekla Oy" />
    <DisplayName>VSSonarExtension2015</DisplayName>
    <Description xml:space="preserve">Visual Studio Extension For SonarQube(TM) Version 2015 - Rosylin</Description>
    <License>LICENSE.txt</License>
    <Icon>Resources\vsiximage.png</Icon>
    <PreviewImage>Resources\vsiximage.png</PreviewImage>
  </Metadata>
  <Installation>
    <InstallationTarget Version="[14.0,15.0)" Id="Microsoft.VisualStudio.IntegratedShell" />
    <InstallationTarget Version="[14.0,15.0)" Id="Microsoft.VisualStudio.Pro" />
  </Installation>
  <Dependencies>
    <Dependency Id="Microsoft.Framework.NDP" DisplayName="Microsoft .NET Framework" d:Source="Manual" Version="[4.5,)" />
  </Dependencies>
  <Assets>
    <Asset Type="Microsoft.VisualStudio.VsPackage" d:Source="Project" d:ProjectName="%CurrentProject%" Path="|%CurrentProject%;PkgdefProjectOutputGroup|" />
    <Asset Type="Microsoft.VisualStudio.MefComponent" d:Source="Project" d:ProjectName="%CurrentProject%" Path="|%CurrentProject%|" />
    <Asset d:Source="File" Path="vs2015\SQAnalyzer\NSonarQubeAnalyzer.exe" d:VsixSubPath="SQAnalyzer" Type="Microsoft.VisualStudio.Analyzer" />
  </Assets>
</PackageManifest>""">

type SubscriberElem(idIn:string) =    
    let mutable parameters : RuleParam List = List.Empty
    let mutable status = false

    member val Id : string = idIn with get, set
    member val Status = false with get, set

    member this.GetParams = parameters
    member this.AddParam(param : RuleParam) =        
        parameters <- parameters @ [param]

type PatchSourceManifestRetCode =
   | AddedMefAndAnalysis = 0
   | OnlyMef = 1
   | OnlyAnalysis = 2
   | AlreadyAdded = 3
   | FileNotFound = 4
   | NotFoundDiagnosticOrCodeFixers = 5

type ProxyDomain() =
    member x.GetCodeFixPresentInAssembly(path:string) = 
        let mutable codeFix = List.Empty
        try
            let assembly = Assembly.LoadFrom(path)
            let types2 = assembly.GetTypes()
            
            for types in types2 do
                if types.BaseType.Equals(typeof<CodeFixProvider>) then
                    let properties = types.GetFields(BindingFlags.NonPublic)                    
                    let data : System.Object array = Array.zeroCreate 1
                                        
                    let diagnosticConstructor = types.GetConstructor(Type.EmptyTypes)
                    let diagnosticConstructorObj = diagnosticConstructor.Invoke(null) :?> CodeFixProvider
                    codeFix <- codeFix @ [diagnosticConstructorObj]

            codeFix
        with
        | ex -> codeFix

    member x.GetDiagnosticPresentInAssembly(path:string) = 
        let mutable diagnostics = List.Empty
        try
            let assembly = Assembly.LoadFrom(path)
            let types2 = assembly.GetTypes()
            

            for types in types2 do
                try
                    if types.IsSubclassOf(typeof<DiagnosticAnalyzer>) then
                        let properties = types.GetFields(BindingFlags.NonPublic)                   
                        let data : System.Object array = Array.zeroCreate 1
                                        
                        let diagnosticConstructor = types.GetConstructor(Type.EmptyTypes)
                        let diagnosticConstructorObj = diagnosticConstructor.Invoke(null) :?> DiagnosticAnalyzer

                        for rule in diagnosticConstructorObj.SupportedDiagnostics do
                            diagnostics <- diagnostics @ [rule]
                with
                | ex -> ()

            diagnostics
        with
        | ex -> diagnostics

    member x.IsDiagnosticPresentInAssembly(path:string) = 
        try
            let assembly = Assembly.LoadFrom(path)
            let types2 = assembly.GetTypes()
            
            let elem = seq types2 |> Seq.tryFind (fun c -> c.BaseType.Equals(typeof<DiagnosticAnalyzer>))
            match elem with 
                | Some x -> true
                | None -> false
        with
        | ex -> false

    member x.IsCodeFixPresentInAssembly(path:string) = 
        try
            let assembly = Assembly.LoadFrom(path)
            let types2 = assembly.GetTypes()
            
            let elem = seq types2 |> Seq.tryFind (fun c -> c.BaseType.Equals(typeof<CodeFixProvider>))
            match elem with 
                | Some x -> true
                | None -> false
        with
        | ex -> false