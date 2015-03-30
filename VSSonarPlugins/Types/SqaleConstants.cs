// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqaleConstants.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.ComponentModel;

    /// <summary>
    /// The resolution.
    /// </summary>
    public enum Resolution
    {
        /// <summary>
        /// The fals e_ positive.
        /// </summary>
        [Description("FALSE-POSITIVE")]
        FALSE_POSITIVE = 0, 

        /// <summary>
        /// The fixed.
        /// </summary>
        [Description("FIXED")]
        FIXED = 2, 

        /// <summary>
        /// The removed.
        /// </summary>
        [Description("REMOVED")]
        REMOVED = 3, 

        /// <summary>
        ///     The linear.
        /// </summary>
        [Description("UNDEFINED")]
        UNDEFINED = 4, 
    }

    /// <summary>
    /// The issue status.
    /// </summary>
    public enum IssueStatus
    {
        /// <summary>
        ///     The linear.
        /// </summary>
        [Description("CLOSED")]
        CLOSED = 0, 

        /// <summary>
        ///     The linea r_ offset.
        /// </summary>
        [Description("OPEN")]
        OPEN = 1, 

        /// <summary>
        ///     The constan t_ issue.
        /// </summary>
        [Description("CONFIRMED")]
        CONFIRMED = 2, 

        /// <summary>
        ///     The linear.
        /// </summary>
        [Description("REOPENED")]
        REOPENED = 3, 

        /// <summary>
        ///     The linear.
        /// </summary>
        [Description("RESOLVED")]
        RESOLVED = 4, 

        /// <summary>
        ///     The linear.
        /// </summary>
        [Description("UNDEFINED")]
        UNDEFINED = 5, 
    }

    /// <summary>
    ///     The remediation function.
    /// </summary>
    public enum Status
    {
        /// <summary>
        ///     The linear.
        /// </summary>
        [Description("Ready")]
        READY = 0, 

        /// <summary>
        ///     The linea r_ offset.
        /// </summary>
        [Description("Deprecated")]
        DEPRECATED = 1, 

        /// <summary>
        ///     The constan t_ issue.
        /// </summary>
        [Description("Beta")]
        BETA = 2, 

        /// <summary>
        ///     The linear.
        /// </summary>
        [Description("Undefined")]
        UNDEFINED = 3, 
    }

    /// <summary>
    ///     The remediation function.
    /// </summary>
    public enum RemediationFunction
    {
        /// <summary>
        ///     The linear.
        /// </summary>
        [Description("linear")]
        LINEAR = 0, 

        /// <summary>
        ///     The linea r_ offset.
        /// </summary>
        [Description("linear_offset")]
        LINEAR_OFFSET = 1, 

        /// <summary>
        ///     The constan t_ issue.
        /// </summary>
        [Description("constant_issue")]
        CONSTANT_ISSUE = 2, 

        /// <summary>
        ///     The undefined.
        /// </summary>
        [Description("UNDEFINED")]
        UNDEFINED = 3, 
    }

    /// <summary>
    ///     The severity.
    /// </summary>
    public enum Severity
    {
        /// <summary>
        ///     The blocker.
        /// </summary>
        [Description("BLOCKER")]
        BLOCKER = 0, 

        /// <summary>
        ///     The critical.
        /// </summary>
        [Description("CRITICAL")]
        CRITICAL = 1, 

        /// <summary>
        ///     The major.
        /// </summary>
        [Description("MAJOR")]
        MAJOR = 2, 

        /// <summary>
        ///     The minor.
        /// </summary>
        [Description("MINOR")]
        MINOR = 3, 

        /// <summary>
        ///     The info.
        /// </summary>
        [Description("INFO")]
        INFO = 4, 

        /// <summary>
        ///     The undefined.
        /// </summary>
        [Description("UNDEFINED")]
        UNDEFINED = 5, 
    }

    /// <summary>
    ///     The remediation unit.
    /// </summary>
    public enum RemediationUnit
    {
        /// <summary>
        ///     The mn.
        /// </summary>
        [Description("mn")]
        MN = 0, 

        /// <summary>
        ///     The h.
        /// </summary>
        [Description("h")]
        H = 1, 

        /// <summary>
        ///     The d.
        /// </summary>
        [Description("d")]
        D = 2, 

        /// <summary>
        ///     The undefined.
        /// </summary>
        [Description("UNDEFINED")]
        UNDEFINED = 3, 
    }

    /// <summary>
    ///     The category.
    /// </summary>
    public enum Category
    {
        /// <summary>
        ///     The portability.
        /// </summary>
        [Description("PORTABILITY")]
        PORTABILITY = 0, 

        /// <summary>
        ///     The maintainability.
        /// </summary>
        [Description("MAINTAINABILITY")]
        MAINTAINABILITY = 1, 

        /// <summary>
        ///     The security.
        /// </summary>
        [Description("SECURITY")]
        SECURITY = 2, 

        /// <summary>
        ///     The efficiency.
        /// </summary>
        [Description("EFFICIENCY")]
        EFFICIENCY = 3, 

        /// <summary>
        ///     The changeability.
        /// </summary>
        [Description("CHANGEABILITY")]
        CHANGEABILITY = 4, 

        /// <summary>
        ///     The reliability.
        /// </summary>
        [Description("RELIABILITY")]
        RELIABILITY = 5, 

        /// <summary>
        ///     The testability.
        /// </summary>
        [Description("TESTABILITY")]
        TESTABILITY = 6, 

        /// <summary>
        ///     The reusability.
        /// </summary>
        [Description("REUSABILITY")]
        REUSABILITY = 7, 

        /// <summary>
        ///     The undefined.
        /// </summary>
        [Description("UNDEFINED")]
        UNDEFINED = 8, 
    }

    /// <summary>
    ///     The sub category.
    /// </summary>
    public enum SubCategory
    {
        /// <summary>
        ///     The modularity.
        /// </summary>
        [Description("MODULARITY")]
        MODULARITY = 0, 

        /// <summary>
        ///     The transportability.
        /// </summary>
        [Description("TRANSPORTABILITY")]
        TRANSPORTABILITY = 1, 

        /// <summary>
        ///     The uni t_ testability.
        /// </summary>
        [Description("UNIT_TESTABILITY")]
        UNIT_TESTABILITY = 2, 

        /// <summary>
        ///     The uni t_ tests.
        /// </summary>
        [Description("UNIT_TESTS")]
        UNIT_TESTS = 3, 

        /// <summary>
        ///     The synchronizatio n_ reliability.
        /// </summary>
        [Description("SYNCHRONIZATION_RELIABILITY")]
        SYNCHRONIZATION_RELIABILITY = 4, 

        /// <summary>
        ///     The instructio n_ reliability.
        /// </summary>
        [Description("INSTRUCTION_RELIABILITY")]
        INSTRUCTION_RELIABILITY = 5, 

        /// <summary>
        ///     The faul t_ tolerance.
        /// </summary>
        [Description("FAULT_TOLERANCE")]
        FAULT_TOLERANCE = 6, 

        /// <summary>
        ///     The exceptio n_ handling.
        /// </summary>
        [Description("EXCEPTION_HANDLING")]
        EXCEPTION_HANDLING = 7, 

        /// <summary>
        ///     The dat a_ reliability.
        /// </summary>
        [Description("DATA_RELIABILITY")]
        DATA_RELIABILITY = 8, 

        /// <summary>
        ///     The architectur e_ reliability.
        /// </summary>
        [Description("ARCHITECTURE_RELIABILITY")]
        ARCHITECTURE_RELIABILITY = 9, 

        /// <summary>
        ///     The logi c_ changeability.
        /// </summary>
        [Description("LOGIC_CHANGEABILITY")]
        LOGIC_CHANGEABILITY = 10, 

        /// <summary>
        ///     The dat a_ changeability.
        /// </summary>
        [Description("DATA_CHANGEABILITY")]
        DATA_CHANGEABILITY = 11, 

        /// <summary>
        ///     The architectur e_ changeability.
        /// </summary>
        [Description("ARCHITECTURE_CHANGEABILITY")]
        ARCHITECTURE_CHANGEABILITY = 12, 

        /// <summary>
        ///     The cp u_ efficiency.
        /// </summary>
        [Description("CPU_EFFICIENCY")]
        CPU_EFFICIENCY = 13, 

        /// <summary>
        ///     The memor y_ efficiency.
        /// </summary>
        [Description("MEMORY_EFFICIENCY")]
        MEMORY_EFFICIENCY = 14, 

        /// <summary>
        ///     The securit y_ features.
        /// </summary>
        [Description("SECURITY_FEATURES")]
        SECURITY_FEATURES = 15, 

        /// <summary>
        ///     The inpu t_ validatio n_ an d_ representation.
        /// </summary>
        [Description("INPUT_VALIDATION_AND_REPRESENTATION")]
        INPUT_VALIDATION_AND_REPRESENTATION = 16, 

        /// <summary>
        ///     The errors.
        /// </summary>
        [Description("ERRORS")]
        ERRORS = 17, 

        /// <summary>
        ///     The ap i_ abuse.
        /// </summary>
        [Description("API_ABUSE")]
        API_ABUSE = 18, 

        /// <summary>
        ///     The understandability.
        /// </summary>
        [Description("UNDERSTANDABILITY")]
        UNDERSTANDABILITY = 19, 

        /// <summary>
        ///     The readability.
        /// </summary>
        [Description("READABILITY")]
        READABILITY = 20, 

        /// <summary>
        ///     The tim e_ zon e_ relate d_ portability.
        /// </summary>
        [Description("TIME_ZONE_RELATED_PORTABILITY")]
        TIME_ZONE_RELATED_PORTABILITY = 21, 

        /// <summary>
        ///     The softwar e_ relate d_ portability.
        /// </summary>
        [Description("SOFTWARE_RELATED_PORTABILITY")]
        SOFTWARE_RELATED_PORTABILITY = 22, 

        /// <summary>
        ///     The o s_ relate d_ portability.
        /// </summary>
        [Description("OS_RELATED_PORTABILITY")]
        OS_RELATED_PORTABILITY = 23, 

        /// <summary>
        ///     The languag e_ relate d_ portability.
        /// </summary>
        [Description("LANGUAGE_RELATED_PORTABILITY")]
        LANGUAGE_RELATED_PORTABILITY = 24, 

        /// <summary>
        ///     The hardwar e_ relate d_ portability.
        /// </summary>
        [Description("HARDWARE_RELATED_PORTABILITY")]
        HARDWARE_RELATED_PORTABILITY = 25, 

        /// <summary>
        ///     The compile r_ relate d_ portability.
        /// </summary>
        [Description("COMPILER_RELATED_PORTABILITY")]
        COMPILER_RELATED_PORTABILITY = 26, 

        /// <summary>
        ///     The logi c_ reliability.
        /// </summary>
        [Description("LOGIC_RELIABILITY")]
        LOGIC_RELIABILITY = 27, 

        /// <summary>
        ///     The integratio n_ testability.
        /// </summary>
        [Description("INTEGRATION_TESTABILITY")]
        INTEGRATION_TESTABILITY = 28, 

        /// <summary>
        ///     The undefined.
        /// </summary>
        [Description("UNDEFINED")]
        UNDEFINED = 29
    }
}