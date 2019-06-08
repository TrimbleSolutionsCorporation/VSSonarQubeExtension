// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestTestTrackPlugin.cs" company="Copyright � 2015 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------
namespace JiraPlugin.Test
{
    using System.IO;
    using System.Reflection;
    using NUnit.Framework;
    using VSSonarPlugins;
    using VSSQTestTrackPlugin;
    using SonarRestService.Types;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Moq;

    /// <summary>
    /// Tests the Git Plugin.
    /// </summary>
    [TestFixture]
    public class TestJiraPlugin
    {
        /// <summary>
        /// The execution path.
        /// </summary>
        private readonly string executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty));

        /// <summary>
        /// dummy test
        /// </summary>
        [Test]
        public void LoadsConfigurationFromFileCorrectly()
        {
            var mockLogger = new Mock<INotificationManager>();
            var mockConfiguration = new Mock<ISonarConfiguration>();

            var jira = new JiraPlugin(mockLogger.Object, mockConfiguration.Object);
            jira.LoadConfigurationFile(Path.Combine(executionPath, "JiraCfg.txt"), mockLogger.Object);
            Assert.That(jira.MandatoryFields.Count, Is.EqualTo(7));
            Assert.That(jira.MandatoryFields["UserName"], Is.EqualTo("user"));
            Assert.That(jira.MandatoryFields["Password"], Is.EqualTo("pass"));
            Assert.That(jira.MandatoryFields["JiraURL"], Is.EqualTo("jiraurl"));
            Assert.That(jira.MandatoryFields["UserNameToUseForIssueCreation"], Is.EqualTo("user"));            
            Assert.That(jira.MandatoryFields["JiraProject"], Is.EqualTo("ProjectAlias"));
            Assert.That(jira.MandatoryFields["IssueType"], Is.EqualTo("Technical Debt"));
            Assert.That(jira.MandatoryFields["Components"], Is.EqualTo("PD internal - Code Analysis"));

            Assert.That(jira.CustomFields.Count, Is.EqualTo(4));
            Assert.That(jira.CustomFields["Type"], Is.EqualTo("Technical debt"));            
            Assert.That(jira.CustomFields["Version Found"], Is.EqualTo("2020_Dev"));
            Assert.That(jira.CustomFields["Defective since"], Is.EqualTo("TS 2020"));
            Assert.That(jira.CustomFields["Origin"], Is.EqualTo("SonarQube"));
        }

        //[Test]
        public async Task CreateTicket()
        {
            var mockConfiguration = new Mock<ISonarConfiguration>();
            var mockLogger = new Mock<INotificationManager>();
            mockConfiguration.SetupGet(m => m.Hostname).Returns("Sonar");

            var jira = new JiraPlugin(mockLogger.Object, mockConfiguration.Object);
            jira.AssociateProject(new Resource() { Key = "Project" }, mockConfiguration.Object, "");
            var issues = new List<Issue>();
            issues.Add(new Issue() { Key = "asdas", Message = "asaasdas"});
            await jira.AttachToNewDefect(issues);
        }

        [Test]
        public async Task AttachTicket()
        {
            var mockConfiguration = new Mock<ISonarConfiguration>();
            var mockLogger = new Mock<INotificationManager>();
            mockConfiguration.SetupGet(m => m.Hostname).Returns("http://sonar");

            var jira = new JiraPlugin(mockLogger.Object, mockConfiguration.Object);
            jira.AssociateProject(new Resource() { Key = "Project" }, mockConfiguration.Object, "");
            var issues = new List<Issue>();
            issues.Add(new Issue() { Key = "asdas", Message = "asaasdas" });
            await jira.AttachToExistentDefect(issues, "TTSD-27421");
        }
    }
}
