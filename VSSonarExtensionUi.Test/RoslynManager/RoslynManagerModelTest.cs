
namespace VSSonarExtensionUi.Test.RoslynManager
{
    using Moq;
    using NUnit.Framework;
    using SonarLocalAnalyser;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using VSSonarExtensionUi.Model.Configuration;
    using Model.Helpers;
    using VSSonarExtensionUi.ViewModel.Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;


    /// <summary>
    /// test roslyn
    /// </summary>
    [TestFixture]
    public class RoslynManagerModelTest
    {
        /// <summary>
        /// The runnin path
        /// </summary>
        public string runninPath = Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty)).ToString();

        /// <summary>
        /// Defaults the contains correct ammout of checks.
        /// </summary>
        [Test]
        public void DefaultContainsCorrectAmmoutOfChecks()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockNotifier = new Mock<INotificationManager>();
            var pluginAnalysis = new Mock<IAnalysisPlugin>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            mockConfiguration.Setup(x => x.ApplicationPath).Returns(runninPath);

            var plugins = new List<IAnalysisPlugin>();
            plugins.Add(pluginAnalysis.Object);

            var roslynModel = new RoslynManagerModel(plugins, mockNotifier.Object, mockConfiguration.Object, mockRest.Object);

            Assert.That(roslynModel.ExtensionDiagnostics.Count, Is.EqualTo(0));
        }
    }
}
