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
            service.GetIssuesInResource(config, "resource");
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

        #endregion
    }
}