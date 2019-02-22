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

    let rec private processTemplate' (sb: StringBuilder, interpreter: IInterpreter, includes: IReadOnlyDictionary<string, string>) =
             List.fold (fun state node -> state |> Result.bind (fun sb -> processTemplateNode (sb, interpreter, includes) node)) (Ok sb)

    and private processTemplateNode
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
                |> Result.bind (processTemplate'(sb, interpreter, includes))

        | NeoSubstitute s ->
            s
            |> sprintf "(() => { try { return ((%s) || '').toString(); } catch (exn) { return ''; }})();"
            |> interpreter.Eval
            |> Result.map sb.Append

        | NeoIfElseTemplate {condition = condition; ifBranchBody = ifBranchBody; elseBranchBody = maybeElseBranchBody} ->
            condition
            |> sprintf "!!(%s)"
            |> interpreter.Eval<bool>
            |> Result.map (function
                | true  -> ifBranchBody
                | false -> Option.defaultValue [] maybeElseBranchBody
            )
            |> Result.bind (processTemplate'(sb, interpreter, includes))


    let private initInterpreterEnvironmentWithGlobals (interpreter: IInterpreter) (globals: string seq) =
        S.Join(S.Empty, globals)
        |> interpreter.Run


    let private initInterpreterEnvironmentWithContextValues (interpreter: IInterpreter) =
        Seq.fold (fun (sb: StringBuilder) (kvp: KeyValuePair<string, string>) -> sprintf "var %s = %s;" kvp.Key kvp.Value |> sb.AppendLine) (new StringBuilder())
        >> toString
        >> interpreter.Run


    let processTemplate x = processTemplate' x >> Result.map toString


    let renderTemplate 
        (interpreter: IInterpreter) (globals: string seq) (includes: IReadOnlyDictionary<string, string>) (context: KeyValuePair<string, string> seq)
        (template: Template): Result<string, string> =
        result {
            do! initInterpreterEnvironmentWithGlobals interpreter globals
            do! initInterpreterEnvironmentWithContextValues interpreter context
            return! processTemplate (new StringBuilder(), interpreter, includes) template
        }


    let renderTemplateWithDefaultInterpreter (globals: string seq) (includes: IReadOnlyDictionary<string, string>) (context: KeyValuePair<string, string> seq)
        (template: Template): Result<string, string> =
        use interpreter = new EdgeJsInterpreter()
        renderTemplate interpreter globals includes context template
