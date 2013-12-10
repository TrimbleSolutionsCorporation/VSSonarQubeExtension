// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPluginsOptions.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections.Generic;
    using System.Windows.Controls;

    using ExtensionTypes;

    /// <summary>
    /// The PluginsOptions interface.
    /// </summary>
    public interface IPluginsOptions
    {
        /// <summary>
        /// The get user control options.
        /// </summary>
        /// <returns>
        /// The <see cref="UserControl"/>.
        /// </returns>
        UserControl GetUserControlOptions(Resource project);

        /// <summary>
        /// The get options.
        /// </summary>
        /// <returns>
        /// The <see>
        ///     <cref>Dictionary</cref>
        /// </see>
        ///     .
        /// </returns>
        Dictionary<string, string> GetOptions();

        /// <summary>
        /// The set options.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        void SetOptions(Dictionary<string, string> options);

        /// <summary>
        /// The is enabled.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool IsEnabled();

        /// <summary>
        /// The reset defaults.
        /// </summary>
        void ResetDefaults();
    }
}
