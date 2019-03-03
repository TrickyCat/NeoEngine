namespace TrickyCat.Text.TemplateEngines.NeoEngine.Parsers

open FParsec
open NeoTemplateParserCore
open TrickyCat.Text.TemplateEngines.NeoEngine.ResultCommon
open System

module NeoTemplateParserApi =
    type TemplateNode =
      | Str of string
      | Neo of string
      | NeoSubstitute of string
      | NeoIncludeView of string
      | NeoIfElseTemplate of NeoIfElseTemplate<TemplateNode>

    type Template = TemplateNode list

    /// <summary>
    /// Transforms low level AST to higher level one.
    /// </summary>
    let rec internal toTemplate (nodes: Template'): Result<Template, string> =
        let rec runner (acc: Result<Template, string>) (nodes: Template') =
            acc
            >>= (fun accList ->
                match nodes with
                    | []      -> Result.Ok accList
                    | x :: xs ->
                        match x with
                        | Str' s            -> runner (Result.Ok <| (Str s)            :: accList) xs
                        | Neo' s            -> runner (Result.Ok <| (Neo s)            :: accList) xs
                        | NeoSubstitute' s  -> runner (Result.Ok <| (NeoSubstitute s)  :: accList) xs
                        | NeoIncludeView' s -> runner (Result.Ok <| (NeoIncludeView s) :: accList) xs
                        | NeoIfElseTemplate' { condition = c; ifBranchBody = ifs'; elseBranchBody = elses' }
                            -> let newAcc =
                                    ifs'
                                    |> toTemplate
                                    >>= (fun ifNodes -> 
                                        let elseNodes =
                                            match elses' |> Option.map toTemplate with
                                            | Some (Result.Ok ees) -> Some ees
                                            | _                    -> None
                                        Result.Ok <| (NeoIfElseTemplate { condition = c; ifBranchBody = ifNodes; elseBranchBody = elseNodes }) :: accList
                                    )
                               runner newAcc xs
                        | y -> Result.Error <| sprintf "Unexpected AST element: %O" y
                )

        runner (Result.Ok []) nodes
        |> Result.map List.rev


    let private dropEmptyBlocks template =
        template
        |> List.filter(function
            | Str' ""
            | Neo' ""
            | NeoSubstitute' ""
            | NeoInclude' ""
            | NeoIncludeView' ""
            | NeoIfElseTemplate' { condition = "" } -> false
            | _                                    -> true
            )

    let private dropEmptyIfBranchBody template =
        template
        |> List.filter(function
            | NeoIfElseTemplate' { ifBranchBody = []; elseBranchBody = None } -> false
            | _                                                              -> true
            )
        |> List.map(function
            | NeoIfElseTemplate' { condition = c; ifBranchBody = []; elseBranchBody = Some(y) }
                -> NeoIfElseTemplate' { condition = sprintf "!(%s)" c; ifBranchBody = y; elseBranchBody = None }
            | x -> x
            )

    type private FoldIfsAcc = { output: Template'; acc: Template' }

    /// <summary>
    /// Groups low level AST nodes from linear sequence into hierarchical one according to semantics of
    /// IF conditional rendering feature.
    /// </summary>
    let private foldIfs templates: Result<Template', string> = result {
        let ok = Result.Ok
        let error = Result.Error

        let collapseConditionalBody (res: FoldIfsAcc): Result<FoldIfsAcc, string> = result {
            let accLength = res.acc |> List.length
            let! closestIfIdx = 
                match List.tryFindIndex (function BeginOfConditionalTemplate' _ -> true | _ -> false) res.acc with
                | Some n -> ok n
                | None   -> error "No matching opening IF for closing tag: <% } %>"

            let closestElseIdx = 
                List.tryFindIndex (function ElseBranchOfConditionalTemplateDelimiter' -> true | _ -> false) res.acc
                |> Option.defaultValue Int32.MaxValue

            let qtyOfElementsToClosestIf = closestIfIdx + 1

            let! ifElseNode = 
                if closestElseIdx < closestIfIdx then
                    let elseBranchContentRaw = List.rev (List.take closestElseIdx res.acc)
                    let elseBranchContent = if List.isEmpty elseBranchContentRaw then None else Some elseBranchContentRaw

                    let listChunk =
                        res.acc
                        |> List.skip (closestElseIdx + 1)
                        |> List.take (closestIfIdx - closestElseIdx)
                        |> List.rev

                    match listChunk with
                    |(BeginOfConditionalTemplate' condition) :: ifBranchContent ->
                        ok <| NeoIfElseTemplate' { condition = condition; ifBranchBody = ifBranchContent; elseBranchBody = elseBranchContent }
                    | _                                                         ->
                        error "No BeginOfConditionalTemplate node found."
                else
                    match List.rev(List.take qtyOfElementsToClosestIf res.acc) with
                    |(BeginOfConditionalTemplate' condition) :: rest ->
                        ok <| NeoIfElseTemplate' { condition = condition; ifBranchBody = rest; elseBranchBody = None }
                    | _                                              ->
                        error "No BeginOfConditionalTemplate node found."

            return!
                if accLength > qtyOfElementsToClosestIf then
                    ok { res with acc = ifElseNode :: (List.skip qtyOfElementsToClosestIf res.acc) }
                else
                    ok { output = ifElseNode :: res.output; acc = List.skip qtyOfElementsToClosestIf res.acc }
            }

        let! { output = folded; acc = acc } =
            templates
            |> List.fold (fun (res: Result<FoldIfsAcc, string>) node ->
                res
                >>= (fun ({acc = acc; output = output} as resRec) ->
                    match node, acc with 
                    | BeginOfConditionalTemplate' _, []            -> ok { resRec with acc = [node] }
                    | BeginOfConditionalTemplate' _, _             -> ok { resRec with acc = node :: acc }
                    | Str' _, []                                   -> ok { resRec with output = node :: output }
                    | Str' _, _                                    -> ok { resRec with acc = node :: acc }
                    | EndOfConditionalTemplate', []                -> error "Not supported: Unexpected end of conditional template."
                    | EndOfConditionalTemplate', _                 -> collapseConditionalBody resRec
                    | ElseBranchOfConditionalTemplateDelimiter', _ -> ok { resRec with acc = node :: acc }
                    | _, []                                        -> ok { resRec with output = node :: output }
                    | _, _                                         -> ok { resRec with acc = node :: acc }
                )
            ) (ok { output = []; acc = [] })

        return!
            if List.isEmpty acc then
                folded |> List.rev |> Result.Ok
            else
                error "Not all nodes were processed during collapsing of AST blocks."
    }
    
    let private fold =
        dropEmptyBlocks
        >> foldIfs
        >> Result.map dropEmptyIfBranchBody

    /// <summary>
    /// The main parser being used for parsing of template.
    /// </summary>
    let templateParser: Parser<Result<Template, string>, unit> = 
        many((attempt neoParser) <|> (notEmpty (strBeforeNeoCustomizationParser <|> strBeforeEos)))
        .>> eof
        |>> (fold >> Result.bind toTemplate)

    /// <summary>
    /// Runs the <see cref="TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserApi.templateParser" /> on specified string
    /// </summary>
    let runParserOnString string : Result<Template, string> =
        match run templateParser string with
        | Success(result, _, _)   -> result
        | Failure(errorMsg, _, _) -> Result.Error errorMsg
