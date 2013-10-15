// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarTagDisplayViolationsAction.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.SmartTags.ContextMenu
{
    using System;
    using System.Collections.ObjectModel;

    using ExtensionTypes;

    using ExtensionViewModel.ViewModel;

    using Microsoft.VisualStudio.Language.Intellisense;

    /// <summary>
    /// The sonar smart tag action.
    /// </summary>
    public class SonarTagDisplayViolationsAction : ISmartTagAction
    {
        /// <summary>
        /// The violation.
        /// </summary>
        private readonly Issue issue;

        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly ExtensionDataModel model;

        /// <summary>
        /// The enabled.
        /// </summary>
        private readonly bool enabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarTagDisplayViolationsAction"/> class.
        /// </summary>
        /// <param name="issue">
        /// The issue.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="enabledIn">
        /// The enabled In.
        /// </param>
        public SonarTagDisplayViolationsAction(Issue issue, ExtensionDataModel model, bool enabledIn = true)
        {
            this.model = model;
            this.issue = issue;
            this.enabled = enabledIn;
        }

        /// <summary>
        /// Gets the display text.
        /// </summary>
        public string DisplayText
        {
            get { return this.issue.Message; }
        }

        /// <summary>
        /// Gets the icon.
        /// </summary>
        public System.Windows.Media.ImageSource Icon
        {
            get { return null; }
        }

        /// <summary>
        /// Gets a value indicating whether is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get { return this.enabled; }
        }

        /// <summary>
        /// Gets the action sets.
        /// </summary>
        public ReadOnlyCollection<SmartTagActionSet> ActionSets
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// This method is executed when action is selected in the context menu.
        /// </summary>
        public void Invoke()
        {
            this.issue.Line = this.issue.Line + 1;

            if (this.issue.Key != new Guid())
            {
                this.model.SelectAIssueFromList(this.issue.Key);
            }
            else
            {
                this.model.SelectAIssueFromList(this.issue.Id);
            }                
        }
    }
}
