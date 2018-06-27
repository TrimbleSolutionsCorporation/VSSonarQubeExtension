namespace SonarRestService.Test

open NUnit.Framework
open SonarRestService
open Foq
open System
open System.IO
open System.Net
open System.Web
open System.Linq

open VSSonarPlugins
open VSSonarPlugins.Types
open System.Reflection
open RestSharp

type SettingsServiceTest() =
   
    let assemblyRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString()

    [<Test>]
    member test.``Get Settings per Resource`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "admin", "admin", 6.3)
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), any()) @>).Returns(File.ReadAllText(assemblyRunningPath + "/testdata/SettingsResponse.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        let settings = (service :> ISonarRestService).GetSettings(conf, new Resource( Key = "Tekla.Tools.RoslynRunner")).ToList()
        Assert.That(settings.Count, Is.EqualTo(4))
        Assert.That(settings.[1].Values.Count, Is.EqualTo(3))
        Assert.That(settings.[3].FieldValues.Count, Is.EqualTo(2))

    [<Test>]
    member test.``Set Settings per Resource Value`` () =
        let conf = ConnectionConfiguration("http://localhost", "admin", "admin", 6.3)

        let mockRestResponse =
            Mock<IRestResponse>()
                .Setup(fun x -> <@ x.StatusCode @>).Returns(Net.HttpStatusCode.NoContent)
                .Create()

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarPostRequest(any(), any(), any()) @>).Returns(mockRestResponse)
                .Create()

        let setting = new Setting()
        setting.key <- "sonar.python.xunit.skipDetails"
        setting.Value <- "true"
        let service = SonarRestService(mockHttpReq)
        let reply = (service :> ISonarRestService).SetSetting(conf, setting, new Resource( Key = "Trimble.Connect.Desktop:feature_test-sonar-6.3"))
        Assert.That(reply, Is.EqualTo(""))

    [<Test>]
    member test.``Set Settings per Resource Values`` () =
        let conf = ConnectionConfiguration("http://localhost", "admin", "admin", 6.3)

        let mockRestResponse =
            Mock<IRestResponse>()
                .Setup(fun x -> <@ x.StatusCode @>).Returns(Net.HttpStatusCode.NoContent)
                .Create()

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarPostRequest(any(), any(), any()) @>).Returns(mockRestResponse)
                .Create()

        let setting = new Setting()
        setting.key <- "sonar.cpd.exclusions"
        setting.Values.Add("**/*.csproj")
        setting.Values.Add("**/*.vcxproj")
        setting.Values.Add("**/*.fsproj")
        let service = SonarRestService(mockHttpReq)
        let reply = (service :> ISonarRestService).SetSetting(conf, setting, new Resource( Key = "Trimble.Connect.Desktop:feature_test-sonar-6.3"))
        Assert.That(reply, Is.EqualTo(""))

            
    [<Test>]
    member test.``Set Settings per Resource FieldValues`` () =
        let conf = ConnectionConfiguration("http://localhost", "admin", "admin", 6.3)

        let mockRestResponse =
            Mock<IRestResponse>()
                .Setup(fun x -> <@ x.StatusCode @>).Returns(Net.HttpStatusCode.NoContent)
                .Create()

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarPostRequest(any(), any(), any()) @>).Returns(mockRestResponse)
                .Create()

        let setting = new Setting()
        setting.key <- "sonar.issue.enforce.multicriteria"
        let fieldValue = new FieldValue()
        fieldValue.Values.Add("resourceKey", "**/test/*.cpp")
        fieldValue.Values.Add("ruleKey", "cxx:MethodName1")
        setting.FieldValues.Add(fieldValue)
        let fieldValue = new FieldValue()
        fieldValue.Values.Add("resourceKey", "**/test/*.cpp")
        fieldValue.Values.Add("ruleKey", "cxx:ClassName1")
        setting.FieldValues.Add(fieldValue)
        let service = SonarRestService(mockHttpReq)
        let reply = (service :> ISonarRestService).SetSetting(conf, setting, new Resource( Key = "Trimble.Connect.Desktop:feature_test-sonar-6.3"))
        Assert.That(reply, Is.EqualTo(""))


    [<Test>]
    member test.``Get Properties per Resource`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "admin", "admin", 5.3)
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), any()) @>).Returns(File.ReadAllText(assemblyRunningPath + "/testdata/PropertiesResponse.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        Assert.That((service :> ISonarRestService).GetProperties(conf, new Resource( Key = "Tekla.Tools.RoslynRunner")).Count, Is.EqualTo(66))


    [<Test>]
    member test.``Get All Properties`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "admin", "admin", 5.3)
        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarGetRequest(any(), any()) @>).Returns(File.ReadAllText(assemblyRunningPath + "/testdata/PropertiesResponse.txt"))
                .Create()

        let service = SonarRestService(mockHttpReq)
        Assert.That((service :> ISonarRestService).GetProperties(conf).Count, Is.EqualTo(66))

    [<Test>]
    member test.``Update a property`` () =
        let conf = ConnectionConfiguration("http://localhost:9000", "admin", "admin", 5.3)
        let response = new RestSharp.RestResponse()
        response.StatusCode <- HttpStatusCode.OK

        let mockHttpReq =
            Mock<IHttpSonarConnector>()
                .Setup(fun x -> <@ x.HttpSonarPostRequest(any(), any(), any()) @>).Returns(response)
                .Create()

        let service = SonarRestService(mockHttpReq)
        Assert.That((service :> ISonarRestService).UpdateProperty(conf, "sonar.roslyn.diagnostic.path", """c:\abc\dll.dll""", null), Is.EqualTo(""))
