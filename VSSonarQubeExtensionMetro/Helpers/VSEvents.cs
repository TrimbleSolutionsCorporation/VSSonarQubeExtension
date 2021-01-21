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
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Text;
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using VSSonarPlugins;

    /// <summary>
    ///     The vs events.
    /// </summary>
    public class VsEvents
    {
        /// <summary>
        ///     The documents events.
        /// </summary>
        public DocumentEvents DocumentsEvents;

        /// <summary>
        ///     The events.
        /// </summary>
        public Events SolutionEvents;

        /// <summary>
        ///     The environment.
        /// </summary>
        private readonly IVsEnvironmentHelper environment;

        /// <summary>The package.</summary>
        private readonly VsSonarExtensionPackage package;

        /// <summary>The build events.</summary>
        private readonly BuildEvents buildEvents;

        /// <summary>
        /// dte service
        /// </summary>
        private readonly DTE2 dte2;

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
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            this.dte2 = dte2;
            package = vsSonarExtensionPackage;
            this.environment = environment;
            SolutionEvents = dte2.Events; ;
            visualStudioEvents = dte2.Events.DTEEvents;
            buildEvents = dte2.Events.BuildEvents;
            DocumentsEvents = SolutionEvents.DocumentEvents;

            SolutionEvents.SolutionEvents.AfterClosing += SolutionClosed;
            SolutionEvents.WindowEvents.WindowActivated += WindowActivated;
            SolutionEvents.WindowEvents.WindowClosing += WindowClosed;
            DocumentsEvents.DocumentSaved += DoumentSaved;
            visualStudioEvents.OnStartupComplete += CloseToolWindows;
            buildEvents.OnBuildProjConfigDone += ProjectHasBuild;

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;

            string extensionRunningPath = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "").ToString();

            string uniqueId = this.dte2.Version;

            if (extensionRunningPath.ToLower().Contains(this.dte2.Version + "exp"))
            {
                uniqueId += "Exp";
            }

            SonarQubeViewModelFactory.StartupModelWithVsVersion(uniqueId, package).AnalysisModeHasChange += AnalysisModeHasChange;
            SonarQubeViewModelFactory.SQViewModel.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.ConfigurationHasChanged +=
                        AnalysisModeHasChange;
        }

        private void ProjectHasBuild(string project, string projectconfig, string platform, string solutionconfig, bool success)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (!success || SonarQubeViewModelFactory.SQViewModel.AssociationModule.AssociatedProject == null)
            {
                return;
            }

            Project projectDte = dte2.Solution.Item(project);

            if (projectDte != null)
            {
                try
                {
                    string outputPath = projectDte.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
                    string assemblyName = projectDte.Properties.Item("AssemblyName").Value.ToString();

                    VSSonarPlugins.Types.VsProjectItem projectItem = environment.VsProjectItem(projectDte.FullName, SonarQubeViewModelFactory.SQViewModel.AssociationModule.AssociatedProject);

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
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            package.CloseToolsWindows();
        }

        private DTEEvents visualStudioEvents { get; set; }

        /// <summary>
        ///     Gets or sets the last document window with focus.
        /// </summary>
        public Window LastDocumentWindowWithFocus { get; set; }

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
                foreach (System.Collections.Generic.KeyValuePair<object, object> item in buffer.Properties.PropertyList.Where(item => item.Value is T))
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

        /// <summary>
        /// The analysis mode has change.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void AnalysisModeHasChange(object sender, EventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (LastDocumentWindowWithFocus != null)
            {
                string fullName = LastDocumentWindowWithFocus.Document.FullName;
                await SonarQubeViewModelFactory.SQViewModel.RefreshDataForResource(fullName, File.ReadAllText(fullName), false);
            }
        }

        /// <summary>
        /// The doument saved.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void DoumentSaved(Document document)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("DoumentSaved : " + document);
            if (document == null)
            {
                return;
            }

            try
            {
                string fullName = document.FullName;
                await SonarQubeViewModelFactory.SQViewModel.RefreshDataForResource(fullName, File.ReadAllText(fullName), true);
            }
            catch (Exception ex)
            {
                SonarQubeViewModelFactory.SQViewModel.Logger.ReportException(ex);
            }
        }

        /// <summary>
        ///     The solution closed.
        /// </summary>
#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void SolutionClosed()
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("Solution Closed");
            await SonarQubeViewModelFactory.SQViewModel.OnSolutionClosed();
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
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
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
#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void WindowActivated(Window gotFocus, Window lostFocus)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("Window Activated: Kind: " + gotFocus.Kind);

            if (gotFocus.Kind != "Document")
            {
                return;
            }

            try
            {
                if (LastDocumentWindowWithFocus == gotFocus)
                {
                    SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("Last and Current Window are the same");
                    return;
                }

                SonarQubeViewModelFactory.SQViewModel.Logger.WriteMessageToLog("New Document Open: " + gotFocus.Document.FullName);

                LastDocumentWindowWithFocus = gotFocus;
                environment.SetCurrentDocumentInView(gotFocus.Document.FullName);
                string fullName = gotFocus.Document.FullName;
                await SonarQubeViewModelFactory.SQViewModel.RefreshDataForResource(fullName, File.ReadAllText(fullName), false);
            }
            catch (Exception ex)
            {
                SonarQubeViewModelFactory.SQViewModel.Logger.ReportException(ex);
            }
        }
    }
}