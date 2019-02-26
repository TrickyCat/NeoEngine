namespace TrickyCat.Text.TemplateEngines.NeoEngine.Runners

open System.Reflection
open System.IO
open System.Collections.Generic

module TemplateRunnerHelpers =

    let private globalFormatDate = lazy (
        let assembly = Assembly.GetExecutingAssembly()
        let resourceName = "TrickyCat.Text.TemplateEngines.NeoEngine.JS.Global_Scope.formatDate.js"
        use stream = assembly.GetManifestResourceStream(resourceName)
        use reader = new StreamReader(stream)
        let result = reader.ReadToEnd()
        result
        )

    /// Globals which include 'formatDate' function available for references from customization blocks.
    let defaultGlobals = seq {
        yield globalFormatDate.Value
        }

    /// Globals without any content aka empty globals.
    let emptyGlobals  = Seq.empty<string>

    /// Empty includes lookup.
    let emptyIncludes = Map.empty<string, string> :> IReadOnlyDictionary<string, string>

    /// Empty context.
    let emptyContext  = Map.empty<string, string>
