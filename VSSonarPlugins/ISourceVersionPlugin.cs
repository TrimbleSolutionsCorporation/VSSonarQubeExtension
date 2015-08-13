namespace VSSonarPlugins
{
    using System.Collections.Generic;
    using VSSonarPlugins.Types;

    public interface ISourceControlProvider
    {
        IList<string> GetHistory(Resource item);
        string GetBranch(string basePath);

        void UpdatePlugins(IList<ISourceVersionPlugin> plugins);
    }

    /// <summary>
    /// source control provider
    /// </summary>
    public interface ISourceVersionPlugin : IPlugin
    {
        /// <summary>
        /// get history for a resource file
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        IList<string> GetHistory(Resource item);

        /// <summary>
        /// get current branch
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns></returns>
        string GetBranch(string basePath);

        bool IsSupported(string basePath);
    }
}