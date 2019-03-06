namespace TrickyCat.Text.TemplateEngines.NeoEngine.Runners

open System
open TrickyCat.Text.TemplateEngines.NeoEngine.Common

module RunnerErrors =

    type JsReferenceErrorData = {
        lineNumber: int
        errorMessage: string
        failingString: string
        failingStringPointerHint: string
        }

    // JsSyntaxError
    // JsTypeError
    type RunnerError =
    | JsReferenceError of JsReferenceErrorData
    | GeneralError of string
        with
        override x.ToString() =
            match x with
            | JsReferenceError r -> sprintf "JS Reference Error: %s%sJS Block Line: %d%sFailing string:%s%s%s%s" r.errorMessage nl r.lineNumber nl nl r.failingString nl r.failingStringPointerHint
            | GeneralError s   -> sprintf "Error: %s" s


    let private referenceErrorToken = "ReferenceError"
    let private referenceErrorData =
        function
        | null            -> None
        | (error: string) ->
            let lines = error.Split([| "\r\n"; "\n" |], StringSplitOptions.RemoveEmptyEntries)

            if lines.Length > 3 && lines.[4].StartsWith referenceErrorToken then
                let lineNumber = lines.[1].Split ':' |> Array.last |> int
                let failingString = lines.[2]
                let hint = lines.[3]
                let errorMessage = lines.[4].Split ':' |> Array.last |> trim
                Some { lineNumber = lineNumber; errorMessage = errorMessage; failingString = failingString; failingStringPointerHint = hint }
            else
                None

    let private referenceError = referenceErrorData >> Option.map JsReferenceError

    let private generalError = GeneralError >> Some

    let private errorBuilders = seq {
        yield referenceError
        yield generalError
    }

    let runnerError error =
        errorBuilders
        |> Seq.map (fun f -> f error)
        |> Seq.find Option.isSome
        |> (fun o -> o.Value)


