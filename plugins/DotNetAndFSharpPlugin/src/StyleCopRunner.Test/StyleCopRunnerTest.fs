namespace NSonarQubeRunner.Test

open NUnit.Framework
open Foq
open StyleCopRunner
open VSSonarPlugins
open VSSonarPlugins.Types
open System.IO
open FSharp.Data
open System.Xml.Linq
open System.Reflection

type NQubeRunnerTest() =

    let extensionRunningPath1 = Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")).ToString()
    let extensionRunningPath = @"D:\Development\stylecop\Project\BuildDrop\Debug"

    [<Test>]
    member test.``Detects Issues Using StyleCop`` () =
        let mockHttpReq =
            Mock<IConfigurationHelper>()
                .Setup(fun x -> <@ x.ReadSetting(any(), any(),"StyleCopPath") @>).Returns(
                    new SonarQubeProperties(Value = extensionRunningPath))
                .Create()

        let mockNotifier =
            Mock<INotificationManager>()
                .Create()

        let project = new StyleCopRunner(mockHttpReq, mockNotifier)
        let item = new VsFileItem(FilePath = extensionRunningPath1 + "\\SonarQubeViewModel.cs")
        item.FileName <- "SonarQubeViewModel.cs"
        item.Project <- new VsProjectItem()
        item.Project.ProjectFilePath <- Path.Combine(extensionRunningPath1, "project.csproj")
        item.SonarResource <- new Resource(Key = "sdadsa")
        let profile = new Profile(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        profile.AddRule(new Rule(ConfigKey = "stylecop:ElementsMustBeDocumented" ))
        profile.AddRule(new Rule(ConfigKey = "stylecop:DoNotUseRegions" ))
        let profiles = new System.Collections.Generic.Dictionary<string, Profile>()
        profiles.Add("cs", profile)
        project.UpdateSettings(profiles)
        let issues = project.RunAnalysis(item, mockHttpReq)

        Assert.That(issues.Count, Is.EqualTo(8))

        