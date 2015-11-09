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

            Assert.That(associationModel.AssignASonarProjectToSolution(null, null, mockSourceProvider.Object), Is.False);
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

            Assert.That(associationModel.AssignASonarProjectToSolution(new Resource() { IsBranch = true }, null, mockSourceProvider.Object), Is.False);
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

            Assert.That(associationModel.AssignASonarProjectToSolution(new Resource() { IsBranch = false }, null, mockSourceProvider.Object), Is.True);
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

            Assert.That(associationModel.AssignASonarProjectToSolution(new Resource() { IsBranch = false }, new Resource() { Default = true }, mockSourceProvider.Object), Is.True);
            Assert.That(associationModel.IsAssociated, Is.True);
        }
    }
}
