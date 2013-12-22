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
namespace ExtensionViewModel.ViewModel
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;

    using ExtensionTypes;

    using VSSonarPlugins;

    /// <summary>
    ///     The extension data model.
    /// </summary>
    public partial class ExtensionDataModel
    {
        #region Fields

        /// <summary>
        ///     The analysis change lines.
        /// </summary>
        private bool analysisChangeLines;

        /// <summary>
        ///     The analysis change lines text.
        /// </summary>
        private string analysisChangeLinesText = "Yes";

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
        private enum AnalysisTypes
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
            Analysis
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether is issues in modified lines enabled.
        /// </summary>
        public bool IsIssuesInModifiedLinesEnabled
        {
            get
            {
                return this.analysisChangeLines;
            }

            set
            {
                this.analysisChangeLines = value;
                this.OnPropertyChanged("IsIssuesInModifiedLinesEnabled");
            }
        }

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
                this.OnPropertyChanged("AnalysisChangeLines");
                this.OnPropertyChanged("AnalysisChangeLinesText");
            }
        }

        /// <summary>
        ///     Gets the analysis mode.
        /// </summary>
        public string AnalysisChangeLinesText
        {
            get
            {
                if (this.analysisModeText.Equals(AnalysisModes.Server))
                {
                    this.analysisChangeLines = false;
                    return "No";
                }

                this.analysisChangeLinesText = this.analysisChangeLines ? "Yes" : "No";

                return this.analysisChangeLinesText;
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
        /// Gets or sets the analysis log.
        /// </summary>
        public string AnalysisLog { get; set; }
        
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
                if (this.analysisTypeText.Equals(AnalysisTypes.File))
                {
                    this.IsIssuesInModifiedLinesEnabled = true;
                }
                else
                {
                    this.IsIssuesInModifiedLinesEnabled = false;
                }

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
                this.PerformfAnalysis(value);
                this.AnalysisLog = string.Empty;
                this.OnPropertyChanged("AnalysisTriggerText");
                this.OnPropertyChanged("AnalysisTrigger");
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
            this.IssuesInViewAreLocked = false;
            switch (this.analysisModeText)
            {
                case AnalysisModes.Server:
                    this.RunServerAnalysis(startStop);
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
            if (this.PluginRunningAnalysis == null || (this.localAnalyserThread != null && this.localAnalyserThread.IsAlive))
            {
                return;
            }

            if (!startStop)
            {
                if (this.localAnalyserThread == null)
                {
                    return;
                }

                if (!this.localAnalyserThread.IsAlive)
                {
                    return;
                }

                this.localAnalyserThread.Abort();
                this.localAnalyserThread.Join();

                return;
            }
        
            if (this.ExtensionRunningLocalAnalysis == null)
            {                
                MessageBox.Show("Current Plugin does not support Local analysis");
                this.analysisTrigger = false;
                this.OnPropertyChanged("AnalysisTriggerText");
                this.OnPropertyChanged("AnalysisTrigger");
                return;
            }

            try
            {            
                this.ExtensionRunningLocalAnalysis.StdErrEvent += this.UpdateOutputMessagesFromPlugin;
                this.ExtensionRunningLocalAnalysis.StdOutEvent += this.UpdateOutputMessagesFromPlugin;

                switch (analysis)
                {
                    case AnalysisTypes.File:
                        this.ExtensionRunningLocalAnalysis.LocalAnalysisCompleted += this.UpdateLocalIssuesForFileAnalysis;
                        this.localAnalyserThread = this.ExtensionRunningLocalAnalysis.GetFileAnalyserThread(this.vsenvironmenthelper.VsProjectItem(this.DocumentInView), this.AssociatedProject.Key);
                        break;
                    case AnalysisTypes.Analysis:
                        this.ExtensionRunningLocalAnalysis.LocalAnalysisCompleted += this.UpdateLocalIssues;
                        this.localAnalyserThread = this.ExtensionRunningLocalAnalysis.GetAnalyserThread(this.vsenvironmenthelper.ActiveSolutionPath());
                        break;
                    case AnalysisTypes.Incremental:
                        this.ExtensionRunningLocalAnalysis.LocalAnalysisCompleted += this.UpdateLocalIssues;
                        this.localAnalyserThread = this.ExtensionRunningLocalAnalysis.GetIncrementalAnalyserThread(this.vsenvironmenthelper.ActiveSolutionPath());
                        break;
                    case AnalysisTypes.Preview:
                        this.ExtensionRunningLocalAnalysis.LocalAnalysisCompleted += this.UpdateLocalIssues;
                        this.localAnalyserThread = this.ExtensionRunningLocalAnalysis.GetPreviewAnalyserThread(this.vsenvironmenthelper.ActiveSolutionPath());
                        break;
                }

                if (this.localAnalyserThread == null)
                {
                    MessageBox.Show("Analysis Type Not Supported By Plugin");
                    this.analysisTrigger = false;
                    this.OnPropertyChanged("AnalysisTriggerText");
                    this.OnPropertyChanged("AnalysisTrigger");
                    this.ExtensionRunningLocalAnalysis.LocalAnalysisCompleted -= this.UpdateLocalIssuesForFileAnalysis;
                    this.ExtensionRunningLocalAnalysis.StdErrEvent -= this.UpdateOutputMessagesFromPlugin;
                    this.ExtensionRunningLocalAnalysis.StdOutEvent -= this.UpdateOutputMessagesFromPlugin;
                    return;
                }

                this.localAnalyserThread.Start();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "Analysis Type Not Supported By Plugin";
                this.analysisTrigger = false;
                this.OnPropertyChanged("AnalysisTriggerText");
                this.OnPropertyChanged("AnalysisTrigger");
                this.DiagnosticMessage = ex.StackTrace;
                this.ExtensionRunningLocalAnalysis.LocalAnalysisCompleted -= this.UpdateLocalIssuesForFileAnalysis;
                this.ExtensionRunningLocalAnalysis.StdErrEvent -= this.UpdateOutputMessagesFromPlugin;
                this.ExtensionRunningLocalAnalysis.StdOutEvent -= this.UpdateOutputMessagesFromPlugin;
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
            try
            {
                var exceptionMsg = (LocalAnalysisCompletedEventArgs)e;
                if (exceptionMsg.Ex != null)
                {
                    MessageBox.Show(
                        "Cannot Execute Analysis: " + exceptionMsg.ErrorMessage + " StackTrace:"
                        + exceptionMsg.Ex.StackTrace);
                    this.ExtensionRunningLocalAnalysis.LocalAnalysisCompleted -= this.UpdateLocalIssues;
                    this.ExtensionRunningLocalAnalysis.StdErrEvent -= this.UpdateOutputMessagesFromPlugin;
                    this.ExtensionRunningLocalAnalysis.StdOutEvent -= this.UpdateOutputMessagesFromPlugin;
                    this.analysisTrigger = false;
                    this.OnPropertyChanged("AnalysisTriggerText");
                    this.OnPropertyChanged("AnalysisTrigger");
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
                this.ExtensionRunningLocalAnalysis.LocalAnalysisCompleted -= this.UpdateLocalIssues;
                this.ExtensionRunningLocalAnalysis.StdErrEvent -= this.UpdateOutputMessagesFromPlugin;
                this.ExtensionRunningLocalAnalysis.StdOutEvent -= this.UpdateOutputMessagesFromPlugin;

                this.ReplaceAllIssuesInCache(this.ExtensionRunningLocalAnalysis.GetIssues());
                this.ErrorMessage = string.Empty;
                this.DiagnosticMessage = string.Empty;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "Failed to retrive issues from Plugin";
                this.DiagnosticMessage = ex.StackTrace;
                Debug.WriteLine("ex: " + ex.Message + " error: " + ex.StackTrace);
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
        private void UpdateLocalIssuesForFileAnalysis(object sender, EventArgs e)
        {
            try
            {
                var exceptionMsg = (LocalAnalysisCompletedEventArgs)e;
                if (exceptionMsg.Ex != null)
                {
                    MessageBox.Show(
                        "Cannot Execute Analysis: " + exceptionMsg.ErrorMessage + " StackTrace:"
                        + exceptionMsg.Ex.StackTrace);
                    this.ExtensionRunningLocalAnalysis.LocalAnalysisCompleted -= this.UpdateLocalIssuesForFileAnalysis;
                    this.ExtensionRunningLocalAnalysis.StdErrEvent -= this.UpdateOutputMessagesFromPlugin;
                    this.ExtensionRunningLocalAnalysis.StdOutEvent -= this.UpdateOutputMessagesFromPlugin;
                    this.analysisTrigger = false;
                    this.OnPropertyChanged("AnalysisTriggerText");
                    this.OnPropertyChanged("AnalysisTrigger");
                    return;
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "Failed to retrive issues from Plugin";
                this.DiagnosticMessage = ex.StackTrace;
                Debug.WriteLine("ex: " + ex.Message + " error: " + ex.StackTrace);
            }

            if (this.ResourceInEditor == null)
            {
                return;
            }

            try
            {
                this.ExtensionRunningLocalAnalysis.LocalAnalysisCompleted -= this.UpdateLocalIssuesForFileAnalysis;
                this.ExtensionRunningLocalAnalysis.StdErrEvent -= this.UpdateOutputMessagesFromPlugin;
                this.ExtensionRunningLocalAnalysis.StdOutEvent -= this.UpdateOutputMessagesFromPlugin;

                var issuesInExtension = this.ExtensionRunningLocalAnalysis.GetIssues();

                if (issuesInExtension.Count == 0)
                {
                    return;
                }

                foreach (var issue in issuesInExtension.ToList())
                {
                    var ruleInProfile = Profile.IsRuleEnabled(this.Profile, issue.Rule);
                    if (ruleInProfile == null)
                    {
                        issuesInExtension.Remove(issue);
                    }
                    else
                    {
                        issue.Severity = ruleInProfile.Severity;
                    }
                }

                this.localEditorCache.UpdateIssues(issuesInExtension);
                this.OnPropertyChanged("IssuesInEditor");
                this.OnPropertyChanged("Issues");
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "Local Analysis Failed";
                this.DiagnosticMessage = ex.StackTrace;
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
            var exceptionMsg = (LocalAnalysisCompletedEventArgs)e;
            this.AnalysisLog += exceptionMsg.ErrorMessage + "\r\n";
            this.OnPropertyChanged("AnalysisLog");
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