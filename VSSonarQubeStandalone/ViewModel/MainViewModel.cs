// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
using GalaSoft.MvvmLight;

namespace VSSonarQubeStandalone.ViewModel
{
    using Helpers;
    using SonarRestService;
    using System;
    using System.Windows.Forms;
    using VSSonarExtensionUi.Model.Helpers;
    using VSSonarExtensionUi.View.Helpers;
    using VSSonarExtensionUi.ViewModel;
    using VSSonarPlugins.Types;
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            var address = PromptUserData.Prompt("Server Address", "Insert Server Address", "http://sonar");
            if (address == null)
            {
                throw new Exception("Server Address must not be empty");
            }

            using (var dialog = new Security.Windows.Forms.UserCredentialsDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dialog.SaveChecked)
                    {
                        dialog.ConfirmCredentials(true);
                    }

                    AuthtenticationHelper.EstablishAConnection(new SonarRestService(new JsonSonarConnector()), address, dialog.User, dialog.PasswordToString());
                }
                else
                {
                    throw new Exception("user name and pass should be set");
                }
            }

            
            this.SonarQubeView = new SonarQubeViewModel("standalone");
            this.SonarQubeView.InitModelFromPackageInitialization(new VsEnvironmentHelper(), new VsStatusBarDummy(), new ServiceProviderDummy(), Environment.CurrentDirectory);
        }

        /// <summary>
        /// Gets or sets the sonar qube view.
        /// </summary>
        public SonarQubeViewModel SonarQubeView { get; set; }
    }
}