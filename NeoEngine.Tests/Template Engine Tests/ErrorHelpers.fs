namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests.Common

open TrickyCat.Text.TemplateEngines.NeoEngine.Errors
open NUnit.Framework

module Errors = 

    type JsErrorDataMeta = {
        titleSet        : bool
        failingStringSet: bool
        errorMessageSet : bool
    }

    type RunnerErrorMeta = {
        includeNameSet : bool
    }

    type ParserErrorMeta = {
        messageSet: bool
    }

    type GeneralErrorMeta = {
        messageSet: bool
    }

    type MetaError<'a, 'b> = {
        meta:  'a
        error: 'b
    }

    type JsMetaError = MetaError<JsErrorDataMeta, JsError>

    type RunnerMetaError = MetaError<RunnerErrorMeta, RunnerError>

    type ParserMetaError = MetaError<ParserErrorMeta, ParserError>

    type GeneralMetaError = MetaError<GeneralErrorMeta, string>

    type TestError =
    | JsMetaError of JsMetaError
    | RunnerMetaError of RunnerMetaError
    | ParserMetaError of ParserMetaError
    | GeneralMetaError of GeneralMetaError

    let fail s = s |> AssertionException |> raise

    let valueWasSet = Option.map (fun _ -> true) >> Option.defaultValue false

    let checkString shouldCheck errorMsg expected (actual: string) =
        if shouldCheck && not <| actual.Contains expected then
            fail <| errorMsg()


    let checkJsErrorData meta genJsErrorData actualJsErrorData =

        checkString meta.titleSet
            (fun () -> sprintf "Error's title does not contain expected text.\nExpected: %s\nActual: %s" genJsErrorData.title actualJsErrorData.title)
            genJsErrorData.title actualJsErrorData.title


        checkString meta.failingStringSet
            (fun () -> sprintf "Error's failing string value does not contain expected text.\nExpected: %s\nActual: %s" genJsErrorData.failingString actualJsErrorData.failingString)
            genJsErrorData.failingString actualJsErrorData.failingString


        if meta.errorMessageSet then
            match genJsErrorData.errorMessage, actualJsErrorData.errorMessage with
            | Some expected, Some actual ->
                checkString meta.errorMessageSet
                    (fun () -> sprintf "Error's message does not contain expected text.\nExpected: %A\nActual: %A" genJsErrorData.errorMessage actualJsErrorData.errorMessage)
                    expected actual

            | None, None                 -> ()
            | _                          ->
                fail <| sprintf "JS error message mismatch.\nExpected: %A\nActual: %A" genJsErrorData.errorMessage actualJsErrorData.errorMessage


    let checkScriptErrors { JsMetaError.meta = meta; error = generatedError } actualJsError =
        match generatedError, actualJsError with
        | JsReferenceError r1, JsReferenceError r2
        | JsTypeError r1,      JsTypeError r2
        | JsSyntaxError r1,    JsSyntaxError r2
        | JsRangeError r1,     JsRangeError r2
        | JsURIError r1,       JsURIError r2
        | JsError r1,          JsError r2
               -> checkJsErrorData meta r1 r2
        | _, _ -> fail <| sprintf "JS error type mismatch.\nExpected: %A.\nActual: %A" generatedError actualJsError



    let checkIncludeNotFoundErrorData meta expectedErrorData actualErrorData =
        checkString meta.includeNameSet
            (fun () -> sprintf "Missing include name mismatch.\nExpected: %s\nActual: %s" expectedErrorData actualErrorData)
            expectedErrorData actualErrorData


    let checkRunnerErrors { RunnerMetaError.meta = meta; error = expectedError } actualRunnerError =
        match expectedError, actualRunnerError with
        | IncludeNotFound n1, IncludeNotFound n2 -> checkIncludeNotFoundErrorData meta n1 n2


    let checkParseErrorData (meta : ParserErrorMeta) expectedErrorData actualErrorData =
        checkString meta.messageSet
            (fun () -> sprintf "Error's message does not contain expected text.\nExpected: %s\nActual: %s" expectedErrorData actualErrorData)
            expectedErrorData actualErrorData

    let checkParserErrors { ParserMetaError.meta = meta; error = expectedError } actualParserError =
        match expectedError, actualParserError with
        | ParseError p1, ParseError p2 -> checkParseErrorData meta p1 p2

    let checkGeneralErrors { GeneralMetaError.meta = meta; error = expectedError } actualError =
        checkString meta.messageSet
            (fun () -> sprintf "Error's message does not contain expected text.\nExpected: %s\nActual: %s" expectedError actualError)
            expectedError actualError


    let expectedError = function
        | JsMetaError e      -> ScriptError e.error
        | RunnerMetaError e  -> RunnerError e.error
        | ParserMetaError e  -> ParserError e.error
        | GeneralMetaError e -> GeneralError e.error


    let checkErrors expected actual =
        match expected, actual with
        | JsMetaError m,      ScriptError e  -> checkScriptErrors  m e
        | RunnerMetaError m,  RunnerError e  -> checkRunnerErrors  m e
        | ParserMetaError m,  ParserError e  -> checkParserErrors  m e
        | GeneralMetaError m, GeneralError e -> checkGeneralErrors m e
        | _, _ ->
            fail <| sprintf "Error type mismatch.\nExpected: %A\nActual: %A" (expectedError expected) actual


    let shouldFailWith testError (renderResult: Result<string, EngineError>) =
        renderResult
        |> Result.map (fun x -> fail <| sprintf "Unexpected engine success result.\nExpected: %A\nActual: %A" (expectedError testError) x)
        |> Result.mapError (checkErrors testError)
        |> ignore


    type ErrorsBuilder =

        static member jsError (kind, ?title, ?failingString, ?errorMessage) =
            JsMetaError {
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

        static member includeNotFound ?includeName =
            RunnerMetaError {
                RunnerMetaError.meta = {
                    includeNameSet = valueWasSet includeName
                }
                error = IncludeNotFound <| defaultArg includeName ""
            }

        static member parseError ?message =
            ParserMetaError {
                ParserMetaError.meta = {
                    messageSet = valueWasSet message
                }
                error = ParseError <| defaultArg message ""
            }

        static member generalError ?message =
            GeneralMetaError {
                GeneralMetaError.meta = {
                    messageSet = valueWasSet message
                }
                error = defaultArg message ""
            }

    type E = ErrorsBuilder