namespace VSSonarPlugins
{
    /// <summary>
    /// The Plugin interface.
    /// </summary>
    public interface IPlugin
    {
        string GetVersion();

        string GetAssemblyPath();
    }
}