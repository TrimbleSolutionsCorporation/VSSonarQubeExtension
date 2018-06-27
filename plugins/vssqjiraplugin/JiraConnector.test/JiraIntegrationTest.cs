using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using JiraConnector;

namespace JiraConnector.test
{
    public class JiraIntegrationTest
    {
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
              //  var connectionMock = new Mock<JiraConnector>());
              //  connectionMock.Setup(con => con.IsConnected).Returns(true);
              //  connectionMock.Setup(con => con.CreateDefect(It.IsAny<CDefect>())).Returns(12321);
              //  var plugin = new TestTrackConnector("user", "password", true, connectionMock.Object);
             //   Assert.That(plugin.CreateDefect("Git Training - Test VSSonarQube Extension", "Some Comment"), Is.EqualTo(12321));
            }
        }
        }
}
