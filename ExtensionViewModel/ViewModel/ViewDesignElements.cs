// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewDesignElements.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace ExtensionViewModel.ViewModel
{
    using System.Windows;

    /// <summary>
    /// The extension data model.
    /// </summary>
    public partial class ExtensionDataModel
    {
        /// <summary>
        ///     The assign enabled.
        /// </summary>
        private Visibility assignVisible = Visibility.Hidden;

        /// <summary>
        ///     The commenting enabled.
        /// </summary>
        private Visibility commentingVisible = Visibility.Hidden;

        /// <summary>
        ///     The confirm enable.
        /// </summary>
        private Visibility confirmVisible = Visibility.Hidden;

        /// <summary>
        ///     The false positive enabled.
        /// </summary>
        private Visibility falsePositiveVisible = Visibility.Hidden;

        /// <summary>
        ///     The open externally.
        /// </summary>
        private Visibility openExternallyVisible = Visibility.Hidden;

        /// <summary>
        ///     The reopen enabled.
        /// </summary>
        private Visibility reopenVisible = Visibility.Hidden;

        /// <summary>
        ///     The resolve enabled.
        /// </summary>
        private Visibility resolveVisible = Visibility.Hidden;

        /// <summary>
        ///     The un confirm enable.
        /// </summary>
        private Visibility unconfirmVisible = Visibility.Hidden;

        /// <summary>
        ///     The user controls height.
        /// </summary>
        private GridLength userControlsHeight;

        /// <summary>
        ///     The user input text box visibility.
        /// </summary>
        private Visibility userInputTextBoxVisibility = Visibility.Hidden;

        /// <summary>
        ///     The user controls height.
        /// </summary>
        private GridLength userTextControlsHeight;

        /// <summary>
        /// The comments width.
        /// </summary>
        private GridLength commentsWidth;
    }
}
