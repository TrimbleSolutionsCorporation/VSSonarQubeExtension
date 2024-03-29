﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotCoveredFormatDefinition.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarQubeExtension.Coverage
{
    using System.ComponentModel.Composition;
    using System.Windows.Media;

    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Utilities;

    /// <summary>
    /// The not covered format definition.
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [Name("LineNotCovered")]
    [UserVisible(true)]
    internal class NotCoveredFormatDefinition : MarkerFormatDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotCoveredFormatDefinition"/> class.
        /// </summary>
        public NotCoveredFormatDefinition()
        {
            this.BackgroundColor = Colors.Red;
            this.DisplayName = "Line Not Covered";
            this.ZOrder = 5;
        }
    }
}