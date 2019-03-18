namespace TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.FSharp

open System
open EventsQueue
open Common
open FakeEmailSender
open TrickyCat.Text.TemplateEngines.NeoEngine.Common

module SimpleEventsProcessor =

    let private eventsProcessor: Async<unit> =
        let rnd = Random()

        let toContext = function
            | PasswordResetEvent { name = name; newPassword = newPassword } ->
                {|
                    event = EventType.PasswordResetEvent
                    ctx = mempty
                        .Add("name", name)
                        .Add("newPassword", newPassword) :> CtxData
                |}

            | GreetOnSiteEvent { name = name; age = age } ->
                {|
                    event = EventType.GreetOnSiteEvent
                    ctx = mempty
                        .Add("name", name)
                        .Add("age", string age) :> CtxData
                |}

        let rec loop() = async {
            do! Async.Sleep <| rnd.Next(1000, 5000)
            if q.Count > 1 then
                q.Dequeue()
                |> toContext
                |> (fun payload -> 
                    match payload.event with
                    | EventType.PasswordResetEvent -> PasswordReset.renderWithContext payload.ctx
                    | EventType.GreetOnSiteEvent   -> GreetOnSite.renderWithContext payload.ctx
                )
                |> Result.map (fun msg -> sendEmail msg)
                |> Result.mapError (fun e ->
                    let color = Console.ForegroundColor
                    Console.ForegroundColor <- ConsoleColor.Red
                    e |> toString |> printfn "ERROR: %s"
                    Console.ForegroundColor <- color
                    )
                |> ignore

            return! loop()
            }

        loop() 

    let startProcessor () = eventsProcessor |> Async.Start
