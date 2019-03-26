namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.ParserTests

open FParsec
open FsUnitTyped
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserCore
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserApi
open System
open Swensen.Unquote

#nowarn "59"
module ``Parser Tests Common`` =
    type private T = Result<Template, string>

    let emptyTemplate : TemplateNode list = []
    let okTemplate x    = (Result.Ok x) :> T
    let okEmptyTemplate = okTemplate emptyTemplate
    
    let errorTemplate e = (Result.Error e) :> T
    let errorTemplateUnexpectedLowNode: TemplateNode' -> T = sprintf "Unexpected AST element: %O" >> errorTemplate

    let runParser parser str =
        match run parser str with
        | Success(result, _, _)   -> result
        | Failure(errorMsg, _, _) -> failwithf "Parsing Error: %s" errorMsg


    let runParserOnSuccessfulData parserUnderTest (templateString, expected) =
        expected =! runParser parserUnderTest templateString


    let runParserOnUnsupportedData parserUnderTest templateString =
        shouldFail<Exception> (fun () ->
            runParser parserUnderTest templateString
            |> ignore
        )