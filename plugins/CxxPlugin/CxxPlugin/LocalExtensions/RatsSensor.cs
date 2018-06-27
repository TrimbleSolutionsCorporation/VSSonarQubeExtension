// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RatsSensor.cs" company="Copyright © 2014 jmecsoftware">
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
    using RestSharp;
    using RestSharp.Deserializers;

    using VSSonarPlugins;
    using VSSonarPlugins.Helpers;
    using VSSonarPlugins.Types;

    /// <summary>
    /// The rats sensor.
    /// </summary>
    public class RatsSensor : ASensor
    {
        /// <summary>
        /// The s key.
        /// </summary>
        public const string SKey = "rats";

        /// <summary>
        /// Initializes a new instance of the <see cref="RatsSensor" /> class.
        /// </summary>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="sonarRestService">The sonar rest service.</param>
        public RatsSensor(INotificationManager notificationManager, IConfigurationHelper configurationHelper, ISonarRestService sonarRestService)
            : base(SKey, true, notificationManager, configurationHelper, sonarRestService)
        {
            this.WriteProperty("RatsEnvironment", string.Empty, true, true);
            this.WriteProperty("RatsExecutable", @"C:\ProgramData\MSBuidSonarQube\RATS\rats.exe", true, true);
            this.WriteProperty("RatsArguments", "--xml", true, true);
        }

        /// <summary>
        /// Updates the profile.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="profileIn">The profile in.</param>
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

            var xml = new XmlDeserializer();
            var output = xml.Deserialize<List<Vulnerability>>(new RestResponse { Content = string.Join("\r\n", lines) });

            foreach (var result in output)
            {
                if (!string.IsNullOrEmpty(result.Type))
                {
                    foreach (var file in result.Files)
                    {
                        foreach (var line in file.LineId)
                        {
                            var entry = new Issue
                                            {
                                                Rule = this.RepositoryKey + ":" + result.Type,
                                                Line = line.Value,
                                                Message = result.Message,
                                                Component = file.Name
                                            };

                            try
                            {
                                entry.Severity = (Severity)Enum.Parse(typeof(Severity), result.Severity);
                            }
                            catch (Exception)
                            {
                                entry.Severity  = Severity.UNDEFINED;
                            }

                            violations.Add(entry);
                        }
                    }
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
            return VsSonarUtils.GetEnvironmentFromString(this.ReadGetProperty("RatsEnvironment"));
        }

        /// <summary>
        /// The get command.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string GetCommand()
        {
            return this.ReadGetProperty("RatsExecutable");
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
            return this.ReadGetProperty("RatsArguments") + " " + filePath;
        }

        /// <summary>
        /// The vulnerability.
        /// </summary>
        internal class Vulnerability
        {
            /// <summary>
            /// Gets or sets the severity.
            /// </summary>
            public string Severity { get; set; }

            /// <summary>
            /// Gets or sets the type.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Gets or sets the message.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets the files.
            /// </summary>
            public List<File> Files { get; set; }
        }

        /// <summary>
        /// The file.
        /// </summary>
        internal class File 
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the line.
            /// </summary>
            public List<Line> LineId { get; set; }
        }

        /// <summary>
        /// The line.
        /// </summary>
        internal class Line
        {
            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            public int Value { get; set; }
        }
    }
}
