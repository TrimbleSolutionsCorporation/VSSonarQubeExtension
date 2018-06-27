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

type DifferentialServiceTest() =
   
    let assemblyRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString()
    let leakFromToday = """ {"component":{"id":"14170a50-b95b-4506-8f1a-19856f187137","key":"Project:ProjectName","name":"ProjectName","qualifier":"TRK","measures":[{"metric":"new_coverage","periods":[{"index":1,"value":"24.5416078984485"}]}]},"periods":[{"index":1,"mode":"previous_version","date":"2018-05-05T00:11:53+0200","parameter":"VersionName"}]} """
    let mockNotMan = Mock<INotificationManager>()

    [<Test>]
    member test.``No new Lines Get Coverage On Leak`` () =
        let conf = ConnectionConfiguration("http://sonar", "admin", "admin", 6.2)

        let url0 = "/api/measures/component?additionalFields=periods&componentKey=projectKey&metricKeys=lines"

        let url1 = "/api/sources/lines?uuid=AVEzWO92k3Oz8Oa46je4&from=1&to=500"
        let url2 = "/api/sources/lines?uuid=AVEzWO92k3Oz8Oa46je4&from=501&to=1000"

        let url4 = "/api/sources/lines?uuid=AVEzWO94k3Oz8Oa46jgV&from=1&to=500"
        let url5 = "/api/sources/lines?uuid=AVEzWO94k3Oz8Oa46jgV&from=501&to=1000"

        let url3 = "/api/measures/component_tree?asc=true&ps=100&metricSortFilter=withMeasuresOnly&s=metricPeriod,name&metricSort=new_coverage&metricPeriodSort=1&baseComponentKey=projectKey&metricKeys=new_coverage,new_uncovered_lines,new_uncovered_conditions&strategy=leaves&p=1"

        let data = """ {"paging":{"pageIndex":1,"pageSize":100,"total":2},"baseComponent":{"id":"e6cebf55-ccc4-4bc1-a27e-7b447c9f724c","key":"Project:ComponentBla","name":"ComponentBla","qualifier":"TRK","measures":[]},"components":[{"id":"AVEzWO92k3Oz8Oa46je4","key":"Project:ComponentBla:Project:ComponentBla:9AC47FE5-B1C8-416A-BFB4-632B7171E031:UndoRedoBlaModel.cs","name":"UndoRedoBlaModel.cs","qualifier":"FIL","path":"UndoRedoBlaModel.cs","language":"cs","measures":[{"metric":"new_uncovered_conditions","periods":[{"index":1,"value":"0"}]},{"metric":"new_uncovered_lines","periods":[{"index":1,"value":"1"}]},{"metric":"new_coverage","periods":[{"index":1,"value":"0.0"}]}]},{"id":"AVEzWO94k3Oz8Oa46jgV","key":"Project:ComponentBla:Project:ComponentBla:86AE155A-EC6F-4975-81F9-773177AD29FF:BlaTreeFeature.cs","name":"BlaTreeFeature.cs","qualifier":"FIL","path":"BlaTreeFeature.cs","language":"cs","measures":[{"metric":"new_uncovered_conditions","periods":[{"index":1,"value":"0"}]},{"metric":"new_uncovered_lines","periods":[{"index":1,"value":"1"}]},{"metric":"new_coverage","periods":[{"index":1,"value":"83.3333333333333"}]}]}]} """
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url0) @>).Returns(leakFromToday)
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url3) @>).Returns(data)
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url1) @>).Returns(SourceService.Lines.GetSample().JsonValue.ToString())
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url2) @>).Returns("{\"sources\":[]}")
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url4) @>).Returns(SourceService.Lines.GetSample().JsonValue.ToString())
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url5) @>).Returns("{\"sources\":[]}")

        let resource = new Resource( Key = "projectKey", IdString = "id")
        let resources = DifferencialService.GetCoverageReportOnNewCodeOnLeak(conf, resource, mockHttpReq.Create(), mockNotMan.Create())
        Assert.That(resources.Count, Is.EqualTo(2))
        let covme = resources.["Project:ComponentBla:Project:ComponentBla:9AC47FE5-B1C8-416A-BFB4-632B7171E031:UndoRedoBlaModel.cs"]
        Assert.That(covme.Id, Is.EqualTo("AVEzWO92k3Oz8Oa46je4"))
        Assert.That(covme.resource.Lines.Count, Is.EqualTo(1))

    [<Test>]
    member test.``Get Coverage On Leak`` () =
        let conf = ConnectionConfiguration("http://sonar", "admin", "admin", 6.2)

        let url0 = "/api/measures/component?additionalFields=periods&componentKey=projectKey&metricKeys=lines"

        let url1 = "/api/sources/lines?uuid=AVEzWO92k3Oz8Oa46je4&from=1&to=500"
        let url2 = "/api/sources/lines?uuid=AVEzWO92k3Oz8Oa46je4&from=501&to=1000"

        let url4 = "/api/sources/lines?uuid=AVEzWO94k3Oz8Oa46jgV&from=1&to=500"
        let url5 = "/api/sources/lines?uuid=AVEzWO94k3Oz8Oa46jgV&from=501&to=1000"

        let url3 = "/api/measures/component_tree?asc=true&ps=100&metricSortFilter=withMeasuresOnly&s=metricPeriod,name&metricSort=new_coverage&metricPeriodSort=1&baseComponentKey=projectKey&metricKeys=new_coverage,new_uncovered_lines,new_uncovered_conditions&strategy=leaves&p=1"

        let data = """ {"paging":{"pageIndex":1,"pageSize":100,"total":2},"baseComponent":{"id":"e6cebf55-ccc4-4bc1-a27e-7b447c9f724c","key":"Project:ComponentBla","name":"ComponentBla","qualifier":"TRK","measures":[]},"components":[{"id":"AVEzWO92k3Oz8Oa46je4","key":"Project:ComponentBla:Project:ComponentBla:9AC47FE5-B1C8-416A-BFB4-632B7171E031:UndoRedoBlaModel.cs","name":"UndoRedoBlaModel.cs","qualifier":"FIL","path":"UndoRedoBlaModel.cs","language":"cs","measures":[{"metric":"new_uncovered_conditions","periods":[{"index":1,"value":"0"}]},{"metric":"new_uncovered_lines","periods":[{"index":1,"value":"1"}]},{"metric":"new_coverage","periods":[{"index":1,"value":"0.0"}]}]},{"id":"AVEzWO94k3Oz8Oa46jgV","key":"Project:ComponentBla:Project:ComponentBla:86AE155A-EC6F-4975-81F9-773177AD29FF:BlaTreeFeature.cs","name":"BlaTreeFeature.cs","qualifier":"FIL","path":"BlaTreeFeature.cs","language":"cs","measures":[{"metric":"new_uncovered_conditions","periods":[{"index":1,"value":"0"}]},{"metric":"new_uncovered_lines","periods":[{"index":1,"value":"1"}]},{"metric":"new_coverage","periods":[{"index":1,"value":"83.3333333333333"}]}]}]} """
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url0) @>).Returns(DifferencialService.PeriodsResponse.GetSample().JsonValue.ToString())
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url3) @>).Returns(data)
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url1) @>).Returns(SourceService.Lines.GetSample().JsonValue.ToString())
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url2) @>).Returns("{\"sources\":[]}")
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url4) @>).Returns(SourceService.Lines.GetSample().JsonValue.ToString())
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url5) @>).Returns("{\"sources\":[]}")

        let resource = new Resource( Key = "projectKey", IdString = "id")
        let resources = DifferencialService.GetCoverageReportOnNewCodeOnLeak(conf, resource, mockHttpReq.Create(), mockNotMan.Create())
        Assert.That(resources.Count, Is.EqualTo(2))
        let covme = resources.["Project:ComponentBla:Project:ComponentBla:9AC47FE5-B1C8-416A-BFB4-632B7171E031:UndoRedoBlaModel.cs"]
        Assert.That(covme.Id, Is.EqualTo("AVEzWO92k3Oz8Oa46je4"))
        Assert.That(covme.resource.Lines.Count, Is.EqualTo(1))
        


