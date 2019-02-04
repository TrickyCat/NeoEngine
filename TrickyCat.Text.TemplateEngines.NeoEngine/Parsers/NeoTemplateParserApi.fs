namespace TrickyCat.Text.TemplateEngines.NeoEngine.Parsers

open FParsec
open NeoTemplateParserCore
open TrickyCat.Text.TemplateEngines.NeoEngine.ResultCommon
open System

module NeoTemplateParserApi =
    type TemplateNode =
        Str2 of string
      | Neo2 of string
      | NeoSubstitute2 of string

      //| NeoInclude of string
      //| NeoIncludeValue of string
      | NeoIncludeView2 of string

      //| BeginOfConditionalTemplate of condition: string
      //| EndOfConditionalTemplate
      //| ElseBranchOfConditionalTemplateDelimiter

      | NeoIfElseTemplate2 of NeoIfElseTemplate2
    and NeoIfElseTemplate2 = { condition2: string; ifBranchBody2: TemplateNode list; elseBranchBody2: TemplateNode list option }

    let rec toTemplate (nodes: TemplateNode' list): Result<TemplateNode list, string> =
        let rec runner (acc: Result<TemplateNode list, string>) (nodes: TemplateNode' list) =
            acc
            >>= (fun accList ->
                match nodes with
                      [] -> Result.Ok accList
                    | x :: xs ->
                        match x with
                          Str s            -> runner (Result.Ok <| (Str2 s)            :: accList) xs
                        | Neo s            -> runner (Result.Ok <| (Neo2 s)            :: accList) xs
                        | NeoSubstitute s  -> runner (Result.Ok <| (NeoSubstitute2 s)  :: accList) xs
                        | NeoIncludeView s -> runner (Result.Ok <| (NeoIncludeView2 s) :: accList) xs
                        | NeoIfElseTemplate { condition = c; ifBranchBody = ifs'; elseBranchBody = elses' }
                            -> let newAcc =
                                    ifs'
                                    |> toTemplate
                                    >>= (fun ifNodes -> 
                                        let elseNodes =
                                            match elses' |> Option.map toTemplate with
                                             Some (Result.Ok ees) -> Some ees
                                            | _ -> None
                                        Result.Ok <| (NeoIfElseTemplate2 { condition2 = c; ifBranchBody2 = ifNodes; elseBranchBody2 = elseNodes }) :: accList
                                    )
                               runner newAcc xs
                        | y -> Result.Error <| sprintf "Unexpected AST element: %O" y
                )

        runner (Result.Ok []) nodes
        |> Result.map List.rev


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

                    let listChunk =
                        res.acc
                        |> List.skip (closestElseIdx + 1)
                        |> List.take (closestIfIdx - closestElseIdx)
                        |> List.rev

                    match listChunk with
                     (BeginOfConditionalTemplate condition) :: ifBranchContent ->
                        NeoIfElseTemplate { condition = condition; ifBranchBody = ifBranchContent; elseBranchBody = elseBranchContent }
                    | _                                                       ->
                        failwith "Empty list OR its head isn't BeginOfConditionalTemplate"
                else
                    match List.rev(List.take qtyOfElementsToClosestIf res.acc) with
                     (BeginOfConditionalTemplate condition) :: rest ->
                        NeoIfElseTemplate { condition = condition; ifBranchBody = rest; elseBranchBody = None }
                    | _                                             ->
                        failwith "Empty list: List.rev(List.take qtyOfElementsToClosestIf res.acc)\nOR\nFirst element is not BeginOfConditionalTemplate"

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
