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
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    using ExtensionHelpers;

    using ExtensionTypes;

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

                this.analysisChangeLinesText = this.analysisChangeLines ? "No" : "Yes";

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
                this.analysisModeText = value ? AnalysisModes.Server : AnalysisModes.Local;
                if (this.analysisModeText.Equals(AnalysisModes.Server))
                {
                    this.analysisTypeText = AnalysisTypes.File;
                    this.OnPropertyChanged("AnalysisTypeText");
                }

                this.OnPropertyChanged("AnalysisModeText");
                this.OnPropertyChanged("AnalysisMode");
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
        ///     Gets the analysis mode text.
        /// </summary>
        public string AnalysisModeText
        {
            get
            {
                this.OnPropertyChanged("AnalysisChangeLinesText");
                return this.analysisModeText.ToString();
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
                this.PerformfAnalysis(value);
                this.OnPropertyChanged("AnalysisTriggerText");
                this.OnPropertyChanged("AnalysisTrigger");
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

            this.ExtensionRunningLocalAnalysis = this.PluginRunningAnalysis.GetLocalAnalysisExtension();
            if (this.ExtensionRunningLocalAnalysis == null)
            {
                this.IssuesInEditor = new List<Issue>();
                this.Issues = new List<Issue>();
                MessageBox.Show("Current Plugin does not support Local analysis");
                return;
            }

            this.ExtensionRunningLocalAnalysis.LocalAnalysisCompleted += this.UpdateLocalIssuesInView;
            switch (analysis)
            {
                case AnalysisTypes.File:
                    this.localAnalyserThread = this.ExtensionRunningLocalAnalysis.GetFileAnalyserThread(this.DocumentInView);
                    break;
                case AnalysisTypes.Analysis:
                    this.localAnalyserThread = this.ExtensionRunningLocalAnalysis.GetAnalyserThread();
                    break;
                case AnalysisTypes.Incremental:
                    this.localAnalyserThread = this.ExtensionRunningLocalAnalysis.GetIncrementalAnalyserThread();
                    break;
                case AnalysisTypes.Preview:
                    this.localAnalyserThread = this.ExtensionRunningLocalAnalysis.GetPreviewAnalyserThread();
                    break;
            }
            
            if (this.localAnalyserThread == null)
            {
                this.IssuesInEditor = new List<Issue>();
                this.Issues = new List<Issue>();
                MessageBox.Show("Current Plugin does not support Local File analysis");
                return;
            }

            this.localAnalyserThread.Start();
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
        private void UpdateLocalIssuesInView(object sender, EventArgs e)
        {
            if (this.ResourceInEditor == null)
            {
                return;
            }

            try
            {
                var issuesInExtension = this.ExtensionRunningLocalAnalysis.GetIssues();
                if (issuesInExtension.Count == 0)
                {
                    return;
                }

                var firstNonNullELems = issuesInExtension.First();

                foreach (var issue in issuesInExtension.ToList().Where(issue => issue.Component != null))
                {
                    firstNonNullELems = issue;
                    break;
                }

                if (firstNonNullELems.Component == null
                    || !firstNonNullELems.Component.Replace('\\', '/')
                            .Equals(this.DocumentInView, StringComparison.OrdinalIgnoreCase))
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

                if (!this.AnalysisChangeLines)
                {
                    var diffReport = VsSonarUtils.GetDifferenceReport(
                        this.DocumentInView,
                        this.UpdateSourceDataForResource(this.ResourceInEditor.Key, false),
                        false);
                    var issuesInModifiedLines = VsSonarUtils.GetIssuesInModifiedLinesOnly(
                        issuesInExtension,
                        diffReport);
                    this.IssuesInEditor = issuesInModifiedLines;
                }
                else
                {
                    this.IssuesInEditor = issuesInExtension;
                }

                if (!this.IssuesInViewLocked)
                {
                    this.Issues = this.IssuesInEditor;
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "Local Analysis Failed";
                this.DiagnosticMessage = ex.StackTrace;
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
                this.Issues = new List<Issue>();
                this.IssuesInEditor = new List<Issue>();
                return;
            }

            var issuesForResource = this.UpdateIssueDataForResource(this.ResourceInEditor.Key);
            this.UpdateSourceDataForResource(this.ResourceInEditor.Key, false);

            if (!this.IssuesInViewLocked)
            {
                this.Issues = issuesForResource;
            }

            this.LastReferenceSource = VsSonarUtils.GetLinesFromSource(
                this.allSourceData[this.ResourceInEditor.Key],
                "\r\n");

            this.IssuesInEditor = VsSonarUtils.ConvertIssuesToLocal(
                issuesForResource,
                this.ResourceInEditor,
                this.currentBuffer,
                this.LastReferenceSource);
        }

        #endregion
    }
}