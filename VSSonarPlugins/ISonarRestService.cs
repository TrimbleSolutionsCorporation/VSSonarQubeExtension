namespace VSSonarPlugins
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
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
        /// <param name="token">The token.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>
        /// returns list of issues
        /// </returns>
        Task<List<Issue>> GetIssuesByAssigneeInProject(ISonarConfiguration config, string projectKey, string userId, CancellationToken token, ILogManager logger);

        /// <summary>
        /// Gets all issues by assignee.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="token">The token.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>task list of issues</returns>
        Task<List<Issue>> GetAllIssuesByAssignee(ISonarConfiguration config, string userId, CancellationToken token, ILogManager logger);

        /// <summary>
        /// Gets the issues for projects.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="projectKey">The project key.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        Task<List<Issue>> GetIssuesForProjects(ISonarConfiguration config, string projectKey, CancellationToken token, ILogManager logger);

        /// <summary>
        /// Gets the issues for projects created after date.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="projectKey">The project key.</param>
        /// <param name="time">The time.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        Task<List<Issue>> GetIssuesForProjectsCreatedAfterDate(ISonarConfiguration config, string projectKey, DateTime time, CancellationToken token, ILogManager logger);

        /// <summary>
        /// Gets the issues in resource.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        Task<List<Issue>> GetIssuesInResource(ISonarConfiguration config, string resourceKey, CancellationToken token, ILogManager logger);

        /// <summary>
        /// Gets the issues.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="query">The query.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        Task<List<Issue>> GetIssues(ISonarConfiguration config, string query, string projectId, CancellationToken token, ILogManager logger);

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
        /// do not confirm issues.
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
        /// Marks the issues as false positive.
        /// </summary>
        /// <param name="newConf">The new conf.</param>
        /// <param name="issues">The issues.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        Dictionary<string, HttpStatusCode> MarkIssuesAsWontFix(ISonarConfiguration newConf, IList issues, string comment);
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
        /// unplan issues.
        /// </summary>
        /// <param name="newConf">The new conf.</param>
        /// <param name="issues">The issues.</param>
        /// <returns>status of operation</returns>
        Dictionary<string, HttpStatusCode> UnPlanIssues(ISonarConfiguration newConf, IList issues);

        /// <summary>
        /// Gets the user list.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <returns>users</returns>
        List<User> GetUserList(ISonarConfiguration conf);

        /// <summary>
        /// Gets the available tags.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="token">The token.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>
        /// tags
        /// </returns>
        Task<List<string>> GetAvailableTags(ISonarConfiguration conf, CancellationToken token, ILogManager logger);

        /// <summary>
        /// Sets the issue tags.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="issue">The issue.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="token">The token.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>
        /// status of operation
        /// </returns>
        Task<string> SetIssueTags(
            ISonarConfiguration conf,
            Issue issue,
            List<string> tags,
            CancellationToken token,
            ILogManager logger);

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <returns>ok for auth</returns>
        bool AuthenticateUser(ISonarConfiguration conf);

        /// <summary>
        /// Gets the resources data.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        List<Resource> GetResourcesData(ISonarConfiguration conf, string key);

        /// <summary>
        /// Searches the component.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="searchString">The search string.</param>
        /// <param name="filterBranches">if set to <c>true</c> [filter branches].</param>
        /// <returns></returns>
        List<Resource> SearchComponent(ISonarConfiguration conf, string searchString, bool filterBranches, string masterBranch);

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
        [Obsolete("GetProperties deprecated after sonarqube 6.3, use GetSettings")]
        Dictionary<string, string> GetProperties(ISonarConfiguration props);

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="props">The props.</param>
        /// <param name="project">The project, in can its null returns server properties.</param>
        /// <returns>
        /// Settings for project or server
        /// </returns>
        IEnumerable<Setting> GetSettings(ISonarConfiguration props, Resource project = null);

        /// <summary>
        /// Sets the setting.
        /// </summary>
        /// <param name="props">The props.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="project">The project.</param>
        /// <returns>status of the operation</returns>
        string SetSetting(ISonarConfiguration props, Setting setting, Resource project = null);

        /// <summary>
        /// Creates the version.
        /// </summary>
        /// <param name="props">The props.</param>
        /// <param name="project">The project.</param>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        string CreateVersion(ISonarConfiguration props, Resource project, string version, DateTime date);

        /// <summary>
        /// Gets the coverage report on new code on leak.
        /// </summary>
        /// <param name="props">The props.</param>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        Dictionary<string, CoverageDifferencial> GetCoverageReportOnNewCodeOnLeak(ISonarConfiguration props, Resource project, ILogManager logger);

        /// <summary>
        /// Gets the coverage report.
        /// </summary>
        /// <param name="props">The props.</param>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        Dictionary<string, CoverageReport> GetCoverageReport(ISonarConfiguration props, Resource project);

        /// <summary>
        /// Updates the property.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="projectIn">The project in, if null it will update global property</param>
        /// <returns>empty string if OK, error message if fails</returns>
        string UpdateProperty(ISonarConfiguration conf, string id, string value, Resource projectIn);

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="props">The props.</param>
        /// <param name="project">The project.</param>
        /// <returns>
        /// list of properties for project
        /// </returns>
        [Obsolete("GetProperties deprecated after sonarqube 6.3, use GetSettings")]
        Dictionary<string, string> GetProperties(ISonarConfiguration props, Resource project);

        /// <summary>
        /// Gets the enabled rules in profile.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="language">The language.</param>
        /// <param name="profile">The profile.</param>
        /// <returns></returns>
        List<Profile> GetEnabledRulesInProfile(ISonarConfiguration conf, string language, string profile);

        /// <summary>
        /// Gets the quality profile.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <returns></returns>
        List<Resource> GetQualityProfile(ISonarConfiguration conf, Resource project);

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
        /// <param name="profilekey">The profile key.</param>
        /// <returns></returns>
        List<string> ActivateRule(ISonarConfiguration conf, string ruleKey, string severity, string profilekey);

        List<string> DeleteRule(ISonarConfiguration conf, Rule rule);

        List<string> DisableRule(ISonarConfiguration conf, Rule rule, string profilekey);

        List<string> CreateRule(ISonarConfiguration conf, Rule rule, Rule templateRule);

        // TODO: might be remove in the future
        List<Profile> GetProfilesUsingRulesApp(ISonarConfiguration conf);

        void GetRulesForProfileUsingRulesApp(ISonarConfiguration conf, Profile profile, bool active);

        Rule GetRuleUsingProfileAppId(ISonarConfiguration conf, string ruleKey);

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

        /// <summary>
        /// Applies the permission template to project.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="projectKey">The project key.</param>
        /// <param name="templateName">Name of the template.</param>
        /// <returns>returns error message or empty if ok</returns>
        string ApplyPermissionTemplateToProject(ISonarConfiguration conf, string projectKey, string templateName);

        /// <summary>
        /// Gets the blame line.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="key">The key.</param>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        BlameLine GetBlameLine(ISonarConfiguration conf, string key, int line);

        /// <summary>
        /// Indexes the server resources.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        List<Resource> IndexServerResources(ISonarConfiguration conf, Resource project);
    }
}