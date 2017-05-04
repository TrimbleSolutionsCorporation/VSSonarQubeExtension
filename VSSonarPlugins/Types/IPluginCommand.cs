namespace VSSonarPlugins.Types
{
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public interface IPluginCommand
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the execute command.
        /// </summary>
        /// <value>
        /// The execute command.
        /// </value>
        List<Issue> ExecuteCommand(VsFileItem resource);
    }
}
