// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalIds.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The context.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarPlugins.Types
{
    using System;
    using System.ComponentModel;

    /// <summary>
    ///     The context.
    /// </summary>
    [Serializable]
    public enum Context
    {
        /// <summary>The ui properties.</summary>
        [Description("UIProperties")]
        UIProperties, 

        /// <summary>The global props id.</summary>
        [Description("GlobalPropsId")]
        GlobalPropsId, 

        /// <summary>The analysis project. The properties in this context will not be sync to file</summary>
        [Description("AnalysisProjectId")]
        AnalysisProject, 

        /// <summary>The analysis general.</summary>
        [Description("AnalysisGeneral")]
        AnalysisGeneral, 

        /// <summary>The file analysis properties.</summary>
        [Description("FileAnalysisProperties")]
        FileAnalysisProperties, 

        /// <summary>The sq analysis properties.</summary>
        [Description("SQAnalysisProperties")]
        SQAnalysisProperties, 

        /// <summary>The menu plugin properties.</summary>
        [Description("MenuPluginProperties")]
        MenuPluginProperties,

        /// <summary>The issue tracker plugin properties.</summary>
        [Description("IssueTrackerProps")]
        IssueTrackerProps
    }

    /// <summary>
    /// The global ids.
    /// </summary>
    public static class GlobalIds
    {
        /// <summary>
        ///     The excluded plugins.
        /// </summary>
        public static readonly string PluginEnabledControlId = "PluginEnabled";

        /// <summary>The is connect at start on.</summary>
        public static readonly string IsConnectAtStartOn = "IsConnectAtStartOn";

        /// <summary>The user defined editor.</summary>
        public static readonly string UserDefinedEditor = "UserDefinedEditor";

        /// <summary>The extension debug mode enabled.</summary>
        public static readonly string ExtensionDebugModeEnabled = "ExtensionDebugModeEnabled";

        /// <summary>The disable editor tags.</summary>
        public static readonly string DisableEditorTags = "DisableEditorTags";

		/// <summary> server address string </summary>
		public static readonly string ServerAdress = "ServerAddress";

		/// <summary> user login string </summary>
		public static readonly string UserLogin = "UserLogin";
		
		/// <summary>
		/// Teams file
		/// </summary>
		public static readonly string TeamsFile = "TeamsFile";
	}

    /// <summary>
    /// The owners id.
    /// </summary>
    public static class OwnersId
    {
        /// <summary>The application owner id.</summary>
        public static readonly string ApplicationOwnerId = "ExtensionControl";

        /// <summary>The analysis owner id.</summary>
        public static readonly string AnalysisOwnerId = "AnalysisControl";

        /// <summary>The plugin general owner id.</summary>
        public static readonly string PluginGeneralOwnerId = "PluginGeneral";
    }

    /// <summary>
    /// The global analysis ids.
    /// </summary>
    public static class GlobalAnalysisIds
    {
        /// <summary>The excluded plugins key.</summary>
        public static readonly string ExcludedPluginsKey = "ExcludedPluginsKey";

        /// <summary>The runner executable key.</summary>
        public static readonly string RunnerExecutableKey = "RunnerExecutable";

        /// <summary>The java executable key.</summary>
        public static readonly string JavaExecutableKey = "JavaExecutable";

        /// <summary>The local analysis timeout key.</summary>
        public static readonly string LocalAnalysisTimeoutKey = "TimeoutAnalysis";

        /// <summary>The is debug analysis on key.</summary>
        public static readonly string IsDebugAnalysisOnKey = "IsDebugAnalysisOn";

        /// <summary>The sonar source key.</summary>
        public static readonly string SonarSourceKey = "SonarSources";

        /// <summary>The source encoding key.</summary>
        public static readonly string SourceEncodingKey = "SourceEncoding";

        /// <summary>The properties file key.</summary>
        public static readonly string PropertiesFileKey = "PropertiesFile";

        /// <summary>The fx cop path key.</summary>
        public static readonly string FxCopPathKey = "FxCopPath";

        /// <summary>
        /// The local analysis project analysis enabled key
        /// </summary>
        public static readonly string LocalAnalysisProjectAnalysisEnabledKey = "AnalysisOnProject";

        /// <summary>
        /// The local analysis solution analysis enabled key
        /// </summary>
        public static readonly string LocalAnalysisSolutionAnalysisEnabledKey = "AnalysisOnSolution";

        /// <summary>
        /// The sonarqube msbuild version key
        /// </summary>
        public static readonly string SonarQubeMsbuildVersionKey = "SQMSBuildVersion";

        /// <summary>
        /// The CXX wrapper verion key
        /// </summary>
        public static readonly string CxxWrapperVersionKey = "CxxWrapperVersionKey";

        /// <summary>
        /// The CXX wrapper path key
        /// </summary>
        public static readonly string CxxWrapperPathKey = "CxxWrapperPathKey";
    }
}
