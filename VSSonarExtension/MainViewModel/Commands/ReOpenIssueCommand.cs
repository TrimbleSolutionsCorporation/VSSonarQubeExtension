// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReOpenIssueCommand.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.MainViewModel.Commands
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Net;
    using System.Windows;
    using System.Windows.Input;

    using ExtensionTypes;

    using SonarRestService;

    using VSSonarExtension.MainViewModel.ViewModel;

    /// <summary>
    /// The view options command.
    /// </summary>
    public class ReOpenIssueCommand : ICommand
    {
        /// <summary>
        /// The issues data model.
        /// </summary>
        private readonly ExtensionDataModel model;

        /// <summary>
        /// The service.
        /// </summary>
        private readonly ISonarRestService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReOpenIssueCommand"/> class.
        /// </summary>
        public ReOpenIssueCommand()
        {
            var handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReOpenIssueCommand"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="service">
        /// The service.
        /// </param>
        public ReOpenIssueCommand(ExtensionDataModel model, ISonarRestService service)
        {
            this.model = model;
            this.service = service;
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
            var list = parameter as IList;
            if (list != null)
            {
                var status = this.service.ReOpenIssues(this.model.UserConfiguration, new System.Collections.Generic.List<Issue>(list.Cast<Issue>().ToList()), this.model.CommentData);
                var allGood = true;
                foreach (var httpStatusCode in status)
                {
                    if (httpStatusCode.Value != HttpStatusCode.OK)
                    {
                        allGood = false;
                        MessageBox.Show("Cannot Modify Status of Issue" + httpStatusCode.Key);
                    }
                }

                if (allGood)
                {
                    this.model.CommentData = string.Empty;
                    this.model.RefreshIssuesInViews();
                }
            }
        }
    }
}
