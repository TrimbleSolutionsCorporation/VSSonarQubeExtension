namespace RoslynAdapter.Test

open NUnit.Framework
open FsUnit
open RoslynAdapter
open System.IO
open System.Text
open Foq
open SonarRestService
open ExtensionTypes
open ZeroMQ

type CommunicationTest() = 
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
    member test.``Test Publisher Syncs Ok`` () = 
        let adapter = RoslynAdapter()
        let diag = adapter.GetDiagnosticElementFromManifest("testdata/source.extension.vsixmanifest")
        let publisherMessages = adapter.GetSubscriberData(diag, profile)     
        adapter.CreateASubscriberWithMessages(publisherMessages)

        use context = ZmqContext.Create()

        let getParamsData(c : SubscriberElem) = 
            let mutable retData = ""
            for elem in c.GetParams do
                retData <- retData + ";" + elem.Key + "=" + elem.DefaultValue.Replace("\"", "")
            retData

        adapter.BroadcastPeriod <- 100
        for elm in publisherMessages do
            use subscriber = context.CreateSocket(SocketType.SUB)
            subscriber.Connect("tcp://127.0.0.1:5561")
            subscriber.Subscribe(Encoding.Unicode.GetBytes(elm.Id))
            let m = subscriber.Receive(Encoding.Unicode)
            let expectedData = sprintf "%s;%b;%s" elm.Id elm.Status (getParamsData(elm))
            m |> should equal expectedData
      


        