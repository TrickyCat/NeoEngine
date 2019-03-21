namespace TrickyCat.Text.TemplateEngines.NeoEngine.ExecutionResults

open System
open TrickyCat.Text.TemplateEngines.NeoEngine.Utils

module Errors =

    type JsErrorData = {
        title:                    string
        lineNumber:               int
        errorMessage:             string option
        failingString:            string
        failingStringPointerHint: string
        stackTrace:               string
    }
        with
        override r.ToString () =
            sprintf "%s%s%sJS Block Line: %d%sFailing string:%s%s%s%s%s%sStack Trace:%s%s%s" 
                r.title
                (Option.defaultValue String.Empty (r.errorMessage |> Option.map (sprintf ": %s")))
                nl r.lineNumber nl nl nl r.failingString nl r.failingStringPointerHint nl nl nl r.stackTrace

    let private emptyJsErrorData = {
        title = "JS Error"; lineNumber = 0; errorMessage = None; failingString = ""; failingStringPointerHint = ""; stackTrace = ""
    }

    type JsError =
    | JsReferenceError of JsErrorData
    | JsTypeError      of JsErrorData
    | JsSyntaxError    of JsErrorData
    | JsRangeError     of JsErrorData
    | JsURIError       of JsErrorData
    | JsError          of JsErrorData
        with
        override x.ToString() =
            match x with
            | JsReferenceError r
            | JsTypeError r
            | JsRangeError r
            | JsError r
            | JsURIError r
            | JsSyntaxError r    -> r.ToString()


    type RunnerError =
    | IncludeNotFound of includeName: string
        with
        override x.ToString() =
            match x with
            | IncludeNotFound i -> sprintf "Include named %s not found." i


    type ParserError =
    | ParseError of message: string
        with
        override x.ToString() =
            match x with
            | ParseError p -> sprintf "Parse error:\n%s" p


    type EngineError =
    | ScriptError  of JsError
    | RunnerError  of RunnerError
    | ParserError  of ParserError
    | GeneralError of message: string
        with
        override x.ToString () =
            match x with
            | ScriptError js -> js.ToString()
            | RunnerError r  -> r.ToString()
            | ParserError p  -> p.ToString()
            | GeneralError g -> g


    let private jsErrorData errorToken title =
        function
        | null            -> None
        | (error: string) ->
            if error.Contains(sprintf "\n%s" errorToken) then
                let lines = error.Split([| "\r\n"; "\n" |], StringSplitOptions.RemoveEmptyEntries)

                if lines.Length > 2 then
                    let lineNumber = lines.[0].Split ':' |> Array.last |> int
                    let failingString = lines.[1]
                    let hint = lines.[2]

                    let errorMessage =
                        if lines.Length > 3 && lines.[3].Contains(":") then
                            lines.[3].Split ':' |> Array.last |> trim |> Some
                        else
                            None
                    let st = if lines.Length > 3 then String.Join("\n", lines.[4..] |> Array.map trim) else ""

                    Some { title = title; lineNumber = lineNumber; errorMessage = errorMessage; failingString = failingString; failingStringPointerHint = hint; stackTrace = st }
                else
                    None
            else
                None

    let private jsGeneralError s = JsError { emptyJsErrorData with errorMessage = Some s }

    let private jsErrorBuilders = seq {
        yield jsErrorData "ReferenceError" "JS Reference Error" >> Option.map JsReferenceError
        yield jsErrorData "TypeError"      "JS Type Error"      >> Option.map JsTypeError
        yield jsErrorData "SyntaxError"    "JS Syntax Error"    >> Option.map JsSyntaxError
        yield jsErrorData "RangeError"     "JS Range Error"     >> Option.map JsRangeError
        yield jsErrorData "URIError"       "JS URI Error"       >> Option.map JsURIError
        yield jsErrorData "Error"          "JS Error"           >> Option.map JsError
        yield jsGeneralError >> Some
    }
    
    let jsError error =
        jsErrorBuilders
        |> Seq.map (fun f -> f error)
        |> Seq.find Option.isSome
        |> Option.map ScriptError
        |> Option.get

    let parseError = ParseError >> ParserError

    let includeNotFound = IncludeNotFound >> RunnerError

    let error = GeneralError


open Errors

type EngineResult = Result<string, EngineError>
