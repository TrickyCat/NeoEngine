namespace TrickyCat.Text.TemplateEngines.NeoEngine.CliRunner

open CommandLine
open TrickyCat.Text.TemplateEngines.NeoEngine.ResultCommon
open IoHelpers
open TrickyCat.Text.TemplateEngines.NeoEngine.Services
open System
open System.Diagnostics
open TrickyCat.Text.TemplateEngines.NeoEngine.Common

module Main = 

    let private timed f a b c d =
        let watch = Stopwatch()
        watch.Start()
        let result = f a b c d
        watch.Stop()
        {| result = result; duration = watch.Elapsed |}


    let private logDuration duration =
        let color = Console.ForegroundColor
        Console.ForegroundColor <- ConsoleColor.Cyan
        printfn "Execution duration: %A" duration
        Console.ForegroundColor <- color


    let private renderSvc = TemplateService() :> ITemplateService

    let private writeToConsole suppressOutput content =
        if not suppressOutput then
            printfn "Render result:%s%s%s" nl nl content


    let private runOnFiles (options: CliOptions) : Result<unit, string> = result {
        let! template = getTemplateFromFile options.TemplateFilePath
        let! context = getContextDataFromFile options.ContextDataFilePath
        let! includes = getIncludesFromFolder options.IncludesFolderPath
        let! globals = getGlobalsFromFolder options.GlobalsFolderPath

        let timedResult = timed renderSvc.RenderTemplateString globals includes template context
        logDuration timedResult.duration
        let! result = timedResult.result |> Result.mapError toString

        writeToConsole options.SuppressOutputToConsole result
        do! writeAllTextToFile options.RenderedOutputFilePath result
        return ()
        }


    let private printError error =
        let color = Console.ForegroundColor
        Console.ForegroundColor <- ConsoleColor.Red
        printfn "%s%s" nl error
        Console.ForegroundColor <- color


    let private outputErrors (result: Result<_, string>) =
        result |> Result.mapError printError |> ignore


    let private runCli argv =
        let parseResult = Parser.Default.ParseArguments<CliOptions> argv
        ParserResultExtensions.WithParsed(parseResult, runOnFiles >> outputErrors) |> ignore


    [<EntryPoint>]
    let main argv =
        printfn ""
        runCli argv
        0
