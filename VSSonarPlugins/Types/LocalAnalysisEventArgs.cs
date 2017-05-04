// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalAnalysisEventArgs.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarPlugins.Types
{
    using System;
    using System.Collections.Generic;

    public class LocalAnalysisEventFileAnalsysisComplete : EventArgs
    {
        /// <summary>
        /// The ex.
        /// </summary>
        public readonly VsFileItem resource;

        /// <summary>
        /// The issues
        /// </summary>
        public readonly List<Issue> issues;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAnalysisEventFileAnalsysisComplete"/> class.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="issues">The issues.</param>
        public LocalAnalysisEventFileAnalsysisComplete(VsFileItem resource, List<Issue> issues)
        {
            this.resource = resource;
            this.issues = issues;
        }
    }

    public class LocalAnalysisEventCommandAnalsysisComplete : EventArgs
    {
        /// <summary>
        /// The ex.
        /// </summary>
        public readonly VsFileItem itemInView;

        /// <summary>
        /// The issues
        /// </summary>
        public readonly List<Issue> issues;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAnalysisEventCommandAnalsysisComplete"/> class.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="issues">The issues.</param>
        public LocalAnalysisEventCommandAnalsysisComplete(VsFileItem itemInView, List<Issue> issues)
        {
            this.itemInView = itemInView;
            this.issues = issues;
        }
    }

    public class LocalAnalysisEventFullAnalsysisComplete : EventArgs
    {
        /// <summary>
        /// The issues
        /// </summary>
        public readonly List<Issue> issues;

        public LocalAnalysisEventFullAnalsysisComplete(List<Issue> issues)
        {
            this.issues = issues;
        }
    }

    public class LocalAnalysisStdoutMessage : EventArgs
    {
        /// <summary>
        /// The ex.
        /// </summary>
        public readonly string message;

        public LocalAnalysisStdoutMessage(string message)
        {
            this.message = message;
        }
    }

    /// <summary>
    /// The model expectes this argument to be sent refering the plugin key
    /// </summary>
    public class LocalAnalysisExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// The error message.
        /// </summary>
        public readonly string ErrorMessage;

        /// <summary>
        /// The ex.
        /// </summary>
        public readonly Exception Ex;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAnalysisEventArgs"/> class.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="errorMessage">
        /// The error Message.
        /// </param>
        /// <param name="ex">
        /// The ex.
        /// </param>
        public LocalAnalysisExceptionEventArgs(string errorMessage, Exception ex)
        {
            this.Ex = ex;
            this.ErrorMessage = errorMessage;
        }
    }
}