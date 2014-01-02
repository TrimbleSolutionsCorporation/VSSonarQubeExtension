// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataInEditorTests.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

    using VSSonarPlugins;

    /// <summary>
    /// The data in editor tests.
    /// </summary>
    public class DataInEditorTests
    {
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
            /// The plugin controller.
            /// </summary>
            private IPluginController pluginController;

            /// <summary>
            /// The extension.
            /// </summary>
            private IServerAnalyserExtension extension;

            /// <summary>
            /// The plugin.
            /// </summary>
            private IPlugin plugin;

            /// <summary>
            /// The setup.
            /// </summary>
            [SetUp]
            public void Setup()
            {
                this.mocks = new MockRepository();
                this.service = this.mocks.Stub<ISonarRestService>();
                this.vshelper = this.mocks.Stub<IVsEnvironmentHelper>();
                this.extension = this.mocks.Stub<IServerAnalyserExtension>();
                this.pluginController = this.mocks.Stub<IPluginController>();
                this.plugin = this.mocks.Stub<IPlugin>();


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
            public void ErrorWhenOnPluginsAreInstalled()
            {
                var data = new ExtensionDataModel(this.service, this.vshelper, null);
                data.RefreshDataForResource(@"e:\test\src.cs");
                Assert.AreEqual("Extension Not Ready", data.ErrorMessage);
                Assert.IsNull(data.ResourceInEditor);
            }

            /// <summary>
            /// The test loading of window.
            /// </summary>
            [Test]
            public void ShouldReturnWhenContstrainsAreNotMet()
            {
                var data = new ExtensionDataModel(this.service, this.vshelper, null);
                data.UpdateIssuesInEditorLocationWithModifiedBuffer("data");                
                Assert.AreEqual(string.Empty, data.ErrorMessage);
                Assert.AreEqual(0, data.GetIssuesInEditor("file").Count);
                data.UpdateIssuesInEditorLocationWithModifiedBuffer("data");
                Assert.AreEqual(0, data.GetIssuesInEditor("file").Count);
                Assert.IsNull(data.ResourceInEditor);
            }

            /// <summary>
            /// The test loading of window.
            /// </summary>
            [Test]
            public void DoServerAnalysisUpdateBufferFileNotFound()
            {
                var source = new Source { Lines = new List<Line> { new Line { Id = 1, Val = "#include bal" }, new Line { Id = 2, Val = "#include bals" } } };

                this.vshelper.Expect(
                    mp => mp.CurrentSelectedDocumentLanguage())
                    .Return("c++");
                this.vshelper.Expect(
                    mp => mp.ActiveFileFullPath())
                    .Return("c:\\src\\file.cpp");
                this.vshelper.Expect(
                    mp => mp.ActiveSolutionPath())
                    .Return("c:\\src");
                this.service.Expect(
                    mp => mp.GetSourceForFileResource(Arg<ConnectionConfiguration>.Is.Anything, Arg<string>.Is.Anything))
                    .Return(source);
                var element = new Resource { Date = new DateTime(2000, 1, 1), Key = "resourceKey"};
                this.service.Expect(
                    mp => mp.GetResourcesData(Arg<ConnectionConfiguration>.Is.Anything, Arg<string>.Is.Anything))
                    .Return(new List<Resource> { element })
                    .Repeat.Twice();

                this.plugin.Expect(mp => mp.IsSupported(Arg<ConnectionConfiguration>.Is.Anything, Arg<string>.Is.Anything)).Return(true).Repeat.Once();
                this.plugin.Expect(mp => mp.GetServerAnalyserExtension(Arg<ConnectionConfiguration>.Is.Anything, Arg<Resource>.Is.Anything)).Return(this.extension).Repeat.Once();
                this.plugin.Expect(
                    mp =>
                    mp.GetResourceKey(
                        Arg<VsProjectItem>.Is.Anything,
                        Arg<string>.Is.Anything)).Return("key").Repeat.Once();


                var data = new ExtensionDataModel(this.service, this.vshelper, null);
                data.AssociatedProject = new Resource { Key = "KEY"};
                data.RefreshDataForResource("c:\\src\\file.cpp");
                Assert.AreEqual("Extension Not Ready", data.ErrorMessage);
                Assert.AreEqual(0, data.GetIssuesInEditor("file").Count);
            }
        }
    }
}
