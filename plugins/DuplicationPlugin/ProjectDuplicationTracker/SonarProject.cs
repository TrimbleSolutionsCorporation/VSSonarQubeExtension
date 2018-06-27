// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarProject.cs" company="Copyright © 2014 jmecsoftware">
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
namespace ProjectDuplicationTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    using VSSonarPlugins.Helpers;

    /// <summary>
    /// The sonar project.
    /// </summary>
    public class SonarProject
    {
        private List<DuplicationData> duplicatedData;

        private ISonarConfiguration conf;

        private ISonarRestService service;

        public SonarProject(Resource project, ISonarRestService service, ISonarConfiguration conf)
        {
            this.service = service;
            this.conf = conf;
            this.Project = project;
        }

        public Resource Project { get; private set; }
        public List<ProjectFile> Files { get; set; }

        public List<DuplicationData> GetDuplicatedData()
        {
            if (duplicatedData == null)
            {
                this.duplicatedData = this.service.GetDuplicationsDataInResource(this.conf, this.Project.Key);
            }

            return this.duplicatedData;
        }

        /// <summary>
        /// The is file found in files.
        /// </summary>
        /// <param name="service">
        /// The service.
        /// </param>
        /// <param name="conf">
        /// The conf.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="Resource"/>.
        /// </returns>
        public ProjectFile IsFileFoundInFiles(string key)
        {            
            if (this.Files == null)
            {
                this.Files = new List<ProjectFile>();
                foreach (var file in this.service.GetResourcesData(this.conf, this.Project.Key + "&scopes=FIL&depth=-1"))
                {
                    this.Files.Add(new ProjectFile(file));                    
                }                
            }

            return this.Files.FirstOrDefault(file => file.Resource.Key.Equals(key));
        }

        /// <summary>
        /// The get source code lines.
        /// </summary>
        /// <param name="fileResourcekey">
        /// The file resourcekey.
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
        public string GetSourceCodeLines(string fileResourcekey, int startLine, int lenght)
        {
            var sourceCode = string.Empty;
            var resource = this.IsFileFoundInFiles(fileResourcekey);
            if (resource == null)
            {
                return sourceCode;
            }

            if (resource.SourceCode == null)
            {
                resource.SourceCode = this.service.GetSourceForFileResource(this.conf, fileResourcekey);
            }

            if (resource.SourceCode == null)
            {
                return sourceCode;
            }

            sourceCode = VsSonarUtils.GetLinesFromSource(resource.SourceCode, "\r\n", startLine-1, lenght);

            return sourceCode;
        }

        /// <summary>
        /// The get duplicated blocks.
        /// </summary>
        /// <param name="secondProject">
        /// The second project.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<DuplicationData> GetDuplicatedBlocks(SonarProject secondProject)
        {
            var newData = new List<DuplicationData>();

            foreach (var otherData in this.GetDuplicatedData())
            {
                var isFound = false;

                foreach (var group in otherData.DuplicatedGroups)
                {
                    foreach (var block in group.DuplicatedBlocks)
                    {
                        if (this.IsFileFoundInFiles(block.Resource.Key) != null)
                        {
                            continue;
                        }

                        if (secondProject.IsFileFoundInFiles(block.Resource.Key) != null)
                        {
                            isFound = true;
                        }
                    }
                }

                if (isFound)
                {
                    newData.Add(otherData);
                }
            }

            return newData;
        }
    }
}