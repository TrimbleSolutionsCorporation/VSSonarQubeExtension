namespace SonarRestService.Test

open NUnit.Framework
open FsUnit
open SonarRestService
open Foq
open System.IO
open System.Web
open System

open VSSonarPlugins
open VSSonarPlugins.Types
open RestSharp
open System.Collections.ObjectModel

type ResourceTests() =

    [<Test>]
    member test.``Should Get Resource Info Properly`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/resources?resource=filename") @>).Returns(File.ReadAllText("testdata/resourceresponse.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        let resourceinfo = (service :> ISonarRestService).GetResourcesData(conf , "filename")
        resourceinfo.Count |> should equal 2
        resourceinfo.[0].Id |> should equal 73978
        resourceinfo.[1].Id |> should equal 1
        resourceinfo.[0].Version |> should equal "work"
        resourceinfo.[1].Version |> should equal "0.1"
        resourceinfo.[0].Name |> should equal "Features"
        resourceinfo.[0].Lname |> should equal "Features"
        resourceinfo.[0].Scope |> should equal "PRJ"
        resourceinfo.[0].Qualifier |> should equal "TRK"
        resourceinfo.[0].Lang |> should equal "cs"
        resourceinfo.[0].Key |> should equal "organization.projectid:Features"
        resourceinfo.[1].Metrics.Count |> should equal 3

    [<Test>]
    member test.``Should Get Valid List of Resources for project List`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/resources") @>).Returns(File.ReadAllText("testdata/listofprojects.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        let resourceinfo = (service :> ISonarRestService).GetProjectsList(conf)
        resourceinfo.Count |> should equal 2
        resourceinfo.[0].Key |> should equal "organization.projectid:Features"
        resourceinfo.[1].Key |> should equal "organization.projectid2:eql"

    [<Test>]
    member test.``Should Get Valid List of Coverage`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/resources?resource=resource&metrics=coverage_line_hits_data,conditions_by_line,covered_conditions_by_line") @>).Returns(File.ReadAllText("testdata/linecoveragesample.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        let resourceinfo = (service :> ISonarRestService).GetCoverageInResource(conf, "resource")
        resourceinfo.BranchHits.Count |> should equal 113

    [<Test>]
    member test.``Should Get Valid Source Data`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/sources?resource=resource") @>).Returns(File.ReadAllText("testdata/sourceres.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        let resourceinfo = (service :> ISonarRestService).GetSourceForFileResource(conf, "resource")
        resourceinfo.Lines.Length |> should equal 20
        resourceinfo.Lines.[0] |> should equal "/**"
        resourceinfo.Lines.[1] |> should equal "    @file       ssds.cpp"

    [<Test>]
    member test.``Should PutComment Correctly On Sonar Less than 3.6 Review Attached`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockIRestReponseHttpReq =
            Mock<IRestResponse>()
                .Setup(fun x -> <@ x.StatusCode @>).Returns(Net.HttpStatusCode.OK)
                .Setup(fun x -> <@ x.Content @>).Returns(File.ReadAllText("testdata/ResponseToAddCommentOldFormat.txt"))
                .Create()

        let mockIRestReponsePostHttpReq =
            Mock<IRestResponse>()
                .Setup(fun x -> <@ x.StatusCode @>).Returns(Net.HttpStatusCode.NotFound)
                .Create()

        let mockHttpReq =
            Mock<IHttpSonarConnector>()            
                .Setup(fun x -> <@ x.HttpSonarPostRequest(any(), "/api/issues/add_comment", any()) @>).Returns(mockIRestReponsePostHttpReq)
                .Setup(fun x -> <@ x.HttpSonarPutRequest(any(), "/api/reviews/add_comment", any()) @>).Returns(mockIRestReponseHttpReq)
                .Create()

        let service = SonarRestService(mockHttpReq)
        let issueList = new System.Collections.Generic.List<Issue>()
        let Comments = new System.Collections.Generic.List<Comment>()
        Comments.Add(new Comment())
        issueList.Add(new Issue(Id = 343, Comments = Comments))

        issueList.[0].Comments.Count |> should equal 1
        let status = (service :> ISonarRestService).CommentOnIssues(conf, issueList, "comment")
        let data = status.Item("343")
        data |> should equal Net.HttpStatusCode.OK
        issueList.[0].Comments.Count |> should equal 2

    [<Test>]
    member test.``Should PutComment Correctly On Sonar Higher than 3.6`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockIRestReponsePostHttpReq =
            Mock<IRestResponse>()
                .Setup(fun x -> <@ x.StatusCode @>).Returns(Net.HttpStatusCode.OK)
                .Setup(fun x -> <@ x.Content @>).Returns(File.ReadAllText("testdata/commentsReply.txt"))
                .Create()

        let mockHttpReq =
            Mock<IHttpSonarConnector>()            
                .Setup(fun x -> <@ x.HttpSonarPostRequest(any(), "/api/issues/add_comment", any()) @>).Returns(mockIRestReponsePostHttpReq)
                .Create()

        let service = SonarRestService(mockHttpReq)
        let issueList = new System.Collections.Generic.List<Issue>()
        let Comments = new System.Collections.Generic.List<Comment>()
        Comments.Add(new Comment())
        issueList.Add(new Issue(Id = 343, Comments = Comments))

        issueList.[0].Comments.Count |> should equal 1
        let status = (service :> ISonarRestService).CommentOnIssues(conf, issueList, "comment")
        let data = status.Item((new Guid()).ToString())
        data |> should equal Net.HttpStatusCode.OK
        issueList.[0].Comments.Count |> should equal 2

    [<Test>]
    member test.``Should PutComment Correctly On Sonar Less than 3.6 No Review Attached`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockIRestReponseHttpReq =
            Mock<IRestResponse>()
                .Setup(fun x -> <@ x.StatusCode @>).Returns(Net.HttpStatusCode.OK)
                .Setup(fun x -> <@ x.Content @>).Returns(File.ReadAllText("testdata/ResponseToAddCommentOldFormat.txt"))
                .Create()

        let mockIRestReponsePostHttpReq =
            Mock<IRestResponse>()
                .Setup(fun x -> <@ x.StatusCode @>).Returns(Net.HttpStatusCode.NotFound)
                .Create()

        let mockHttpReq =
            Mock<IHttpSonarConnector>()            
                .Setup(fun x -> <@ x.HttpSonarPostRequest(any(), "/api/issues/add_comment", any()) @>).Returns(mockIRestReponsePostHttpReq)
                .Setup(fun x -> <@ x.HttpSonarRequest(any(), "/api/reviews?violation_id=343&status=OPEN&comment=comment", any()) @>).Returns(mockIRestReponseHttpReq)
                .Create()

        let service = SonarRestService(mockHttpReq)
        let issueList = new System.Collections.Generic.List<Issue>()
        issueList.Add(new Issue(Id = 343))

        issueList.[0].Comments.Count |> should equal 0
        let status = (service :> ISonarRestService).CommentOnIssues(conf, issueList, "comment")
        let data = status.Item("343")
        data |> should equal Net.HttpStatusCode.OK
        issueList.[0].Comments.Count |> should equal 1

    //[<Test>]
    member test.``GetTypes`` () =
        let conf = ConnectionConfiguration("http://sonar:80", "jocs1", "jocs1")

        let jsonHttp = new JsonSonarConnector()
        let url = "/api/violations?resource=tekla.structures.core:Common:libgeometry/vector_utilities.cpp"
        let data = (jsonHttp :> IHttpSonarConnector).HttpSonarGetRequest(conf, url)
        ()

    //[<Test>]
    member test.``PutTypes`` () =
        let conf = ConnectionConfiguration("http://sonar:80", "jocs1", "jocs1")

        let jsonHttp = new JsonSonarConnector()
        let parameters = Map.empty.Add("id", Convert.ToString(812)).Add("comment", "commenttest")
        //let response = (jsonHttp :> IHttpSonarConnector).HttpSonarPutRequest(conf, "/api/reviews/add_comment", parameters);

        ()