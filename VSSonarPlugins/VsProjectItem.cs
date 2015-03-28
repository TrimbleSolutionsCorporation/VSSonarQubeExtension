// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VsProjectItem.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    /// The vs project item.
    /// </summary>
    public class VsProjectItem
    {
        /// <summary>
        /// The file name.
        /// </summary>
        private readonly string fileName;

        /// <summary>
        /// The file path.
        /// </summary>
        private readonly string filePath;

        /// <summary>
        /// The project file.
        /// </summary>
        private readonly string projectName;

        /// <summary>
        /// The project file path.
        /// </summary>
        private readonly string projectFilePath;

        /// <summary>
        /// The solution path.
        /// </summary>
        private readonly string solutionPath;

        /// <summary>
        /// The solution name.
        /// </summary>
        private readonly string solutionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="VsProjectItem"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <param name="projectName">
        /// The project file.
        /// </param>
        /// <param name="projectFilePath">
        /// The project file path.
        /// </param>
        /// <param name="solutionName">
        /// The solution Name.
        /// </param>
        /// <param name="solutionPath">
        /// The solution Path.
        /// </param>
        public VsProjectItem(string fileName, string filePath, string projectName, string projectFilePath, string solutionName, string solutionPath)
        {
            this.fileName = fileName;
            this.filePath = filePath;
            this.projectName = projectName;
            this.projectFilePath = projectFilePath;
            this.solutionPath = solutionPath;
            this.solutionName = solutionName;
        }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public string FileName
        {
            get
            {
                return this.fileName;
            }
        }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        public string FilePath
        {
            get
            {
                return this.filePath;
            }
        }

        /// <summary>
        /// Gets the project file.
        /// </summary>
        public string ProjectName
        {
            get
            {
                return this.projectName;
            }
        }

        /// <summary>
        /// Gets the project file path.
        /// </summary>
        public string ProjectFilePath
        {
            get
            {
                return this.projectFilePath;
            }
        }

        /// <summary>
        /// Gets the project file path.
        /// </summary>
        public string SolutionPath
        {
            get
            {
                return this.solutionPath;
            }
        }

        /// <summary>
        /// Gets the solution name.
        /// </summary>
        public string SolutionName
        {
            get
            {
                return this.solutionName;
            }
        }

        public string SqResourceKey { get; set; }
    }
}