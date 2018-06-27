// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqaleManager.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace SqaleManager

open System
open System.IO
open System.Reflection
open System.Runtime.Serialization.Formatters.Binary
open System.Security
open System.Web
open System.Xml
open System.Text
open System.Xml.Linq
open SonarRestService
open System.ComponentModel 
open VSSonarPlugins
open VSSonarPlugins.Types

type SqaleManager(service : ISonarRestService, conf : ISonarConfiguration) =
    let content = new StringBuilder()
    let importLog = new System.Collections.Generic.List<ImportLogEntry>()
    let SqaleDefaultModelDefinitionPath = "defaultmodel.xml"

    let EncodeStringAsXml(str : string) = SecurityElement.Escape(str).Replace("‘", "&#8216;").Replace("’", "&#8217;").Replace("–", "&#8211;").Replace("—", "&#8212;").Replace("„", "&#8222;").Replace("‟", "&#8223;")

    member x.GetImportLog() = importLog
        
    member x.GetDefaultSqaleModel() = 

        let model = new SqaleModel(service, conf)
        let chars = SqaleDefaultModel.Model
        for char in chars do
            let charInModel = model.CreateAChar(char.Key, char.Name)
            for subchar  in char.Subchars do
                charInModel.CreateSubChar(subchar.Key, subchar.Name)
                
        model

    member x.CreateModelFromRules(rules : System.Collections.Generic.IEnumerable<Rule>) = 
        let model = x.GetDefaultSqaleModel()

        for rule in rules do
            model.GetProfile().AddRule(rule)

        model

    member x.ParseSqaleModelFromXmlFile(file : string) =
        let model = new SqaleModel(service, conf)
        model.LoadSqaleModelFromFile(file)
        model

    member x.GetRepositoriesInModel(model : SqaleModel) =
        let mutable repos : string list = []

        for rule in model.GetProfile().GetAllRules() do
            try
                Array.find (fun elem -> elem.Equals(rule.Repo)) (List.toArray repos) |> ignore
            with
            | :? System.Collections.Generic.KeyNotFoundException -> repos <- repos @ [rule.Repo]

        repos

    member x.WriteProfileToFile(model : SqaleModel, repo : string, fileName : string) =
        let mutable rules : Rule list = []

        for ruleinprofile in model.GetProfile().GetAllRules() do
            if ruleinprofile.Repo.Equals(repo) then
                rules <- rules @ [ruleinprofile]

        x.CreateQualityProfile(fileName, rules)

    member x.CreateQualityProfile(file : string, rules : Rule list) =

        let addLine (line:string) =                  
            use wr = new StreamWriter(file, true)
            wr.WriteLine(line)

        let writeXmlRule(rule : Rule) =
            addLine(sprintf """    <rule key="%s">""" rule.Key)
            addLine(sprintf """        <name><![CDATA[%s]]></name>""" rule.Name)
            addLine(sprintf """        <configKey><![CDATA[%s]]></configKey>""" rule.ConfigKey)
            addLine(sprintf """        <category name="%s" />""" (EnumHelper.getEnumDescription(rule.Category)))
            addLine(sprintf """        <description><![CDATA[  %s  ]]></description>""" rule.HtmlDescription)
            addLine(sprintf """    </rule>""")

        addLine(sprintf """<?xml version="1.0" encoding="ASCII"?>""")
        addLine(sprintf """<rules>""")

        for rule in rules do
            writeXmlRule rule

        addLine(sprintf """</rules>""")       

    member x.AddAProfileFromFileToSqaleModel(repo : string, model : SqaleModel, fileToRead : string) =
        let addLine (line:string, fileToWrite:string) =                  
            use wr = new StreamWriter(fileToWrite, true)
            wr.WriteLine(line)

        try
            let profile = RulesXmlOldType.Parse(File.ReadAllText(fileToRead))

            for rule in profile.Rules do
                let createdRule = new Rule()
                createdRule.EnableSetDeafaults <- false
                createdRule.Repo <- repo
                try
                    createdRule.ConfigKey <- rule.Configkey
                with
                | ex -> ()
                try
                    createdRule.Category <- (EnumHelper.asEnum<Category>(rule.Category.Value)).Value                   
                with
                | ex -> ()
                createdRule.HtmlDescription <- rule.Description
                createdRule.Name <- rule.Name
                createdRule.EnableSetDeafaults <- true
                if rule.Key.StartsWith(repo + ":") then
                    createdRule.Key <- rule.Key
                else
                    createdRule.Key <- createdRule.Repo + ":" + rule.Key

                model.CreateRuleInProfile(createdRule) |> ignore
        with
        | ex ->
            let profile = RulesXmlNewType.Parse(File.ReadAllText(fileToRead))

            for rule in profile.Rules do
                let createdRule = new Rule()
                createdRule.EnableSetDeafaults <- false
                createdRule.Repo <- repo
                createdRule.ConfigKey <- rule.ConfigKey.Replace("![CDATA[", "").Replace("]]", "").Trim()
                try
                    createdRule.Category <- (EnumHelper.asEnum<Category>(rule.Category.Name)).Value
                with
                | ex -> ()
                createdRule.HtmlDescription <- rule.Description.Replace("![CDATA[", "").Replace("]]", "").Trim()
                createdRule.Name <- rule.Name.Replace("![CDATA[", "").Replace("]]", "").Trim()
                if rule.Key.StartsWith(repo + ":") then
                    createdRule.Key <- rule.Key
                else
                    createdRule.Key <- createdRule.Repo + ":" + rule.Key

                createdRule.EnableSetDeafaults <- true
                model.CreateRuleInProfile(createdRule) |> ignore

    member x.WriteCharacteristicsFromScaleModelToFile(model : SqaleModel, fileToWrite : string) =
        content.Clear() |> ignore

        let addLine (line:string, fileToWrite:string) =                  
            content.AppendLine(line) |> ignore

        if File.Exists(fileToWrite) then
            File.Delete(fileToWrite)

        addLine("""<?xml version="1.0"?>""", fileToWrite)
        addLine("""<sqale>""", fileToWrite)
        for char in model.GetCharacteristics() do
            addLine(sprintf """    <chc>""", fileToWrite)
            addLine(sprintf """    <key>%s</key>""" (char.Key.ToString()), fileToWrite)
            addLine(sprintf """    <name>%s</name>""" char.Name, fileToWrite)
            for subchar in char.Subchars do
                addLine(sprintf """        <chc>""", fileToWrite)
                addLine(sprintf """            <key>%s</key>""" (subchar.Key.ToString()), fileToWrite)
                addLine(sprintf """            <name>%s</name>""" subchar.Name, fileToWrite)
                addLine(sprintf """        </chc>""", fileToWrite)

            addLine(sprintf """    </chc>""", fileToWrite)

        addLine("""</sqale>""", fileToWrite)          
        addLine("""""", fileToWrite)

        File.WriteAllText(fileToWrite, content.ToString())
    
    member x.WriteSqaleModelToFile(model : SqaleModel, fileToWrite : string) =

        content.Clear() |> ignore

        let addLine (line:string, fileToWrite:string) =  
            content.AppendLine(line) |> ignore

        if File.Exists(fileToWrite) then
            File.Delete(fileToWrite)

        let writePropToFile(key : string, value : string, txt : string, file : string) = 
            addLine(sprintf """                <prop>""", file)
            addLine(sprintf """                    <key>%s</key>""" key, file)
            if not(String.IsNullOrEmpty(value)) then
                addLine(sprintf """                    <val>%s</val>""" value, file)
            if not(String.IsNullOrEmpty(txt)) then
                addLine(sprintf """                    <txt>%s</txt>""" txt, file)

            addLine(sprintf """                </prop>""", file)

        let writeRulesChcToFile (charName : Category, subcharName : SubCategory, file : string) = 
            for rule in model.GetProfile().GetAllRules() do
                if rule.Category.Equals(charName) && rule.Subcategory.Equals(subcharName) then
                    addLine(sprintf """            <chc>""", fileToWrite)
                    addLine(sprintf """                <rule-repo>%s</rule-repo>""" rule.Repo, file)
                    addLine(sprintf """                <rule-key>%s</rule-key>""" (rule.Key.Split(':').[1]), file)
                    writePropToFile("remediationFunction", "", EnumHelper.getEnumDescription(rule.RemediationFunction), file)
                    writePropToFile("remediationFactor", rule.RemediationFactorVal.ToString().Replace(',', '.'), EnumHelper.getEnumDescription(rule.RemediationFactorTxt), file)

                    if not(rule.RemediationFunction.Equals(RemediationFunction.CONSTANT_ISSUE)) then
                        writePropToFile("offset", rule.RemediationOffsetVal.ToString().Replace(',', '.'), EnumHelper.getEnumDescription(rule.RemediationOffsetTxt), file)

                    addLine(sprintf """            </chc>""", fileToWrite)
           
        addLine("""<?xml version="1.0"?>""", fileToWrite)
        addLine("""<sqale>""", fileToWrite)
        for char in model.GetCharacteristics() do
            addLine(sprintf """    <chc>""", fileToWrite)
            addLine(sprintf """    <key>%s</key>""" (char.Key.ToString()), fileToWrite)
            addLine(sprintf """    <name>%s</name>""" char.Name, fileToWrite)
            for subchar in char.Subchars do
                addLine(sprintf """        <chc>""", fileToWrite)
                addLine(sprintf """            <key>%s</key>""" (subchar.Key.ToString()), fileToWrite)
                addLine(sprintf """            <name>%s</name>""" subchar.Name, fileToWrite)
                writeRulesChcToFile(char.Key, subchar.Key, fileToWrite)
                addLine(sprintf """        </chc>""", fileToWrite)
                

            addLine(sprintf """    </chc>""", fileToWrite)

        addLine("""</sqale>""", fileToWrite)          
        addLine("""""", fileToWrite)      
                
        File.WriteAllText(fileToWrite, content.ToString())               

    member x.SaveSqaleModelToDsk(model : SqaleModel, fileToWrite : string) =
        let WriteToBytes obj = 
            let formatter = new BinaryFormatter()
            use writeStream = new StreamWriter(fileToWrite, true)
            formatter.Serialize(writeStream.BaseStream, obj)
            writeStream.Flush
        WriteToBytes model

    member x.AddProfileDefinition(model : SqaleModel, file : string) = 
        let profile = ProfileDefinition.Parse(File.ReadAllText(file))
        model.language <- profile.Language
        model.profileName <- profile.Name
        for rule in profile.Rules do
            let ruletoUpdate = model.GetProfile().GetRule(rule.Key)
            if ruletoUpdate <> null then
                ruletoUpdate.Severity <- (Enum.Parse(typeof<Severity>, rule.Priority) :?> Severity)
                ruletoUpdate.Repo <- rule.RepositoryKey
            else
                let ruletoUpdate = new Rule()
                ruletoUpdate.Severity <- (Enum.Parse(typeof<Severity>, rule.Priority) :?> Severity)
                ruletoUpdate.Repo <- rule.RepositoryKey
                ruletoUpdate.Key <- rule.Key
                model.GetProfile().AddRule(ruletoUpdate)


    member x.CombineWithDefaultProfileDefinition(model : SqaleModel, file : string) = 
        let profile = ProfileDefinition.Parse(File.ReadAllText(file))
        model.language <- profile.Language
        model.profileName <- profile.Name
        for rule in profile.Rules do
            let ruletoUpdate = model.GetProfile().GetRule(rule.RepositoryKey + ":"+ rule.Key)
            if ruletoUpdate <> null then
                ruletoUpdate.Severity <- (Enum.Parse(typeof<Severity>, rule.Priority) :?> Severity)
                ruletoUpdate.Repo <- rule.RepositoryKey
            
    member x.SaveSqaleModelAsXmlProject(model : SqaleModel, fileToWrite : string) =

        content.Clear() |> ignore
                
        let addLine (line:string, fileToWrite:string) =                  
            content.AppendLine(line) |> ignore

        if File.Exists(fileToWrite) then
            File.Delete(fileToWrite)

        addLine(sprintf """<?xml version="1.0" encoding="ASCII"?>""", fileToWrite)
        addLine(sprintf """<sqaleManager xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="cxx-model-project.xsd">""", fileToWrite)
        //for char in model.GetCharacteristics() do
        //    addLine(sprintf """    <characteristic>""", fileToWrite)
        //    addLine(sprintf """        <key>%s</key>""" char.Key, fileToWrite)
        //    addLine(sprintf """        <name>%s</name>""" char.Name, fileToWrite)            
        //    for subchar in char.GetSubChars do
        //        addLine(sprintf """        <subcaracteristic>""", fileToWrite)
        //        addLine(sprintf """            <key>%s</key>""" subchar.Key, fileToWrite)
        //        addLine(sprintf """            <name>%s</name>""" subchar.Name, fileToWrite)                
         //       addLine(sprintf """        </subcaracteristic>""", fileToWrite)
         //   addLine(sprintf """    </characteristic>""", fileToWrite)
        
        addLine(sprintf """    <rules>""", fileToWrite)
        for rule in model.GetProfile().GetAllRules() do
            addLine(sprintf """    <rule key="%s">""" (rule.Key.Split(':').[1]), fileToWrite)           
            addLine(sprintf """        <name>%s</name>""" (EncodeStringAsXml(rule.Name)), fileToWrite)
            if String.IsNullOrEmpty(EnumHelper.getEnumDescription(rule.Subcategory)) then
                addLine(sprintf """        <requirement>undefined</requirement>""", fileToWrite)
            else
                addLine(sprintf """        <requirement>%s</requirement>""" (EnumHelper.getEnumDescription(rule.Subcategory)), fileToWrite)
                addLine(sprintf """        <remediationFactorVal>%s</remediationFactorVal>""" (rule.RemediationFactorVal.ToString().Replace(',', '.')), fileToWrite)

            if String.IsNullOrEmpty(EnumHelper.getEnumDescription(rule.RemediationFactorTxt)) then
                addLine(sprintf """        <remediationFactorUnit>undefined</remediationFactorUnit>""", fileToWrite)
            else 
                addLine(sprintf """        <remediationFactorUnit>%s</remediationFactorUnit>""" (EnumHelper.getEnumDescription(rule.RemediationFactorTxt)), fileToWrite)

            if String.IsNullOrEmpty(EnumHelper.getEnumDescription(rule.RemediationFunction)) then
                addLine(sprintf """        <remediationFunction>undefined</remediationFunction>""", fileToWrite)
            else
                addLine(sprintf """        <remediationFunction>%s</remediationFunction>""" (EnumHelper.getEnumDescription(rule.RemediationFunction)), fileToWrite)

                addLine(sprintf """        <remediationOffsetVal>%s</remediationOffsetVal>""" (rule.RemediationOffsetVal.ToString().Replace(',', '.')), fileToWrite)

            if String.IsNullOrEmpty(EnumHelper.getEnumDescription(rule.RemediationOffsetTxt)) then
                addLine(sprintf """        <remediationOffsetUnit>undefined</remediationOffsetUnit>""", fileToWrite)
            else
                addLine(sprintf """        <remediationOffsetUnit>%s</remediationOffsetUnit>""" (EnumHelper.getEnumDescription(rule.RemediationOffsetTxt)), fileToWrite)

            if String.IsNullOrEmpty(rule.Severity.ToString()) then
                addLine(sprintf """        <severity>undefined</severity>""", fileToWrite)
            else
                addLine(sprintf """        <severity>%s</severity>""" (EnumHelper.getEnumDescription(rule.Severity)), fileToWrite)

            addLine(sprintf """        <repo>%s</repo>""" rule.Repo, fileToWrite)
            addLine(sprintf """        <description>%s</description>""" (EncodeStringAsXml(rule.HtmlDescription).Trim()), fileToWrite)            
            addLine(sprintf """    </rule>""", fileToWrite)
        addLine(sprintf """    </rules>""", fileToWrite)
        addLine(sprintf """</sqaleManager>""", fileToWrite)

        File.WriteAllText(fileToWrite, content.ToString())

    member x.GetCategoryFromSubcategoryKey(model : SqaleModel, requirement : SubCategory) = 
        let chars = model.GetCharacteristics()
        let mutable key = Category.UNDEFINED
                         
        for char in chars do
            if char.IsSubCharPresent(requirement) then
                key <- char.Key
        key

    member x.ImportSqaleProjectFromFile(fileToRead : string) =

        let model = x.GetDefaultSqaleModel()

        let dskmodel = CxxProjectDefinition.Parse(File.ReadAllText(fileToRead))

        importLog.Clear()
        for item in dskmodel.Rules do
            let entryLog = new ImportLogEntry()
            let info = item.XElement :> IXmlLineInfo
            entryLog.message <- item.XElement.Value
            if info.HasLineInfo() then
                entryLog.line <- info.LineNumber
            try
                let rule = new Rule()
                rule.EnableSetDeafaults <- false
                rule.Key <- item.Repo + ":" + item.Key
                rule.Name <- item.Name
                rule.Repo <- item.Repo
                rule.ConfigKey <- item.Repo + ":" + item.Key
                rule.HtmlDescription <- item.Description
                rule.Category <- x.GetCategoryFromSubcategoryKey(model, EnumHelper.asEnum<SubCategory>(item.Requirement).Value)
                rule.Subcategory <- (EnumHelper.asEnum<SubCategory>(item.Requirement)).Value
                rule.RemediationFactorVal <- Int32.Parse(item.RemediationFactorVal.ToString())
                rule.RemediationFactorTxt <- (EnumHelper.asEnum<RemediationUnit>(item.RemediationFactorUnit)).Value
                rule.RemediationFunction <- (EnumHelper.asEnum<RemediationFunction>(item.RemediationFunction)).Value
                rule.RemediationOffsetTxt <- (EnumHelper.asEnum<RemediationUnit>(item.RemediationOffsetUnit)).Value
                rule.RemediationOffsetVal <- Int32.Parse(item.RemediationOffsetVal.ToString())
                rule.Severity <- (EnumHelper.asEnum<Severity>(item.Severity)).Value
                rule.EnableSetDeafaults <- true
                model.CreateRuleInProfile(rule) |> ignore
            with
             | ex ->
                entryLog.exceptionMessage <- ex.Message
                importLog.Add(entryLog)
        model                 

    member x.AddProfileDefinitionFromServerToModel(model : SqaleModel, language : string, profile : string, conectionConf : ConnectionConfiguration) =
        let service = SonarRestService(new JsonSonarConnector()) 
        let profile = (service :> ISonarRestService).GetEnabledRulesInProfile(conectionConf , language, profile)
        let rules = (service :> ISonarRestService).GetRules(conectionConf , language)

        for rule in profile.[0].GetAllRules() do
            let createdRule = new Rule()
            createdRule.Repo <- rule.Repo            
            createdRule.Key <- rule.Key
            createdRule.Severity <- rule.Severity

            for ruledef in rules do
                if ruledef.Key.EndsWith(rule.Key, true, Globalization.CultureInfo.InvariantCulture) then
                    createdRule.HtmlDescription <- ruledef.HtmlDescription
                    createdRule.Name <- ruledef.Name
                    createdRule.ConfigKey <- ruledef.ConfigKey

            model.CreateRuleInProfile(createdRule) |> ignore

        ()

    member x.MergeSqaleDataModels(sourceModel : SqaleModel, externalModel : SqaleModel) = 
        for rule in externalModel.GetProfile().GetAllRules() do
            if not(rule.Category.Equals("undefined")) then
                let ruleinModel = sourceModel.GetProfile().GetRule(rule.Key)
                if ruleinModel <> null then
                    ruleinModel.Category <- rule.Category
                    ruleinModel.Subcategory <- rule.Subcategory
                    ruleinModel.RemediationFactorTxt <- rule.RemediationFactorTxt
                    ruleinModel.RemediationFactorVal <- rule.RemediationFactorVal
                    ruleinModel.RemediationFunction <- rule.RemediationFunction

    member x.LoadSqaleModelFromDsk(fileToRead : string) =
        let ReadFromBytes(file : string)  = 
            let formatter = new BinaryFormatter()
            use readStream = new StreamReader(file)
            let obj = formatter.Deserialize(readStream.BaseStream)
            unbox obj
        let model:SqaleModel = ReadFromBytes fileToRead
        model

