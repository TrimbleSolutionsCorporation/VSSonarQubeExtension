// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarGeneralOptionsControlForm.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.VSControls.DialogOptions
{
    using System;
    using System.Windows.Forms;

    using ExtensionTypes;

    using ExtensionView;

    using ExtensionViewModel.ViewModel;

    using SonarRestService;

    /// <summary>
    /// The sonar general options control form.
    /// </summary>
    public partial class SonarGeneralOptionsControlForm : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SonarGeneralOptionsControlForm"/> class.
        /// </summary>
        public SonarGeneralOptionsControlForm()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the SonarHost.
        /// </summary>
        public string SonarHost
        {
            get
            {
                return this.textBoxSonarHost.Text;
            }

            set
            {
                this.textBoxSonarHost.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName
        {
            get
            {
                return this.textBoxUserName.Text;
            }

            set
            {
                this.textBoxUserName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the user password.
        /// </summary>
        public string UserPassword
        {
            get
            {
                return this.textBoxPassword.Text;
            }

            set
            {
                this.textBoxPassword.Text = value;
            }
        }

        /// <summary>
        /// The button test connection click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ButtonTestConnectionClick(object sender, EventArgs e)
        {
            try
            {
                var userConf = new ConnectionConfiguration(this.textBoxSonarHost.Text, this.textBoxUserName.Text, this.textBoxPassword.Text);
                ISonarRestService restService = new SonarRestService(new JsonSonarConnector());
                var versionDouble = restService.GetServerInfo(userConf);

                if (versionDouble < 3.3)
                {
                    MessageBox.Show("Authentication Ok");
                    return;
                }

                MessageBox.Show(restService.AuthenticateUser(userConf)
                                                         ? "Authentication Ok"
                                                         : "Authentication Failed, Check Host/Password/User");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot Authenticate" + ex.StackTrace + " Messsage: " + ex.Message);
            }
        }

        /// <summary>
        /// The plugins user control_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void PluginsUserControlClick(object sender, EventArgs e)
        {
            var window = new PluginOptionsWindow(ExtensionDataModel.PluginsOptionsData);
            window.ShowDialog();
        }
    }
}
