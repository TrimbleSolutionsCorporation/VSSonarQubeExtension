module RedundantNewKeywordConfig

open FSharpLint.Rules.RedundantNewKeyword
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
                            ("Enabled", ConfHelper.GetEnaFlagForRule(config, "RulesRedundantNewKeywordError"))
                        ]
                });
    ]