// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSEvents.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarQubeExtension.Helpers
{
    using System;
    using System.IO;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using EnvDTE;
    using EnvDTE80;

    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Text;

    using VSSonarPlugins;
    using VSSonarPlugins.Helpers;
    using VSSonarPlugins.Types;
    using System.Reflection;
    using System.Threading.Tasks;


    /// <summary>
    ///     The vs events.
    /// </summary>
    public class VsEvents
    {
        #region Fields

        /// <summary>
        ///     The documents events.
        /// </summary>
        public readonly DocumentEvents DocumentsEvents;

        /// <summary>
        ///     The events.
        /// </summary>
        public readonly Events SolutionEvents;

        /// <summary>
        ///     The environment.
        /// </summary>
        private readonly IVsEnvironmentHelper environment;

        /// <summary>The package.</summary>
        private VsSonarExtensionPackage package;

        /// <summary>The build events.</summary>
        private BuildEvents buildEvents;

        private readonly DTE2 dte2;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VsEvents"/> class.
        /// </summary>
        /// <param name="environment">
        ///     The environment.
        /// </param>
        /// <param name="dte2">
        ///     The dte 2.
        /// </param>
        /// <param name="vsSonarExtensionPackage"></param>
        public VsEvents(IVsEnvironmentHelper environment, DTE2 dte2, VsSonarExtensionPackage vsSonarExtensionPackage)
        {
            this.dte2 = dte2;
            this.package = vsSonarExtensionPackage;
            this.environment = environment;
            this.SolutionEvents = dte2.Events;
            this.visualStudioEvents = dte2.Events.DTEEvents;
            this.buildEvents = dte2.Events.BuildEvents;
            this.DocumentsEvents = this.SolutionEvents.DocumentEvents;

            this.SolutionEvents.SolutionEvents.Opened += this.SolutionOpened;
            this.SolutionEvents.SolutionEvents.AfterClosing += this.SolutionClosed;
            this.SolutionEvents.WindowEvents.WindowActivated += this.WindowActivated;
            this.SolutionEvents.WindowEvents.WindowClosing += this.WindowClosed;
            this.DocumentsEvents.DocumentSaved += this.DoumentSaved;
            this.visualStudioEvents.OnStartupComplete += this.CloseToolWindows;
            this.buildEvents.OnBuildProjConfigDone += this.ProjectHasBuild;

            VSColorTheme.ThemeChanged += this.VSColorTheme_ThemeChanged;

            var extensionRunningPath = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "").ToString();

            var uniqueId = this.dte2.Version;

            if (extensionRunningPath.ToLower().Contains(this.dte2.Version + "exp"))
            {
                uniqueId += "Exp";
            }
                        
            SonarQubeViewModelFactory.StartupModelWithVsVersion(uniqueId, this.package).AnalysisModeHasChange += this.AnalysisModeHasChange;
            SonarQubeViewModelFactory.SQViewModel.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.ConfigurationHasChanged +=
                this.AnalysisModeHasChange;
        }

        private void ProjectHasBuild(string project, string projectconfig, string platform, string solutionconfig, bool success)
        {
            if (!success || SonarQubeViewModelFactory.SQViewModel.AssociationModule.AssociatedProject == null)
            {
                return;
            }

            var projectDte = this.dte2.Solution.Item(project);

            if (projectDte != null)
            {
                try
                {
                    string outputPath = projectDte.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
                    string assemblyName = projectDte.Properties.Item("AssemblyName").Value.ToString();

                    var projectItem = this.environment.VsProjectItem(projectDte.FullName, SonarQubeViewModelFactory.SQViewModel.AssociationModule.AssociatedProject);

                    if (Path.IsPathRooted(outputPath))
                    {
                        projectItem.OutputPath = Path.Combine(outputPath, assemblyName + ".dll");
                        if (!File.Exists(projectItem.OutputPath))
                        {
                            projectItem.OutputPath = Path.Combine(outputPath, assemblyName + ".exe");
                        }

                        if (!File.Exists(projectItem.OutputPath))
                        {
                            return;
                        }
                    }
                    else
                    {
                        outputPath = Path.GetFullPath(
                            Path.Combine(Directory.GetParent(projectDte.FullName).ToString(), outputPath));

                        projectItem.OutputPath = Path.Combine(outputPath, assemblyName + ".dll");
                        if (!File.Exists(projectItem.OutputPath))
                        {
                            projectItem.OutputPath = Path.Combine(outputPath, assemblyName + ".exe");
                        }

                        if (!File.Exists(projectItem.OutputPath))
                        {
                            return;
                        }
                    }

                    SonarQubeViewModelFactory.SQViewModel.ProjectHasBuild(projectItem);
                }
                catch (Exception ex)
                {
                    SonarQubeViewModelFactory.SQViewModel.Logger.ReportException(ex);
                }
            }
        }

        private void CloseToolWindows()
        {
            this.package.CloseToolsWindows();
        }

        private DTEEvents visualStudioEvents { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the last document window with focus.
        /// </summary>
        public Window LastDocumentWindowWithFocus { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get property from buffer.
        /// </summary>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <typeparam>
        ///     <name>T</name>
        /// </typeparam>
        /// <returns>
        /// The <see>
        ///         <cref>T</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static T GetPropertyFromBuffer<T>(ITextBuffer buffer)
        {
            try
            {
                foreach (var item in buffer.Properties.PropertyList.Where(item => item.Value is T))
                {
                    return (T)item.Value;
                }
            }
            catch (Exception ex)
            {
                SonarQubeViewModelFactory.SQViewModel.Logger.ReportException(ex);
            }

            return default(T);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The analysis mode has change.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void AnalysisModeHasChange(object sender, EventArgs e)
        {
            if (this.LastDocumentWindowWithFocus != null)
            {
                SonarQubeViewModelFactory.SQViewModel.RefreshDataForResource(
                    this.LastDocumentWindowWithFocus.Document.FullName, File.ReadAllText(this.LastDocumentWindowWithFocus.Document.FullName), false);
            }
        }

        /// <summary>
        /// The doument saved.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        private void DoumentSaved(Document document)
        {
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("DoumentSaved : " + document);
            if (document == null)
            {
                return;
            }

            try
            {
                SonarQubeViewModelFactory.SQViewModel.RefreshDataForResource(document.FullName, File.ReadAllText(document.FullName), true);
            }
            catch (Exception ex)
            {
                SonarQubeViewModelFactory.SQViewModel.Logger.ReportException(ex);
            }
        }

        /// <summary>
        ///     The solution closed.
        /// </summary>
        private void SolutionClosed()
        {
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("Solution Closed");
            SonarQubeViewModelFactory.SQViewModel.OnSolutionClosed();
        }

        /// <summary>
        ///     The solution opened.
        /// </summary>
        private async void SolutionOpened()
        {
            string solutionName = this.environment.ActiveSolutionName();
            string solutionPath = this.environment.ActiveSolutionPath();
            string fileName = this.environment.ActiveFileFullPath();
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("Solution Opened: " + solutionName + " : " + solutionPath);
            await SonarQubeViewModelFactory.SQViewModel.OnSolutionOpen(solutionName, solutionPath, fileName);
        }

        /// <summary>
        /// The vs color theme_ theme changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            Color defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            Color defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);

            SonarQubeViewModelFactory.SQViewModel.UpdateTheme(
                VsSonarExtensionPackage.ToMediaColor(defaultBackground), 
                VsSonarExtensionPackage.ToMediaColor(defaultForeground));
        }

        private void WindowClosed(Window window)
        {
            if (window.Kind != "Document")
            {
                return;
            }

            try
            {
                if (window.Document == null)
                {
                    SonarQubeViewModelFactory.SQViewModel.ClosedWindow(window.Caption);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// The window activated.
        /// </summary>
        /// <param name="gotFocus">
        /// The got focus.
        /// </param>
        /// <param name="lostFocus">
        /// The lost focus.
        /// </param>
        private void WindowActivated(Window gotFocus, Window lostFocus)
        {
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("Window Activated: Kind: " + gotFocus.Kind);

            if (gotFocus.Kind != "Document")
            {
                return;
            }


            try
            {
                if (this.LastDocumentWindowWithFocus == gotFocus)
                {
                    SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("Last and Current Window are the same");
                    return;
                }

                SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("New Document Open: " + gotFocus.Document.FullName);

                this.LastDocumentWindowWithFocus = gotFocus;
                this.environment.SetCurrentDocumentInView(gotFocus.Document.FullName);
                SonarQubeViewModelFactory.SQViewModel.RefreshDataForResource(gotFocus.Document.FullName, File.ReadAllText(gotFocus.Document.FullName), false);
            }
            catch (Exception ex)
            {
                SonarQubeViewModelFactory.SQViewModel.Logger.ReportException(ex);
            }
        }

        #endregion
    }
}