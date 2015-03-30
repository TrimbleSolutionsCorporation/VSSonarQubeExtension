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
namespace VSSonarPlugins.Types
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// The context.
    /// </summary>
    [Serializable]
    public enum Context
    {
        [Description("UIProperties")]
        UIProperties,
        [Description("GlobalPropsId")]
        GlobalPropsId,
        [Description("AnalysisProjectId")]
        AnalysisProject,
        [Description("AnalysisGeneral")]
        AnalysisGeneral,
        [Description("FileAnalysisProperties")]
        FileAnalysisProperties,
        [Description("SQAnalysisProperties")]
        SQAnalysisProperties,
    }

    /// <summary>
    /// The global ids.
    /// </summary>
    public class GlobalIds
    {
        /// <summary>
        /// The excluded plugins.
        /// </summary>
        public static readonly string PluginEnabledControlId = "PluginEnabled";

        public static readonly string IsConnectAtStartOn = "IsConnectAtStartOn";

        public static readonly string UserDefinedEditor = "UserDefinedEditor";

        public static readonly string ExtensionDebugModeEnabled = "ExtensionDebugModeEnabled";

        public static readonly string DisableEditorTags = "DisableEditorTags";
    }

    public class OwnersId
    {
        public static readonly string ApplicationOwnerId = "ExtensionControl";

        public static readonly string AnalysisOwnerId = "AnalysisControl";

        public static readonly string PluginGeneralOwnerId = "PluginGeneral";

    }
}
