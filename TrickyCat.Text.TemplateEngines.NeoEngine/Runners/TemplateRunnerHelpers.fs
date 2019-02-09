namespace TrickyCat.Text.TemplateEngines.NeoEngine.Runners

open System.Reflection
open System.IO

module TemplateRunnerHelpers =

    let globalFormatDate () =
        let assembly = Assembly.GetExecutingAssembly()
        let resourceName = "TrickyCat.Text.TemplateEngines.NeoEngine.JS.Global_Scope.formatDate.js"
        use stream = assembly.GetManifestResourceStream(resourceName)
        use reader = new StreamReader(stream)
        let result = reader.ReadToEnd()
        result

    let defaultGlobals = seq {
        yield globalFormatDate()
        }