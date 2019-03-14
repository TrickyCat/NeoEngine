namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests.Common

open TrickyCat.Text.TemplateEngines.NeoEngine.Errors
open NUnit.Framework

module ErrorsCommon2 = 

    type JsErrorDataMeta = {
        titleSet        : bool
        failingStringSet: bool
        errorMessageSet : bool
    }

    type JsMetaError = {
        meta: JsErrorDataMeta
        error: JsError
    }

    type RunnerErrorMeta = {
        includeNameSet : bool
    }

    type RunnerMetaError = {
        meta: RunnerErrorMeta
        error: RunnerError
    }

    type ParserErrorMeta = {
        messageSet: bool
    }

    

    type MetaError<'a, 'b> = {
        meta: 'a
        error: 'b
    }

    type ParserMetaError = MetaError<ParserErrorMeta, ParserError>

    let fail s = s |> AssertionException |> raise

    let valueWasSet = Option.map (fun _ -> true) >> Option.defaultValue false

    let checkString shouldCheck errorMsg expected (actual: string) =
        if shouldCheck && not <| actual.Contains expected then
            fail <| errorMsg()


    let checkTitle shouldCheck generatedTitle title =
        checkString shouldCheck
            (fun () -> sprintf "Error's title does not contain expected text.\nExpected: %s\nActual: %s" generatedTitle title)
            generatedTitle title


    let checkFailingString shouldCheck generatedFailingString failingString =
        checkString shouldCheck
            (fun () -> sprintf "Error's failing string value does not contain expected text.\nExpected: %s\nActual: %s" generatedFailingString failingString)
            generatedFailingString failingString


    let checkErrorMessage shouldCheck generatedErrorMessage errorMessage =
        checkString shouldCheck
            (fun () -> sprintf "Error's message does not contain expected text.\nExpected: %s\nActual: %s" generatedErrorMessage errorMessage)
            generatedErrorMessage errorMessage


    let compareJsErrorData meta genJsErrorData actualJsErrorData =
        checkTitle          meta.titleSet          genJsErrorData.title          actualJsErrorData.title
        checkFailingString  meta.failingStringSet  genJsErrorData.failingString  actualJsErrorData.failingString
        checkErrorMessage   meta.errorMessageSet   genJsErrorData.failingString  actualJsErrorData.failingString

    let compareJsErrors { JsMetaError.meta = meta; error = generatedError } actualJsError =
        match generatedError, actualJsError with
        | JsReferenceError r1, JsReferenceError r2
        | JsTypeError r1,      JsTypeError r2
        | JsSyntaxError r1,    JsSyntaxError r2
        | JsRangeError r1,     JsRangeError r2
        | JsURIError r1,       JsURIError r2
        | JsError r1,          JsError r2
               -> compareJsErrorData meta r1 r2
        | _, _ -> fail <| sprintf "JS error type mismatch.\nExpected: %A.\nActual: %A" generatedError actualJsError



    let compareIncludeNotFoundErrorData meta expectedErrorData actualErrorData =
        checkString meta.includeNameSet
            (fun () -> sprintf "Missing include name mismatch.\nExpected: %s\nActual: %s" expectedErrorData actualErrorData)
            expectedErrorData actualErrorData


    let compareRunnerErrors { RunnerMetaError.meta = meta; error = expectedError } actualRunnerError =
        match expectedError, actualRunnerError with
        | IncludeNotFound n1, IncludeNotFound n2 -> compareIncludeNotFoundErrorData meta n1 n2
        ()

    //let shouldRenderFailWith ({ error = generatedError} as metaError) (renderResult: Result<string, EngineError>) =
    //    renderResult
    //    |> Result.map (fun x -> fail <| sprintf "Unexpected rendering success result.\nExpected: %A\nActual: %A" generatedError x)
    //    |> Result.mapError (compareJsErrors metaError)
    //    |> ignore


open ErrorsCommon2

type ErrorsHelper2 =

    static member jsError (kind, ?title, ?failingString, ?errorMessage) =
        {
            JsMetaError.meta = {
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

    static member includeNotFound (?includeName: string) =
        {
            RunnerMetaError.meta = {
                includeNameSet = valueWasSet includeName
            }
            error = IncludeNotFound <| defaultArg includeName ""
        }
