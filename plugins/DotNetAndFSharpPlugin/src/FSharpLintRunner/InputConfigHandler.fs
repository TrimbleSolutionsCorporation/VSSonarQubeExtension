module InputConfigHandler

open SonarRestService.Types
open FSharpLint.Framework.Configuration

let CreateALintConfiguration(config : System.Collections.Generic.Dictionary<string, Profile>) =
    let join (p:Map<'a,'b>) (q:Map<'a,'b>) = 
        Map(Seq.concat [ (Map.toSeq p) ; (Map.toSeq q) ])
    let configdata = ()
    {
        Configuration.IgnoreFiles =  None
        Analysers = Map(Seq.concat [
                            (Map.toSeq (NamingConventionsConfig.SonarConfiguration(config.["fs"])));
                            (Map.toSeq (SourceLengthConfig.SonarConfiguration(config.["fs"])));
                            (Map.toSeq (BindingConfig.SonarConfiguration(config.["fs"])));
                            (Map.toSeq (NumberOfItemsConfig.SonarConfiguration(config.["fs"])));
                            (Map.toSeq (NestedStatementsConfig.SonarConfiguration(config.["fs"])));
                            (Map.toSeq (TypographyConfig.SonarConfiguration(config.["fs"])));
                            (Map.toSeq (FunctionReimplementationConfig.SonarConfiguration(config.["fs"])));
                            (Map.toSeq (RaiseWithTooManyArgumentsConfig.SonarConfiguration(config.["fs"])));
                            (Map.toSeq (HintsConfig.SonarConfiguration(config.["fs"])));
                            (Map.toSeq (RedundantNewKeywordConfig.SonarConfiguration(config.["fs"])));
                        ])
    }
