module DifferencialService

open FSharp.Data
open VSSonarPlugins.Types
open SonarRestService
open System
open VSSonarPlugins

type ComponentTreeSearch = JsonProvider<""" {"paging":{"pageIndex":1,"pageSize":100,"total":2},"baseComponent":{"id":"e6cebf55-ccc4-4bc1-a27e-7b447c9f724c","key":"Project:ComponentBla","name":"ComponentBla","qualifier":"TRK","measures":[]},"components":[{"id":"AVEzWO92k3Oz8Oa46je4","key":"Project:ComponentBla:Project:ComponentBla:9AC47FE5-B1C8-416A-BFB4-632B7171E031:UndoRedoBlaModel.cs","name":"UndoRedoBlaModel.cs","qualifier":"FIL","path":"UndoRedoBlaModel.cs","language":"cs","measures":[{"metric":"new_uncovered_conditions","periods":[{"index":1,"value":"0"}]},{"metric":"new_uncovered_lines","periods":[{"index":1,"value":"1"}]},{"metric":"new_coverage","periods":[{"index":1,"value":"0.0"}]}]},{"id":"AVEzWO94k3Oz8Oa46jgV","key":"Project:ComponentBla:Project:ComponentBla:86AE155A-EC6F-4975-81F9-773177AD29FF:BlaTreeFeature.cs","name":"BlaTreeFeature.cs","qualifier":"FIL","path":"BlaTreeFeature.cs","language":"cs","measures":[{"metric":"new_uncovered_conditions","periods":[{"index":1,"value":"0"}]},{"metric":"new_uncovered_lines","periods":[{"index":1,"value":"1"}]},{"metric":"new_coverage","periods":[{"index":1,"value":"83.3333333333333"}]}]}]} """>
type PeriodsResponse = JsonProvider<""" {"component":{"id":"14170a50-b95b-4506-8f1a-19856f187137","key":"Project:ProjectName","name":"ProjectName","qualifier":"TRK","measures":[{"metric":"new_coverage","periods":[{"index":1,"value":"24.5416078984485"}]}]},"periods":[{"index":1,"mode":"previous_version","date":"2012-02-05T00:11:53+0200","parameter":"VersionName"}]} """>
type CoverageReportType = JsonProvider<""" {"component":{"id":"sdafldfjlakshdjfh","key":"Tekla.Structures.DotApps:ComponentCatalog","name":"ComponentCatalog","qualifier":"TRK","measures":[{"metric":"ncloc","value":"23622","periods":[{"index":1,"value":"23"}]},{"metric":"new_lines","periods":[{"index":1,"value":"62"}]},{"metric":"coverage","value":"62.8","periods":[{"index":1,"value":"11.0"}]},{"metric":"new_coverage","periods":[{"index":1,"value":"73.0769230769231"}]}]}} """>

let GetLeakPeriodStart(conf : ISonarConfiguration, projectIn : Resource, httpconnector : IHttpSonarConnector) =
    let url = sprintf "/api/measures/component?additionalFields=periods&componentKey=%s&metricKeys=lines" projectIn.Key
    let responsecontent = httpconnector.HttpSonarGetRequest(conf, url)
    let data = PeriodsResponse.Parse(responsecontent)
    data.Periods.[0].Date

let GetCoverageReportOnNewCodeOnLeak(conf : ISonarConfiguration, projectIn : Resource, httpconnector : IHttpSonarConnector, logger:ILogManager) =
    let coverageLeak = new System.Collections.Generic.Dictionary<string, CoverageDifferencial>()
    let AddComponentToLeak(comp:ComponentTreeSearch.Component, date:DateTime) = 
        let resource = new Resource()
        resource.Key <- comp.Key
        resource.IdString <- comp.Id
        resource.Qualifier <- comp.Qualifier
        resource.Path <- comp.Path
        resource.Name <- comp.Name
        resource.Lang <- comp.Language

        SourceService.GetLinesFromDateInResource(conf, resource, httpconnector, date)

        let covmeas = new CoverageDifferencial()
        covmeas.resource <- resource
        covmeas.Id <- comp.Id
        let newcov = comp.Measures |> Seq.find (fun meas -> meas.Metric = "new_coverage")
        covmeas.NewCoverage <- newcov.Periods.[0].Value
        let newcond = comp.Measures |> Seq.find (fun meas -> meas.Metric = "new_uncovered_conditions")
        covmeas.UncoveredConditons <- Convert.ToInt32(newcond.Periods.[0].Value)
        let newlines = comp.Measures |> Seq.find (fun meas -> meas.Metric = "new_uncovered_lines")
        covmeas.UncoveredLines <- Convert.ToInt32(newlines.Periods.[0].Value)

        if resource.Lines.Count > 0 then
            coverageLeak.Add(comp.Key, covmeas)

    let rec GetComponents(page:int, date:DateTime) =
        
        let url = sprintf "/api/measures/component_tree?asc=true&ps=100&metricSortFilter=withMeasuresOnly&s=metricPeriod,name&metricSort=new_coverage&metricPeriodSort=1&baseComponentKey=%s&metricKeys=new_coverage,new_uncovered_lines,new_uncovered_conditions&strategy=leaves&p=%i" projectIn.Key page
        logger.ReportMessage("Progress: " + url)
        let responsecontent = httpconnector.HttpSonarGetRequest(conf, url)
        let data = ComponentTreeSearch.Parse(responsecontent)

        data.Components |> Seq.iter (fun c -> AddComponentToLeak(c, date))

        if data.Components.Length = data.Paging.PageSize then
            let nextPage = page + 1
            try
            GetComponents(nextPage, date)
            with
            | ex -> let message = sprintf "Failed Page: %i with %s" nextPage ex.Message
                    logger.ReportMessage(message)

    try
        let startDate = GetLeakPeriodStart(conf, projectIn, httpconnector)
        GetComponents(1, startDate)
    with
    | ex -> logger.ReportMessage("Failed to Collect Coverage Leak: " + ex.Message + " for project: " + projectIn.Key)
    coverageLeak

let GetCoverageReport(conf : ISonarConfiguration, projectIn : Resource, httpconnector : IHttpSonarConnector) =
    let coverageReport = new System.Collections.Generic.Dictionary<string, CoverageReport>()
    let AddComponentToReport(comp:CoverageReportType.Component) = 
        let resource = new Resource()
        resource.Key <- comp.Key
        resource.IdString <- comp.Id
        resource.Qualifier <- comp.Qualifier
        resource.Name <- comp.Name

        let covmeas = new CoverageReport()
        covmeas.resource <- resource
        covmeas.Id <- comp.Id.ToString()
        let newcov = comp.Measures |> Seq.tryFind (fun meas -> meas.Metric = "new_coverage")
        if newcov.IsSome then
            covmeas.NewCoverage <- newcov.Value.Periods.[0].Value

        let newcond = comp.Measures |> Seq.tryFind (fun meas -> meas.Metric = "coverage")
        if newcond.IsSome then
            covmeas.Coverage <- Convert.ToDecimal(newcond.Value.Value.Value)

        let newlines = comp.Measures |> Seq.tryFind (fun meas -> meas.Metric = "new_lines")
        if newlines.IsSome then
            covmeas.NewLines <- Convert.ToInt64(newlines.Value.Periods.[0].Value)
        let lines = comp.Measures |> Seq.tryFind (fun meas -> meas.Metric = "ncloc")
        if lines.IsSome then
            covmeas.LinesOfCode <- Convert.ToInt64(lines.Value.Value.Value)

        coverageReport.Add(comp.Key, covmeas)


    let url = sprintf "/api/measures/component?componentKey=%s&metricKeys=new_coverage,ncloc,coverage,new_lines" projectIn.Key
    let responsecontent = httpconnector.HttpSonarGetRequest(conf, url)
    let data = CoverageReportType.Parse(responsecontent)
    AddComponentToReport(data.Component)

    coverageReport