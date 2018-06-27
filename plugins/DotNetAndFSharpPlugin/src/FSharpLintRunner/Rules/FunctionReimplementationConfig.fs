module FunctionReimplementationConfig

open FSharpLint.Rules.FunctionReimplementation
open FSharpLint.Framework.Configuration

open VSSonarPlugins.Types

let SonarConfiguration(config : Profile) =
    Map.ofList 
        [ 
            (AnalyserName, 
                { 
                    Rules = Map.ofList 
                        [
                            ("CanBeReplacedWithComposition", 
                                { 
                                    Settings = Map.ofList 
                                        [ 
                                            ("Enabled", ConfHelper.GetEnaFlagForRule(config, "RulesCanBeReplacedWithComposition"))
                                        ] 
                                }) 
                            ("ReimplementsFunction", 
                                { 
                                    Settings = Map.ofList 
                                        [ 
                                            ("Enabled", ConfHelper.GetEnaFlagForRule(config, "RulesReimplementsFunction"))
                                        ] 
                                }) 
                        ]
                    Settings = Map.ofList 
                        [ 
                            ("Enabled", Enabled(true))
                        ]
                });
    ]