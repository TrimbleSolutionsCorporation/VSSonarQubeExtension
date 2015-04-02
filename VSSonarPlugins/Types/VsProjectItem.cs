namespace VSSonarPlugins.Types
{
    using PropertyChanged;

    /// <summary>The vs project item.</summary>
    [ImplementPropertyChanged]
    public class VsProjectItem
    {
        /// <summary>Gets or sets the project name.</summary>
        public string ProjectName { get; set; }

        /// <summary>Gets or sets the project file path. Full path includes file name</summary>
        public string ProjectFilePath { get; set; }

        /// <summary>Gets or sets the output path.</summary>
        public string OutputPath { get; set; }

        /// <summary>Gets or sets the solution.</summary>
        public VsSolutionItem Solution { get; set; }
    }
}