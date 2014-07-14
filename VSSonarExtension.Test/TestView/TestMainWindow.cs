// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestMainWindow.cs" company="">
//   
// </copyright>
// <summary>
//   The comment on issue command test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtension.Test.TestView
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading;
    using System.Windows;

    using ExtensionTypes;

    using NUnit.Framework;

    using Rhino.Mocks;

    using SonarRestService;

    using VSSonarExtension.MainView;
    using VSSonarExtension.MainViewModel.ViewModel;

    using VSSonarPlugins;

    /// <summary>
    ///     The comment on issue command test.
    /// </summary>
    [TestFixture]
    public class TestMainWindow
    {
        #region Fields

        /// <summary>
        ///     The model.
        /// </summary>
        private ExtensionDataModel model;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The should not throw any exceptions when creating issues window.
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
        ///     The test plugins option window.
        /// </summary>
        public void TestPluginsOptionWindow()
        {
            var t = new Thread(this.Threadprc);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        /// <summary>
        ///     The test window.
        /// </summary>
        public void TestWindow()
        {
            var mocks = new MockRepository();
            var mockHttpReq = mocks.Stub<IHttpSonarConnector>();
            var mockVsHelpers = mocks.Stub<IVsEnvironmentHelper>();
            var config = new ConnectionConfiguration("serveraddr", "login", "password");

            // set expectations
            using (mocks.Record())
            {
                SetupResult.For(mockHttpReq.HttpSonarGetRequest(config, "/api/issues/search?components=resource"))
                    .Return(File.ReadAllText("TestData/issuessearchbycomponent.txt"));
                SetupResult.For(mockHttpReq.HttpSonarGetRequest(config, "/api/users/search")).Return(File.ReadAllText("TestData/userList.txt"));
            }

            ISonarRestService service = new SonarRestService(mockHttpReq);
            var issues = service.GetIssuesInResource(config, "resource");
            var associatedProject = new Resource { Key = "core:Common" };

            this.model = new ExtensionDataModel(service, mockVsHelpers, associatedProject, null);
            var t = new Thread(this.Threadprc);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The threadprc.
        /// </summary>
        private void CreateMainIssueWindow()
        {
            var win = new Window();
            var view = new IssueWindow(this.model);
            win.Content = view;
            view.UpdateDataContext(this.model);
        }

        /// <summary>
        ///     The threadprc.
        /// </summary>
        private void Threadprc()
        {
            var win = new Window();
            var view = new IssueWindow(this.model);
            win.Content = view;
            win.ShowDialog();
        }

        /// <summary>
        ///     The threadprc.
        /// </summary>
        private void WindowPlugins()
        {            
            var plugins = new List<IAnalysisPlugin> { new DummyLocalAnalyserExtension() };
            var modelPl = new ExtensionOptionsModel(new PluginController(), null, null, null);
            var windowPlugins = new ExtensionOptionsWindow(modelPl);
            windowPlugins.ShowDialog();
        }

        #endregion

        /// <summary>
        ///     The dummy local analyser extension.
        /// </summary>
        private class DummyLocalAnalyserExtension : IAnalysisPlugin
        {
            #region Public Methods and Operators

            /// <summary>
            /// The generate token id.
            /// </summary>
            /// <param name="configuration">
            /// The configuration.
            /// </param>
            /// <returns>
            /// The <see cref="string"/>.
            /// </returns>
            public string GenerateTokenId(ConnectionConfiguration configuration)
            {
                return string.Empty;
            }

            public PluginDescription GetPluginDescription(IVsEnvironmentHelper vsinter)
            {
                throw new System.NotImplementedException();
            }

            public PluginDescription GetPluginDescription()
            {
                throw new System.NotImplementedException();
            }

            public string GetAnalyisOption(string key)
            {
                throw new System.NotImplementedException();
            }

            /// <summary>
            /// The get key.
            /// </summary>
            /// <param name="configuration">
            /// The configuration.
            /// </param>
            /// <returns>
            /// The <see cref="string"/>.
            /// </returns>
            public string GetKey(ConnectionConfiguration configuration)
            {
                return string.Empty;
            }

            public string GetLanguageKey()
            {
                throw new System.NotImplementedException();
            }

            public IPluginsOptions GetPluginControlOptions(ConnectionConfiguration configuration)
            {
                throw new System.NotImplementedException();
            }

            public ILocalAnalyserExtension GetLocalAnalysisExtension()
            {
                throw new System.NotImplementedException();
            }

            public ILocalAnalyserExtension GetLocalAnalysisExtension(Resource project)
            {
                throw new System.NotImplementedException();
            }

            public string GetResourceKey(VsProjectItem projectItem, string projectKey, bool safeGeneration)
            {
                throw new System.NotImplementedException();
            }

            public ILocalAnalyserExtension GetLocalAnalysisExtension(ConnectionConfiguration configuration)
            {
                throw new System.NotImplementedException();
            }

            /// <summary>
            /// The get licenses.
            /// </summary>
            /// <param name="configuration">
            /// The configuration.
            /// </param>
            /// <returns>
            /// The <see>
            ///         <cref>Dictionary</cref>
            ///     </see>
            ///     .
            /// </returns>
            public Dictionary<string, VsLicense> GetLicenses(ConnectionConfiguration configuration)
            {
                return null;
            }

            /// <summary>
            /// The get local analysis extension.
            /// </summary>
            /// <param name="configuration">
            /// The configuration.
            /// </param>
            /// <param name="project">
            /// The project.
            /// </param>
            /// <param name="sonarVersion">
            /// The sonar version.
            /// </param>
            /// <returns>
            /// The <see cref="ILocalAnalyserExtension"/>.
            /// </returns>
            public ILocalAnalyserExtension GetLocalAnalysisExtension(ConnectionConfiguration configuration, Resource project, double sonarVersion)
            {
                return null;
            }

            /// <summary>
            /// The get plugin control options.
            /// </summary>
            /// <param name="configuration">
            /// The configuration.
            /// </param>
            /// <param name="project">
            /// The project.
            /// </param>
            /// <returns>
            /// The <see cref="IPluginsOptions"/>.
            /// </returns>
            public IPluginsOptions GetPluginControlOptions(ConnectionConfiguration configuration, Resource project)
            {
                return null;
            }

            public IPluginsOptions GetPluginControlOptions(Resource associatedProject)
            {
                throw new System.NotImplementedException();
            }

            public bool IsSupported(VsProjectItem fileToAnalyse)
            {
                throw new System.NotImplementedException();
            }

            /// <summary>
            /// The get resource key.
            /// </summary>
            /// <param name="projectItem">
            /// The project item.
            /// </param>
            /// <param name="projectKey">
            /// The project key.
            /// </param>
            /// <returns>
            /// The <see cref="string"/>.
            /// </returns>
            public string GetResourceKey(VsProjectItem projectItem, string projectKey)
            {
                return string.Empty;
            }

            /// <summary>
            /// The is supported.
            /// </summary>
            /// <param name="configuration">
            /// The configuration.
            /// </param>
            /// <param name="project">
            /// The project.
            /// </param>
            /// <returns>
            /// The <see cref="bool"/>.
            /// </returns>
            public bool IsSupported(ConnectionConfiguration configuration, Resource project)
            {
                return true;
            }

            #endregion

            public string GetVersion()
            {
                throw new System.NotImplementedException();
            }

            public string GetAssemblyPath()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}