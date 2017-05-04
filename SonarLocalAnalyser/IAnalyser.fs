// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAnalyser.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace SonarLocalAnalyser

open System
open System.Runtime.InteropServices
open System.Security.Permissions
open System.Threading
open System.IO
open System.Diagnostics
open VSSonarPlugins
open VSSonarPlugins.Types
open CommonExtensions
open System.Diagnostics
open Microsoft.Build.Utilities
open VSSonarPlugins.Types
open System.ComponentModel

type ISonarLocalAnalyser = 
  abstract member StopAllExecution : unit -> unit
  abstract member IsExecuting : unit -> bool
  abstract member GetResourceKey : VsFileItem * safeIsOn:bool -> string
  abstract member AnalyseFile : VsFileItem * Resource * onModifiedLinesOnly:bool *  version:double * ISonarConfiguration * ISQKeyTranslator * vsInter : IVsEnvironmentHelper * fromSave : bool -> unit
  abstract member RunProjectAnalysis : project : VsProjectItem * conf : ISonarConfiguration -> unit
  abstract member RunFullAnalysis : Resource * version:double * ISonarConfiguration -> unit
  abstract member AssociateWithProject : project:Resource * conf:ISonarConfiguration -> unit
  abstract member OnDisconect : unit -> unit 
  abstract member ResetInitialization : unit -> unit
  abstract member UpdateExclusions : exclusions : System.Collections.Generic.IList<Exclusion> -> unit
  abstract member GetRuleForKey : key : string * project : Resource -> Rule
  abstract member GetProfile : project:Resource -> System.Collections.Generic.Dictionary<string, Profile>
  
  [<CLIEvent>]
  abstract member AssociateCommandCompeted : IDelegateEvent<System.EventHandler>

  [<CLIEvent>]
  abstract member LocalAnalysisCompleted : IDelegateEvent<System.EventHandler>

  [<CLIEvent>]
  abstract member StdOutEvent : IDelegateEvent<System.EventHandler>
                          
