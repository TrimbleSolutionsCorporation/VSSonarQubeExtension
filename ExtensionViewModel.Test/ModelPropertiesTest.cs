// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelPropertiesTest.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    /// The comment on issue command test.
    /// </summary>
    [TestFixture]
    public class ModelPropertiesTest
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
        public void TestDefaultConstructor()
        {
            var model = new ExtensionDataModel();
            Assert.AreEqual(0, model.Issues.Count);
        }

        /// <summary>
        /// The test loading of window.
        /// </summary>
        [Test]
        public void TestUpdateModelUpdateData()
        {
            var model = new ExtensionDataModel();
            model.ExtensionDataModelUpdate(this.service, this.vshelper, new Resource());
            Assert.AreEqual(0, model.Issues.Count);
            Assert.AreEqual(this.service, model.RestService);
            Assert.AreEqual(this.vshelper, model.Vsenvironmenthelper);
        }

        /// <summary>
        /// The test loading of window.
        /// </summary>
        [Test]
        public void TestSetIssuesList()
        {
            var issueWithId = new Issue { Id = 20, Component = "asdaskjd:sdaskjd:aksjdkas/asdkasj.cs", Key = new Guid() };

            var model = new ExtensionDataModel(this.service, this.vshelper, null);
            model.ResourceInEditor = new Resource { Key = "asdaskjd:sdaskjd:aksjdkas/asdkasj.cs" };
            model.DocumentInView = "aksjdkas/asdkasj.cs";
            model.ReplaceAllIssuesInCache(new List<Issue> { issueWithId });

            Assert.AreEqual(1, model.Issues.Count);
            Assert.AreEqual("Number of Issues: 1 ", model.StatsLabel);
            Assert.AreEqual(string.Empty, model.ErrorMessage);
        }

        /// <summary>
        /// The test loading of window.
        /// </summary>
        [Test]
        public void TestSetIssuesInEditor()
        {
            var issueWithId = new Issue { Id = 20, Component = "asdaskjd:sdaskjd:aksjdkas/asdkasj.cs", Key = new Guid() };

            var model = new ExtensionDataModel(this.service, this.vshelper, null);
            model.ResourceInEditor = new Resource { Key = "asdaskjd:sdaskjd:aksjdkas/asdkasj.cs" };
            model.DocumentInView = "aksjdkas/asdkasj.cs";
            model.ReplaceAllIssuesInCache(new List<Issue> { issueWithId });

            Assert.AreEqual(1, model.GetIssuesInEditor("asdaskjd:sdaskjd:aksjdkas/asdkasj.cs").Count);
            Assert.AreEqual(string.Empty, model.ErrorMessage);
        }

        /// <summary>
        /// The test loading of window.
        /// </summary>
        [Test]
        public void TestSetIssuesListWithCommentsWithRefreshView()
        {

            var issueWithId = new Issue { Id = 20, Component = "asdaskjd:sdaskjd:aksjdkas/asdkasj.cs", Key = new Guid() };
            issueWithId.Comments.Add(new Comment());
            var list = new List<Issue> { issueWithId };
            var model = new ExtensionDataModel(this.service, this.vshelper, null);
            model.ResourceInEditor = new Resource { Key = "asdaskjd:sdaskjd:aksjdkas/asdkasj.cs" };
            model.DocumentInView = "aksjdkas/asdkasj.cs";
            model.ReplaceAllIssuesInCache(list);


            model.SelectedIssuesInView = list;
            Assert.AreEqual(1, model.Issues.Count);
            Assert.AreEqual(1, model.Comments.Count);
            Assert.AreEqual("Number of Issues: 1 ", model.StatsLabel);
        }

        /// <summary>
        /// The test loading of window.
        /// </summary>
        [Test]
        public void TestSelectIssueFromListUsingId()
        {
            var issueWithId = new Issue { Id = 20, Component = "asdaskjd:sdaskjd:aksjdkas/asdkasj.cs" };

            var model = new ExtensionDataModel(this.service, this.vshelper, null);
            model.ReplaceAllIssuesInCache(new List<Issue> { issueWithId });
            model.SelectAIssueFromList(20);
            Assert.AreEqual(issueWithId, model.SelectedIssue);
        }

        /// <summary>
        /// The test loading of window.
        /// </summary>
        [Test]
        public void TestSelectIssueFromListUsingKey()
        {
            var issueWithId = new Issue { Id = 20, Component = "asdaskjd:sdaskjd:aksjdkas/asdkasj.cs", Key = new Guid() };

            var model = new ExtensionDataModel(this.service, this.vshelper, null);
            model.ReplaceAllIssuesInCache(new List<Issue> { issueWithId });

            model.SelectAIssueFromList(new Guid());
            Assert.AreEqual(issueWithId, model.SelectedIssue);
        }

        /// <summary>
        /// The test loading of window.
        /// </summary>
        [Test]
        public void TestSetUserList()
        {
            var model = new ExtensionDataModel(this.service, this.vshelper, null)
            {
                UsersList = new System.Collections.Generic.List<User>
                                        {
                                           new User()
                                        }
            };
            Assert.AreEqual(1, model.UsersList.Count);
        }

        /// <summary>
        /// The test association with solution.
        /// </summary>
        [Test]
        public void TestAssociationWithSolution()
        {
            var model = new ExtensionDataModel(this.service, this.vshelper, null);
            model.PluginController = new PluginController();
            var projectAsso = new Resource { Key = "KEY" };

            model.AssociateProjectToSolution();
            Assert.AreEqual("Selected Project: KEY", model.AssociatedProjectKey);
            model.AssociateProjectToSolution();
            Assert.AreEqual("Selected Project: KEY", model.AssociatedProjectKey);
        }

        /// <summary>
        /// The test loading of window.
        /// </summary>
        [Test]
        public void TesRestOfProperties()
        {
            var issue = new Issue();
            var connector = new SonarRestService(new JsonSonarConnector());
            var projectAsso = new Resource { Key = "proj" };
            var model = new ExtensionDataModel(this.service, this.vshelper, null)
            {
                AssociatedProjectKey = "proj",
                CommentData = "comment",
                SelectedIssue = issue,
                SonarInfo = "ver",
                SelectedUser = new User { Login = "login" },
                DiagnosticMessage = "MessageData",
                DisableEditorTags = false,
                RestService = connector,
                AssociatedProject = projectAsso,
            };

            Assert.AreEqual("Selected Project: proj", model.AssociatedProjectKey);            
            Assert.AreEqual(issue, model.SelectedIssue);
            Assert.AreEqual("comment", model.CommentData);
            Assert.AreEqual("login", model.SelectedUser.Login);
            Assert.AreEqual("ver", model.SonarInfo);
            Assert.AreEqual("MessageData", model.DiagnosticMessage);
            Assert.IsFalse(model.DisableEditorTags);
            Assert.AreEqual(connector, model.RestService);
            Assert.AreEqual(projectAsso, model.AssociatedProject);
        }
    }
}
