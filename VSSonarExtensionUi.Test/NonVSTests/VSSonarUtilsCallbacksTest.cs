// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VSSonarUtilsCallbacksTest.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarExtensionUi.Helpers.Test.NonVSTests
{
    using System;
    using System.Collections;
    using System.IO;

    using DifferenceEngine;

    using NUnit.Framework;

    using VSSonarPlugins.Helpers;

    using System.Reflection;

    /// <summary>
    /// The vs sonar utils callbacks test.
    /// </summary>
    [TestFixture]
    public class VsSonarUtilsCallbacksTest
    {
        /// <summary>
        /// The sample data path.
        /// </summary>
        private readonly string sampleDataPath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString(), "NonVSTests\\TestData");

        /// <summary>
        /// The s lf.
        /// </summary>
        private DiffListTextFile sLf;

        /// <summary>
        /// The d lf.
        /// </summary>
        private DiffListTextFile dLf;

        /// <summary>
        /// The de.
        /// </summary>
        private DiffEngine de;

        /// <summary>
        /// The rep.
        /// </summary>
        private ArrayList rep;

        /// <summary>
        /// The setup.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            var source = Path.Combine(this.sampleDataPath, "SonarSource.txt");
            var destination = Path.Combine(this.sampleDataPath, "LocalSource.txt");

            this.sLf = new DiffListTextFile(source);
            this.dLf = new DiffListTextFile(destination);
            this.de = new DiffEngine();
            this.de.ProcessDiff(this.sLf, this.dLf, DiffEngineLevel.SlowPerfect);
            this.rep = this.de.DiffReport();
        }

        /// <summary>
        /// The test method 1.
        /// </summary>
        [Test]
        public void TestSimpleCompare()
        {
            //Results win = new Results(sLf, dLf, rep, 0);
            // win.ShowDialog();
            Assert.AreEqual(9, this.rep.Count);
        }

        /// <summary>
        /// The should report violation in same line.
        /// </summary>
        [Test]
        public void ShouldReportViolationInSameLine()
        {
            Assert.AreEqual(1, VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(1, this.rep));
            Assert.AreEqual(2, VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(2, this.rep));
            Assert.AreEqual(3, VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(3, this.rep));
            Assert.AreEqual(7, VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(7, this.rep));
            Assert.AreEqual(8, VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(8, this.rep));
        }

        /// <summary>
        /// The should report violation in same line.
        /// </summary>
        [Test]
        public void GetCorrespondentChangeForLineNoChange()
        {
            var diffentry = VsSonarUtils.GetChangeForLine(3, this.rep);
            Assert.AreEqual(0, diffentry.SourceIndex);
            Assert.AreEqual(0, diffentry.DestIndex);
            Assert.AreEqual(DiffResultSpanStatus.NoChange, diffentry.Status);

            var diffentry1 = VsSonarUtils.GetChangeForLine(4, this.rep);
            Assert.AreEqual(3, diffentry1.SourceIndex);
            Assert.AreEqual(4, diffentry1.DestIndex);
            Assert.AreEqual(DiffResultSpanStatus.NoChange, diffentry1.Status);

            var diffentry2 = VsSonarUtils.GetChangeForLine(7, this.rep);
            Assert.AreEqual(6, diffentry2.SourceIndex);
            Assert.AreEqual(6, diffentry2.DestIndex);
            Assert.AreEqual(DiffResultSpanStatus.NoChange, diffentry2.Status);

            var diffentry3 = VsSonarUtils.GetChangeForLine(10, this.rep);
            Assert.AreEqual(9, diffentry3.SourceIndex);
            Assert.AreEqual(9, diffentry3.DestIndex);
            Assert.AreEqual(DiffResultSpanStatus.NoChange, diffentry3.Status);
        }

        /// <summary>
        /// The get correspondent change for line for moved line.
        /// </summary>
        [Test]
        public void GetNullChangeForLineForDeletedLine()
        {
            var diffentry = VsSonarUtils.GetChangeForLine(6, this.rep);
            Assert.IsNull(diffentry);
        }

        /// <summary>
        /// The get correspondent change for line for moved line.
        /// </summary>
        [Test]
        public void GetNullChangeForLineForModifiedLine()
        {
            var diffentry = VsSonarUtils.GetChangeForLine(9, this.rep);
            Assert.IsNull(diffentry);
        }

        /// <summary>
        /// The should report violation in same line.
        /// </summary>
        [Test]
        public void ShouldReportViolationInTheSameLine()
        {
            Assert.AreEqual(2, VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(2, this.rep));
        }

        /// <summary>
        /// The should report violation in same line.
        /// </summary>
        [Test]
        public void ShouldReportViolationInNextLine()
        {
            Assert.AreEqual(5, VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(4, this.rep));
        }

        /// <summary>
        /// The should report violation in same line.
        /// </summary>
        [Test]
        public void ShouldReportViolationInLineWithCorrectNumberOfAddedLines()
        {
            Assert.AreEqual(14, VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(11, this.rep));
            Assert.AreEqual(15, VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(12, this.rep));
        }

        /// <summary>
        /// The should report violation in same line.
        /// </summary>
        [Test]
        public void ShouldReportReturnInvalidIndex()
        {
            Assert.AreEqual(-1, VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(6, this.rep));
        }
    }
}
