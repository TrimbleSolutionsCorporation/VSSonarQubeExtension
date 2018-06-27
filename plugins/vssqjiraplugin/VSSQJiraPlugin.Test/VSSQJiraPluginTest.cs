// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestTestTrackPlugin.cs" company="Copyright � 2015 jmecsoftware">
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

namespace VSSQJiraPlugin.Test
{
    using System.IO;
    using System.Reflection;

    using NUnit.Framework;
    using VSSonarPlugins;
    using Moq;
    using VSSQTestTrackPlugin;
    using VSSonarPlugins.Types;
    using JiraConnector;

    /// <summary>
    /// Tests the Git Plugin.
    /// </summary>
    [TestFixture]
    public class TestJiraPlugin
    {
        /// <summary>
        /// The execution path.
        /// </summary>
        private readonly string executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty));

        /// <summary>
        /// dummy test
        /// </summary>
        [Test]
        public void ParsesCommitMessagesProperly()
        {
        }
    }
}
