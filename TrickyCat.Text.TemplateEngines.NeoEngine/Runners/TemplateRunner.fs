namespace TrickyCat.Text.TemplateEngines.NeoEngine.Runners

open TrickyCat.Text.TemplateEngines.NeoEngine.Common
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserCore
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserApi
open TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters.InterpreterBase
open TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters.EdgeJsInterpreter
open System.Text
open System.Collections.Generic

module TemplateRunner =

    let rec private processTemplateNode
        (sb: StringBuilder, interpreter: IInterpreter, includes: IReadOnlyDictionary<string, string>) (templateNode: TemplateNode') =

        match templateNode with // use 'function'?
        | Str s -> sb.Append s
        | Neo s -> s |> (interpreter.Run >> sb.Append)
    
        | NeoInclude s | NeoIncludeValue s -> sb.Append(sprintf "Unsupported include: %s" s)
        | NeoIncludeView viewName ->
            let (viewFound, viewTmplStr) = includes.TryGetValue(viewName)
            if not viewFound then sb.Append(sprintf "Include not found: %s." viewName) else

            match runParserOnString viewTmplStr with
            | Error e  -> sb.Append(sprintf "Include parse failed.\nInclude: %s.\nError: %s." viewName e)
            | Ok nodes -> nodes |> List.fold (fun sb n -> processTemplateNode (sb, interpreter, includes) n) sb

        | NeoSubstitute s ->
            s 
            |> sprintf "function evalFn() { try { return ((%s) || '').toString(); } catch (exn) { return ''; }}; evalFn();"
            |> interpreter.Eval
            |> sb.Append

        | NeoIfElseTemplate {condition = c; ifBranchBody = bs; elseBranchBody = elseBranchBody} ->
            let booleanCondition = sprintf "!!(%s)" c
            match interpreter.Eval<bool> booleanCondition with
            | None -> sb
            | Some conditionResult ->
                if conditionResult then
                    bs |> List.fold (fun sb n -> processTemplateNode (sb, interpreter, includes) n) sb
                else
                    match elseBranchBody with
                    | None -> sb
                    | Some es -> es |> List.fold (fun sb n -> processTemplateNode (sb, interpreter, includes) n) sb
        
        // TODO: remove in favor of specific matches for better correctness and type safety
        | _ -> sb

    let private initInterpreterEnvironment (interpreter: IInterpreter) (env: KeyValuePair<string, string> seq) =
        env
        |> Seq.iter(fun x -> interpreter.Run(sprintf "%s = %s" x.Key x.Value))

    let private processTemplateNode' x y =
        processTemplateNode x y |> ignore
        x

    let private fst' (x, _, _) = x

    let renderTemplate (interpreter: IInterpreter) (globals: string seq) (includes: IReadOnlyDictionary<string, string>) (env: KeyValuePair<string, string> seq) (template: Template) =
        try
            globals |> Seq.iter interpreter.Run
            initInterpreterEnvironment interpreter env

            template
            |> List.fold processTemplateNode' (new StringBuilder(), interpreter, includes)
            |> fst'
            |> (fun sb -> sb.ToString())
            |> Ok
        with
        | e -> e |> fullMessage |> Error

    let renderTemplateWithDefaultInterpreter (globals: string seq) (includes: IReadOnlyDictionary<string, string>) (env: KeyValuePair<string, string> seq) (template: Template) =
        use interpreter = new EdgeJsInterpreter() :> IInterpreter
        renderTemplate interpreter globals includes env template

    let renderTemplate' = renderTemplateWithDefaultInterpreter