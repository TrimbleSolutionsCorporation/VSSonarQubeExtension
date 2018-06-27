// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeProviders.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace SqaleManager

open FSharp.Data
open System.Xml.Linq
open System
open System.ComponentModel 

type CxxProjectDefinition = XmlProvider<"""<?xml version="1.0" encoding="ASCII"?>
<sqaleManager xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="cxx-model-project.xsd">
    <rules>
    <rule key="AssignmentAddressToInteger">
        <name>Assigning an address value to an integer (int/long/etc.) type is not portable</name>
        <requirement>undefined</requirement>
        <remediationFactorVal>0.0</remediationFactorVal>
        <remediationFactorUnit>undefined</remediationFactorUnit>
        <remediationFunction>undefined</remediationFunction>
        <remediationOffsetVal>0.0</remediationOffsetVal>
        <remediationOffsetUnit>undefined</remediationOffsetUnit>
        <severity>MINOR</severity>
        <repo>cppcheck</repo>
        <description>
      Assigning an address value to an integer (int/long/etc.) type is
      not portable.
    </description>
    </rule>
    <rule key="AssignmentIntegerToAddress">
        <name>Assigning an integer (int/long/etc) to a pointer is not portable</name>
        <requirement>undefined</requirement>
        <remediationFactorVal>0.0</remediationFactorVal>
        <remediationFactorUnit>undefined</remediationFactorUnit>
        <remediationFunction>undefined</remediationFunction>
        <remediationOffsetVal>0.0</remediationOffsetVal>
        <remediationOffsetUnit>d</remediationOffsetUnit>
        <severity>MINOR</severity>
        <repo>cppcheck</repo>
        <description>
      Assigning an integer (int/long/etc) to a pointer is not portable.
    </description>
    </rule>
    </rules>
</sqaleManager>""" >


type ProfileDefinition = XmlProvider<"""<?xml version="1.0" encoding="ASCII"?>
<profile>
  <name>Sonar way</name>
 <language>c++</language>
  <rules>
    <rule>
      <repositoryKey>cppcheck</repositoryKey>
      <key>AssignmentAddressToInteger</key>
      <priority>MINOR</priority>
    </rule>
    <rule>
      <repositoryKey>cppcheck</repositoryKey>
      <key>AssignmentIntegerToAddress</key>
      <priority>MINOR</priority>
    </rule>
  </rules>
</profile>""">

type RulesXmlNewType = XmlProvider<"""<?xml version="1.0" encoding="ASCII"?>
<rules>
    <rule key="cpplint.readability/nolint-0">
        <name><![CDATA[ Unknown NOLINT error category: %s  % category]]></name>
        <configKey><![CDATA[cpplint.readability/nolint-0@CPP_LINT]]></configKey>
        <category name="readability" />
        <description><![CDATA[  Unknown NOLINT error category: %s  % category ]]></description>
    </rule>
    <rule key="cpplint.readability/fn_size-0">
        <name><![CDATA[ Small and focused functions are preferred:   %s has %d non-comment lines   (error triggered by exceeding %d lines).  % (self.current_function, self.lines_in_function, trigger)]]></name>
        <configKey><![CDATA[cpplint.readability/fn_size-0@CPP_LINT]]></configKey>
        <category name="readability" />
        <description><![CDATA[  Small and focused functions are preferred:   %s has %d non-comment lines   (error triggered by exceeding %d lines).  % (self.current_function, self.lines_in_function, trigger) ]]></description>
    </rule>
</rules>""">

type RulesXmlOldType = XmlProvider<"""<?xml version="1.0" encoding="ASCII"?>
<rules>
  <rule>
    <key>AssignmentAddressToInteger</key>
    <configkey>AssignmentAddressToInteger</configkey>
    <name>Assigning an address value to an integer (int/long/etc.) type is not portable</name>
    <description>
      Assigning an address value to an integer (int/long/etc.) type is
      not portable.
    </description>
  </rule>
  <rule>
    <key>AssignmentIntegerToAddress</key>
    <configkey>AssignmentIntegerToAddress</configkey>
    <category>Maintainability</category>
    <name>Assigning an integer (int/long/etc) to a pointer is not portable</name>
    <description>
      Assigning an integer (int/long/etc) to a pointer is not portable.
    </description>
  </rule>
 </rules>""">   



