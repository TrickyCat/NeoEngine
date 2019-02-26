namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests

open ``Template Engine Tests Common``
open FsUnitTyped
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.TemplateRunnerHelpers

module ``Template Engine Uses Globals`` =

    let private successTestData: obj [][] = [|
        [| "<%= greet('World') %>"; "Hello, World!" |]
        [| "<%= f() %>"; "" |]
        [| "<%= f(x) %>"; "" |]
        [| "<%= f(100) %>"; "" |]
        [| "<%= foo(11) %>"; "221" |]
        [| "<%= greet(foo(10)) %>"; "Hello, 200!" |]
        [| "<%= greet(foo(10)) + '' + f() %>"; "" |]
        [| "<%= greet(foo(10)) %><%= f() %><%= f(x) %>"; "Hello, 200!" |]

        [| "<%= magicNumber %>"; "7" |]
        [| "<%= magicNumber2 %>"; "10" |]
        [| "<%= magicNumber3 %>"; "3" |]
        [| "<%= magicNumber4 %>"; "9" |]

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
        [| "<% const c1 = 2; %>" |]
        [| "<% let c1 = 2; %>" |]
        [| "<% var c1 = 2; %>" |]
        [| "<% c1 = 2; %>" |]
    |]

    [<Test; TestCaseSource("noOverridesForGlobalConstantsTestData")>]
    let ``Template can't override global 'const' bindings`` templateString =
        runEngineOnMalformedInputs (seq { yield "const c1 = 1;" }) emptyIncludes emptyContext templateString



    let private noOverridesForGlobalLetsTestData: obj [][] = [| 
        [| "<% const l1 = 2; %>" |]
        [| "  <% let l1 = 2; %>" |]
        [| "  <% var l1 = 2; %>" |]
    |]

    [<Test; TestCaseSource("noOverridesForGlobalLetsTestData")>]
    let ``Template can't override global 'let' bindings`` templateString =
        runEngineOnMalformedInputs (seq { yield "let l1 = 100;" }) emptyIncludes emptyContext templateString




    let private noOverridesForGlobalVarsTestData: obj [][] = [| 
        [| "<% const v1 = 2; %>" |]
        [| "  <% let v1 = 2; %>" |]
    |]

    [<Test; TestCaseSource("noOverridesForGlobalVarsTestData")>]
    let ``Template can't override global 'var' bindings with 'let' and 'const'`` templateString =
        runEngineOnMalformedInputs (seq { yield "var v1 = 100;" }) emptyIncludes emptyContext templateString




    let private overridesForGlobalVarsTestData: obj [][] = [| 
        [| "<%= v1 %>"; "1" |]
        [| "<%= v1 %><% v1 = 2; %><%= v1 %>"; "12" |]
        [| "<%= v1 %><% var v1 = 2; %><%= v1 %>"; "12" |]
    |]

    [<Test; TestCaseSource("overridesForGlobalVarsTestData")>]
    let ``Template can override global 'var' bindings with 'var' and 'implicit var'`` templateString expected =
        renderTemplate (seq { yield "var v1 = 1;" }) emptyIncludes emptyContext templateString
        |> shouldEqual expected



    let private overridesForGlobalImplicitVarsTestData: obj [][] = [| 
        [| "<%= v1 %>"; "1" |]
        [| "<%= v1 %><% v1 = 2; %><%= v1 %>"; "12" |]
        [| "<%= v1 %><% var v1 = 2; %><%= v1 %>"; "12" |]
    |]

    [<Test; TestCaseSource("overridesForGlobalImplicitVarsTestData")>]
    let ``Template can override global implicit 'var' bindings with 'var' and 'implicit var'`` templateString expected =
        renderTemplate (seq { yield "v1 = 1;" }) emptyIncludes emptyContext templateString
        |> shouldEqual expected



    let private canLookupGlobalValues: obj [] seq = seq {
        yield [| "<%= v1 %>"; "100" |]
        yield [| "<%= c1 %>"; "200" |]
        yield [| "<%= l1 %>"; "300" |]
        yield [| "<%= v2 %>"; "400" |]
        yield [| "<%= !!(f) %>"; "true" |]
    }

    [<Test; TestCaseSource("canLookupGlobalValues")>]
    let ``Template's code can lookup global values`` templateString expected =
        renderTemplate (seq { yield """
        var v1 = 100;
        const c1 = 200;
        let l1 = 300;
        v2 = 400;
        function f(){}
        """ }) emptyIncludes emptyContext templateString
        |> shouldEqual expected