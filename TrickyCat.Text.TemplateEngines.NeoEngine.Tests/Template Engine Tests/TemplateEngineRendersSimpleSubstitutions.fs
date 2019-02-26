namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests

open ``Template Engine Tests Common``
open FsUnitTyped
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.TemplateRunnerHelpers

module ``Template Engine Renders Simple Substitutions`` =
    let private successTestData: obj [][] = [|
        [| "<%= 42 %>"; "42" |]
        [| "<%= 1 %><%= 2 %>"; "12" |]
        [| "<%=   1      %>"; "1" |]
        [| "<%= 2 - 1 %>"; "1" |]
        [| "<%= a %>"; "" |]
        [| "<%= f(z) %>"; "" |]
        [| "<%= a.b %>"; "" |]
        [| "<%= a.b.c(d) + e.f(g) %>"; "" |]
        [| "<% function f(x) { return (x + 2) * 3; } %>Hello<% function double(x) { return 2 * x;} %> <%= double(f(5)) %>!"; "Hello 42!" |]
        [| "<% function f(x) { return x + 10; } %><%= f(1) %> <% function f(x) { return x - 200; } %><%= f(300) %>"; "11 100"|]
    |]


    [<Test; TestCaseSource("successTestData")>]
    let ``Template Engine Should Correctly Render Simple Substitutions When Context Data Is Empty`` templateString expected =
        renderTemplate emptyGlobals emptyIncludes emptyContext templateString
        |> shouldEqual expected
    