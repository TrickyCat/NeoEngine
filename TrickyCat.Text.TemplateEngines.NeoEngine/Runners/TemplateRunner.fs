namespace TrickyCat.Text.TemplateEngines.NeoEngine.Runners

open TrickyCat.Text.TemplateEngines.NeoEngine.Common
open TrickyCat.Text.TemplateEngines.NeoEngine.ResultCommon
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserCore
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserApi
open TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters.InterpreterBase
open TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters.EdgeJsInterpreter
open System.Text
open System.Collections.Generic

module TemplateRunner =

    type private S = System.String

    let rec private processTemplateNode
        (sb: StringBuilder, interpreter: IInterpreter, includes: IReadOnlyDictionary<string, string>) =
        function
        | Str s                   -> s |> sb.Append |> Ok
        | Neo s                   -> s |> interpreter.Run |> Result.map (fun _ -> sb)
        | NeoIncludeView viewName ->
            let (viewFound, viewTemplateString) = includes.TryGetValue(viewName)
            if not viewFound then
                Error <| sprintf "Include not found: %s." viewName
            else
                viewTemplateString
                |> runParserOnString
                |> Result.mapError (sprintf "Parse of include failed.\nInclude: %s.\nError: %s." viewName)
                |> Result.bind (List.fold (fun acc n ->
                        match acc with
                        | Ok sb -> processTemplateNode (sb, interpreter, includes) n
                        | _ -> acc
                    ) (Ok sb))

        | NeoSubstitute s ->
            s 
            |> sprintf "(() => { try { return ((%s) || '').toString(); } catch (exn) { return ''; }})();" // todo: remove guards
            |> interpreter.Eval
            |> Result.map sb.Append

        | NeoIfElseTemplate {condition = condition; ifBranchBody = ifBranchBody; elseBranchBody = elseBranchBody} ->
            condition
            |> sprintf "!!(%s)"
            |> interpreter.Eval<bool>
            |> Result.bind (function
                | true -> ifBranchBody |> List.fold (fun acc n ->
                    match acc with
                    | Ok sb -> processTemplateNode (sb, interpreter, includes) n
                    | _ -> acc
                            ) (Ok sb)

                | false ->
                    match elseBranchBody with
                    | None -> Ok sb
                    | Some es -> es |> List.fold (fun acc n ->
                        match acc with
                        | Ok sb -> processTemplateNode (sb, interpreter, includes) n
                        | _ -> acc
                                    ) (Ok sb)
                )

        //| _ -> Ok sb

    let private initInterpreterEnvironmentWithGlobals (interpreter: IInterpreter) (globals: string seq) =
        S.Join(S.Empty, globals)
        |> interpreter.Run


    let private initInterpreterEnvironmentWithContextValues (interpreter: IInterpreter) (env: KeyValuePair<string, string> seq) =
        env
        |> Seq.fold (fun (sb: StringBuilder) kvp -> sprintf "var %s = %s;" kvp.Key kvp.Value |> sb.AppendLine) (new StringBuilder())
        |> (toString >> interpreter.Run)


    let private processTemplateNode' x y =
        processTemplateNode x y |> ignore
        x

    let private fst' (x, _, _) = x

    let renderTemplate 
        (interpreter: IInterpreter) (globals: string seq) (includes: IReadOnlyDictionary<string, string>) (env: KeyValuePair<string, string> seq)
        (template: Template): Result<string, string> =
        try
            let r1 = initInterpreterEnvironmentWithGlobals interpreter globals
            let r2 = initInterpreterEnvironmentWithContextValues interpreter env

            template
            |> List.fold processTemplateNode' (new StringBuilder(), interpreter, includes)
            |> fst'
            |> (fun sb -> sb.ToString())
            |> Ok
        with
        | e -> e |> fullMessage |> Error

    let renderTemplateWithDefaultInterpreter (globals: string seq) (includes: IReadOnlyDictionary<string, string>) (env: KeyValuePair<string, string> seq)
        (template: Template): Result<string, string> =
        use interpreter = new EdgeJsInterpreter() :> IInterpreter
        renderTemplate interpreter globals includes env template
