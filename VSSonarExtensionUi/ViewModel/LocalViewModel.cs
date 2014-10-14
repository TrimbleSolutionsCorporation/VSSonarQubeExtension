// --------------------------------------------------------------------------------------------------------------------
// <copyright file="serverviewmodel.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSonarExtensionUi.ViewModel
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using PropertyChanged;
    using SonarLocalAnalyser;
    using SonarRestService;
    using System.Windows.Forms;
    using VSSonarExtensionUi.Cache;
    using VSSonarExtensionUi.View;
    using VSSonarPlugins;
    using ExtensionTypes;



    /// <summary>
    ///     The analysis types.
    /// </summary>
    public enum AnalysisTypes
    {
        /// <summary>
        ///     The preview.
        /// </summary>
        Preview,

        /// <summary>
        ///     The incremental.
        /// </summary>
        Incremental,

        /// <summary>
        ///     The file.
        /// </summary>
        File,

        /// <summary>
        ///     The analysis.
        /// </summary>
        Analysis,

        None
    }

    [ImplementPropertyChanged]
    public class LocalViewModel : IViewModelBase
    {
        private const string PROJECTLOCATION = "PROJECTLOCATION";
        private readonly SonarQubeViewModel sonarQubeViewModel;
        private readonly ModelEditorCache localEditorCache = new ModelEditorCache();

        public LocalViewModel()
        {
            this.IssuesGridView = new IssueGridViewModel();
            this.Header = "Local Analysis";
            this.InitCommanding();
        }

        public LocalViewModel(SonarQubeViewModel sonarQubeViewModel, List<IAnalysisPlugin> plugins, ISonarRestService service, IVsEnvironmentHelper helper)
        {
            this.RestService = service;
            this.Vsenvironmenthelper = helper;
            this.sonarQubeViewModel = sonarQubeViewModel;
            this.Header = "Local Analysis";
            this.IssuesGridView = new IssueGridViewModel(sonarQubeViewModel, this, true);

            this.InitCommanding();

            this.LocalAnalyserModule = new SonarLocalAnalyser(plugins, this.RestService, this.Vsenvironmenthelper);
            this.LocalAnalyserModule.StdOutEvent += this.UpdateOutputMessagesFromPlugin;
            this.LocalAnalyserModule.LocalAnalysisCompleted += this.UpdateLocalIssues;
        }

        private void UpdateOutputMessagesFromPlugin(object sender, EventArgs e)
        {
            var exceptionMsg = (LocalAnalysisEventArgs)e;
            this.OutputLog += exceptionMsg.ErrorMessage + "\r\n";
            this.Vsenvironmenthelper.WriteToVisualStudioOutput(exceptionMsg.ErrorMessage);
        }

        private void InitCommanding()
        {
            this.OpenSourceDirCommand = new RelayCommand(this.OnOpenSourceDirCommand, () => this.sonarQubeViewModel.AssociatedProject != null);
            this.IncrementalCommand = new RelayCommand(OnIncrementalCommand, () => this.CanRunAnalysis);
            this.PreviewCommand = new RelayCommand(this.OnPreviewCommand, () => this.CanRunAnalysis);
            this.AnalysisCommand = new RelayCommand(this.OnAnalysisCommand, () => this.CanRunAnalysis);
            this.FileCommand = new RelayCommand(this.OnFileCommand, () => this.CanRunAnalysis);            
            this.StopLocalAnalysisCommand = new RelayCommand(this.OnStopLocalAnalysisCommand, () => !this.CanRunAnalysis);
        }

        private void OnFileCommand()
        {
            this.sonarQubeViewModel.IsExtensionBusy = true;
            this.CanRunAnalysis = false;
            this.sonarQubeViewModel.BusyToolTip = "Running File Analysis";
            this.RunLocalAnalysis(AnalysisTypes.File);
        }

        public void OnAnalysisCommand()
        {
            this.sonarQubeViewModel.IsExtensionBusy = true;
            this.CanRunAnalysis = false;
            this.sonarQubeViewModel.BusyToolTip = "Running Full Analysis";
            this.RunLocalAnalysis(AnalysisTypes.Analysis);
        }

        public void OnPreviewCommand()
        {
            this.sonarQubeViewModel.IsExtensionBusy = true;
            this.CanRunAnalysis = false;
            this.sonarQubeViewModel.BusyToolTip = "Running Preview Analysis";
            this.RunLocalAnalysis(AnalysisTypes.Preview);
        }

        public void OnIncrementalCommand()
        {
            this.sonarQubeViewModel.IsExtensionBusy = true;
            this.CanRunAnalysis = false;
            this.sonarQubeViewModel.BusyToolTip = "Running Incremental Analysis";
            this.RunLocalAnalysis(AnalysisTypes.Incremental);
        }

        private void OnStopLocalAnalysisCommand()
        {
            if (this.LocalAnalyserModule == null)
            {
                return;
            }

            this.LocalAnalyserModule.StopAllExecution();
        }

        /// <summary>
        /// The run local analysis new.
        /// </summary>
        /// <param name="startStop">
        /// The start Stop.
        /// </param>
        /// <param name="analysis">
        /// The analysis.
        /// </param>
        private void RunLocalAnalysis(AnalysisTypes analysis)
        {
            this.OutputLog = string.Empty;
            try
            {
                switch (analysis)
                {
                    case AnalysisTypes.File:
                        this.LocalAnalyserModule.AnalyseFile(
                            this.Vsenvironmenthelper.VsProjectItem(this.sonarQubeViewModel.DocumentInView),
                            this.AssociatedProject,
                            this.sonarQubeViewModel.AnalysisChangeLines,
                            this.sonarQubeViewModel.SonarVersion,
                            this.sonarQubeViewModel.SonarCubeConfiguration);
                        break;
                    case AnalysisTypes.Analysis:
                        this.LocalAnalyserModule.RunFullAnalysis(
                            this.SourceWorkingDir,
                            this.AssociatedProject,
                            this.sonarQubeViewModel.SonarVersion,
                            this.sonarQubeViewModel.SonarCubeConfiguration);
                        break;
                    case AnalysisTypes.Incremental:
                        this.LocalAnalyserModule.RunIncrementalAnalysis(
                            this.SourceWorkingDir,
                            this.AssociatedProject,
                            this.sonarQubeViewModel.SonarVersion,
                            this.sonarQubeViewModel.SonarCubeConfiguration);
                        break;
                    case AnalysisTypes.Preview:
                        this.LocalAnalyserModule.RunPreviewAnalysis(
                            this.SourceWorkingDir,
                            this.AssociatedProject,
                            this.sonarQubeViewModel.SonarVersion,
                            this.sonarQubeViewModel.SonarCubeConfiguration);
                        break;
                }
            }
            catch (VSSonarExtension ex)
            {
                UserExceptionMessageBox.ShowException("Analysis Failed: ", ex);
                this.CanRunAnalysis = true;
                this.sonarQubeViewModel.IsExtensionBusy = false;
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Critical Error: Please Report This Error: ", ex);
                this.CanRunAnalysis = true;
                this.sonarQubeViewModel.IsExtensionBusy = false;
            }
        }

        public ISonarLocalAnalyser LocalAnalyserModule { get; set; }

        internal void InitDataAssociation(Resource associatedProject, ISonarConfiguration sonarCubeConfiguration)
        {
            this.AssociatedProject = associatedProject;
            this.SourceWorkingDir = this.Vsenvironmenthelper.ReadOptionFromApplicationData(associatedProject.Key, PROJECTLOCATION);
            if (!string.IsNullOrEmpty(this.SourceWorkingDir))
            {
                this.CanRunAnalysis = true;
            }
        }



        /// <summary>
        /// The update local issues in view.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void UpdateLocalIssues(object sender, EventArgs e)
        {
            this.CanRunAnalysis = true;
            this.sonarQubeViewModel.IsExtensionBusy = false;

            try
            {
                var exceptionMsg = (LocalAnalysisEventArgs)e;
                if (exceptionMsg != null && exceptionMsg.Ex != null)
                {
                    UserExceptionMessageBox.ShowException("Analysis Ended: " + exceptionMsg.ErrorMessage, exceptionMsg.Ex, "To be Implemented - todo");
                    return;
                }
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Analysis Ended: ", ex, "To be Implemented - todo");
            }

            try
            {

                this.localEditorCache.UpdateIssues(this.LocalAnalyserModule.GetIssues(this.sonarQubeViewModel.SonarCubeConfiguration, this.sonarQubeViewModel.AssociatedProject));
                this.IssuesGridView.UpdateIssues(this.localEditorCache.GetIssues());
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Analysis Finish", ex, "Can Retrive Any Issues From Analysis. For the installed plugins");
            }
        }

        private void OnOpenSourceDirCommand()
        {
            var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.SourceWorkingDir = dialog.SelectedPath;
            this.CanRunAnalysis = true;
            this.Vsenvironmenthelper.WriteOptionInApplicationData(this.AssociatedProject.Key, PROJECTLOCATION, dialog.SelectedPath);
        }


        public string Header { get; set; }
        public IssueGridViewModel IssuesGridView { get; private set; }
        public RelayCommand OpenSourceDirCommand { get; private set; }
        public RelayCommand PreviewCommand { get; private set; }
        public RelayCommand IncrementalCommand { get; private set; }
        public RelayCommand AnalysisCommand { get; private set; }
        public string SourceWorkingDir { get; private set; }
        public bool CanRunAnalysis { get; private set; }
        public ISonarRestService RestService { get; private set; }
        public IVsEnvironmentHelper Vsenvironmenthelper { get; private set; }
        public RelayCommand StopLocalAnalysisCommand { get; private set; }
        public RelayCommand FileCommand { get; private set; }
        public string OutputLog { get; private set; }
        public Resource AssociatedProject { get; private set; }

        internal void ClearAssociation()
        {
            this.AssociatedProject = null;
            this.CanRunAnalysis = false;
        }
    }
}
