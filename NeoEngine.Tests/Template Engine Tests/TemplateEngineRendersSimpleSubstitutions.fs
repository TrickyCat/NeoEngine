namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests

open ``Template Engine Tests Common``
open FsUnitTyped
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.Helpers
open TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests.Common
open TrickyCat.Text.TemplateEngines.NeoEngine.ExecutionResults.Errors
open Errors


module ``Template Engine Renders Simple Substitutions`` =
    let private successTestData: obj [][] = [|
        [| "<%= 42 %>"; renderOk "42" |]
        [| "<%= -100 %>"; renderOk "-100" |]
        [| "<%= 1.23 %>"; renderOk "1.23" |]
        [| "<%= -3.14 %>"; renderOk "-3.14" |]
        [| "<%= 12345678 %>"; renderOk "12345678" |]
        [| "<%= -12345678 %>"; renderOk "-12345678" |]
        [| "<%= 2147483647 %>"; renderOk "2147483647" |]
        [| "<%= -2147483648 %>"; renderOk "-2147483648" |]

        [| "<%= 0 %>"; renderOk "0" |]
        [| "<%= +0 %>"; renderOk "0" |]
        [| "<%= -0 %>"; renderOk "0" |]
        [| "<%= 0. %>"; renderOk "0" |]
        [| "<%= 0.0 %>"; renderOk "0" |]
        [| "<%= +0. %>"; renderOk "0" |]
        [| "<%= +0.0 %>"; renderOk "0" |]
        [| "<%= -0. %>"; renderOk "0" |]
        [| "<%= -0.0 %>"; renderOk "0" |]
        [| "<%= .0 %>"; renderOk "0" |]
        [| "<%= +.0 %>"; renderOk "0" |]
        [| "<%= -.0 %>"; renderOk "0" |]

        [| "<%= true %>"; renderOk "true" |]
        [| "<%= false %>"; renderOk "false" |]
        [| "<%= 1 < 3 %>"; renderOk "true" |]
        [| "<%= 1 > 100 %>"; renderOk "false" |]

        [| "<%= 'hello' %>"; renderOk "hello" |]
        [| "<%= \"world\" %>"; renderOk "world" |]
        [| "<%= 'foo' + 'bar' %>"; renderOk "foobar" |]

        [| "<%= 1 %><%= 2 %>"; renderOk "12" |]
        [| "<%=   1      %>"; renderOk "1" |]
        [| "<%= 2 - 1 %>"; renderOk "1" |]
        [| "<% function f(x) { return (x + 2) * 3; } %>Hello<% function double(x) { return 2 * x;} %> <%= double(f(5)) %>!"; renderOk "Hello 42!" |]
        [| "<% function f(x) { return x + 10; } %><%= f(1) %> <% function f(x) { return x - 200; } %><%= f(300) %>"; renderOk "11 100"|]
    |]


    [<Test; TestCaseSource("successTestData")>]
    let ``Template Engine Should Correctly Render Simple Substitutions When Context Data Is Empty`` templateString expected =
        renderTemplate emptyGlobals emptyIncludes emptyContext templateString
        |> shouldEqual expected


    let private failingExpressionsTestData: obj [][] = [|
        [| "<%= a %>"; E.jsError (JsReferenceError) |]
        [| "<%= f(z) %>"; E.jsError (JsReferenceError) |]
        [| "<%= a.b %>"; E.jsError (JsReferenceError) |]
        [| "<%= a.b.c(d) + e.f(g) %>"; E.jsError (JsReferenceError) |]
        [| "<% function f() { f(); } %><%= f() %>"; E.jsError (JsRangeError) |]
    |]

    [<Test; TestCaseSource("failingExpressionsTestData")>]
    let ``Template Engine Should Not Forgive Errors In Substitutions`` templateString expected =
        renderTemplate emptyGlobals emptyIncludes emptyContext templateString
        |> shouldFailWith expected
    


    let private unexpectedExpressionResultTypesTestData: obj [][] = [|
        [| "<%= null %>"; E.generalError () |]
        [| "<%= undefined %>"; E.generalError () |]
        [| "<%= {} %>"; E.generalError () |]
        [| "<% const o = { x: 42 }; %><%= o %>"; E.generalError () |]
        [| "<%= [] %>"; E.generalError () |]
        [| "<%= [1,2,3] %>"; E.generalError () |]
        [| "<% const f = () => 42; %><%= f %>"; E.generalError () |]
        [| "<% const s = Symbol(); %><%= s %>"; E.jsError (JsError) |]
        [| "<% const s = Symbol('foo'); %><%= s %>"; E.jsError (JsError) |]
    |]

    [<Test; TestCaseSource("unexpectedExpressionResultTypesTestData")>]
    let ``Template Engine Should Signal On Occurence Of Unexpected Result Types In Substitutions`` templateString expected =
        renderTemplate emptyGlobals emptyIncludes emptyContext templateString
        |> shouldFailWith expected
    