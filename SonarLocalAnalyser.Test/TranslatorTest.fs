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
open FsUnit
open SonarLocalAnalyser
open Foq
open System.IO
open VSSonarPlugins


open SonarRestService
open VSSonarPlugins.Types
open System.Reflection

type TraTests() =
    
    let assemblyRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString()

    let mockAVsinterface =
        Mock<IVsEnvironmentHelper>()
            .Setup(fun x -> <@ x.GetProperFilePathCapitalization(any()) @>).Returns(Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\FlatProject\\A\\1\\a\\dup.cpp"))
            .Setup(fun x -> <@ x.GetProjectByNameInSolution(any()) @>).Returns(new VsProjectItem(ProjectFilePath = Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\FlatProject\\A\\1\\a\\bla.vcxproj")))
            .Create()

    [<Test>]
    member test.``Should Return Key Correctly When Multi Module`` () =
        let translator = new SQKeyTranslator()
        translator.CreateConfiguration(Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\MultiModuleTest\\sonar-project.properties"));
        translator.Modules().Length |> should equal 2
        translator.GetProjectKey() |> should equal "AB:ProjectX_CPP"
        translator.GetProjectName() |> should equal "ProjectX_CPP"

        let item = new VsFileItem()
        item.FileName <- "dup.cpp"
        item.FilePath <- Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\MultiModuleTest\\A\\1\\a\\dup.cpp")
        item.Project <- new VsProjectItem()
        item.Project.Solution <- new VsSolutionItem()
        item.Project.Solution.SolutionPath <- Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\FlatProject")
        translator.TranslatePath(item, mockAVsinterface) |> should equal "AB:ProjectX_CPP:A:1:a:dup.cpp"

    [<Test>]
    member test.``Should Return Path Correctly When Multi Module`` () =
        let translator = new SQKeyTranslator()
        translator.CreateConfiguration(Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\MultiModuleTest\\sonar-project.properties"));
        translator.Modules().Length |> should equal 2
        translator.GetProjectKey() |> should equal "AB:ProjectX_CPP"
        translator.GetProjectName() |> should equal "ProjectX_CPP"

        translator.TranslateKey("AB:ProjectX_CPP:A:1:a:dup.cpp", mockAVsinterface) |> should equal (Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\MultiModuleTest\\A\\1\\a\\dup.cpp"))

    [<Test>]
    member test.``Should Return Key Correctly With Flat Structure`` () =
        let translator = new SQKeyTranslator()
        translator.CreateConfiguration(Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\FlatProject\\sonar-project.properties"));
        translator.Modules().Length |> should equal 0
        translator.GetProjectKey() |> should equal "AB:ProjectX_CPP_Flat"
        translator.GetProjectName() |> should equal "ProjectX_CPP"

        let item = new VsFileItem()
        item.FileName <- "dup.cpp"
        item.FilePath <- Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\FlatProject\\A\\1\\a\\dup.cpp")
        item.Project <- new VsProjectItem()
        item.Project.Solution <- new VsSolutionItem()
        item.Project.Solution.SolutionPath <- Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\FlatProject")

        translator.TranslatePath(item, mockAVsinterface) |> should equal "AB:ProjectX_CPP_Flat:A/1/a/dup.cpp"

    [<Test>]
    member test.``Should Return Path Correctly With Flat Structure`` () =
        let translator = new SQKeyTranslator()
        translator.CreateConfiguration(Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\FlatProject\\sonar-project.properties"));
        translator.Modules().Length |> should equal 0
        translator.GetProjectKey() |> should equal "AB:ProjectX_CPP_Flat"
        translator.GetProjectName() |> should equal "ProjectX_CPP"

        translator.TranslateKey("AB:ProjectX_CPP_Flat:A/1/a/dup.cpp", mockAVsinterface) |> should equal (Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\FlatProject\\A\\1\\a\\dup.cpp"))


    [<Test>]
    member test.``Should Return Key Correctly With Visual Studio BootStrapper`` () =
        let translator = new SQKeyTranslator()
        translator.CreateConfiguration(Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\VisualBootStrapper\\sonar-project.properties"));
        translator.Modules().Length |> should equal 0
        translator.GetProjectKey() |> should equal "AB:ProjectX_CPP_BootStrapper"
        translator.GetProjectName() |> should equal "ProjectX_CPP"
        let item = new VsFileItem()
        item.FileName <- "dup.cpp"
        item.FilePath <- Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\VisualBootStrapper\\A\\1\\a\\dup.cpp")
        item.Project <- new VsProjectItem()
        item.Project.ProjectFilePath <- Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\VisualBootStrapper\\A\\1\\a\\a1a.vcxproj")
        item.Project.ProjectName <- "a1a"
        item.Project.Solution <- new VsSolutionItem()
        item.Project.Solution.SolutionPath <- Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\VisualBootStrapper")

        translator.TranslatePath(item, mockAVsinterface) |> should equal "AB:ProjectX_CPP_BootStrapper:a1a:dup.cpp"

    [<Test>]
    member test.``Should Return Path Correctly With Visual Studio BootStrapper`` () =
        let translator = new SQKeyTranslator()
        translator.CreateConfiguration(Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\VisualBootStrapper\\sonar-project.properties"));
        translator.Modules().Length |> should equal 0
        translator.GetProjectKey() |> should equal "AB:ProjectX_CPP_BootStrapper"
        translator.GetProjectName() |> should equal "ProjectX_CPP"

        translator.TranslateKey("AB:ProjectX_CPP_BootStrapper:a1a:dup.cpp", mockAVsinterface) |> should equal (Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\FlatProject\\A\\1\\a\\dup.cpp"))


    [<Test>]
    member test.``Should Return Key Correctly With Visual Studio BootStrapper False, but modules defined`` () =
        let translator = new SQKeyTranslator()
        translator.CreateConfiguration(Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\ModulesDefinedAllInOnePropertiesFile\\sonar-project.properties"));
        translator.Modules().Length |> should equal 4
        translator.GetProjectKey() |> should equal "cpp-multimodule-project"
        translator.GetProjectName() |> should equal "cpp-multimodule-project"
        let item = new VsFileItem()
        item.FileName <- "dup.cpp"
        item.FilePath <- Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\ModulesDefinedAllInOnePropertiesFile\\lib\\dup.cpp")
        item.Project <- new VsProjectItem()
        item.Project.ProjectFilePath <- Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\ModulesDefinedAllInOnePropertiesFile\\lib\\lib.vcxproj")
        item.Project.ProjectName <- "a1a"
        item.Project.Solution <- new VsSolutionItem()
        item.Project.Solution.SolutionPath <- Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\ModulesDefinedAllInOnePropertiesFile")

        translator.TranslatePath(item, mockAVsinterface) |> should equal "cpp-multimodule-project:lib:dup.cpp"

    [<Test>]
    member test.``Should Return Path Correctly With Visual Studio BootStrapper False, but modules defined`` () =
        let translator = new SQKeyTranslator()
        translator.CreateConfiguration(Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\ModulesDefinedAllInOnePropertiesFile\\sonar-project.properties"));
        translator.Modules().Length |> should equal 4
        translator.GetProjectKey() |> should equal "cpp-multimodule-project"
        translator.GetProjectName() |> should equal "cpp-multimodule-project"

        translator.TranslateKey("cpp-multimodule-project:lib:dup.cpp", mockAVsinterface) |> should equal (Path.Combine(assemblyRunningPath, "TestData\\SampleProjects\\ModulesDefinedAllInOnePropertiesFile\\lib\\dup.cpp"))