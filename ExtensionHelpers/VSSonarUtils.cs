// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSSonarUtils.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace ExtensionHelpers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using EnvDTE;

    using EnvDTE80;

    using ExtensionTypes;

    using RestSharp;
    using RestSharp.Deserializers;

    using VSSonarPlugins;

    /// <summary>
    /// The vs sonar utils.
    /// </summary>
    public static partial class VsSonarUtils
    {
        /// <summary>
        /// Gets the solution glob prop key.
        /// </summary>
        public static string SolutionGlobPropKey(string projectKey)
        {
            return GlobalIds.GeneralSolutionPropsKey + "." + projectKey;
        }

        /// <summary>
        /// The get document language.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string GetDocumentLanguage(Document doc)
        {
            var textDoc = doc.Object("TextDocument") as TextDocument;

            if (textDoc == null)
            {
                return string.Empty;
            }

            return textDoc.Language.ToLower();
        }

        /// <summary>
        /// The get repository key.
        /// </summary>
        /// <param name="solutionPath">
        /// The Solution Path.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string GetProjectKey(string solutionPath)
        {
            if (File.Exists(solutionPath + "\\sonar-project.properties"))
            {
                return ReadPropertyFromFile("sonar.projectKey", solutionPath + "\\sonar-project.properties");
            }

            if (File.Exists(solutionPath + "\\pom.xml"))
            {
                var xmlpath = solutionPath + "\\pom.xml";
                var doc = XDocument.Load(xmlpath);
                var xml = new XmlDeserializer();
                var output = xml.Deserialize<PomFile>(new RestResponse { Content = doc.ToString() });
                return output.GroupId + ":" + output.ArtifactId;
            }

            return string.Empty;
        }

        /// <summary>
        /// The solution path.
        /// </summary>
        /// <param name="applicationObject">
        /// The application object.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string SolutionPath(DTE2 applicationObject)
        {
            return applicationObject != null ? Path.GetDirectoryName(applicationObject.Solution.FullName) : string.Empty;
        }

        /// <summary>
        /// The read property from file.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string ReadPropertyFromFile(string key, string file)
        {
            var data = string.Empty;

            if (File.Exists(file))
            {
                string[] lines = File.ReadAllLines(file);
                foreach (string line in lines)
                {
                    if (line.Contains(key + "="))
                    {
                        string[] elems = line.Split('=');
                        data = elems[1].Trim();
                    }
                }
            }

            return data.Trim();
        }

        /// <summary>
        /// The write data to configuration file.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        public static void WriteDataToConfigurationFile(string key, string data, string file)
        {
            if (File.Exists(file))
            {
                bool replace = false;
                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(key + "="))
                    {
                        lines[i] = key + "=" + data;
                        replace = true;
                    }
                }

                if (replace)
                {
                    File.Delete(file);
                    using (StreamWriter sw = File.AppendText(file))
                    {
                        foreach (var line in lines)
                        {
                            if (line.Contains(key + "="))
                            {
                                sw.WriteLine(key + "=" + data);
                            }
                            else
                            {
                                sw.WriteLine(line);
                            }
                        }
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(file))
                    {
                        sw.WriteLine(key + "=" + data);
                    }
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(file))
                {
                    sw.WriteLine(key + "=" + data);
                }
            }
        }

        /// <summary>
        /// The get environment from string.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static Dictionary<string, string> GetEnvironmentFromString(string data)
        {
            var outDic = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(data))
            {
                return outDic;
            }

            var elems = data.Split(';');
            if (elems.Length > 0)
            {
                foreach (var elem in elems)
                {
                    var propelems = elem.Split('=');
                    if (propelems.Length == 2)
                    {
                        outDic.Add(propelems[0], propelems[1]);
                    }
                }
            }

            return outDic;
        }

        /// <summary>
        /// The get lines from source.
        /// </summary>
        /// <param name="sourceSource">
        /// The source source.
        /// </param>
        /// <param name="sep">
        /// The sep.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string GetLinesFromSource(Source sourceSource, string sep)
        {
            return sourceSource.Lines.Aggregate(string.Empty, (current, line) => current + (line + sep));
        }

        /// <summary>
        /// The get lines from source.
        /// </summary>
        /// <param name="sourceSource">
        /// The source source.
        /// </param>
        /// <param name="lineSep">
        /// The line sep.
        /// </param>
        /// <param name="startLine">
        /// The start line.
        /// </param>
        /// <param name="lenght">
        /// The lenght.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetLinesFromSource(Source sourceSource, string lineSep, int startLine, int lenght)
        {
            var data = string.Empty;
            for (var i = startLine; i < startLine + lenght; i++)
            {
                data += sourceSource.Lines[i] + lineSep;
            }

            return data;
        }

        /// <summary>
        /// The pom file.
        /// </summary>
        internal class PomFile
        {
            /// <summary>
            /// Gets or sets the group id.
            /// </summary>
            public string GroupId { get; set; }

            /// <summary>
            /// Gets or sets the artifact id.
            /// </summary>
            public string ArtifactId { get; set; }
        }
    }
}
