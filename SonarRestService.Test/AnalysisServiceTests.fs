namespace SonarRestService.Test

open NUnit.Framework
open SonarRestService
open Foq
open System
open System.IO
open System.Net
open System.Web
open System.Linq

open VSSonarPlugins
open VSSonarPlugins.Types
open System.Reflection
open RestSharp

type ProjectAnalysisServiceTest() =
   
    let assemblyRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString()

    [<Test>]
    member test.``Fails Get ProjectAnalysis per Resource`` () =
        let conf = ConnectionConfiguration("http://sonar", "admin", "admin", 6.2)
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), any()) @>).Returns(AnalysisService.ProjectAnalysis.GetSample().JsonValue.ToString())
                .Create()

        let date = new DateTime(2017, 3, 3)
        let id, date, error = AnalysisService.GetAnalysisId(conf, new Resource( Key = "Tekla.Structures.DotApps:ComponentCatalog"), date, mockHttpReq)
        Assert.That(error, Is.EqualTo("Not available in this version of sonar"))

    [<Test>]
    member test.``Get ProjectAnalysis per Resource`` () =
        let conf = ConnectionConfiguration("http://sonar", "admin", "admin", 6.3)
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), any()) @>).Returns(AnalysisService.ProjectAnalysis.GetSample().JsonValue.ToString())
                .Create()

        let date = new DateTime(2017, 3, 3)
        let id, date, error = AnalysisService.GetAnalysisId(conf, new Resource( Key = "Tekla.Structures.DotApps:ComponentCatalog"), date, mockHttpReq)
        Assert.That(id, Is.EqualTo("AVp3YJo6J2GOl-mlN1z3"))


    [<Test>]
    member test.``Create Version in Project`` () =
        let conf = ConnectionConfiguration("http://sonar", "admin", "admin", 6.3)
        let reponse =
            Mock<IRestResponse>()
                .Setup(fun x -> <@ x.Content @>).Returns(AnalysisService.CreateVersionOk.GetSample().JsonValue.ToString())
                .Setup(fun x -> <@ x.StatusCode @>).Returns(Net.HttpStatusCode.OK)
                .Create()
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarPostRequest(any(), any(), any()) @>).Returns(reponse)
                .Create()

        let retu = AnalysisService.CreateVersion(conf, "AVp3YJo6J2GOl-mlN1z3", "test", mockHttpReq)
        Assert.That(retu, Is.EqualTo("ok"))



