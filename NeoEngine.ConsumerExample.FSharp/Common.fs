namespace TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.FSharp

open TrickyCat.Text.TemplateEngines.NeoEngine.Services
open System.Collections.Generic

module Common =
    type CtxData = KeyValuePair<string, string> seq

    let mempty = Map.empty<string, string>
    let renderSvc = TemplateService() :> ITemplateService

