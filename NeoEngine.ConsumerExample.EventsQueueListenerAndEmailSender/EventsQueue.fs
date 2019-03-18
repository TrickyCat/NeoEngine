namespace TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.FSharp

open System
open System.Collections.Generic

module EventsQueue =

    type EventType = PasswordResetEvent | GreetOnSiteEvent

    type PasswordResetEvent = {
        name: string
        newPassword: string
    }

    type GreetOnSiteEvent = {
        name: string
        age: int
    }

    type QEvent =
        | PasswordResetEvent of PasswordResetEvent
        | GreetOnSiteEvent of GreetOnSiteEvent

    let q = Queue<QEvent>()

    let private generatorAgent: Async<unit> =
        let rnd = Random()

        let names = [| "John"; "Bill"; "Bob"; "Adam"; "Patrick"; "Suzy"; "Mary"; "Jane" |]
        let ages = [| 19..40 |]
        let asJsString = sprintf "'%s'"

        let getName() = names.[rnd.Next(0, names.Length)] |> asJsString
        let getAge() = ages.[rnd.Next(0, ages.Length)]
        let randomStr minLength maxLength =
            let length = rnd.Next(minLength, maxLength + 1)
            new String(Array.init length (fun _ -> rnd.Next(97, 123) |> char))
            |> asJsString

        let getEvent () =
            if rnd.NextDouble() > 0.5 then
                PasswordResetEvent { name = getName(); newPassword = randomStr 20 30 }
            else
                GreetOnSiteEvent { name = getName(); age = getAge() }

        let rec loop() = async {
            do! Async.Sleep <| rnd.Next(1000, 5000)
            if q.Count < 100 then
                let event = getEvent()
                q.Enqueue event
            return! loop()
            }
        loop() 

    let startQ () = generatorAgent |> Async.Start
