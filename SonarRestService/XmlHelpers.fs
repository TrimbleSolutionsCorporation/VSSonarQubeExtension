// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlHelpers.fs" company="Trimble Navigation Limited">
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

namespace SonarRestService

open ExtensionTypes
open System.Collections.ObjectModel
open System
open System.Web
open System.IO

type IXmlHelpersService = 
  abstract member GetUserSRCDir : string -> string

type XmlHelpersService() = 
    interface IXmlHelpersService with
        member this.GetUserSRCDir(filepath : string) =
            let helper = UserSettingsXml.Parse(File.ReadAllText(filepath))
            let mutable value = ""
            for data in helper.GetPropertyGroups() do
                try
                    let condition = data.Condition
                    if condition.Value.Contains("'Work'") || condition.Value = "''"  || condition.Value = "" then
                        value <- data.UserSrcdIr
                with
                | ex -> value <- data.UserSrcdIr
            value
