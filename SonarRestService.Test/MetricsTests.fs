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
