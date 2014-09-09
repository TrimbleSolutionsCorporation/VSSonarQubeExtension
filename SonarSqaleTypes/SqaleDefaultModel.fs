module SonarSqaleTypes

open FSharp.Data
open System.Xml.Linq
open System.IO
open System.Reflection
open System
open System.ComponentModel 
open ExtensionTypes

type SqaleModelType = XmlProvider<"""<?xml version="1.0"?>
<sqale>
    <chc>
        <key>PORTABILITY</key>
        <name>Portability</name>
        <chc>
            <key>COMPILER_RELATED_PORTABILITY</key>
            <name>Compiler related portability</name>
            <chc>
                <rule-repo>gendarme</rule-repo>
                <rule-key>DoNotPrefixValuesWithEnumNameRule</rule-key>
                <prop>
                    <key>remediationFactor</key>
                    <val>0.03d</val>
                    <txt>d</txt>
                </prop>
                <prop>
                    <key>remediationFunction</key>
                    <txt>linear</txt>
                </prop>
            </chc>
            <chc>
                <rule-repo>gendarme</rule-repo>
                <rule-key>DoNotPrefixValuesWithEnumNameRule1</rule-key>
                <prop>
                    <key>remediationFactor</key>
                    <val>0.03d</val>
                    <txt>d</txt>
                </prop>
                <prop>
                    <key>remediationFunction</key>
                    <txt>linear</txt>
                </prop>
                <prop>
                    <key>offset</key>
                    <val>0.0d</val>
                    <txt>d</txt>
                </prop>
            </chc>                          
        </chc>
        <chc>
            <key>HARDWARE_RELATED_PORTABILITY</key>
            <name>Hardware related portability</name>
        </chc>
        <chc>
            <key>LANGUAGE_RELATED_PORTABILITY</key>
            <name>Language related portability</name>
        </chc>
        <chc>
            <key>OS_RELATED_PORTABILITY</key>
            <name>OS related portability</name>
        </chc>
        <chc>
            <key>SOFTWARE_RELATED_PORTABILITY</key>
            <name>Software related portability</name>
        </chc>
        <chc>
            <key>TIME_ZONE_RELATED_PORTABILITY</key>
            <name>Time zone related portability</name>
        </chc>
    </chc>
    <chc>
        <key>PORTABILITY</key>
        <name>Portability</name>
        <chc>
            <key>COMPILER_RELATED_PORTABILITY</key>
            <name>Compiler related portability</name>
        </chc>
    </chc>
</sqale>""">

module EnumHelper = 
    let asEnum<'T 
      when 'T : enum<int>
      and 'T : struct
      and 'T :> ValueType
      and 'T : (new : unit -> 'T)> text =
      match Enum.TryParse<'T>(text, true) with
      | true, value -> Some value
      | _ -> None

    let getEnumDescription (value : Enum) =
       let typ = value.GetType()
       let name = value.ToString();
       let attrs = typ.GetField(name).GetCustomAttributes(typedefof<DescriptionAttribute>, false)
       if (attrs.Length > 0) then (attrs.[0] :?> DescriptionAttribute).Description
       else name

module GlobalsVars =
    let mutable characteristics : Characteristic list = []

    let GetChar(key : Category) =                    
        (List.toArray characteristics) |> Array.find (fun elem -> elem.Key.Equals(key))

    let IsCharPresent(key : Category) =                    
        (List.toArray characteristics) |> Array.exists (fun elem -> elem.Key.Equals(key))

    let CreateAChar(key : Category, name : string) =
        if not(IsCharPresent(key)) then
            let newChar = new Characteristic(key, name)
            characteristics <- characteristics @ [newChar]
            newChar
        else
            GetChar(key)

    let defaultSqaleModel =
        let assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)
        let asm = Assembly.GetExecutingAssembly()
        use stream = asm.GetManifestResourceStream("defaultmodel.xml")
        let xmldoc = XDocument.Load(stream).ToString()
        let sqale = SqaleModelType.Parse(xmldoc)
        
        for chk in sqale.GetChcs() do
            let char = CreateAChar(EnumHelper.asEnum<Category>(chk.Key).Value, chk.Name)            

            for subchk in chk.GetChcs() do
                char.CreateSubChar(EnumHelper.asEnum<SubCategory>(subchk.Key).Value, subchk.Name)

        characteristics
