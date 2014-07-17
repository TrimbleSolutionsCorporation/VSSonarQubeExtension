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

open ExtensionTypes
open SonarRestService

type SupportTests() =

    [<Test>]
    [<ExpectedException>]
    member test.``Should throw exception if no plugins are loaded for multilanguage scenario`` () =
        let project = new Resource()

        let analyser = new SonarLocalAnalyser(null, Mock<ISonarRestService>().Create(), Mock<IVsEnvironmentHelper>().Create())        
        (analyser.IsMultiLanguageAnalysis(project)) |> should throw typeof<NoPluginInstalledException>

    [<Test>]
    [<ExpectedException>]
    member test.``Should throw exception project is not associated for multilanguage scenario`` () =
        let project = new Resource()

        let listofPlugins = new System.Collections.Generic.List<IAnalysisPlugin>()
        listofPlugins.Add(Mock<IAnalysisPlugin>().Create())                
        let analyser = new SonarLocalAnalyser(listofPlugins, Mock<ISonarRestService>().Create(), Mock<IVsEnvironmentHelper>().Create())        
        (analyser.IsMultiLanguageAnalysis(null)) |> should throw typeof<ProjectNotAssociatedException>

    [<Test>]
    member test.``Should allow multi language if lang is not defined`` () =
        let project = new Resource()

        let listofPlugins = new System.Collections.Generic.List<IAnalysisPlugin>()
        listofPlugins.Add(Mock<IAnalysisPlugin>().Create())
        let analyser = new SonarLocalAnalyser(listofPlugins, Mock<ISonarRestService>().Create(), Mock<IVsEnvironmentHelper>().Create())
        (analyser.IsMultiLanguageAnalysis(project)) |> should be True

    [<Test>]
    member test.``Should not allow multi language if lang is defined`` () =
        let project = new Resource(Lang = "c++")

        let listofPlugins = new System.Collections.Generic.List<IAnalysisPlugin>()
        listofPlugins.Add(Mock<IAnalysisPlugin>().Create())
        let analyser = new SonarLocalAnalyser(listofPlugins, Mock<ISonarRestService>().Create(), Mock<IVsEnvironmentHelper>().Create())
        (analyser.IsMultiLanguageAnalysis(project)) |> should be False

