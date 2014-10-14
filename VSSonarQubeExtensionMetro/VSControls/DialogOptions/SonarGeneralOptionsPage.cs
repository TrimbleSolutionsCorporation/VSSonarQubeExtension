// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarGeneralOptionsPage.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarQubeExtension.VSControls.DialogOptions
{
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using Microsoft.VisualStudio.Shell;

    using VSSonarQubeExtension;

    /// <summary>
    /// The a style general options page.
    /// </summary>
    [ComVisible(true)]
    public class SonarGeneralOptionsPage : DialogPage
    {
        /// <summary>
        /// The control.
        /// </summary>
        private SonarGeneralOptionsControlForm control;

        /// <summary>
        /// The sonar host.
        /// </summary>
        private string sonarHost = string.Empty;

        /// <summary>
        /// The sonar user name.
        /// </summary>
        private string sonarUserName = string.Empty;

        /// <summary>
        /// The sonar user password.
        /// </summary>
        private string sonarUserPassword = string.Empty;

        /// <summary>
        /// Gets or sets the SonarHost.
        /// </summary>
        public string SonarHost
        {
            get
            {
                return this.sonarHost;
            }

            set
            {
                this.sonarHost = value;
            }
        }

        /// <summary>
        /// Gets or sets the SonarUserName.
        /// </summary>
        public string SonarUserName
        {
            get
            {
                return this.sonarUserName;
            }

            set
            {
                this.sonarUserName = value;
            }
        }

        /// <summary>
        /// Gets or sets the SonarUserPassword.
        /// </summary>
        public string SonarUserPassword
        {
            get
            {
                return this.sonarUserPassword;
            }

            set
            {
                this.sonarUserPassword = value;
            }
        }

        /// <summary>
        /// Gets the window.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get
            {
                this.control = new SonarGeneralOptionsControlForm();

                if (this.control != null)
                {
                    this.UpdatePropertiesInDialog();
                }

                return this.control;
            }
        }

        /// <summary>
        /// The on deactivate.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnDeactivate(CancelEventArgs e)
        {
            if (this.control != null)
            {
                this.UpdateProperties();
            }

            base.OnDeactivate(e);
        }

        /// <summary>
        /// The on activate.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnActivate(CancelEventArgs e)
        {
            if (this.control != null)
            {
                this.UpdatePropertiesInDialog();
            }

            base.OnActivate(e);
        }

        /// <summary>
        /// The on apply.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnApply(PageApplyEventArgs e)
        {
            if (this.control != null)
            {
                this.UpdateProperties();
                VsSonarExtensionPackage.ExtensionModelData.StartupModelData();
            }

            base.OnApply(e);
        }

        /// <summary>
        /// The update properties in dialog.
        /// </summary>
        private void UpdatePropertiesInDialog()
        {
            this.control.SonarHost = this.SonarHost;
            this.control.UserName = this.SonarUserName;
            this.control.UserPassword = this.SonarUserPassword;
        }

        /// <summary>
        /// The update properties.
        /// </summary>
        private void UpdateProperties()
        {
            this.SonarHost = this.control.SonarHost;
            this.SonarUserName = this.control.UserName;
            this.SonarUserPassword = this.control.UserPassword;
        }
    }
}
