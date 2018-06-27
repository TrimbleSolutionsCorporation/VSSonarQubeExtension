using NUnit.Framework;
using CxxPlugin.LocalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using VSSonarPlugins;
using Moq;
using VSSonarPlugins.Types;

namespace CxxPlugin.LocalExtensions.Tests
{
    [TestFixture()]
    public class CxxLintSensorTests
    {
        string executionFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", ""));

        [Test()]
        public void CxxLintSensorTest()
        {

            var notificationService = new Mock<INotificationManager>();
            var configurationService = new Mock<IConfigurationHelper>();
            var sonarService = new Mock<ISonarConfiguration>();
            var restService = new Mock<ISonarRestService>();
            var vsService = new Mock<IVsEnvironmentHelper>();
            var lintSensor = new CxxLintSensor(notificationService.Object, configurationService.Object, restService.Object, vsService.Object);
            var profile = new Dictionary<string, Profile>();
            var project = new Resource();
            project.SolutionName = "Common.sln";
            project.SolutionRoot = @"D:\prod\structures\core\common";

            lintSensor.UpdateProfile(project, sonarService.Object, profile, "14.0");
            Assert.That(lintSensor.SolutionData.Projects.Count, Is.EqualTo(1));
        }
    }
}