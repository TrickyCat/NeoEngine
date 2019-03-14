namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests.Common

open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.RunnerErrors
open NUnit.Framework

module ErrorsCommon = 

    type JsErrorMeta = {
        titleSet        : bool
        failingStringSet: bool
        errorMessageSet : bool
    }

    type MetaError = {
        meta: JsErrorMeta
        error: RunnerError
    }

    let fail s = s |> AssertionException |> raise

    let valueWasSet = Option.map (fun _ -> true) >> Option.defaultValue false

    let checkTitle meta generatedTitle (title: string) =
        if meta.titleSet && not <| title.Contains(generatedTitle) then
            fail <| sprintf "Error's title does not contain expected text.\nExpected: %s\nActual: %s" generatedTitle title


    let checkFailingString meta generatedFailingString (failingString: string) =
        if meta.failingStringSet && not <| failingString.Contains(generatedFailingString) then
            fail <| sprintf "Error's failing string value does not contain expected text.\nExpected: %s\nActual: %s" generatedFailingString failingString


    let checkErrorMessage meta generatedErrorMessage (errorMessage: string) =
        if meta.errorMessageSet && not <| errorMessage.Contains(generatedErrorMessage) then
            fail <| sprintf "Error's message does not contain expected text.\nExpected: %s\nActual: %s" generatedErrorMessage errorMessage


    let compareErrorData meta genErrorData errorData =
        checkTitle meta genErrorData.title errorData.title
        checkFailingString meta genErrorData.failingString errorData.failingString
        checkErrorMessage meta genErrorData.failingString errorData.failingString

    let compareErrors { meta = meta; error = generatedError } error =
        match generatedError, error with
        | JsReferenceError r1, JsReferenceError r2
        | JsTypeError r1,      JsTypeError r2
        | JsSyntaxError r1,    JsSyntaxError r2
        | JsRangeError r1,     JsRangeError r2
        | JsURIError r1,       JsURIError r2
        | JsError r1,          JsError r2
            -> compareErrorData meta r1 r2
        | _ -> fail <| sprintf "Rendering error type mismatch.\nExpected: %A.\nActual: %A" generatedError error

    let shouldRenderFailWith ({ error = generatedError} as metaError) (renderResult: Result<string, RunnerError>) =
        renderResult
        |> Result.map (fun x -> fail <| sprintf "Unexpected rendering success result.\nExpected: %A\nActual: %A" generatedError x)
        |> Result.mapError (compareErrors metaError)
        |> ignore


open ErrorsCommon

type ErrorsHelper =

    static member jsError (kind, ?title, ?failingString, ?errorMessage) =
        {
            meta = {
                titleSet         = valueWasSet title
                failingStringSet = valueWasSet failingString
                errorMessageSet  = valueWasSet errorMessage
            }
            error = kind
                {
                    title                    = defaultArg title ""
                    lineNumber               = 0
                    errorMessage             = errorMessage
                    failingString            = defaultArg failingString ""
                    failingStringPointerHint = ""
                    stackTrace               = ""
                }
        }
