namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests

open ``Template Engine Tests Common``
open FsUnitTyped
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.TemplateRunnerHelpers

module ``Template Engine Uses Includes`` =

    let private successTestData: obj [][] = [|
        [| "<%= greet('World') %>"; "" |]
        [| "<%@ include view='include1' %><%= greet('World') %>"; "Hello, World!" |]
        [| "<%@ include view='include1' %><%@ include view='include2' %><%= greet('World') %>"; "Hello, World! (from include2)" |]
        [| "<%@ include view='include2' %><%@ include view='include1' %><%= greet('World') %>"; "Hello, World!" |]
        [| "<%= greet('World') %><%@ include view='include1' %><%= greet('World') %> <%@ include view='include2' %><%= greet('World') %>"; "Hello, World! Hello, World! (from include2)" |]
        

        //[| "<%@ include view='include1' %><%@ include view='include2' %><%= magicNumber %>"; "" |]
        [| "<%= magicNumber %>"; "" |]
        //[| "<%@ include view='include1' %><%= magicNumber %>"; "42" |]
        //[| "<%@ include view='include2' %><%= magicNumber %>"; "" |]
    |]



    let private includes =
        Map.empty<string, string>
           .Add("include1", """<%
           var magicNumber = 42;

           function greet(name) {
               return 'Hello, ' + name + '!';
           }
           %>""")
           .Add("include2", """<%
           function greet(name) {
               return 'Hello, ' + name + '! (from include2)';
           }
           %>""")

    [<Test; TestCaseSource("successTestData")>]
    let ``Template Engine Should Use Includes Referenced In Template`` templateString expected =
        renderTemplate emptyGlobals includes emptyContext templateString
        |> shouldEqual expected


    let private missingIncludesTestData: obj [][] = [|
        [| "<%@ include view='someInclude' %><%= greet('World') %>" |]
        |]

    [<Test; TestCaseSource("missingIncludesTestData")>]
    let ``Template Engine Should Signal On Missing Includes Referenced In Template`` templateString =
        renderTemplate emptyGlobals includes emptyContext templateString
        |> shouldEqual (Ok "")


    let private malformedIncludesTestData: obj [][] = [|
        [| 
            "<%@ include view='someInclude' %><%= greet('World') %>"
            includes.Add("someInclude", """<%""")
        |]
        [| 
            "<%@ include view='someInclude' %><%= greet('World') %>"
            includes.Add("someInclude", """<%=""")
        |]
        //[| 
        //    "<%@ include view='someInclude' %><%= greet('World') %>";
        //    includes.Add("someInclude", """%>""")
        //|]
        [| 
            "<%@ include view='someInclude' %><%= greet('World') %>";
            includes.Add("someInclude", """a<%b""")
        |]
        [| 
            "<%@ include view='someInclude' %><%= greet('World') %>";
            includes.Add("someInclude", """a<%= b""")
        |]
        |]

    [<Test; TestCaseSource("malformedIncludesTestData")>]
    let ``Template Engine Should Signal On Malformed Includes Referenced In Template`` templateString includes =
        renderTemplate emptyGlobals includes emptyContext templateString
        |> shouldEqual (Ok "")

