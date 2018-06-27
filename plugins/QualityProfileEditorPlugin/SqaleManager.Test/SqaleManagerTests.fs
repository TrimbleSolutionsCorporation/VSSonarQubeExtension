// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqaleManagerTests.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace SqaleManager.Test

open NUnit.Framework
open SqaleManager
open Foq
open FSharp.Data
open System.Xml.Linq
open System.IO
open VSSonarPlugins
open VSSonarPlugins.Types


type RootConfigurationPropsChecksTests() = 
    let executingPath = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")).ToString()
    let rulesinFile = Path.Combine(executingPath, "fxcop-profile.xml")

    [<SetUp>]
    member test.``SetUp`` () = 
        if File.Exists(rulesinFile) then
            File.Delete(rulesinFile)

    [<TearDown>]
    member test.``tearDown`` () = 
        if File.Exists(rulesinFile) then
            File.Delete(rulesinFile)
            
    [<Test>]
    member test.``It Creates a Default Model`` () = 
        let manager = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let def = manager.GetDefaultSqaleModel()
        Assert.That(def.GetCharacteristics().Length, Is.EqualTo(8))
        Assert.That(def.GetProfile().GetAllRules().Count, Is.EqualTo(0))

    [<Test>]
    member test.``Should Load Profile into Model With New Format`` () = 
        let manager = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let def = manager.GetDefaultSqaleModel()
        manager.AddAProfileFromFileToSqaleModel("intel", def, Path.Combine(executingPath,"samples/intel-profile.xml"))
        let rules = def.GetProfile().GetAllRules()
        Assert.That(rules.Count, Is.EqualTo(22))
        Assert.That(rules.[2].Key, Is.EqualTo("intel:intelXe.CrossThreadStackAccess"))
        Assert.That(rules.[2].Name, Is.EqualTo("Cross-thread Stack Access"))
        Assert.That(rules.[2].Repo, Is.EqualTo("intel"))
        Assert.That(rules.[2].Category, Is.EqualTo(Category.RELIABILITY))
        Assert.That(rules.[2].ConfigKey, Is.EqualTo("intelXe.CrossThreadStackAccess@INTEL"))
        Assert.That(rules.[2].HtmlDescription, Is.EqualTo("Occurs when a thread accesses a different thread's stack."))

    [<Test>]
    member test.``Should Load Profile into Model With Old Format`` () = 
        let manager = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let def = manager.GetDefaultSqaleModel()
        manager.AddAProfileFromFileToSqaleModel("cppcheck", def, Path.Combine(executingPath,"samples/cppcheck.xml"))
        Assert.That(def.GetProfile().GetAllRules().Count, Is.EqualTo(305))

    [<Test>]
    member test.``Should Load Model From CSharp Xml File`` () = 
        let manager = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let model = manager.ParseSqaleModelFromXmlFile(Path.Combine(executingPath,"samples/CSharpSqaleModel.xml"))
        Assert.That(model.GetCharacteristics().Length, Is.EqualTo(8))
        let rules = model.GetProfile().GetAllRules()
        Assert.That(rules.Count, Is.EqualTo(617))
        Assert.That(rules.[0].Category, Is.EqualTo(Category.PORTABILITY))
        Assert.That(rules.[0].Subcategory, Is.EqualTo(SubCategory.COMPILER_RELATED_PORTABILITY))
        Assert.That(rules.[0].Repo, Is.EqualTo("common-c++"))
        Assert.That(rules.[0].Key, Is.EqualTo("common-c++:InsufficientBranchCoverage"))
        Assert.That(rules.[0].RemediationFunction, Is.EqualTo(RemediationFunction.LINEAR))
        Assert.That(rules.[0].RemediationFactorTxt, Is.EqualTo(RemediationUnit.D))
        Assert.That(rules.[0].RemediationFactorVal, Is.EqualTo(0))


    [<Test>]
    member test.``Should Get Correct Number Of Repositories From Model`` () = 
        let manager = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let model = manager.ParseSqaleModelFromXmlFile(Path.Combine(executingPath,"samples/CSharpSqaleModel.xml"))
        Assert.That(manager.GetRepositoriesInModel(model).Length, Is.EqualTo(6))
    
    [<Test>]
    member test.``Should Create Xml Profile`` () = 
        let manager = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let model = manager.ParseSqaleModelFromXmlFile(Path.Combine(executingPath,"samples/CSharpSqaleModel.xml"))

        manager.WriteProfileToFile(model, "fxcop", rulesinFile)
        let managerNew = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let newModel = managerNew.GetDefaultSqaleModel()
        managerNew.AddAProfileFromFileToSqaleModel("fxcop", newModel, rulesinFile)
        let newrules = model.GetProfile().GetAllRules()
        Assert.That(newrules.Count, Is.EqualTo(617))

    [<Test>]
    member test.``Should Create Write A Sqale Model To Xml Correctly And Read It`` () = 
        let manager = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let def = manager.GetDefaultSqaleModel()

        let rule = new Rule()
        rule.Key <- "Example:RuleKey"
        rule.Name <- "Rule Name"
        rule.ConfigKey <- "Rule Name@Example"
        rule.HtmlDescription <- "this is description"
        rule.Category <- Category.MAINTAINABILITY
        rule.Subcategory <- SubCategory.READABILITY
        rule.RemediationFactorVal <- 10
        rule.RemediationFactorTxt <- RemediationUnit.MN
        rule.RemediationFunction <- RemediationFunction.LINEAR
        rule.Severity <- Severity.MINOR
        rule.Repo <- "Example"
        
        def.CreateRuleInProfile(rule) |> ignore
        manager.WriteSqaleModelToFile(def, rulesinFile)

        let model = manager.ParseSqaleModelFromXmlFile(rulesinFile)
        let rules = model.GetProfile().GetAllRules()
        Assert.That(rules.Count, Is.EqualTo(1))
        Assert.That(rules.[0].Key, Is.EqualTo("Example:RuleKey"))
        Assert.That(rules.[0].ConfigKey, Is.EqualTo("RuleKey@Example"))
        Assert.That(rules.[0].Category, Is.EqualTo(Category.MAINTAINABILITY))
        Assert.That(rules.[0].Subcategory, Is.EqualTo(SubCategory.READABILITY))
        Assert.That(rules.[0].RemediationFactorVal, Is.EqualTo(10))
        Assert.That(rules.[0].RemediationFactorTxt, Is.EqualTo(RemediationUnit.MN))
        Assert.That(rules.[0].RemediationFunction, Is.EqualTo(RemediationFunction.LINEAR))
        Assert.That(rules.[0].Severity, Is.EqualTo(Severity.UNDEFINED))
        Assert.That(rules.[0].Repo, Is.EqualTo("Example"))

    //[<Test>]
    member test.``Should Serialize the model Correctly And Read It`` () = 
        let manager = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let def = manager.GetDefaultSqaleModel()

        let rule = new Rule()
        rule.Key <- "RuleKey"
        rule.Name <- "Rule Name"
        rule.ConfigKey <- "Rule Name@Example"
        rule.HtmlDescription <- "this is description"
        rule.Category <- Category.MAINTAINABILITY
        rule.Subcategory <- SubCategory.READABILITY
        rule.RemediationFactorVal <- 10
        rule.RemediationFactorTxt <- RemediationUnit.MN
        rule.RemediationFunction <- RemediationFunction.LINEAR
        rule.Severity <- Severity.MINOR
        rule.Repo <- "Example"
        
        def.CreateRuleInProfile(rule) |> ignore
        manager.SaveSqaleModelToDsk(def, rulesinFile) |> ignore

        let model = manager.LoadSqaleModelFromDsk(rulesinFile)
        let rules = model.GetProfile().GetAllRules()
        Assert.That(rules.Count, Is.EqualTo(1))
        Assert.That(rules.[0].Key, Is.EqualTo("RuleKey"))
        Assert.That(rules.[0].Name, Is.EqualTo("Rule Name"))
        Assert.That(rules.[0].ConfigKey, Is.EqualTo("Rule Name@Example"))
        Assert.That(rules.[0].HtmlDescription, Is.EqualTo("this is description"))
        Assert.That(rules.[0].Category, Is.EqualTo(Category.MAINTAINABILITY))
        Assert.That(rules.[0].Subcategory, Is.EqualTo(SubCategory.READABILITY))
        Assert.That(rules.[0].RemediationFactorVal, Is.EqualTo(10))
        Assert.That(rules.[0].RemediationFactorTxt, Is.EqualTo(RemediationUnit.MN))
        Assert.That(rules.[0].RemediationFunction, Is.EqualTo(RemediationFunction.LINEAR))
        Assert.That(rules.[0].Severity, Is.EqualTo(Severity.MINOR))
        Assert.That(rules.[0].Repo, Is.EqualTo("Example"))

    [<Test>]
    member test.``Read A ProfileDefinition`` () = 
        let manager = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let model = manager.GetDefaultSqaleModel()
        manager.AddAProfileFromFileToSqaleModel("cppcheck", model, Path.Combine(executingPath,"samples/cppcheck.xml"))
        manager.CombineWithDefaultProfileDefinition(model, Path.Combine(executingPath,"samples/default-profile.xml"))
        let rules = model.GetProfile().GetAllRules()
        Assert.That(rules.Count, Is.EqualTo(305))
        Assert.That(rules.[0].Severity, Is.EqualTo(Severity.UNDEFINED))
        

    [<Test>]
    member test.``Should Save Model As XML`` () = 
        let manager = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let model = manager.GetDefaultSqaleModel()
        manager.AddAProfileFromFileToSqaleModel("cppcheck", model, Path.Combine(executingPath,"samples/cppcheck.xml"))
        manager.AddAProfileFromFileToSqaleModel("pclint", model, Path.Combine(executingPath,"samples/pclint.xml"))
        manager.AddAProfileFromFileToSqaleModel("rats", model, Path.Combine(executingPath,"samples/rats.xml"))
        manager.AddAProfileFromFileToSqaleModel("vera++", model, Path.Combine(executingPath,"samples/vera++.xml"))
        manager.AddAProfileFromFileToSqaleModel("valgrind", model, Path.Combine(executingPath,"samples/valgrind.xml"))

        manager.AddAProfileFromFileToSqaleModel("compiler", model, Path.Combine(executingPath,"samples/compiler.xml"))
        manager.CombineWithDefaultProfileDefinition(model, Path.Combine(executingPath,"samples/default-profile.xml"))

        manager.SaveSqaleModelAsXmlProject(model, Path.Combine(executingPath,"cxx-model-project.xml"))
        manager.WriteSqaleModelToFile(model, Path.Combine(executingPath,"cxx-model.xml"))

    //[<Test>]
    member test.``Read Cxx Project`` () = 
        let manager = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let model = manager.ImportSqaleProjectFromFile(Path.Combine(executingPath,"cxx-model-project.xml"))
        Assert.That(model.GetCharacteristics().Length, Is.EqualTo(8))
        manager.WriteSqaleModelToFile(model, Path.Combine(executingPath,"cxx-model.xml"))

    //[<Test>]
    member test.``Get C++ Profile`` () = 
        let manager = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let model = manager.GetDefaultSqaleModel()
        manager.AddProfileDefinitionFromServerToModel(model, "c++", "DefaultTeklaC++", new ConnectionConfiguration("http://sonar", "jocs1", "jocs1", 4.5))
        manager.SaveSqaleModelAsXmlProject(model, "cxx-model-project-updated.xml")
        ()

    //[<Test>]
    member test.``Read A Project and Merge Info From Another Project`` () = 
        let manager = new SqaleManager(Mock<ISonarRestService>().Create(), Mock<ISonarConfiguration>().Create())
        let model = manager.ImportSqaleProjectFromFile("cxx-model-project.xml")
        let modelToMerge = manager.ImportSqaleProjectFromFile("cppcheck-model-project.xml")
        manager.MergeSqaleDataModels(model, modelToMerge)
        manager.WriteSqaleModelToFile(model, "cxx-model-combined.xml")
        manager.SaveSqaleModelAsXmlProject(model, "cxx-model-project-combined.xml")
        ()
