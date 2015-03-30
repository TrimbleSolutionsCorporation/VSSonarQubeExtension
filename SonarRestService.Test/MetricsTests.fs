namespace SonarRestService.Test

open NUnit.Framework
open FsUnit
open SonarRestService
open Foq
open System.IO

open VSSonarPlugins.Types

type MetricsTests() =

    [<Test>]
    member test.``Should Get Corret Duplications From Data`` () =
        let conf = ConnectionConfiguration("http://sonar", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), any()) @>).Returns(File.ReadAllText("testdata/duplicationsdata.txt"))
                .Create()
        let service = SonarRestService(mockHttpReq)
        let dups = (service :> ISonarRestService).GetDuplicationsDataInResource(conf, "groupid:projectid:directory/file.cpp")
        dups.Count |> should equal 66
