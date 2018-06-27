// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestLocalAnalysis.cs" company="Copyright © 2014 jmecsoftware">
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
    using System.IO;

    

    using global::CxxPlugin.Commands;
    using global::CxxPlugin.LocalExtensions;

    using Moq;

    using NUnit.Framework;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    /// <summary>
    /// The test server extension.
    /// </summary>
    [TestFixture]
    public class TestLocalAnalysis
    {
        /// <summary>
        /// The options.
        /// </summary>
        private Dictionary<string, string> options = new Dictionary<string, string>();

        /// <summary>
        /// The file to analyse.
        /// </summary>
        private string fileToAnalyse = "file.cpp";

        /// <summary>
        /// The set up.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.options.Add("CustomKey", "cpplint");
            this.options.Add("CustomArguments", "cpplint_mod.py --output=vs7");
            this.options.Add("CustomExecutable", "python.exe");
            this.options.Add("CustomEnvironment", string.Empty);            
            this.options.Add("CppCheckEnvironment", string.Empty);
            this.options.Add("CppCheckArguments", "--inline-suppr --enable=all --xml -D__cplusplus -DNT");
            this.options.Add("CppCheckExecutable", "cppcheck.exe");
            this.options.Add("RatsEnvironment", string.Empty);
            this.options.Add("RatsArguments", "-nodup -showrules");
            this.options.Add("RatsExecutable", "rats.exe");
            this.options.Add("VeraEnvironment", "VERA_ROOT=lib\\\vera++");
            this.options.Add("VeraArguments", "-nodup -showrules");
            this.options.Add("VeraExecutable", "vera++.exe");

            using (File.Create(this.fileToAnalyse))
            {
            }

            using (File.Create("cppcheck.exe"))
            {
            }

            using (File.Create("vera++.exe"))
            {
            }

            using (File.Create("rats.exe"))
            {
            }

            using (File.Create("python.exe"))
            {
            }
        }

        /// <summary>
        /// The tear down.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            File.Delete(this.fileToAnalyse);
            File.Delete("cppcheck.exe");
            File.Delete("vera++.exe");
            File.Delete("rats.exe");
            File.Delete("python.exe");
        }

        private void AnalysisCompleted(object sender, System.EventArgs e)
        {
            var args = e as LocalAnalysisExceptionEventArgs;
            Assert.IsNull(args.Ex);
        }
    }
}
