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

    let defaultGlobals = seq {
        yield globalFormatDate.Value
        }

    let emptyGlobals  = Seq.empty<string>
    let emptyIncludes = Map.empty<string, string> :> IReadOnlyDictionary<string, string>
    let emptyContext  = Map.empty<string, string>
