namespace VSSonarExtensionUi.View.Helpers
{
    using PropertyChanged;
    using System.Collections.Generic;

    [AddINotifyPropertyChangedInterface]
    public class ResourceInComponentDialog
    {
        public ResourceInComponentDialog()
        {
            this.BranchResources = new List<ResourceInComponentDialog>();
        }

        /// <summary>
        /// internal resources
        /// </summary>
        public List<ResourceInComponentDialog> BranchResources;

        /// <summary>
        /// is invalid if key does not exist
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// qualifier
        /// </summary>
        public string Qualifier { get; set; }

        /// <summary>
        /// name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// signals if resource is branch
        /// </summary>
        public bool IsBranch { get; set; }

        /// <summary>
        /// branch name
        /// </summary>
        public string BranchName { get; set; }
    }
}