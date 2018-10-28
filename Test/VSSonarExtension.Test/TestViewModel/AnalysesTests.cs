// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysesTests.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The coverage tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtension.Test.TestViewModel
{
    using System;

    using NUnit.Framework;

    using Rhino.Mocks;

    using SonarRestService;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// The coverage tests.
    /// </summary>
    public class AnalysesTests
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
            /// The vs helper.
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
                    SetupResult.For(this.service.AuthenticateUser(Arg<ISonarConfiguration>.Is.Anything))
                        .Return(true);
                    SetupResult.For(this.vshelper.ReadSavedOption("Sonar Options", "General", "SonarHost"))
                        .Return("serveraddr");
                    SetupResult.For(this.vshelper.ReadSavedOption("Sonar Options", "General", "SonarUserPassword"))
                        .Return("password");
                    SetupResult.For(this.vshelper.ReadSavedOption("Sonar Options", "General", "SonarUserName"))
                        .Return("login");
                }
            }

            /// <summary>
            /// The test on off analyses.
            /// </summary>
            [Test]
            [STAThread]
            public void TestOnOffAnalyses()
            {
                var data = new ExtensionDataModel(this.service, this.vshelper, null, null) { AnalysisMode = true };
                Assert.AreEqual(data.AnalysisModeText, "Server");
                data.AnalysisMode = false;
                Assert.AreEqual(data.AnalysisModeText, "Local");
                data.AnalysisMode = false;
                Assert.AreEqual(data.AnalysisModeText, "Local");
                data.AnalysisMode = true;
                Assert.AreEqual(data.AnalysisModeText, "Server");
            }

            /// <summary>
            /// The test loading of window.
            /// </summary>
            [Test]
            [STAThread]
            public void TypeShouldBeFileWhenModeIsServer()
            {
                var data = new ExtensionDataModel(this.service, this.vshelper, null, null)
                               {
                                   AnalysisMode = true, 
                                   AnalysisType = true
                               };
                Assert.AreEqual(data.AnalysisTypeText, "File");
                data.AnalysisType = false;
                Assert.AreEqual(data.AnalysisTypeText, "File");
            }

            /// <summary>
            /// The frequency and trigger on demand execute when type is not file.
            /// </summary>
            [Test]
            [STAThread]
            public void FrequencyAndTriggerOnDemandExecuteWhenTypeIsNotFile()
            {
                var data = new ExtensionDataModel(this.service, this.vshelper, null, null)
                               {
                                   AnalysisMode = false, 
                                   AnalysisType = true
                               };
                Assert.AreEqual(data.AnalysisModeText, "Local");
                Assert.AreEqual(data.AnalysisTypeText, "Analysis");
                data.AnalysisType = false;
                Assert.AreEqual(data.AnalysisTypeText, "Preview");
                data.AnalysisType = false;
                Assert.AreEqual(data.AnalysisTypeText, "Incremental");
                data.AnalysisType = false;
                Assert.AreEqual(data.AnalysisTypeText, "File");
            }

            /// <summary>
            /// The test loading of window.
            /// </summary>
            [Test]
            [STAThread]
            public void ShouldActivateAllTypesWhenModelIsLocal()
            {
                var data = new ExtensionDataModel(this.service, this.vshelper, null, null)
                               {
                                   AnalysisMode = false, 
                                   AnalysisType = true
                               };

                Assert.AreEqual(data.AnalysisTypeText, "Analysis");
                data.AnalysisType = true;
                Assert.AreEqual(data.AnalysisTypeText, "Preview");
                data.AnalysisType = true;
                Assert.AreEqual(data.AnalysisTypeText, "Incremental");
                data.AnalysisType = true;
                Assert.AreEqual(data.AnalysisTypeText, "File");
                data.AnalysisType = true;
                Assert.AreEqual(data.AnalysisTypeText, "Analysis");
                data.AnalysisType = false;
                Assert.AreEqual(data.AnalysisTypeText, "Preview");
                data.AnalysisType = false;
                Assert.AreEqual(data.AnalysisTypeText, "Incremental");
                data.AnalysisType = false;
                Assert.AreEqual(data.AnalysisTypeText, "File");
                data.AnalysisType = false;
                Assert.AreEqual(data.AnalysisTypeText, "Analysis");
            }
        }
    }
}
