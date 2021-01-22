// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PcLintSensor.cs" company="Copyright © 2014 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
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

namespace CxxPlugin.LocalExtensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using SonarRestService;
    using SonarRestService.Types;

    using VSSonarPlugins;
    using VSSonarPlugins.Helpers;

    /// <summary>
    /// The vera sensor.
    /// </summary>
    public class PcLintSensor : ASensor
    {
        /// <summary>
        /// The s key.
        /// </summary>
        public const string SKey = "pclint";

        /// <summary>
        /// Initializes a new instance of the <see cref="PcLintSensor" /> class.
        /// </summary>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="sonarRestService">The sonar rest service.</param>
        public PcLintSensor(INotificationManager notificationManager, IConfigurationHelper configurationHelper, ISonarRestService sonarRestService)
            : base(SKey, false, notificationManager, configurationHelper, sonarRestService)
        {
        }

        /// <summary>
        /// Updates the profile.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="profileIn">The profile in.</param>
        /// <param name="vsVersion">version</param>
        public override void UpdateProfile(
            Resource project,
            ISonarConfiguration configuration,
            Dictionary<string, Profile> profileIn,
            string vsVersion)
        {
            // not needed
        }

        /// <summary>
        /// The get violations.
        /// </summary>
        /// <param name="lines">
        ///     The lines.
        /// </param>
        /// <returns>
        /// The VSSonarPlugin.SonarInterface.ResponseMappings.Violations.ViolationsResponse.
        /// </returns>
        public override List<Issue> GetViolations(List<string> lines)
        {
            var violations = new List<Issue>();

            if (lines == null || lines.Count == 0)
            {
                return violations;
            }

            foreach (var line in lines)
            {
                try
                {
                    var start = 0;
                    var file = GetStringUntilFirstChar(ref start, line, '(');

                    start++;
                    var linenumber = Convert.ToInt32(GetStringUntilFirstChar(ref start, line, ')'));

                    start += 2;
                    var msg = GetStringUntilFirstChar(ref start, line, '[').Trim();

                    start++;
                    var id = GetStringUntilFirstChar(ref start, line, ']');

                    var entry = new Issue
                                    {
                                        Line = linenumber,
                                        Message = msg,
                                        Rule = this.RepositoryKey + ":" + id,
                                        Component = file
                                    };

                    violations.Add(entry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error: " + ex.Message);
                }
            }

            return violations;
        }

        /// <summary>
        /// The get environment.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public override Dictionary<string, string> GetEnvironment()
        {
            return VsSonarUtils.GetEnvironmentFromString(CxxConfiguration.CxxSettings.PcLintEnvironment);
        }

        /// <summary>
        /// The get command.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string GetCommand()
        {
            return this.ReadGetProperty("PcLintExecutable");
        }

        /// <summary>
        /// The get arguments.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        public override string GetArguments(string filePath)
        {
            var executable = this.ReadGetProperty("PcLintExecutable");
            var parent = Directory.GetParent(executable);
            return "-\"format=%(%F(%l):%) error : (%t -- %m) : [%n]\"" + "-i\"" + parent + "\" +ffn std.lnt env-vc10.lnt " + this.ReadGetProperty("PcLintArguments") + " " + filePath;
        }

        /// <summary>
        /// The get string until first char.
        /// </summary>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="line">
        /// The line.
        /// </param>
        /// <param name="charCheck">
        /// The char check.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        private static string GetStringUntilFirstChar(ref int start, string line, char charCheck)
        {
            var data = string.Empty;

            for (var i = start; i < line.Length; i++)
            {
                start = i;
                if (line[i].Equals(charCheck))
                {
                    break;
                }

                data += line[i];
            }

            return data;
        }
    }
}
