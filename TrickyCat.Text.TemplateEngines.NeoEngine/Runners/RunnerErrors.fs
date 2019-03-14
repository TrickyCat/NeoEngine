﻿namespace TrickyCat.Text.TemplateEngines.NeoEngine.Runners

open System
open TrickyCat.Text.TemplateEngines.NeoEngine.Common

module RunnerErrors =

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


    type RunnerError =
        | JsReferenceError of JsErrorData
        | JsTypeError      of JsErrorData
        | JsSyntaxError    of JsErrorData
        | JsRangeError     of JsErrorData
        | JsURIError       of JsErrorData
        | JsError          of JsErrorData

        //| IncludeNotFound  of string
        //| ParsingError  of string

        | GeneralError     of string
            with
            override x.ToString() =
                match x with
                | JsReferenceError r
                | JsTypeError r
                | JsRangeError r
                | JsError r
                | JsURIError r
                | JsSyntaxError r    -> r.ToString()
                | GeneralError s     -> sprintf "Error: %s" s


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
                        if lines.[3].Contains(":") then                         //TODO: check index!!!!!!
                            lines.[3].Split ':' |> Array.last |> trim |> Some
                        else
                            None
                    let st = if lines.Length > 3 then String.Join("\n", lines.[4..] |> Array.map trim) else ""
                    Some { title = title; lineNumber = lineNumber; errorMessage = errorMessage; failingString = failingString; failingStringPointerHint = hint; stackTrace = st }
                else
                    None
            else
                None


    let private jsReferenceError = jsErrorData "ReferenceError" "JS Reference Error" >> Option.map JsReferenceError
    let private jsTypeError      = jsErrorData "TypeError"      "JS Type Error"      >> Option.map JsTypeError
    let private jsSyntaxError    = jsErrorData "SyntaxError"    "JS Syntax Error"    >> Option.map JsSyntaxError
    let private jsRangeError     = jsErrorData "RangeError"     "JS Range Error"     >> Option.map JsRangeError
    let private jsUriError       = jsErrorData "URIError"       "JS URI Error"       >> Option.map JsURIError
    let private jsError          = jsErrorData "Error"          "JS Error"           >> Option.map JsError
    let private generalError   = GeneralError >> Some
    
    let private errorBuilders = seq {
        yield jsReferenceError
        yield jsTypeError
        yield jsSyntaxError
        yield jsRangeError
        yield jsUriError
        yield jsError
        yield generalError
    }
    
    let runnerError error =
        errorBuilders
        |> Seq.map (fun f -> f error)
        |> Seq.find Option.isSome
        |> (fun o -> o.Value)