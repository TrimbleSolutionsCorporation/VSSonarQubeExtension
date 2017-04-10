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

type SourceServiceTest() =
   
    let assemblyRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString()

    [<Test>]
    member test.``Get Source For Resource Resource`` () =
        let conf = ConnectionConfiguration("http://sonar", "admin", "admin", 6.2)

        let url1 = "/api/sources/lines?uuid=id&from=1&to=500"
        let url2 = "/api/sources/lines?uuid=id&from=501&to=1000"

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url1) @>).Returns(SourceService.Lines.GetSample().JsonValue.ToString())
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url2) @>).Returns("{\"sources\":[]}")

        let resource = new Resource( Key = "file", IdString = "id")
        SourceService.UpdateSourceLinesInResource(conf, resource, mockHttpReq.Create())
        Assert.That(resource.Lines.Count, Is.EqualTo(10))
        Assert.That(resource.Lines.[1].LineHits, Is.EqualTo(1))
        Assert.That(resource.Lines.[1].Conditions, Is.EqualTo(2))
        Assert.That(resource.Lines.[1].CoveredConditions, Is.EqualTo(1))
        Assert.That(resource.Lines.[1].IsExecutableLine, Is.EqualTo(true))

        let newData = """ {"sources":[{"line":1,"code":"<span class=\"cd\">// -----------------------------------------------------------------------</span>","duplicated":false},{"line":2,"code":"<span class=\"cd\">// &lt;copyright file=\"CatalogTreeFeature.cs\" company=\"org\"&gt;</span>","scmAuthor":"email@org.com@org.com","scmRevision":"048122991a97ceb117da5e98a30ed3a4a13d37dc","scmDate":"2013-09-10T16:57:00+0300","duplicated":false},{"line":3,"code":"<span class=\"cd\">//   Copyright (c) by org 2013-2014. All rights reserved.</span>","scmAuthor":"email@org.com","scmRevision":"9ceaf377faaff910a9d2d590d4ccf7726e957804","scmDate":"2014-09-02T15:19:01+0300","duplicated":false},{"line":4,"code":"<span class=\"cd\">// &lt;/copyright&gt;</span>","scmAuthor":"email@org.com@org.com","scmRevision":"048122991a97ceb117da5e98a30ed3a4a13d37dc","scmDate":"2013-09-10T16:57:00+0300","duplicated":false},{"line":5,"code":"<span class=\"cd\">// -----------------------------------------------------------------------</span>","scmAuthor":"email@org.com","scmRevision":"9ceaf377faaff910a9d2d590d4ccf7726e957804","scmDate":"2014-09-02T15:19:01+0300","duplicated":false},{"line":6,"code":"<span class=\"k\">namespace</span> CatalogTree","scmAuthor":"email@org.com@org.com","scmRevision":"048122991a97ceb117da5e98a30ed3a4a13d37dc","scmDate":"2013-09-10T16:57:00+0300","duplicated":false},{"line":7,"code":"{","scmAuthor":"email@org.com@org.com","scmRevision":"048122991a97ceb117da5e98a30ed3a4a13d37dc","scmDate":"2013-09-10T16:57:00+0300","duplicated":false},{"line":8,"code":"    <span class=\"k\">using</span> System;","scmAuthor":"email@org.com@org.com","scmRevision":"048122991a97ceb117da5e98a30ed3a4a13d37dc","scmDate":"2013-09-10T16:57:00+0300","duplicated":false},{"line":9,"code":"    <span class=\"k\">using</span> System.Collections.Concurrent;","scmAuthor":"email@org.com@org.com","scmRevision":"65db4135bb751d5a402f9f49183c3821bd55872a","scmDate":"2015-02-25T18:24:41+0200","duplicated":false},{"line":10,"code":"    <span class=\"k\">using</span> System.Collections.Generic;","scmAuthor":"email@org.com@org.com","scmRevision":"da62a8e5e9c608ca50a3641f3bec6b0090de5d55","scmDate":"2014-09-22T19:24:52+0300","duplicated":false}]} """
        let mockHttpReq2 =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url1) @>).Returns(newData)
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), url2) @>).Returns("{\"sources\":[]}")
        SourceService.UpdateSourceLinesInResource(conf, resource, mockHttpReq2.Create())
        Assert.That(resource.Lines.Count, Is.EqualTo(10))
        Assert.That(resource.Lines.[1].LineHits, Is.EqualTo(0))
        Assert.That(resource.Lines.[1].Conditions, Is.EqualTo(0))
        Assert.That(resource.Lines.[1].CoveredConditions, Is.EqualTo(0))
        Assert.That(resource.Lines.[1].IsExecutableLine, Is.EqualTo(false))


