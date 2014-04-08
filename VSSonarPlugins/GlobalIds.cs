// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalIds.cs" company="">
//   
// </copyright>
// <summary>
//   The global ids.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarPlugins
{
    /// <summary>
    /// The global ids.
    /// </summary>
    public class GlobalIds
    {
        /// <summary>
        /// The global props id.
        /// </summary>
        public static readonly string GlobalPropsId = "LocalAnalyserOptions";

        /// <summary>
        /// The general solution props key.
        /// </summary>
        public static readonly string GeneralSolutionPropsKey = "SolutionGeneralAnalysisOptions";

        /// <summary>
        /// The excluded plugins.
        /// </summary>
        public static readonly string ExcludedPluginsKey = "ExcludedPluginsKey";

        /// <summary>
        /// The runner executable key.
        /// </summary>
        public static readonly string RunnerExecutableKey = "RunnerExecutable";

        /// <summary>
        /// The java executable key.
        /// </summary>
        public static readonly string JavaExecutableKey = "JavaExecutable";

        /// <summary>
        /// The local analysis timeout key.
        /// </summary>
        public static readonly string LocalAnalysisTimeoutKey = "TimeoutAnalysis";

        /// <summary>
        /// The is debug analysis on key.
        /// </summary>
        public static readonly string IsDebugAnalysisOnKey = "IsDebugAnalysisOn";

        /// <summary>
        /// The sonar source key.
        /// </summary>
        public static readonly string SonarSourceKey = "SonarSources";

        /// <summary>
        /// The source encoding.
        /// </summary>
        public static readonly string SourceEncodingKey = "SourceEncoding";

        /// <summary>
        /// The plugin enabled control id.
        /// </summary>
        public static readonly string PluginEnabledControlId = "PluginEnableControl";
    }
}
