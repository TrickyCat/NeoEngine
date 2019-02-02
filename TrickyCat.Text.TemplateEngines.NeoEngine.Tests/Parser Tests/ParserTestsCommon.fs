namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.ParserTests

open FParsec
open FsUnitTyped
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserCore
open System

module ``Parser Tests Common`` =

    let emptyTemplate : TemplateNode' list = []

    let runParser parser str =
        match run parser str with
        | Success(result, _, _)   -> result
        | Failure(errorMsg, _, _) -> failwithf "Parsing Error: %s" errorMsg


    let runParserOnSuccessfulData parserUnderTest (templateString, expected) =
        runParser parserUnderTest templateString
        |> shouldEqual expected


    let runParserOnUnsupportedData parserUnderTest templateString =
        shouldFail<Exception> (fun () ->
            runParser parserUnderTest templateString
            |> ignore
        )