// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Profile.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
namespace ExtensionTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using PropertyChanged;

    /// <summary>
    /// The profile.
    /// </summary>
    [Serializable]
    [ImplementPropertyChanged]
    public class Profile
    {
        public Profile()
        {
            this.Rules = new List<Rule>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether default.
        /// </summary>
        public bool Default { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the alerts.
        /// </summary>
        public List<Alert> Alerts { get; set; }

        /// <summary>
        /// Gets or sets the rules.
        /// </summary>
        public List<Rule> Rules { get; set; }

        /// <summary>
        /// Gets or sets the projects.
        /// </summary>
        public List<SonarProject> Projects { get; set; }

        /// <summary>
        /// The is rule enabled with repo.
        /// </summary>
        /// <param name="profile">
        /// The profile.
        /// </param>
        /// <param name="idWithRepository">
        /// The id with repository.
        /// </param>
        /// <returns>
        /// The <see cref="Rule"/>.
        /// </returns>
        public static Rule IsRuleEnabled(Profile profile, string idWithRepository)
        {
            if (profile == null)
            {
                return null;
            }

            return profile.Rules.FirstOrDefault(rule => (rule.Repo + "." + rule.Key).Equals(idWithRepository));
        }

        /// <summary>
        /// The is rule present.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsRulePresent(string key)
        {
            return this.Rules.Any(rule => rule.Key.Equals(key));
        }

        /// <summary>
        /// The get rule.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="Rule"/>.
        /// </returns>
        public Rule GetRule(string key)
        {
            return this.Rules.FirstOrDefault(rule => rule.Key.Equals(key));
        }

        /// <summary>
        /// The create rule.
        /// </summary>
        /// <param name="rule">
        /// The rule.
        /// </param>
        public void CreateRule(Rule rule)
        {
            if (!this.IsRulePresent(rule.Key))
            {
                this.Rules.Add(rule);
            }
        }
    }
}