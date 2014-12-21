namespace RoslynAdapter.Test

open NUnit.Framework
open FsUnit
open RoslynAdapter
open System.IO
open System
open System.Text
open Foq
open SonarRestService
open ExtensionTypes
open ZeroMQ
open System.Diagnostics

type CommunicationTest() = 
    let profile = Profile()

    [<SetUp>]
    member test.``SetUp`` () =
        let conf = ConnectionConfiguration("http://sonar", "user", "user")
       
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), any()) @>).Returns(File.ReadAllText("profileusingappid.txt"))
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
    member test.``Test Master Server Syncs Ok`` () = 
        let adapter = RoslynAdapter()
        let diag = adapter.GetDiagnosticElementFromManifest(Path.Combine(Environment.CurrentDirectory, "source.extension.vsixmanifest"))
        let publisherMessages = adapter.GetSubscriberData(diag, profile)     
        adapter.CreateASubscriberWithMessages(publisherMessages)

        use context = ZmqContext.Create()

        let getParamsData(c : SubscriberElem) = 
            let mutable retData = ""
            for elem in c.GetParams do
                retData <- retData + ";" + elem.Key + "=" + elem.DefaultValue.Replace("\"", "")
            retData

        adapter.BroadcastPeriod <- 100
        let currentProcess = Process.GetCurrentProcess()
        let mutable id = currentProcess.Id
        if id < 5000 then
            id <- id + 5000

        for elm in publisherMessages do
            use subscriber = context.CreateSocket(SocketType.REQ)
            subscriber.Connect(sprintf "tcp://127.0.0.1:%i" id)
            subscriber.Send(Encoding.Unicode.GetBytes(elm.Id)) |> ignore
            let message = subscriber.ReceiveMessage()
            let m = Encoding.Unicode.GetString(message.[0].Buffer)
            let expectedData = sprintf "%s;%b;%s" elm.Id elm.Status (getParamsData(elm))
            m |> should equal expectedData
        
        adapter.EndBroadcaster <- true
        use subscriber = context.CreateSocket(SocketType.REQ)
        subscriber.Connect(sprintf "tcp://127.0.0.1:%i" id)
        subscriber.Send(Encoding.Unicode.GetBytes("")) |> ignore
        let message = subscriber.ReceiveMessage()
        ()
