// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestPlugin.cs" company="Copyright © 2014 jmecsoftware">
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

namespace CxxPlugin.Test
{
    using System.Threading;
    using System.Windows;
    using SonarRestService.Types;
    using NUnit.Framework;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// The test server extension.
    /// </summary>
    [TestFixture]
    public class TestPlugin
    {
        /// <summary>
        /// The test resource key.
        /// </summary>
        [Test]
        public void TestPluginKey()
        {
            var plugin = new CxxPlugin();
            Assert.AreEqual("CxxPlugin", plugin.GetKey(new ConnectionConfiguration()));
        }

        /// <summary>
        /// The test resource key.
        /// </summary>
        [Test]
        public void TestLanguageIsSupported()
        {
            var plugin = new CxxPlugin();
            Assert.IsTrue(CxxPlugin.IsSupported("file.cpp"));
            Assert.IsTrue(CxxPlugin.IsSupported("file.h"));
            Assert.IsTrue(CxxPlugin.IsSupported("file.cc"));
            Assert.IsFalse(CxxPlugin.IsSupported("file.cs"));
        }
    }
}
