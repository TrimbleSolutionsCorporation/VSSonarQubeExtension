namespace SonarLocalAnalyser

open System
open System.Runtime.InteropServices
open System.Security.Permissions
open System.Threading
open System.IO
open System.Diagnostics
open VSSonarPlugins
open ExtensionTypes
open CommonExtensions
open MSBuild.Tekla.Tasks.Executor
open System.Diagnostics
open Microsoft.Build.Utilities

type ISonarLocalAnalyser =     
  abstract member StopAllExecution : unit -> unit
  abstract member IsExecuting : unit -> bool
  abstract member GetResourceKey : VsProjectItem * Resource * ConnectionConfiguration -> string                               

  abstract member AnalyseFile : VsProjectItem * Resource * Profile * bool * double * ConnectionConfiguration -> unit
  abstract member RunIncrementalAnalysis : string * Resource * Profile * double * ConnectionConfiguration -> unit
  abstract member RunPreviewAnalysis : string * Resource * Profile * double * ConnectionConfiguration -> unit
  abstract member RunFullAnalysis : string * Resource * double * ConnectionConfiguration -> unit

  abstract member GetIssues : ConnectionConfiguration -> System.Collections.Generic.List<Issue>
 
  [<CLIEvent>]
  abstract member LocalAnalysisCompleted : IDelegateEvent<System.EventHandler>

  [<CLIEvent>]
  abstract member StdOutEvent : IDelegateEvent<System.EventHandler>
                          
