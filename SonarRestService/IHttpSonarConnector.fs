// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHttpSonarConnector.fs" company="Trimble Navigation Limited">
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
open RestSharp

type IHttpSonarConnector = 
  abstract member HttpSonarGetRequest : ConnectionConfiguration * string -> string
  abstract member HttpSonarRequest : ConnectionConfiguration * string * Method -> IRestResponse
  abstract member HttpSonarPutRequest : ConnectionConfiguration * string * Map<string, string> -> IRestResponse
  abstract member HttpSonarPostRequest : ConnectionConfiguration * string * Map<string, string> -> IRestResponse


