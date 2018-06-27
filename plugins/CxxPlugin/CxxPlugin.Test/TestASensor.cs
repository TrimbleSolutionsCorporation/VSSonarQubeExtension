// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestASensor.cs" company="Copyright © 2014 jmecsoftware">
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
    using System.Collections.Generic;

    using global::CxxPlugin.Commands;
    using global::CxxPlugin.LocalExtensions;
    using global::CxxPlugin.Options;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// The comment on issue command test.
    /// </summary>
    [TestFixture]
    public class TestASensor
    {
        /// <summary>
        /// The test window.
        /// </summary>
        [Test]
        public void TestRetriveProperties()
        {
            var map = new Dictionary<string,string>();
            map.Add("key", "value");
            Assert.AreEqual(1, map.Count);

        }
    }
}
