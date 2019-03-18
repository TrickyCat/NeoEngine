namespace TrickyCat.Text.TemplateEngines.NeoEngine.Services

open System.Collections.Generic
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserApi
open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.TemplateRunner
open TrickyCat.Text.TemplateEngines.NeoEngine.ResultCommon
open TrickyCat.Text.TemplateEngines.NeoEngine.Errors

type ITemplateService =
    /// <summary>
    /// Renders a template in environment specified by globals with provided includes lookup and context data.
    /// </summary>
    /// <param name="globals">
    /// Strings which represent JS script files which define global execution scope for scripts inside templates and includes.
    /// Not null.
    /// </param>
    /// <param name="includes">
    /// A lookup dictionary for resolution of includes being referenced from the template. Syntactically they are also templates.
    /// Not null.
    /// </param>
    /// <param name="template">
    /// A string which alongside the string literals may or may not contain customization blocks.
    /// Not null.
    /// </param>
    /// <param name="context">
    /// A sequence of named values available for lookup\reference from template or include customization block.
    /// Not null.
    /// </param>
    /// <returns>Result value with rendered template string in case of success or with the error in case of failure.</returns>
    /// <seealso cref="Microsoft.FSharp.Core.FSharpResult{System.String,TrickyCat.Text.TemplateEngines.NeoEngine.Runners.RunnerErrors.RunnerError}"/>
    abstract RenderTemplateString:
        globals: string seq
        -> includes: IReadOnlyDictionary<string, string>
        -> template: string
        -> context: KeyValuePair<string, string> seq
        -> Result<string, EngineError>


type TemplateService() = 
    interface ITemplateService with
        member __.RenderTemplateString globals includes template context = 
            template
            |> runParserOnString
            >>= renderTemplateWithDefaultInterpreter globals includes context
