namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests

open ``Template Engine Tests Common``
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.Helpers
open Swensen.Unquote

module ``Members In Includes Override Members In Globals With The Same Name In Template Engine`` =

    let private successTestData: obj [][] = [|
        //[| ""; "" |]
        [| "<%= greet('World') %>"; renderOk "Hi, global World!" |]
        [| "<%@ include view='include1' %><%= greet('World') %>"; renderOk "Hello, World!" |]
        
        [| "<%@ include view='include1' %><%= magicNumber %>"; renderOk "100" |]
        
    |]

    let private globals = seq {
        yield """
            magicNumber = 1;

            function greet(name) {
                return 'Hi, global ' + name + '!';
            }
        """
    }

    let private includes =
        Map.empty<string, string>
           .Add("include1", """<%
           function greet(name) {
               return 'Hello, ' + name + '!';
           }

           magicNumber = 100;
           %>""")

    [<Test; TestCaseSource("successTestData")>]
    let ``Template Engine Should Allow Members In Includes To Override Members In Globals With The Same Name`` templateString expected =
        test <@ expected = renderTemplate globals includes emptyContext templateString @>