// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CoverageTests.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The coverage tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtension.Test.TestViewModel
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    using Rhino.Mocks;

    using SonarRestService;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// The coverage tests.
    /// </summary>
    public class CoverageTests
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
                SetupResult.For(this.service.GetServerInfo(Arg<ISonarConfiguration>.Is.Anything)).Return(3.6);
                SetupResult.For(this.service.AuthenticateUser(Arg<ISonarConfiguration>.Is.Anything)).Return(true);
                SetupResult.For(this.vshelper.ReadSavedOption("Sonar Options", "General", "SonarHost")).Return("serveraddr");
                SetupResult.For(this.vshelper.ReadSavedOption("Sonar Options", "General", "SonarUserPassword")).Return("password");
                SetupResult.For(this.vshelper.ReadSavedOption("Sonar Options", "General", "SonarUserName")).Return("login");
            }
        }

            /// <summary>
            /// The test loading of window.
            /// </summary>
            [Test]
            [STAThread]
            public void CoverageInEditorSetAndGetTest()
            {
                var data = new ExtensionDataModel(this.service, this.vshelper, null, null);
                var sourceCoverage = new Dictionary<int, CoverageElement>();
                Assert.AreEqual(sourceCoverage, data.GetCoverageInEditor(string.Empty));
            }

            /// <summary>
            /// The test loading of window.
            /// </summary>
            [Test]
            [STAThread]
            public void DisableCoverageInEditor()
            {
                var data = new ExtensionDataModel(this.service, this.vshelper, null, null);
                var sourceCoverage = new SourceCoverage();
                data.CoverageInEditorEnabled = false;
                Assert.IsFalse(data.CoverageInEditorEnabled);
                Assert.AreNotEqual(sourceCoverage, data.GetCoverageInEditor(string.Empty));
            }
        }
    }
}
