// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IVsEnvironmentHelper.cs" company="Copyright � 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The ConfigurationHelper interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarPlugins
{
    using SonarRestService.Types;
    using Microsoft.CodeAnalysis;
    using Types;

    /// <summary>
    ///     The VsPropertiesHelper interface.
    /// </summary>
    public interface IVsEnvironmentHelper
    {
        /// <summary>The set default option.</summary>
        /// <param name="sonarOptions">The sonar options.</param>
        /// <param name="communityOptions">The community Options.</param>
        /// <param name="item">The item.</param>
        /// <param name="value">The value.</param>
        void WriteDefaultOption(string sonarOptions, string communityOptions, string item, string value);

        /// <summary>The set option.</summary>
        /// <param name="category">The category.</param>
        /// <param name="page">The page.</param>
        /// <param name="item">The item.</param>
        /// <param name="value">The value.</param>
        void WriteOption(string category, string page, string item, string value);

        /// <summary>The write to visual studio output.</summary>
        /// <param name="errorMessage">The error message.</param>
        void WriteToVisualStudioOutput(string errorMessage);

        /// <summary>The navigate to resource.</summary>
        /// <param name="url">The url.</param>
        void NavigateToResource(string url);

        /// <summary>
        /// The open resource in visual studio.
        /// </summary>
        /// <param name="workfolder">The workfolder.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="line">The line.</param>
        /// <param name="editorCommandExec">The editor command execute.</param>
        void OpenResourceInVisualStudio(
            string workfolder, 
            string filename, 
            int line, 
            string editorCommandExec = "notepad");

        /// <summary>The open resource in visual studio.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="line">The line.</param>
        /// <param name="editorCommandExec">The editor command exec.</param>
        void OpenResourceInVisualStudio(string filename, int line, string editorCommandExec = "notepad");

        /// <summary>The get proper file path capitalization.</summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The <see cref="string"/>.</returns>
        string GetProperFilePathCapitalization(string filename);

        /// <summary>
        ///     The get active project from solution name.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        string ActiveProjectName();

        /// <summary>
        ///     The get active project from solution full name.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        string ActiveProjectFileFullPath();

        /// <summary>
        ///     The get active project from solution full name.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        string ActiveFileFullPath();

        /// <summary>
        ///     The get document language.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        string CurrentSelectedDocumentLanguage();

        /// <summary>
        ///     The solution path.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        string ActiveSolutionPath();

        /// <summary>The active configuration.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        string ActiveConfiguration();

        /// <summary>The active platform.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        string ActivePlatform();

        /// <summary>
        ///     The solution name.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        string ActiveSolutionName();

        /// <summary>The restart visual studio.</summary>
        void RestartVisualStudio();

        /// <summary>
        /// The get vs project item.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="associatedProject">The associated project.</param>
        /// <param name="fileResource">The file resource.</param>
        /// <returns>
        /// The <see cref="Types.VsProjectItem" />.
        /// </returns>
        VsFileItem VsFileItem(string filename, Resource associatedProject, Resource fileResource);

        /// <summary>
        /// Vses the file item.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <param name="projectFullPath">The project full path.</param>
        /// <param name="associatedProject">The associated project.</param>
        /// <param name="fileResource">The file resource.</param>
        /// <returns>
        /// The <see cref="Types.VsProjectItem" />.
        /// </returns>
        VsFileItem VsFileItem(string fullPath, string projectFullPath, Resource associatedProject, Resource fileResource);

        /// <summary>The vs project item.</summary>
        /// <param name="projectFileName">The project file name.</param>
        /// <param name="associatedProject">The associated project.</param>
        /// <returns>The <see cref="VsProjectItem"/>.</returns>
        VsProjectItem VsProjectItem(string projectFileName, Resource associatedProject);

        /// <summary>
        /// Gets the project by name in solution.
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <returns>The <see cref="VsProjectItem"/>.</returns>
        VsProjectItem GetProjectByNameInSolution(string projectName, string solutionPath);

        /// <summary>
        /// Gets the project guid from path.
        /// </summary>
        /// <param name="projectPath">The project path.</param>
        /// <param name="solutionPath">The solution path.</param>
        /// <returns>
        /// project unique identifier
        /// </returns>
        string GetGuidForProject(string projectPath, string solutionPath);

        /// <summary>
        /// Gets the project by unique identifier in solution.
        /// </summary>
        /// <param name="projectGuid">The project unique identifier.</param>
        /// <returns></returns>
        VsProjectItem GetProjectByGuidInSolution(string projectGuid, string solutionPath);

        /// <summary>The get file real path for solution.</summary>
        /// <param name="fileInView">The file in view.</param>
        /// <returns>The <see cref="string"/>.</returns>
        string GetFileRealPathForSolution(string fileInView);

        /// <summary>
        ///     The are we running in visual studio.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        bool AreWeRunningInVisualStudio();

        /// <summary>The show source diff.</summary>
        /// <param name="resourceInEditorTxt">The resource in editor txt.</param>
        /// <param name="documentInViewPath">The document in view path.</param>
        void ShowSourceDiff(string resourceInEditorTxt, string documentInViewPath);

        /// <summary>The get saved option.</summary>
        /// <param name="category">The category.</param>
        /// <param name="page">The page.</param>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="string"/>.</returns>
        string ReadSavedOption(string category, string page, string item);

        /// <summary>The clear diff file.</summary>
        /// <param name="localFileName">The local file name.</param>
        /// <param name="serverFileName">The server file name.</param>
        void ClearDiffFile(string localFileName, string serverFileName);

        /// <summary>
        /// Evaluated the value for include file.
        /// </summary>
        /// <param name="msbuildProjectFile">The msbuild project file.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>returns evaluated include file</returns>
        string EvaluatedValueForIncludeFile(string msbuildProjectFile, string filePath);

        /// <summary>
        /// Sets the current document in view.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        void SetCurrentDocumentInView(string fullName);

        /// <summary>
        /// Gets the current document in view.
        /// </summary>
        /// <returns>current document in view</returns>
        string GetCurrentDocumentInView();

        /// <summary>
        /// Does the i have admin rights.
        /// </summary>
        /// <returns>true if admin</returns>
        bool DoIHaveAdminRights();

        /// <summary>
        /// Gets the current roslyn solution.
        /// </summary>
        /// <returns>returns current roslyn solution</returns>
        Solution GetCurrentRoslynSolution();

        /// <summary>
        /// Vses the version.
        /// </summary>
        /// <returns></returns>
        string VsVersion();
    }
}