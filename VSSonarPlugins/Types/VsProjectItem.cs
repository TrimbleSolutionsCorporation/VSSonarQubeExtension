// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VsProjectItem.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The vs project item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarPlugins.Types
{
    using PropertyChanged;

    /// <summary>
    /// The vs project item.
    /// </summary>
    [ImplementPropertyChanged]
    public class VsProjectItem
    {
        /// <summary>Initializes a new instance of the <see cref="VsProjectItem"/> class.</summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="projectName">The project file.</param>
        /// <param name="projectFilePath">The project file path.</param>
        /// <param name="solutionName">The solution Name.</param>
        /// <param name="solutionPath">The solution Path.</param>
        public VsProjectItem(
            string fileName, 
            string filePath, 
            string projectName, 
            string projectFilePath, 
            string solutionName, 
            string solutionPath)
        {
            this.FileName = fileName;
            this.FilePath = filePath;
            this.ProjectName = projectName;
            this.ProjectFilePath = projectFilePath;
            this.SolutionPath = solutionPath;
            this.SolutionName = solutionName;
        }

        public VsProjectItem()
        {
            // TODO: Complete member initialization
        }

        /// <summary>Gets or sets the file name.</summary>
        public string FileName { get; set; }

        /// <summary>Gets or sets the file path.</summary>
        public string FilePath { get; set; }

        /// <summary>Gets or sets the project name.</summary>
        public string ProjectName { get; set; }

        /// <summary>Gets or sets the project file path.</summary>
        public string ProjectFilePath { get; set; }

        /// <summary>Gets or sets the solution path.</summary>
        public string SolutionPath { get; set; }

        /// <summary>Gets or sets the solution name.</summary>
        public string SolutionName { get; set; }

        /// <summary>Gets or sets the sq resource key.</summary>
        public string SqResourceKey { get; set; }

        public string OutputPath { get; set; }
    }
}