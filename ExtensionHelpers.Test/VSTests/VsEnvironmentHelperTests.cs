// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VsEnvironmentHelperTests.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace ExtensionHelpers.Test.VSTests
{
    using NUnit.Framework;

    /// <summary>
    /// The vs sonar utils callbacks test.
    /// </summary>
    [TestFixture]
    public class VsEnvironmentHelperTests
    {
        /// <summary>
        /// The get this file from project.
        /// </summary>
        [Test]
        public void NoExcpetionsThrownTests()
        {
            var vshelper = new VsPropertiesHelper(null, null);
            Assert.IsNullOrEmpty(vshelper.ActiveFileFullPath());
            Assert.IsNullOrEmpty(vshelper.ActiveProjectFileFullPath());
            Assert.IsNullOrEmpty(vshelper.ActiveProjectName());
            Assert.IsNullOrEmpty(vshelper.ActiveSolutionPath());
            Assert.IsNullOrEmpty(vshelper.CurrentSelectedDocumentLanguage());
            Assert.IsNullOrEmpty(vshelper.ReadSavedOption("sdsa", "sds", "dss"));
            vshelper.NavigateToResource("sdsds");
            vshelper.OpenResourceInVisualStudio("", "sdsds", 1);
            vshelper.WriteDefaultOption("sdsds", "sdsas", "sdasd", "sdss");
            vshelper.WriteOption("sds", "sds", "sds", "sds");
        }
    }
}
