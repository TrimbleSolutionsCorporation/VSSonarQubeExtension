// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageTest.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The package test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtension.Test
{
    using Microsoft.VisualStudio.Shell.Interop;

    using NUnit.Framework;

    using VSSonarQubeExtension;

    /// <summary>
    /// The package test.
    /// </summary>
    [TestFixture]
    public class PackageTest
    {
        /// <summary>
        /// The is i vs package.
        /// </summary>
        [Test]
        public void IsIVsPackage()
        {
            var package = new VsSonarExtensionPackage();
            Assert.IsNotNull(package, "The object does not implement IVsPackage");
        }

        /// <summary>
        /// The set site.
        /// </summary>
        [Test]
        public void SetSite()
        {
            // Create the package
            var package = new VsSonarExtensionPackage() as IVsPackage;
            Assert.IsNotNull(package, "The object does not implement IVsPackage");

            // Unsite the package
            Assert.AreEqual(0, package.SetSite(null), "SetSite(null) did not return S_OK");
        }
    }
}
