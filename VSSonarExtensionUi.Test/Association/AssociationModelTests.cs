namespace VSSonarExtensionUi.Test.Association
{
    using NUnit.Framework;
    using System;
    using Model.Association;
    using VSSonarPlugins.Types;
    using Moq;
    using SonarLocalAnalyser;
    using VSSonarPlugins;
    using ViewModel.Helpers;
    using ViewModel;

    [TestFixture]
    class AssociationModelTests
    {
        [Test]
        public void CreateResourcePathFileIsNull()
        {
            AssociationModel associationModel;
            Resource resource;
            associationModel =
              new AssociationModel(null, null, null, null, null, null);
            Assert.That(associationModel.CreateResourcePathFile((string)null, (Resource)null), Is.Null);
        }

        [Test]
        public void CreateResourcePathFileThrowsNotImplementedException170()
        {
            AssociationModel associationModel;
            Resource resource;
            StandAloneVsHelper s0 = new StandAloneVsHelper();
            associationModel = new AssociationModel(null, null, null, null, null, null);
            associationModel.VsHelper = s0;
            Assert.Throws<NotImplementedException>(() => resource = associationModel.CreateResourcePathFile((string)null, (Resource)null));
        }


        [Test]
        public void AssignWhenNoProjectDefinedReturnsFalse()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<IVsSonarExtensionLogger>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();
            
            AssociationModel associationModel;            
            associationModel = new AssociationModel(mockTranslator.Object, mockLogger.Object, mockRest.Object, mockConfiguration.Object, mockSourceProvider.Object, null);
            associationModel.VsHelper = mockVsHelper.Object;

            Assert.That(associationModel.AssignASonarProjectToSolution(null, null), Is.False);
        }

        [Test]
        public void AssignWhenProjectIsMainAndBranchIsNullReturnsFalse()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<IVsSonarExtensionLogger>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            AssociationModel associationModel;
            associationModel = new AssociationModel(mockTranslator.Object, mockLogger.Object, mockRest.Object, mockConfiguration.Object, mockSourceProvider.Object, new SonarQubeViewModel("test"));
            associationModel.VsHelper = mockVsHelper.Object;

            Assert.That(associationModel.AssignASonarProjectToSolution(new Resource() { IsBranch = true}, null), Is.False);
        }


        [Test]
        public void AssignProjectIsNotMainReturnsTrue()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<IVsSonarExtensionLogger>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = "dummy"});
            var mockObj = mockConfiguration.Object;

            AssociationModel associationModel;
            associationModel = new AssociationModel(mockTranslator.Object, mockLogger.Object, mockRest.Object, mockObj, mockSourceProvider.Object, new SonarQubeViewModel("test", mockObj));
            associationModel.VsHelper = mockVsHelper.Object;

            Assert.That(associationModel.AssignASonarProjectToSolution(new Resource() { IsBranch = false }, null), Is.True);
            Assert.That(associationModel.IsAssociated, Is.True);
        }

        [Test]
        public void AssignProjectIsMainReturnsTrueWhenBranchIsDefined()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<IVsSonarExtensionLogger>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = "dummy" });
            var mockObj = mockConfiguration.Object;

            AssociationModel associationModel;
            associationModel = new AssociationModel(mockTranslator.Object, mockLogger.Object, mockRest.Object, mockObj, mockSourceProvider.Object, new SonarQubeViewModel("test", mockObj));
            associationModel.VsHelper = mockVsHelper.Object;

            Assert.That(associationModel.AssignASonarProjectToSolution(new Resource() { IsBranch = false }, new Resource() { Default = true }), Is.True);
            Assert.That(associationModel.IsAssociated, Is.True);
        }
    }
}
