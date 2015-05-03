namespace VSSonarPlugins
{
    using System;
    using System.Collections.Generic;
    using VSSonarPlugins.Types;
    using System.Net;
    using System.Collections;

    public interface ISonarRestService
    {
        List<Issue> GetIssuesByAssigneeInProject(ISonarConfiguration config, string projectKey, string userId);
        List<Issue> GetAllIssuesByAssignee(ISonarConfiguration config, string userId);
        List<Issue> GetIssuesForProjects(ISonarConfiguration config, string projectKey);
        List<Issue> GetIssuesForProjectsCreatedAfterDate(ISonarConfiguration config, string projectKey, DateTime time);
        List<Issue> GetIssuesInResource(ISonarConfiguration config, string resourceKey);
        List<Issue> GetIssues(ISonarConfiguration config, string query, string projectId);
        List<SonarProject> GetProjects(ISonarConfiguration config);

        Dictionary<string, HttpStatusCode> CommentOnIssues(ISonarConfiguration newConf, IList issues, string comment);
        Dictionary<string, HttpStatusCode> ReOpenIssues(ISonarConfiguration Conf, List<Issue> issues, string comment);
        Dictionary<string, HttpStatusCode> ConfirmIssues(ISonarConfiguration newConf, IList issues, string comment);
        Dictionary<string, HttpStatusCode> UnConfirmIssues(ISonarConfiguration newConf, IList issues, string comment);
        Dictionary<string, HttpStatusCode> MarkIssuesAsFalsePositive(ISonarConfiguration newConf, IList issues, string comment);
        Dictionary<string, HttpStatusCode> ResolveIssues(ISonarConfiguration newConf, IList issues, string comment);
        Dictionary<string, HttpStatusCode> AssignIssuesToUser(ISonarConfiguration newConf, List<Issue> issues, User user, string comment);

        List<User> GetUserList(ISonarConfiguration conf);
        bool AuthenticateUser(ISonarConfiguration conf);
        List<Resource> GetResourcesData(ISonarConfiguration conf, string key);
        List<Resource> GetProjectsList(ISonarConfiguration conf);

        float GetServerInfo(ISonarConfiguration conf);
        SourceCoverage GetCoverageInResource(ISonarConfiguration conf, string key);
        Source GetSourceForFileResource(ISonarConfiguration conf, string key);   
        Dictionary<string, string> GetProperties(ISonarConfiguration props);

        List<Profile> GetEnabledRulesInProfile(ISonarConfiguration conf, string language, string profile);
        List<Resource> GetQualityProfile(ISonarConfiguration conf, string resourceKey);
        void GetRulesForProfile(ISonarConfiguration conf, Profile profile, bool ruleDetails, bool active);
        void GetRulesForProfile(ISonarConfiguration conf, Profile profile, bool searchDetails);
        void UpdateRuleData(ISonarConfiguration conf, Rule newRule);

        List<Profile> GetQualityProfilesForProject(ISonarConfiguration conf, string projectKey);
        List<Profile> GetQualityProfilesForProject(ISonarConfiguration conf, string projectKey, string language);
        List<Profile> GetAvailableProfiles(ISonarConfiguration conf);

        List<Rule> GetRules(ISonarConfiguration conf, string languangeKey);
        void GetTemplateRules(ISonarConfiguration conf, Profile profile);
        List<string> UpdateRule(ISonarConfiguration conf, string key, Dictionary<string, string> optionalProps);
        List<string> GetAllTags(ISonarConfiguration conf);
        List<string> UpdateTags(ISonarConfiguration conf, Rule rule, List<string> tags);
        List<string> ActivateRule(ISonarConfiguration conf, Rule rule, string profilekey);
        List<string> DeleteRule(ISonarConfiguration conf, Rule rule);
        List<string> DisableRule(ISonarConfiguration conf, Rule rule, string profilekey);
        List<string> CreateRule(ISonarConfiguration conf, Rule rule, Rule templateRule);

        // might be remove in the future
        List<Profile> GetProfilesUsingRulesApp(ISonarConfiguration conf);
        void GetRulesForProfileUsingRulesApp(ISonarConfiguration conf, Profile profile, bool active);
        Rule GetRuleUsingProfileAppId(ISonarConfiguration conf, string ruleKey);

        List<Issue> ParseReportOfIssues(string path);
        List<Issue> ParseReportOfIssuesOld(string path);
        List<Issue> ParseDryRunReportOfIssues(string path);

        List<DuplicationData> GetDuplicationsDataInResource(ISonarConfiguration conf, string resourceKey);
    }
}
