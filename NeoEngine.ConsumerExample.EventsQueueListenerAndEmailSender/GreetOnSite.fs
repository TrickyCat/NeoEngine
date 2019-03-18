namespace TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.FSharp

open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.Helpers
open System.Collections.Generic
open Common

module GreetOnSite =

    let private template = """
        Hello <%= name %>!
        You're <%= age %> years old.
        And we welcome you on our site!
        <%= formatDate(new Date(), '%4Y') %>
    """

    let private templateRenderer: KeyValuePair<string, string> seq -> Result<string, _> =
        renderSvc.RenderTemplateString emptyGlobals emptyIncludes template

    let renderWithContext = templateRenderer
