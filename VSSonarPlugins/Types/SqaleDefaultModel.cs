// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqaleDefaultModel.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Collections.Generic;

    /// <summary>
    /// The sqale default model.
    /// </summary>
    public static class SqaleDefaultModel
    {
        #region Static Fields

        /// <summary>
        /// The model.
        /// </summary>
        private static readonly List<Characteristic> model = new List<Characteristic>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="SqaleDefaultModel" /> class.
        /// </summary>
        static SqaleDefaultModel()
        {
            Model.Add(GetPortabilityChar());
            Model.Add(GetMaintainabilityChar());
            Model.Add(GetSecurityChar());
            Model.Add(GetEfficiencyChar());
            Model.Add(GetChangeabilityChar());
            Model.Add(GetReliabilityChar());
            Model.Add(GetTestabilityChar());
            Model.Add(GetReusabilityChar());
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        public static List<Characteristic> Model
        {
            get
            {
                return model;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get changeability char.
        /// </summary>
        /// <returns>
        /// The <see cref="Characteristic"/>.
        /// </returns>
        private static Characteristic GetChangeabilityChar()
        {
            var newchar = new Characteristic(Category.CHANGEABILITY, "Changeability");
            newchar.CreateSubChar(SubCategory.ARCHITECTURE_CHANGEABILITY, "Architecture related changeability");
            newchar.CreateSubChar(SubCategory.DATA_CHANGEABILITY, "Data related changeability");
            newchar.CreateSubChar(SubCategory.LOGIC_CHANGEABILITY, "Logic related changeability");
            return newchar;
        }

        /// <summary>
        /// The get efficiency char.
        /// </summary>
        /// <returns>
        /// The <see cref="Characteristic"/>.
        /// </returns>
        private static Characteristic GetEfficiencyChar()
        {
            var newchar = new Characteristic(Category.EFFICIENCY, "Efficiency");
            newchar.CreateSubChar(SubCategory.MEMORY_EFFICIENCY, "Memory use");
            newchar.CreateSubChar(SubCategory.CPU_EFFICIENCY, "Processor use");
            return newchar;
        }

        /// <summary>
        /// The get maintainability char.
        /// </summary>
        /// <returns>
        /// The <see cref="Characteristic"/>.
        /// </returns>
        private static Characteristic GetMaintainabilityChar()
        {
            var newchar = new Characteristic(Category.MAINTAINABILITY, "Maintainability");
            newchar.CreateSubChar(SubCategory.READABILITY, "Readability");
            newchar.CreateSubChar(SubCategory.UNDERSTANDABILITY, "Understandability");
            return newchar;
        }

        /// <summary>
        /// The get portability char.
        /// </summary>
        /// <returns>
        /// The <see cref="Characteristic"/>.
        /// </returns>
        private static Characteristic GetPortabilityChar()
        {
            var newchar = new Characteristic(Category.PORTABILITY, "Portability");
            newchar.CreateSubChar(SubCategory.COMPILER_RELATED_PORTABILITY, "Compiler related portability");
            newchar.CreateSubChar(SubCategory.HARDWARE_RELATED_PORTABILITY, "Hardware related portability");
            newchar.CreateSubChar(SubCategory.LANGUAGE_RELATED_PORTABILITY, "Language related portability");
            newchar.CreateSubChar(SubCategory.OS_RELATED_PORTABILITY, "OS related portability");
            newchar.CreateSubChar(SubCategory.SOFTWARE_RELATED_PORTABILITY, "Software related portability");
            newchar.CreateSubChar(SubCategory.TIME_ZONE_RELATED_PORTABILITY, "Time zone related portability");
            return newchar;
        }

        /// <summary>
        /// The get reliability char.
        /// </summary>
        /// <returns>
        /// The <see cref="Characteristic"/>.
        /// </returns>
        private static Characteristic GetReliabilityChar()
        {
            var newchar = new Characteristic(Category.RELIABILITY, "Reliability");
            newchar.CreateSubChar(SubCategory.ARCHITECTURE_RELIABILITY, "Architecture related reliability");
            newchar.CreateSubChar(SubCategory.DATA_RELIABILITY, "Data related reliability");
            newchar.CreateSubChar(SubCategory.EXCEPTION_HANDLING, "Exception handling");
            newchar.CreateSubChar(SubCategory.FAULT_TOLERANCE, "Fault tolerance");
            newchar.CreateSubChar(SubCategory.INSTRUCTION_RELIABILITY, "Instruction related reliability");
            newchar.CreateSubChar(SubCategory.LOGIC_RELIABILITY, "Logic related reliability");
            newchar.CreateSubChar(SubCategory.SYNCHRONIZATION_RELIABILITY, "Synchronization related reliability");
            newchar.CreateSubChar(SubCategory.UNIT_TESTS, "Unit tests");
            return newchar;
        }

        /// <summary>
        /// The get reusability char.
        /// </summary>
        /// <returns>
        /// The <see cref="Characteristic"/>.
        /// </returns>
        private static Characteristic GetReusabilityChar()
        {
            var newchar = new Characteristic(Category.REUSABILITY, "Reusability");
            newchar.CreateSubChar(SubCategory.MODULARITY, "Modularity");
            newchar.CreateSubChar(SubCategory.TRANSPORTABILITY, "Transportability");
            return newchar;
        }

        /// <summary>
        /// The get security char.
        /// </summary>
        /// <returns>
        /// The <see cref="Characteristic"/>.
        /// </returns>
        private static Characteristic GetSecurityChar()
        {
            var newchar = new Characteristic(Category.SECURITY, "Security");
            newchar.CreateSubChar(SubCategory.API_ABUSE, "API abuse");
            newchar.CreateSubChar(SubCategory.ERRORS, "Errors");
            newchar.CreateSubChar(SubCategory.INPUT_VALIDATION_AND_REPRESENTATION, "Input validation and representation");
            newchar.CreateSubChar(SubCategory.SECURITY_FEATURES, "Security features");
            return newchar;
        }

        /// <summary>
        /// The get testability char.
        /// </summary>
        /// <returns>
        /// The <see cref="Characteristic"/>.
        /// </returns>
        private static Characteristic GetTestabilityChar()
        {
            var newchar = new Characteristic(Category.TESTABILITY, "Testability");
            newchar.CreateSubChar(SubCategory.INTEGRATION_TESTABILITY, "Integration level testability");
            newchar.CreateSubChar(SubCategory.UNIT_TESTABILITY, "Unit level testability");
            return newchar;
        }

        #endregion
    }
}