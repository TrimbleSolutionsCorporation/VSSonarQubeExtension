// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewWorkFlowTests.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace ExtensionViewModel.Test
{
    using System.Collections.Generic;
    using System.Windows;

    using ExtensionHelpers;

    using ExtensionTypes;

    using ExtensionViewModel.ViewModel;

    using NUnit.Framework;

    using Rhino.Mocks;

    using SonarRestService;

    /// <summary>
    /// The comment on issue command test.
    /// </summary>
    [TestFixture]
    public class NewWorkFlowTests
    {
        /// <summary>
        /// The mocks.
        /// </summary>
        private MockRepository mocks;

        /// <summary>
        /// The service.
        /// </summary>
        private ISonarRestService service;

        /// <summary>
        /// The vshelper.
        /// </summary>
        private IVsEnvironmentHelper vshelper;

        /// <summary>
        /// The setup.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.mocks = new MockRepository();
            this.service = this.mocks.Stub<ISonarRestService>();
            this.vshelper = this.mocks.Stub<IVsEnvironmentHelper>();

            using (this.mocks.Record())
            {
                SetupResult.For(this.service.GetServerInfo(Arg<ConnectionConfiguration>.Is.Anything)).Return(3.6);
                SetupResult.For(this.service.AuthenticateUser(Arg<ConnectionConfiguration>.Is.Anything)).Return(true);
                SetupResult.For(this.vshelper.ReadSavedOption("Sonar Options", "General", "SonarHost")).Return("serveraddr");
                SetupResult.For(this.vshelper.ReadSavedOption("Sonar Options", "General", "SonarUserPassword")).Return("password");
                SetupResult.For(this.vshelper.ReadSavedOption("Sonar Options", "General", "SonarUserName")).Return("login");
            }
        }

        /// <summary>
        /// The test loading of window.
        /// </summary>
        [Test]
        public void VerifyOpenAndReopenStatusTest()
        {
            var data = new ExtensionDataModel(this.service, this.vshelper, null)
                           {
                               SelectedIssuesInView =
                                   new List<Issue>
                                       {
                                           new Issue { Status = "OPEN" },
                                           new Issue { Status = "REOPENED" }
                                       }
                           };

            Assert.AreEqual(Visibility.Hidden, data.IsReopenVisible);
            Assert.AreEqual(Visibility.Visible, data.IsResolveVisible);
            Assert.AreEqual(Visibility.Hidden, data.IsFalsePositiveVisible);
            Assert.AreEqual(Visibility.Visible, data.IsCommentingVisible);
            Assert.AreEqual(Visibility.Visible, data.IsConfirmVisible);
            Assert.AreEqual(Visibility.Hidden, data.IsUnconfirmVisible);
            Assert.AreEqual(Visibility.Visible, data.IsAssignVisible);
            Assert.AreEqual(Visibility.Visible, data.IsOpenExternallyVisible);
        }

        /// <summary>
        /// The verify confirm status.
        /// </summary>
        [Test]
        public void VerifyConfirmStatus()
        {
            var data = new ExtensionDataModel(this.service, this.vshelper, null)
            {
                SelectedIssuesInView =
                    new List<Issue>
                                       {
                                           new Issue { Status = "CONFIRMED" }
                                       }
            };

            Assert.AreEqual(Visibility.Hidden, data.IsReopenVisible);
            Assert.AreEqual(Visibility.Visible, data.IsResolveVisible);
            Assert.AreEqual(Visibility.Visible, data.IsFalsePositiveVisible);
            Assert.AreEqual(Visibility.Visible, data.IsCommentingVisible);
            Assert.AreEqual(Visibility.Hidden, data.IsConfirmVisible);
            Assert.AreEqual(Visibility.Visible, data.IsUnconfirmVisible);
            Assert.AreEqual(Visibility.Visible, data.IsAssignVisible);
            Assert.AreEqual(Visibility.Visible, data.IsOpenExternallyVisible);
        }

        /// <summary>
        /// The verify resolve status.
        /// </summary>
        [Test]
        public void VerifyResolveStatus()
        {
            var data = new ExtensionDataModel(this.service, this.vshelper, null)
            {
                SelectedIssuesInView =
                    new List<Issue>
                                       {
                                           new Issue { Status = "RESOLVED" }
                                       }
            };

            Assert.AreEqual(Visibility.Visible, data.IsReopenVisible);
            Assert.AreEqual(Visibility.Hidden, data.IsResolveVisible);
            Assert.AreEqual(Visibility.Hidden, data.IsFalsePositiveVisible);
            Assert.AreEqual(Visibility.Visible, data.IsCommentingVisible);
            Assert.AreEqual(Visibility.Hidden, data.IsConfirmVisible);
            Assert.AreEqual(Visibility.Hidden, data.IsUnconfirmVisible);
            Assert.AreEqual(Visibility.Hidden, data.IsAssignVisible);
            Assert.AreEqual(Visibility.Visible, data.IsOpenExternallyVisible);
        }

        /// <summary>
        /// The verify reopen status.
        /// </summary>
        [Test]
        public void VerifyMultiSelectionStatus()
        {
            var data = new ExtensionDataModel(this.service, this.vshelper, null)
            {
                SelectedIssuesInView =
                    new List<Issue>
                                       {
                                           new Issue { Status = "REOPENED" },
                                           new Issue { Status = "CONFIRMED" },
                                           new Issue { Status = "RESOLVED" }
                                       }
            };

            Assert.AreEqual(Visibility.Visible, data.IsReopenVisible);
            Assert.AreEqual(Visibility.Visible, data.IsResolveVisible);
            Assert.AreEqual(Visibility.Visible, data.IsFalsePositiveVisible);
            Assert.AreEqual(Visibility.Visible, data.IsCommentingVisible);
            Assert.AreEqual(Visibility.Visible, data.IsConfirmVisible);
            Assert.AreEqual(Visibility.Visible, data.IsUnconfirmVisible);
            Assert.AreEqual(Visibility.Visible, data.IsAssignVisible);
            Assert.AreEqual(Visibility.Visible, data.IsOpenExternallyVisible);
        }
    }
}
