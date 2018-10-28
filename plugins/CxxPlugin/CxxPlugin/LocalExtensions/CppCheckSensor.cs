// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CppCheckSensor.cs" company="Copyright © 2014 jmecsoftware">
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
    using System.Collections.Generic;
    using System.Linq;

    using RestSharp;
    using RestSharp.Deserializers;

    using VSSonarPlugins;
    using VSSonarPlugins.Helpers;
    using VSSonarPlugins.Types;

    using SonarRestService;
    using SonarRestService.Types;

    /// <summary>
    /// The cpp check sensor.
    /// </summary>
    public class CppCheckSensor : ASensor
    {
        /// <summary>
        /// The repository key.
        /// </summary>
        public const string SKey = "cppcheck";

        /// <summary>
        /// Initializes a new instance of the <see cref="CppCheckSensor" /> class.
        /// </summary>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="sonarRestService">The sonar rest service.</param>
        public CppCheckSensor(INotificationManager notificationManager, IConfigurationHelper configurationHelper, ISonarRestService sonarRestService)
            : base(SKey, false, notificationManager, configurationHelper, sonarRestService)
        {
            this.WriteProperty("CppCheckEnvironment", string.Empty, true, true);
            this.WriteProperty("CppCheckExecutable", @"C:\Program Files\Cppcheck\cppcheck.exe", true, true);
            this.WriteProperty("CppCheckArguments", "--inline-suppr --enable=all --xml -D__cplusplus -DNT", true, true);
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
            var output = xml.Deserialize<Results>(new RestResponse { Content = string.Join("\r\n", lines) });

            violations.AddRange(
                from error in output.Errors let ruleKey = this.RepositoryKey + ":" + error.Id where !ruleKey.Equals("cppcheck:unusedFunction")
                select new Issue { Line = error.Line, Message = error.Msg, Rule = this.RepositoryKey + ":" + error.Id, Component = error.File } );

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
            return VsSonarUtils.GetEnvironmentFromString(this.ReadGetProperty("CppCheckEnvironment"));
        }

        /// <summary>
        /// The get command.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string GetCommand()
        {
            return this.ReadGetProperty("CppCheckExecutable");
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
            return this.ReadGetProperty("CppCheckArguments") + " " + filePath;
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
        /// The cpp check xml.
        /// </summary>
        internal class Results
        {
            /// <summary>
            /// Gets or sets the errors.
            /// </summary>
            public List<Error> Errors { get; set; }
        }

        /// <summary>
        /// The error.
        /// </summary>
        internal class Error
        {
            /// <summary>
            /// Gets or sets the file.
            /// </summary>
            public string File { get; set; }

            /// <summary>
            /// Gets or sets the line.
            /// </summary>
            public int Line { get; set; }

            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the severity.
            /// </summary>
            public string Severity { get; set; }

            /// <summary>
            /// Gets or sets the msg.
            /// </summary>
            public string Msg { get; set; }
        }
    }
}
