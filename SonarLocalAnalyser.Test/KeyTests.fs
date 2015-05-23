// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyTests.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------
namespace SonarLocalAnalyser.Test

open NUnit.Framework
open SonarLocalAnalyser
open Foq
open System.IO
open VSSonarPlugins

open SonarRestService
open VSSonarPlugins.Types

type KeyTests() =

    [<Test>]
    member test.``Should throw exception when no plugin is found`` () =
        let analyser = new SonarLocalAnalyser(null, Mock<ISonarRestService>().Create(), Mock<IConfigurationHelper>().Create(), Mock<ISonarConfiguration>().Create(), Mock<INotificationManager>().Create())
        Assert.Throws<ResourceNotSupportedException>(fun c -> ((analyser :> ISonarLocalAnalyser).GetResourceKey(new VsFileItem(), true)) |> ignore)

    [<Test>]
    member test.``Should throw exception if No plugins are loaded and we give a good resource`` () =
        let analyser = new SonarLocalAnalyser(null, Mock<ISonarRestService>().Create(), Mock<IConfigurationHelper>().Create(), Mock<ISonarConfiguration>().Create(), Mock<INotificationManager>().Create())
        let project = new Resource()
        project.Lang <- "c++"
        Assert.Throws<ResourceNotSupportedException>(fun c -> ((analyser :> ISonarLocalAnalyser).GetResourceKey(new VsFileItem(), true)) |> ignore)

    [<Test>]
    member test.``Should Return Key Correctly`` () =
        let project = new Resource()
        let vsItem = new VsFileItem(FileName = "fileName", FilePath = "filePath")
        let pluginDescription = new PluginDescription()
        pluginDescription.Name <- "TestPlugin"

        let mockAVsinterface =
            Mock<IConfigurationHelper>()
                .Setup(fun x -> <@ x.ReadSetting(any(), any(), any()) @>).Returns(new SonarQubeProperties(Value = "true"))
                .Create()

        let mockAPlugin =
            Mock<IAnalysisPlugin>()
                .Setup(fun x -> <@ x.IsSupported(vsItem) @>).Returns(true)
                .Setup(fun x -> <@ x.GetResourceKey(any(), any()) @>).Returns("Key")
                .Setup(fun x -> <@ x.GetPluginDescription() @>).Returns(pluginDescription)
                .Create()

        let listofPlugins = new System.Collections.Generic.List<IAnalysisPlugin>()
        listofPlugins.Add(mockAPlugin)
        let analyser = new SonarLocalAnalyser(listofPlugins, Mock<ISonarRestService>().Create(), mockAVsinterface, Mock<ISonarConfiguration>().Create(), Mock<INotificationManager>().Create())
        
        Assert.That((analyser :> ISonarLocalAnalyser).GetResourceKey(vsItem, true), Is.EqualTo("Key"))