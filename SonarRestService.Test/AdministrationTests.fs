namespace SonarRestService.Test

open NUnit.Framework
open FsUnit
open SonarRestService
open Foq
open System.IO

open VSSonarPlugins.Types

type AdministrationTests() =

    [<Test>]
    member test.``Should Get Users`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/users/search") @>).Returns(File.ReadAllText("testdata/userList.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        let userList = (service :> ISonarRestService).GetUserList(conf)
        userList.Count |> should equal 3
        userList.[0].Active |> should be True
        userList.[0].Email |> should equal ""
        userList.[0].Name |> should equal ""
        userList.[0].Login |> should equal "user1"
        userList.[2].Active |> should be True
        userList.[2].Email |> should equal "real.name@org.com"
        userList.[2].Name |> should equal "Real Name"
        userList.[2].Login |> should equal "user2"

    [<Test>]
    member test.``Should Not Get Users for Server Less than 3.6`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/users/search") @>).Returns(File.ReadAllText("testdata/NonExistentPage.xml"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        let userList = (service :> ISonarRestService).GetUserList(conf)
        userList.Count |> should equal 0

    [<Test>]
    member test.``Should Authenticate User`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/authentication/validate") @>).Returns(""" {"valid":true} """)
                .Create()

        let service = SonarRestService(mockHttpReq)
        (service :> ISonarRestService).AuthenticateUser(conf) |> should be True

    [<Test>]
    member test.``Should Not Authenticate User`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/authentication/validate") @>).Returns(""" {"valid":false} """)
                .Create()

        let service = SonarRestService(mockHttpReq)
        (service :> ISonarRestService).AuthenticateUser(conf) |> should be False

    [<Test>]
    member test.``Should Fail authentication When Sonar less than 3.3 so skip authetication`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/authentication/validate") @>).Returns(File.ReadAllText("testdata/NonExistentPage.xml"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        (service :> ISonarRestService).AuthenticateUser(conf) |> should be False

    [<Test>]
    member test.``Should Get Correct server version with 3.6`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/server") @>).Returns("""{"id":"20130712144608","version":"3.6","status":"UP"}""")
                .Create()

        let service = SonarRestService(mockHttpReq)
        (service :> ISonarRestService).GetServerInfo(conf) |> should equal 3.6

    [<Test>]
    member test.``Should Get Correct server version with 3,6`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/server") @>).Returns("""{"id":"20130712144608","version":"3,6","status":"UP"}""")
                .Create()

        let service = SonarRestService(mockHttpReq)
        (service :> ISonarRestService).GetServerInfo(conf) |> should equal 3.6

    [<Test>]
    member test.``Should Get Correct server version with 3.6-SNAPSHOT`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/server") @>).Returns("""{"id":"20130712144608","version":"3.6-SNAPSHOT","status":"UP"}""")
                .Create()

        let service = SonarRestService(mockHttpReq)
        (service :> ISonarRestService).GetServerInfo(conf) |> should equal 3.6

    [<Test>]
    member test.``Should Get Correct server version with 3,6-SNAPSHOT`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/server") @>).Returns("""{"id":"20130712144608","version":"3,6-SNAPSHOT","status":"UP"}""")
                .Create()

        let service = SonarRestService(mockHttpReq)
        (service :> ISonarRestService).GetServerInfo(conf) |> should equal 3.6

    [<Test>]
    member test.``Should Get Correct server version with 3.6.1`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/server") @>).Returns("""{"id":"20130712144608","version":"3.6.1","status":"UP"}""")
                .Create()

        let service = SonarRestService(mockHttpReq)
        (service :> ISonarRestService).GetServerInfo(conf) |> should equal 3.6

    [<Test>]
    member test.``Should Get Correct server version with 3.6.1-SNAPSHOT`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/server") @>).Returns("""{"id":"20130712144608","version":"3.6.1-SNAPSHOT","status":"UP"}""")
                .Create()

        let service = SonarRestService(mockHttpReq)
        (service :> ISonarRestService).GetServerInfo(conf) |> should equal 3.6

    [<Test>]
    member test.``Get Properties`` () =
        let conf = ConnectionConfiguration("http://sonar", "jocs1", "jocs1")
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/properties") @>).Returns(File.ReadAllText("testdata/PropertiesResponse.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        (service :> ISonarRestService).GetProperties(conf).Count |> should equal 66