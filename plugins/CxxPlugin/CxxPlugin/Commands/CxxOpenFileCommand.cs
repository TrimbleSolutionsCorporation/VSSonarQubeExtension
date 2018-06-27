// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxOpenFileCommand.cs" company="Copyright © 2014 jmecsoftware">
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

namespace CxxPlugin.Commands
{
    using System;
    using System.Windows.Input;

    using global::CxxPlugin.Options;

    /// <summary>
    /// The view options command.
    /// </summary>
    public class CxxOpenFileCommand : ICommand
    {
        /// <summary>
        /// The model.
        /// </summary>
        private readonly CxxOptionsController model;

        /// <summary>
        /// The service.
        /// </summary>
        private readonly ICxxIoService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="CxxOpenFileCommand"/> class. 
        /// </summary>
        public CxxOpenFileCommand()
        {
            var handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CxxOpenFileCommand"/> class. 
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="ioservice">
        /// The service.
        /// </param>
        public CxxOpenFileCommand(CxxOptionsController model, ICxxIoService ioservice)
        {
            this.model = model;
            this.service = ioservice;
        }

        /// <summary>
        /// The can execute changed.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// The can execute.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool CanExecute(object parameter)
        {            
            return true;
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        public void Execute(object parameter)
        {
            var optionsTab = parameter as string;
            if (optionsTab == null)
            {
                return;
            }

            if (optionsTab.Equals("CxxLint"))
            {
                var data = this.service.OpenFileDialog("CxxLint executable|*.jar");
                this.model.CxxLint = data;
            }

            if (optionsTab.Equals("Vera++"))
            {
                var data = this.service.OpenFileDialog("Vera++ executable|vera++.exe");
                this.model.VeraExecutable = data;
            }

            if (optionsTab.Equals("CppCheck"))
            {
                var data = this.service.OpenFileDialog("CppCheck executable|cppcheck.exe");
                this.model.CppCheckExecutable = data;
            }

            if(optionsTab.Equals("ClangTidy"))
            {
                var data = this.service.OpenFileDialog("ClangTidy executable|clang-tidy.exe");
                this.model.ClangTidyExecutable = data;
            }

            if (optionsTab.Equals("Rats"))
            {
                var data = this.service.OpenFileDialog("Rats executable|rats.exe");
                this.model.RatsExecutable = data;
            }

            if (optionsTab.Equals("PcLint"))
            {
                var data = this.service.OpenFileDialog("PcLint executable|*.exe");
                this.model.PcLintExecutable = data;
            }

            if (optionsTab.Equals("ExternalSensor"))
            {
                var data = this.service.OpenFileDialog("Custom executable|*.exe");
                this.model.CustomExecutable = data;
            }
        }
    }
}
