// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSSonarUtilsDifferenceHelpersTest.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections.Generic;
    using System.IO;
    using ExtensionHelpers;
    using ExtensionTypes;

    using NUnit.Framework;

    /// <summary>
    /// The vs sonar utils callbacks test.
    /// </summary>
    [TestFixture]
    public class VsSonarUtilsDifferenceHelpersTest
    {
        /// <summary>
        /// The sample data path.
        /// </summary>
        private readonly string sampleDataPath = Path.Combine(Environment.CurrentDirectory, "NonVSTests\\TestData");

        /// <summary>
        /// The setup.
        /// </summary>
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// The test method 1.
        /// </summary>
        [Test]
        public void TestCompare()
        {
            var source = Path.Combine(this.sampleDataPath, "SonarSource.txt");
            var destination = Path.Combine(this.sampleDataPath, "LocalSource.txt");
            var rep = VsSonarUtils.GetSourceDiffFromStrings(File.ReadAllText(source), File.ReadAllText(destination));
            Assert.AreEqual(9, rep.Count);
        }

        /// <summary>
        /// The test convert open issues to local source.
        /// </summary>
        [Test]
        public void TestConvertOpenIssuesToLocalSourceSomeLinesAdded()
        {
            var source = Path.Combine(this.sampleDataPath, "OriginalFileTest.txt");
            var destination = Path.Combine(this.sampleDataPath, "ModifyFileTest.txt");
            var rep = VsSonarUtils.GetSourceDiffFromStrings(File.ReadAllText(source), File.ReadAllText(destination));

            var issueList = new List<Issue>
                                {
                                    new Issue { Line = 8, Status = "OPEN" },
                                    new Issue { Line = 2, Status = "OPEN" },
                                    new Issue { Line = 2, Status = "CLOSE" }
                                };

            //var convertedIssue = VsSonarUtils.ConvertOpenIssuesToLocalSource(issueList, rep);
            //Assert.AreEqual(2, convertedIssue.Count);
            //Assert.AreEqual(2, convertedIssue[0].Line);
            //Assert.AreEqual(24, convertedIssue[1].Line);
        }
    }
}
