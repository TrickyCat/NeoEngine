namespace TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.FSharp

open System

module Main =

    let emailSendingProcessorExample() =
        EventsQueue.startQ()
        SimpleEventsProcessor.startProcessor()

    [<EntryPoint>]
    let main _ = 
        emailSendingProcessorExample()
        Console.ReadLine() |> ignore
        0
