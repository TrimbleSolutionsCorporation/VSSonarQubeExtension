namespace VSSonarExtensionUi.Model.Helpers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using SonarLocalAnalyser;
    using ViewModel;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using Association;
    using System.Windows.Media;
    using System.Collections.Generic;
    using SonarRestService.Types;
    using SonarRestService;
    using System.Threading.Tasks;


    /// <summary>The notify cation manager.</summary>
    public class NotifyCationManager : INotificationManager, IModelBase, IRestLogger
    {
        /// <summary>The model.</summary>
        private readonly SonarQubeViewModel model;

        /// <summary>
        /// The user roaming file
        /// </summary>
        private readonly string userRoamingFile;

        /// <summary>
        /// The helper
        /// </summary>
        private IVsEnvironmentHelper helper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyCationManager" /> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="visualStudioVersion">The visual studio version.</param>
        public NotifyCationManager(SonarQubeViewModel model, string visualStudioVersion)
        {
            this.model = model;
            this.userRoamingFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VSSonarExtension\\debug.log." + visualStudioVersion);

            try
            {
                if (File.Exists(this.userRoamingFile))
                {
                    File.Delete(this.userRoamingFile);
                }
            }
            catch (Exception error)
            {
                Debug.WriteLine(error.Message);
            }

            AssociationModel.RegisterNewModelInPool(this);
        }

        /// <summary>
        /// Gets a value indicating whether [analysis change lines].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [analysis change lines]; otherwise, <c>false</c>.
        /// </value>
        public bool AnalysisChangeLines
        {
            get
            {
                return this.model.AnalysisChangeLines;
            }
        }

        /// <summary>
        /// Gets the user defined editor. TODO to be removed from here
        /// </summary>
        /// <value>
        /// The user defined editor.
        /// </value>
        public string UserDefinedEditor
        {
            get
            {
                return this.model.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.UserDefinedEditor;
            }
        }

        /// <summary>
        /// Called when [issues updated].
        /// </summary>
        public void OnIssuesUpdated()
        {
            this.model.OnIssuesChangeEvent();
        }

        /// <summary>
        /// Called when [disconnect].
        /// </summary>
        public async Task OnDisconnect()
        {
            await Task.Delay(0);
        }

        /// <summary>
        /// Called when [connect to sonar].
        /// </summary>
        /// <param name="configuration">sonar configuration</param>
        public async Task OnConnectToSonar(ISonarConfiguration configuration, IEnumerable<Resource> availableProjects, IList<IIssueTrackerPlugin> issuePlugin)
        {
            // does nothing
            await Task.Delay(0);
        }

        /// <summary>The report exception.</summary>
        /// <param name="ex">The ex.</param>
        public void ReportException(Exception ex)
        {
            if (this.helper == null)
            {
                return;
            }

            this.helper.WriteToVisualStudioOutput(DateTime.Now + ":" + ex.Source + ":" + ex.Message + " : " + ex.StackTrace);
        }

        /// <summary>The report message.</summary>
        /// <param name="messages">The messages.</param>
        public void ReportMessage(Message messages)
        {
            if (this.helper == null)
            {
                return;
            }

            this.helper.WriteToVisualStudioOutput(DateTime.Now + ":" + messages.Id + " : " + messages.Data);
        }

        /// <summary>
        /// Reports the message.
        /// </summary>
        /// <param name="message">The messages.</param>
        public void ReportMessage(string message)
        {
            if(this.helper == null)
            {
                return;
            }

            this.helper.WriteToVisualStudioOutput(DateTime.Now + ":" + message);
        }

        /// <summary>
        /// Starteds the working.
        /// </summary>
        /// <param name="busyMessage">The busy message.</param>
        public void StartedWorking(string busyMessage)
        {
            this.model.BusyToolTip = busyMessage;
            this.model.IsExtensionBusy = true;
        }

        /// <summary>
        /// Endeds the working.
        /// </summary>
        public void EndedWorking()
        {
            this.model.IsExtensionBusy = false;
        }

        /// <summary>
        /// The write exception.
        /// </summary>
        /// <param name="ex">
        /// The ex.
        /// </param>
        public void WriteExceptionToLog(Exception ex)
        {
            if (!this.model.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.ExtensionDebugModeEnabled)
            {
                return;
            }

            try
            {
                using (var writer = new StreamWriter(this.userRoamingFile, true))
                {
                    writer.WriteLineAsync(ex.Message + " : " + ex.StackTrace);
                }
            }
            catch (Exception error)
            {
                Debug.WriteLine(error.Message);
            }
        }

        /// <summary>
        /// The write message.
        /// </summary>
        /// <param name="msg">
        /// The msg.
        /// </param>
        public void WriteMessageToLog(string msg)
        {
            if (!this.model.VSonarQubeOptionsViewData.GeneralConfigurationViewModel.ExtensionDebugModeEnabled)
            {
                return;
            }

            try
            {
                using (var writer = new StreamWriter(this.userRoamingFile, true))
                {
                    writer.WriteLineAsync(msg);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>
        /// returns view model
        /// </returns>
        public object GetViewModel()
        {
            return null;
        }

        /// <summary>
        /// Removes the menu plugin.
        /// </summary>
        /// <param name="menuPlugin">The menu plugin.</param>
        public void RemoveMenuPlugin(IMenuCommandPlugin menuPlugin)
        {
            this.model.RemoveMenuPlugin(menuPlugin);
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void ClearCache()
        {
            this.model.ServerViewModel.ClearCache();
        }

        /// <summary>
        /// Associates the project to solution. TODO, to be removed
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        public async void AssociateProjectToSolution(string v1, string v2)
        {
            await this.model.AssociationModule.AssociateProjectToSolution(v1, v2, this.model.AvailableProjects, this.model.SourceControl);
        }

        /// <summary>
        /// Refreshes the data for resource.
        /// </summary>
        public void RefreshDataForResource()
        {
            this.model.RefreshDataForResource();
        }

        /// <summary>
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public async Task UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            this.helper = vsenvironmenthelperIn;
            await Task.Delay(0);
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="sourcePlugin">The source plugin.</param>
        public async Task AssociateWithNewProject(
            Resource project,
            string workingDir,
            ISourceControlProvider provider,
            Dictionary<string, Profile> profile,
            string visualStudioVersion)
        {
            // not needed
            await Task.Delay(0);
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public async Task OnSolutionClosed()
        {
            // not needed
            await Task.Delay(0);
        }

        /// <summary>
        /// Resets the failure.
        /// </summary>
        public void ResetFailure()
        {
            this.model.ErrorMessageTooltip = string.Empty;
            this.model.ErrorIsFound = false;
        }

        /// <summary>
        /// Flags the failure.
        /// </summary>
        /// <param name="v">The v.</param>
        public void FlagFailure(string v)
        {
            this.model.ErrorMessageTooltip = v;
            this.model.ErrorIsFound = true;
        }

        /// <summary>
        /// Called when [new issues updated].
        /// </summary>
        public void OnNewIssuesUpdated()
        {
            this.model.RefreshNewListOfIssues();
        }

        /// <summary>
        /// Resets the and establish a new connection to server.
        /// </summary>
        public void ResetAndEstablishANewConnectionToServer()
        {
            this.model.ResetAndEstablishANewConnectionToServer();
        }
    }
}