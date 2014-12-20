// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISonarRestService.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace SonarRestService

open ExtensionTypes
open System.Collections.ObjectModel
open System
open System.Web

type ISonarRestService = 
  abstract member GetIssuesByAssigneeInProject : ISonarConfiguration * string *  string -> System.Collections.Generic.List<Issue>
  abstract member GetAllIssuesByAssignee : ISonarConfiguration * string -> System.Collections.Generic.List<Issue>
  abstract member GetIssuesForProjects : ISonarConfiguration * string -> System.Collections.Generic.List<Issue>
  abstract member GetIssuesForProjectsCreatedAfterDate : ISonarConfiguration * string * DateTime -> System.Collections.Generic.List<Issue>
  abstract member GetIssuesInResource : ISonarConfiguration * string  -> System.Collections.Generic.List<Issue>
  abstract member GetIssues : ISonarConfiguration * string * string -> System.Collections.Generic.List<Issue>
  abstract member GetProjects  : ISonarConfiguration -> System.Collections.Generic.List<SonarProject>
  
  abstract member CommentOnIssues : ISonarConfiguration * System.Collections.IList * comment : string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>
  abstract member ReOpenIssues : ISonarConfiguration * System.Collections.Generic.List<Issue> * comment : string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>
  abstract member ConfirmIssues : ISonarConfiguration * System.Collections.IList * comment : string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>
  abstract member UnConfirmIssues : ISonarConfiguration * System.Collections.IList * comment : string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>
  abstract member MarkIssuesAsFalsePositive : ISonarConfiguration * System.Collections.IList * string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>
  abstract member ResolveIssues : ISonarConfiguration * System.Collections.IList * string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>
  abstract member AssignIssuesToUser : ISonarConfiguration * System.Collections.Generic.List<Issue> * User * string -> System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>

  abstract member GetUserList : ISonarConfiguration -> System.Collections.Generic.List<User>
  abstract member AuthenticateUser : ISonarConfiguration -> bool
  abstract member GetResourcesData : ISonarConfiguration * string -> System.Collections.Generic.List<Resource>
  abstract member GetProjectsList : ISonarConfiguration -> System.Collections.Generic.List<Resource>
  abstract member GetServerInfo : ISonarConfiguration ->  float
  abstract member GetCoverageInResource : ISonarConfiguration * string ->  SourceCoverage
  abstract member GetSourceForFileResource : ISonarConfiguration * string ->  Source

  abstract member GetProperties : ISonarConfiguration -> System.Collections.Generic.Dictionary<string, string>

  abstract member GetEnabledRulesInProfile : ISonarConfiguration * string * string -> System.Collections.Generic.List<Profile>
  abstract member GetQualityProfile : ISonarConfiguration * string -> System.Collections.Generic.List<Resource>
  abstract member GetRulesForProfile : conf:ISonarConfiguration * profile:Profile * ruleDetails:bool * active:bool -> unit
  abstract member GetRulesForProfile : conf:ISonarConfiguration * profile:Profile -> unit
  abstract member GetQualityProfilesForProject : ISonarConfiguration * string -> System.Collections.Generic.List<Profile>
  abstract member GetQualityProfilesForProject : ISonarConfiguration * projectKey:string * language:string -> System.Collections.Generic.List<Profile>
  abstract member GetAvailableProfiles : ISonarConfiguration -> System.Collections.Generic.List<Profile>
  abstract member GetRules : ISonarConfiguration * string -> System.Collections.Generic.List<Rule>
  abstract member GetTemplateRules : ISonarConfiguration * profile:Profile -> unit
  abstract member UpdateRule : conf:ISonarConfiguration * key:string * optionalProps:System.Collections.Generic.Dictionary<string, string> -> System.Collections.Generic.List<string>
  abstract member GetAllTags : ISonarConfiguration -> System.Collections.Generic.List<string>
  abstract member UpdateTags : ISonarConfiguration * rule:Rule * tags:System.Collections.Generic.List<string> -> System.Collections.Generic.List<string>

  abstract member ActivateRule : ISonarConfiguration * rule:Rule * profilekey:string -> System.Collections.Generic.List<string>
  abstract member DeleteRule : ISonarConfiguration * rule:Rule  -> System.Collections.Generic.List<string>
  abstract member DisableRule : ISonarConfiguration * rule:Rule * profilekey:string -> System.Collections.Generic.List<string>
  abstract member CreateRule : ISonarConfiguration * rule:Rule * templateRule:Rule -> System.Collections.Generic.List<string>

  // might be remove in the future
  abstract member GetProfilesUsingRulesApp : ISonarConfiguration -> System.Collections.Generic.List<Profile>
  abstract member GetRulesForProfileUsingRulesApp : conf:ISonarConfiguration * profile:Profile * active:bool -> unit
  abstract member GetRuleUsingProfileAppId :  conf:ISonarConfiguration * ruleKey:string -> Rule

  abstract member ParseReportOfIssues : string -> System.Collections.Generic.List<Issue>
  abstract member ParseReportOfIssuesOld : string -> System.Collections.Generic.List<Issue>
  abstract member ParseDryRunReportOfIssues : string -> System.Collections.Generic.List<Issue>

  abstract member GetDuplicationsDataInResource : ISonarConfiguration * string ->  Collections.Generic.List<DuplicationData>
  
        

