namespace TrickyCat.Text.TemplateEngines.NeoEngine.CliRunner

open CommandLine
open TrickyCat.Text.TemplateEngines.NeoEngine.ResultCommon
open IoHelpers
open TrickyCat.Text.TemplateEngines.NeoEngine.Services

module Main = 

    let renderSvc = TemplateService() :> ITemplateService

    let writeToConsole suppressOutput content =
        if not suppressOutput then
            printfn "Render result:%s%s%s" nl nl content


    let runOnFiles (options: CliOptions): Result<unit, string> = result {
        let! template = getTemplateFromFile options.TemplateFilePath
        let! context = getContextDataFromFile options.ContextDataFilePath
        let! includes = getIncludesFromFolder options.IncludesFolderPath
        let! globals = getGlobalsFromFolder options.GlobalsFolderPath

        let! renderResult = renderSvc.RenderTemplateString globals includes template context

        writeToConsole options.SuppressOutputToConsole renderResult
        do! writeAllTextToFile options.RenderedOutputFilePath renderResult
        }


    let outputErrors (result: Result<_, string>) =
        result |> Result.mapError (printfn "%s") |> ignore


    let runCli argv =
        let parseResult = Parser.Default.ParseArguments<CliOptions> argv
        ParserResultExtensions.WithParsed(parseResult, runOnFiles >> outputErrors) |> ignore


    [<EntryPoint>]
    let main argv = 
        runCli argv
        0
