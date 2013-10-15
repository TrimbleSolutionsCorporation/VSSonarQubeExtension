// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssuesTests.fs" company="Trimble Navigation Limited">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@trimble.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. 
// You should have received a copy of the GNU General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------

namespace SonarRestService.Test

open NUnit.Framework
open FsUnit
open SonarRestService
open Foq
open System.IO

open ExtensionTypes

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
    member test.``Should Throw Exception When Sonar less than 3.3 so skip authetication`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/authentication/validate") @>).Returns(File.ReadAllText("testdata/NonExistentPage.xml"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        (service :> ISonarRestService).AuthenticateUser(conf) |> should be True

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