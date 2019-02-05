namespace TrickyCat.Text.TemplateEngines.NeoEngine.Parsers

open FParsec
open System

module NeoTemplateParserCore =
    type TemplateNode' =
            | Str' of string
            | Neo' of string
            | NeoSubstitute' of string

            | NeoInclude' of string
            | NeoIncludeValue' of string
            | NeoIncludeView' of string

            | BeginOfConditionalTemplate' of condition: string
            | EndOfConditionalTemplate'
            | ElseBranchOfConditionalTemplateDelimiter'

            | NeoIfElseTemplate' of NeoIfElseTemplate<TemplateNode'>
    and 'node NeoIfElseTemplate = { condition: string; ifBranchBody: 'node list; elseBranchBody: 'node list option }



    let private Neo' (s: string)                        = s.Trim() |> Neo'
    let private NeoSubstitute' (s: string)              = s.Trim() |> NeoSubstitute'
    let private BeginOfConditionalTemplate' (s: string) = s.Trim() |> BeginOfConditionalTemplate'
    let private NeoInclude' (s: string)                 = s.Trim() |> NeoInclude'
    let private NeoIncludeValue' (s: string)            = s.Trim() |> NeoIncludeValue'
    let private NeoIncludeView' (s: string)             = s.Trim() |> NeoIncludeView'

    let private maxStrLen = Int32.MaxValue
    let private str       = pstring
    let private ws        = spaces
    let str_ws s          = str s .>> ws
    let str_ws1 s         = str s .>> spaces1

    let jsStringDelim: Parser<unit, unit> = skipChar '\'' <|> skipChar '"'
    let internal strBeforeNeoCustomizationParser : Parser<TemplateNode', unit> = attempt <| charsTillString "<%" false maxStrLen |>> Str'
    let strBeforeEos : Parser<TemplateNode', unit>                             = many1Satisfy ((<>)EOS) |>> Str'
    
    let endOfConditionalTemplate =
        attempt
        <| (
            str_ws "<%" 
            >>. str_ws "}" 
            >>. str "%>" 
            |>> (fun _ -> EndOfConditionalTemplate')
            )

    let elseBranchOfConditionalTemplate =
        attempt
        <| (
            str_ws "<%" 
            >>. str_ws "}" 
            >>. str_ws "else" 
            >>. str_ws "{" 
            >>. str "%>" 
            |>> (fun _ -> ElseBranchOfConditionalTemplateDelimiter')
            )

    let beginOfConditionalTemplate: Parser<TemplateNode', unit> =
        let endp = str_ws ")" .>> str_ws "{"
        let conditionBase = anyChar
        let conditionP = manyTill conditionBase (attempt endp) |>> (fun x -> String.Join("", x))
        attempt
        <| ( 
            str_ws "<%" 
            >>. str_ws "if" 
            >>. str_ws "(" 
            >>. conditionP 
            .>> str "%>" 
            |>> BeginOfConditionalTemplate'
            )


    let neoBodyParser            = charsTillString "%>" true maxStrLen
    let neoGeneralIncludeParser  = str_ws "<%@" >>. neoBodyParser |>> NeoInclude'

    let neoIncludeViewParser: Parser<TemplateNode', unit> = 
        str_ws "<%@" 
        >>. str_ws1 "include" 
        >>. str_ws "view" 
        >>. str_ws "=" 
        >>. jsStringDelim
        >>. manyCharsTill anyChar jsStringDelim
        .>> ws
        .>> str "%>"
        |>> NeoIncludeView'

    let neoValueParser: Parser<TemplateNode', unit> =
        str_ws "<%@" 
        >>. str_ws1 "value" 
        >>. neoBodyParser
        |>> NeoIncludeValue'

    let neoIncludeParser =
        attempt neoIncludeViewParser
        <|> attempt neoValueParser
        <|> attempt neoGeneralIncludeParser

    let neoSubstituteParser = str "<%=" >>. neoBodyParser |>> NeoSubstitute'
    let neoBlockParser      = str "<%"  >>. neoBodyParser |>> Neo'
    let neoParser = 
        neoIncludeParser
        <|> neoSubstituteParser
        <|> beginOfConditionalTemplate
        <|> endOfConditionalTemplate
        <|> elseBranchOfConditionalTemplate
        <|> neoBlockParser

