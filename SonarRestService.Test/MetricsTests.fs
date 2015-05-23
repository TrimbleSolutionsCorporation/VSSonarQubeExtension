namespace SonarRestService.Test

open NUnit.Framework
open SonarRestService
open Foq
open System.IO

open VSSonarPlugins
open VSSonarPlugins.Types
open System.Reflection

type MetricsTests() =
    let assemblyRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString()
    [<Test>]
    member test.``Should Get Corret Duplications From Data`` () =
        let conf = ConnectionConfiguration("http://sonar", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), any()) @>).Returns(File.ReadAllText(assemblyRunningPath + "/testdata/duplicationsdata.txt"))
                .Create()
        let service = SonarRestService(mockHttpReq)
        let dups = (service :> ISonarRestService).GetDuplicationsDataInResource(conf, "groupid:projectid:directory/file.cpp")
        Assert.That(dups.Count, Is.EqualTo(66))
