// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginException.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarPlugins
{
    using System;

    /// <summary>
    /// The vs sonar extension.
    /// </summary>
    public class VSSonarExtension : Exception
    {
    }

    /// <summary>
    /// The plugin exception.
    /// </summary>
    public class ResourceNotSupportedException : VSSonarExtension
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Resource is not supported by any of the installed plugins";
            }
        }        
    }

    /// <summary>
    /// The plugin exception.
    /// </summary>
    public class NoFileInViewException : VSSonarExtension
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        public override string Message
        {
            get
            {
                return "No File in View, please open a file in Editor";
            }
        }
    }

    /// <summary>
    /// The plugin exception.
    /// </summary>
    public class NoPluginInstalledException : VSSonarExtension
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        public override string Message
        {
            get
            {
                return "No plugin is Installed";
            }
        }
    }

    /// <summary>
    /// The plugin exception.
    /// </summary>
    public class MultiLanguageExceptionNotSupported : VSSonarExtension
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Multi Language Is Not Supported in this Version";
            }
        }
    }

    /// <summary>
    /// The plugin exception.
    /// </summary>
    public class ProjectNotAssociatedException : VSSonarExtension
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Project Not Associated";
            }
        }
    }
}