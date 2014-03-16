// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarRestService.fs" company="Trimble Navigation Limited">
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

open FSharp.Data
open FSharp.Data.Json
open FSharp.Data.Json.Extensions
open RestSharp
open ExtensionTypes
open System.Collections.ObjectModel
open System
open System.Web
open System.Net
open System.IO
open System.Text.RegularExpressions

open SonarRestService

type SonarRestService(httpconnector : IHttpSonarConnector) = 
    let httpconnector = httpconnector

    let GetSeverity(value : string) =
        let mutable sev = "1 : INFO"
        if value.Equals("MINOR") then
            sev <- "2 : MINOR"
        if value.Equals("MAJOR") then
            sev <- "3 : MAJOR"
        if value.Equals("CRITICAL") then
            sev <- "4 : CRITICAL"
        if value.Equals("BLOCKER") then
            sev <- "5 : BLOCKER"
        sev
                    
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

            if not(obj.ReferenceEquals(user.Name, null)) then
                newUser.Name <- user.Name
            else
                newUser.Name <- ""

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
        issue.Status <- data.Status
        issue.Severity <- GetSeverity(data.Severity)
        issue.Rule <- data.Rule
        issue.Key <- data.Key

        if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("assignee"), null)) then
            issue.Assignee <- data.Assignee

        if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("comments"), null)) then
            for elemC in data.Comments do issue.Comments.Add(new Comment(elemC.CreatedAt, elemC.HtmlText, elemC.Key, elemC.Login, -1))
            
        if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("effortToFix"), null)) then
            let itemValue = data.JsonValue.Item("effortToFix")
            match itemValue with
            | decimal -> issue.EffortToFix <- itemValue.AsDecimal()

        if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("closeDate"), null)) then
            issue.CloseDate <- data.CloseDate

        if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("resolution"), null)) then
            issue.Resolution <- data.Resolution

        issue
                        
    let getIssuesFromString(responsecontent : string) =
        let data = JsonIssues.Parse(responsecontent)
        let issueList = new System.Collections.Generic.List<Issue>()
        for elem in data.Issues do
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
            issue.Status <- elem.Status
            issue.Severity <- GetSeverity(elem.Severity)
            issue.Rule <- elem.Rule
            issue.Key <- elem.Key

            if not(obj.ReferenceEquals(elem.Assignee, null)) then
                issue.Assignee <- elem.Assignee.Value
            
            if not(obj.ReferenceEquals(elem.Comments, null)) then
                for elemC in elem.Comments.Value do issue.Comments.Add(new Comment(elemC.CreatedAt, elemC.HtmlText, elemC.Key, elemC.Login, -1))

            if not(obj.ReferenceEquals(elem.CloseDate, null)) then
                issue.CloseDate <- elem.CloseDate.Value

            if not(obj.ReferenceEquals(elem.Resolution, null)) then
                issue.Resolution <- elem.Resolution.Value

            if not(obj.ReferenceEquals(elem.EffortToFix, null)) then
                let itemValue = elem.JsonValue.Item("effortToFix")
                issue.EffortToFix <- 0.0m
                match itemValue with
                | decimal -> issue.EffortToFix <- itemValue.AsDecimal()

            issueList.Add(issue)

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
            issue.Status <- "OPEN"

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
            issue.Status <- elem.Status
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
                for elemC in elem.Comments do issue.Comments.Add(new Comment(elemC.UpdatedAt, elemC.Text, new Guid(), elemC.Author, elem.Id))
            | _ -> ()

            issueList.Add(issue)

        issueList

    let getIssuesOldAndNewVersions(userConf, newurl : string, oldurlreviews: string, oldurlviolations) = 
        try
            let allIssues = new System.Collections.Generic.List<Issue>()
            let responsecontent = httpconnector.HttpSonarGetRequest(userConf, newurl)
            let data = JsonIssues.Parse(responsecontent)

            let AddElements(all : System.Collections.Generic.List<Issue>) = 
                for issue in all do
                    allIssues.Add(issue)

            AddElements(getIssuesFromString(responsecontent))

            // we need to get all pages
            
            for i = 2 to data.Paging.Pages do
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
            let url =  "/api/issues/search?components=" + resource
            let responsecontent = httpconnector.HttpSonarGetRequest(userConf, url)
            getIssuesFromString(responsecontent)
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

                        if not(obj.ReferenceEquals(metric.FrmtVal.Number, null)) then
                            met.FormatedValue <- sprintf "%f" metric.FrmtVal.Number.Value
                        else
                            met.FormatedValue <- sprintf "%s" metric.FrmtVal.String.Value

                        match metric.JsonValue.TryGetProperty("val") with
                        | NotNull ->
                            met.Val <- metric.Val
                        | _ -> ()

                        metrics.Add(met)

                        res.Metrics <- metrics


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
            rule.Description <- ruleInServer.Description
            rule.Key <- ruleInServer.Key
            rule.Severity <- ruleInServer.Priority
            rule.Repo <- ruleInServer.Plugin
            rulesToReturn.Add(rule)

        rulesToReturn
        

    let GetProfileFromContent(responsecontent : string) = 
        let parsed = JSonProfile.Parse(responsecontent)
        let profiles = new System.Collections.Generic.List<Profile>()

        for eachprofile in parsed do
            let newProfile = new Profile()
            newProfile.Default <- eachprofile.Default
            newProfile.Language <- eachprofile.Language
            newProfile.Name <- eachprofile.Name

            let profileRules = new System.Collections.Generic.List<Rule>()
            let profileAlerts = new System.Collections.Generic.List<Alert>()

            for eachrule in eachprofile.Rules do
                let newRule = new Rule()
                newRule.Key <- eachrule.Key
                newRule.Repo <- eachrule.Repo
                newRule.Severity <- GetSeverity(eachrule.Severity)
                profileRules.Add(newRule)

            newProfile.Rules <- profileRules

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
            arrayOfLines.[Int32.Parse(data) - 1] <- elem.InnerText
            ()

        data.[0].Properties |> Seq.iter (fun elem -> CreateLine(elem))

        source.Lines <- arrayOfLines
        source

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
                            for group in data.GetGs() do
                                let groupToAdd = new DuplicatedGroup()
                                for block in group.GetBs() do
                                    let blockToAdd = new DuplicatedBlock()
                                    blockToAdd.Lenght <- block.L
                                    blockToAdd.Startline <- block.S
                                    blockToAdd.Resource <- new Resource(Key = block.R)
                                    groupToAdd.DuplicatedBlocks.Add(blockToAdd)
                                duplicatedResource.DuplicatedGroups.Add(groupToAdd)

                            duplicationData.Add(duplicatedResource))
                            

        duplicationData

    let GetCoverageFromContent(responsecontent : string) = 
        let resources = JsonResourceWithMetrics.Parse(responsecontent)

        let source = new SourceCoverage()

        try
            source.SetLineCoverageData(resources.[0].Msr.[0].Data.Value)
        with
            | ex -> ()

        try
            source.SetBranchCoverageData(resources.[0].Msr.[1].Data.Value, resources.[0].Msr.[2].Data.Value)
        with
            | ex -> ()

        source

    let QuerySonar(userconf : ConnectionConfiguration, urltosue : string, methodin : Method) =
        let client = new RestClient(userconf.Hostname)
        client.Authenticator <- new HttpBasicAuthenticator(userconf.Username, userconf.Password)
        let request = new RestRequest(urltosue, methodin)
        request.AddHeader(HttpRequestHeader.Accept.ToString(), "text/xml") |> ignore
        client.Execute(request)

    let PerformWorkFlowTransition(userconf : ConnectionConfiguration, issue : Issue, transition : string) =
        let parameters = Map.empty.Add("issue", issue.Key.ToString()).Add("transition", transition)
        httpconnector.HttpSonarPostRequest(userconf, "/api/issues/do_transition", parameters)

    let DoStateTransition(userconf : ConnectionConfiguration, issue : Issue, finalState : string, transition : string) = 
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

    interface ISonarRestService with
        member this.GetIssues(newConf : ConnectionConfiguration, query : string, project : string) = 
            let url =  "/api/issues/search" + query + "&pageSize=200"
            let oldurlreview = "/api/reviews?projects="+ project
            let oldurlviolations = "/api/violations?resource="+ project + "&depth=-1"
            getIssuesOldAndNewVersions(newConf, url, oldurlreview, oldurlviolations)

        member this.GetIssuesByAssigneeInProject(newConf : ConnectionConfiguration, project : string, login : string) = 
            let url =  "/api/issues/search?componentRoots=" + project + "&assignees="+ login
            let oldurl = "/api/reviews?projects=" + project + "&assignees="+ login
            getIssuesOldAndNewVersions(newConf, url, oldurl, "")
   
        member this.GetAllIssuesByAssignee(newConf : ConnectionConfiguration, login : string) = 
            let url =  "/api/issues/search?assignees="+ login
            let oldurl = "/api/reviews?assignees="+ login
            getIssuesOldAndNewVersions(newConf, url, oldurl, "")

        member this.GetIssuesForProjectsCreatedAfterDate(newConf : ConnectionConfiguration, project : string, date : DateTime) =
            let url =  "/api/issues/search?componentRoots=" + project + "&pageSize=200&createdAfter=" + Convert.ToString(date.Year) + "-" + Convert.ToString(date.Month) + "-"  + Convert.ToString(date.Day)
            let oldurl = "/api/reviews?projects="+ project
            getIssuesOldAndNewVersions(newConf, url, oldurl, "")

        member this.GetIssuesForProjects(newConf : ConnectionConfiguration, project : string) =
            let url =  "/api/issues/search?componentRoots=" + project + "&pageSize=200"
            let oldurlreview = "/api/reviews?projects="+ project
            let oldurlviolations = "/api/violations?resource="+ project + "&depth=-1"
            getIssuesOldAndNewVersions(newConf, url, oldurlreview, oldurlviolations)

        member this.GetIssuesInResource(conf : ConnectionConfiguration, resource : string) =
            getViolationsOldAndNewFormat(conf, resource)
        
        member this.GetUserList(newConf : ConnectionConfiguration) =
            let url = "/api/users/search"           
            try
                let responsecontent = httpconnector.HttpSonarGetRequest(newConf, url)
                getUserListFromXmlResponse(responsecontent)
            with
             | ex -> new System.Collections.Generic.List<User>()

        member this.GetProperties(newConf : ConnectionConfiguration) =
            let url = "/api/properties"           

            let responsecontent = httpconnector.HttpSonarGetRequest(newConf, url)
            let data = JSonProperties.Parse(responsecontent)
            let dic = new System.Collections.Generic.Dictionary<string, string>()
            for i in data do
                try
                    dic.Add(i.Key, i.Value.JsonValue.InnerText)
                with
                | ex -> ()

            dic                                

        member this.AuthenticateUser(newConf : ConnectionConfiguration) =
            let url = "/api/authentication/validate"

            if newConf.Username = "" && newConf.Password = "" then
                true
            else             
                let responsecontent = httpconnector.HttpSonarGetRequest(newConf, url)
                try
                    JsonValidateUser.Parse(responsecontent).Valid
                with
                    | ex -> true

        member this.GetResourcesData(conf : ConnectionConfiguration, resource : string) =
            let url = "/api/resources?resource=" + resource
            getResourcesFromResponseContent(httpconnector.HttpSonarGetRequest(conf, url))

        member this.GetQualityProfile(conf : ConnectionConfiguration, resource : string) =
            let url = "/api/resources?resource=" + resource + "&metrics=profile"
            getResourcesFromResponseContent(httpconnector.HttpSonarGetRequest(conf, url))
                        
        member this.GetProjectsList(conf : ConnectionConfiguration) =                   
            let url = "/api/resources"
            getResourcesFromResponseContent(httpconnector.HttpSonarGetRequest(conf, url))

        member this.GetEnabledRulesInProfile(conf : ConnectionConfiguration, language : string, profile : string) =
            let url = "/api/profiles?language=" + HttpUtility.UrlEncode(language) + "&name=" + HttpUtility.UrlEncode(profile)
            GetProfileFromContent(httpconnector.HttpSonarGetRequest(conf, url))

        member this.GetRules(conf : ConnectionConfiguration, language : string) = 
            let GetLanguageUrl =
                if language = "" then
                    ""
                else
                    "?language=" + HttpUtility.UrlEncode(language)

            let url = "/api/rules" + GetLanguageUrl
            getRulesFromResponseContent(httpconnector.HttpSonarGetRequest(conf, url))

        member this.GetServerInfo(conf : ConnectionConfiguration) = 
            let url = "/api/server"
            let responsecontent = httpconnector.HttpSonarGetRequest(conf, url)
            let versionstr = JSonServerInfo.Parse(responsecontent).Version.Replace(",", ".")
            let elems = versionstr.Split('.')
            float (elems.[0] + "." + Regex.Replace(elems.[1], @"[^\d]", ""))
            
        member this.GetSourceForFileResource(conf : ConnectionConfiguration, resource : string) =
            let url = "/api/sources?resource=" + resource
            GetSourceFromContent(httpconnector.HttpSonarGetRequest(conf, url))

        member this.GetCoverageInResource(conf : ConnectionConfiguration, resource : string) =
            let url = "/api/resources?resource=" + resource + "&metrics=coverage_line_hits_data,conditions_by_line,covered_conditions_by_line";
            GetCoverageFromContent(httpconnector.HttpSonarGetRequest(conf, url))

        member this.GetDuplicationsDataInResource(conf : ConnectionConfiguration, resource : string) =
            let url = "/api/resources?resource=" + resource + "&metrics=duplications_data&depth=-1";
            GetDuplicationsFromContent(httpconnector.HttpSonarGetRequest(conf, url))

        member this.CommentOnIssues(newConf : ConnectionConfiguration, issues : System.Collections.Generic.List<Issue>, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()

            for issue in issues do
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

                responseMap.Add(idstr, response.StatusCode) |> ignore
                ()

            responseMap

        member this.MarkIssuesAsFalsePositive(newConf : ConnectionConfiguration, issues : System.Collections.Generic.List<Issue>, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()

            for issue in issues do
                if issue.Status <> "RESOLVED" then
                    let mutable idstr = issue.Key.ToString()
                    let mutable status = DoStateTransition(newConf, issue, "RESOLVED", "falsepositive")
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
                                    issue.Resolution <- "FALSE-POSITIVE"
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

        member this.ResolveIssues(newConf : ConnectionConfiguration, issues : System.Collections.Generic.List<Issue>, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()

            for issue in issues do
                if issue.Status <> "RESOLVED" then
                    let mutable idstr = issue.Key.ToString()
                    let mutable status = Net.HttpStatusCode.OK
                    status <- DoStateTransition(newConf, issue, "RESOLVED", "resolve")

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
                                issue.Resolution <- "FIXED"
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

        member this.ReOpenIssues(newConf : ConnectionConfiguration, issues : System.Collections.Generic.List<Issue>, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()
            for issue in issues do
                if issue.Status <> "REOPENED" || issue.Status <> "OPEN" then
                    let mutable idstr = issue.Key.ToString()
                    let mutable status = Net.HttpStatusCode.OK
                    status <- DoStateTransition(newConf, issue, "REOPENED", "reopen")
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

        member this.UnConfirmIssues(newConf : ConnectionConfiguration, issues : System.Collections.Generic.List<Issue>, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()
            for issue in issues do
                responseMap.Add(issue.Key.ToString(), DoStateTransition(newConf, issue, "REOPENED", "unconfirm"))
            if not(String.IsNullOrEmpty(comment)) then
                (this :> ISonarRestService).CommentOnIssues(newConf, issues, comment) |> ignore                                
            responseMap

        member this.ConfirmIssues(newConf : ConnectionConfiguration, issues : System.Collections.Generic.List<Issue>, comment : string) =
            let responseMap = new System.Collections.Generic.Dictionary<string, Net.HttpStatusCode>()
            for issue in issues do
                responseMap.Add(issue.Key.ToString(), DoStateTransition(newConf, issue, "CONFIRMED", "confirm"))                

            if not(String.IsNullOrEmpty(comment)) then
                (this :> ISonarRestService).CommentOnIssues(newConf, issues, comment) |> ignore
            responseMap

        member this.AssignIssuesToUser(newConf : ConnectionConfiguration, issues : System.Collections.Generic.List<Issue>, user : User, comment : string) =
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
                issue.Severity <- severity.AsString()
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
                issue.Severity <- severity.AsString()
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

            let ConvertIssue(elem : JSonIssues.DomainTypes.Issue ) =
                let issue = new Issue()
                if not(obj.ReferenceEquals(elem.Component, null)) then
                    issue.Component <- elem.Component.Value
                if not(obj.ReferenceEquals(elem.CreationDate, null)) then
                    issue.CreationDate <- elem.CreationDate.Value
                if not(obj.ReferenceEquals(elem.Key, null)) && not(obj.ReferenceEquals(elem.Key.Guid, null)) then
                    issue.Key <- elem.Key.Guid.Value
                if not(obj.ReferenceEquals(elem.Line, null)) then
                    issue.Line <- elem.Line.Value
                if not(obj.ReferenceEquals(elem.Message, null)) then
                    issue.Message <- elem.Message.Value
                if not(obj.ReferenceEquals(elem.Rule, null)) then
                    issue.Rule <- elem.Rule
                if not(obj.ReferenceEquals(elem.Severity, null)) then
                    issue.Severity <- elem.Severity.Value
                if not(obj.ReferenceEquals(elem.UpdateDate, null)) then
                    issue.UpdateDate <- elem.UpdateDate.Value
                if not(obj.ReferenceEquals(elem.IsNew, null)) then
                    issue.IsNew <- elem.IsNew.Value

                issue

            issuesInFile.Issues |> Seq.iter (fun elem -> currentListOfIssues.Add(ConvertIssue(elem)))            
            currentListOfIssues          
