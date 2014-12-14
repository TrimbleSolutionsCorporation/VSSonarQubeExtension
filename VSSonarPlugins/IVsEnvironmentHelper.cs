// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IVsEnvironmentHelper.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarPlugins
{
    using System.Collections.Generic;

    using ExtensionTypes;

    public interface IConfigurationHelper
    {
        /// <summary>
        /// The read option from application data.
        /// </summary>
        /// <param name="pluginKey">
        /// The plugin key.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string ReadOptionFromApplicationData(string pluginKey, string key);

        /// <summary>
        /// The write option in application data.
        /// </summary>
        /// <param name="pluginKey">
        /// The plugin key.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        void WriteOptionInApplicationData(string pluginKey, string key, string value);

        /// <summary>
        /// The read all options for plugin option in application data.
        /// </summary>
        /// <param name="pluginKey">
        /// The plugin key.
        /// </param>
        /// <returns>
        /// The <see>
        ///     <cref>Dictionary</cref>
        /// </see>
        ///     .
        /// </returns>
        Dictionary<string, string> ReadAllAvailableOptionsInSettings(string pluginKey);


        /// <summary>
        /// The write all options for plugin option in application data.
        /// </summary>
        /// <param name="pluginKey">
        /// The plugin key.
        /// </param>
        /// <param name="project">
        /// The project.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        void WriteAllOptionsForPluginOptionInApplicationData(string pluginKey, Resource project, Dictionary<string, string> options);



        /// <summary>
        /// The get user app data configuration file.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string UserAppDataConfigurationFile();






    }

    /// <summary>
    /// The VsPropertiesHelper interface.
    /// </summary>
    public interface IVsEnvironmentHelper
    {

        /// <summary>
        /// The set default option.
        /// </summary>
        /// <param name="sonarOptions">
        /// The sonar options.
        /// </param>
        /// <param name="communityOptions">
        /// The community Options.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        void WriteDefaultOption(string sonarOptions, string communityOptions, string item, string value);

        /// <summary>
        /// The set option.
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        void WriteOption(string category, string page, string item, string value);

        void WriteToVisualStudioOutput(string errorMessage);

        /// <summary>
        /// The navigate to resource.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        void NavigateToResource(string url);

        /// <summary>
        /// The open resource in visual studio.
        /// </summary>
        /// <param name="workfolder"></param>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <param name="line">
        /// The line.
        /// </param>
        void OpenResourceInVisualStudio(string workfolder, string filename, int line, string editorCommandExec = "notepad");


        string GetProperFilePathCapitalization(string filename);

        /// <summary>
        /// The get active project from solution name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string ActiveProjectName();

        /// <summary>
        /// The get active project from solution full name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string ActiveProjectFileFullPath();

        /// <summary>
        /// The get active project from solution full name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string ActiveFileFullPath();

        /// <summary>
        /// The get document language.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string CurrentSelectedDocumentLanguage();

        /// <summary>
        /// The solution path.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string ActiveSolutionPath();

        /// <summary>
        /// The solution name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string ActiveSolutionName();




        void RestartVisualStudio();


        /// <summary>
        /// The get vs project item.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="VSSonarPlugins.VsProjectItem"/>.
        /// </returns>
        VsProjectItem VsProjectItem(string filename);

        /// <summary>
        /// The get file real path for solution.
        /// </summary>
        /// <param name="fileInView">
        /// The file in view.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetFileRealPathForSolution(string fileInView);

        /// <summary>
        /// The get current text in view.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetCurrentTextInView();

        /// <summary>
        /// The are we running in visual studio.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool AreWeRunningInVisualStudio();

        void ShowSourceDiff(string resourceInEditorTxt, string documentInViewPath);

        /// <summary>
        /// The get saved option.
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string ReadSavedOption(string category, string page, string item);

        void ClearDiffFile(string localFileName, string serverFileName);
    }
}