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
    using System.IO;

    /// <summary>
    /// Interaction logic for PromptUserData.xaml
    /// </summary>
    public partial class ProvisionProject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PromptUserForNewPlan"/> class.
        /// </summary>
        /// <param name="existetPlans">The existet plans.</param>
        public ProvisionProject(string solutionName, string branch)
        {
            this.InitializeComponent();

            this.key.Text = "your.organization." + Path.GetFileNameWithoutExtension(solutionName);
            this.name.Text = Path.GetFileNameWithoutExtension(solutionName);
            this.hintbranch.Content = branch;
        }

        /// <summary>
        /// The prompt.
        /// </summary>
        /// <param name="existetPlans">The existet plans.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        public static ProvisionProject Prompt(string solutionName, string branch)
        {
            var inst = new ProvisionProject(solutionName, branch);
            inst.ShowDialog();

            if ((bool)inst.DialogResult)
            {
                return inst;
            }

            return null;
        }


        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <returns></returns>
        public string GetKey()
        {
            return this.key.Text;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return this.name.Text;
        }

        /// <summary>
        /// Gets the branch.
        /// </summary>
        /// <returns></returns>
        public string GetBranch()
        {
            return this.branchtouse.Text;
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
            this.DialogResult = true;
            this.Close();
        }
    }
}