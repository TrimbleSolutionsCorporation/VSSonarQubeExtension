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

namespace SonarLocalAnalyser.Test

open NUnit.Framework
open FsUnit
open SonarLocalAnalyser
open Foq
open System.IO
open VSSonarPlugins

open SonarRestService
open ExtensionTypes

type KeyTests() =

    [<Test>]
    [<ExpectedException>]
    member test.``Should throw exception when no plugin is found`` () =
        let analyser = new SonarLocalAnalyser(null, Mock<ISonarRestService>().Create(), Mock<IVsEnvironmentHelper>().Create())
        ((analyser :> ISonarLocalAnalyser).GetResourceKey(new VsProjectItem("fileName", "filePath", "projectName", "projectFilePath", "solutionName", "solutionPath"), new Resource(), new ConnectionConfiguration())) |> should throw typeof<ResourceNotSupportedException>

    [<Test>]
    [<ExpectedException>]
    member test.``Should throw exception if No plugins are loaded and we give a good resource`` () =
        let analyser = new SonarLocalAnalyser(null, Mock<ISonarRestService>().Create(), Mock<IVsEnvironmentHelper>().Create())
        let project = new Resource()
        project.Lang <- "c++"
        ((analyser :> ISonarLocalAnalyser).GetResourceKey(new VsProjectItem("fileName", "filePath", "projectName", "projectFilePath", "solutionName", "solutionPath"), new Resource(), new ConnectionConfiguration())) |> should throw typeof<ResourceNotSupportedException>

    [<Test>]
    member test.``Should Return Key Correctly`` () =
        let project = new Resource()
        let vsItem = new VsProjectItem("fileName", "filePath", "projectName", "projectFilePath", "solutionName", "solutionPath")

        let mockAPlugin =
            Mock<IPlugin>()
                .Setup(fun x -> <@ x.IsSupported(vsItem) @>).Returns(true)
                .Setup(fun x -> <@ x.GetResourceKey(any(), any()) @>).Returns("Key")
                .Create()

        let listofPlugins = new System.Collections.Generic.List<IPlugin>()
        listofPlugins.Add(mockAPlugin)
        let analyser = new SonarLocalAnalyser(listofPlugins, Mock<ISonarRestService>().Create(), Mock<IVsEnvironmentHelper>().Create())
        
        (analyser :> ISonarLocalAnalyser).GetResourceKey(vsItem, project, new ConnectionConfiguration()) |> should equal "Key"