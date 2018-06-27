// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqaleModel.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
open VSSonarPlugins.Types
open VSSonarPlugins
open SonarRestService

type SqaleModel(service : ISonarRestService, conf : ISonarConfiguration) =
    let profile : Profile = new Profile(service, conf)
    let mutable characteristics : Characteristic list = []

    member val profileName = "" with get, set
    member val language = "" with get, set
    member x.GetCharacteristics() = characteristics
    member x.GetProfile() = profile

    member x.IsCharPresent(key : Category) =
        let CompareKey(char : Characteristic) =
            char.Key.Equals(key)
                                 
        (List.toArray characteristics) |> Array.exists (fun elem ->  CompareKey(elem))

    member x.GetChar(key : Category) =
        let CompareKey(char : Characteristic) =
            char.Key.Equals(key)
                                 
        (List.toArray characteristics) |> Array.find (fun elem ->  CompareKey(elem))

    member x.CreateAChar(key : Category, name : string) =
        if not(x.IsCharPresent(key)) then
            let newChar = new Characteristic(key, name)
            characteristics <- characteristics @ [newChar]
            newChar
        else
            x.GetChar(key)
    
    member x.CreateRuleInProfile(rule : Rule) =
        profile.AddRule(rule)
      
    member x.LoadSqaleModelFromString(str : string) =
        let sqale = SqaleModelType.Parse(str)

        for chk in sqale.Chcs do
            let char = x.CreateAChar(EnumHelper.asEnum<Category>(chk.Key).Value, chk.Name)            

            for subchk in chk.Chcs do
                char.CreateSubChar(EnumHelper.asEnum<SubCategory>(subchk.Key).Value, subchk.Name)

                for chc in subchk.Chcs do
                    let rule = new Rule()
                    rule.Repo <- chc.RuleRepo
                    rule.Key <- chc.RuleKey
                    rule.ConfigKey <- chc.RuleKey + "@" + chc.RuleRepo
                    rule.EnableSetDeafaults <- false

                    for prop in chc.Props do
                        if prop.Key.Equals("remediationFactor") then
                            rule.Category <- (EnumHelper.asEnum<Category>(chk.Key)).Value
                            rule.Subcategory <- (EnumHelper.asEnum<SubCategory>(subchk.Key)).Value
                            try
                                rule.RemediationFactorVal <- Int32.Parse(prop.Val.Value)
                            with
                            | ex -> ()
                            try
                                rule.RemediationFactorTxt <- (EnumHelper.asEnum<RemediationUnit>(prop.Txt)).Value
                            with
                            | ex -> ()

                        if prop.Key.Equals("remediationFunction") then
                            try
                                rule.RemediationFunction <- (EnumHelper.asEnum<RemediationFunction>(prop.Txt)).Value
                            with
                            | ex -> ()

                    rule.EnableSetDeafaults <- true
                    profile.AddRule(rule) |> ignore

    member x.LoadSqaleModelFromFile(path : string) =
        let sqale = SqaleModelType.Parse(File.ReadAllText(path))

        for chk in sqale.Chcs do
            let char = x.CreateAChar(EnumHelper.asEnum<Category>(chk.Key).Value, chk.Name)            

            for subchk in chk.Chcs do
                char.CreateSubChar(EnumHelper.asEnum<SubCategory>(subchk.Key).Value, subchk.Name) |> ignore

                for chc in subchk.Chcs do
                    let rule = new Rule()
                    rule.Repo <- chc.RuleRepo
                    rule.Key <- chc.RuleRepo + ":" + chc.RuleKey
                    rule.ConfigKey <- chc.RuleKey + "@" + chc.RuleRepo
                    rule.EnableSetDeafaults <- false

                    for prop in chc.Props do
                        if prop.Key.Equals("remediationFactor") then
                            rule.Category <- (EnumHelper.asEnum<Category>(chk.Key)).Value
                            rule.Subcategory <- (EnumHelper.asEnum<SubCategory>(subchk.Key)).Value
                            try
                                rule.RemediationFactorVal <- Int32.Parse(prop.Val.Value)
                            with
                            | ex -> ()
                            try
                                rule.RemediationFactorTxt <- (EnumHelper.asEnum<RemediationUnit>(prop.Txt)).Value
                            with
                            | ex -> ()

                        if prop.Key.Equals("remediationFunction") then
                            try
                                rule.RemediationFunction <- (EnumHelper.asEnum<RemediationFunction>(prop.Txt)).Value
                            with
                            | ex -> ()

                        if prop.Key.Equals("offset") then
                            try
                                rule.RemediationOffsetVal <- Int32.Parse(prop.Val.Value)
                            with
                            | ex -> ()
                            try
                                rule.RemediationOffsetTxt <- (EnumHelper.asEnum<RemediationUnit>(prop.Txt)).Value
                            with
                            | ex -> ()

                    rule.EnableSetDeafaults <- true
                    profile.AddRule(rule) |> ignore


    

    





