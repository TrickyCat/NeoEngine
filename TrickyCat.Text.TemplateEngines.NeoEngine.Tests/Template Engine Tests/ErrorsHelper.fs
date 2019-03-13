namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests.Common

open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.RunnerErrors
open NUnit.Framework

module ErrorsCommon = 

    type JsErrorMeta = {
        titleSet        : bool
        failingStringSet: bool
        errorMessageSet : bool
    }

    let valueWasSet = Option.map (fun _ -> true) >> Option.defaultValue false

    let titleIsOk meta genTitle (title: string) =
        if meta.titleSet then title.Contains(genTitle) else true

    let failingStringIsOk meta genFailingString (failingString: string) =
        if meta.failingStringSet then failingString.Contains(genFailingString) else true

    let errorMessageIsOk meta genErrorMessage (errorMessage: string) =
        if meta.errorMessageSet then errorMessage.Contains(genErrorMessage) else true

    let compareErrorData meta genErrorData errorData =
        titleIsOk meta genErrorData.title errorData.title
        && failingStringIsOk meta genErrorData.failingString errorData.failingString
        && errorMessageIsOk meta genErrorData.failingString errorData.failingString

    let fail s = s |> AssertionException |> raise

    let compareErrors (meta, generatedError) error =
        match generatedError, error with
        | JsReferenceError r1, JsReferenceError r2
        | JsTypeError r1,      JsTypeError r2
        | JsSyntaxError r1,    JsSyntaxError r2
        | JsRangeError r1,     JsRangeError r2
        | JsURIError r1,       JsURIError r2
        | JsError r1,          JsError r2
            -> compareErrorData meta r1 r2
        | _ -> fail <| sprintf "Rendering error type mismatch.\nExpected: %A.\nActual: %A" generatedError error

    let shouldRenderFailWith ((_, generatedError) as metaError) (renderResult: Result<string, RunnerError>) =
        renderResult
        |> Result.map (fun x -> fail <| sprintf "Unexpected rendering success result.\nExpected: %A\nActual: %A" generatedError x)
        |> Result.mapError (compareErrors metaError)
        |> ignore


open ErrorsCommon

type ErrorsHelper =

    static member error (kind, ?``title?``, ?``failingString?``, ?errorMessage) =
        {
            titleSet         = valueWasSet ``title?``
            failingStringSet = valueWasSet ``failingString?``
            errorMessageSet  = valueWasSet errorMessage
        },
        kind
            {
                title                    = defaultArg ``title?`` ""
                lineNumber               = 0
                errorMessage             = errorMessage
                failingString            = defaultArg ``failingString?`` ""
                failingStringPointerHint = ""
                stackTrace               = ""
            }
