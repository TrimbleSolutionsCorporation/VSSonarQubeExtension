// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestViewModel.cs" company="Copyright © 2014 jmecsoftware">
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
    using global::CxxPlugin.Options;

    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// The comment on issue command test.
    /// </summary>
    [TestFixture]
    public class ConnectionConfigurationHelpersTests
    {
        /// <summary>
        /// The test window.
        /// </summary>
        [Test]
        public void TestExecutionOfCommand()
        {
            var serviceStub = new Mock<ICxxIoService>();
            var controller = new CxxOptionsController(serviceStub.Object);
            serviceStub.Setup(control => control.OpenFileDialog("Vera++ executable|vera++.exe")).Returns("Executable");
            serviceStub.Setup(control => control.OpenFileDialog("CppCheck executable|cppcheck.exe")).Returns("Executable");
            serviceStub.Setup(control => control.OpenFileDialog("Custom executable|*.exe")).Returns("Executable");
            serviceStub.Setup(control => control.OpenFileDialog("Rats executable|rats.exe")).Returns("Executable");
            controller.OpenCommand.Execute("Vera++");
            Assert.AreEqual("Executable", controller.VeraExecutable);
            controller.OpenCommand.Execute("CppCheck");
            Assert.AreEqual("Executable", controller.CppCheckExecutable);
            controller.OpenCommand.Execute("Rats");
            Assert.AreEqual("Executable", controller.RatsExecutable);
            controller.OpenCommand.Execute("ExternalSensor");
            Assert.AreEqual("Executable", controller.CustomExecutable);
        }
    }
}
