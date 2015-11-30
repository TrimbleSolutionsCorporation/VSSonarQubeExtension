
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

            var plugins = new List<IAnalysisPlugin>();
            plugins.Add(pluginAnalysis.Object);


            var roslynModel = new RoslynManagerModel(plugins, mockNotifier.Object, mockConfiguration.Object);

            Assert.That(roslynModel.ExtensionDiagnostics.Count, Is.EqualTo(2));
            Assert.That(roslynModel.ExtensionDiagnostics["SonarLint.CSharp.dll"].AvailableChecks.Count, Is.EqualTo(121));
            Assert.That(roslynModel.ExtensionDiagnostics["SonarLint.VisualBasic.dll"].AvailableChecks.Count, Is.EqualTo(30));            
        }

        [Test]
        public void AddsRoslynCheckOk()
        {
            var mockTranslator = new Mock<ISQKeyTranslator>();
            var mockNotifier = new Mock<INotificationManager>();
            var pluginAnalysis = new Mock<IAnalysisPlugin>();
            var mockRest = new Mock<ISonarRestService>();
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<IConfigurationHelper>();
            var mockSourceProvider = new Mock<ISourceControlProvider>();
            var mockVsHelper = new Mock<IVsEnvironmentHelper>();

            var plugins = new List<IAnalysisPlugin>();
            plugins.Add(pluginAnalysis.Object);


            var roslynModel = new RoslynManagerModel(plugins, mockNotifier.Object, mockConfiguration.Object);
            
            Assert.That(roslynModel.AddNewRoslynPack(Path.Combine(this.runninPath, "TestData", "SonarLintDummy.dll")), Is.True);
            Assert.That(roslynModel.ExtensionDiagnostics.Count, Is.EqualTo(3));

            Assert.That(roslynModel.AddNewRoslynPack(Path.Combine(this.runninPath, "externalAnalysers", "roslynDiagnostics", "SonarLint.dll")), Is.False);
            Assert.That(roslynModel.ExtensionDiagnostics.Count, Is.EqualTo(3));
        }
    }
}
