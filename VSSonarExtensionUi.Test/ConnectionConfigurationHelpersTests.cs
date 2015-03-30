// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionConfigurationHelpersTests.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarExtensionUi.Helpers.Test.NonVSTests
{
    using System;

    

    using NUnit.Framework;

    using Moq;

    using SonarRestService;

    using VSSonarPlugins;

    /// <summary>
    /// The comment on issue command test.
    /// </summary>
    [TestFixture]
    public class ISonarConfigurationHelpersTests
    {
        /// <summary>
        /// The test window.
        /// </summary>
        [Test]
        public void GetUserConfigurationIsOkAfterAuthentication()
        {
            var mocks = new MockRepository();
            var mockHttpReq = mocks.Stub<ISonarRestService>();
            var mockPropertiesHelper = mocks.Stub<IVsEnvironmentHelper>();

            using (mocks.Record())
            {
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarUserName")).Return("userlogin");
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarUserPassword")).Return("userpassword");
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarHost")).Return("host");
                SetupResult.For(mockHttpReq.AuthenticateUser(Arg<ISonarConfiguration>.Matches(y => y.Username.Equals("userlogin")))).Return(true);
            }

            var authconf = ConnectionConfigurationHelpers.GetConnectionConfiguration(mockPropertiesHelper, mockHttpReq);
            Assert.AreEqual("userlogin", authconf.Username);
            Assert.AreEqual("userpassword", authconf.Password);
            Assert.AreEqual("host", authconf.Hostname);
        }

        /// <summary>
        /// The test window.
        /// </summary>
        [Test]
        public void GetUserConfigurationIsNullIfInvalidUserName()
        {
            var mocks = new MockRepository();
            var mockHttpReq = mocks.Stub<ISonarRestService>();
            var mockPropertiesHelper = mocks.Stub<IVsEnvironmentHelper>();

            using (mocks.Record())
            {
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarUserName")).Return(string.Empty);
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarUserPassword")).Return("userpassword");
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarHost")).Return("host");
                SetupResult.For(mockHttpReq.AuthenticateUser(Arg<ISonarConfiguration>.Matches(y => y.Username.Equals("userlogin")))).Return(true);
            }

            var authconf = ConnectionConfigurationHelpers.GetConnectionConfiguration(mockPropertiesHelper, mockHttpReq);
            Assert.AreEqual("Authentication Failed, Check User, Password and Hostname", ConnectionConfigurationHelpers.ErrorMessage);
            Assert.IsNull(authconf);
        }

        /// <summary>
        /// The test window.
        /// </summary>
        [Test]
        public void GetUserConfigurationIsNullIfInvalidPassword()
        {
            var mocks = new MockRepository();
            var mockHttpReq = mocks.Stub<ISonarRestService>();
            var mockPropertiesHelper = mocks.Stub<IVsEnvironmentHelper>();

            using (mocks.Record())
            {
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarUserName")).Return("asdasdas");
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarUserPassword")).Return(string.Empty);
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarHost")).Return("host");
                SetupResult.For(mockHttpReq.AuthenticateUser(Arg<ISonarConfiguration>.Matches(y => y.Username.Equals("userlogin")))).Return(true);
            }

            var authconf = ConnectionConfigurationHelpers.GetConnectionConfiguration(mockPropertiesHelper, mockHttpReq);
            Assert.AreEqual("Authentication Failed, Check User, Password and Hostname", ConnectionConfigurationHelpers.ErrorMessage);
            Assert.IsNull(authconf);
        }

        /// <summary>
        /// The test window.
        /// </summary>
        [Test]
        public void GetUserConfigurationIsNullIfInvalidHost()
        {
            var mocks = new MockRepository();
            var mockHttpReq = mocks.Stub<ISonarRestService>();
            var mockPropertiesHelper = mocks.Stub<IVsEnvironmentHelper>();

            using (mocks.Record())
            {
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarUserName")).Return("asdasdas");
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarUserPassword")).Return("sadasdsa");
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarHost")).Return(string.Empty);
                SetupResult.For(mockHttpReq.AuthenticateUser(Arg<ISonarConfiguration>.Matches(y => y.Username.Equals("userlogin")))).Return(true);
            }

            var authconf = ConnectionConfigurationHelpers.GetConnectionConfiguration(mockPropertiesHelper, mockHttpReq);
            Assert.AreEqual("User Configuration is Invalid, Check Tools > Options > Sonar Options", ConnectionConfigurationHelpers.ErrorMessage);
            Assert.IsNull(authconf);
        }

        /// <summary>
        /// The test window.
        /// </summary>
        [Test]
        public void GetUserConfigurationIsNullAfterFailAuthentication()
        {
            var mocks = new MockRepository();
            var mockHttpReq = mocks.Stub<ISonarRestService>();
            var mockPropertiesHelper = mocks.Stub<IVsEnvironmentHelper>();

            using (mocks.Record())
            {
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarUserName")).Return("userlogin");
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarUserPassword")).Return("userpassword");
                SetupResult.For(mockPropertiesHelper.ReadSavedOption("Sonar Options", "General", "SonarHost")).Return("host");
                SetupResult.For(mockHttpReq.AuthenticateUser(Arg<ISonarConfiguration>.Matches(y => y.Username.Equals("userlogin")))).Return(false);
            }

            var authconf = ConnectionConfigurationHelpers.GetConnectionConfiguration(mockPropertiesHelper, mockHttpReq);
            Assert.AreEqual("Authentication Failed, Check User, Password and Hostname", ConnectionConfigurationHelpers.ErrorMessage);
            Assert.IsNull(authconf);
        }
    }
}
