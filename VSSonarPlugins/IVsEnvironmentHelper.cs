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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using VSSonarPlugins.Types;

    /// <summary>The ConfigurationHelper interface.</summary>
    public interface IConfigurationHelper
    {
        /// <summary>The write configuration.</summary>
        /// <param name="context">The context.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="SonarQubeProperties"/>.</returns>
        SonarQubeProperties ReadSetting(Context context, string owner, string key);

        /// <summary>The read settings.</summary>
        /// <param name="context">The context.</param>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        IEnumerable<SonarQubeProperties> ReadSettings(Context context, string owner);

        /// <summary>The write setting.</summary>
        /// <param name="prop">The prop.</param>
        /// <param name="sync">The sync.</param>
        /// <param name="skipIfExist">The skip if exist.</param>
        void WriteSetting(SonarQubeProperties prop, bool sync = false, bool skipIfExist = false);

        /// <summary>The sync settings.</summary>
        void SyncSettings();

        /// <summary>The clear non saved settings.</summary>
        void ClearNonSavedSettings();

        /// <summary>The delete settings file.</summary>
        void DeleteSettingsFile();

        /// <summary>
        ///     The get user app data configuration file.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        string UserAppDataConfigurationFile();

        /// <summary>The user log for analysis file.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        string UserLogForAnalysisFile();

        /// <summary>The write option in application data.</summary>
        /// <param name="context">The context.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="sync">The sync.</param>
        /// <param name="skipIfExist">The skip if exist.</param>
        void WriteOptionInApplicationData(
            Context context, 
            string owner, 
            string key, 
            string value, 
            bool sync = false, 
            bool skipIfExist = false);
    }

    /// <summary>The stand alone vs helper.</summary>
    public class StandAloneVsHelper : IVsEnvironmentHelper
    {
        /// <summary>The write default option.</summary>
        /// <param name="sonarOptions">The sonar options.</param>
        /// <param name="communityOptions">The community options.</param>
        /// <param name="item">The item.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteDefaultOption(string sonarOptions, string communityOptions, string item, string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>The write option.</summary>
        /// <param name="category">The category.</param>
        /// <param name="page">The page.</param>
        /// <param name="item">The item.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteOption(string category, string page, string item, string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>The write to visual studio output.</summary>
        /// <param name="errorMessage">The error message.</param>
        public void WriteToVisualStudioOutput(string errorMessage)
        {
        }

        /// <summary>The navigate to resource.</summary>
        /// <param name="url">The url.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void NavigateToResource(string url)
        {
            throw new NotImplementedException();
        }

        /// <summary>The open resource in visual studio.</summary>
        /// <param name="workfolder">The workfolder.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="line">The line.</param>
        /// <param name="editorCommandExec">The editor command exec.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void OpenResourceInVisualStudio(
            string workfolder, 
            string filename, 
            int line, 
            string editorCommandExec = "notepad")
        {
            throw new NotImplementedException();
        }

        /// <summary>The open resource in visual studio.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="line">The line.</param>
        /// <param name="editorCommandExec">The editor command exec.</param>
        public void OpenResourceInVisualStudio(string filename, int line, string editorCommandExec = "notepad")
        {
            Process.Start(editorCommandExec, filename);
        }

        /// <summary>The get proper file path capitalization.</summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetProperFilePathCapitalization(string filename)
        {
            throw new NotImplementedException();
        }

        /// <summary>The active project name.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActiveProjectName()
        {
            throw new NotImplementedException();
        }

        /// <summary>The active project file full path.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActiveProjectFileFullPath()
        {
            throw new NotImplementedException();
        }

        /// <summary>The active file full path.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActiveFileFullPath()
        {
            throw new NotImplementedException();
        }

        /// <summary>The current selected document language.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string CurrentSelectedDocumentLanguage()
        {
            throw new NotImplementedException();
        }

        /// <summary>The active solution path.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActiveSolutionPath()
        {
            throw new NotImplementedException();
        }

        /// <summary>The active solution name.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActiveSolutionName()
        {
            throw new NotImplementedException();
        }

        /// <summary>The restart visual studio.</summary>
        /// <exception cref="NotImplementedException"></exception>
        public void RestartVisualStudio()
        {
            throw new NotImplementedException();
        }

        /// <summary>The vs project item.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="associatedProject"></param>
        /// <param name="projectResource"></param>
        /// <returns>The <see cref="VsFileItem"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public VsFileItem VsFileItem(string filename, Resource associatedProject, Resource projectResource)
        {
            throw new NotImplementedException();
        }

        public VsFileItem VsFileItem(string fullPath, string projectFullPath, Resource associatedProject, Resource fileResource)
        {
            throw new NotImplementedException();
        }

        public VsProjectItem VsProjectItem(string projectFileName, Resource associatedProject)
        {
            throw new NotImplementedException();
        }

        /// <summary>The get file real path for solution.</summary>
        /// <param name="fileInView">The file in view.</param>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetFileRealPathForSolution(string fileInView)
        {
            throw new NotImplementedException();
        }

        /// <summary>The get current text in view.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetCurrentTextInView()
        {
            throw new NotImplementedException();
        }

        /// <summary>The are we running in visual studio.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool AreWeRunningInVisualStudio()
        {
            return false;
        }

        /// <summary>The show source diff.</summary>
        /// <param name="resourceInEditorTxt">The resource in editor txt.</param>
        /// <param name="documentInViewPath">The document in view path.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void ShowSourceDiff(string resourceInEditorTxt, string documentInViewPath)
        {
            throw new NotImplementedException();
        }

        /// <summary>The read saved option.</summary>
        /// <param name="category">The category.</param>
        /// <param name="page">The page.</param>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ReadSavedOption(string category, string page, string item)
        {
            throw new NotImplementedException();
        }

        /// <summary>The clear diff file.</summary>
        /// <param name="localFileName">The local file name.</param>
        /// <param name="serverFileName">The server file name.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void ClearDiffFile(string localFileName, string serverFileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>The active configuration.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActiveConfiguration()
        {
            throw new NotImplementedException();
        }

        /// <summary>The active platform.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string ActivePlatform()
        {
            throw new NotImplementedException();
        }


        public VsProjectItem GetProjectByNameInSolution(string projectName)
        {
            throw new NotImplementedException();
        }
    }

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

        /// <summary>The open resource in visual studio.</summary>
        /// <param name="workfolder"></param>
        /// <param name="filename">The filename.</param>
        /// <param name="line">The line.</param>
        /// <param name="editorCommandExec"></param>
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

        /// <summary>The get vs project item.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="associatedProject"></param>
        /// <param name="projectResource"></param>
        /// <returns>The <see cref="Types.VsProjectItem"/>.</returns>
        VsFileItem VsFileItem(string filename, Resource associatedProject, Resource fileResource);

        VsFileItem VsFileItem(string fullPath, string projectFullPath, Resource associatedProject, Resource fileResource);

        /// <summary>The vs project item.</summary>
        /// <param name="projectFileName">The project file name.</param>
        /// <param name="associatedProject">The associated project.</param>
        /// <returns>The <see cref="VsProjectItem"/>.</returns>
        VsProjectItem VsProjectItem(string projectFileName, Resource associatedProject);

        VsProjectItem GetProjectByNameInSolution(string projectName);

        /// <summary>The get file real path for solution.</summary>
        /// <param name="fileInView">The file in view.</param>
        /// <returns>The <see cref="string"/>.</returns>
        string GetFileRealPathForSolution(string fileInView);

        /// <summary>
        ///     The get current text in view.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        string GetCurrentTextInView();

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
    }
}