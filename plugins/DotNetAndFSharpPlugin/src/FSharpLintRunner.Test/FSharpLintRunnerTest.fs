namespace FSharpLintRunner.Test

open NUnit.Framework
open Foq
open FSharpLintRunner
open VSSonarPlugins
open VSSonarPlugins.Types
open System.IO
open FSharp.Data
open System.Xml.Linq
open System.Reflection

type FSharpLintRunnerTest() =

    let extensionRunningPath = Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")).ToString()
    let item = new VsFileItem(FilePath = Path.Combine(extensionRunningPath, "file.fs"))

    let contentFs = """
namespace FSharpLintRunner

open System.Resources
open System.Reflection
open System.Globalization
open System.Collections
open System.IO

open VSSonarPlugins.Types

open FSharpLint.Framework
open FSharpLint.Framework.Ast
open FSharpLint.Framework.Configuration
open FSharpLint.Application

[<AllowNullLiteralAttribute>]
type FsLintRule(name : string, value : string) =
    member val Rule : string = name with get
    member val Text : string = value with get

type SonarRules() = 

    let fsLintProfile = 
        let resourceManager = new ResourceManager("Text" ,Assembly.Load("FSharpLint.Framework"))
        let set = resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true)
        let mutable rules = List.Empty
        
        for resoure in set do
            let lem = resoure :?> DictionaryEntry
            try
                if (lem.Key :?> string).StartsWith("Rules") ||
                   (lem.Key :?> string).Equals("LintError")  ||
                   (lem.Key :?> string).Equals("LintSourceError") then
                    let rule = new FsLintRule(lem.Key :?> string, lem.Value :?> string)
                    rules <- rules @ [rule]
            with
            | _ -> ()       
        rules
    """

    [<SetUp>]
    member test.``Setup`` () =
        item.Project <- new VsProjectItem()
        item.Project.ProjectFilePath <- extensionRunningPath
        item.SonarResource <- new Resource(Key = "sdadsa")
        File.WriteAllText(item.FilePath, contentFs)

    [<TearDown>]
    member test.``TearDown`` () =
        File.Delete(item.FilePath)
        ()


    [<Test>]
    member test.``Analyses Correctly Code`` () =
        let mockHttpReq =
            Mock<IConfigurationHelper>()
                .Setup(fun x -> <@ x.ReadSetting(any(), any(),"NSQAnalyserPath") @>).Returns(
                    new SonarQubeProperties(Value = extensionRunningPath + "\\SonarLint.dll;" + extensionRunningPath + "\\SonarLint.Extra.dll"))
                .Create()

        let mockNotifier =
            Mock<INotificationManager>()
                .Create()

        let vshelper =
            Mock<IVsEnvironmentHelper>()
                .Create()

        let project = new FSharpLintAnalyser(mockNotifier)

        let profile = new Profile(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())

        let rule = new Rule(ConfigKey = "fsharplint:RulesTypographyFileLengthError" )
        rule.Params.Add(new RuleParam(Key = "Lines", Value = "10" ))
        profile.AddRule(rule)
        let profiles = new System.Collections.Generic.Dictionary<string, Profile>();
        profiles.Add("fs", profile)

        let rule = new Rule(ConfigKey = "fsharplint:RulesSourceLengthError" )
        rule.Params.Add(new RuleParam(Key = "MaxLinesInModule", Value = "1" ))
        rule.Params.Add(new RuleParam(Key = "MaxLinesInProperty", Value = "1" ))
        rule.Params.Add(new RuleParam(Key = "MaxLinesInRecord", Value = "2" ))
        rule.Params.Add(new RuleParam(Key = "MaxLinesInValue", Value = "1" ))
        rule.Params.Add(new RuleParam(Key = "MaxLinesInLambdaFunction", Value = "1" ))
        rule.Params.Add(new RuleParam(Key = "MaxLinesInMatchLambdaFunction", Value = "1" ))
        rule.Params.Add(new RuleParam(Key = "MaxLinesInFunction", Value = "1" ))
        rule.Params.Add(new RuleParam(Key = "MaxLinesInEnum", Value = "1" ))
        rule.Params.Add(new RuleParam(Key = "MaxLinesInClass", Value = "1" ))
        rule.Params.Add(new RuleParam(Key = "MaxLinesInConstructor", Value = "1" ))
        rule.Params.Add(new RuleParam(Key = "MaxLinesInMember", Value = "1" ))
        rule.Params.Add(new RuleParam(Key = "MaxLinesInUnion", Value = "1" ))
        profile.AddRule(rule)
        project.UpdateProfile(profiles)
        let issues = project.RunLint(item)

        Assert.That(issues.Length, Is.EqualTo(4))

        