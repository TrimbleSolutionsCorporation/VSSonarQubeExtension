// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestTrackIntegrationTest.cs" company="Copyright © 2015 jmecsoftware">
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

namespace TestTrackIntegration.Test
{
    using Moq;
    using NUnit.Framework;
    using System.IO;
    using System.Reflection;
    using TestTrackConnector;

    /// <summary>
    /// Tests the Git Plugin.
    /// </summary>
    [TestFixture]
    public class TestSQGitPlugin
    {
        /// <summary>#Filter:3
        /// The execution path.
        /// </summary>
        private readonly string executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty));

        /// <summary>
        /// Correctlies the get branch.
        /// </summary>
        [Test]
        public void CorrectlyCreatesItem()
        {
            var connectionMock = new Mock<IIssueManagementConnection>();
            connectionMock.Setup(con => con.IsConnected).Returns(true);
            connectionMock.Setup(con => con.CreateDefect(It.IsAny<CDefect>())).Returns(12321);
            var plugin = new TestTrackConnector("user", "password", true, connectionMock.Object);
            Assert.That(plugin.CreateDefect("Git Training - Test VSSonarQube Extension", "Some Comment"), Is.EqualTo(12321));
        }

        [Test]
        public void FailsIfNotConnected()
        {
            var connectionMock = new Mock<IIssueManagementConnection>();
            connectionMock.Setup(con => con.IsConnected).Returns(false);
            connectionMock.Setup(con => con.CreateDefect(It.IsAny<CDefect>())).Returns(12321);
            var plugin = new TestTrackConnector("user", "password", true, connectionMock.Object);
            Assert.That(plugin.CreateDefect("Git Training - Test VSSonarQube Extension", "Some Comment"), Is.EqualTo(-1));
        }

        [Test]
        public void CorrectlyAttachesItem()
        {
            var connectionMock = new Mock<IIssueManagementConnection>();
            connectionMock.Setup(con => con.IsConnected).Returns(true);
            connectionMock.Setup(con => con.AttachComment(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            var plugin = new TestTrackConnector("user", "password", true, connectionMock.Object);
            Assert.That(plugin.AttachCommentToTestTrackItem(11202, "Some Comment"), Is.True);
        }

        [Test]
        public void FailsIfNotConnectedWhenAttachesItem()
        {
            var connectionMock = new Mock<IIssueManagementConnection>();
            connectionMock.Setup(con => con.IsConnected).Returns(false);
            connectionMock.Setup(con => con.AttachComment(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            var plugin = new TestTrackConnector("user", "password", true, connectionMock.Object);
            Assert.That(plugin.AttachCommentToTestTrackItem(11202, "Some Comment"), Is.False);
        }

        [Test]
        public void CorrectlyCreatesConfigurationFromFile()
        {
            var connectionMock = new Mock<IIssueManagementConnection>();
            var defect = new TtDefect("user", "version", "summary", "comment", Path.Combine(executionPath, "TesttrackSetup.cfg"), true);
            Assert.That(defect.GetDefect().type, Is.EqualTo("Incorrect functionality"));
            Assert.That(defect.GetDefect().product, Is.EqualTo("Product Data"));
            Assert.That(defect.GetDefect().severity, Is.EqualTo("Automated Testing"));
            Assert.That(defect.GetDefect().state, Is.EqualTo("Open"));
            Assert.That(defect.GetDefect().disposition, Is.EqualTo("Team"));
            Assert.That(defect.GetDefect().customFieldList.Length, Is.EqualTo(3));

            Assert.That((defect.GetDefect().customFieldList[0] as CStringField).name, Is.EqualTo("CustomStringFied"));
            Assert.That((defect.GetDefect().customFieldList[0] as CStringField).value, Is.EqualTo(string.Empty));

            Assert.That((defect.GetDefect().customFieldList[1] as CIntegerField).name, Is.EqualTo("CustomIntFied"));
            Assert.That((defect.GetDefect().customFieldList[1] as CIntegerField).value, Is.EqualTo(1));


            Assert.That((defect.GetDefect().customFieldList[2] as CDropdownField).name, Is.EqualTo("Defective since v."));
            Assert.That((defect.GetDefect().customFieldList[2] as CDropdownField).value, Is.EqualTo("Work"));
        }
    }
}
