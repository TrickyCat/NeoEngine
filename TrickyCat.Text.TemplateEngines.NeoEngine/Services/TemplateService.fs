﻿namespace TrickyCat.Text.TemplateEngines.NeoEngine.Services

open System.Collections.Generic
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserApi
open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.TemplateRunner
open TrickyCat.Text.TemplateEngines.NeoEngine.ResultCommon

type ITemplateService =
    abstract RenderTemplateString:
        template: string
        -> globalScopeCodeBlocks: string seq
        -> includes: IReadOnlyDictionary<string, string>
        -> contextData: KeyValuePair<string, string> seq
        -> Result<string, string>


type TemplateService() = 
    interface ITemplateService with
        member __.RenderTemplateString template globals includes context = 
                template
                |> runParserOnString
                >>= renderTemplate' globals includes context

