namespace TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.FSharp

open System

module Main =

    [<EntryPoint>]
    let main _ = 
        EventsQueue.startQ()
        SimpleEventsProcessor.startProcessor()
        Console.ReadLine() |> ignore
        0
