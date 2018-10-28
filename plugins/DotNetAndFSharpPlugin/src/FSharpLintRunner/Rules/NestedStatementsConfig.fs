module NestedStatementsConfig

open FSharpLint.Rules.NestedStatements
open FSharpLint.Framework.Configuration

open SonarRestService.Types

let SonarConfiguration(config : Profile) =
    Map.ofList 
        [ 
            (AnalyserName, 
                { 
                    Rules = Map.ofList []
                    Settings = Map.ofList 
                        [ 
                            ("Enabled", ConfHelper.GetEnaFlagForRule(config, "RulesNestedStatementsError"))
                            ("Depth", Depth(ConfHelper.GetValueForInt(config, "RulesNestedStatementsError", "Depth", 5)))
                        ]
                });
    ]