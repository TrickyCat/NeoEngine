namespace TrickyCat.Text.TemplateEngines.NeoEngine.Parsers

open FParsec
open System

module ParserCore =
    type TemplateNode' =
            | Str' of string
            | Neo' of string
            | NeoSubstitute' of string

            | NeoInclude' of string
            | NeoIncludeView' of string

            | BeginOfConditionalTemplate' of condition: string
            | EndOfConditionalTemplate'
            | ElseBranchOfConditionalTemplateDelimiter'

            | NeoIfElseTemplate' of NeoIfElseTemplate<TemplateNode'>
    and 'node NeoIfElseTemplate = { condition: string; ifBranchBody: 'node list; elseBranchBody: 'node list option }
    
    type Template' = TemplateNode' list


    let private Neo' (s: string)                        = s.Trim() |> Neo'
    let private NeoSubstitute' (s: string)              = s.Trim() |> NeoSubstitute'
    let private BeginOfConditionalTemplate' (s: string) = s.Trim() |> BeginOfConditionalTemplate'
    let private NeoInclude' (s: string)                 = s.Trim() |> NeoInclude'
    let private NeoIncludeView' (s: string)             = s.Trim() |> NeoIncludeView'

    let private maxStrLen = Int32.MaxValue
    let private str       = pstring
    let private ws        = spaces
    let private str_ws s  = str s .>> ws
    let private str_ws1 s = str s .>> spaces1

    let private jsStringDelim: Parser<unit, unit>                              = skipChar '\'' <|> skipChar '"'
    let internal strBeforeNeoCustomizationParser : Parser<TemplateNode', unit> = attempt <| charsTillString "<%" false maxStrLen |>> Str'
    let internal strBeforeEos : Parser<TemplateNode', unit>                    = many1Satisfy ((<>)EOS) |>> Str'
    
    let private endOfConditionalTemplate =
        attempt
        <| (
            str_ws "<%" 
            >>. str_ws "}" 
            >>. str "%>" 
            |>> (fun _ -> EndOfConditionalTemplate')
            )

    let private elseBranchOfConditionalTemplate =
        attempt
        <| (
            str_ws "<%" 
            >>. str_ws "}" 
            >>. str_ws "else" 
            >>. str_ws "{" 
            >>. str "%>" 
            |>> (fun _ -> ElseBranchOfConditionalTemplateDelimiter')
            )

    let private beginOfConditionalTemplate: Parser<TemplateNode', unit> =
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


    let private neoBodyParser            = charsTillString "%>" true maxStrLen
    let private neoGeneralIncludeParser  = str_ws "<%@" >>. neoBodyParser |>> NeoInclude'

    let private neoIncludeViewParser: Parser<TemplateNode', unit> = 
        str_ws "<%@" 
        >>. str_ws1 "include" 
        >>. str_ws "view" 
        >>. str_ws "=" 
        >>. jsStringDelim
        >>. manyCharsTill anyChar jsStringDelim
        .>> ws
        .>> str "%>"
        |>> NeoIncludeView'

    let internal neoIncludeParser =
        attempt neoIncludeViewParser
        <|> attempt neoGeneralIncludeParser

    let internal neoSubstituteParser = str "<%=" >>. neoBodyParser |>> NeoSubstitute'
    let internal neoBlockParser      = str "<%"  >>. neoBodyParser |>> Neo'
    let internal neoParser = 
        neoIncludeParser
        <|> neoSubstituteParser
        <|> beginOfConditionalTemplate
        <|> endOfConditionalTemplate
        <|> elseBranchOfConditionalTemplate
        <|> neoBlockParser

