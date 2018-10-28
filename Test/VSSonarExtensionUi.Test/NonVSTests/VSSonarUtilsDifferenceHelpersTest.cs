// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSSonarUtilsDifferenceHelpersTest.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The vs sonar utils callbacks test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtensionUi.Helpers.Test.NonVSTests
{
    using System;
    using System.IO;
    using System.Reflection;

    using NUnit.Framework;

    using VSSonarPlugins.Helpers;

    /// <summary>
    /// The vs sonar utils callbacks test.
    /// </summary>
    [TestFixture]
    public class VsSonarUtilsDifferenceHelpersTest
    {
        /// <summary>
        /// The sample data path.
        /// </summary>
        private readonly string sampleDataPath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString(), "NonVSTests\\TestData");

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
    }
}
