namespace TrickyCat.Text.TemplateEngines.NeoEngine.CliRunner

open System
open System.IO
open TrickyCat.Text.TemplateEngines.NeoEngine.Common

module IoHelpers =

    let nl = Environment.NewLine

    let getTemplate (templateFilePath: string) : Result<string, string> =
        if File.Exists templateFilePath then
            try
                templateFilePath |> File.ReadAllText |> Ok
            with
            | e ->
                e
                |> fullMessage
                |> sprintf "Error occured while processing template file: %s%sError: %s" templateFilePath nl
                |> Error
        else
            Error <| sprintf "Template file does not exist: %s" templateFilePath

