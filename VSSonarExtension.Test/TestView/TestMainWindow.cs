// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestMainWindow.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.Test.TestView
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading;
    using System.Windows;

    using ExtensionHelpers;

    using ExtensionTypes;

    using NUnit.Framework;

    using Rhino.Mocks;

    using SonarRestService;

    using VSSonarExtension.MainView;
    using VSSonarExtension.MainViewModel.ViewModel;

    using VSSonarPlugins;

    /// <summary>
    /// The comment on issue command test.
    /// </summary>
    [TestFixture]
    public class TestMainWindow
    {
        /// <summary>
        /// The model.
        /// </summary>
        private ExtensionDataModel model;

        /// <summary>
        /// The should not throw any exceptions when creating issues window.
        /// </summary>
        [Test]
        public void ShouldNotThrowAnyExceptionsWhenCreatingIssuesWindow()
        {
            var t = new Thread(this.CreateMainIssueWindow);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        /// <summary>
        /// The test window.
        /// </summary>
        //[Test]
        public void TestWindow()
        {
            var mocks = new MockRepository();
            var mockHttpReq = mocks.Stub<IHttpSonarConnector>();
            var mockVsHelpers = mocks.Stub<IVsEnvironmentHelper>();
            var config = new ConnectionConfiguration("serveraddr", "login", "password");

            // set expectations
            using (mocks.Record())
            {
                SetupResult.For(mockHttpReq.HttpSonarGetRequest(config, "/api/issues/search?components=resource")).Return(File.ReadAllText("TestData/issuessearchbycomponent.txt"));
                SetupResult.For(mockHttpReq.HttpSonarGetRequest(config, "/api/users/search")).Return(File.ReadAllText("TestData/userList.txt"));
            }

            ISonarRestService service = new SonarRestService(mockHttpReq);
            var issues = service.GetIssuesInResource(config, "resource");
            var associatedProject = new Resource { Key = "core:Common" };

            this.model = new ExtensionDataModel(service, mockVsHelpers, associatedProject) {};
            var t = new Thread(new ThreadStart(this.Threadprc));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();    
        }

        private class DummyLocalAnalyserExtension : IPlugin
        {
            private IPluginsOptions pluginOptions = new VSSonarExtension.Test.TestView.DummyOptionsController();

            public IPluginsOptions GetUsePluginControlOptions()
            {
                return this.pluginOptions;
            }

            public string GetKey()
            {
                return "dummy plugin";
            }

            public string GetKey(ConnectionConfiguration configuration)
            {
                throw new NotImplementedException();
            }

            public IPluginsOptions GetPluginControlOptions(ConnectionConfiguration configuration, Resource project)
            {
                throw new NotImplementedException();
            }

            public IPluginsOptions GetPluginControlOptions(ConnectionConfiguration configuration)
            {
                throw new NotImplementedException();
            }

            public bool IsSupported(ConnectionConfiguration configuration, string resource)
            {
                throw new NotImplementedException();
            }

            public bool IsSupported(ConnectionConfiguration configuration, Resource resource)
            {
                throw new NotImplementedException();
            }

            public string GetResourceKey(VsProjectItem projectItem, string projectKey)
            {
                throw new NotImplementedException();
            }

            public ILocalAnalyserExtension GetLocalAnalysisExtension(ConnectionConfiguration configuration, Resource project, double sonarVersion)
            {
                throw new NotImplementedException();
            }

            public ILocalAnalyserExtension GetLocalAnalysisExtension(ConnectionConfiguration configuration, Resource project)
            {
                throw new NotImplementedException();
            }

            public IServerAnalyserExtension GetServerAnalyserExtension(ConnectionConfiguration configuration, Resource project)
            {
                throw new NotImplementedException();
            }

            public Dictionary<string, VsLicense> GetLicenses(ConnectionConfiguration configuration)
            {
                throw new NotImplementedException();
            }

            public string GenerateTokenId(ConnectionConfiguration configuration)
            {
                throw new NotImplementedException();
            }
        }

        //[Test]
        public void TestPluginsOptionWindow()
        {
            var t = new Thread(new ThreadStart(this.WindowPlugins));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        /// <summary>
        /// The threadprc.
        /// </summary>
        private void WindowPlugins()
        {
            var modelPl = new PluginsOptionsModel();
            var plugins = new List<IPlugin> { new DummyLocalAnalyserExtension() };

            var windowPlugins = new PluginOptionsWindow(modelPl);
            modelPl.Plugins = new ReadOnlyCollection<IPlugin>(plugins);
            windowPlugins.ShowDialog();
        }

        /// <summary>
        /// The threadprc.
        /// </summary>
        private void CreateMainIssueWindow()
        {
            var win = new Window();
            var view = new IssueWindow(this.model);
            win.Content = view;
            view.UpdateDataContext(this.model);
        }

        /// <summary>
        /// The threadprc.
        /// </summary>
        private void Threadprc()
        {
            var win = new Window();
            var view = new IssueWindow(this.model);
            win.Content = view;
            win.ShowDialog();
        }
    }
}
