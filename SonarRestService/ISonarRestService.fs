// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISonarRestService.fs" company="Trimble Navigation Limited">
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

type ISonarRestService = 
  abstract member GetIssuesByAssigneeInProject : ConnectionConfiguration * string *  string -> System.Collections.Generic.List<Issue>
  abstract member GetAllIssuesByAssignee : ConnectionConfiguration * string -> System.Collections.Generic.List<Issue>
  abstract member GetIssuesForProjects : ConnectionConfiguration * string -> System.Collections.Generic.List<Issue>
  abstract member GetIssuesForProjectsCreatedAfterDate : ConnectionConfiguration * string * DateTime -> System.Collections.Generic.List<Issue>
  abstract member GetIssuesInResource : ConnectionConfiguration * string  -> System.Collections.Generic.List<Issue>
  abstract member GetIssues : ConnectionConfiguration * string * string -> System.Collections.Generic.List<Issue>
  
  abstract member CommentOnIssues : ConnectionConfiguration * System.Collections.Generic.List<Issue> * comment : string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>
  abstract member ReOpenIssues : ConnectionConfiguration * System.Collections.Generic.List<Issue> * comment : string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>
  abstract member ConfirmIssues : ConnectionConfiguration * System.Collections.Generic.List<Issue> * comment : string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>
  abstract member UnConfirmIssues : ConnectionConfiguration * System.Collections.Generic.List<Issue> * comment : string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>
  abstract member MarkIssuesAsFalsePositive : ConnectionConfiguration * System.Collections.Generic.List<Issue> * string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>
  abstract member ResolveIssues : ConnectionConfiguration * System.Collections.Generic.List<Issue> * string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>
  abstract member AssignIssuesToUser : ConnectionConfiguration * System.Collections.Generic.List<Issue> * User * string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>

  abstract member GetUserList : ConnectionConfiguration -> System.Collections.Generic.List<User>
  abstract member AuthenticateUser : ConnectionConfiguration -> bool
  abstract member GetResourcesData : ConnectionConfiguration * string -> System.Collections.Generic.List<Resource>
  abstract member GetProjectsList : ConnectionConfiguration -> System.Collections.Generic.List<Resource>
  abstract member GetServerInfo : ConnectionConfiguration ->  float
  abstract member GetCoverageInResource : ConnectionConfiguration * string ->  SourceCoverage
  abstract member GetSourceForFileResource : ConnectionConfiguration * string ->  Source

  abstract member GetProperties : ConnectionConfiguration -> System.Collections.Generic.Dictionary<string, string>

  abstract member GetEnabledRulesInProfile : ConnectionConfiguration * string * string -> System.Collections.Generic.List<Profile>
  abstract member GetQualityProfile : ConnectionConfiguration * string -> System.Collections.Generic.List<Resource>
  abstract member GetRules : ConnectionConfiguration * string -> System.Collections.Generic.List<Rule>

  abstract member ParseReportOfIssues : string -> System.Collections.Generic.List<Issue>
  abstract member ParseReportOfIssuesOld : string -> System.Collections.Generic.List<Issue>
  abstract member ParseDryRunReportOfIssues : string -> System.Collections.Generic.List<Issue>

  abstract member GetDuplicationsDataInResource : ConnectionConfiguration * string ->  Collections.Generic.List<DuplicationData>
  
        

