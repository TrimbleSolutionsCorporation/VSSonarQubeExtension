// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataGridControlViewModel.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    ///     The extension data model.
    /// </summary>
    public partial class ExtensionDataModel
    {
        #region Fields

        /// <summary>
        ///     The data grid options key.
        /// </summary>
        private const string DataGridOptionsKey = "DataGridOptions";

        /// <summary>
        ///     The assignee.
        /// </summary>
        private bool assignee;

        /// <summary>
        ///     The assignee index.
        /// </summary>
        private int assigneeIndex;

        /// <summary>
        ///     The close date.
        /// </summary>
        private bool closeDate;

        /// <summary>
        ///     The close date index.
        /// </summary>
        private int closeDateIndex;

        /// <summary>
        ///     The component.
        /// </summary>
        private bool component;

        /// <summary>
        ///     The component index.
        /// </summary>
        private int componentIndex;

        /// <summary>
        ///     The creation date.
        /// </summary>
        private bool creationDate;

        /// <summary>
        ///     The creation date index.
        /// </summary>
        private int creationDateIndex;

        /// <summary>
        ///     The effort to fix.
        /// </summary>
        private bool effortToFix;

        /// <summary>
        ///     The effort to fix index.
        /// </summary>
        private int effortToFixIndex;

        /// <summary>
        ///     The id.
        /// </summary>
        private bool id;

        /// <summary>
        ///     The id index.
        /// </summary>
        private int idIndex;

        /// <summary>
        ///     The key.
        /// </summary>
        private bool key;

        /// <summary>
        ///     The key index.
        /// </summary>
        private int keyIndex;

        /// <summary>
        ///     The line.
        /// </summary>
        private bool line;

        /// <summary>
        ///     The line index.
        /// </summary>
        private int lineIndex;

        /// <summary>
        ///     The message.
        /// </summary>
        private bool message;

        /// <summary>
        ///     The message index.
        /// </summary>
        private int messageIndex;

        /// <summary>
        ///     The project.
        /// </summary>
        private bool project;

        /// <summary>
        ///     The project index.
        /// </summary>
        private int projectIndex;

        /// <summary>
        ///     The resolution.
        /// </summary>
        private bool resolution;

        /// <summary>
        ///     The resolution index.
        /// </summary>
        private int resolutionIndex;

        /// <summary>
        ///     The rule.
        /// </summary>
        private bool rule;

        /// <summary>
        ///     The rule index.
        /// </summary>
        private int ruleIndex;

        /// <summary>
        ///     The severity.
        /// </summary>
        private bool severity;

        /// <summary>
        ///     The severity index.
        /// </summary>
        private int severityIndex;

        /// <summary>
        ///     The status.
        /// </summary>
        private bool status;

        /// <summary>
        ///     The status index.
        /// </summary>
        private int statusIndex;

        /// <summary>
        ///     The update date.
        /// </summary>
        private bool updateDate;

        /// <summary>
        ///     The update date index.
        /// </summary>
        private int updateDateIndex;

        private bool queryForIssuesIsRunning = false;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the assignee index.
        /// </summary>
        public int AssigneeIndex
        {
            get
            {
                return this.assigneeIndex;
            }

            set
            {
                this.assigneeIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "AssigneeIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("AssigneeIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether assignee visible.
        /// </summary>
        public bool AssigneeVisible
        {
            get
            {
                return this.assignee;
            }

            set
            {
                this.assignee = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "AssigneeVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("AssigneeVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the close date index.
        /// </summary>
        public int CloseDateIndex
        {
            get
            {
                return this.closeDateIndex;
            }

            set
            {
                this.closeDateIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "CloseDateIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("CloseDateIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether close date visible.
        /// </summary>
        public bool CloseDateVisible
        {
            get
            {
                return this.closeDate;
            }

            set
            {
                this.closeDate = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "CloseDateVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("CloseDateVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the component index.
        /// </summary>
        public int ComponentIndex
        {
            get
            {
                return this.componentIndex;
            }

            set
            {
                this.componentIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "ComponentIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("ComponentIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether component visible.
        /// </summary>
        public bool ComponentVisible
        {
            get
            {
                return this.component;
            }

            set
            {
                this.component = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "ComponentVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("ComponentVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the creation date index.
        /// </summary>
        public int CreationDateIndex
        {
            get
            {
                return this.creationDateIndex;
            }

            set
            {
                this.creationDateIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "CreationDateIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("CreationDateIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether Creation Date visible.
        /// </summary>
        public bool CreationDateVisible
        {
            get
            {
                return this.creationDate;
            }

            set
            {
                this.creationDate = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "CreationDateVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("CreationDateVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the effort to fix index.
        /// </summary>
        public int EffortToFixIndex
        {
            get
            {
                return this.effortToFixIndex;
            }

            set
            {
                this.effortToFixIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "EffortToFixIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("EffortToFixIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether effort to fix is visible.
        /// </summary>
        public bool EffortToFixVisible
        {
            get
            {
                return this.effortToFix;
            }

            set
            {
                this.effortToFix = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "EffortToFixVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("EffortToFixVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the id index.
        /// </summary>
        public int IdIndex
        {
            get
            {
                return this.idIndex;
            }

            set
            {
                this.idIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "IdIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("IdIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether id visible.
        /// </summary>
        public bool IdVisible
        {
            get
            {
                return this.id;
            }

            set
            {
                this.id = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey,
                        "IdVisible",
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("IdVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the key index.
        /// </summary>
        public int KeyIndex
        {
            get
            {
                return this.keyIndex;
            }

            set
            {
                this.keyIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "KeyIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("KeyIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether key visible.
        /// </summary>
        public bool KeyVisible
        {
            get
            {
                return this.key;
            }

            set
            {
                this.key = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "KeyVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("KeyVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the line index.
        /// </summary>
        public int LineIndex
        {
            get
            {
                return this.lineIndex;
            }

            set
            {
                this.lineIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "LineIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("LineIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether line visible.
        /// </summary>
        public bool LineVisible
        {
            get
            {
                return this.line;
            }

            set
            {
                this.line = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "LineVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("LineVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the message index.
        /// </summary>
        public int MessageIndex
        {
            get
            {
                return this.messageIndex;
            }

            set
            {
                this.messageIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "MessageIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("MessageIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether message visible.
        /// </summary>
        public bool MessageVisible
        {
            get
            {
                return this.message;
            }

            set
            {
                this.message = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "MessageVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("MessageVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the project index.
        /// </summary>
        public int ProjectIndex
        {
            get
            {
                return this.projectIndex;
            }

            set
            {
                this.projectIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "ProjectIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("ProjectIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether project visible.
        /// </summary>
        public bool ProjectVisible
        {
            get
            {
                return this.project;
            }

            set
            {
                this.project = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "ProjectVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("ProjectVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the resolution index.
        /// </summary>
        public int ResolutionIndex
        {
            get
            {
                return this.resolutionIndex;
            }

            set
            {
                this.resolutionIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey,
                        "ResolutionIndex",
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("ResolutionIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether resolution visible.
        /// </summary>
        public bool ResolutionVisible
        {
            get
            {
                return this.resolution;
            }

            set
            {
                this.resolution = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "ResolutionVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("ResolutionVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the rule index.
        /// </summary>
        public int RuleIndex
        {
            get
            {
                return this.ruleIndex;
            }

            set
            {
                this.ruleIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "RuleIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("RuleIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether rule visible.
        /// </summary>
        public bool RuleVisible
        {
            get
            {
                return this.rule;
            }

            set
            {
                this.rule = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "RuleVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("RuleVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the severity index.
        /// </summary>
        public int SeverityIndex
        {
            get
            {
                return this.severityIndex;
            }

            set
            {
                this.severityIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "SeverityIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("SeverityIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether severity visible.
        /// </summary>
        public bool SeverityVisible
        {
            get
            {
                return this.severity;
            }

            set
            {
                this.severity = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "SeverityVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("SeverityVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the status index.
        /// </summary>
        public int StatusIndex
        {
            get
            {
                return this.statusIndex;
            }

            set
            {
                this.statusIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "StatusIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("StatusIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether status visible.
        /// </summary>
        public bool StatusVisible
        {
            get
            {
                return this.status;
            }

            set
            {
                this.status = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "StatusVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("StatusVisible");
            }
        }

        /// <summary>
        ///     Gets or sets the update date index.
        /// </summary>
        public int UpdateDateIndex
        {
            get
            {
                return this.updateDateIndex;
            }

            set
            {
                this.updateDateIndex = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "UpdateDateIndex", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("UpdateDateIndex");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether update date visible.
        /// </summary>
        public bool UpdateDateVisible
        {
            get
            {
                return this.updateDate;
            }

            set
            {
                this.updateDate = value;
                if (this.vsenvironmenthelper != null)
                {
                    this.vsenvironmenthelper.WriteOptionInApplicationData(
                        DataGridOptionsKey, 
                        "UpdateDateVisible", 
                        value.ToString(CultureInfo.InvariantCulture));
                }

                this.OnPropertyChanged("UpdateDateVisible");
            }
        }

        /// <summary>
        /// Gets a value indicating whether extension is busy.
        /// </summary>
        public bool ExtensionIsBusy
        {
            get
            {
                if (this.AnalysisTrigger)
                {
                    return true;
                }

                if (!this.QueryForIssuesIsRunning)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether query for issues is running.
        /// </summary>
        public bool QueryForIssuesIsRunning
        {
            get
            {
                return !this.queryForIssuesIsRunning;
            }

            set
            {
                this.queryForIssuesIsRunning = value;
                this.OnPropertyChanged("QueryForIssuesIsRunning");
                this.OnPropertyChanged("ExtensionIsBusy");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The restore user settings.
        /// </summary>
        private void RestoreUserSettingsInIssuesDataGrid()
        {
            if (this.vsenvironmenthelper == null)
            {
                this.ResetWindowDefaults();
                return;
            }

            var options = this.vsenvironmenthelper.ReadAllOptionsForPluginOptionInApplicationData(DataGridOptionsKey);
            if (options != null && options.Count > 0)
            {
                this.ReadWindowOptions(options);
            }
            else
            {
                this.WriteWindowOptions();
            }
        }

        /// <summary>
        /// The reset window defaults.
        /// </summary>
        private void ResetWindowDefaults()
        {
            this.ComponentIndex = 0;
            this.LineIndex = 1;
            this.AssigneeIndex = 2;
            this.MessageIndex = 3;
            this.StatusIndex = 4;
            this.SeverityIndex = 5;
            this.RuleIndex = 6;
            this.CreationDateIndex = 7;
            this.ProjectIndex = 8;
            this.ResolutionIndex = 9;
            this.EffortToFixIndex = 10;
            this.UpdateDateIndex = 11;
            this.CloseDateIndex = 12;
            this.KeyIndex = 13;
            this.IdIndex = 14;

            this.ComponentVisible = true;
            this.LineVisible = true;
            this.AssigneeVisible = true;
            this.MessageVisible = true;
            this.StatusVisible = true;
            this.SeverityVisible = true;
            this.RuleVisible = true;
            this.CreationDateVisible = true;
            this.ProjectVisible = true;
            this.ResolutionVisible = true;
            this.EffortToFixVisible = true;
            this.UpdateDateVisible = true;
            this.CloseDateVisible = true;
            this.KeyVisible = true;
            this.IdVisible = true;
        }

        /// <summary>
        /// The write window options.
        /// </summary>
        private void WriteWindowOptions()
        {
            this.ComponentIndex = 0;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "ComponentIndex", "0");
            this.LineIndex = 1;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "LineIndex", "1");
            this.AssigneeIndex = 2;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "AssigneeIndex", "2");
            this.MessageIndex = 3;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "MessageIndex", "3");
            this.StatusIndex = 4;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "StatusIndex", "4");
            this.SeverityIndex = 5;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "SeverityIndex", "5");
            this.RuleIndex = 6;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "RuleIndex", "6");
            this.CreationDateIndex = 7;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "CreationDateIndex", "7");
            this.ProjectIndex = 8;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "ProjectIndex", "8");
            this.ResolutionIndex = 9;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "ResolutionIndex", "9");
            this.EffortToFixIndex = 10;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "EffortToFixIndex", "10");
            this.UpdateDateIndex = 11;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "UpdateDateIndex", "11");
            this.CloseDateIndex = 12;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "CloseDateIndex", "12");
            this.KeyIndex = 13;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "KeyIndex", "13");
            this.IdIndex = 14;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "IdIndex", "14");

            this.ComponentVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "ComponentVisible", "true");
            this.LineVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "LineVisible", "true");
            this.AssigneeVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "AssigneeVisible", "true");
            this.MessageVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "MessageVisible", "true");
            this.StatusVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "StatusVisible", "true");
            this.SeverityVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "SeverityVisible", "true");
            this.RuleVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "RuleVisible", "true");
            this.CreationDateVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "CreationDateVisible", "true");
            this.ProjectVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "ProjectVisible", "true");
            this.ResolutionVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "ResolutionVisible", "true");
            this.EffortToFixVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "EffortToFixVisible", "true");
            this.UpdateDateVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "UpdateDateVisible", "true");
            this.CloseDateVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "CloseDateVisible", "true");
            this.KeyVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "KeyVisible", "true");
            this.IdVisible = true;
            this.vsenvironmenthelper.WriteOptionInApplicationData(DataGridOptionsKey, "IdVisible", "true");
        }

        /// <summary>
        /// The read window options.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        private void ReadWindowOptions(Dictionary<string, string> options)
        {
            this.ComponentIndex = int.Parse(options["ComponentIndex"], CultureInfo.InvariantCulture);
            this.LineIndex = int.Parse(options["LineIndex"], CultureInfo.InvariantCulture);
            this.AssigneeIndex = int.Parse(options["AssigneeIndex"], CultureInfo.InvariantCulture);
            this.MessageIndex = int.Parse(options["MessageIndex"], CultureInfo.InvariantCulture);
            this.StatusIndex = int.Parse(options["StatusIndex"], CultureInfo.InvariantCulture);
            this.SeverityIndex = int.Parse(options["SeverityIndex"], CultureInfo.InvariantCulture);
            this.RuleIndex = int.Parse(options["RuleIndex"], CultureInfo.InvariantCulture);
            this.CreationDateIndex = int.Parse(options["CreationDateIndex"], CultureInfo.InvariantCulture);
            this.ProjectIndex = int.Parse(options["ProjectIndex"], CultureInfo.InvariantCulture);
            this.ResolutionIndex = int.Parse(options["ResolutionIndex"], CultureInfo.InvariantCulture);
            this.EffortToFixIndex = int.Parse(options["EffortToFixIndex"], CultureInfo.InvariantCulture);
            this.UpdateDateIndex = int.Parse(options["UpdateDateIndex"], CultureInfo.InvariantCulture);
            this.CloseDateIndex = int.Parse(options["CloseDateIndex"], CultureInfo.InvariantCulture);
            this.KeyIndex = int.Parse(options["KeyIndex"], CultureInfo.InvariantCulture);
            this.IdIndex = int.Parse(options["IdIndex"], CultureInfo.InvariantCulture);

            this.ComponentVisible = bool.Parse(options["ComponentVisible"]);
            this.LineVisible = bool.Parse(options["LineVisible"]);
            this.AssigneeVisible = bool.Parse(options["AssigneeVisible"]);
            this.MessageVisible = bool.Parse(options["MessageVisible"]);
            this.StatusVisible = bool.Parse(options["StatusVisible"]);
            this.SeverityVisible = bool.Parse(options["SeverityVisible"]);
            this.RuleVisible = bool.Parse(options["RuleVisible"]);
            this.CreationDateVisible = bool.Parse(options["CreationDateVisible"]);
            this.ProjectVisible = bool.Parse(options["ProjectVisible"]);
            this.ResolutionVisible = bool.Parse(options["ResolutionVisible"]);
            this.EffortToFixVisible = bool.Parse(options["EffortToFixVisible"]);
            this.UpdateDateVisible = bool.Parse(options["UpdateDateVisible"]);
            this.CloseDateVisible = bool.Parse(options["CloseDateVisible"]);
            this.KeyVisible = bool.Parse(options["KeyVisible"]);
            this.IdVisible = bool.Parse(options["IdVisible"]);
        }

        #endregion
    }
}