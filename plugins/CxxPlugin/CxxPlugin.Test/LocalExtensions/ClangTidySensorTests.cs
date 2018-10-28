using NUnit.Framework;
using CxxPlugin.LocalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSonarPlugins.Types;
using VSSonarPlugins;
using Moq;
using System.IO;
using SonarRestService.Types;
using SonarRestService;

namespace CxxPlugin.LocalExtensions.Tests
{
    [TestFixture()]
    public class ClangTidySensorTests
    {
        string executionFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", ""));

        [Test()]
        public void DumpBasicProfile()
        {
            var logPath = Path.Combine(executionFolder, "TestData", "clanglog.log");
            var serviceStub = new Mock<IVsEnvironmentHelper>();
            var serviceExcStub = new Mock<IVSSonarQubeCmdExecutor>();
            var notificationManager = new Mock<INotificationManager>();
            var configurationManager = new Mock<IConfigurationHelper>();
            var sonarConfig = new Mock<ISonarConfiguration>();
            var restCon = new Mock<ISonarRestService>();
            serviceStub.Setup(control => control.VsVersion()).Returns("14.0");
            serviceExcStub
                .Setup(control => control.GetStdOut())
                .Returns(File.ReadLines(logPath).ToList<string>());

            var linterClang = new ClangTidySensor(serviceStub.Object, configurationManager.Object, notificationManager.Object);
            var resource = new Resource();
            resource.SolutionRoot = executionFolder;
            var profiles = new Dictionary<string, Profile>();
            profiles.Add("c++", new Profile(restCon.Object, sonarConfig.Object));
            linterClang.UpdateProfile(resource, sonarConfig.Object, profiles, "vs15");
            Assert.That(File.Exists(Path.Combine(executionFolder, ".clang-tidy")), Is.True);
        }

        [Test()]
        public void ExecuteClangTidyTest()
        {
            var logPath = Path.Combine(executionFolder, "TestData", "clanglog.log");
            var serviceStub = new Mock<IVsEnvironmentHelper>();
            var serviceExcStub = new Mock<IVSSonarQubeCmdExecutor>();
            var notificationManager = new Mock<INotificationManager>();
            var configurationManager = new Mock<IConfigurationHelper>();
            serviceStub.Setup(control => control.VsVersion()).Returns("14.0");
            serviceExcStub
                .Setup(control => control.GetStdOut())
                .Returns(File.ReadLines(logPath).ToList<string>());
            serviceExcStub
                .Setup(control => control.GetStdError())
                .Returns(File.ReadLines(logPath).ToList<string>());

            configurationManager
                .Setup(control => control.ReadSetting(It.IsAny<Context>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new SonarQubeProperties() { Value = logPath });

            var linterClang = new ClangTidySensor(serviceStub.Object, configurationManager.Object, notificationManager.Object);
            var resource = new Resource();
            resource.Key = "project.name:common.cpp";
            resource.Path = @"d:\path\common.cpp";
            resource.SolutionRoot = @"D:\path";
            VsFileItem item = new VsFileItem();
            item.FilePath = @"d:\path\common.cpp";
            item.SonarResource = resource;
            var issues = linterClang.ExecuteClangTidy(item, serviceExcStub.Object, "");
            Assert.That(issues.Count, Is.EqualTo(3));
            Assert.That(issues.First().Explanation.Count, Is.EqualTo(65));
        }
    }
}