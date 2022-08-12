namespace VSSonarQubeExtension
{
    using Microsoft.CodeAnalysis;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.LanguageServices;

    /// <summary>
    /// workspace provider
    /// </summary>
    public static class WorspaceProvider
    {
        /// <summary>
        /// Provides the current roslyn solution.
        /// </summary>
        /// <returns>current roslyn solution</returns>
        internal static Solution ProvideCurrentRoslynSolution()
        {
            var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            return componentModel.GetService<VisualStudioWorkspace>().CurrentSolution;
        }
    }
}
