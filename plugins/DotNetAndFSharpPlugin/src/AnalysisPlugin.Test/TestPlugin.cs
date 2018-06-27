// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestPlugin.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TestServerExtension type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CsPlugin.Test
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;

    using PluginsOptionsController;

    using VSSonarPlugins.Types;

    using NUnit.Framework;

    using AnalysisPlugin;

    using VSSonarPlugins;
    using Moq;
    using System.IO;
    using System.Reflection;
    using System.Linq;
    using SonarRestService;
    /// <summary>
    ///     The test server extension.
    /// </summary>
    [TestFixture]
    public class TestPlugin
    {
        string execpath = Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")).ToString();
        string nqube = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")).ToString(), "NSonarQubeAnalyzer.exe");

        #region Public Methods and Operators

        /// <summary>
        ///     The test resource key.
        /// </summary>
        [Test]
        public void TestPluginKey()
        {
            var notificaitonManager = new Mock<INotificationManager>();
            var confManager = new Mock<IConfigurationHelper>();
            var rest = new Mock<ISonarRestService>();
            var helper = new Mock<IVsEnvironmentHelper>();

            confManager.Setup(caller => caller.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new SonarQubeProperties() { Value = nqube });


            IAnalysisPlugin plugin = new AnalysisPlugin(notificaitonManager.Object, confManager.Object, rest.Object, helper.Object);

            Assert.AreEqual("AnalysisPlugin", plugin.GetPluginDescription().Name);
        }


        #endregion

        #region Methods

        /// <summary>
        /// The test resource key.
        /// </summary>
        [Test]
        public void ShouldRunDiagnosticsIfSyncIsFalse()
        {
            var notificaitonManager = new Mock<INotificationManager>();
            var confManager = new Mock<IConfigurationHelper>();
            var helper = new Mock<IVsEnvironmentHelper>();
            var rest = new Mock<ISonarRestService>();
            var dictionaryprops = new Dictionary<string, string>();
            dictionaryprops.Add("sonar.roslyn.sync.type", "false");

            var dictionaryplugins = new Dictionary<string, string>();
            dictionaryplugins.Add("Roslyn", "1.0");

            rest.Setup(caller => caller.GetInstalledPlugins(It.IsAny<ISonarConfiguration>())).Returns(dictionaryplugins);
            rest.Setup(caller => caller.GetProperties(It.IsAny<ISonarConfiguration>(), It.IsAny<Resource>())).Returns(dictionaryprops);


            var conf = new ConnectionConfiguration("http://localhost:9000", "admin", "admin", 5.3);
            IAnalysisPlugin plugin = new AnalysisPlugin(notificaitonManager.Object, confManager.Object, rest.Object, helper.Object);

            var project = new Resource();
            project.Key = "Tekla.Tools.RoslynRunner";
            project.SolutionRoot = Path.Combine(execpath, "TestData");
            project.SolutionName = "RoslynRunner.sln";

            var profiledata = new Dictionary<string, Profile>();
            var profile = new Profile(null, null);
            profile.AddRule(new Rule { Repo = "roslyn-cs", Key = "SA140", IsParamsRetrivedFromServer = true, ConfigKey = "roslyn-cs:SA140" });
            profile.AddRule(new Rule { Repo = "roslyn-cs", Key = "SA1001", IsParamsRetrivedFromServer = true, ConfigKey = "roslyn-cs:SA1001" });
            profile.AddRule(new Rule { Repo = "roslyn-cs", Key = "SA1200", IsParamsRetrivedFromServer = true, ConfigKey = "roslyn-cs:SA1200" });
            profiledata.Add("cs", profile);

            plugin.AssociateProject(project, conf, profiledata, "14.0");

            var vsfile = new VsFileItem();
            var vsproject = new VsProjectItem();
            vsproject.ProjectName = "ClassLibrary1";
            vsproject.ProjectFilePath = Path.Combine(project.SolutionRoot, "ClassLibrary1", "ClassLibrary1.csproj");
            vsfile.Project = vsproject;
            vsfile.FilePath = Path.Combine(project.SolutionRoot, "ClassLibrary1", "Class1.cs");
            vsfile.FileName = "Class1.cs";
            vsfile.SonarResource = new Resource() { Key = "ProjectKey" };
            var issues = plugin.GetLocalAnalysisExtension(conf).ExecuteAnalysisOnFile(vsfile, project, conf, false);
            Assert.That(issues.Count, Is.EqualTo(5));
        }


        /// <summary>
        /// The test resource key.
        /// </summary>
        [Test]
        public void ShouldRunDiagnosticsIfSyncIsTrueAndSonarLintIsUsed()
        {
            var notificaitonManager = new Mock<INotificationManager>();
            var confManager = new Mock<IConfigurationHelper>();
            var helper = new Mock<IVsEnvironmentHelper>();
            var rest = new Mock<ISonarRestService>();
            var dictionaryprops = new Dictionary<string, string>();
            dictionaryprops.Add("sonar.roslyn.sync.type", "false");

            var dictionaryplugins = new Dictionary<string, string>();
            dictionaryplugins.Add("Roslyn", "1.0");

            rest.Setup(caller => caller.GetInstalledPlugins(It.IsAny<ISonarConfiguration>())).Returns(dictionaryplugins);
            rest.Setup(caller => caller.GetProperties(It.IsAny<ISonarConfiguration>(), It.IsAny<Resource>())).Returns(dictionaryprops);


            var conf = new ConnectionConfiguration("http://localhost:9000", "admin", "admin", 5.3);
            IAnalysisPlugin plugin = new AnalysisPlugin(notificaitonManager.Object, confManager.Object, rest.Object, helper.Object);

            var project = new Resource();
            project.Key = "Tekla.Tools.RoslynRunner";
            project.SolutionRoot = Path.Combine(execpath, "TestData");
            project.SolutionName = "RoslynRunner.sln";

            var profiledata = new Dictionary<string, Profile>();
            var profile = new Profile(null, null);
            profile.AddRule(new Rule { Repo = "roslyn-cs", Key = "SA140", IsParamsRetrivedFromServer = true, ConfigKey = "roslyn-cs:SA140" });
            profile.AddRule(new Rule { Repo = "roslyn-cs", Key = "SA1001", IsParamsRetrivedFromServer = true, ConfigKey = "roslyn-cs:SA1001" });
            profile.AddRule(new Rule { Repo = "roslyn-cs", Key = "SA1200", IsParamsRetrivedFromServer = true, ConfigKey = "roslyn-cs:SA1200" });
            profiledata.Add("cs", profile);

            plugin.AssociateProject(project, conf, profiledata, "14.0");

            var vsfile = new VsFileItem();
            var vsproject = new VsProjectItem();
            vsproject.ProjectName = "ClassLibrary1";
            vsproject.ProjectFilePath = Path.Combine(project.SolutionRoot, "ClassLibrary1", "ClassLibrary1.csproj");
            vsfile.Project = vsproject;
            vsfile.FilePath = Path.Combine(project.SolutionRoot, "ClassLibrary1", "Class1.cs");
            vsfile.FileName = "Class1.cs";
            vsfile.SonarResource = new Resource() { Key = "ProjectKey" };
            var issues = plugin.GetLocalAnalysisExtension(conf).ExecuteAnalysisOnFile(vsfile, project, conf, false);
            Assert.That(issues.Count, Is.EqualTo(5));
        }

        #endregion
    }
}