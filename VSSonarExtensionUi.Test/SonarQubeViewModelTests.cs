namespace VSSonarExtensionUi.Test.Association
{
    using NUnit.Framework;
    using System.IO;
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
    using System.Reflection;

    [TestFixture]
    class SonarQubeViewModelTests
    {
        string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);

        Mock<ISQKeyTranslator> mockTranslator;
        Mock<ISonarRestService> mockRest;
        Mock<INotificationManager> mockLogger;
        Mock<IConfigurationHelper> mockConfiguration;
        Mock<IPluginManager> mockPlugin;
        Mock<ISourceControlProvider> mockSourceProvider;
        Mock<IVsEnvironmentHelper> mockVsHelper;

        List<Resource> CreatProjects()
        {
            var brancheData = new List<Resource>();
            brancheData.Add(new Resource { Key = "tekla.utilities:project", Name = "project", IsBranch = false });
            brancheData.Add(new Resource { Key = "tekla.utilities:project:master", Name = "project master", BranchName = "master", IsBranch = true });
            brancheData.Add(new Resource { Key = "tekla.utilities:project:feature_A", Name = "project feature_A", BranchName = "feature_A", IsBranch = true });
            brancheData.Add(new Resource { Key = "tekla.utilities:project:feature_B", Name = "project feature_B", BranchName = "feature_B", IsBranch = true });
            return brancheData;
        }

        [SetUp]
        public void Setup()
        {
            mockTranslator = new Mock<ISQKeyTranslator>();
            mockRest = new Mock<ISonarRestService>();
            mockLogger = new Mock<INotificationManager>();
            mockConfiguration = new Mock<IConfigurationHelper>();
            mockPlugin = new Mock<IPluginManager>();
            mockSourceProvider = new Mock<ISourceControlProvider>();
            mockVsHelper = new Mock<IVsEnvironmentHelper>();
        }

        [Test]
        public void OnDisconectInNotAssociatedShouldClearAllData()
        {
            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties { Value = "dummy" });
            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(this.CreatProjects());

            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object);
            associationModel.RefreshProjectList(false);
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(2));

            associationModel.OnDisconnectToSonar();
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(0));
            Assert.That(associationModel.SelectedProjectInView, Is.Null);
            Assert.That(associationModel.SelectedProjectName, Is.EqualTo(""));
            Assert.That(associationModel.SelectedProjectKey, Is.EqualTo(""));
            Assert.That(associationModel.SelectedProjectVersion, Is.EqualTo(""));
            Assert.That(associationModel.IsConnected, Is.False);
        }

        [Test]
        public void OnConnectWithoutSolutionOpenShouldNotAssociate()
        {
            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties { Value = "dummy" });
            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(this.CreatProjects());
            mockRest.Setup(x => x.AuthenticateUser(It.IsAny<ISonarConfiguration>())).Returns(true);

            AuthtenticationHelper.EstablishAConnection(mockRest.Object, "as", "asda", "asd");

            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object);
            associationModel.VsHelper = mockVsHelper.Object;

            associationModel.OnConnectToSonar(false);

            WaitForCompletionOrTimeout(associationModel);

            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(2));
            Assert.That(associationModel.AssociationModule.IsAssociated, Is.False);
            Assert.That(associationModel.SelectedProjectInView, Is.Null);
            Assert.That(associationModel.SelectedProjectName, Is.Null);
            Assert.That(associationModel.SelectedProjectKey, Is.Null);
            Assert.That(associationModel.SelectedProjectVersion, Is.Null);
            Assert.That(associationModel.IsConnected, Is.True);
        }

        [Test]
        public void OnConnectWithSolutionOpenShouldAssociateToMasterWhenBranchIsMaster()
        {
            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties { Value = "project_Main" });
            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(this.CreatProjects());
            mockRest.Setup(x => x.AuthenticateUser(It.IsAny<ISonarConfiguration>())).Returns(true);
            mockVsHelper.Setup(x => x.ActiveSolutionName()).Returns("solutionaname");
            mockVsHelper.Setup(x => x.ActiveSolutionPath()).Returns("solutionapath");

            AuthtenticationHelper.EstablishAConnection(mockRest.Object, "as", "asda", "asd");

            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object, mockSourceProvider.Object, mockPlugin.Object);
            associationModel.VsHelper = mockVsHelper.Object;
            associationModel.IsSolutionOpen = true;            
            associationModel.OnConnectToSonar(false);
            WaitForCompletionOrTimeout(associationModel);

            Assert.That(associationModel.AssociationModule.IsAssociated, Is.True);
            Assert.That(associationModel.AssociationModule.AssociatedProject.Name, Is.EqualTo("project master"));
            Assert.That(associationModel.AssociationModule.AssociatedProject.Key, Is.EqualTo("tekla.utilities:project:master"));
            Assert.That(associationModel.IsConnected, Is.True);
        }

        [Test]
        public void OnConnectWithSolutionOpenShouldAssociateToMasterWhenBranchIsNotAvailableInServer()
        {
            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties { Value = "project_Main" });
            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(this.CreatProjects());
            mockRest.Setup(x => x.AuthenticateUser(It.IsAny<ISonarConfiguration>())).Returns(true);
            mockVsHelper.Setup(x => x.ActiveSolutionName()).Returns("solutionaname");
            mockVsHelper.Setup(x => x.ActiveSolutionPath()).Returns("solutionapath");
            mockSourceProvider.Setup(x => x.GetBranch()).Returns("feature/1234-asdas");

            AuthtenticationHelper.EstablishAConnection(mockRest.Object, "as", "asda", "asd");

            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object, mockSourceProvider.Object, mockPlugin.Object);
            associationModel.VsHelper = mockVsHelper.Object;
            associationModel.IsSolutionOpen = true;
            associationModel.OnConnectToSonar(false);
            WaitForCompletionOrTimeout(associationModel);

            Assert.That(associationModel.AssociationModule.IsAssociated, Is.True);
            Assert.That(associationModel.AssociationModule.AssociatedProject.Name, Is.EqualTo("project master"));
            Assert.That(associationModel.AssociationModule.AssociatedProject.Key, Is.EqualTo("tekla.utilities:project:master"));
            Assert.That(associationModel.IsConnected, Is.True);
        }

        [Test]
        public void OnConnectWithSolutionOpenShouldAssociateToBranchWhenBranchIsAvailableInServer()
        {
            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties { Value = "project_Main" });
            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(this.CreatProjects());
            mockRest.Setup(x => x.AuthenticateUser(It.IsAny<ISonarConfiguration>())).Returns(true);
            mockVsHelper.Setup(x => x.ActiveSolutionName()).Returns("solutionaname");
            mockVsHelper.Setup(x => x.ActiveSolutionPath()).Returns("solutionapath");
            mockSourceProvider.Setup(x => x.GetBranch()).Returns("feature_A");

            AuthtenticationHelper.EstablishAConnection(mockRest.Object, "as", "asda", "asd");

            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object, mockSourceProvider.Object, mockPlugin.Object);
            associationModel.VsHelper = mockVsHelper.Object;
            associationModel.IsSolutionOpen = true;
            associationModel.OnConnectToSonar(false);
            WaitForCompletionOrTimeout(associationModel);

            Assert.That(associationModel.AssociationModule.IsAssociated, Is.True);
            Assert.That(associationModel.AssociationModule.AssociatedProject.Name, Is.EqualTo("project feature_A"));
            Assert.That(associationModel.AssociationModule.AssociatedProject.Key, Is.EqualTo("tekla.utilities:project:feature_A"));
            Assert.That(associationModel.IsConnected, Is.True);
        }

        [Test]
        public void OnConnectWithSolutionOpenShouldAssociateToANormalProject()
        {
            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties { Value = "tekla.utilities:project" });
            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(this.CreatProjects());
            mockRest.Setup(x => x.AuthenticateUser(It.IsAny<ISonarConfiguration>())).Returns(true);
            mockVsHelper.Setup(x => x.ActiveSolutionName()).Returns("solutionaname");
            mockVsHelper.Setup(x => x.ActiveSolutionPath()).Returns("solutionapath");

            AuthtenticationHelper.EstablishAConnection(mockRest.Object, "as", "asda", "asd");

            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object, mockSourceProvider.Object, mockPlugin.Object);
            associationModel.VsHelper = mockVsHelper.Object;
            associationModel.IsSolutionOpen = true;
            associationModel.OnConnectToSonar(false);
            WaitForCompletionOrTimeout(associationModel);

            Assert.That(associationModel.AssociationModule.IsAssociated, Is.True);
            Assert.That(associationModel.ErrorIsFound, Is.False);
            Assert.That(associationModel.ShowRightFlyout, Is.False);
            Assert.That(associationModel.StatusMessage, Is.EqualTo("successfully associated with : project master"));
            Assert.That(associationModel.IsConnected, Is.True);
            Assert.That(associationModel.AssociationModule.AssociatedProject.Name, Is.EqualTo("project"));
            Assert.That(associationModel.AssociationModule.AssociatedProject.Key, Is.EqualTo("tekla.utilities:project"));
        }

        [Test]
        public void OnConnectWithSolutionOpenButNoFormerAssociationItShouldNotAssociate()
        {
            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(this.CreatProjects());
            mockRest.Setup(x => x.AuthenticateUser(It.IsAny<ISonarConfiguration>())).Returns(true);
            mockVsHelper.Setup(x => x.ActiveSolutionName()).Returns("solutionaname");
            mockVsHelper.Setup(x => x.ActiveSolutionPath()).Returns("solutionapath");
            mockPlugin.Setup(x => x.SourceCodePlugins).Returns(new List<ISourceVersionPlugin>());

            AuthtenticationHelper.EstablishAConnection(mockRest.Object, "as", "asda", "asd");

            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object, mockSourceProvider.Object, mockPlugin.Object);
            associationModel.AssociationModule.AssociatedProject = new Resource();
            associationModel.SelectedProjectKey = "jasd";
            associationModel.SelectedProjectName = "jasd";
            associationModel.SelectedProjectVersion = "jasd";
            associationModel.VsHelper = mockVsHelper.Object;
            associationModel.IsSolutionOpen = true;
            associationModel.IsConnected = true;

            associationModel.OnSolutionOpen("abc", "dfc", "sds");
            WaitForCompletionOrTimeout(associationModel);

            Assert.That(associationModel.AssociationModule.IsAssociated, Is.False);
            Assert.That(associationModel.IsConnected, Is.True);
            Assert.That(associationModel.IsConnected, Is.True);
            Assert.That(associationModel.ErrorIsFound, Is.True);
            Assert.That(associationModel.ShowRightFlyout, Is.True);
            Assert.That(associationModel.StatusMessage, Is.EqualTo("Was unable to associate with sonar project, use project association dialog"));
            Assert.That(associationModel.AssociationModule.AssociatedProject, Is.Null);
            Assert.That(associationModel.SelectedProjectName, Is.Null);
            Assert.That(associationModel.SelectedProjectKey, Is.Null);
            Assert.That(associationModel.SelectedProjectVersion, Is.Null);
        }

        [Test]
        public void OnSolutionOpenButProjectNotFoundItShouldNotAssociate()
        {
            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(this.CreatProjects());
            mockRest.Setup(x => x.AuthenticateUser(It.IsAny<ISonarConfiguration>())).Returns(true);
            mockVsHelper.Setup(x => x.ActiveSolutionName()).Returns("solutionaname");
            mockVsHelper.Setup(x => x.ActiveSolutionPath()).Returns("solutionapath");
            mockPlugin.Setup(x => x.SourceCodePlugins).Returns(new List<ISourceVersionPlugin>());

            AuthtenticationHelper.EstablishAConnection(mockRest.Object, "as", "asda", "asd");
            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object, mockSourceProvider.Object, mockPlugin.Object);
            associationModel.AssociationModule.AssociatedProject = new Resource();
            associationModel.SelectedProjectKey = "jasd";
            associationModel.SelectedProjectName = "jasd";
            associationModel.SelectedProjectVersion = "jasd";
            associationModel.VsHelper = mockVsHelper.Object;
            associationModel.IsSolutionOpen = true;
            associationModel.IsConnected = true;

            associationModel.OnSolutionOpen("abc", "dfc", "sds");
            WaitForCompletionOrTimeout(associationModel);

            Assert.That(associationModel.AssociationModule.IsAssociated, Is.False);
            Assert.That(associationModel.IsConnected, Is.True);
            Assert.That(associationModel.ErrorIsFound, Is.True);
            Assert.That(associationModel.ShowRightFlyout, Is.True);
            Assert.That(associationModel.StatusMessage, Is.EqualTo("Was unable to associate with sonar project, use project association dialog"));
            Assert.That(associationModel.AssociationModule.AssociatedProject, Is.Null);
            Assert.That(associationModel.SelectedProjectName, Is.Null);
            Assert.That(associationModel.SelectedProjectKey, Is.Null);
            Assert.That(associationModel.SelectedProjectVersion, Is.Null);
        }

        [Test]
        public void OnConnectToSonarIfAssociationIsMadeItShouldResetItIfProjectNotFound()
        {
            mockRest.Setup(x => x.GetProjectsList(It.IsAny<ISonarConfiguration>())).Returns(this.CreatProjects());
            mockRest.Setup(x => x.AuthenticateUser(It.IsAny<ISonarConfiguration>())).Returns(true);
            mockVsHelper.Setup(x => x.ActiveSolutionName()).Returns("solutionaname");
            mockVsHelper.Setup(x => x.ActiveSolutionPath()).Returns("solutionapath");

            AuthtenticationHelper.EstablishAConnection(mockRest.Object, "as", "asda", "asd");

            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object, mockSourceProvider.Object, mockPlugin.Object);
            associationModel.VsHelper = mockVsHelper.Object;
            associationModel.IsSolutionOpen = true;
            associationModel.OnConnectToSonar(false);
            WaitForCompletionOrTimeout(associationModel);

            Assert.That(associationModel.AssociationModule.IsAssociated, Is.False);
            Assert.That(associationModel.IsConnected, Is.True);
            Assert.That(associationModel.ErrorIsFound, Is.True);
            Assert.That(associationModel.ShowRightFlyout, Is.True);
            Assert.That(associationModel.StatusMessage, Is.EqualTo("Was unable to associate with sonar project, use project association dialog"));
            Assert.That(associationModel.AssociationModule.AssociatedProject, Is.Null);
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
            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object);
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
            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object);
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
            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object, mockSourceProvider.Object);
            associationModel.RefreshProjectList(false);
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(4));

            associationModel.SelectedProjectInView = associationModel.AvailableProjects[3];
            Assert.That(associationModel.SelectedBranchProject.Name, Is.EqualTo("project master"));
            Assert.That(associationModel.SelectedBranchProject.BranchName, Is.EqualTo("master"));
            Assert.That(associationModel.SelectedBranchProject.Key, Is.EqualTo("tekla.utilities:project:master"));
            Assert.That(associationModel.StatusMessageAssociation, Is.EqualTo("Using master branch, because current branch does not exist or source control not supported. Press associate to confirm."));
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
            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object, mockSourceProvider.Object); 
            associationModel.RefreshProjectList(false);
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(4));

            associationModel.SelectedProjectInView = associationModel.AvailableProjects[3];
            Assert.That(associationModel.SelectedBranchProject.Name, Is.EqualTo("project feature_A"));
            Assert.That(associationModel.SelectedBranchProject.BranchName, Is.EqualTo("feature_A"));
            Assert.That(associationModel.SelectedBranchProject.Key, Is.EqualTo("tekla.utilities:project:feature_A"));
            Assert.That(associationModel.StatusMessageAssociation, Is.EqualTo("Association Ready. Press associate to confirm."));
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

            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object, mockSourceProvider.Object);
            associationModel.RefreshProjectList(false);
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(4));

            associationModel.SelectedProjectInView = associationModel.AvailableProjects[3];
            Assert.That(associationModel.SelectedBranchProject, Is.Null);
            Assert.That(associationModel.StatusMessageAssociation, Is.EqualTo("Unable to find branch, please manually choose one from list and confirm."));
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

            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object);
            associationModel.RefreshProjectList(false);
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(4));

            associationModel.SelectedProjectInView = associationModel.AvailableProjects[1];
            Assert.That(associationModel.SelectedBranchProject, Is.Null);
            Assert.That(associationModel.StatusMessageAssociation, Is.EqualTo("Normal project type. Press associate to confirm."));
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

            var associationModel = new SonarQubeViewModel("test", mockConfiguration.Object, mockLogger.Object, mockTranslator.Object, mockRest.Object);
            associationModel.RefreshProjectList(false);
            Assert.That(associationModel.AvailableProjects.Count, Is.EqualTo(4));

            associationModel.SelectedProjectInView = associationModel.AvailableProjects[1];
            Assert.That(associationModel.SelectedProjectName, Is.EqualTo("Apache Maven"));
            Assert.That(associationModel.SelectedProjectKey, Is.EqualTo("org.apache.maven:maven"));
            Assert.That(associationModel.SelectedProjectVersion, Is.EqualTo("Work"));
            Assert.That(associationModel.SelectedBranchProject, Is.Null);
            Assert.That(associationModel.StatusMessageAssociation, Is.EqualTo("Normal project type. Press associate to confirm."));

            associationModel.SelectedProjectInView = null;
            Assert.That(associationModel.SelectedBranchProject, Is.Null);
            Assert.That(associationModel.StatusMessageAssociation, Is.EqualTo("No project selected, select from above."));
        }

        /// <summary>
        /// Waits for completion or timeout.
        /// </summary>
        /// <param name="associationModel">The association model.</param>
        private static void WaitForCompletionOrTimeout(SonarQubeViewModel associationModel)
        {
            int timeout = 20;
            bool controlFound = false;
            for (int i = 0; i < timeout; i++)
            {
                if (!associationModel.IsExtensionBusy)
                {
                    break;
                }
                else
                {
                    System.Threading.Thread.Sleep(500);
                }
            }
        }
    }
}
