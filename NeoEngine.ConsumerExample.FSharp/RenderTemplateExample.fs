namespace TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.FSharp

open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.TemplateRunnerHelpers
open TrickyCat.Text.TemplateEngines.NeoEngine.Services
open System.Collections.Generic

module RenderTemplateExample =

    let mempty = Map.empty<string, string>
    let renderSvc = TemplateService() :> ITemplateService

    let template = """
        Hello <%= name %>
        You're <%= age %> years old.
        <% function f() { return "Hello from f"; } %>
        world
        <%= f() %>
        <%@ include view='myInclude' %>
        <%= g() %>
        <%= formatDate(new Date(), '%4Y')  %>
    """

    let globals = defaultGlobals

    let includes =
        mempty
          .Add("myInclude", """
            <% function g() { return 'Hello from g'; } %>
          """)

    type private t = KeyValuePair<string, string> seq
    let invocationContexts = seq {
        yield mempty.Add("name", "'Bill'").Add("age", "11") :> t
        yield mempty.Add("name", "'John'").Add("age", "22") :> t
        yield mempty.Add("name", "'Adam'").Add("age", "33") :> t
        }

    let templateRenderer = renderSvc.RenderTemplateString globals includes template

    let renderTemplateMultipleTimesExample (ctxs: KeyValuePair<string, string> seq seq) =
        ctxs
        |> Seq.map templateRenderer
        |> Seq.map (function Ok s -> s | Error e -> sprintf "Error: %s" e)
        |> Seq.iter (printfn "%s")

    let run () = renderTemplateMultipleTimesExample invocationContexts
