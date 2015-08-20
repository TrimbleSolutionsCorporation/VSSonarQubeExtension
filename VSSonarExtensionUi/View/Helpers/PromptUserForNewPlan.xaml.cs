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
    using VSSonarPlugins.Types;

    /// <summary>
    ///     Interaction logic for PromptUserData.xaml
    /// </summary>
    public partial class PromptUserForNewPlan
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PromptUserForNewPlan"/> class.
        /// </summary>
        /// <param name="existetPlans">The existet plans.</param>
        public PromptUserForNewPlan(List<SonarActionPlan> existetPlans)
        {
            this.InitializeComponent();

            this.availableplans.Text = string.Empty;

            foreach (var plan in existetPlans)
            {
                this.availableplans.Text += plan.Name + " ";
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The prompt.
        /// </summary>
        /// <param name="existetPlans">The existet plans.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        public static SonarActionPlan Prompt(List<SonarActionPlan> existetPlans)
        {
            var inst = new PromptUserForNewPlan(existetPlans);
            inst.ShowDialog();

            if ((bool)inst.DialogResult)
            {
                var plan = new SonarActionPlan();
                plan.Name = inst.nameOfPlan.Text;
                plan.Description = inst.descriptionPlan.Text;
                if (inst.datePicker.SelectedDate.HasValue)
                {
                    plan.DeadLine = inst.datePicker.SelectedDate.Value;
                }
                
                return plan;
            }

            return null;
        }

        #endregion

        #region Methods

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
            this.DialogResult = true;
            this.Close();
        }

        #endregion
    }
}