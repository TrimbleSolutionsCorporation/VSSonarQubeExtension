module RedundantNewKeywordConfig

open FSharpLint.Rules.RedundantNewKeyword
open FSharpLint.Framework.Configuration

open VSSonarPlugins.Types

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