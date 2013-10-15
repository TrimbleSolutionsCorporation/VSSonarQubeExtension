// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSSonarUtilsTest.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
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

namespace ExtensionHelpers.Test.NonVSTests
{
    using System;
    using System.IO;

    using ExtensionHelpers;

    using NUnit.Framework;

    /// <summary>
    /// The vs sonar utils test.
    /// </summary>
    [TestFixture]
    public class VsSonarUtilsTest
    {
        /// <summary>
        /// The file name.
        /// </summary>
        private readonly string fileName = Path.GetTempPath() + "\\.vssonar";

        /// <summary>
        /// The sample data path.
        /// </summary>
        private readonly string sampleDataPath = Path.Combine(Environment.CurrentDirectory, "NonVSTests\\TestData");

        /// <summary>
        /// The set up.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            if (File.Exists(this.fileName))
            {
                File.Delete(this.fileName);
            }
        }

        /// <summary>
        /// The tear down.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (File.Exists(this.fileName))
            {
                File.Delete(this.fileName);
            }
        }

        /// <summary>
        /// The test file does not exist.
        /// </summary>
        [Test]
        public void TestFileDoesNotExist()
        {
            VsSonarUtils.WriteDataToConfigurationFile("key", "data", this.fileName);
            string[] lines = File.ReadAllLines(this.fileName);
            Assert.AreEqual(lines.Length, 1);
            Assert.AreEqual(lines[0], "key=data");
        }

        /// <summary>
        /// The test file exist add new data.
        /// </summary>
        [Test]
        public void TestFileExistAddNewData()
        {
            VsSonarUtils.WriteDataToConfigurationFile("key", "data", this.fileName);
            string[] lines = File.ReadAllLines(this.fileName);
            Assert.AreEqual(lines.Length, 1);
            Assert.AreEqual(lines[0], "key=data");
            VsSonarUtils.WriteDataToConfigurationFile("key1", "data1", this.fileName);
            lines = File.ReadAllLines(this.fileName);
            Assert.AreEqual(lines.Length, 2);
            Assert.AreEqual(lines[0], "key=data");
            Assert.AreEqual(lines[1], "key1=data1");
        }

        /// <summary>
        /// The test file exist replace.
        /// </summary>
        [Test]
        public void TestFileExistReplace()
        {
            VsSonarUtils.WriteDataToConfigurationFile("key", "data", this.fileName);
            string[] lines = File.ReadAllLines(this.fileName);
            Assert.AreEqual(lines.Length, 1);
            Assert.AreEqual(lines[0], "key=data");
            VsSonarUtils.WriteDataToConfigurationFile("key", "data2", this.fileName);
            lines = File.ReadAllLines(this.fileName);
            Assert.AreEqual(lines.Length, 1);
            Assert.AreEqual(lines[0], "key=data2");
        }

        /// <summary>
        /// The test multiples data.
        /// </summary>
        [Test]
        public void TestMultiplesData()
        {
            VsSonarUtils.WriteDataToConfigurationFile("key", "data", this.fileName);
            VsSonarUtils.WriteDataToConfigurationFile("key1", "data1", this.fileName);
            VsSonarUtils.WriteDataToConfigurationFile("key2", "data2", this.fileName);
            string[] lines = File.ReadAllLines(this.fileName);
            Assert.AreEqual(lines.Length, 3);
            Assert.AreEqual(lines[0], "key=data");
            Assert.AreEqual(lines[1], "key1=data1");
            Assert.AreEqual(lines[2], "key2=data2");
        }

        /// <summary>
        /// The test read data.
        /// </summary>
        [Test]
        public void TestReadData()
        {
            VsSonarUtils.WriteDataToConfigurationFile("key", "data", this.fileName);
            VsSonarUtils.WriteDataToConfigurationFile("key1", "data1", this.fileName);
            VsSonarUtils.WriteDataToConfigurationFile("key2", "data2", this.fileName);
            Assert.AreEqual(VsSonarUtils.ReadPropertyFromFile("key", this.fileName), "data");
            Assert.AreEqual(VsSonarUtils.ReadPropertyFromFile("key1", this.fileName), "data1");
            Assert.AreEqual(VsSonarUtils.ReadPropertyFromFile("key2", this.fileName), "data2");
        }
        
        /// <summary>
        /// The test get resource from pom file.
        /// </summary>
        [Test]
        public void TestGetProjetKeyFromPomFile()
        {
            var sonrunfilepath = Path.Combine(this.sampleDataPath, "maven\\pom.xml");
            var sol = Path.GetDirectoryName(sonrunfilepath);
            Assert.AreEqual("my:project", VsSonarUtils.GetProjectKey(sol));
        }

        /// <summary>
        /// The test get resource from pom file.
        /// </summary>
        [Test]
        public void TestGetProjetKeyFromSonarRunnerFile()
        {
            var sonrunfilepath = Path.Combine(this.sampleDataPath, "sonar-runner\\sonar-project.properties");
            var sol = Path.GetDirectoryName(sonrunfilepath);
            Assert.AreEqual("my:project", VsSonarUtils.GetProjectKey(sol));
        }

        /// <summary>
        /// The test get resource from pom file.
        /// </summary>
        [Test]
        public void TestGetProjetKeyFromPomFileNonExistentFile()
        {
            var sonrunfilepath = Path.Combine(this.sampleDataPath, string.Empty);
            var sol = Path.GetDirectoryName(sonrunfilepath);
            Assert.AreEqual(string.Empty, VsSonarUtils.GetProjectKey(sol));
        }
    }
}
