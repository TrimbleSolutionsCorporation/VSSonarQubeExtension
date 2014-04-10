// <copyright file="AnalysisViewModel.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.MainViewModel.ViewModel
{
    using System;
    using System.Diagnostics;

    using SonarLocalAnalyser;

    using VSSonarExtension.MainView;

    using VSSonarPlugins;

    /// <summary>
    ///     The extension data model.
    /// </summary>
    public partial class ExtensionDataModel
    {
        #region Fields

        /// <summary>
        ///     The analysis mode.
        /// </summary>
        private bool analysisMode = true;

        /// <summary>
        ///     The analysis mode text.
        /// </summary>
        private AnalysisModes analysisModeText = AnalysisModes.Server;

        /// <summary>
        ///     The analysis trigger.
        /// </summary>
        private bool analysisTrigger;

        /// <summary>
        ///     The analysis type.
        /// </summary>
        private bool analysisType;

        /// <summary>
        ///     The analysis type text.
        /// </summary>
        private AnalysisTypes analysisTypeText = AnalysisTypes.File;

        /// <summary>
        /// The is coverage on.
        /// </summary>
        private bool isCoverageOn;

        /// <summary>
        /// The analysis change lines.
        /// </summary>
        private bool analysisChangeLines;

        #endregion

        #region Enums

        /// <summary>
        ///     The analysis modes.
        /// </summary>
        private enum AnalysisModes
        {
            /// <summary>
            ///     The local.
            /// </summary>
            Local, 

            /// <summary>
            ///     The server.
            /// </summary>
            Server, 
        }

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

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether analysis change lines.
        /// </summary>
        public bool AnalysisChangeLines
        {
            get
            {                
                return this.analysisChangeLines;
            }

            set
            {
                this.analysisChangeLines = value;
                this.OnPropertyChanged("AnalysisChangeLinesText");
                this.OnPropertyChanged("AnalysisChangeLines");
                this.RefreshIssuesInViews();
            }
        }

        /// <summary>
        ///     Gets the analysis mode.
        /// </summary>
        public string AnalysisChangeLinesText
        {
            get
            {
                if (this.analysisChangeLines)
                {
                    return "Yes";
                }

                return "No";
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether analysis mode.
        /// </summary>
        public bool AnalysisMode
        {
            get
            {
                return this.analysisMode;
            }

            set
            {
                this.analysisMode = value;
                this.analysisTrigger = false;
                this.OnPropertyChanged("AnalysisTriggerText");
                this.OnPropertyChanged("AnalysisTrigger");                
                this.analysisModeText = value ? AnalysisModes.Server : AnalysisModes.Local;
                if (this.analysisModeText.Equals(AnalysisModes.Server))
                {
                    this.analysisTypeText = AnalysisTypes.File;
                    this.OnPropertyChanged("AnalysisTypeText");
                }

                this.OnPropertyChanged("AnalysisModeText");
                this.OnPropertyChanged("AnalysisMode");

                this.localEditorCache.ClearData();
                this.TriggerUpdateSignals();
                this.OnPropertyChanged("Issues");
            }
        }

        /// <summary>
        /// Gets the analysis trigger text.
        /// </summary>
        public string AnalysisTriggerText
        {
            get
            {
                this.OnPropertyChanged("ExtensionIsBusy");
                return this.AnalysisTrigger ? "Stop" : "Start";
            }
        }

        /// <summary>
        /// Gets a value indicating whether is server analysis on.
        /// </summary>
        public bool IsServerAnalysisOn
        {
            get
            {
                return this.analysisModeText.Equals(AnalysisModes.Server);
            }
        }
        
        /// <summary>
        ///     Gets the analysis mode text.
        /// </summary>
        public string AnalysisModeText
        {
            get
            {
                this.OnPropertyChanged("AnalysisChangeLinesText");
                this.OnPropertyChanged("IsServerAnalysisOn");
                return this.analysisModeText.ToString();
            }
        }

        /// <summary>
        /// Gets a value indicating whether is solution open.
        /// </summary>
        public bool IsSolutionOpen
        {
            get
            {
                return this.AssociatedProject != null;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether analysis mode.
        /// </summary>
        public bool AnalysisType
        {
            get
            {
                return this.analysisType;
            }

            set
            {
                this.analysisType = value;
                this.analysisTrigger = false;
                this.OnPropertyChanged("AnalysisTriggerText");
                this.OnPropertyChanged("AnalysisTrigger");
                if (this.analysisTypeText.Equals(AnalysisTypes.Analysis))
                {
                    this.analysisTypeText = AnalysisTypes.Preview;
                }
                else
                {
                    this.analysisTypeText += 1;
                }

                if (this.analysisModeText.Equals(AnalysisModes.Server))
                {
                    this.analysisTypeText = AnalysisTypes.File;
                }

                this.OnPropertyChanged("AnalysisTypeText");
                this.OnPropertyChanged("AnalysisType");
            }
        }

        /// <summary>
        ///     Gets the analysis mode.
        /// </summary>
        public string AnalysisTypeText
        {
            get
            {
                return this.analysisTypeText.ToString();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether analysis trigger.
        /// </summary>
        public bool AnalysisTrigger
        {
            get
            {
                return this.analysisTrigger;
            }

            set
            {
                this.analysisTrigger = value;

                if (this.CustomPane != null && this.analysisTrigger)
                {
                    this.CustomPane.Clear();
                }

                this.PerformfAnalysis(value);
            }
        }

        /// <summary>
        /// Gets the coverage is on text.
        /// </summary>
        public string CoverageIsOnText
        {
            get
            {
                return this.IsCoverageOn ? "Turn Off" : "Turn On";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is coverage on.
        /// </summary>
        public bool IsCoverageOn
        {
            get
            {
                return this.isCoverageOn;
            }

            set
            {
                this.isCoverageOn = value;
                this.CoverageInEditorEnabled = this.isCoverageOn;
                this.OnPropertyChanged("CoverageIsOnText");
                this.OnPropertyChanged("IsCoverageOn");
            }            
        }

        /// <summary>
        /// Gets or sets a value indicating whether is source diff on.
        /// </summary>
        public bool IsSourceDiffOn
        {
            get
            {
                return true;
            }

            set
            {
                this.DisplayDiferenceToServerSource();
            }
        }

        /// <summary>
        /// Gets or sets the output log.
        /// </summary>
        public string OutputLog { get; set; }

        /// <summary>
        /// Gets or sets the local analyser module.
        /// </summary>
        public ISonarLocalAnalyser LocalAnalyserModule { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The permorf analysis.
        /// </summary>
        /// <param name="startStop">
        /// The start Stop.
        /// </param>
        private void PerformfAnalysis(bool startStop)
        {
            switch (this.analysisModeText)
            {
                case AnalysisModes.Server:
                    this.RunServerAnalysis(startStop);
                    this.OnPropertyChanged("AnalysisTriggerText");
                    this.OnPropertyChanged("AnalysisTrigger");
                    break;

                case AnalysisModes.Local:
                    this.RunLocalAnalysis(startStop, this.analysisTypeText);
                    break;
            }
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
        private void RunLocalAnalysis(bool startStop, AnalysisTypes analysis)
        {
            if (!startStop)
            {
                this.LocalAnalyserModule.StopAllExecution();
                return;
            }

            if (this.LocalAnalyserModule.IsExecuting())
            {
                return;
            }

            try
            {
                this.IssuesInViewAreLocked = false;
                this.OutputLog = string.Empty;
                switch (analysis)
                {
                    case AnalysisTypes.File:
                        this.LocalAnalyserModule.AnalyseFile(
                            this.vsenvironmenthelper.VsProjectItem(this.DocumentInView),
                            this.AssociatedProject,
                            this.Profile,
                            this.AnalysisChangeLines,
                            this.SonarVersion,
                            this.UserConfiguration);
                        break;
                    case AnalysisTypes.Analysis:
                        this.LocalAnalyserModule.RunFullAnalysis(
                            this.vsenvironmenthelper.ActiveSolutionPath(),
                            this.AssociatedProject,
                            this.SonarVersion,
                            this.UserConfiguration);
                        break;
                    case AnalysisTypes.Incremental:
                        this.LocalAnalyserModule.RunIncrementalAnalysis(
                            this.vsenvironmenthelper.ActiveSolutionPath(),
                            this.AssociatedProject,
                            this.Profile,
                            this.SonarVersion,
                            this.UserConfiguration);
                        break;
                    case AnalysisTypes.Preview:
                        this.LocalAnalyserModule.RunPreviewAnalysis(
                            this.vsenvironmenthelper.ActiveSolutionPath(),
                            this.AssociatedProject,
                            this.Profile,
                            this.SonarVersion,
                            this.UserConfiguration);
                        break;
                }
            }
            catch (VSSonarExtension ex)
            {
                UserExceptionMessageBox.ShowException("Analysis Failed: ", ex);
                this.ResetAnalysisTrigger();
            }
            catch (Exception ex)
            {
                UserExceptionMessageBox.ShowException("Critical Error: Please Report This Error: ", ex);
                this.ResetAnalysisTrigger();
            }            
        }

        /// <summary>
        /// The reset analysis trigger.
        /// </summary>
        private void ResetAnalysisTrigger()
        {
            this.IssuesInViewAreLocked = true;
            this.analysisTrigger = false;
            this.OnPropertyChanged("AnalysisTriggerText");
            this.OnPropertyChanged("AnalysisTrigger");
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
            try
            {
                var exceptionMsg = (LocalAnalysisEventArgs)e;
                if (exceptionMsg != null && exceptionMsg.Ex != null)
                {
                    if (this.analysisTypeText != AnalysisTypes.File)
                    {
                        this.analysisTrigger = false;
                    }

                    this.OnPropertyChanged("AnalysisTriggerText");
                    this.OnPropertyChanged("AnalysisTrigger");                    
                    UserExceptionMessageBox.ShowException("Analysis Ended: " + exceptionMsg.ErrorMessage, exceptionMsg.Ex, this.OutputLog);
                    return;
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "Failed to retrive issues from Plugin";
                this.DiagnosticMessage = ex.StackTrace;
                Debug.WriteLine("ex: " + ex.Message + " error: " + ex.StackTrace);
            }

            try
            {                
                this.ReplaceAllIssuesInCache(this.LocalAnalyserModule.GetIssues(this.UserConfiguration));
                this.ErrorMessage = string.Empty;
                this.DiagnosticMessage = string.Empty;
            }
            catch (Exception ex)
            {
                if (this.analysisTypeText != AnalysisTypes.File)
                {
                    this.analysisTrigger = false;
                }

                this.ErrorMessage = "Failed to retrive issues from Plugin";
                this.DiagnosticMessage = ex.StackTrace;
                Debug.WriteLine("ex: " + ex.Message + " error: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// The update output messages from plugin.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void UpdateOutputMessagesFromPlugin(object sender, EventArgs e)
        {
            var exceptionMsg = (LocalAnalysisEventArgs)e;
            if (this.CustomPane != null)
            {
                this.OutputLog += exceptionMsg.ErrorMessage + "\r\n";
                this.CustomPane.OutputString(exceptionMsg.ErrorMessage + "\r\n");
                this.CustomPane.FlushToTaskList();
            }
        }

        /// <summary>
        /// The run server analysis.
        /// </summary>
        /// <param name="startStop">
        /// The start Stop.
        /// </param>
        private void RunServerAnalysis(bool startStop)
        {
            if (this.ResourceInEditor == null || !startStop)
            {
                this.TriggerUpdateSignals();
                return;
            }

            this.RefreshDataForResource(this.DocumentInView);
        }

        #endregion
    }
}