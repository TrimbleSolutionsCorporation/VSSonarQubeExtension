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
    member test.``Should Throw Exception if No plugins are loaded`` () =
        let analyser = new SonarLocalAnalyser(null, Mock<ISonarRestService>().Create(), Mock<IVsEnvironmentHelper>().Create())
        (analyser.GetPluginThatRunFileAnalysis(new VsProjectItem("fileName", "filePath", "projectName", "projectFilePath", "solutionName", "solutionPath"),new ConnectionConfiguration())) |> should throw typeof<ResourceNotSupportedException>

    [<Test>]
    [<ExpectedException>]
    member test.``Should throw exceptionif No plugins are loaded and we give a good resource`` () =
        let analyser = new SonarLocalAnalyser(null, Mock<ISonarRestService>().Create(), Mock<IVsEnvironmentHelper>().Create())
        (analyser.GetPluginThatRunFileAnalysis(new VsProjectItem("fileName", "filePath", "projectName", "projectFilePath", "solutionName", "solutionPath"), new ConnectionConfiguration())) |> should throw typeof<ResourceNotSupportedException>

    [<Test>]
    [<ExpectedException>]
    member test.``Should throw exception File Local Execution if Plugin is Not Supported`` () =
        let project = new Resource()
        let vsItem = new VsProjectItem("fileName", "filePath", "projectName", "projectFilePath", "solutionName", "solutionPath")

        let mockAPlugin =
            Mock<IAnalysisPlugin>()
                .Setup(fun x -> <@ x.IsSupported(vsItem) @>).Returns(false)
                .Create()
        let listofPlugins = new System.Collections.Generic.List<IAnalysisPlugin>()
        listofPlugins.Add(mockAPlugin)
        let analyser = new SonarLocalAnalyser(listofPlugins, Mock<ISonarRestService>().Create(), Mock<IVsEnvironmentHelper>().Create())
                
        (analyser.GetPluginThatRunFileAnalysis(vsItem, new ConnectionConfiguration())) |> should throw typeof<ResourceNotSupportedException>

    [<Test>]
    member test.``Should Allow File Local Execution if Plugin is Not Supported`` () =
        let project = new Resource()
        let vsItem = new VsProjectItem("fileName", "filePath", "projectName", "projectFilePath", "solutionName", "solutionPath")
        let pluginDescription = new PluginDescription()
        pluginDescription.Name <- "TestPlugin"

        let mockAVsinterface =
            Mock<IVsEnvironmentHelper>()
                .Setup(fun x -> <@ x.ReadOptionFromApplicationData(VSSonarPlugins.GlobalIds.PluginEnabledControlId, "TestPlugin") @>).Returns("True")
                .Create()

        
        let mockAPlugin =
            Mock<IAnalysisPlugin>()
                .Setup(fun x -> <@ x.IsSupported(vsItem) @>).Returns(true)
                .Setup(fun x -> <@ x.GetPluginDescription(mockAVsinterface) @>).Returns(pluginDescription)
                .Create()

        let listofPlugins = new System.Collections.Generic.List<IAnalysisPlugin>()
        listofPlugins.Add(mockAPlugin)
        let analyser = new SonarLocalAnalyser(listofPlugins, Mock<ISonarRestService>().Create(), mockAVsinterface)
                
        (analyser.GetPluginThatRunFileAnalysis(vsItem, new ConnectionConfiguration())) |> should be (sameAs mockAPlugin)


    [<Test>]
    [<ExpectedException>]
    member test.``Should throw exception if No plugins are loaded and we give a good project association`` () =
        let analyser = new SonarLocalAnalyser(null, Mock<ISonarRestService>().Create(), Mock<IVsEnvironmentHelper>().Create())
        let project = new Resource()
        project.Lang <- "c++"
        (analyser.GetPluginThatRunsAnalysisOnSingleLanguageProject(project, new ConnectionConfiguration())) |> should throw typeof<ResourceNotSupportedException>


    [<Test>]
    [<ExpectedException>]
    member test.``Should throw exception in single language if project is Not Supported`` () =
        let project = new Resource()
        project.Lang <- "c++"

        let mockAPlugin =
            Mock<IAnalysisPlugin>()
                .Setup(fun x -> <@ x.IsSupported(any(), project) @>).Returns(false)
                .Create()

        let listofPlugins = new System.Collections.Generic.List<IAnalysisPlugin>()
        listofPlugins.Add(mockAPlugin)
        let analyser = new SonarLocalAnalyser(listofPlugins, Mock<ISonarRestService>().Create(), Mock<IVsEnvironmentHelper>().Create())
                
        (analyser.GetPluginThatRunsAnalysisOnSingleLanguageProject(project, new ConnectionConfiguration())) |> should throw typeof<ResourceNotSupportedException>






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

