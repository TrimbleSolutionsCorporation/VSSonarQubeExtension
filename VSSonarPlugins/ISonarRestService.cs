namespace VSSonarPlugins
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using Types;

    /// <summary>
    /// sonar rest service
    /// </summary>
    public interface ISonarRestService
    {
        /// <summary>
        /// Gets the issues by assignee in project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="projectKey">The project key.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>returns list of issues</returns>
        List<Issue> GetIssuesByAssigneeInProject(ISonarConfiguration config, string projectKey, string userId);

        /// <summary>
        /// Gets all issues by assignee.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        List<Issue> GetAllIssuesByAssignee(ISonarConfiguration config, string userId);

        /// <summary>
        /// Gets the issues for projects.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="projectKey">The project key.</param>
        /// <returns></returns>
        List<Issue> GetIssuesForProjects(ISonarConfiguration config, string projectKey);

        /// <summary>
        /// Gets the issues for projects created after date.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="projectKey">The project key.</param>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        List<Issue> GetIssuesForProjectsCreatedAfterDate(ISonarConfiguration config, string projectKey, DateTime time);

        /// <summary>
        /// Gets the issues in resource.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <returns></returns>
        List<Issue> GetIssuesInResource(ISonarConfiguration config, string resourceKey);

        /// <summary>
        /// Gets the issues.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="query">The query.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        List<Issue> GetIssues(ISonarConfiguration config, string query, string projectId);

        /// <summary>
        /// Gets the projects.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns></returns>
        List<SonarProject> GetProjects(ISonarConfiguration config);

        /// <summary>
        /// Provisions the project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="key">The key.</param>
        /// <param name="name">The name.</param>
        /// <param name="branch">The branch.</param>
        /// <returns></returns>
        string ProvisionProject(ISonarConfiguration config, string key, string name, string branch);

        /// <summary>
        /// Comments the on issues.
        /// </summary>
        /// <param name="newConf">The new conf.</param>
        /// <param name="issues">The issues.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        Dictionary<string, HttpStatusCode> CommentOnIssues(ISonarConfiguration newConf, IList issues, string comment);

        /// <summary>
        /// Res the open issues.
        /// </summary>
        /// <param name="Conf">The conf.</param>
        /// <param name="issues">The issues.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        Dictionary<string, HttpStatusCode> ReOpenIssues(ISonarConfiguration Conf, List<Issue> issues, string comment);

        /// <summary>
        /// Confirms the issues.
        /// </summary>
        /// <param name="newConf">The new conf.</param>
        /// <param name="issues">The issues.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        Dictionary<string, HttpStatusCode> ConfirmIssues(ISonarConfiguration newConf, IList issues, string comment);

        /// <summary>
        /// Uns the confirm issues.
        /// </summary>
        /// <param name="newConf">The new conf.</param>
        /// <param name="issues">The issues.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        Dictionary<string, HttpStatusCode> UnConfirmIssues(ISonarConfiguration newConf, IList issues, string comment);

        /// <summary>
        /// Marks the issues as false positive.
        /// </summary>
        /// <param name="newConf">The new conf.</param>
        /// <param name="issues">The issues.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        Dictionary<string, HttpStatusCode> MarkIssuesAsFalsePositive(ISonarConfiguration newConf, IList issues, string comment);

        /// <summary>
        /// Resolves the issues.
        /// </summary>
        /// <param name="newConf">The new conf.</param>
        /// <param name="issues">The issues.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        Dictionary<string, HttpStatusCode> ResolveIssues(ISonarConfiguration newConf, IList issues, string comment);

        /// <summary>
        /// Assigns the issues to user.
        /// </summary>
        /// <param name="newConf">The new conf.</param>
        /// <param name="issues">The issues.</param>
        /// <param name="user">The user.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        Dictionary<string, HttpStatusCode> AssignIssuesToUser(ISonarConfiguration newConf, List<Issue> issues, User user, string comment);

        /// <summary>
        /// Plans the issues.
        /// </summary>
        /// <param name="newConf">The new conf.</param>
        /// <param name="issues">The issues.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>status code</returns>
        Dictionary<string, HttpStatusCode> PlanIssues(ISonarConfiguration newConf, IList issues, string planId);

        /// <summary>
        /// Creates the new plan.
        /// </summary>
        /// <param name="newConf">The new conf.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="plan">The plan.</param>
        /// <returns>
        /// created action plan
        /// </returns>
        SonarActionPlan CreateNewPlan(ISonarConfiguration newConf, string projectId, SonarActionPlan plan);

        /// <summary>
        /// Uns the plan issues.
        /// </summary>
        /// <param name="newConf">The new conf.</param>
        /// <param name="issues">The issues.</param>
        /// <returns>status of operation</returns>
        Dictionary<string, HttpStatusCode> UnPlanIssues(ISonarConfiguration newConf, IList issues);

        /// <summary>
        /// Gets the available action plan.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <returns>returns actions plans</returns>
        List<SonarActionPlan> GetAvailableActionPlan(ISonarConfiguration conf, string resourceKey);

        /// <summary>
        /// Gets the user list.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <returns></returns>
        List<User> GetUserList(ISonarConfiguration conf);

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <returns></returns>
        bool AuthenticateUser(ISonarConfiguration conf);

        /// <summary>
        /// Gets the resources data.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        List<Resource> GetResourcesData(ISonarConfiguration conf, string key);

        /// <summary>
        /// Gets the projects list.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <returns></returns>
        List<Resource> GetProjectsList(ISonarConfiguration conf);

        /// <summary>
        /// Gets the server information.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <returns></returns>
        float GetServerInfo(ISonarConfiguration conf);

        /// <summary>
        /// Gets the coverage in resource.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        SourceCoverage GetCoverageInResource(ISonarConfiguration conf, string key);

        /// <summary>
        /// Gets the source for file resource.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        Source GetSourceForFileResource(ISonarConfiguration conf, string key);

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="props">The props.</param>
        /// <returns>
        /// list of properties for server
        /// </returns>
        Dictionary<string, string> GetProperties(ISonarConfiguration props);

        /// <summary>
        /// Updates the property.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="projectIn">The project in, if null it will update global property</param>
        /// <returns>empty string if ok, error message if fails</returns>
        string UpdateProperty(ISonarConfiguration conf, string id, string value, Resource projectIn);

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="props">The props.</param>
        /// <param name="project">The project.</param>
        /// <returns>
        /// list of properties for project
        /// </returns>
        Dictionary<string, string> GetProperties(ISonarConfiguration props, Resource project);

        List<Profile> GetEnabledRulesInProfile(ISonarConfiguration conf, string language, string profile);

        /// <summary>
        /// Gets the quality profile.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <returns></returns>
        List<Resource> GetQualityProfile(ISonarConfiguration conf, Resource project);

        void GetRulesForProfile(ISonarConfiguration conf, Profile profile, bool ruleDetails, bool active);

        void GetRulesForProfile(ISonarConfiguration conf, Profile profile, bool searchDetails);

        void UpdateRuleData(ISonarConfiguration conf, Rule newRule);

        List<Profile> GetQualityProfilesForProject(ISonarConfiguration conf, Resource project);

        List<Profile> GetQualityProfilesForProject(ISonarConfiguration conf, Resource project, string language);

        List<Profile> GetAvailableProfiles(ISonarConfiguration conf);

        List<Rule> GetRules(ISonarConfiguration conf, string languangeKey);

        void GetTemplateRules(ISonarConfiguration conf, Profile profile);

        List<string> UpdateRule(ISonarConfiguration conf, string key, Dictionary<string, string> optionalProps);

        List<string> GetAllTags(ISonarConfiguration conf);

        List<string> UpdateTags(ISonarConfiguration conf, Rule rule, List<string> tags);

        /// <summary>
        /// Activates the rule.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="ruleKey">The rule key.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="profilekey">The profilekey.</param>
        /// <returns></returns>
        List<string> ActivateRule(ISonarConfiguration conf, string ruleKey, string severity, string profilekey);

        List<string> DeleteRule(ISonarConfiguration conf, Rule rule);

        List<string> DisableRule(ISonarConfiguration conf, Rule rule, string profilekey);

        List<string> CreateRule(ISonarConfiguration conf, Rule rule, Rule templateRule);

        // TODO: might be remove in the future
        List<Profile> GetProfilesUsingRulesApp(ISonarConfiguration conf);

        void GetRulesForProfileUsingRulesApp(ISonarConfiguration conf, Profile profile, bool active);

        Rule GetRuleUsingProfileAppId(ISonarConfiguration conf, string ruleKey);

        List<Issue> ParseReportOfIssues(string path);

        List<Issue> ParseReportOfIssuesOld(string path);

        List<Issue> ParseDryRunReportOfIssues(string path);

        List<DuplicationData> GetDuplicationsDataInResource(ISonarConfiguration conf, string resourceKey);

        /// <summary>
        /// Ignores all file.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="project">The project.</param>
        /// <param name="file">The file.</param>
        /// <returns>returns a list of exclusions</returns>
        IList<Exclusion> IgnoreAllFile(ISonarConfiguration conf, Resource project, string file);

        /// <summary>
        /// Ignores the rule on file.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="project">The project.</param>
        /// <param name="file">The file.</param>
        /// <param name="rule">The rule.</param>
        /// <returns>returns a list of exclusions</returns>
        IList<Exclusion> IgnoreRuleOnFile(ISonarConfiguration conf, Resource project, string file, Rule rule);

        /// <summary>
        /// Gets the exclusions.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="project">The project.</param>
        /// <returns>returns a list of exclusions</returns>
        IList<Exclusion> GetExclusions(ISonarConfiguration conf, Resource project);

        /// <summary>
        /// Copies the profile.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="newName">The new name.</param>
        /// <returns></returns>
        string CopyProfile(ISonarConfiguration conf, string id, string newName);

        /// <summary>
        /// Deletes the profile.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="profileKey">The profile key.</param>
        /// <returns></returns>
        string DeleteProfile(ISonarConfiguration conf, string profileKey);

        /// <summary>
        /// Changes the parent profile.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="profileKey">The profile key.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <returns></returns>
        string ChangeParentProfile(ISonarConfiguration conf, string profileKey, string parentKey);

        /// <summary>
        /// Gets the parent profile.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="profileKey">The profile key.</param>
        /// <returns></returns>
        string GetParentProfile(ISonarConfiguration conf, string profileKey);

        /// <summary>
        /// Assigns the profile to project.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="profileKey">The profile key.</param>
        /// <param name="projectKey">The project key.</param>
        /// <returns></returns>
        string AssignProfileToProject(ISonarConfiguration conf, string profileKey, string projectKey);

        /// <summary>
        /// Gets the installed plugins.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <returns></returns>
        Dictionary<string, string> GetInstalledPlugins(ISonarConfiguration conf);

        /// <summary>
        /// Cancels the request.
        /// </summary>
        void CancelRequest();
    }
}