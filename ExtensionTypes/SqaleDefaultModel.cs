// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqaleDefaultModel.cs" company="">
//   
// </copyright>
// <summary>
//   The sqale default model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExtensionTypes
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