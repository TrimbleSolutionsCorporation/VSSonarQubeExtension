// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PromptUserData.xaml.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2014 [Jorge Costa, Jorge.Costa@tekla.com]
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

namespace VSSonarExtensionUi.View.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using VSSonarPlugins.Types;

    /// <summary>
    ///     Interaction logic for PromptUserData.xaml
    /// </summary>
    public partial class PromptUserForNewPlan
    {
        /// <summary>
        /// The plans
        /// </summary>
        private readonly List<SonarActionPlan> plans;

        /// <summary>
        /// The current project
        /// </summary>
        private readonly Resource currentProject;

        /// <summary>
        /// Initializes a new instance of the <see cref="PromptUserForNewPlan" /> class.
        /// </summary>
        /// <param name="existentPlans">The existent plans.</param>
        /// <param name="projects">The projects.</param>
        /// <param name="associatedProject">The associated project.</param>
        public PromptUserForNewPlan(List<SonarActionPlan> existentPlans, List<Resource> projects, Resource associatedProject)
        {
            this.currentProject = associatedProject;
            this.plans = existentPlans;
            this.InitializeComponent();
            this.Projects.ItemsSource = projects;

            if (this.currentProject != null)
            {
                foreach (Resource item in this.Projects.ItemsSource)
                {
                    if (item.Key == this.currentProject.Key)
                    {
                        this.Projects.SelectedItem = item;
                    }
                }
            }
        }

        /// <summary>
        /// The prompt.
        /// </summary>
        /// <param name="existentPlans">The existent plans.</param>
        /// <param name="projects">The projects.</param>
        /// <param name="associatedProject">The associated project.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        public static SonarActionPlan Prompt(List<SonarActionPlan> existentPlans, List<Resource> projects, Resource associatedProject)
        {
            var inst = new PromptUserForNewPlan(existentPlans, projects, associatedProject);
            inst.ShowDialog();

            if ((bool)inst.DialogResult && inst.Projects.SelectedItem != null)
            {
                var plan = new SonarActionPlan();
                plan.Name = inst.nameOfPlan.Text;
                plan.Description = inst.descriptionPlan.Text;
                plan.Project = (inst.Projects.SelectedItem as Resource).Key;
                if (inst.datePicker.SelectedDate.HasValue)
                {
                    plan.DeadLine = inst.datePicker.SelectedDate.Value;
                }

                return plan;
            }

            return null;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var value = comboBox.SelectedItem as Resource;
            if (value == null)
            {
                return;
            }

            this.availableplans.Text = string.Empty;

            foreach (var plan in this.plans)
            {
                if (value.Key == plan.Project)
                {
                    this.availableplans.Text += plan.Name + " ";
                }
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            this.OkButton.IsEnabled = true;

            foreach (var existentPlan in this.plans)
            {
                if (existentPlan.Name.Equals(this.nameOfPlan.Text))
                {
                    this.OkButton.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// The btn cancel_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// The btn ok_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            if (this.Projects.SelectedItem == null)
            {
                this.Status.Content = "Please select a project";
                return;
            }

            if (this.datePicker.SelectedDate == null)
            {
                this.Status.Content = "Please select a deadline";
                return;
            }

            if (string.IsNullOrEmpty(this.nameOfPlan.Text))
            {
                this.Status.Content = "Please fill in plan name";
                return;
            }

            this.DialogResult = true;
            this.Close();
        }
    }
}