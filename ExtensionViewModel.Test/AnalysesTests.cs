// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysesTests.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using ExtensionHelpers;

    using ExtensionTypes;

    using ExtensionViewModel.ViewModel;

    using NUnit.Framework;

    using Rhino.Mocks;

    using SonarRestService;

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
                    SetupResult.For(this.service.GetServerInfo(Arg<ConnectionConfiguration>.Is.Anything)).Return(3.6);
                    SetupResult.For(this.service.AuthenticateUser(Arg<ConnectionConfiguration>.Is.Anything))
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
            public void TestOnOffAnalyses()
            {
                var data = new ExtensionDataModel(this.service, this.vshelper, null) { AnalysisMode = true };
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
            public void TypeShouldBeFileWhenModeIsServer()
            {
                var data = new ExtensionDataModel(this.service, this.vshelper, null)
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
            public void FrequencyAndTriggerOnDemandExecuteWhenTypeIsNotFile()
            {
                var data = new ExtensionDataModel(this.service, this.vshelper, null)
                               {
                                   AnalysisMode = false,
                                   AnalysisType = true
                               };
                Assert.AreEqual(data.AnalysisModeText, "Local");
                Assert.AreEqual(data.AnalysisTypeText, "Analysis");
                Assert.AreEqual(data.AnalysisFrequencyText, "OnDemand");
                Assert.AreEqual(data.AnalysisTriggerText, "Execute");
                data.AnalysisType = false;
                Assert.AreEqual(data.AnalysisTypeText, "Preview");
                Assert.AreEqual(data.AnalysisFrequencyText, "OnDemand");
                Assert.AreEqual(data.AnalysisTriggerText, "Execute");
                data.AnalysisType = false;
                Assert.AreEqual(data.AnalysisTypeText, "Incremental");
                Assert.AreEqual(data.AnalysisFrequencyText, "OnDemand");
                Assert.AreEqual(data.AnalysisTriggerText, "Execute");
                data.AnalysisType = false;
                Assert.AreEqual(data.AnalysisTypeText, "File");
                Assert.AreEqual(data.AnalysisFrequencyText, "OnDemand");
                Assert.AreEqual(data.AnalysisTriggerText, "Execute");
            }

            /// <summary>
            /// The trigger should toggle on and off demand execute when type is file.
            /// </summary>
            [Test]
            public void TriggerShouldToggleOnAndOffDemandExecuteWhenTypeIsFile()
            {
                var data = new ExtensionDataModel(this.service, this.vshelper, null) { AnalysisMode = false };
                Assert.AreEqual(data.AnalysisTypeText, "File");
                Assert.AreEqual(data.AnalysisModeText, "Local");
                Assert.AreEqual(data.AnalysisFrequencyText, "OnDemand");
                Assert.AreEqual(data.AnalysisTriggerText, "Execute");
                data.AnalysisFrequency = false;
                Assert.AreEqual(data.AnalysisFrequencyText, "AlwaysOn");
                Assert.AreEqual(data.AnalysisTriggerText, "Turn On");
                data.AnalysisTrigger = false;
                Assert.AreEqual(data.AnalysisFrequencyText, "AlwaysOn");
                Assert.AreEqual(data.AnalysisTriggerText, "Turn On");
                data.AnalysisTrigger = true;
                Assert.AreEqual(data.AnalysisFrequencyText, "AlwaysOn");
                Assert.AreEqual(data.AnalysisTriggerText, "Turn Off");
            }

            /// <summary>
            /// The test loading of window.
            /// </summary>
            [Test]
            public void ShouldActivateAllTypesWhenModelIsLocal()
            {
                var data = new ExtensionDataModel(this.service, this.vshelper, null)
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
