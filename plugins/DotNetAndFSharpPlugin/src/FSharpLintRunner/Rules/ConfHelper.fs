module ConfHelper

open System

open VSSonarPlugins.Types

open FParsec
open FSharpLint.Framework.HintParser
open FSharpLint.Framework.Configuration

let GetEnaFlagForParam(externlProfileIn : Profile, ruleId : string, paramName : string) =
    try
        let rule = externlProfileIn.GetRule("fsharplint:" + ruleId)
        if rule <> null then
            let enabledis = rule.Params |> Seq.find (fun c -> c.Key.Equals(paramName))

            if enabledis.Value = "0" then
                Enabled(false)
            else
                Enabled(true)
        else
            Enabled(false)
    with
    | ex -> Enabled(false)

let GetEnaFlagForRule(externlProfileIn : Profile, ruleId : string) =
    try
        let rule = externlProfileIn.GetRule("fsharplint:" + ruleId)
        if rule <> null then
            Enabled(true)
        else
            Enabled(false)
    with
    | ex -> Enabled(false)

let GetValueForInt(externlProfileIn : Profile, ruleId : string, paramName : string, defaultValue : int) =
    try
        let rule = externlProfileIn.GetRule("fsharplint:" + ruleId)
        if rule <> null then
            let param = rule.Params |> Seq.find (fun c -> c.Key.Equals(paramName))
            Int32.Parse(param.Value.Replace("\"", ""))
        else
            defaultValue
    with
    | ex -> defaultValue

let GetValueForString(externlProfileIn : Profile, ruleId : string, paramName : string, defaultValue : string) =
    try
        let rule = externlProfileIn.GetRule("fsharplint:" + ruleId)
        if rule <> null then
            let param = rule.Params |> Seq.find (fun c -> c.Key.Equals(paramName))
            param.Value
        else
            defaultValue
    with
    | ex -> defaultValue

let GetValueForEnum(externlProfileIn : Profile, ruleId : string, paramName : string, defaultValue : string, enumType : 'T) =
    try
        let rule = externlProfileIn.GetRule("fsharplint:" + ruleId)
        if rule <> null then
            let param = rule.Params |> Seq.find (fun c -> c.Key.Equals(paramName))
            Enum.Parse(enumType, param.Value) :?> 'T
        else
            Enum.Parse(enumType, defaultValue) :?> 'T
    with
    | ex -> Enum.Parse(enumType, defaultValue) :?> 'T

let GetValueForStringList(externlProfileIn : Profile, ruleId : string, paramName : string, defaultValue : string List) =
    try
        let rule = externlProfileIn.GetRule("fsharplint:" + ruleId)
        if rule <> null then
            let param = rule.Params |> Seq.find (fun c -> c.Key.Equals(paramName))
            param.Value.Split(';') |> Array.toList
        else
            defaultValue
    with
    | ex -> defaultValue

let parseHints hints =
    let parseHint hint =
        match CharParsers.run phint hint with
        | FParsec.CharParsers.Success(hint, _, _) -> hint
        | FParsec.CharParsers.Failure(error, _, _) -> failwithf "Invalid hint %s" error

    let hintsData = List.map (fun x -> { Hint = x; ParsedHint = parseHint x }) hints

    { Hints = hintsData; Update = Update.Overwrite }

let GetValueForBool(externlProfileIn : Profile, ruleId : string, paramName : string, defaultValue : bool) =
    try
        let rule = externlProfileIn.GetRule("fsharplint:" + ruleId)
        if rule <> null then
            let param = rule.Params |> Seq.find (fun c -> c.Key.Equals(paramName))
            bool.Parse(param.Value.Replace("\"", ""))
        else
            defaultValue
    with
    | ex -> defaultValue