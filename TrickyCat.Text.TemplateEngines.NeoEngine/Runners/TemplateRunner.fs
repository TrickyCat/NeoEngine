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

    /// <summary>
    /// Handles execution of nodes from template's AST.
    /// </summary>
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

    /// <summary>
    /// Initialize the script execution environment inside interpreter session with with specified globals.
    /// </summary>
    let private initInterpreterEnvironmentWithGlobals (interpreter: IInterpreter) (globals: string seq) =
        S.Join(";\n", globals)
        |> interpreter.Run

    /// <summary>
    /// Initialize the script execution environment inside interpreter session with with specified context values.
    /// </summary>
    let private initInterpreterEnvironmentWithContextValues (interpreter: IInterpreter) =
        Seq.fold (fun (sb: StringBuilder) (kvp: KeyValuePair<string, string>) -> sprintf "var %s = %s;" kvp.Key kvp.Value |> sb.AppendLine) (new StringBuilder())
        >> toString
        >> interpreter.Run


    let private processTemplate x = processTemplate' x >> Result.map toString


    /// <summary>
    /// Renders a template in environment specified by globals with provided includes lookup and context data.
    /// JS scripts from customizations are executed using the specified interpreter instance.
    /// </summary>
    /// <param name="interpreter">
    /// The interpreter which is used for execution of JS.
    /// <see cref="TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters.InterpreterBase.IInterpreter" />
    /// </param>
    /// <param name="globals">
    /// Sequence of strings which represent JS script files which define global execution scope for scripts inside templates and includes.
    /// Not null.
    /// </param>
    /// <param name="includes">
    /// A lookup dictionary for resolution of includes being referenced from the template. Syntactically they are also templates.
    /// Not null.
    /// </param>
    /// <param name="context">
    /// A sequence of named values available for lookup\reference from template or include customization block.
    /// Not null.
    /// </param>
    /// <param name="template">
    /// Template's AST.
    /// </param>
    /// <returns>Result value with rendered template string in case of success or with the error string in case of failure.</returns>
    /// <seealso cref="Microsoft.FSharp.Core.FSharpResult{System.String,System.String}"/>
    let renderTemplate 
        (interpreter: IInterpreter) (globals: string seq) (includes: IReadOnlyDictionary<string, string>) (context: KeyValuePair<string, string> seq)
        (template: Template): Result<string, string> =
        result {
            do! initInterpreterEnvironmentWithGlobals interpreter globals
            do! initInterpreterEnvironmentWithContextValues interpreter context
            return! processTemplate (new StringBuilder(), interpreter, includes) template
        }

    /// <summary>
    /// Renders a template in environment specified by globals with provided includes lookup and context data.
    /// JS scripts from customizations are executed using the <see cref="TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters.EdgeJsInterpreter.EdgeJsInterpreter" /> interpreter instance.
    /// </summary>
    /// <param name="interpreter">
    /// The interpreter which is used for execution of JS.
    /// <see cref="TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters.InterpreterBase.IInterpreter" />
    /// </param>
    /// <param name="globals">
    /// Sequence of strings which represent JS script files which define global execution scope for scripts inside templates and includes.
    /// Not null.
    /// </param>
    /// <param name="includes">
    /// A lookup dictionary for resolution of includes being referenced from the template. Syntactically they are also templates.
    /// Not null.
    /// </param>
    /// <param name="context">
    /// A sequence of named values available for lookup\reference from template or include customization block.
    /// Not null.
    /// </param>
    /// <param name="template">
    /// Template's AST.
    /// </param>
    /// <returns>Result value with rendered template string in case of success or with the error string in case of failure.</returns>
    /// <seealso cref="Microsoft.FSharp.Core.FSharpResult{System.String,System.String}"/>
    let renderTemplateWithDefaultInterpreter (globals: string seq) (includes: IReadOnlyDictionary<string, string>) (context: KeyValuePair<string, string> seq)
        (template: Template): Result<string, string> =
        use interpreter = new EdgeJsInterpreter()
        renderTemplate interpreter globals includes context template
