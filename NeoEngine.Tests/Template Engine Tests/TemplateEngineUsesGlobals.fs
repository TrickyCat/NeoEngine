namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests

open ``Template Engine Tests Common``
open FsUnitTyped
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.Helpers
open TrickyCat.Text.TemplateEngines.NeoEngine.ExecutionResults.Errors
open TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests.Common
open Errors

module ``Template Engine Uses Globals`` =

    let private successTestData: obj [][] = [|
        [| "<%= greet('World') %>"; renderOk "Hello, World!" |]
        [| "<%= foo(11) %>"; renderOk "221" |]
        [| "<%= greet(foo(10)) %>"; renderOk "Hello, 200!" |]

        [| "<%= magicNumber %>"; renderOk "7" |]
        [| "<%= magicNumber2 %>"; renderOk "10" |]
        [| "<%= magicNumber3 %>"; renderOk "3" |]
        [| "<%= magicNumber4 %>"; renderOk "9" |]
    |]

    let private globals = seq {
        yield """
            var magicNumber2 = 10;
            magicNumber = 7;
            magicNumber3 = 8;
            let magicNumber4 = 9;
            let l1 = 100;
            var v1 = 1;
            
            function greet(name) {
                return 'Goodbye, ' + name + '!';
            }
        """
        yield """
            var magicNumber3 = 2;
            function greet(name) {
                return 'Hello, ' + name + '!';
            }
        """
        yield """
            var magicNumber3 = 3;
            function foo(x) {
                return x * x + 100;
            }
        """
    }

    [<Test; TestCaseSource("successTestData")>]
    let ``Template Engine Should Use Globals`` templateString expected =
        renderTemplate globals emptyIncludes emptyContext templateString
        |> shouldEqual expected


    let private noOverridesForGlobalConstantsTestData: obj [][] = [| 
        [| "<% const c1 = 2; %>"; E.jsError (JsSyntaxError) |]
        [| "<% let c1 = 2; %>";   E.jsError (JsSyntaxError) |]
        [| "<% var c1 = 2; %>";   E.jsError (JsSyntaxError) |]
        [| "<% c1 = 2; %>";       E.jsError (JsTypeError) |]
    |]

    [<Test; TestCaseSource("noOverridesForGlobalConstantsTestData")>]
    let ``Template can't override global 'const' bindings`` templateString expected =
        renderTemplate (seq { yield "const c1 = 1;" }) emptyIncludes emptyContext templateString
        |> shouldFailWith expected


    let private noOverridesForGlobalLetsTestData: obj [][] = [| 
        [| "<% const l1 = 2; %>"; E.jsError (JsSyntaxError) |]
        [| "  <% let l1 = 2; %>"; E.jsError (JsSyntaxError) |]
        [| "  <% var l1 = 2; %>"; E.jsError (JsSyntaxError) |]
    |]

    [<Test; TestCaseSource("noOverridesForGlobalLetsTestData")>]
    let ``Template can't override global 'let' bindings`` templateString expected =
        renderTemplate (seq { yield "let l1 = 100;" }) emptyIncludes emptyContext templateString
        |> shouldFailWith expected



    let private noOverridesForGlobalVarsTestData: obj [][] = [| 
        [| "<% const v1 = 2; %>"; E.jsError (JsSyntaxError) |]
        [| "  <% let v1 = 2; %>"; E.jsError (JsSyntaxError) |]
    |]

    [<Test; TestCaseSource("noOverridesForGlobalVarsTestData")>]
    let ``Template can't override global 'var' bindings with 'let' and 'const'`` templateString expected =
        renderTemplate (seq { yield "var v1 = 100;" }) emptyIncludes emptyContext templateString
        |> shouldFailWith expected



    let private overridesForGlobalVarsTestData: obj [][] = [| 
        [| "<%= v1 %>"; renderOk "1" |]
        [| "<%= v1 %><% v1 = 2; %><%= v1 %>"; renderOk "12" |]
        [| "<%= v1 %><% var v1 = 2; %><%= v1 %>"; renderOk "12" |]
    |]

    [<Test; TestCaseSource("overridesForGlobalVarsTestData")>]
    let ``Template can override global 'var' bindings with 'var' and 'implicit var'`` templateString expected =
        renderTemplate (seq { yield "var v1 = 1;" }) emptyIncludes emptyContext templateString
        |> shouldEqual expected



    let private overridesForGlobalImplicitVarsTestData: obj [][] = [| 
        [| "<%= v1 %>"; renderOk "1" |]
        [| "<%= v1 %><% v1 = 2; %><%= v1 %>"; renderOk "12" |]
        [| "<%= v1 %><% var v1 = 2; %><%= v1 %>"; renderOk "12" |]
    |]

    [<Test; TestCaseSource("overridesForGlobalImplicitVarsTestData")>]
    let ``Template can override global implicit 'var' bindings with 'var' and 'implicit var'`` templateString expected =
        renderTemplate (seq { yield "v1 = 1;" }) emptyIncludes emptyContext templateString
        |> shouldEqual expected



    let private canLookupGlobalValues: obj [] seq = seq {
        yield [| "<%= v1 %>"; renderOk "100" |]
        yield [| "<%= c1 %>"; renderOk "200" |]
        yield [| "<%= l1 %>"; renderOk "300" |]
        yield [| "<%= v2 %>"; renderOk "400" |]
        yield [| "<%= !!(f) %>"; renderOk "true" |]
    }

    [<Test; TestCaseSource("canLookupGlobalValues")>]
    let ``Template's code can lookup global values`` templateString expected =
        renderTemplate (seq { yield """
        var   v1 = 100;
        const c1 = 200;
        let   l1 = 300;
              v2 = 400;
        function f(){}
        """ }) emptyIncludes emptyContext templateString
        |> shouldEqual expected