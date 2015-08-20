// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportTests.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

open VSSonarPlugins.Types
open SonarRestService

type SupportTests() =

    [<Test>]
    member test.``Should throw exception if no plugins are loaded for multilanguage scenario`` () =
        let project = new Resource()

        let analyser = new SonarLocalAnalyser(null, Mock<ISonarRestService>().Create(), Mock<IConfigurationHelper>().Create(), Mock<INotificationManager>().Create())        
        Assert.Throws<NoPluginInstalledException>(fun c -> (analyser.IsMultiLanguageAnalysis(project)) |> ignore) |> ignore 

    [<Test>]
    member test.``Should throw exception project is not associated for multilanguage scenario`` () =
        let project = new Resource()

        let listofPlugins = new System.Collections.Generic.List<IAnalysisPlugin>()
        listofPlugins.Add(Mock<IAnalysisPlugin>().Create())                
        let analyser = new SonarLocalAnalyser(listofPlugins, Mock<ISonarRestService>().Create(), Mock<IConfigurationHelper>().Create(), Mock<INotificationManager>().Create())        
        Assert.Throws<ProjectNotAssociatedException>(fun c -> (analyser.IsMultiLanguageAnalysis(null)) |> ignore) |> ignore

    [<Test>]
    member test.``Should allow multi language if lang is not defined`` () =
        let project = new Resource()
        let mockConfReq =
            Mock<IConfigurationHelper>()
                .Setup(fun x -> <@ x.ReadSetting(any(), any(), any()) @>).Returns(new SonarQubeProperties(Value = "something"))
                .Create()

        let listofPlugins = new System.Collections.Generic.List<IAnalysisPlugin>()
        listofPlugins.Add(Mock<IAnalysisPlugin>().Create())
        let analyser = new SonarLocalAnalyser(listofPlugins, Mock<ISonarRestService>().Create(), mockConfReq, Mock<INotificationManager>().Create())
        Assert.That((analyser.IsMultiLanguageAnalysis(project)), Is.True)

    [<Test>]
    member test.``Should not allow multi language if lang is defined`` () =
        let project = new Resource(Lang = "c++")

        let mockConfReq =
            Mock<IConfigurationHelper>()
                .Setup(fun x -> <@ x.ReadSetting(any(), any(), any()) @>).Returns(new SonarQubeProperties(Value = "something"))
                .Create()

        let listofPlugins = new System.Collections.Generic.List<IAnalysisPlugin>()
        listofPlugins.Add(Mock<IAnalysisPlugin>().Create())
        let analyser = new SonarLocalAnalyser(listofPlugins, Mock<ISonarRestService>().Create(), mockConfReq, Mock<INotificationManager>().Create())
        Assert.That((analyser.IsMultiLanguageAnalysis(project)), Is.False)

