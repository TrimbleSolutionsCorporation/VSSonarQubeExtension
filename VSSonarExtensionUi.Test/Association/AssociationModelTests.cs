namespace VSSonarExtensionUi.Test.Association
{
    using NUnit.Framework;
    using System;
    using VSSonarPlugins.Types;
    using Moq;
    using SonarLocalAnalyser;
    using VSSonarPlugins;
    using ViewModel.Helpers;
    using ViewModel;
    using Model.Helpers;
    using ViewModel.Configuration;
    using System.Collections.Generic;
    using VSSonarExtensionUi.Association;

    [TestFixture]
    class AssociationModelTests
    {
        [Test]
        public void CreateResourcePathFileIsNull()
        {
            AssociationModel associationModel;
            associationModel =
              new AssociationModel(null, null, null, null, null, null);
            Assert.That(associationModel.CreateResourcePathFile(null, null), Is.Null);
        }

        [Test]
        public void CreateResourcePathFileThrowsNotImplementedException170()
        {
            AssociationModel associationModel;
            Resource resource;
            StandAloneVsHelper s0 = new StandAloneVsHelper();
            associationModel = new AssociationModel(null, null, null, null, null, null);
            associationModel.UpdateServicesInModels(s0, null, null);
            Assert.Throws<NotImplementedException>(() => resource = associationModel.CreateResourcePathFile((string)null, (Resource)null));
        }


        [Test]
        public void AssignWhenNoProjectDefinedReturnsFalse()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockPlugin = new Mock<IPluginManager>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();
            
            AssociationModel associationModel;            
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockConfiguration.Object, mockTranslator.Object, mockPlugin.Object, null);
            associationModel.UpdateServicesInModels(mockVsHelper.Object, null, null);

            Assert.That(associationModel.AssignASonarProjectToSolution(null, null), Is.False);
        }

        [Test]
        public void AssignWhenProjectIsMainAndBranchIsNullReturnsFalse()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockPlugin = new Mock<IPluginManager>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            AssociationModel associationModel;
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockConfiguration.Object, mockTranslator.Object, mockPlugin.Object, new SonarQubeViewModel("test"));
            associationModel.UpdateServicesInModels(mockVsHelper.Object, null, null);

            Assert.That(associationModel.AssignASonarProjectToSolution(new Resource() { IsBranch = true}, null), Is.False);
        }


        [Test]
        public void AssignProjectIsNotMainReturnsTrue()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockPlugin = new Mock<IPluginManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = "dummy"});
            var mockObj = mockConfiguration.Object;

            AssociationModel associationModel;
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockObj, mockTranslator.Object, mockPlugin.Object, new SonarQubeViewModel("test", mockObj));
            associationModel.UpdateServicesInModels(mockVsHelper.Object, null, null);

            Assert.That(associationModel.AssignASonarProjectToSolution(new Resource() { IsBranch = false }, null), Is.True);
            Assert.That(associationModel.IsAssociated, Is.True);
        }

        [Test]
        public void AssignProjectIsMainReturnsTrueWhenBranchIsDefined()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockPlugin = new Mock<IPluginManager>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = "dummy" });
            var mockObj = mockConfiguration.Object;

            AssociationModel associationModel;
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockObj, mockTranslator.Object, mockPlugin.Object, new SonarQubeViewModel("test", mockObj));
            associationModel.UpdateServicesInModels(mockVsHelper.Object, null, null);

            Assert.That(associationModel.AssignASonarProjectToSolution(new Resource() { IsBranch = false }, new Resource() { Default = true }), Is.True);
            Assert.That(associationModel.IsAssociated, Is.True);
        }


        [Test]
        public void CreatesASingleProjectWhenOnlyBranches()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockPlugin = new Mock<IPluginManager>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = "dummy" });
            var mockObj = mockConfiguration.Object;

            var brancheData = new List<Resource>();
            brancheData.Add(new Resource() { Key = "tekla.utilities:project:master", Name = "project master", BranchName = "master", IsBranch = true });
            brancheData.Add(new Resource() { Key = "tekla.utilities:project:feature_A", Name = "project feature_A", BranchName = "feature_A", IsBranch = true });
            brancheData.Add(new Resource() { Key = "tekla.utilities:project:feature_B", Name = "project feature_B", BranchName = "feature_B", IsBranch = true });

            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(brancheData);
            AssociationModel associationModel;
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockObj, mockTranslator.Object, mockPlugin.Object, new SonarQubeViewModel("test", mockObj));
            associationModel.RefreshProjectList(false);
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(1));
            Assert.That(associationModel.AvailableProjects[0].BranchResources.Count, Is.EqualTo(3));
            Assert.That(associationModel.AvailableProjects[0].Name, Is.EqualTo("project"));
            Assert.That(associationModel.AvailableProjects[0].BranchResources[0].BranchName, Is.EqualTo("master"));
            Assert.That(associationModel.AvailableProjects[0].BranchResources[1].BranchName, Is.EqualTo("feature_A"));
            Assert.That(associationModel.AvailableProjects[0].BranchResources[2].BranchName, Is.EqualTo("feature_B"));
        }

        [Test]
        public void CreatesBranchesAndNormalProjects()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockPlugin = new Mock<IPluginManager>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = "dummy" });
            var mockObj = mockConfiguration.Object;

            var brancheData = new List<Resource>();
            brancheData.Add(new Resource() { Key = "tekla.utilities:project:master", Name = "project master", BranchName = "master", IsBranch = true });
            brancheData.Add(new Resource() { Key = "tekla.utilities:project:feature_A", Name = "project feature_A", BranchName = "feature_A", IsBranch = true });
            brancheData.Add(new Resource() { Key = "org.apache.xbean:xbean", Name = "Apache XBean" });
            brancheData.Add(new Resource() { Key = "org.apache.activemq:activemq-parent", Name = "ActiveMQ" });
            brancheData.Add(new Resource() { Key = "org.apache.maven:maven", Name = "Apache Maven" });

            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(brancheData);
            AssociationModel associationModel;
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockObj, mockTranslator.Object, mockPlugin.Object, new SonarQubeViewModel("test", mockObj));
            associationModel.RefreshProjectList(false);
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(4));
            Assert.That(associationModel.AvailableProjects[0].Name, Is.EqualTo("ActiveMQ"));
            Assert.That(associationModel.AvailableProjects[0].IsBranch, Is.False);
            Assert.That(associationModel.AvailableProjects[1].Name, Is.EqualTo("Apache Maven"));
            Assert.That(associationModel.AvailableProjects[1].IsBranch, Is.False);
            Assert.That(associationModel.AvailableProjects[2].Name, Is.EqualTo("Apache XBean"));
            Assert.That(associationModel.AvailableProjects[2].IsBranch, Is.False);
            Assert.That(associationModel.AvailableProjects[3].BranchResources.Count, Is.EqualTo(2));
            Assert.That(associationModel.AvailableProjects[3].BranchResources[0].BranchName, Is.EqualTo("master"));
            Assert.That(associationModel.AvailableProjects[3].BranchResources[1].BranchName, Is.EqualTo("feature_A"));
        }

        [Test]
        public void GetsMasterBranchIfBranchIsNotDetected()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockPlugin = new Mock<IPluginManager>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = "dummy" });
            var mockObj = mockConfiguration.Object;

            var brancheData = new List<Resource>();
            brancheData.Add(new Resource() { Key = "tekla.utilities:project:master", Name = "project master", BranchName = "master", IsBranch = true });
            brancheData.Add(new Resource() { Key = "tekla.utilities:project:feature_A", Name = "project feature_A", BranchName = "feature_A", IsBranch = true });
            brancheData.Add(new Resource() { Key = "org.apache.xbean:xbean", Name = "Apache XBean" });
            brancheData.Add(new Resource() { Key = "org.apache.activemq:activemq-parent", Name = "ActiveMQ" });
            brancheData.Add(new Resource() { Key = "org.apache.maven:maven", Name = "Apache Maven" });

            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(brancheData);
            mockSourceProvider.Setup(x => x.GetBranch()).Returns("feature-x");
            AssociationModel associationModel;
            var model = new SonarQubeViewModel("test", mockObj);
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockObj, mockTranslator.Object, mockPlugin.Object, model, mockSourceProvider.Object);
            associationModel.RefreshProjectList(false);
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(4));

            associationModel.SelectedProjectInView = associationModel.AvailableProjects[3];
            Assert.That(associationModel.SelectedBranchProject.Name, Is.EqualTo("project master"));
            Assert.That(associationModel.SelectedBranchProject.BranchName, Is.EqualTo("master"));
            Assert.That(associationModel.SelectedBranchProject.Key, Is.EqualTo("tekla.utilities:project:master"));
            Assert.That(model.StatusMessageAssociation, Is.EqualTo("Using master branch, because current branch does not exist or source control not supported. Press associate to confirm."));
        }

        [Test]
        public void GetsBranchIfBranchIsDetected()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockPlugin = new Mock<IPluginManager>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = "dummy" });
            var mockObj = mockConfiguration.Object;

            var brancheData = new List<Resource>();
            brancheData.Add(new Resource() { Key = "tekla.utilities:project:master", Name = "project master", BranchName = "master", IsBranch = true });
            brancheData.Add(new Resource() { Key = "tekla.utilities:project:feature_A", Name = "project feature_A", BranchName = "feature_A", IsBranch = true });
            brancheData.Add(new Resource() { Key = "org.apache.xbean:xbean", Name = "Apache XBean" });
            brancheData.Add(new Resource() { Key = "org.apache.activemq:activemq-parent", Name = "ActiveMQ" });
            brancheData.Add(new Resource() { Key = "org.apache.maven:maven", Name = "Apache Maven" });

            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(brancheData);
            mockSourceProvider.Setup(x => x.GetBranch()).Returns("feature_A");
            AssociationModel associationModel;
            var model = new SonarQubeViewModel("test", mockObj);
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockObj, mockTranslator.Object, mockPlugin.Object, model, mockSourceProvider.Object);
            associationModel.RefreshProjectList(false);
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(4));

            associationModel.SelectedProjectInView = associationModel.AvailableProjects[3];
            Assert.That(associationModel.SelectedBranchProject.Name, Is.EqualTo("project feature_A"));
            Assert.That(associationModel.SelectedBranchProject.BranchName, Is.EqualTo("feature_A"));
            Assert.That(associationModel.SelectedBranchProject.Key, Is.EqualTo("tekla.utilities:project:feature_A"));
            Assert.That(model.StatusMessageAssociation, Is.EqualTo("Association Ready. Press associate to confirm."));
        }

        [Test]
        public void DoesNotGetBranchIfCannotDetectBranch()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockPlugin = new Mock<IPluginManager>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = "dummy" });
            var mockObj = mockConfiguration.Object;

            var brancheData = new List<Resource>();
            brancheData.Add(new Resource() { Key = "tekla.utilities:project:feature_A", Name = "project feature_A", BranchName = "feature_A", IsBranch = true });
            brancheData.Add(new Resource() { Key = "org.apache.xbean:xbean", Name = "Apache XBean" });
            brancheData.Add(new Resource() { Key = "org.apache.activemq:activemq-parent", Name = "ActiveMQ" });
            brancheData.Add(new Resource() { Key = "org.apache.maven:maven", Name = "Apache Maven" });

            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(brancheData);
            mockSourceProvider.Setup(x => x.GetBranch()).Returns(string.Empty);
            AssociationModel associationModel;
            var model = new SonarQubeViewModel("test", mockObj);
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockObj, mockTranslator.Object, mockPlugin.Object, model, mockSourceProvider.Object);
            associationModel.RefreshProjectList(false);
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(4));

            associationModel.SelectedProjectInView = associationModel.AvailableProjects[3];
            Assert.That(associationModel.SelectedBranchProject, Is.Null);
            Assert.That(model.StatusMessageAssociation, Is.EqualTo("Unable to find branch, please manually choose one from list and confirm."));
        }

        [Test]
        public void SetsCorrectProjectWithoutBranches()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockPlugin = new Mock<IPluginManager>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = "dummy" });
            var mockObj = mockConfiguration.Object;

            var brancheData = new List<Resource>();
            brancheData.Add(new Resource() { Key = "tekla.utilities:project:feature_A", Name = "project feature_A", BranchName = "feature_A", IsBranch = true });
            brancheData.Add(new Resource() { Key = "org.apache.xbean:xbean", Name = "Apache XBean" });
            brancheData.Add(new Resource() { Key = "org.apache.activemq:activemq-parent", Name = "ActiveMQ" });
            brancheData.Add(new Resource() { Key = "org.apache.maven:maven", Name = "Apache Maven" });

            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(brancheData);
            mockSourceProvider.Setup(x => x.GetBranch()).Returns(string.Empty);
            AssociationModel associationModel;
            var model = new SonarQubeViewModel("test", mockObj);
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockObj, mockTranslator.Object, mockPlugin.Object, model, mockSourceProvider.Object);
            associationModel.RefreshProjectList(false);
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(4));

            associationModel.SelectedProjectInView = associationModel.AvailableProjects[1];
            Assert.That(associationModel.SelectedBranchProject, Is.Null);
            Assert.That(model.StatusMessageAssociation, Is.EqualTo("Normal project type. Press associate to confirm."));
        }

        [Test]
        public void WhenSettingNullProjectItShouldClearData()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockPlugin = new Mock<IPluginManager>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = "dummy" });
            var mockObj = mockConfiguration.Object;

            var brancheData = new List<Resource>();
            brancheData.Add(new Resource() { Key = "tekla.utilities:project:feature_A", Name = "project feature_A", BranchName = "feature_A", IsBranch = true });
            brancheData.Add(new Resource() { Key = "org.apache.xbean:xbean", Name = "Apache XBean" });
            brancheData.Add(new Resource() { Key = "org.apache.activemq:activemq-parent", Name = "ActiveMQ" });
            brancheData.Add(new Resource() { Key = "org.apache.maven:maven", Name = "Apache Maven", Version = "Work" });

            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(brancheData);
            mockSourceProvider.Setup(x => x.GetBranch()).Returns(string.Empty);
            AssociationModel associationModel;
            var model = new SonarQubeViewModel("test", mockObj);
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockObj, mockTranslator.Object, mockPlugin.Object, model, mockSourceProvider.Object);
            associationModel.RefreshProjectList(false);
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(4));

            associationModel.SelectedProjectInView = associationModel.AvailableProjects[1];
            Assert.That(associationModel.SelectedProjectName, Is.EqualTo("Apache Maven"));
            Assert.That(associationModel.SelectedProjectKey, Is.EqualTo("org.apache.maven:maven"));
            Assert.That(associationModel.SelectedProjectVersion, Is.EqualTo("Work"));
            Assert.That(associationModel.SelectedBranchProject, Is.Null);
            Assert.That(model.StatusMessageAssociation, Is.EqualTo("Normal project type. Press associate to confirm."));

            associationModel.SelectedProjectInView = null;
            Assert.That(associationModel.SelectedBranchProject, Is.Null);
            Assert.That(model.StatusMessageAssociation, Is.EqualTo("No project selected, select from above."));
        }
    }
}
