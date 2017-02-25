// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarRestService.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

open FSharp.Data
open FSharp.Data.JsonExtensions
open RestSharp
open VSSonarPlugins
open VSSonarPlugins.Types
open System.Collections.ObjectModel
open System
open System.Web
open System.Net
open System.IO
open System.Text.RegularExpressions
open System.Linq

open SonarRestService

type SonarRestService(httpconnector : IHttpSonarConnector) = 
    let httpconnector = httpconnector
    let mutable cancelRequest = false

    let GetSeverity(value : string) =
        (EnumHelper.asEnum<Severity>(value)).Value
                    
    let (|NotNull|_|) value = 
        if obj.ReferenceEquals(value, null) then None 
        else Some()

    let GetComment(comment : string) =
        if String.IsNullOrEmpty(comment) then
            String.Empty
        else
            "&comment=" + HttpUtility.UrlEncode(comment)

    let getUserListFromXmlResponse(responsecontent : string) =
        let data = JsonUsers.Parse(responsecontent) 

        let userList = new System.Collections.Generic.List<User>()
        for user in data.Users do
            let newUser = new User()

            if not(obj.ReferenceEquals(user.Email, null)) then
                newUser.Email <- user.Email.Value
            else
                newUser.Email <- ""

            match user.Name with
            | None -> newUser.Name <- ""
            | Some c -> newUser.Name <- c

            newUser.Active <- user.Active
            newUser.Login <- user.Login

            userList.Add(newUser)

        userList

    let getIssueFromString(responsecontent : string) =
        let data = JsonIssue.Parse(responsecontent).Issue
        let issue = new Issue()
        issue.Message <- data.Message
        issue.CreationDate <- data.CreationDate

        issue.Component <- data.Component
        try
            issue.Line <- data.Line
        with
        | ex -> issue.Line <- 0

        issue.Project <- data.Project
        issue.UpdateDate <- data.UpdateDate
        issue.Status <- (EnumHelper.asEnum<IssueStatus>(data.Status)).Value
        issue.Severity <- GetSeverity(data.Severity)
        issue.Rule <- data.Rule
        issue.Key <- data.Key.ToString()

        if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("assignee"), null)) then
            issue.Assignee <- data.Assignee

        if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("author"), null)) then
            issue.Author <- data.Author

        if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("comments"), null)) then
            for elemC in data.Comments do issue.Comments.Add(new Comment(elemC.CreatedAt, elemC.HtmlText, elemC.Key, elemC.Login, -1))

        if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("closeDate"), null)) then
            issue.CloseDate <- data.CloseDate

        if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("resolution"), null)) then
            issue.Resolution <- (EnumHelper.asEnum<Resolution>(data.Resolution.Replace("-","_"))).Value

        if issue.Comments.Count <> 0 then
            for comment in issue.Comments do
                if comment.HtmlText.StartsWith("[VSSonarQubeExtension] Attached to issue: ") then
                    for item in Regex.Matches(comment.HtmlText, "\\d+") do
                        if issue.IssueTrackerId = null || issue.IssueTrackerId = "" then
                            issue.IssueTrackerId <- item.Value
        issue

    let getIssuesFromStringAfter45(responsecontent : string) =
        let data = JSonIssuesRest.Parse(responsecontent)
        let issueList = new System.Collections.Generic.List<Issue>()
        for elem in data.Issues do
            let issue = new Issue()
            issue.Message <- elem.Message
            issue.CreationDate <- elem.CreationDate

            issue.Component <- elem.Component
            try
                if not(obj.ReferenceEquals(elem.Line, null)) then
                    issue.Line <- elem.Line
                else
                    issue.Line <- 0
            with
            | ex -> ()

            issue.Project <- elem.Project
            issue.UpdateDate <- elem.UpdateDate
            issue.Status <- (EnumHelper.asEnum<IssueStatus>(elem.Status)).Value
            issue.Severity <- GetSeverity(elem.Severity)
            issue.Rule <- elem.Rule
            issue.Key <- elem.Key.ToString().Replace("\"", "")

            if not(obj.ReferenceEquals(elem.Assignee, null)) then
                match elem.Assignee with
                | None -> ()
                | Some value -> issue.Assignee <- value

            if not(obj.ReferenceEquals(elem.Author, null)) then
                match elem.Author with
                | None -> ()
                | Some value -> issue.Author <- value

            if not(obj.ReferenceEquals(elem.Comments, null)) then
                for elemC in elem.Comments do issue.Comments.Add(new Comment(elemC.CreatedAt, elemC.HtmlText, elemC.Key, elemC.Login, -1))

            if not(obj.ReferenceEquals(elem.CloseDate, null)) then
                match elem.CloseDate with
                | None -> ()
                | Some value -> issue.CloseDate <- value

            if not(obj.ReferenceEquals(elem.Resolution, null)) then
                match elem.Resolution with
                | None -> ()
                | Some value -> issue.Resolution <- (EnumHelper.asEnum<Resolution>(value.Replace("-", "_"))).Value

            if not(obj.ReferenceEquals(elem.Effort, null)) then
                match elem.Effort with
                | None -> ()
                | Some value -> issue.Effort <- value

            if not(obj.ReferenceEquals(elem.Tags, null)) then
                for tag in elem.Tags do issue.Tags.Add(tag)

            if issue.Comments.Count <> 0 then
                for comment in issue.Comments do
                    if comment.HtmlText.StartsWith("[VSSonarQubeExtension] Attached to issue: ") then
                        for item in Regex.Matches(comment.HtmlText, "\\d+") do
                            if issue.IssueTrackerId = null || issue.IssueTrackerId = "" then
                                issue.IssueTrackerId <- item.Value

            issueList.Add(issue)

        issueList

    let getIssuesFromString(responsecontent : string) =
        let data = JsonIssues.Parse(responsecontent)
        let issueList = new System.Collections.Generic.List<Issue>()
        for elem in data.Issues do
            try
                let issue = new Issue()
                issue.Message <- elem.Message
                issue.CreationDate <- elem.CreationDate

                issue.Component <- elem.Component
                if not(obj.ReferenceEquals(elem.Line, null)) then
                    issue.Line <- elem.Line.Value
                else
                    issue.Line <- 0

                issue.Project <- elem.Project
                issue.UpdateDate <- elem.UpdateDate
                issue.Status <- (EnumHelper.asEnum<IssueStatus>(elem.Status)).Value
                issue.Severity <- GetSeverity(elem.Severity)
                issue.Rule <- elem.Rule
                issue.Key <- elem.Key.ToString()

                if not(obj.ReferenceEquals(elem.Assignee, null)) then
                    issue.Assignee <- elem.Assignee.Value

                if not(obj.ReferenceEquals(elem.Author, null)) then
                    issue.Author <- elem.Author.Value
            
                if not(obj.ReferenceEquals(elem.Comments, null)) then
                    for elemC in elem.Comments do issue.Comments.Add(new Comment(elemC.CreatedAt, elemC.HtmlText, elemC.Key, elemC.Login, -1))

                if not(obj.ReferenceEquals(elem.CloseDate, null)) then
                    issue.CloseDate <- elem.CloseDate.Value

                if not(obj.ReferenceEquals(elem.Resolution, null)) then
                    issue.Resolution <- (EnumHelper.asEnum<Resolution>(elem.Resolution.Value.Replace("-", "_"))).Value

                if issue.Comments.Count <> 0 then
                    for comment in issue.Comments do
                        if comment.HtmlText.StartsWith("[VSSonarQubeExtension] Attached to issue: ") then
                            for item in Regex.Matches(comment.HtmlText, "\\d+") do
                                if issue.IssueTrackerId = null || issue.IssueTrackerId = "" then
                                    issue.IssueTrackerId <- item.Value

                issueList.Add(issue)
            with
            | ex -> ()

        issueList

    let getViolationsFromString(responsecontent : string, reviewAsIssues : System.Collections.Generic.List<Issue>) =
        let data = JSonViolation.Parse(responsecontent)
        let issueList = new System.Collections.Generic.List<Issue>()
        for elem in data do
            let issue = new Issue()
            issue.CreationDate <- elem.CreatedAt
            issue.Component <- elem.Resource.Key
            issue.Id <- elem.Id
            issue.Message <- elem.Message
            if not(obj.ReferenceEquals(elem.Line, null)) then
                issue.Line <- elem.Line.Value
            else
                issue.Line <- 0

            issue.Severity <- GetSeverity(elem.Priority)
            issue.Status <- IssueStatus.OPEN

            // convert violation into review if a review is present
            for review in reviewAsIssues do
                if review.Message = issue.Message && review.Line = issue.Line then
                    issue.Id <- review.Id
                    issue.Assignee <- review.Assignee
                    issue.Resolution <- review.Resolution
                    issue.Status <- review.Status
                    for comment in review.Comments do
                        issue.Comments.Add(comment)
                    
            issueList.Add(issue)
        issueList
        
    let getReviewsFromString(responsecontent : string) =
        let data = JSonarReview.Parse(responsecontent)
        let issueList = new System.Collections.Generic.List<Issue>()
        for elem in data do
            let issue = new Issue()
            issue.Message <- elem.Title
            issue.CreationDate <- elem.CreatedAt

            issue.Component <- elem.Resource
            try
                issue.Line <- elem.Line
            with
            | ex -> issue.Line <- 0

            let keyelems = elem.Resource.Split(':')
            issue.Project <-  keyelems.[0] + ":" + keyelems.[1] 
            issue.UpdateDate <- elem.UpdatedAt
            issue.Status <- (EnumHelper.asEnum<IssueStatus>(elem.Status)).Value
            issue.Severity <- GetSeverity(elem.Severity)
            issue.Id <- elem.Id
            issue.Rule <- ""
            let violationId = elem.ViolationId
            issue.ViolationId <- violationId
            try
                issue.Assignee <- elem.Assignee.Value
            with
            | ex -> ()            

            match elem.JsonValue.TryGetProperty("comments") with
            | NotNull ->
                for elemC in elem.Comments do issue.Comments.Add(new Comment(elemC.UpdatedAt, elemC.Text, "", elemC.Author, elem.Id))
            | _ -> ()

            issueList.Add(issue)

        issueList

    let getIssuesOldAndNewVersions(userConf, newurl : string, oldurlreviews: string, oldurlviolations) = 

        cancelRequest <- false

        try
            let allIssues = new System.Collections.Generic.List<Issue>()
            try
                let responsecontent = httpconnector.HttpSonarGetRequest(userConf, newurl)
                let data = JSonIssuesRest.Parse(responsecontent)

                let AddElements(all : System.Collections.Generic.List<Issue>) = 
                    for issue in all do
                        allIssues.Add(issue)

                AddElements(getIssuesFromStringAfter45(responsecontent))

                // we need to get all pages
                let value = int(System.Math.Ceiling(float(data.Paging.Total) / float(data.Paging.PageSize)))

                for i = 2 to value do
                    if not(cancelRequest) then
                        let url = newurl + "&pageIndex=" + Convert.ToString(i)
                        let newresponse = httpconnector.HttpSonarGetRequest(userConf, url)
                        AddElements(getIssuesFromStringAfter45(newresponse))
            with
            | ex -> 
                let responsecontent = httpconnector.HttpSonarGetRequest(userConf, newurl)
                let data = JsonIssues.Parse(responsecontent)

                let AddElements(all : System.Collections.Generic.List<Issue>) = 
                    for issue in all do
                        allIssues.Add(issue)

                AddElements(getIssuesFromString(responsecontent))

                // we need to get all pages
            
                for i = 2 to data.Paging.Pages do
                    if not(cancelRequest) then
                        let url = newurl + "&pageIndex=" + Convert.ToString(i)
                        let newresponse = httpconnector.HttpSonarGetRequest(userConf, url)
                        AddElements(getIssuesFromString(newresponse))
            allIssues
        with
        | ex ->
            if oldurlviolations = "" then
                let oldcontent = httpconnector.HttpSonarGetRequest(userConf, oldurlreviews)
                getReviewsFromString(oldcontent)
            else
                let reviewsInResource = getReviewsFromString(httpconnector.HttpSonarGetRequest(userConf, oldurlreviews))
                let oldcontent = httpconnector.HttpSonarGetRequest(userConf, oldurlviolations)
                getViolationsFromString(oldcontent, reviewsInResource)

        

    let getViolationsOldAndNewFormat(userConf, resource : string) = 
        try
            let url =  "/api/issues/search?components=" + resource + "&statuses=OPEN,CONFIRMED,REOPENED"
            let responsecontent = httpconnector.HttpSonarGetRequest(userConf, url)
            try
                getIssuesFromStringAfter45(responsecontent)
            with
            | ex -> getIssuesFromString(responsecontent)
        with
        | ex ->          
            let reviewsurl = "/api/reviews?resources="+ resource
            let reviewsInResource = getReviewsFromString(httpconnector.HttpSonarGetRequest(userConf, reviewsurl))
            let violationsurl = "/api/violations?resource=" + resource
            let oldcontent = httpconnector.HttpSonarGetRequest(userConf, violationsurl)
            getViolationsFromString(oldcontent, reviewsInResource)

    let getProjectResourcesFromResponseContent(responsecontent : string) = 
        let resources = JsonResourceWithMetrics.Parse(responsecontent)
        let resourcelist = System.Collections.Generic.List<Resource>()
            
        for resource in resources do
            let res = new Resource()
            res.Id <- resource.Id
            res.Date <- resource.Date
            res.Key <- resource.Key.Trim()
            res.Lang <- resource.Lang
            res.Lname <- resource.Lname
            res.Name <- resource.Name
            res.Qualifier <- resource.Qualifier
            res.Scope <- resource.Scope
            try
                res.Version <- sprintf "%s" resource.Version
            with
                | ex -> ()

            resourcelist.Add(res)
        resourcelist

    let getResourcesFromResponseContent(responsecontent : string) = 
        let resources = JsonResourceWithMetrics.Parse(responsecontent)
        let resourcelist = System.Collections.Generic.List<Resource>()
            
        for resource in resources do
            try
                let res = new Resource()
                res.Id <- resource.Id
                res.Date <- resource.Date
                res.Key <- resource.Key.Trim()

                if not(obj.ReferenceEquals(resource.JsonValue.TryGetProperty("lang"), null)) then
                    res.Lang <- sprintf "%s" resource.Lang

                res.Lname <- resource.Lname
                res.Name <- resource.Name
                res.Qualifier <- resource.Qualifier
                res.Scope <- resource.Scope
                res.IsBranch <- false
                if not(obj.ReferenceEquals(resource.JsonValue.TryGetProperty("branch"), null)) then
                    res.BranchName <- sprintf "%s" resource.Branch
                    res.IsBranch <- true

                if not(obj.ReferenceEquals(resource.JsonValue.TryGetProperty("version"), null)) then
                    res.Version <- sprintf "%s" resource.Version

                let metrics = new System.Collections.Generic.List<Metric>()

                if not(obj.ReferenceEquals(resource.JsonValue.TryGetProperty("msr"), null)) then
                    for metric in resource.Msr do
                        let met = new Metric()

                        met.Key <- metric.Key

                        match metric.JsonValue.TryGetProperty("data") with
                        | NotNull ->
                            met.Data <- metric.Data.Value.Trim()
                        | _ -> ()

                        
                        met.FormatedValue <- sprintf "%f" metric.FrmtVal

                        match metric.JsonValue.TryGetProperty("val") with
                        | NotNull ->
                            met.Val <- metric.Val
                        | _ -> ()

                        metrics.Add(met)

                        res.Metrics <- metrics


                if not(res.IsBranch) then
                    let keyelems = res.Key.Split(':')
                    let nameelems = res.Name.Split(' ')
                    if keyelems.[keyelems.Length - 1] = nameelems.[nameelems.Length - 1] && nameelems.Length > 1 then
                        res.IsBranch <- true
                        res.BranchName <- nameelems.[nameelems.Length - 1]
                    
                resourcelist.Add(res)
            with
                | ex -> ()
        resourcelist

    let getRulesFromResponseContent(responsecontent : string) = 
        let rules = JSonRule.Parse(responsecontent)
        let rulesToReturn = new System.Collections.Generic.List<Rule>()

        for ruleInServer in rules do
            let rule = new Rule()
            rule.Name <- ruleInServer.Title            
            rule.ConfigKey <- ruleInServer.ConfigKey
            rule.HtmlDescription <- ruleInServer.Description
            rule.Key <- ruleInServer.Key
            rule.Severity <- (EnumHelper.asEnum<Severity>(ruleInServer.Priority)).Value
            rule.Repo <- ruleInServer.Plugin
            rulesToReturn.Add(rule)

        rulesToReturn
        
    let GetQualityProfilesFromContent(responsecontent : string, conf : ISonarConfiguration, rest : ISonarRestService) = 
        let parsed = JsonQualityProfiles.Parse(responsecontent)
        let profiles = new System.Collections.Generic.List<Profile>()

        for eachprofile in parsed do
            let newProfile = new Profile(rest, conf)
            newProfile.Default <- eachprofile.Default
            newProfile.Language <- eachprofile.Language
            newProfile.Name <- eachprofile.Name

            let profileRules = new System.Collections.Generic.List<Rule>()
            let profileAlerts = new System.Collections.Generic.List<Alert>()

            profiles.Add(newProfile)

        profiles

    let GetProfileFromContent(responsecontent : string, conf : ISonarConfiguration, rest : ISonarRestService) = 
        let parsed = JSonProfile.Parse(responsecontent)
        let profiles = new System.Collections.Generic.List<Profile>()

        for eachprofile in parsed do
            let newProfile = new Profile(rest, conf)
            newProfile.Default <- eachprofile.Default
            newProfile.Language <- eachprofile.Language
            newProfile.Name <- eachprofile.Name

            let profileRules = new System.Collections.Generic.Dictionary<string, Rule>()
            let profileAlerts = new System.Collections.Generic.List<Alert>()

            for eachrule in eachprofile.Rules do
                let newRule = new Rule()
                newRule.Key <- eachrule.Key
                newRule.Repo <- eachrule.Repo
                newRule.ConfigKey <- eachrule.Key + ":" + eachrule.Repo
                newRule.Severity <- (EnumHelper.asEnum<Severity>(eachrule.Severity)).Value
                if not(profileRules.ContainsKey(newRule.ConfigKey)) then
                    profileRules.Add(newRule.ConfigKey, newRule)

                newProfile.AddRule(newRule)

            try
                for eachalert in eachprofile.Alerts do
                    let newAlert = new Alert()
                    match eachalert.JsonValue.TryGetProperty("error") with
                    | NotNull ->
                        newAlert.Error <- eachalert.Error
                    | _ -> ()

                    match eachalert.JsonValue.TryGetProperty("warning") with
                    | NotNull ->
                        newAlert.Warning <- eachalert.Warning
                    | _ -> ()

                    newAlert.Metric <- eachalert.Metric
                    newAlert.Operator  <- eachalert.Operator
                    profileAlerts.Add(newAlert)

                newProfile.Alerts <- profileAlerts
            with
             | ex -> ()

            profiles.Add(newProfile)

        profiles

    let GetSourceFromContent(content : string) =
        let data = JsonValue.Parse(content)
        let source = new Source()

        let arrayOfLines : string array = Array.zeroCreate (data.[0].Properties |> Seq.length)

        let CreateLine(data : string, elem : JsonValue) =
            let line = new Line()
            arrayOfLines.[Int32.Parse(data) - 1] <- elem.InnerText()
            ()

        data.[0].Properties |> Seq.iter (fun elem -> CreateLine(elem))

        source.Lines <- arrayOfLines
        source

    let GetSourceFromRaw(raw : string) =
        new Source(Lines = Regex.Split(raw, "\r\n|\r|\n"))

    let GetDuplicationsFromContent(responsecontent : string) = 
        let dups = JSonDuplications.Parse(responsecontent)
        let duplicationData = new Collections.Generic.List<DuplicationData>()

        dups |> Seq.iter (fun x ->
                            let duplicatedResource = new DuplicationData()
                            let resource = new Resource(Date = x.Date,
                                                        Id = x.Id,
                                                        Key = x.Key,
                                                        Name = x.Name,
                                                        Lname = x.Lname,
                                                        Qualifier = x.Qualifier,
                                                        Scope = x.Scope,
                                                        Lang = x.Lang)
                            
                            duplicatedResource.Resource <- resource

                            let data = DupsData.Parse(x.Msr.[0].Data)
                            for group in data.Gs do
                                let groupToAdd = new DuplicatedGroup()
                                for block in group.Bs do
                                    let blockToAdd = new DuplicatedBlock()
                                    blockToAdd.Lenght <- block.L
                                    blockToAdd.Startline <- block.S
                                    blockToAdd.Resource <- new Resource(Key = block.R)
                                    groupToAdd.DuplicatedBlocks.Add(blockToAdd)
                                duplicatedResource.DuplicatedGroups.Add(groupToAdd)

                            duplicationData.Add(duplicatedResource))
                            

        duplicationData

    let GetCoverageFromContent(responsecontent : string) = 
        let source = new SourceCoverage()

        try
            let resources = JsonResourceWithMetrics.Parse(responsecontent)


            try
                source.SetLineCoverageData(resources.[0].Msr.[0].Data.Value)
            with
                | ex -> ()

            try
                source.SetBranchCoverageData(resources.[0].Msr.[1].Data.Value, resources.[0].Msr.[2].Data.Value)
            with
                | ex -> ()
        with
            | ex -> ()

        source

    let QuerySonar(userconf : ISonarConfiguration, urltosue : string, methodin : Method) =
        let client = new RestClient(userconf.Hostname)
        client.Authenticator <- new HttpBasicAuthenticator(userconf.Username, userconf.Password)
        let request = new RestRequest(urltosue, methodin)
        request.AddHeader(HttpRequestHeader.Accept.ToString(), "text/xml") |> ignore
        client.Execute(request)

    let PerformWorkFlowTransition(userconf : ISonarConfiguration, issue : Issue, transition : string) =
        let parameters = Map.empty.Add("issue", issue.Key.ToString()).Add("transition", transition)
        httpconnector.HttpSonarPostRequest(userconf, "/api/issues/do_transition", parameters)

    let DoStateTransition(userconf : ISonarConfiguration, issue : Issue, finalState : IssueStatus, transition : string) = 
        let mutable status = Net.HttpStatusCode.OK

        let response = PerformWorkFlowTransition(userconf, issue, transition)
        status <- response.StatusCode
        if status = Net.HttpStatusCode.OK then
            let data = getIssueFromString(response.Content)
            if data.Status.Equals(finalState) then
                issue.Status <- data.Status
                issue.Resolution <- data.Resolution
            else
                status <- Net.HttpStatusCode.BadRequest

        status

    let CreateRuleInProfile2(parsedDataRule:JsonRule.Rule, profile : Profile)=
        let newRule = new Rule()
        newRule.Key <-  try parsedDataRule.InternalKey with | ex -> ""
        newRule.ConfigKey <-  try parsedDataRule.InternalKey with | ex -> ""
        newRule.Repo <- parsedDataRule.Repo
        newRule.Name <- parsedDataRule.Name
        newRule.CreatedAt <- parsedDataRule.CreatedAt
        newRule.Severity <- try (EnumHelper.asEnum<Severity>(parsedDataRule.Severity)).Value with | ex -> Severity.UNDEFINED
        newRule.Status <- try (EnumHelper.asEnum<Status>(parsedDataRule.Status)).Value with | ex -> Status.UNDEFINED
        newRule.InternalKey <- try parsedDataRule.InternalKey with | ex -> ""
        newRule.IsTemplate <- try parsedDataRule.IsTemplate with | ex -> false
        for tag in parsedDataRule.Tags do
            newRule.Tags.Add(tag)
        for tag in parsedDataRule.SysTags do
            newRule.SysTags.Add(tag)
        newRule.Lang <- parsedDataRule.Lang
        newRule.LangName <- parsedDataRule.LangName
        newRule.HtmlDescription <- parsedDataRule.HtmlDesc
        newRule.DefaultDebtChar <- try (EnumHelper.asEnum<Category>(parsedDataRule.DefaultDebtChar)).Value with | ex -> Category.UNDEFINED
        newRule.DefaultDebtSubChar <- try (EnumHelper.asEnum<SubCategory>(parsedDataRule.DefaultDebtSubChar)).Value with | ex -> SubCategory.UNDEFINED
        newRule.Category <- try (EnumHelper.asEnum<Category>(parsedDataRule.DebtChar)).Value with | ex -> Category.UNDEFINED
        newRule.Subcategory <- try (EnumHelper.asEnum<SubCategory>(parsedDataRule.DebtSubChar)).Value with | ex -> SubCategory.UNDEFINED

        newRule.SubcategoryName <- try parsedDataRule.DebtCharName with | ex -> ""
        newRule.CategoryName <- try parsedDataRule.DebtSubCharName with | ex -> ""                 

        newRule.DefaultDebtRemFnType <- try (EnumHelper.asEnum<RemediationFunction>(parsedDataRule.DefaultDebtRemFnType)).Value with | ex -> RemediationFunction.UNDEFINED
        newRule.DefaultDebtRemFnCoeff <- try parsedDataRule.DefaultDebtRemFnCoeff with | ex -> ""
        newRule.DebtOverloaded <- try parsedDataRule.DebtOverloaded with | ex -> false
        newRule.RemediationFunction <- try (EnumHelper.asEnum<RemediationFunction>(parsedDataRule.DebtRemFnType)).Value with | ex -> RemediationFunction.UNDEFINED
        newRule.DebtRemFnCoeff <- try parsedDataRule.DebtRemFnCoeff with | ex -> ""
        try
            let value = Regex.Replace(newRule.DebtRemFnCoeff, "[^0-9.]", "")
            let unit = newRule.DebtRemFnCoeff.Replace(value, "").Replace("min", "MN").Replace("hour", "H").Replace("day", "D")
            newRule.RemediationFactorVal <- Int32.Parse(value)
            newRule.RemediationFactorTxt <- try (EnumHelper.asEnum<RemediationUnit>(unit)).Value with | ex -> RemediationUnit.UNDEFINED
        with
        | ex ->
            newRule.RemediationFactorVal <- 0
            newRule.RemediationFactorTxt <- RemediationUnit.UNDEFINED

        try
            for param in parsedDataRule.Params do
                let ruleparam  = new RuleParam()
                ruleparam.Key <- param.Key
                let isFound = (List.ofSeq newRule.Params) |> List.tryFind (fun c -> c.Key.Equals(param.Key))
                match isFound with
                | Some elem -> ()
                | _ -> newRule.Params.Add(ruleparam)
        with
        | ex -> ()

        profile.AddRule(newRule)

    let UpdateRuleInProfile(parsedDataRule:JsonRuleSearchResponse.Rule,
                            rule : Rule,
                            skipSeverity : bool,
                            userconf : ISonarConfiguration,
                            profile:Profile) =

        let IfExists(propertyToSet:string) =
            match parsedDataRule.JsonValue.TryGetProperty(propertyToSet) with
            | NotNull ->
                true
            | _ -> 
                false

        if IfExists("repo") then rule.Repo <- parsedDataRule.Repo

        if IfExists("key") then
            rule.Key <-  try parsedDataRule.Key.Replace(rule.Repo  + ":", "") with | ex -> ""
            rule.ConfigKey <-  try parsedDataRule.Key with | ex -> ""


        if IfExists("name") then rule.Name <- parsedDataRule.Name
        if IfExists("type") then rule.Type <- parsedDataRule.Type
        if IfExists("createdAt") then rule.CreatedAt <- parsedDataRule.CreatedAt
        if not(skipSeverity) then
            if IfExists("severity") then rule.Severity <- try (EnumHelper.asEnum<Severity>(parsedDataRule.Severity)).Value with | ex -> Severity.UNDEFINED

        if IfExists("status") then rule.Status <- try (EnumHelper.asEnum<Status>(parsedDataRule.Status)).Value with | ex -> Status.UNDEFINED
        if IfExists("internalKey") then rule.InternalKey <- parsedDataRule.InternalKey

        if IfExists("isTemplate") then rule.IsTemplate <- try parsedDataRule.IsTemplate with | ex -> false

        if IfExists("tags") then
            for tag in parsedDataRule.Tags do
                let foundAlready = rule.Tags |> Seq.tryFind (fun c -> c.Equals(tag))
                match foundAlready with 
                | Some data -> ()
                | _ -> rule.Tags.Add(tag)
                
        if IfExists("sysTags") then
            for tag in parsedDataRule.SysTags do
                rule.SysTags.Add(tag)

        if IfExists("lang") then rule.Lang <- parsedDataRule.Lang
        if IfExists("langName") then rule.LangName <- parsedDataRule.LangName
        if IfExists("htmlDesc") then rule.HtmlDescription <- parsedDataRule.HtmlDesc
        if IfExists("defaultDebtChar") then rule.DefaultDebtChar <- try (EnumHelper.asEnum<Category>(parsedDataRule.DefaultDebtChar)).Value with | ex -> Category.UNDEFINED
        if IfExists("defaultDebtSubChar") then rule.DefaultDebtSubChar <- try (EnumHelper.asEnum<SubCategory>(parsedDataRule.DefaultDebtSubChar)).Value with | ex -> SubCategory.UNDEFINED
        if IfExists("debtChar") then rule.Category <- try (EnumHelper.asEnum<Category>(parsedDataRule.DebtChar)).Value with | ex -> Category.UNDEFINED
        if IfExists("debtSubChar") then rule.Subcategory <- try (EnumHelper.asEnum<SubCategory>(parsedDataRule.DebtSubChar)).Value with | ex -> SubCategory.UNDEFINED

        if IfExists("debtCharName") then rule.SubcategoryName <- try parsedDataRule.DebtCharName with | ex -> ""
        if IfExists("debtSubCharName") then rule.CategoryName <- try parsedDataRule.DebtSubCharName with | ex -> ""                 

        if IfExists("defaultDebtRemFnType") then  rule.DefaultDebtRemFnType <- try (EnumHelper.asEnum<RemediationFunction>(parsedDataRule.DefaultDebtRemFnType)).Value with | ex -> RemediationFunction.UNDEFINED
        if IfExists("defaultDebtRemFnCoeff") then rule.DefaultDebtRemFnCoeff <- try parsedDataRule.DefaultDebtRemFnCoeff with | ex -> ""
        if IfExists("debtOverloaded") then rule.DebtOverloaded <- try parsedDataRule.DebtOverloaded with | ex -> false

        if IfExists("debtRemFnType") then 
            rule.RemediationFunction <- try (EnumHelper.asEnum<RemediationFunction>(parsedDataRule.DebtRemFnType)).Value with | ex -> RemediationFunction.UNDEFINED

        if IfExists("debtRemFnCoeff") then
            rule.DebtRemFnCoeff <- parsedDataRule.DebtRemFnCoeff
            let value = Regex.Replace(rule.DebtRemFnCoeff, "[^0-9.]", "")
            let unit = rule.DebtRemFnCoeff.Replace(value, "").Replace("min", "MN").Replace("hour", "H").Replace("day", "D")
            rule.RemediationFactorVal <- Int32.Parse(value)
            rule.RemediationFactorTxt <- try (EnumHelper.asEnum<RemediationUnit>(unit)).Value with | ex -> RemediationUnit.UNDEFINED
        else
            rule.DebtRemFnCoeff <- ""
            rule.RemediationFactorVal <- 0
            rule.RemediationFactorTxt <- RemediationUnit.UNDEFINED

        if IfExists("params") && parsedDataRule.Params.Length > 0 && profile <> null then
            let ruleShow = 
                try
                    let url = sprintf "/api/rules/show?key=%s&actives=true" (HttpUtility.UrlEncode(rule.Repo + ":" + rule.Key))
                    let reply = httpconnector.HttpSonarGetRequest(userconf, url)
                    JsonarRuleShowResponse.Parse(reply)
                with
                | ex -> System.Diagnostics.Debug.WriteLine("Error Show rule: " + ex.Message)
                        JsonarRuleShowResponse.GetSample()

            if IfExists("templateKey") then
                rule.TemplateKey <- parsedDataRule.TemplateKey.Value.Replace(rule.Repo + ":", "")
            
            let AddParam(param:JsonRuleSearchResponse.Param) =
                try
                    let IfExistsParam(propertyToSet:string) =
                        match param.JsonValue.TryGetProperty(propertyToSet) with
                        | NotNull ->
                            true
                        | _ -> 
                            false
                    let ruleparam  = new RuleParam()
                    ruleparam.Key <- param.Key
                    ruleparam.DefaultValue <- param.DefaultValue
                    ruleparam.Value <- param.DefaultValue
                    ruleparam.Type <- param.Type
                    if IfExistsParam("htmlDesc") then
                        ruleparam.Desc <- param.HtmlDesc

                    let isFound = (List.ofSeq ruleShow.Actives) |> List.tryFind (fun active -> active.QProfile.Equals(profile.Key))
                    match isFound with
                    | Some elem -> 
                        rule.Severity <- try (EnumHelper.asEnum<Severity>(elem.Severity)).Value with | ex -> Severity.UNDEFINED
                        let paramshow = elem.Params |> Seq.tryFind (fun param -> param.Key.Equals(ruleparam.Key))
                        match paramshow with
                        | Some data -> ruleparam.Value <- data.Value.String.Value
                        | _ -> ()
                    | _ -> ()

                    rule.Params.Add(ruleparam)
                with
                | ex -> ()

            parsedDataRule.Params |> Seq.iter (fun param -> AddParam(param))



    let CreateRuleInProfile(parsedDataRule:JsonRuleSearchResponse.Rule, profile : Profile, enabledStatus:bool) =

        let IfExists(propertyToSet:string) =
            match parsedDataRule.JsonValue.TryGetProperty(propertyToSet) with
            | NotNull ->
                true
            | _ -> 
                false


        let newRule = new Rule()
        if IfExists("key") then
            newRule.Enabled <- enabledStatus
            newRule.Key <-  try parsedDataRule.Key with | ex -> ""
            newRule.ConfigKey <-  try parsedDataRule.Key with | ex -> ""

        if IfExists("repo") then newRule.Repo <- parsedDataRule.Repo
        if IfExists("name") then newRule.Name <- parsedDataRule.Name
        if IfExists("createdAt") then newRule.CreatedAt <- parsedDataRule.CreatedAt
        if IfExists("severity") then newRule.Severity <- try (EnumHelper.asEnum<Severity>(parsedDataRule.Severity)).Value with | ex -> Severity.UNDEFINED
        if IfExists("status") then newRule.Status <- try (EnumHelper.asEnum<Status>(parsedDataRule.Status)).Value with | ex -> Status.UNDEFINED
        if IfExists("internalKey") then newRule.InternalKey <- parsedDataRule.InternalKey

        if IfExists("isTemplate") then newRule.IsTemplate <- try parsedDataRule.IsTemplate with | ex -> false

        if IfExists("tags") then
            for tag in parsedDataRule.Tags do
                newRule.Tags.Add(tag)
        if IfExists("sysTags") then
            for tag in parsedDataRule.SysTags do
                newRule.SysTags.Add(tag)

        if IfExists("lang") then newRule.Lang <- parsedDataRule.Lang
        if IfExists("langName") then newRule.LangName <- parsedDataRule.LangName
        if IfExists("htmlDesc") then newRule.HtmlDescription <- parsedDataRule.HtmlDesc
        if IfExists("defaultDebtChar") then newRule.DefaultDebtChar <- try (EnumHelper.asEnum<Category>(parsedDataRule.DefaultDebtChar)).Value with | ex -> Category.UNDEFINED
        if IfExists("defaultDebtSubChar") then newRule.DefaultDebtSubChar <- try (EnumHelper.asEnum<SubCategory>(parsedDataRule.DefaultDebtSubChar)).Value with | ex -> SubCategory.UNDEFINED
        if IfExists("debtChar") then newRule.Category <- try (EnumHelper.asEnum<Category>(parsedDataRule.DebtChar)).Value with | ex -> Category.UNDEFINED
        if IfExists("debtSubChar") then newRule.Subcategory <- try (EnumHelper.asEnum<SubCategory>(parsedDataRule.DebtSubChar)).Value with | ex -> SubCategory.UNDEFINED

        if IfExists("debtCharName") then newRule.SubcategoryName <- try parsedDataRule.DebtCharName with | ex -> ""
        if IfExists("debtSubCharName") then newRule.CategoryName <- try parsedDataRule.DebtSubCharName with | ex -> ""                 

        if IfExists("defaultDebtRemFnType") then  newRule.DefaultDebtRemFnType <- try (EnumHelper.asEnum<RemediationFunction>(parsedDataRule.DefaultDebtRemFnType)).Value with | ex -> RemediationFunction.UNDEFINED
        if IfExists("defaultDebtRemFnCoeff") then newRule.DefaultDebtRemFnCoeff <- try parsedDataRule.DefaultDebtRemFnCoeff with | ex -> ""
        if IfExists("debtOverloaded") then newRule.DebtOverloaded <- try parsedDataRule.DebtOverloaded with | ex -> false



        if IfExists("debtRemFnType") then 
            newRule.RemediationFunction <- try (EnumHelper.asEnum<RemediationFunction>(parsedDataRule.DebtRemFnType)).Value with | ex -> RemediationFunction.UNDEFINED

        if IfExists("debtRemFnCoeff") then
            newRule.DebtRemFnCoeff <- parsedDataRule.DebtRemFnCoeff
            let value = Regex.Replace(newRule.DebtRemFnCoeff, "[^0-9.]", "")
            let unit = newRule.DebtRemFnCoeff.Replace(value, "").Replace("min", "MN").Replace("hour", "H").Replace("day", "D")
            newRule.RemediationFactorVal <- Int32.Parse(value)
            newRule.RemediationFactorTxt <- try (EnumHelper.asEnum<RemediationUnit>(unit)).Value with | ex -> RemediationUnit.UNDEFINED
        else
            newRule.DebtRemFnCoeff <- ""
            newRule.RemediationFactorVal <- 0
            newRule.RemediationFactorTxt <- RemediationUnit.UNDEFINED

        if IfExists("params") then
            try
                for param in parsedDataRule.Params do
                    let ruleparam  = new RuleParam()
                    ruleparam.Key <- param.Key
                    let isFound = (List.ofSeq newRule.Params) |> List.tryFind (fun c -> c.Key.Equals(param.Key))
                    match isFound with
                    | Some elem -> ()
                    | _ -> newRule.Params.Add(ruleparam)
                    
            with
            | ex -> ()

        profile.AddRule(newRule)  

    let GetRulesFromSearchQuery(rules : JsonRuleSearchResponse.Rule [], profile : Profile, enabledStatus:bool) =
        for parsedDataRule in rules do
            CreateRuleInProfile(parsedDataRule, profile, enabledStatus)


    member this.IgnoreAllFile2(conf : ISonarConfiguration, projectIn : Resource, file : string) =
        let epoch = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString().Replace(",", "")

        let uploadProperty(currentproject : Resource) =
            // let get properties for project
            let properties = (this :> ISonarRestService).GetProperties(conf, currentproject)

            let getPropertiesForUpload =                                 
                let elem = properties |> Seq.tryFind (fun c -> c.Key.Equals("sonar.issue.ignore.allfile"))
                match elem with
                | Some(c) -> c.Value + "," + epoch
                | _ -> epoch

            // upload new epochs
            let url = sprintf "/api/properties?id=sonar.issue.ignore.allfile&value=%s&resource=%s" getPropertiesForUpload currentproject.Key
            let response = httpconnector.HttpSonarPostRequest(conf, url, Map.empty)
            if response.StatusCode = Net.HttpStatusCode.OK then
                let url = sprintf "/api/properties?id=sonar.issue.ignore.allfile.%s.fileRegexp&value=%s&resource=%s" epoch file currentproject.Key
                httpconnector.HttpSonarPostRequest(conf, url, Map.empty) |> ignore

        if projectIn.IsBranch then
            for branch in projectIn.BranchResources do
                uploadProperty(branch)
        else
            uploadProperty(projectIn)

        true
            

    interface VSSonarPlugins.ISonarRestService with
        member this.CancelRequest() =
            cancelRequest <- true

        member this.IndexServerResources(conf : ISonarConfiguration, project : Resource) =                   
            let url = "/api/resources/index?qualifiers=DIR,TRK,BRC&depth=-1&resource=" + project.Key
            getResourcesFromResponseContent(httpconnector.HttpSonarGetRequest(conf, url))
        
        member this.GetBlameLine(conf:ISonarConfiguration, key:string, line:int) = 
            let url = "/api/sources/scm?key=" + key.Trim() + "&commits_by_line=true&from=" + line.ToString() + "&to=" + line.ToString()
            let reply = httpconnector.HttpSonarGetRequest(conf, url)
            let data = ScmAnswer.Parse(reply)
            let blame = new BlameLine()
            let scmLine = data.Scm.[0]
            blame.Author <- scmLine.Strings.[1]
            blame.Date <- scmLine.DateTime
            blame.Email <- scmLine.Strings.[1]
            blame.Line <- scmLine.Number

            blame

        member this.ApplyPermissionTemplateToProject(conf:ISonarConfiguration, key:string, name:string) =
            let url = "/api/permissions/search_templates?q=" + name
            try
                let reply = httpconnector.HttpSonarGetRequest(conf, url)
                let data = TemplateSearchAnswer.Parse(reply)

                let template = data.PermissionTemplates |> Seq.find (fun c -> c.Name.Equals(name))
                let url = "/api/permissions/apply_template?projectKey=" + key.Trim() + "&templateId=" + template.Id
                let response = httpconnector.HttpSonarPostRequest(conf, url, Map.empty)
                if response.StatusCode <> Net.HttpStatusCode.NoContent then
                    "Failed to apply template id, please report issue: " + response.StatusCode.ToString() + " : " + response.Content 
                else
                    ""
            with
            | ex -> "Unable to apply or find template : " + name + " " + ex.Message                    
         
        member this.ProvisionProject(conf:ISonarConfiguration, key:string, name:string, branch:string) =
            let branchtogo = 
                if String.IsNullOrEmpty(branch.Trim()) then
                    ""
                else
                    "&branch=" + branch.Trim()

            let nametogo = 
                "&name=" + HttpUtility.UrlEncode(name.Trim())

            let keytogo =
                "?key=" + key.Trim()

            let url = "/api/projects/create" + keytogo + nametogo + branchtogo
            let response = httpconnector.HttpSonarPostRequest(conf, url, Map.empty)
            if response.StatusCode <> Net.HttpStatusCode.OK then
                "Failed to provision project: " + response.StatusCode.ToString() + " : " + response.Content 
            else
                ""
        
        member this.DeleteProfile(conf:ISonarConfiguration, profileKey : string) =
            let url = "/api/qualityprofiles/delete?profileKey=" + profileKey
            let response = httpconnector.HttpSonarPostRequest(conf, url, Map.empty)
            if response.StatusCode <> Net.HttpStatusCode.OK then
                "Failed delete profile: " + response.StatusCode.ToString()
            else
                ""

        member this.GetParentProfile(conf:ISonarConfiguration, profileKey : string) = 
            let url = "/api/qualityprofiles/inheritance?profileKey=" + profileKey
            let reply = httpconnector.HttpSonarGetRequest(conf, url)
            let data = JsonarProfileInheritance.Parse(reply)
            try
                data.Profile.Parent
            with
            | _ -> ""
                            
        member this.GetInstalledPlugins(conf:ISonarConfiguration) =
            let dicret = new System.Collections.Generic.Dictionary<string, string>()
            let url = "/api/plugins/installed"

            try
                let reply = httpconnector.HttpSonarGetRequest(conf, url)
                let data = PluginsMessage.Parse(reply)

                for plugin in data.Plugins do
                    dicret.Add(plugin.Name, plugin.Version.String.Value)

            with
            |_ -> ()

            dicret

        member this.AssignProfileToProject(conf:ISonarConfiguration, profileKey:string, projectKey:string) = 
            let url = "/api/qualityprofiles/add_project"
            let options = Map.empty.Add("profileKey", profileKey).Add("projectKey", projectKey)
            let response = httpconnector.HttpSonarPostRequest(conf, url, options)
            if response.StatusCode <> Net.HttpStatusCode.NoContent then
                "Failed change parent: " + response.StatusCode.ToString()
            else
                ""


        member this.ChangeParentProfile(conf:ISonarConfiguration, profileKey:string, parentKey:string) = 
            let url = "/api/qualityprofiles/change_parent?profileKey=" + profileKey + "&parentKey=" + parentKey
            let response = httpconnector.HttpSonarPostRequest(conf, url, Map.empty)
            if response.StatusCode <> Net.HttpStatusCode.OK then
                "Failed change parent: " + response.StatusCode.ToString()
            else
                ""

        member this.CopyProfile(conf:ISonarConfiguration, id:string, newName:string) = 
            let url = "/api/qualityprofiles/copy?fromKey=" + id + "&toName=" + HttpUtility.UrlEncode(newName)
            let response = httpconnector.HttpSonarPostRequest(conf, url, Map.empty)
            if response.StatusCode <> Net.HttpStatusCode.OK then
                ""
            else
                let data = CopyProfileAnswer.Parse(response.Content)
                let profileKey = data.Key
                profileKey

        member this.CreateRule(conf:ISonarConfiguration, rule:Rule, ruleTemplate:Rule) =
            let errorMessages = new System.Collections.Generic.List<string>()
            let url ="/api/rules/create"

            let optionalProps =
                if conf.SonarVersion < 5.1 then
                    Map.empty.Add("custom_key", rule.Key.Replace( rule.Repo + ":", ""))
                                             .Add("html_description", rule.HtmlDescription)
                                             .Add("name", rule.Name)
                                             .Add("severity", rule.Severity.ToString())
                                             .Add("template_key", ruleTemplate.Key)
                else
                    Map.empty.Add("custom_key", rule.Key.Replace( rule.Repo + ":", ""))
                                             .Add("html_description", rule.HtmlDescription)
                                             .Add("markdown_description", rule.MarkDownDescription)                                             
                                             .Add("name", rule.Name)
                                             .Add("severity", rule.Severity.ToString())
                                             .Add("template_key", ruleTemplate.Key)
                                                                 
            let errorMessages = new System.Collections.Generic.List<string>()
            let response = httpconnector.HttpSonarPostRequest(conf, url, optionalProps)
            if response.StatusCode <> Net.HttpStatusCode.OK then
                let errors = JsonErrorMessage.Parse(response.Content)
                for error in errors.Errors do
                    errorMessages.Add(error.Msg)
            
            errorMessages

        member this.GetTemplateRules(conf:ISonarConfiguration, profile:Profile) =
            let rules = new System.Collections.Generic.List<Rule>()
            let url ="/api/rules/search?is_template=true&qprofile=" + HttpUtility.UrlEncode(profile.Key) + "&languages=" + HttpUtility.UrlEncode(profile.Language)
            let reply = httpconnector.HttpSonarGetRequest(conf, url)                        
            let rules = JsonRuleSearchResponse.Parse(reply)

            GetRulesFromSearchQuery(rules.Rules, profile, true)

                        
        member this.ActivateRule(conf:ISonarConfiguration, ruleKey:string, severity:string, profilekey:string) =
            let errorMessages = new System.Collections.Generic.List<string>()
            let url ="/api/qualityprofiles/activate_rule"

            let optionalProps = Map.empty.Add("profile_key", HttpUtility.UrlEncode(profilekey))
                                         .Add("rule_key", ruleKey)
                                         .Add("severity", severity.ToString())

            let errorMessages = new System.Collections.Generic.List<string>()
            let response = httpconnector.HttpSonarPostRequest(conf, url, optionalProps)
            if response.StatusCode <> Net.HttpStatusCode.OK then
                let errors = JsonErrorMessage.Parse(response.Content)
                for error in errors.Errors do
                    errorMessages.Add(error.Msg)
            
            errorMessages

        member this.DeleteRule(conf:ISonarConfiguration, rule:Rule) =
            let errorMessages = new System.Collections.Generic.List<string>()
            let url ="/api/rules/delete"

            let optionalProps = Map.empty.Add("key", rule.Key)

            let errorMessages = new System.Collections.Generic.List<string>()
            let response = httpconnector.HttpSonarPostRequest(conf, url, optionalProps)
            if response.StatusCode <> Net.HttpStatusCode.OK then
                let errors = JsonErrorMessage.Parse(response.Content)
                for error in errors.Errors do
                    errorMessages.Add(error.Msg)
            
            errorMessages

        member this.DisableRule(conf:ISonarConfiguration, rule:Rule, profilekey:string) =
            let errorMessages = new System.Collections.Generic.List<string>()
            let url ="/api/qualityprofiles/deactivate_rule"

            let optionalProps = Map.empty.Add("profile_key", HttpUtility.UrlEncode(profilekey))
                                         .Add("rule_key", rule.Key)

            let errorMessages = new System.Collections.Generic.List<string>()
            let response = httpconnector.HttpSonarPostRequest(conf, url, optionalProps)
            if response.StatusCode <> Net.HttpStatusCode.OK then
                let errors = JsonErrorMessage.Parse(response.Content)
                for error in errors.Errors do
                    errorMessages.Add(error.Msg)
            
            errorMessages
            
        member this.UpdateTags(conf:ISonarConfiguration, rule:Rule, tags:System.Collections.Generic.List<string>) =
            let errorMessages = new System.Collections.Generic.List<string>()
                    
            let settags = new System.Collections.Generic.HashSet<string>()
            let dic = new System.Collections.Generic.Dictionary<string, string>()
            let mutable newtags = ""
            for tag in tags do
                if not(settags.Contains(tag)) then
                    newtags <- newtags + tag + ","
                    settags.Add(tag) |> ignore

            newtags <- newtags.Trim(',')
            dic.Add("tags", newtags)

            (this :> ISonarRestService).UpdateRule(conf, rule.Key, dic)

        member this.GetAllTags(conf:ISonarConfiguration) =
            let tags = System.Collections.Generic.List<string>()
            let url ="/api/rules/tags"
            let response = httpconnector.HttpSonarGetRequest(conf, url)
            let tagsP = JsonTags.Parse(response)

            for tag in tagsP.Tags do
                tags.Add(tag)

            tags

        member this.UpdateRule(conf:ISonarConfiguration, key:string, optionalProps:System.Collections.Generic.Dictionary<string, string>) = 
            let url ="/api/rules/update?key=" + HttpUtility.UrlEncode(key)
            let errorMessages = new System.Collections.Generic.List<string>()
            let response = httpconnector.HttpSonarPostRequestDic(conf, url, optionalProps)
            if response.StatusCode <> Net.HttpStatusCode.OK then
                let errors = JsonErrorMessage.Parse(response.Content)
                for error in errors.Errors do
                    errorMessages.Add(error.Msg)
            
            errorMessages

        member this.UpdateRuleData(conf:ISonarConfiguration, newRule : Rule) = 
            let url = 
                if newRule.Key.StartsWith(newRule.Repo + ":") then
                    "/api/rules/search?rule_key=" + HttpUtility.UrlEncode(newRule.Key) + "&facets=types"
                else
                    "/api/rules/search?rule_key=" + HttpUtility.UrlEncode(newRule.Repo) + ":" + HttpUtility.UrlEncode(newRule.Key) + "&facets=types"
            try
                System.Diagnostics.Debug.WriteLine(url)
                let reply = httpconnector.HttpSonarGetRequest(conf, url)
                let rules = JsonRuleSearchResponse.Parse(reply)
                if rules.Total = 1 then
                    newRule.DefaultDebtRemFnType <- RemediationFunction.UNDEFINED
                    newRule.DefaultDebtChar <- Category.UNDEFINED
                    newRule.DefaultDebtSubChar <- SubCategory.UNDEFINED
                    // update values except for severity, since this is the default severity
                    UpdateRuleInProfile(rules.Rules.[0], newRule, true, conf, null)

                    try
                        for facet in rules.Facets do
                            if facet.Values.[0].Val <> "NONE" then
                                newRule.Subcategory <- try (EnumHelper.asEnum<SubCategory>(facet.Values.[0].Val)).Value with | ex -> SubCategory.UNDEFINED                                            
                                newRule.Category <- try (EnumHelper.asEnum<Category>(facet.Values.[1].Val)).Value with | ex -> Category.UNDEFINED                                        
                    with
                    | ex -> ()

                    newRule.IsParamsRetrivedFromServer <- true
            with
            | ex -> System.Diagnostics.Debug.WriteLine("FAILED: " + url + " : ", ex.Message)

        member this.GetRulesForProfile(conf:ISonarConfiguration, profile:Profile, searchData:bool) = 
            if profile <> null then
                if conf.SonarVersion < 5.2 then
                    let url = "/api/profiles/index?language=" + HttpUtility.UrlEncode(profile.Language) + "&name=" + HttpUtility.UrlEncode(profile.Name)
                    System.Diagnostics.Debug.WriteLine(url)
                    let reply = httpconnector.HttpSonarGetRequest(conf, url)
                    let data = JsonProfileAfter44.Parse(reply)
                    let rules = data.[0].Rules
                    for rule in rules do
                        let newRule = new Rule()
                        newRule.Key <- rule.Key
                        newRule.Repo <- rule.Repo
                        match rule.Params with
                        | NotNull ->
                            for para in rule.Params do
                                let param = new RuleParam()
                                param.Key <- para.Key
                                param.DefaultValue <- para.Value.ToString()
                                param.Value <- para.Value.ToString()
                                let isFound = (List.ofSeq newRule.Params) |> List.tryFind (fun c -> c.Key.Equals(para.Key))
                                match isFound with
                                | Some elem -> ()
                                | _ -> newRule.Params.Add(param)

                        | _ -> ()

                        newRule.Severity <- (EnumHelper.asEnum<Severity>(rule.Severity)).Value

                        if searchData then
                            let url = "/api/rules/search?rule_key=" + HttpUtility.UrlEncode(rule.Repo + ":" + rule.Key)
                            try
                                System.Diagnostics.Debug.WriteLine(url)
                                let reply = httpconnector.HttpSonarGetRequest(conf, url)
                                let rules = JsonRuleSearchResponse.Parse(reply)
                                if rules.Total = 1 then
                                    // update values except for severity, since this is the default severity
                                    UpdateRuleInProfile(rules.Rules.[0], newRule, true, conf, null)
                            with
                            | ex -> System.Diagnostics.Debug.WriteLine("FAILED: " + url + " : ", ex.Message)

                        try
                            profile.AddRule(newRule)
                        with
                        | ex -> System.Diagnostics.Debug.WriteLine("Cannot Add Rule: " + newRule.ConfigKey + " ex: " + ex.Message)
                else
                    let rec GetRules(page:int) = 
                        let url = sprintf "/api/rules/search?activation=true&qprofile=%s&p=%i" (HttpUtility.UrlEncode(profile.Key)) page
                        let reply = httpconnector.HttpSonarGetRequest(conf, url)
                        let rules = JsonRuleSearchResponse.Parse(reply)

                        let CreateRuleInProfile(rule:JsonRuleSearchResponse.Rule) =
                            let newRule = new Rule()
                            UpdateRuleInProfile(rule, newRule, false, conf, profile)
                            profile.AddRule(newRule)

                        rules.Rules |> Seq.iter (fun rule -> CreateRuleInProfile(rule))

                        if rules.Rules.Length = rules.Ps then
                            GetRules(page + 1)

                    GetRules(1)

                    
        member this.GetRuleUsingProfileAppId(conf:ISonarConfiguration, key:string) = 
                let url = "/api/rules/search?&rule_key=" + HttpUtility.UrlEncode(key)
                let reply = httpconnector.HttpSonarGetRequest(conf, url)
                let rules = JsonRuleSearchResponse.Parse(reply)
                if rules.Total = 1 then
                    let profile = new Profile(this :> ISonarRestService, conf)
                    CreateRuleInProfile(rules.Rules.[0], profile, false)                
                    profile.GetRule(key)
                else
                    null

        member this.GetRulesForProfileUsingRulesApp(conf:ISonarConfiguration , profile:Profile, active:bool) = 
            if profile <> null then
                let getActivation() =
                    if active then
                        "&activation=true" 
                    else
                        "&activation=false" 

                let url = "/api/rules/search?ps=200&qprofile=" + HttpUtility.UrlEncode(profile.Key) + getActivation() + "&languages=" + HttpUtility.UrlEncode(profile.Language)
                let reply = httpconnector.HttpSonarGetRequest(conf, url)                        
                let rules = JsonRuleSearchResponse.Parse(reply)
                let numberOfPages = rules.Total / rules.Ps + 1

                GetRulesFromSearchQuery(rules.Rules, profile, active)
                

                for i = 2 to numberOfPages do
                    let url = "/api/rules/search?ps=200&qprofile=" + HttpUtility.UrlEncode(profile.Key) + getActivation() + "&p=" + Convert.ToString(i)
                    let reply = httpconnector.HttpSonarGetRequest(conf, url)
                    let rules = JsonRuleSearchResponse.Parse(reply)
                    GetRulesFromSearchQuery(rules.Rules, profile, active)
                 
                ()

        member this.GetProfilesUsingRulesApp(conf : ISonarConfiguration) = 
            let profiles = new System.Collections.Generic.List<Profile>()
            try
                let reply = httpconnector.HttpSonarGetRequest(conf, "/api/rules/app")
                let data = JsonInternalData.Parse(reply)
                for profile in data.Qualityprofiles do
                    let newprofile = new Profile(this :> ISonarRestService, conf)
                    newprofile.Key <- profile.Key
                    newprofile.Language <- profile.Lang
                    newprofile.Name <- profile.Name
                    profiles.Add(newprofile)
            with
            | ex -> ()

            profiles

        member this.GetAvailableProfiles(conf : ISonarConfiguration) = 
            let profiles = new System.Collections.Generic.List<Profile>()
            let reply = httpconnector.HttpSonarGetRequest(conf, "/api/profiles/list")
            let data = JsonQualityProfiles.Parse(reply)
            for profile in data do
                let newprofile = new Profile(this :> ISonarRestService, conf)
                newprofile.Language <- profile.Language
                newprofile.Name <- profile.Name
                newprofile.Default <- profile.Default
                profiles.Add(newprofile)

            profiles

        member this.GetProjects(newConf:ISonarConfiguration) = 
            let projects = new System.Collections.Generic.List<SonarProject>()
            let reply = httpconnector.HttpSonarGetRequest(newConf, "/api/projects/index")
            let serverProjects = JsonProjectIndex.Parse(reply)
            for project in serverProjects do
                let projectToAdd = new SonarProject()
                projectToAdd.Name <- project.Nm
                projectToAdd.Qualifier <- project.Qu
                projectToAdd.Scope <- project.Sc
                projectToAdd.Key <- project.K
                projectToAdd.Id <- project.Id
                projects.Add(projectToAdd)

            projects

        member this.GetIssues(newConf : ISonarConfiguration, query : string, project : string) = 
            let url =  "/api/issues/search" + query + "&additionalFields=comments&pageSize=200"
            let oldurlreview = "/api/reviews?projects="+ project
            let oldurlviolations = "/api/violations?resource="+ project + "&depth=-1"
            getIssuesOldAndNewVersions(newConf, url, oldurlreview, oldurlviolations)

        member this.GetIssuesByAssigneeInProject(newConf : ISonarConfiguration, project : string, login : string) = 
            let url =  "/api/issues/search?componentRoots=" + project + "&assignees="+ login+ "&pageSize=200&statuses=OPEN,CONFIRMED,REOPENED"
            let oldurl = "/api/reviews?projects=" + project + "&assignees="+ login
            getIssuesOldAndNewVersions(newConf, url, oldurl, "")
   
        member this.GetAllIssuesByAssignee(newConf : ISonarConfiguration, login : string) = 
            let url =  "/api/issues/search?assignees="+ login + "&pageSize=200&statuses=OPEN,CONFIRMED,REOPENED"
            let oldurl = "/api/reviews?assignees="+ login
            getIssuesOldAndNewVersions(newConf, url, oldurl, "")

        member this.GetIssuesForProjectsCreatedAfterDate(newConf : ISonarConfiguration, project : string, date : DateTime) =
            let url =  "/api/issues/search?componentRoots=" + project + "&pageSize=200&createdAfter=" + Convert.ToString(date.Year) + "-" + Convert.ToString(date.Month) + "-"  + Convert.ToString(date.Day) + "&statuses=OPEN,CONFIRMED,REOPENED"
            let oldurl = "/api/reviews?projects="+ project
            getIssuesOldAndNewVersions(newConf, url, oldurl, "")

        member this.GetIssuesForProjects(newConf : ISonarConfiguration, project : string) =
            let url =  "/api/issues/search?componentRoots=" + project + "&pageSize=200&statuses=OPEN,CONFIRMED,REOPENED"
            let oldurlreview = "/api/reviews?projects="+ project
            let oldurlviolations = "/api/violations?resource="+ project + "&depth=-1"
            getIssuesOldAndNewVersions(newConf, url, oldurlreview, oldurlviolations)

        member this.GetIssuesInResource(conf : ISonarConfiguration, resource : string) =
            getViolationsOldAndNewFormat(conf, resource)

        member this.SetIssueTags(conf : ISonarConfiguration, issue : Issue, tags : System.Collections.Generic.List<string>) =
            let url = "/api/issues/set_tags"
            let tags = List.ofSeq tags |> String.concat ","
            try
                let parameters = Map.empty.Add("key", issue.Key).Add("tags", tags)
                let reply = httpconnector.HttpSonarPostRequest(conf, url, parameters)
                reply.StatusCode.ToString()
            with
            | ex -> "Error : Cannot set tag: " + ex.Message

        member this.GetAvailableTags(newConf : ISonarConfiguration) =
            let url = "/api/issues/tags?ps=1000"
            let listOfTags = new System.Collections.Generic.List<string>();
            try
                let responsecontent = httpconnector.HttpSonarGetRequest(newConf, url)
                let tags = JsonTags.Parse(responsecontent)
                for tag in tags.Tags do
                    listOfTags.Add(tag)
            with
            | _ -> ()

            listOfTags

        member this.GetUserList(newConf : ISonarConfiguration) =
            let url = "/api/users/search?ps=1000"           
            try
                let responsecontent = httpconnector.HttpSonarGetRequest(newConf, url)
                getUserListFromXmlResponse(responsecontent)
            with
             | ex -> new System.Collections.Generic.List<User>()

        member this.GetProperties(newConf : ISonarConfiguration, project : Resource) =
            let url = "/api/properties?resource=" + project.Key

            let responsecontent = httpconnector.HttpSonarGetRequest(newConf, url)
            let data = JSonProperties.Parse(responsecontent)
            let dic = new System.Collections.Generic.Dictionary<string, string>()
            for i in data do
                try
                    dic.Add(i.Key, i.Value.JsonValue.InnerText())
                with
                | ex -> ()

            dic

        member this.GetProperties(newConf : ISonarConfiguration) =
            let url = "/api/properties"           

            let responsecontent = httpconnector.HttpSonarGetRequest(newConf, url)
            let data = JSonProperties.Parse(responsecontent)
            let dic = new System.Collections.Generic.Dictionary<string, string>()
            for i in data do
                try
                    dic.Add(i.Key, i.Value.JsonValue.InnerText())
                with
                | ex -> ()

            dic                                

        member this.AuthenticateUser(newConf : ISonarConfiguration) =
            let url = "/api/authentication/validate"

            try
                let responsecontent = httpconnector.HttpSonarGetRequest(newConf, url)
                JsonValidateUser.Parse(responsecontent).Valid
            with
                | ex -> false

        member this.SearchComponent(conf : ISonarConfiguration, searchString : string, filterBranchs : bool, masterBranch : string) = 
            let url = "/api/components/search?qualifiers=DIR,TRK,PAC,CLA,BRC&q=" + searchString
            

            let getComponentFromResponse(data : string) = 
                let resources = new System.Collections.Generic.List<Resource>()
                let components = ComponentSearchAnswer.Parse(data)
                for comp in components.Components do
                    let newRes = new Resource()
                    newRes.Name <- comp.Name
                    newRes.Key <- comp.Key
                    newRes.Qualifier <- comp.Qualifier

                    if filterBranchs then
                        if comp.Name.Contains(" ") then
                            let lastElem = comp.Name.Split(' ')
                            let branchName = lastElem.[lastElem.Length - 1]
                            if not(comp.Key.EndsWith(":" + branchName)) || branchName = masterBranch then
                                resources.Add(newRes)
                        else
                            resources.Add(newRes)
                    else
                        resources.Add(newRes)

                // we need to get all pages
                let value = int(System.Math.Ceiling(float(components.Paging.Total) / float(components.Paging.PageSize)))

                for i = 2 to value do
                    if not(cancelRequest) then
                        let url = url + "&pageIndex=" + Convert.ToString(i)
                        let response = httpconnector.HttpSonarGetRequest(conf, url)
                        let components = ComponentSearchAnswer.Parse(data)
                        for comp in components.Components do
                            let newRes = new Resource()
                            newRes.Name <- comp.Name
                            newRes.Key <- comp.Key
                            newRes.Qualifier <- comp.Qualifier
                            resources.Add(newRes)
                resources

            getComponentFromResponse(httpconnector.HttpSonarGetRequest(conf, url))

        member this.GetResourcesData(conf : ISonarConfiguration, resource : string) =
            if conf.SonarVersion >= 6.3 then
                let url = "/api/components/show?key=" + resource
                let response = httpconnector.HttpSonarGetRequest(conf, url)
                let componentData = JsonComponentShow.Parse(response)
                let resource = new Resource()
                resource.IdType <- componentData.Component.Id
                resource.Key <- componentData.Component.Key
                resource.Name <- componentData.Component.Name
                resource.Qualifier <- componentData.Component.Qualifier
                resource.Path <- componentData.Component.Path
                let resourcelist = System.Collections.Generic.List<Resource>()
                resourcelist.Add(resource)
                resourcelist
            else
                let url = "/api/resources?resource=" + resource
                getResourcesFromResponseContent(httpconnector.HttpSonarGetRequest(conf, url))

        member this.GetQualityProfile(conf : ISonarConfiguration, project : Resource) =
            let resource =
                if String.IsNullOrEmpty(project.BranchName) then
                    project.Key
                else
                    if not(project.Key.EndsWith(":" + project.BranchName))  then
                        project.Key + ":" + project.BranchName
                    else
                        project.Key

            let url = "/api/resources?resource=" + resource + "&metrics=profile"
            getResourcesFromResponseContent(httpconnector.HttpSonarGetRequest(conf, url))

        member this.GetQualityProfilesForProject(conf : ISonarConfiguration, project : Resource) = 
            let resource =
                if String.IsNullOrEmpty(project.BranchName) then
                    project.Key
                else
                    if not(project.Key.EndsWith(":" + project.BranchName))  then
                        project.Key + ":" + project.BranchName
                    else
                        project.Key
                    
            if conf.SonarVersion >= 5.2 then
                let url = "/api/qualityprofiles/search?projectKey=" + resource
                let response = httpconnector.HttpSonarGetRequest(conf, url)
                let profilesJson = JsonQualityProfiles63.Parse(response)
                let profiles = new System.Collections.Generic.List<Profile>()

                let AddProfile(profile:JsonQualityProfiles63.Profile) =
                    let newProfile = new Profile(this :> ISonarRestService, conf)
                    newProfile.Key <- profile.Key
                    newProfile.Name <- profile.Name
                    newProfile.Language <- profile.Language
                    profiles.Add(newProfile)

                profilesJson.Profiles |> Seq.iter (fun profile -> AddProfile(profile))

                profiles
            else
                let url = "/api/profiles/list?project=" + resource
                GetQualityProfilesFromContent(httpconnector.HttpSonarGetRequest(conf, url), conf, this :> ISonarRestService)
                        
        member this.GetQualityProfilesForProject(conf : ISonarConfiguration, project : Resource, language : string) = 
            let resource =
                if String.IsNullOrEmpty(project.BranchName) then
                    project.Key
                else
                    if not(project.Key.EndsWith(":" + project.BranchName))  then
                        project.Key + ":" + project.BranchName
                    else
                        project.Key

            let url = "/api/profiles/list?project=" + resource + "&language=" + HttpUtility.UrlEncode(language)
            GetQualityProfilesFromContent(httpconnector.HttpSonarGetRequest(conf, url), conf, this :> ISonarRestService)

        member this.GetProjectsList(conf : ISonarConfiguration) = 
            if conf.SonarVersion < 6.3 then
                let url = "/api/resources"
                getResourcesFromResponseContent(httpconnector.HttpSonarGetRequest(conf, url))
            else

                let resourcelist = System.Collections.Generic.List<Resource>()

                let rec GetComponentsRec(page:int) = 
                    let url = sprintf "/api/components/search?qualifiers=TRK&ps=100&p=%i" page
                    let response = httpconnector.HttpSonarGetRequest(conf, url)
                    let answer = JsonComponents.Parse(response)

                    let ProcessComponents(elem:JsonComponents.Component) =
                        let resource = new Resource()
                        resource.Key <- elem.Key
                        resource.Name <- elem.Name
                        resource.IdType <- elem.Id
                        resource.Qualifier <- elem.Qualifier
                        resourcelist.Add(resource)
                        ()

                    answer.Components |> Seq.iter (fun elem -> ProcessComponents(elem))

                    if answer.Paging.PageSize = answer.Components.Length then
                        GetComponentsRec(page + 1)

                GetComponentsRec(1)
                resourcelist



        member this.GetEnabledRulesInProfile(conf : ISonarConfiguration, language : string, profile : string) =
            let url = "/api/profiles?language=" + HttpUtility.UrlEncode(language) + "&name=" + HttpUtility.UrlEncode(profile)
            GetProfileFromContent(httpconnector.HttpSonarGetRequest(conf, url), conf, this :> ISonarRestService)

        member this.GetRules(conf : ISonarConfiguration, language : string) = 
            let GetLanguageUrl =
                if language = "" then
                    ""
                else
                    "?language=" + HttpUtility.UrlEncode(language)

            let url = "/api/rules" + GetLanguageUrl
            getRulesFromResponseContent(httpconnector.HttpSonarGetRequest(conf, url))

        member this.GetServerInfo(conf : ISonarConfiguration) = 
            let url = "/api/server/version"
            let responsecontent = httpconnector.HttpSonarGetRequest(conf, url)
            let versionstr = responsecontent
            let elems = versionstr.Split('.')
            float32 (elems.[0] + "." + Regex.Replace(elems.[1], @"[^\d]", ""))
            
        member this.GetSourceForFileResource(conf : ISonarConfiguration, resource : string) =
            if conf.SonarVersion < 5.0 then
                GetSourceFromContent(httpconnector.HttpSonarGetRequest(conf, "/api/sources?resource=" + resource))
            else
                GetSourceFromRaw(httpconnector.HttpSonarGetRequest(conf, "/api/sources/raw?key=" + resource))

        member this.GetCoverageInResource(conf : ISonarConfiguration, resource : string) =
            let url = "/api/resources?resource=" + resource + "&metrics=coverage_line_hits_data,conditions_by_line,covered_conditions_by_line";
            GetCoverageFromContent(httpconnector.HttpSonarGetRequest(conf, url))

        member this.GetDuplicationsDataInResource(conf : ISonarConfiguration, resource : string) =
            let url = "/api/resources?resource=" + resource + "&metrics=duplications_data&depth=-1";
            GetDuplicationsFromContent(httpconnector.HttpSonarGetRequest(conf, url))

        member this.CommentOnIssues(newConf : ISonarConfiguration, issues : System.Collections.IList, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()

            for issueobj in issues do
                let issue = issueobj :?> Issue
                let mutable idstr = Convert.ToString(issue.Key)
                let parameters = Map.empty.Add("issue", idstr).Add("text", comment)

                let AddCommentFromResponse(issuesToCheck : System.Collections.Generic.List<Issue>) = 
                    for commentNew in issuesToCheck.[0].Comments do
                        let mutable found = false
                        for issueExist in issue.Comments do
                            if issueExist.HtmlText = commentNew.HtmlText then
                                found <- true

                        if not found then
                            issue.Comments.Add(commentNew)

                let mutable response = httpconnector.HttpSonarPostRequest(newConf, "/api/issues/add_comment", parameters)
                if response.StatusCode <> Net.HttpStatusCode.OK then
                    idstr <- Convert.ToString(issue.Id)
                    if issue.Comments.Count > 0 then
                        let parameters = Map.empty.Add("id", Convert.ToString(issue.Comments.[0].Id)).Add("comment", comment)
                        response <- httpconnector.HttpSonarPutRequest(newConf, "/api/reviews/add_comment", parameters)
                    else
                        let url = "/api/reviews?violation_id=" + Convert.ToString(idstr) + "&status=OPEN" + GetComment(comment)
                        response <- httpconnector.HttpSonarRequest(newConf, url, Method.POST)

                try
                    let comment = JSonComment.Parse(response.Content)
                    let commentToAdd = new Comment()
                    commentToAdd.CreatedAt <- comment.Comment.CreatedAt
                    commentToAdd.HtmlText <- comment.Comment.HtmlText
                    commentToAdd.Key <- comment.Comment.Key
                    commentToAdd.Login <- comment.Comment.Login
                    issue.Comments.Add(commentToAdd)
                with
                    | ex -> AddCommentFromResponse(getReviewsFromString(response.Content))

                try
                    if not(responseMap.ContainsKey(idstr)) then
                        responseMap.Add(idstr, response.StatusCode) |> ignore
                    else
                        responseMap.Add(idstr + "-dup", response.StatusCode) |> ignore
                with
                    | ex -> ()
                ()

            responseMap

            
        member this.MarkIssuesAsWontFix(newConf : ISonarConfiguration, issues : System.Collections.IList, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()

            for issueobj in issues do
                let issue = issueobj :?> Issue
                if issue.Status <> IssueStatus.RESOLVED then
                    let mutable idstr = issue.Key.ToString()
                    let mutable status = DoStateTransition(newConf, issue, IssueStatus.RESOLVED, "wontfix")
                    if status = Net.HttpStatusCode.OK then
                        if not(String.IsNullOrEmpty(comment)) then
                            (this :> ISonarRestService).CommentOnIssues(newConf, issues, comment) |> ignore
                    else
                        if not(idstr.Equals("0")) then
                            idstr <- Convert.ToString(issue.Id)
                            let CheckReponse(response : IRestResponse)= 
                                if response.StatusCode = Net.HttpStatusCode.OK then
                                    let reviews = getReviewsFromString(response.Content)
                                    issue.Id <- reviews.[0].Id
                                    issue.Status <- reviews.[0].Status
                                    issue.Resolution <- Resolution.WONTFIX
                                    let newComment = new Comment()
                                    newComment.CreatedAt <- DateTime.Now
                                    newComment.HtmlText <- comment
                                    newComment.Login <- newConf.Username
                                    issue.Comments.Add(newComment)

                            if issue.Comments.Count > 0 then
                                let parameters = Map.empty.Add("id", idstr).Add("resolution", "WONTFIX").Add("comment", comment)
                                let response = httpconnector.HttpSonarPutRequest(newConf, "/api/reviews/resolve", parameters)
                                status <- response.StatusCode
                                CheckReponse(response)
                            else
                                let url = "/api/reviews?violation_id=" + Convert.ToString(idstr) + "&status=RESOLVED&resolution=WONTFIX" + GetComment(comment)
                                let response = httpconnector.HttpSonarRequest(newConf, url, Method.POST)
                                CheckReponse(response)
                                status <- response.StatusCode

                    responseMap.Add(idstr, status)

            responseMap


        member this.MarkIssuesAsFalsePositive(newConf : ISonarConfiguration, issues : System.Collections.IList, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()

            for issueobj in issues do
                let issue = issueobj :?> Issue
                if issue.Status <> IssueStatus.RESOLVED then
                    let mutable idstr = issue.Key.ToString()
                    let mutable status = DoStateTransition(newConf, issue, IssueStatus.RESOLVED, "falsepositive")
                    if status = Net.HttpStatusCode.OK then
                        if not(String.IsNullOrEmpty(comment)) then
                            (this :> ISonarRestService).CommentOnIssues(newConf, issues, comment) |> ignore
                    else
                        if not(idstr.Equals("0")) then
                            idstr <- Convert.ToString(issue.Id)
                            let CheckReponse(response : IRestResponse)= 
                                if response.StatusCode = Net.HttpStatusCode.OK then
                                    let reviews = getReviewsFromString(response.Content)
                                    issue.Id <- reviews.[0].Id
                                    issue.Status <- reviews.[0].Status
                                    issue.Resolution <- Resolution.FALSE_POSITIVE
                                    let newComment = new Comment()
                                    newComment.CreatedAt <- DateTime.Now
                                    newComment.HtmlText <- comment
                                    newComment.Login <- newConf.Username
                                    issue.Comments.Add(newComment)

                            if issue.Comments.Count > 0 then
                                let parameters = Map.empty.Add("id", idstr).Add("resolution", "FALSE-POSITIVE").Add("comment", comment)
                                let response = httpconnector.HttpSonarPutRequest(newConf, "/api/reviews/resolve", parameters)
                                status <- response.StatusCode
                                CheckReponse(response)
                            else
                                let url = "/api/reviews?violation_id=" + Convert.ToString(idstr) + "&status=RESOLVED&resolution=FALSE-POSITIVE" + GetComment(comment)
                                let response = httpconnector.HttpSonarRequest(newConf, url, Method.POST)
                                CheckReponse(response)
                                status <- response.StatusCode

                    responseMap.Add(idstr, status)

            responseMap

        member this.PlanIssues(newConf : ISonarConfiguration, issues : System.Collections.IList, planid : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()
            for issueobj in issues do
                let issue = issueobj :?> Issue
                let parameters = Map.empty.Add("issue", issue.Key.ToString()).Add("plan", planid)
                let data = httpconnector.HttpSonarPostRequest(newConf, "/api/issues/plan", parameters)
                responseMap.Add(issue.Key.ToString(), data.StatusCode)

            responseMap


        member this.UnPlanIssues(newConf : ISonarConfiguration, issues : System.Collections.IList) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()
            for issueobj in issues do
                let issue = issueobj :?> Issue
                let parameters = Map.empty.Add("issue", issue.Key.ToString())
                let data = httpconnector.HttpSonarPostRequest(newConf, "/api/issues/plan", parameters)
                responseMap.Add(issue.Key.ToString(), data.StatusCode)

            responseMap
                        
        member this.ResolveIssues(newConf : ISonarConfiguration, issues : System.Collections.IList, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()

            for issueobj in issues do
                let issue = issueobj :?> Issue
                if issue.Status <> IssueStatus.RESOLVED then
                    let mutable idstr = issue.Key.ToString()
                    let mutable status = Net.HttpStatusCode.OK
                    status <- DoStateTransition(newConf, issue, IssueStatus.RESOLVED, "resolve")

                    if status = Net.HttpStatusCode.OK then
                        if not(String.IsNullOrEmpty(comment)) then
                            (this :> ISonarRestService).CommentOnIssues(newConf, issues, comment) |> ignore
                    else
                        idstr <- Convert.ToString(issue.Id)
                        let CheckReponse(response : IRestResponse)= 
                            if response.StatusCode = Net.HttpStatusCode.OK then
                                let reviews = getReviewsFromString(response.Content)
                                issue.Id <- reviews.[0].Id
                                issue.Status <- reviews.[0].Status
                                issue.Resolution <- Resolution.FIXED
                                let newComment = new Comment()
                                newComment.CreatedAt <- DateTime.Now
                                newComment.HtmlText <- comment
                                newComment.Login <- newConf.Username
                                issue.Comments.Add(newComment)

                        if issue.Comments.Count > 0 then
                            let parameters = Map.empty.Add("id", idstr).Add("resolution", "FIXED").Add("comment", comment)
                            let response = httpconnector.HttpSonarPutRequest(newConf, "/api/reviews/resolve", parameters)
                            status <- response.StatusCode
                            CheckReponse(response)
                        else
                            let url = "/api/reviews?violation_id=" + Convert.ToString(idstr) + "&status=RESOLVED&resolution=FIXED" + GetComment(comment)
                            let response = httpconnector.HttpSonarRequest(newConf, url, Method.POST)
                            CheckReponse(response)
                            status <- response.StatusCode

                    responseMap.Add(idstr, status)

            responseMap

        member this.ReOpenIssues(newConf : ISonarConfiguration, issues : System.Collections.Generic.List<Issue>, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()
            for issue in issues do
                if issue.Status <> IssueStatus.REOPENED || issue.Status <> IssueStatus.OPEN then
                    let mutable idstr = issue.Key.ToString()
                    let mutable status = Net.HttpStatusCode.OK
                    status <- DoStateTransition(newConf, issue, IssueStatus.REOPENED, "reopen")
                    if status = Net.HttpStatusCode.OK then
                        if not(String.IsNullOrEmpty(comment)) then
                            (this :> ISonarRestService).CommentOnIssues(newConf, issues, comment) |> ignore
                    else
                        idstr <- Convert.ToString(issue.Id)
                        let CheckReponse(response : IRestResponse)= 
                            if response.StatusCode = Net.HttpStatusCode.OK then
                                let reviews = getReviewsFromString(response.Content)
                                issue.Id <- reviews.[0].Id
                                issue.Status <- reviews.[0].Status
                                let newComment = new Comment()
                                newComment.CreatedAt <- DateTime.Now
                                newComment.HtmlText <- comment
                                newComment.Login <- newConf.Username
                                issue.Comments.Add(newComment)

                        let parameters = Map.empty.Add("id", idstr).Add("comment", comment)
                        let response = httpconnector.HttpSonarPutRequest(newConf, "/api/reviews/reopen", parameters)
                        CheckReponse(response)
                        status <- response.StatusCode
                        
                    responseMap.Add(idstr, status)
            responseMap

        member this.UnConfirmIssues(newConf : ISonarConfiguration, issues : System.Collections.IList, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()
            for issueobj in issues do
                let issue = issueobj :?> Issue
                responseMap.Add(issue.Key.ToString(), DoStateTransition(newConf, issue, IssueStatus.REOPENED, "unconfirm"))
            if not(String.IsNullOrEmpty(comment)) then
                (this :> ISonarRestService).CommentOnIssues(newConf, issues, comment) |> ignore                                
            responseMap

        member this.ConfirmIssues(newConf : ISonarConfiguration, issues : System.Collections.IList, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()
            for issueobj in issues do
                let issue = issueobj :?> Issue
                if issue.Status.Equals(IssueStatus.RESOLVED) then
                    DoStateTransition(newConf, issue, IssueStatus.REOPENED, "reopen") |> ignore

                responseMap.Add(issue.Key.ToString(), DoStateTransition(newConf, issue, IssueStatus.CONFIRMED, "confirm"))                

            if not(String.IsNullOrEmpty(comment)) then
                (this :> ISonarRestService).CommentOnIssues(newConf, issues, comment) |> ignore
            responseMap

        member this.AssignIssuesToUser(newConf : ISonarConfiguration, issues : System.Collections.Generic.List<Issue>, user : User, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()
            for issue in issues do
                if not(String.IsNullOrEmpty(user.Login)) then
                    let parameters = Map.empty.Add("issue", issue.Key.ToString()).Add("assignee", user.Login)
                    let data = httpconnector.HttpSonarPostRequest(newConf, "/api/issues/assign", parameters)
                    responseMap.Add(issue.Key.ToString(), data.StatusCode)
                    if data.StatusCode = Net.HttpStatusCode.OK then
                        issue.Assignee <- user.Login
                else
                    let parameters = Map.empty.Add("issue", issue.Key.ToString())
                    let data = httpconnector.HttpSonarPostRequest(newConf, "/api/issues/assign", parameters)
                    if data.StatusCode = Net.HttpStatusCode.OK then
                        issue.Assignee <- ""
                    responseMap.Add(issue.Key.ToString(), data.StatusCode)
                
            if not(String.IsNullOrEmpty(comment)) then
                (this :> ISonarRestService).CommentOnIssues(newConf, issues, comment) |> ignore

            responseMap

        member this.ParseReportOfIssuesOld(path : string) =

            let txt = File.ReadAllText(path)
            let issuesInFile = JSonIssuesOld.Parse(File.ReadAllText(path))
            let currentListOfIssues = new System.Collections.Generic.List<Issue>()

            let CreateIssue(resource : string, elem : JsonValue) =
                let issue = new Issue()
                let message = elem.GetProperty("message")
                let severity = elem.GetProperty("severity")
                let rule_repository = elem.GetProperty("rule_repository")
                let rule_key = elem.GetProperty("rule_key")
                let rule_name = elem.GetProperty("rule_name")

                issue.Component <- resource
                issue.Line <- 0
                issue.Message <- message.AsString()
                issue.Rule <- rule_key.AsString()
                issue.Severity <- (EnumHelper.asEnum<Severity>(severity.AsString())).Value
                currentListOfIssues.Add(issue)

            let ProcessViolation(str : string, elem : JsonValue) =
                let resource = str
                let data = elem
                elem.AsArray() |> Seq.iter (fun elem -> CreateIssue(resource, elem))
                ()

            let ProcessElem(str : string, elem : JsonValue) =
                
                if str.Equals("violations_per_resource") then
                    elem.Properties |> Seq.iter (fun elem -> ProcessViolation(elem))
                ()

            issuesInFile.JsonValue.Properties |> Seq.iter (fun elem -> ProcessElem(elem))

            currentListOfIssues                       
                            

        member this.ParseDryRunReportOfIssues(path : string) =

            let txt = File.ReadAllText(path)
            let issuesInFile = JSonIssuesDryRun.Parse(File.ReadAllText(path))

            let currentListOfIssues = new System.Collections.Generic.List<Issue>()

            let CreateIssue(resource : string, elem : JsonValue) =                
                let issue = new Issue()
                let message = elem.GetProperty("message")
                let severity = elem.GetProperty("severity")
                let rule_repository = elem.GetProperty("rule_repository")
                let rule_key = elem.GetProperty("rule_key")
                let created_at = elem.GetProperty("created_at")
                let is_new = elem.GetProperty("is_new")

                issue.Component <- resource
                issue.Line <- 0
                issue.Message <- message.AsString()
                issue.Rule <- rule_key.AsString()
                issue.Severity <- (EnumHelper.asEnum<Severity>(severity.AsString())).Value
                issue.CreationDate <- created_at.AsDateTime()
                currentListOfIssues.Add(issue)

            let ProcessViolation(str : string, elem : JsonValue) =
                let resource = str
                let data = elem
                elem.AsArray() |> Seq.iter (fun elem -> CreateIssue(resource, elem))
                ()

            let ProcessElem(str : string, elem : JsonValue) =
                
                if str.Equals("violations_per_resource") then
                    elem.Properties |> Seq.iter (fun elem -> ProcessViolation(elem))
                ()

            issuesInFile.JsonValue.Properties |> Seq.iter (fun elem -> ProcessElem(elem))

            currentListOfIssues 



        member this.ParseReportOfIssues(path : string) =
            let issuesInFile = JSonIssues.Parse(File.ReadAllText(path))
            let currentListOfIssues = new System.Collections.Generic.List<Issue>()

            let ConvertIssue(elem : JSonIssues.Issue ) =
                let issue = new Issue()
                if not(obj.ReferenceEquals(elem.Component, null)) then
                    issue.Component <- elem.Component.Value
                if not(obj.ReferenceEquals(elem.CreationDate, null)) then
                    issue.CreationDate <- elem.CreationDate.Value
                if not(obj.ReferenceEquals(elem.Key, null)) && not(obj.ReferenceEquals(elem.Key.Guid, null)) then
                    issue.Key <- elem.Key.Guid.Value.ToString()
                if not(obj.ReferenceEquals(elem.Line, null)) then
                    issue.Line <- elem.Line.Value
                if not(obj.ReferenceEquals(elem.Message, null)) then
                    issue.Message <- elem.Message.Value
                if not(obj.ReferenceEquals(elem.Rule, null)) then
                    issue.Rule <- elem.Rule
                if not(obj.ReferenceEquals(elem.Severity, null)) then
                    issue.Severity <- (EnumHelper.asEnum<Severity>(elem.Severity.Value)).Value
                if not(obj.ReferenceEquals(elem.UpdateDate, null)) then
                    issue.UpdateDate <- elem.UpdateDate.Value
                if not(obj.ReferenceEquals(elem.IsNew, null)) then
                    issue.IsNew <- elem.IsNew.Value

                issue

            issuesInFile.Issues |> Seq.iter (fun elem -> currentListOfIssues.Add(ConvertIssue(elem)))            
            currentListOfIssues          

        member this.IgnoreAllFile(conf : ISonarConfiguration, projectIn : Resource, file : string) =
            let mutable ok = true

            let epoch = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString().Replace(",", "")
            let dummyRule = new Rule(Key = "*")
            (this :> ISonarRestService).IgnoreRuleOnFile(conf, projectIn, file, dummyRule)


        member this.GetExclusions(conf : ISonarConfiguration, projectIn : Resource) =
            let mutable ok = true
            let exclusions = new System.Collections.Generic.List<Exclusion>()

            let uploadProperty(currentproject : Resource) =

                let properties = (this :> ISonarRestService).GetProperties(conf, currentproject)

                // upload new epochs
                let AddExclusionToList(ruleKey : string, fileKey : string) =
                    let resourceKeyElem = List.ofSeq exclusions |> Seq.tryFind (fun c -> c.RuleRegx.Equals(ruleKey) && c.FileRegx.Equals(fileKey))
                    match resourceKeyElem with
                    | Some(m) -> ()
                    | _ -> exclusions.Add(new Exclusion(RuleRegx = ruleKey, FileRegx = fileKey))
                    
                let AddExclusion(key : string, value : string) =
                    if key.EndsWith(".ruleKey") && key.StartsWith("sonar.issue.ignore.multicriteria") then
                        let id = key.Replace("sonar.issue.ignore.multicriteria.", "").Replace(".ruleKey", "")
                        let ruleRegex = value
                        let resourceKeyElem = properties |> Seq.tryFind (fun c -> c.Key.Equals("sonar.issue.ignore.multicriteria." + id + ".resourceKey"))
                        match resourceKeyElem with
                        | Some(m) -> AddExclusionToList(ruleRegex, m.Value)
                        | _ -> ()

                properties |> Seq.iter (fun c -> AddExclusion(c.Key, c.Value))


            if projectIn.IsBranch then
                for branch in projectIn.BranchResources do
                    uploadProperty(branch)
            else
                uploadProperty(projectIn)


            exclusions :> System.Collections.Generic.IList<Exclusion>


        member this.UpdateProperty(conf : ISonarConfiguration, id : string, value : string, projectIn : Resource) = 
            let resourcestr = 
                if projectIn = null then
                    ""
                else
                    sprintf "&resource=%s" projectIn.Key

            let url = sprintf "/api/properties?id=%s&value=%s%s" id (HttpUtility.UrlEncode(value)) resourcestr
            let response = httpconnector.HttpSonarPostRequest(conf, url, Map.empty)
            if response.StatusCode <> Net.HttpStatusCode.OK then
                "Failed update property: " + response.StatusCode.ToString()
            else
                ""
                

        member this.IgnoreRuleOnFile(conf : ISonarConfiguration, projectIn : Resource, file : string, rule : Rule) =

            let exclusions = new System.Collections.Generic.List<Exclusion>()

            let epoch = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString().Replace(",", "")

            let uploadProperty(currentproject : Resource) =

                let properties = (this :> ISonarRestService).GetProperties(conf, currentproject)

                let getPropertiesForUpload =                                 
                    let elem = properties |> Seq.tryFind (fun c -> c.Key.Equals("sonar.issue.ignore.multicriteria"))
                    match elem with
                    | Some(c) -> c.Value + "," + epoch
                    | _ -> epoch

                // upload new epochs
                let url = sprintf "/api/properties?id=sonar.issue.ignore.multicriteria.%s.ruleKey&value=%s&resource=%s" epoch rule.Key currentproject.Key
                let response = httpconnector.HttpSonarPostRequest(conf, url, Map.empty)
                if response.StatusCode = Net.HttpStatusCode.OK then
                    let url = sprintf "/api/properties?id=sonar.issue.ignore.multicriteria.%s.resourceKey&value=%s&resource=%s" epoch file currentproject.Key
                    let response = httpconnector.HttpSonarPostRequest(conf, url, Map.empty)
                    if response.StatusCode = Net.HttpStatusCode.OK then
                        let url = sprintf "/api/properties?id=sonar.issue.ignore.multicriteria&value=%s&resource=%s" getPropertiesForUpload currentproject.Key
                        httpconnector.HttpSonarPostRequest(conf, url, Map.empty) |> ignore

                let AddExclusionToList(ruleKey : string, fileKey : string) =
                    let resourceKeyElem = List.ofSeq exclusions |> Seq.tryFind (fun c -> c.RuleRegx.Equals(ruleKey) && c.FileRegx.Equals(fileKey))
                    match resourceKeyElem with
                    | Some(m) -> ()
                    | _ -> exclusions.Add(new Exclusion(RuleRegx = ruleKey, FileRegx = fileKey))
                    
                let AddExclusion(key : string, value : string) =
                    if key.EndsWith(".ruleKey") && key.StartsWith("sonar.issue.ignore.multicriteria") then
                        let id = key.Replace("sonar.issue.ignore.multicriteria.", "").Replace(".ruleKey", "")
                        let ruleRegex = value
                        let resourceKeyElem = properties |> Seq.tryFind (fun c -> c.Key.Equals("sonar.issue.ignore.multicriteria." + id + ".resourceKey"))
                        match resourceKeyElem with
                        | Some(m) -> AddExclusionToList(ruleRegex, m.Value)
                        | _ -> ()

                properties |> Seq.iter (fun c -> AddExclusion(c.Key, c.Value))


            if projectIn.IsBranch then
                for branch in projectIn.BranchResources do
                    uploadProperty(branch)
            else
                uploadProperty(projectIn)

            let foundAlready = List.ofSeq exclusions |> Seq.tryFind (fun  c -> c.RuleRegx.Equals(rule.Key) && c.FileRegx.Equals(file))
            match foundAlready with
            | Some(c) -> ()
            | _ -> exclusions.Add(new Exclusion(RuleRegx = rule.Key, FileRegx = file))

            exclusions :> System.Collections.Generic.IList<Exclusion>
