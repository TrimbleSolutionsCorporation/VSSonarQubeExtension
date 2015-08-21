namespace VSSonarPlugins
{
    using Types;

    /// <summary>
    /// source control provider
    /// </summary>
    public interface ISourceVersionPlugin : IPlugin
    {
        /// <summary>
        /// Gets the blame by line.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="line">The line.</param>
        /// <returns>
        /// Returns Blame data for line
        /// </returns>
        BlameLine GetBlameByLine(string filePath, int line);

        /// <summary>
        /// get current branch
        /// </summary>
        /// <returns>
        /// current branch
        /// </returns>
        string GetBranch();

        /// <summary>
        /// Determines whether the specified base path is supported.
        /// </summary>
        /// <returns>
        /// returns if plugin supports current system.
        /// </returns>
        bool IsSupported();

        /// <summary>
        /// Sets the source dir.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        void InitializeRepository(string basePath);
    }
}