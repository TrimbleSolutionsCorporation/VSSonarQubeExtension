// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReadWriteToApplicationDataTests.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    

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
    }
}
