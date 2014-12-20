namespace RoslynAdapter.Test

open NUnit.Framework
open FsUnit
open RoslynAdapter
open System.IO
open Foq
open SonarRestService
open ExtensionTypes
open System.Text
open ZeroMQ

type RoslynAdapterTest() = 
    let profile = Profile()

    [<SetUp>]
    member test.``SetUp`` () =
        let conf = ConnectionConfiguration("http://sonar", "user", "user")
       
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), any()) @>).Returns(File.ReadAllText("testdata/profileusingappid.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)

        profile.Name <- "Sonar way"
        profile.Language <- "cs"
        (service :> ISonarRestService).GetRulesForProfile(conf, profile)
        profile.Rules.Count |> should equal 198
        profile.Rules.[1].Params.Count |> should equal 1

    [<TearDown>]
    member test.``tearDown`` () = ()
            
    [<Test>]
    member test.``If manifest does not contain diagnostic, it should add it`` () = 
        let adapter = RoslynAdapter()
        adapter.AddDiagnosticElementToManifest("testdata/Analyzer1.dll", "testdata/source.extension.vsixmanifest") |> should equal PatchSourceManifestRetCode.AddedMefAndAnalysis
        let textFile = File.ReadAllText("testdata/source.extension.vsixmanifest")
        textFile.Contains(""" <Asset d:Source="File" Path="diagnostics\Analyzer1.dll" d:VsixSubPath="diagnostics" Type="Microsoft.VisualStudio.Analyzer" />""") |> should be True
        textFile.Contains(""" <Asset d:Source="File" Path="diagnostics\Analyzer1.dll" d:VsixSubPath="diagnostics" Type="Microsoft.VisualStudio.MefComponent" />""") |> should be True

    [<Test>]
    member test.``test assembly not found`` () = 
        let adapter = RoslynAdapter()
        adapter.AddDiagnosticElementToManifest("testdata/Analyzer.dll", "testdata/source.extension.vsixmanifest") |> should equal PatchSourceManifestRetCode.FileNotFound

    [<Test>]
    member test.``assembly contains no diagnostics or codefixes`` () = 
        let adapter = RoslynAdapter()
        adapter.AddDiagnosticElementToManifest("RoslynAdapter.dll", "testdata/source.extension.vsixmanifest") |> should equal PatchSourceManifestRetCode.NotFoundDiagnosticOrCodeFixers

    [<Test>]
    member test.``gets correct number of diagnostics from assembly`` () = 
        let adapter = RoslynAdapter()
        let diagnostics = adapter.GetDiagnosticsFromAssembly("testdata/NSonarQubeDiagnostics.dll")
        diagnostics.Length |> should equal 24
        diagnostics.[0].Id |> should equal "AssignmentInsideSubExpression"
        

    [<Test>]
    member test.``gets correct number of codefix from assembly`` () = 
        let adapter = RoslynAdapter()
        let codeFix = adapter.GetCodeFixFromAssembly("testdata/Analyzer1.dll")
        codeFix.Length |> should equal 1
        codeFix.[0].GetFixableDiagnosticIds().Length |> should equal 1


    [<Test>]
    member test.``gets correct number of codefix from diagnostic`` () = 
        let adapter = RoslynAdapter()
        let codeFix = adapter.GetCodeFixFromAssembly("testdata/Analyzer1.dll")
        let codeDiag = adapter.GetDiagnosticsFromAssembly("testdata/Analyzer1.dll")

        let fix = adapter.GetCodeFixesForDiagnostics(codeDiag.[0].Id, codeFix)
        fix.Count |> should equal 1

    [<Test>]
    member test.``gets correct number of codefix from diagnostic, no codefix`` () = 
        let adapter = RoslynAdapter()
        let codeFix = adapter.GetCodeFixFromAssembly("testdata/NSonarQubeDiagnostics.dll")
        let codeDiag = adapter.GetDiagnosticsFromAssembly("testdata/NSonarQubeDiagnostics.dll")

        let fix = adapter.GetCodeFixesForDiagnostics(codeDiag.[0].Id, codeFix)
        fix.Count |> should equal 0

    [<Test>]
    member test.``gets subscriber list with enable and disable rules`` () = 
        let adapter = RoslynAdapter()
        let codeDiag = adapter.GetDiagnosticsFromAssembly("testdata/NSonarQubeDiagnostics.dll")

        let subscription = adapter.GetSubscriberData(codeDiag, profile)
        subscription.Length |> should equal 24
        let elem = subscription |> List.find (fun d -> d.Id.Equals("FileLoc"))
        elem.GetParams.Length |> should equal 0
        elem.Status |> should equal false

        let elem = subscription |> List.find (fun d -> d.Id.Equals("LineLength"))
        elem.GetParams.Length |> should equal 1
        elem.GetParams.[0].Key |> should equal "maximumLineLength"
        elem.Status |> should equal true

    [<Test>]
    member test.``should get correct number of diagnostics from manifest`` () = 
        let adapter = RoslynAdapter()
        let diag = adapter.GetDiagnosticElementFromManifest("testdata/source.extension.vsixmanifest")
        diag.Length |> should equal 28

    //[<Test>]
    member test.``start a server`` () = 
        use context = ZmqContext.Create()
        use publisher = context.CreateSocket(SocketType.PUB)
        use syncService = context.CreateSocket(SocketType.REP)
        publisher.Bind("tcp://*:5561")
        syncService.Bind("tcp://*:5562")

        
        let data = syncService.Receive(Encoding.Unicode)
        let data = syncService.Send("Ack", Encoding.Unicode)
        ()


        