﻿namespace TrickyCat.Text.TemplateEngines.NeoEngine.Parsers

open FParsec
open NeoTemplateParserCore
open TrickyCat.Text.TemplateEngines.NeoEngine.ResultCommon
open System

module NeoTemplateParserApi =
    //type TemplateNode =
    //    Str of string
    //  | Neo of string
    //  | NeoSubstitute of string

    //  //| NeoInclude of string
    //  //| NeoIncludeValue of string
    //  | NeoIncludeView of string

    //  //| BeginOfConditionalTemplate of condition: string
    //  //| EndOfConditionalTemplate
    //  //| ElseBranchOfConditionalTemplateDelimiter

    //  | NeoIfElseTemplate of NeoIfElseTemplate
    //and NeoIfElseTemplate = { condition: string; ifBranchBody: TemplateNode list; elseBranchBody: TemplateNode list option }

    //type t = TemplateNode'

    //let rec toTemplate (nodes: TemplateNode' list): Result<TemplateNode list, string> =
    //    let rec runner (acc: Result<TemplateNode list, string>) (nodes: TemplateNode' list) =
    //        acc
    //        >>= (fun accList -> match nodes with
    //                | [] -> Result.Ok accList
    //                | x :: xs -> match x with
    //                    | t.Str s -> runner (Result.Ok <| (Str s) :: accList) xs
    //                    | t.Neo s -> runner (Result.Ok <| (Neo s) :: accList) xs
    //                    | t.NeoSubstitute s -> runner (Result.Ok <| (NeoSubstitute s) :: accList) xs
    //                    | t.NeoIncludeView s -> runner (Result.Ok <| (NeoIncludeView s) :: accList) xs

    //                    //| t.NeoIfElseTemplate { condition = c; ifBranchBody = ifs'; elseBranchBody = elses' }
    //                    //    ->
    //                    //        let ifs = toTemplate ifs'
    //                    //        let elses = elses' |> Option.map toTemplate
    //                    //        match elses with
    //                    //        | None -> 


    //                    //        runner (Result.Ok <| (NeoIncludeView s) :: accList) xs
                        
    //                    | y -> Result.Error <| sprintf "Unexpected AST element: %O" y
    //            )

    //    runner (Result.Ok []) nodes


    type Template = TemplateNode' list

    type private FoldIfsAcc = { output: TemplateNode' list; acc: TemplateNode' list }

    let private dropEmptyBlocks template =
        template
        |> List.filter(function
            | Str ""
            | Neo ""
            | NeoSubstitute ""
            | NeoInclude ""
            | NeoIncludeValue ""
            | NeoIncludeView ""
            | NeoIfElseTemplate { condition = "" } -> false
            | _                                    -> true
            )

    let private dropEmptyIfBranchBody template =
        template
        |> List.filter(function
            | NeoIfElseTemplate { ifBranchBody = []; elseBranchBody = None } -> false
            | _                                                              -> true
            )
        |> List.map(function
            | NeoIfElseTemplate { condition = c; ifBranchBody = []; elseBranchBody = Some(y) }
                -> NeoIfElseTemplate { condition = sprintf "!(%s)" c; ifBranchBody = y; elseBranchBody = None }
            | x -> x
            )

    let private foldIfs templates =
        let collapseConditionalBody (res: FoldIfsAcc) (t: TemplateNode') =
            let accLength = res.acc |> List.length
            let closestIfIdx = 
                match List.tryFindIndex (function BeginOfConditionalTemplate _ -> true | _ -> false) res.acc with
                | Some n -> n
                | None   -> failwith "No matching opening IF for closing tag: <% } %>"

            let closestElseIdx = 
                match List.tryFindIndex (function ElseBranchOfConditionalTemplateDelimiter -> true | _ -> false) res.acc with
                | Some n -> n
                | None   -> Int32.MaxValue

            let qtyOfElementsToClosestIf = closestIfIdx + 1

            let ifElseNode = 
                if closestElseIdx < closestIfIdx then
                    let elseBranchContentRaw = List.rev (List.take closestElseIdx res.acc)
                    let elseBranchContent = if List.isEmpty elseBranchContentRaw then None else Some elseBranchContentRaw
                    let (BeginOfConditionalTemplate condition) :: ifBranchContent =
                        res.acc
                        |> List.skip (closestElseIdx + 1)
                        |> List.take (closestIfIdx - closestElseIdx)
                        |> List.rev
                    NeoIfElseTemplate { condition = condition; ifBranchBody = ifBranchContent; elseBranchBody = elseBranchContent }
                else
                    let (BeginOfConditionalTemplate condition) :: rest = List.rev(List.take qtyOfElementsToClosestIf res.acc)
                    NeoIfElseTemplate { condition = condition; ifBranchBody = rest; elseBranchBody = None }

            if accLength > qtyOfElementsToClosestIf then
                { res with acc = ifElseNode :: (List.skip qtyOfElementsToClosestIf res.acc) }
            else
                { output = ifElseNode :: res.output; acc = List.skip qtyOfElementsToClosestIf res.acc }


        let { output = folded; acc = acc } =
            templates
            |> List.fold (fun res t ->
                match t, res with 
                  BeginOfConditionalTemplate _, { acc = [] }  -> { res with acc = [t] }
                | BeginOfConditionalTemplate _, _             -> { res with acc = t :: res.acc }
                | Str _, { acc = [] }                         -> { res with output = t :: res.output }
                | Str _, _                                    -> { res with acc = t :: res.acc }
                | EndOfConditionalTemplate, { acc = [] }      -> failwith "Not supported: Unexpected end of conditional template."
                | EndOfConditionalTemplate, _                 -> collapseConditionalBody res t
                | ElseBranchOfConditionalTemplateDelimiter, _ -> { res with acc = t :: res.acc }
                | _, { acc = [] }                             -> { res with output = t :: res.output }
                | _, _                                        -> { res with acc = t :: res.acc }
            ) { output = []; acc = [] }

        assert (List.isEmpty acc)
        List.rev folded
    
    let private fold = dropEmptyBlocks >> foldIfs >> dropEmptyIfBranchBody

    let templateParser = 
        many((attempt neoParser) <|> (notEmpty (strBeforeNeoCustomizationParser <|> strBeforeEos)))
        .>> eof
        |>> fold

    let runParserOnString string : Result<TemplateNode' list, string> =
        match run templateParser string with
        | Success(result, _, _)   -> Result.Ok result
        | Failure(errorMsg, _, _) -> Result.Error errorMsg
