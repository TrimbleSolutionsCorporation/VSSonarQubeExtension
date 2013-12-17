// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateCachesTests.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System;
    using System.Collections.Generic;
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
    public class UpdateCachesTests
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
        public void UpdateIssueDataForResourceWithNewDateDataTestWithCache()
        {
            var element = new Resource();
            var newResource = new Resource { Date = DateTime.Now };
            var newResource1 = new Resource { Date = DateTime.Now };

            this.service.Expect(
                mp => mp.GetResourcesData(Arg<ConnectionConfiguration>.Is.Anything, Arg<string>.Is.Equal("resource")))
                .Return(new List<Resource> { element })
                .Repeat.Once();
            this.service.Expect(
                mp => mp.GetResourcesData(Arg<ConnectionConfiguration>.Is.Anything, Arg<string>.Is.Equal("resource")))
                .Return(new List<Resource> { newResource });
            this.service.Expect(
                mp => mp.GetResourcesData(Arg<ConnectionConfiguration>.Is.Anything, Arg<string>.Is.Equal("resource1")))
                .Return(new List<Resource> { newResource1 });

            this.service.Expect(
                mp => mp.GetIssuesInResource(Arg<ConnectionConfiguration>.Is.Anything, Arg<string>.Is.Equal("resource"), Arg<bool>.Is.Anything))
                .Return(new List<Issue> { new Issue() })
                .Repeat.Once();
            this.service.Expect(
                mp => mp.GetIssuesInResource(Arg<ConnectionConfiguration>.Is.Anything, Arg<string>.Is.Equal("resource"), Arg<bool>.Is.Anything))
                .Return(new List<Issue> { new Issue(), new Issue() });
            this.service.Expect(
                mp => mp.GetIssuesInResource(Arg<ConnectionConfiguration>.Is.Anything, Arg<string>.Is.Equal("resource1"), Arg<bool>.Is.Anything))
                .Return(new List<Issue> { new Issue() });

            var data = new ExtensionDataModel(this.service, this.vshelper, null);
            data.RefreshDataForResource("resource");
            Assert.AreEqual(1, data.GetIssuesInEditor("alksjdlakjs").Count);
        }

        /// <summary>
        /// The test loading of window.
        /// </summary>
        [Test]
        public void UpdateCoverageDataForResourceWithNewDateDataTest()
        {
            var element = new Resource();
            var newResource = new Resource { Date = DateTime.Now };
            var source1 = new SourceCoverage();
            source1.SetLineCoverageData("1=0;2=3;2=3");
            source1.SetBranchCoverageData("1=0;2=3;2=3", "1=0;2=3;2=3");
            var source2 = new SourceCoverage();

            this.service.Expect(
                mp => mp.GetResourcesData(Arg<ConnectionConfiguration>.Is.Anything, Arg<string>.Is.Equal("resource")))
                .Return(new List<Resource> { element })
                .Repeat.Once();
            this.service.Expect(
                mp => mp.GetResourcesData(Arg<ConnectionConfiguration>.Is.Anything, Arg<string>.Is.Equal("resource")))
                .Return(new List<Resource> { newResource });

            this.service.Expect(
                mp => mp.GetCoverageInResource(Arg<ConnectionConfiguration>.Is.Anything, Arg<string>.Is.Equal("resource")))
                .Return(source1)
                .Repeat.Once();
            this.service.Expect(
                mp => mp.GetCoverageInResource(Arg<ConnectionConfiguration>.Is.Anything, Arg<string>.Is.Equal("resource")))
                .Return(source2);

            var data = new ExtensionDataModel(this.service, this.vshelper, null);
            data.CoverageInEditorEnabled = true;
            data.RefreshDataForResource("resource");
            Assert.AreEqual(source1, data.GetCoverageInEditor("resource"));
        }
    }
}
