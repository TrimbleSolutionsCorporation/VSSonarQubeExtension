// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneralOptionsDialogsTest.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarExtension.Test
{
    using NUnit.Framework;

    using VSSonarExtension.VSControls.DialogOptions;

    /// <summary>
    /// The package test.
    /// </summary>
    [TestFixture]
    public class GeneralOptionsDialogsTest : SonarGeneralOptionsPage
    {
        /// <summary>
        /// The create instance.
        /// </summary>
        [Test]
        public void TestCppOptionsDialogOnApply()
        {
            var page = this.Window as SonarGeneralOptionsControlForm;

            Assert.IsNotNull(page);
            page.SonarHost = "host";
            page.UserName = "name";
            page.UserPassword = "passord";
            
            this.OnApply(null);
            Assert.AreEqual("host", this.SonarHost);
            Assert.AreEqual("name", this.SonarUserName);
            Assert.AreEqual("passord", this.SonarUserPassword);
        }

        /// <summary>
        /// The create instance.
        /// </summary>
        [Test]
        public void TestCppOptionsDialogOnClosed()
        {
            var page = this.Window as SonarGeneralOptionsControlForm;

            Assert.IsNotNull(page);
            page.SonarHost = "host";
            page.UserName = "name";
            page.UserPassword = "passord";

            this.OnClosed(null);
            Assert.AreEqual("host", this.SonarHost);
            Assert.AreEqual("name", this.SonarUserName);
            Assert.AreEqual("passord", this.SonarUserPassword);
        }

        /// <summary>
        /// The test cpp options dialog on deactivate.
        /// </summary>
        [Test]
        public void TestCppOptionsDialogOnDeactivate()
        {
            var page = this.Window as SonarGeneralOptionsControlForm;

            Assert.IsNotNull(page);
            page.SonarHost = "host";
            page.UserName = "name";
            page.UserPassword = "passord";

            this.OnDeactivate(null);
            Assert.AreEqual("host", this.SonarHost);
            Assert.AreEqual("name", this.SonarUserName);
            Assert.AreEqual("passord", this.SonarUserPassword);
        }

        /// <summary>
        /// The test cpp options dialog on deactivate.
        /// </summary>
        [Test]
        public void TestCppOptionsDialogOnActivate()
        {
            var page = this.Window as SonarGeneralOptionsControlForm;

            Assert.IsNotNull(page);
            this.SonarHost = "host";
            this.SonarUserName = "name";
            this.SonarUserPassword = "passord";

            this.OnActivate(null);
            Assert.AreEqual("host", page.SonarHost);
            Assert.AreEqual("name", page.UserName);
            Assert.AreEqual("passord", page.UserPassword);
        }
    }
}