// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RunAnalysisTests.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

type AnalyserTests() =

    [<Test>]
    member test.``Filters multimodule log message`` () =

        let mockConfReq =
            Mock<IConfigurationHelper>()
                .Setup(fun x -> <@ x.ReadSetting(any(), any(), any()) @>).Returns(new SonarQubeProperties(Value = "something"))
                .Create()

        let analyser = new SonarLocalAnalyser(null, Mock<ISonarRestService>().Create(), mockConfReq, Mock<INotificationManager>().Create(), Mock<IVsEnvironmentHelper>().Create(), "14.0")
        Assert.That(analyser.GetFileToBeAnalysedFromSonarLog("22:09:02.848 DEBUG - Populating index from [moduleKey=Community:cppcheck:cli:Dev, relative=cmdlineparser.cpp, basedir=C:\GitTFS\CppCheck\cli]"), Is.EqualTo(@"C:\GitTFS\CppCheck\cli\cmdlineparser.cpp"))



    [<Test>]
    member test.``Filters normal log message`` () =

        let mockConfReq =
            Mock<IConfigurationHelper>()
                .Setup(fun x -> <@ x.ReadSetting(any(), any(), any()) @>).Returns(new SonarQubeProperties(Value = "something"))
                .Create()

        let analyser = new SonarLocalAnalyser(null, Mock<ISonarRestService>().Create(), mockConfReq, Mock<INotificationManager>().Create(), Mock<IVsEnvironmentHelper>().Create(), "14.0")
        Assert.That(analyser.GetFileToBeAnalysedFromSonarLog("22:09:02.848 DEBUG - Populating index from [abs=C:\GitTFS\CppCheck\cli\cmdlineparser.cpp]"), Is.EqualTo(@"C:\GitTFS\CppCheck\cli\cmdlineparser.cpp"))
        Assert.That(analyser.GetFileToBeAnalysedFromSonarLog("02:23:26.139 DEBUG - Populating index fromoduleKey=projectkey:bla, relative=Interfaces/file.hpp, basedir=E:\prod\project\src]"), Is.EqualTo(@"E:\prod\project\src\Interfaces/file.hpp"))

