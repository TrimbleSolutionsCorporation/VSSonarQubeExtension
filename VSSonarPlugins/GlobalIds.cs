// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalIds.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
