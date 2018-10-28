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
    using SonarRestService;
    using SonarRestService.Types;

    [TestFixture]
    class AssociationModelTests
    {
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
            var mockanalyser = new Mock<ISonarLocalAnalyser>();

            AssociationModel associationModel;            
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockConfiguration.Object, mockTranslator.Object, mockPlugin.Object, null, mockanalyser.Object, "14.0");
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
            var mockanalyser = new Mock<ISonarLocalAnalyser>();

            AssociationModel associationModel;
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockConfiguration.Object, mockTranslator.Object, mockPlugin.Object, new SonarQubeViewModel("test", mockConfiguration.Object), mockanalyser.Object, "14.0");
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
            var mockanalyser = new Mock<ISonarLocalAnalyser>();

            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = "dummy"});
            var mockObj = mockConfiguration.Object;

            AssociationModel associationModel;
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockObj, mockTranslator.Object, mockPlugin.Object, new SonarQubeViewModel("test", mockObj), mockanalyser.Object, "14.0");
            associationModel.UpdateServicesInModels(mockVsHelper.Object, null, null);
            associationModel.OpenSolutionName = "solution";
            associationModel.OpenSolutionPath = "path";


            Assert.That(associationModel.AssignASonarProjectToSolution(new Resource() { IsBranch = false }, null, mockSourceProvider.Object), Is.True);
            Assert.That(associationModel.IsAssociated, Is.True);
        }

        [Test]
        public void AssociateProjectFailesIfPathNotDefined()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockPlugin = new Mock<IPluginManager>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();
            var mockanalyser = new Mock<ISonarLocalAnalyser>();

            mockConfiguration.Setup(x => x.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = "dummy" });
            var mockObj = mockConfiguration.Object;

            AssociationModel associationModel;
            associationModel = new AssociationModel(mockLogger.Object, mockRest.Object, mockObj, mockTranslator.Object, mockPlugin.Object, new SonarQubeViewModel("test", mockObj), mockanalyser.Object, "14.0");
            associationModel.UpdateServicesInModels(mockVsHelper.Object, null, null);
            Assert.IsFalse(associationModel.AssignASonarProjectToSolution(new Resource() { IsBranch = false }, new Resource() { Default = true }, mockSourceProvider.Object).GetAwaiter().GetResult());
        }
    }
}
