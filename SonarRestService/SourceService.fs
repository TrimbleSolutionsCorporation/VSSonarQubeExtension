module SourceService

open FSharp.Data
open FSharp.Data.JsonExtensions
open VSSonarPlugins
open VSSonarPlugins.Types
open SonarRestService
open System

type Lines = JsonProvider<""" {"sources":[{"line":1,"lineHits": 1,"conditions": 2,"coveredConditions": 1,"code":"<span class=\"cd\">// -----------------------------------------------------------------------</span>","duplicated":false},{"line":2,"code":"<span class=\"cd\">// &lt;copyright file=\"CatalogTreeFeature.cs\" company=\"org\"&gt;</span>","scmAuthor":"email@org.com@org.com","scmRevision":"048122991a97ceb117da5e98a30ed3a4a13d37dc","scmDate":"2013-09-10T16:57:00+0300","duplicated":false},{"line":3,"code":"<span class=\"cd\">//   Copyright (c) by org 2013-2014. All rights reserved.</span>","scmAuthor":"email@org.com","scmRevision":"9ceaf377faaff910a9d2d590d4ccf7726e957804","scmDate":"2014-09-02T15:19:01+0300","duplicated":false},{"line":4,"code":"<span class=\"cd\">// &lt;/copyright&gt;</span>","scmAuthor":"email@org.com@org.com","scmRevision":"048122991a97ceb117da5e98a30ed3a4a13d37dc","scmDate":"2013-09-10T16:57:00+0300","duplicated":false},{"line":5,"code":"<span class=\"cd\">// -----------------------------------------------------------------------</span>","scmAuthor":"email@org.com","scmRevision":"9ceaf377faaff910a9d2d590d4ccf7726e957804","scmDate":"2014-09-02T15:19:01+0300","duplicated":false},{"line":6,"code":"<span class=\"k\">namespace</span> CatalogTree","scmAuthor":"email@org.com@org.com","scmRevision":"048122991a97ceb117da5e98a30ed3a4a13d37dc","scmDate":"2013-09-10T16:57:00+0300","duplicated":false},{"line":7,"code":"{","scmAuthor":"email@org.com@org.com","scmRevision":"048122991a97ceb117da5e98a30ed3a4a13d37dc","scmDate":"2013-09-10T16:57:00+0300","duplicated":false},{"line":8,"code":"    <span class=\"k\">using</span> System;","scmAuthor":"email@org.com@org.com","scmRevision":"048122991a97ceb117da5e98a30ed3a4a13d37dc","scmDate":"2013-09-10T16:57:00+0300","duplicated":false},{"line":9,"code":"    <span class=\"k\">using</span> System.Collections.Concurrent;","scmAuthor":"email@org.com@org.com","scmRevision":"65db4135bb751d5a402f9f49183c3821bd55872a","scmDate":"2015-02-25T18:24:41+0200","duplicated":false},{"line":10,"code":"    <span class=\"k\">using</span> System.Collections.Generic;","scmAuthor":"email@org.com@org.com","scmRevision":"da62a8e5e9c608ca50a3641f3bec6b0090de5d55","scmDate":"2014-09-22T19:24:52+0300","duplicated":false}]} """>

let UpdateSourceLinesInResource(conf : ISonarConfiguration, resource : Resource, httpconnector : IHttpSonarConnector) =
    let rec GetLines(from:int, into:int) =
        let CreateLineInResource(linedata:Lines.Sourcis) = 
            if not(resource.Lines.ContainsKey(linedata.Line)) then
                let line = new Line()
                line.Code <- linedata.Code
                line.Id <- linedata.Line
                if linedata.LineHits.IsSome then
                    line.IsExecutableLine <- true
                line.LineHits <- if linedata.LineHits.IsSome then linedata.LineHits.Value else 0
                line.Conditions <- if linedata.Conditions.IsSome then linedata.Conditions.Value else 0
                line.CoveredConditions <- if linedata.CoveredConditions.IsSome then linedata.CoveredConditions.Value else 0
                line.Duplicated <- linedata.Duplicated
                line.ScmAuthor <- if linedata.ScmAuthor.IsSome then linedata.ScmAuthor.Value else ""
                line.ScmRevision <- if linedata.ScmRevision.IsSome then linedata.ScmRevision.Value else ""
                line.ScmDate <- if linedata.ScmDate.IsSome then linedata.ScmDate.Value else DateTime.Now
                resource.Lines.Add(linedata.Line, line)
            else
                resource.Lines.[linedata.Line].Code <- linedata.Code
                resource.Lines.[linedata.Line].Duplicated <- linedata.Duplicated
                if linedata.LineHits.IsSome then
                    resource.Lines.[linedata.Line].IsExecutableLine <- true
                else
                    resource.Lines.[linedata.Line].IsExecutableLine <- false

                resource.Lines.[linedata.Line].LineHits <- if linedata.LineHits.IsSome then linedata.LineHits.Value else 0
                resource.Lines.[linedata.Line].Conditions <- if linedata.Conditions.IsSome then linedata.Conditions.Value else 0
                resource.Lines.[linedata.Line].CoveredConditions <- if linedata.CoveredConditions.IsSome then linedata.CoveredConditions.Value else 0
                if linedata.ScmAuthor.IsSome then resource.Lines.[linedata.Line].ScmAuthor <-linedata.ScmAuthor.Value
                if linedata.ScmDate.IsSome then resource.Lines.[linedata.Line].ScmDate <-linedata.ScmDate.Value
                if linedata.ScmRevision.IsSome then resource.Lines.[linedata.Line].ScmRevision <-linedata.ScmRevision.Value

        let url = sprintf "/api/sources/lines?uuid=%s&from=%i&to=%i" resource.IdString from into
        let responsecontent = httpconnector.HttpSonarGetRequest(conf, url)
        let lines = Lines.Parse(responsecontent)
        lines.Sources |> Seq.iter (fun line -> CreateLineInResource(line))

        if lines.Sources.Length > 0 then
            GetLines(from + 500, into + 500)

    GetLines(1, 500)


let GetLinesFromDateInResource(conf : ISonarConfiguration, resource : Resource, httpconnector : IHttpSonarConnector, date : DateTime) =
    let rec GetLines(from:int, into:int) =
        let CreateLineInResource(linedata:Lines.Sourcis) = 
            if not(resource.Lines.ContainsKey(linedata.Line)) then
                let line = new Line()
                line.Code <- linedata.Code
                line.Id <- linedata.Line
                if linedata.LineHits.IsSome then
                    line.IsExecutableLine <- true
                line.LineHits <- if linedata.LineHits.IsSome then linedata.LineHits.Value else 0
                line.Conditions <- if linedata.Conditions.IsSome then linedata.Conditions.Value else 0
                line.CoveredConditions <- if linedata.CoveredConditions.IsSome then linedata.CoveredConditions.Value else 0
                line.Duplicated <- linedata.Duplicated
                line.ScmAuthor <- if linedata.ScmAuthor.IsSome then linedata.ScmAuthor.Value else ""
                line.ScmRevision <- if linedata.ScmRevision.IsSome then linedata.ScmRevision.Value else ""
                line.ScmDate <- if linedata.ScmDate.IsSome then linedata.ScmDate.Value else DateTime.Now

                if line.IsExecutableLine then
                    if linedata.ScmDate.IsSome then
                        if linedata.ScmDate.Value > date then
                            resource.Lines.Add(linedata.Line, line)
                    else
                        resource.Lines.Add(linedata.Line, line)
            else
                resource.Lines.[linedata.Line].Code <- linedata.Code
                resource.Lines.[linedata.Line].Duplicated <- linedata.Duplicated
                if linedata.LineHits.IsSome then
                    resource.Lines.[linedata.Line].IsExecutableLine <- true
                else
                    resource.Lines.[linedata.Line].IsExecutableLine <- false

                resource.Lines.[linedata.Line].LineHits <- if linedata.LineHits.IsSome then linedata.LineHits.Value else 0
                resource.Lines.[linedata.Line].Conditions <- if linedata.Conditions.IsSome then linedata.Conditions.Value else 0
                resource.Lines.[linedata.Line].CoveredConditions <- if linedata.CoveredConditions.IsSome then linedata.CoveredConditions.Value else 0
                if linedata.ScmAuthor.IsSome then resource.Lines.[linedata.Line].ScmAuthor <-linedata.ScmAuthor.Value
                if linedata.ScmDate.IsSome then resource.Lines.[linedata.Line].ScmDate <-linedata.ScmDate.Value
                if linedata.ScmRevision.IsSome then resource.Lines.[linedata.Line].ScmRevision <-linedata.ScmRevision.Value

                if resource.Lines.[linedata.Line].ScmDate < date || not(resource.Lines.[linedata.Line].IsExecutableLine) then
                    resource.Lines.Remove(linedata.Line) |> ignore

        let url = sprintf "/api/sources/lines?uuid=%s&from=%i&to=%i" resource.IdString from into
        let responsecontent = httpconnector.HttpSonarGetRequest(conf, url)
        let lines = Lines.Parse(responsecontent)
        lines.Sources |> Seq.iter (fun line -> CreateLineInResource(line))

        if lines.Sources.Length > 0 then
            GetLines(from + 500, into + 500)

    GetLines(1, 500)