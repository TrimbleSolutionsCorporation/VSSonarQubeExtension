// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VsFileItem.cs" company="">
//   
// </copyright>
// <summary>
//   The vs project item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarPlugins.Types
{
    using SonarRestService.Types;
    
    /// <summary>
    /// The vs project item.
    /// </summary>
    public class VsFileItem
    {
        /// <summary>Initializes a new instance of the <see cref="VsFileItem"/> class.</summary>
        public VsFileItem()
        {
            this.FileName = string.Empty;
            this.FilePath = string.Empty;
        }

        /// <summary>Gets or sets the file name.</summary>
        public string FileName { get; set; }

        /// <summary>Gets or sets the file path.</summary>
        public string FilePath { get; set; }

        /// <summary>Gets or sets the sonar project that this item belongs</summary>
        public Resource SonarResource { get; set; }

        /// <summary>Gets or sets the solution.</summary>
        public VsProjectItem Project { get; set; }
    }
}