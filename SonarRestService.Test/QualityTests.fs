namespace SonarRestService.Test

open NUnit.Framework
open FsUnit
open SonarRestService
open Foq
open System.IO

open VSSonarPlugins.Types

type QualityTests() =

    [<Test>]
    member test.``Should Get A correct Quality Profile`` () =
        let conf = ConnectionConfiguration("host", "user", "password")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), any()) @>).Returns(File.ReadAllText("testdata/profileresponse.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        let profile = (service :> ISonarRestService).GetEnabledRulesInProfile(conf , "language", "profile")
        profile.Count |> should equal 1
        profile.[0].Alerts.Count |> should equal 6
        profile.[0].Rules.Count |> should equal 787

    [<Test>]
    member test.``Should Get Valid Profile from Resource Response`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1")

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), "/api/resources?resource=project&metrics=profile") @>).Returns(File.ReadAllText("testdata/qualityProfileOfResource.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        let resourceinfo = (service :> ISonarRestService).GetQualityProfile(conf, "project")
        resourceinfo.Count |> should equal 1
        resourceinfo.[0].Name |> should equal "bla"
        resourceinfo.[0].Lname |> should equal "bla"
        resourceinfo.[0].Metrics.[0].Key |> should equal "profile"
        resourceinfo.[0].Metrics.[0].Data |> should equal "DefaultC++"

    [<Test>]
    member test.``Should Get Rules From Profile using App`` () =
        let conf = ConnectionConfiguration("http://sonar", "user", "pass")
       
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), any()) @>).Returns(File.ReadAllText("testdata/rulesearchquery.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)

        let profile = Profile()
        profile.Key <- "msbuild-sonar-way-77787"
        profile.Language <- "msbuild"
        (service :> ISonarRestService).GetRulesForProfileUsingRulesApp(conf, profile, true)
        profile.Rules.Count |> should equal 3

    [<Test>]
    member test.``Should Get Rules From Profile`` () =
        let conf = ConnectionConfiguration("http://sonar", "user", "user")
       
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), any()) @>).Returns(File.ReadAllText("testdata/profileusingappid.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)

        let profile = Profile()
        profile.Name <- "Sonar way"
        profile.Language <- "cs"
        (service :> ISonarRestService).GetRulesForProfile(conf, profile)
        profile.Rules.Count |> should equal 199
        profile.Rules.[1].Params.Count |> should equal 1

    [<Test>]
    member test.``Should Search Rule in Profile`` () =
        let conf = ConnectionConfiguration("http://sonar", "user", "pass")
       
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), any()) @>).Returns(File.ReadAllText("testdata/rulesearchanswer.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)

        let profile = Profile()
        profile.Key <- "msbuild-sonar-way-77787"
        profile.Language <- "msbuild"
        let rule = (service :> ISonarRestService).GetRuleUsingProfileAppId(conf, "csharpsquid:S108")
        rule.ConfigKey |> should equal "csharpsquid:S108"
