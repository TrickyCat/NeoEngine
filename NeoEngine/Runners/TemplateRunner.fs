namespace TrickyCat.Text.TemplateEngines.NeoEngine.Runners

open System.Collections.Generic
open System.Text
open TrickyCat.Text.TemplateEngines.NeoEngine.Utils
open TrickyCat.Text.TemplateEngines.NeoEngine.ResultCommon
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserCore
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserApi
open TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters.InterpreterBase
open TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters.EdgeJsInterpreter
open TrickyCat.Text.TemplateEngines.NeoEngine.ExecutionResults
open TrickyCat.Text.TemplateEngines.NeoEngine.ExecutionResults.Errors

module TemplateRunner =

    type private S = System.String

    let private handleExpressionResult expression (x : obj) =
        match x with
        | :? string as s -> s |> Ok
        | :? bool as b   -> b |> sprintf "%b" |> Ok
        | :? int as i    -> i |> sprintf "%i" |> Ok
        | :? double as d -> d |> sprintf "%g" |> Ok
        | _              -> x |> sprintf "Unexpected expression result type.\nExpression: %s\nCalculated value: %A" expression |> error |> Error


    let rec private processTemplate' (sb: StringBuilder, interpreter: IInterpreter, includes: IReadOnlyDictionary<string, string>) =
             List.fold
               (fun state node -> state >>= (fun sb -> processTemplateNode (sb, interpreter, includes) node))
               (Ok sb)

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
            if viewFound then
                viewTemplateString
                |> runParserOnString
                |> Result.mapError (function
                    | ParserError (ParseError p) ->
                        p
                        |> sprintf "Parse of include failed.\nInclude: %s\nError: %s" viewName
                        |> parseError
                    | x -> x
                    )
                >>= (processTemplate'(sb, interpreter, includes))
            else
                viewName
                |> sprintf "Include not found: %s."
                |> includeNotFound
                |> Error

        | NeoSubstitute s ->
            s
            |> interpreter.Eval
            >>= handleExpressionResult s
            |> Result.map sb.Append

        | NeoIfElseTemplate {condition = condition; ifBranchBody = ifBranchBody; elseBranchBody = maybeElseBranchBody} ->
            condition
            |> sprintf "!!(%s)"
            |> interpreter.Eval<bool>
            |> Result.map (function
                | true  -> ifBranchBody
                | false -> Option.defaultValue [] maybeElseBranchBody
            )
            >>= processTemplate'(sb, interpreter, includes)

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
    /// A lookup dictionary for resolution of includes being referenced within the template and\or include. Syntactically they are also templates.
    /// Not null.
    /// </param>
    /// <param name="context">
    /// A sequence of named values available for lookup\reference from template or include customization block.
    /// Not null.
    /// </param>
    /// <param name="template">
    /// Template's AST.
    /// </param>
    /// <returns>Result value with rendered template string in case of success or with the error in case of failure.</returns>
    /// <seealso cref="Microsoft.FSharp.Core.FSharpResult{System.String,TrickyCat.Text.TemplateEngines.NeoEngine.ExecutionResults.Errors.EngineError}"/>
    let renderTemplate 
        (interpreter: IInterpreter) (globals: string seq) (includes: IReadOnlyDictionary<string, string>) (context: KeyValuePair<string, string> seq)
        (template: Template) : EngineResult =

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
    /// A lookup dictionary for resolution of includes being referenced within the template and\or include. Syntactically they are also templates.
    /// Not null.
    /// </param>
    /// <param name="context">
    /// A sequence of named values available for lookup\reference from template or include customization block.
    /// Not null.
    /// </param>
    /// <param name="template">
    /// Template's AST.
    /// </param>
    /// <returns>Result value with rendered template string in case of success or with the error in case of failure.</returns>
    /// <seealso cref="Microsoft.FSharp.Core.FSharpResult{System.String,TrickyCat.Text.TemplateEngines.NeoEngine.ExecutionResults.Errors.EngineError}"/>
    let renderTemplateWithDefaultInterpreter (globals: string seq) (includes: IReadOnlyDictionary<string, string>) (context: KeyValuePair<string, string> seq)
        (template: Template): EngineResult =
        use interpreter = new EdgeJsInterpreter()
        renderTemplate interpreter globals includes context template
