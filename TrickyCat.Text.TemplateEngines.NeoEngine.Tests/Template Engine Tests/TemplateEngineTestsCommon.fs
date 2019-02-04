namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests

open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.TemplateRunner
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserApi
open TrickyCat.Text.TemplateEngines.NeoEngine.ResultCommon
open System.Collections.Generic
open FsUnitTyped
open System

module ``Template Engine Tests Common`` = 
    let emptyContext  = Map.empty<string, string>
    let emptyIncludes = Map.empty<string, string> :> IReadOnlyDictionary<string, string>
    let emptyGlobals  = Seq.empty<string>

    let renderTemplate globals includes context templateString =
            templateString
            |> runParserOnString
            >>= renderTemplate' globals includes context
            |> (function
                | Ok x    -> x
                | Error e -> failwithf "Rendering Error In Test: %s" e
            )

    let runEngineOnMalformedInputs globals includes context templateString =
        shouldFail<Exception> (fun () ->
            renderTemplate globals includes context templateString
            |> ignore
        )
        
