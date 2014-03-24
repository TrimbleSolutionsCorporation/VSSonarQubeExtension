// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReadWriteToApplicationDataTests.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections.Generic;
    using System.IO;

    using ExtensionTypes;

    using NUnit.Framework;

    /// <summary>
    /// The write to application data tests.
    /// </summary>
    [TestFixture]
    public class ReadWriteToApplicationDataTests
    {
        /// <summary>
        /// The file name.
        /// </summary>
        private const string FileName = "file.cfg";

        /// <summary>
        /// The set up.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);             
            }
        }

        /// <summary>
        /// The test write of option to file.
        /// </summary>
        [Test]
        public void TestReadAndWriteOfOptionToANonExistentFile()
        {
            var vshelper = new VsPropertiesHelper(null) { ApplicationDataUserSettingsFile = FileName };
            vshelper.WriteOptionInApplicationData("key", "data", "value");
            var value = vshelper.ReadOptionFromApplicationData("key", "data");
            Assert.AreEqual("value", value);
        }

        /// <summary>
        /// The test write of option to file.
        /// </summary>
        [Test]
        public void TestReadAndWriteOfComplexOptionToANonExistentFile()
        {
            var vshelper = new VsPropertiesHelper(null) { ApplicationDataUserSettingsFile = FileName };
            vshelper.WriteOptionInApplicationData("key", "data", "value=xpto");
            var value = vshelper.ReadOptionFromApplicationData("key", "data");
            Assert.AreEqual("value=xpto", value);
        }

        /// <summary>
        /// The test write of option to file.
        /// </summary>
        [Test]
        public void TestReadAndWriteOfOptionToAExistentFile()
        {
            var vshelper = new VsPropertiesHelper(null) { ApplicationDataUserSettingsFile = FileName };
            vshelper.WriteOptionInApplicationData("key", "data", "value");
            var value = vshelper.ReadOptionFromApplicationData("key", "data");
            Assert.AreEqual("value", value);

            vshelper.WriteOptionInApplicationData("key", "data2", "value2");
            Assert.AreEqual("value2", vshelper.ReadOptionFromApplicationData("key", "data2"));
        }

        /// <summary>
        /// The test write of option to file.
        /// </summary>
        [Test]
        public void TestReadAndWriteOfOptionReplaceValueInFile()
        {
            var vshelper = new VsPropertiesHelper(null) { ApplicationDataUserSettingsFile = FileName };
            vshelper.WriteOptionInApplicationData("key", "data", "value");
            var value = vshelper.ReadOptionFromApplicationData("key", "data");
            Assert.AreEqual("value", value);

            vshelper.WriteOptionInApplicationData("key 1", "data", string.Empty);
            Assert.AreEqual(string.Empty, vshelper.ReadOptionFromApplicationData("key 1", "data"));

            vshelper.WriteOptionInApplicationData("key 1", "data", "value3");
            Assert.AreEqual("value3", vshelper.ReadOptionFromApplicationData("key 1", "data"));
        }

        /// <summary>
        /// The test write of option to file.
        /// </summary>
        [Test]
        public void TestReadAllOptionsReplaceValueInFile()
        {
            var vshelper = new VsPropertiesHelper(null) { ApplicationDataUserSettingsFile = FileName };
            vshelper.WriteOptionInApplicationData("key", "data", "value");
            vshelper.WriteOptionInApplicationData("key", "data1", "value1");
            vshelper.WriteOptionInApplicationData("key", "data2", "value2");
            vshelper.WriteOptionInApplicationData("key1", "data3", "value3");

            var value = vshelper.ReadAllAvailableOptionsInSettings("key");
            Assert.AreEqual(3, value.Count);
            Assert.AreEqual("value", value["data"]);
            Assert.AreEqual("value1", value["data1"]);
            Assert.AreEqual("value2", value["data2"]);
        }

        /// <summary>
        /// The test write of option to file.
        /// </summary>
        [Test]
        public void TestSetAllOptions()
        {
            var vshelper = new VsPropertiesHelper(null) { ApplicationDataUserSettingsFile = FileName };
            var options = new Dictionary<string, string>
                              {
                                  { "data", "value" },
                                  { "data1", "value1" },
                                  { "data2", "value2" },
                                  { "data3", "value3" }
                              };
            vshelper.WriteAllOptionsForPluginOptionInApplicationData("key", new Resource(), options);
            var value = vshelper.ReadAllAvailableOptionsInSettings("key");
            Assert.AreEqual(4, value.Count);
            Assert.AreEqual("value", value["data"]);
            Assert.AreEqual("value1", value["data1"]);
            Assert.AreEqual("value2", value["data2"]);
            Assert.AreEqual("value3", value["data3"]);
        }

        /// <summary>
        /// The test write of option to file.
        /// </summary>
        [Test]
        public void TestReadAllOptionsWithMultipleDefinedKeysInFile()
        {
            var vshelper = new VsPropertiesHelper(null) { ApplicationDataUserSettingsFile = FileName };

            using (var writer = new StreamWriter(FileName))
            {
                writer.Write("key=data,value\r\n");
                writer.Write("key=data,value1\r\n");
                writer.Write("key=data,value2\r\n");
            }

            var value = vshelper.ReadAllAvailableOptionsInSettings("key");
            Assert.AreEqual(1, value.Count);
            Assert.AreEqual("value", value["data"]);
        }
    }
}
