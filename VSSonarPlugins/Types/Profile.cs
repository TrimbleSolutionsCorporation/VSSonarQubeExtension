// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Profile.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarPlugins.Types
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using PropertyChanged;

    /// <summary>
    /// The profile.
    /// </summary>
    [Serializable]
    [ImplementPropertyChanged]
    public class Profile
    {
        private readonly ISonarRestService service;
        private readonly ISonarConfiguration conf;

        public Profile(ISonarRestService service, ISonarConfiguration conf)
        {
            this.conf = conf;
            this.service = service;
            this.Rules = new Dictionary<string, Rule>();
            this.RulesByIternalKey = new Dictionary<string, Rule>();
        }

        private Dictionary<string, Rule> RulesByIternalKey { get; set; }

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
        private Dictionary<string, Rule> Rules { get; set; }

        /// <summary>
        /// Gets or sets the projects.
        /// </summary>
        public List<SonarProject> Projects { get; set; }

        /// <summary>The get rule.</summary>
        /// <param name="internalKeyOrConfigKey">The internal Key Or Key.</param>
        /// <returns>The <see cref="Rule"/>.</returns>
        public Rule GetRule(string internalKeyOrConfigKey)
        {
            try
            {
                if (this.Rules.ContainsKey(internalKeyOrConfigKey))
                {
                    var rule = this.Rules[internalKeyOrConfigKey];
                    this.UpdateRuleData(rule);
                    return rule;
                }

                if (this.RulesByIternalKey.ContainsKey(internalKeyOrConfigKey))
                {
                    var rule = this.RulesByIternalKey[internalKeyOrConfigKey];
                    this.UpdateRuleData(rule);
                    return rule;
                }

                return null;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        private void UpdateRuleData(Rule rule)
        {
            if (rule.IsParamsRetrivedFromServer)
            {
                return;
            }

            this.service.UpdateRuleData(this.conf, rule);
        }


        /// <summary>The get all rules.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        public List<Rule> GetAllRules()
        {
            return this.Rules.Values.ToList();
        }

        /// <summary>
        /// The create rule.
        /// </summary>
        /// <param name="rule">
        /// The rule.
        /// </param>
        public void AddRule(Rule rule)
        {
            if (rule.ConfigKey == null)
            {
                rule.ConfigKey = rule.Repo + ":" + rule.Key;
            }

            if (!this.Rules.ContainsKey(rule.ConfigKey))
            {
                this.Rules.Add(rule.ConfigKey, rule);
            }

            if (!string.IsNullOrEmpty(rule.InternalKey))
            {
                if (this.RulesByIternalKey.ContainsKey(rule.InternalKey))
                {
                    return;
                }

                this.RulesByIternalKey.Add(rule.InternalKey, rule);
            }
        }
    }
}