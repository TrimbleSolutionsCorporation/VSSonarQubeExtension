// --------------------------------------------------------------------------------------------------------------------
// <copyright file="createruleviewmodel.cs" company="Copyright © 2014 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
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

namespace SqaleUi.ViewModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    using PropertyChanged;

    using SqaleUi.Menus;
    using System.Windows.Input;
    using ViewModel;    /// <summary>
                        ///     The create rule view model.
                        /// </summary>
    [ImplementPropertyChanged]
    public class CreateRuleViewModel : IViewModelTheme
    {
        #region Fields

        /// <summary>
        ///     The selected rule.
        /// </summary>
        private Rule selectedRule;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CreateRuleViewModel" /> class.
        /// </summary>
        public CreateRuleViewModel()
        {
            this.TemplateRules = new ObservableCollection<Rule>();
            this.BackGroundColor = Colors.Black;
            this.ForeGroundColor = Colors.White;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRuleViewModel"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public CreateRuleViewModel(ISqaleGridVm model, ISonarRestService service, ISonarConfiguration configuration)
        {
            this.BackGroundColor = Colors.Black;
            this.ForeGroundColor = Colors.White;
            this.Model = model;
            this.Profile = new Profile(this.service, this.configuration);
            this.TemplateRules = new ObservableCollection<Rule>();
            this.ExecuteRefreshCustomRuleCommand(null);
            this.service = service;
            this.configuration = configuration;

            this.CanExecuteCreateCustomRuleCommand = false;
            this.CreateCustomRuleCommand = new RelayCommand<object>(this.ExecuteCreateCustomRuleCommand);
            this.RefreshCustomRuleCommand = new RelayCommand<object>(this.ExecuteRefreshCustomRuleCommand);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRuleViewModel"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="selectedProfile">
        /// The selected profile.
        /// </param>
        public CreateRuleViewModel(ISqaleGridVm model, Profile selectedProfile)
        {
            this.Model = model;
            this.Profile = selectedProfile;
            this.TemplateRules = new ObservableCollection<Rule>();
            this.ExecuteRefreshCustomRuleCommand(null);

            this.CanExecuteCreateCustomRuleCommand = false;
            this.CreateCustomRuleCommand = new RelayCommand<object>(this.ExecuteCreateCustomRuleCommand);
            this.RefreshCustomRuleCommand = new RelayCommand<object>(this.ExecuteRefreshCustomRuleCommand);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether can execute create custom rule command.
        /// </summary>
        public bool CanExecuteCreateCustomRuleCommand { get; set; }

        /// <summary>
        ///     Gets or sets the create custom rule command.
        /// </summary>
        public ICommand CreateCustomRuleCommand { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public ISqaleGridVm Model { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the profile.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Gets or sets the refresh custom rule command.
        /// </summary>
        public ICommand RefreshCustomRuleCommand { get; set; }

        /// <summary>
        ///     Gets or sets the selected rule.
        /// </summary>
        public Rule SelectedRule
        {
            get
            {
                return this.selectedRule;
            }

            set
            {
                this.selectedRule = value;
                if (value != null)
                {
                    this.CanExecuteCreateCustomRuleCommand = true;
                    this.SelectedSeverity = value.Severity;
                }
                else
                {
                    this.CanExecuteCreateCustomRuleCommand = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected severity.
        /// </summary>
        public Severity SelectedSeverity { get; set; }

        /// <summary>
        ///     Gets or sets the template rules.
        /// </summary>
        public ObservableCollection<Rule> TemplateRules { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     The execute create custom rule command.
        /// </summary>
        private void ExecuteCreateCustomRuleCommand(object data)
        {
            if (string.IsNullOrEmpty(this.Name) || string.IsNullOrEmpty(this.Key) || string.IsNullOrEmpty(this.Description))
            {
                MessageBox.Show("Cannot add rule, some elements have not been set");
                return;
            }

            var rule = this.SelectedRule.Clone() as Rule;
            if (rule != null)
            {
                rule.Name = this.Name;
                rule.Key = this.Key;
                rule.Severity = this.SelectedSeverity;
                rule.HtmlDescription = this.Description;

                List<string> errors = this.Model.RestService.CreateRule(this.Model.Configuration, rule, this.SelectedRule);

                if (errors != null && errors.Count != 0)
                {
                    MessageBox.Show("Cannot Update Status Of Data in Server: " + errors.Aggregate(this.Model.AggregateErrorStrings));
                }
                else
                {
                    this.Model.ProfileRules.Add(rule);
                    MessageBox.Show("Rule Added");
                }
            }
        }

        /// <summary>
        /// The execute refresh custom rule command.
        /// </summary>
        private void ExecuteRefreshCustomRuleCommand(object data)
        {
            this.TemplateRules.Clear();
            
            if (this.Profile.Key == null)
            {
                this.Profile.Key = this.Model.QualityViewerModel.SelectedProfile.Key;
                this.Profile.Language = this.Model.QualityViewerModel.SelectedProfile.Language;
                this.Model.RestService.GetTemplateRules(this.Model.Configuration, this.Profile);
                foreach (Rule rule in this.Profile.GetAllRules())
                {
                    this.TemplateRules.Add(rule);
                }
            }
            else
            {
                var profileTag = new Profile(this.service, this.configuration);
                profileTag.Key = this.Profile.Key;
                profileTag.Language = this.Profile.Language;

                this.Model.RestService.GetTemplateRules(this.Model.Configuration, profileTag);
                foreach (var rule in profileTag.GetAllRules())
                {
                    this.TemplateRules.Add(rule);
                }
            }
        }

        public void UpdateColors(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        public Color BackGroundColor { get; set; }

        public Color ForeGroundColor { get; set; }

        #endregion

        public ISonarConfiguration configuration { get; set; }
        public ISonarRestService service { get; set; }
    }
}