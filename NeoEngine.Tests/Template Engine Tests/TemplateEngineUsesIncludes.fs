﻿namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests

open ``Template Engine Tests Common``
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.Helpers
open TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests.Common
open Errors
open Swensen.Unquote
open System

module ``Template Engine Uses Includes`` =

    let private singleLine (s: string) = s.Replace("\r\n", String.Empty).Replace("\n", String.Empty)

    let private successTestData: obj [][] = [|
        [| "<%@ include view='include1' %><%= greet('World') %>"; renderOk "Hello, World!" |]
        [| "<%@ include view='include2' %><%= greet('World') %>"; renderOk "Hello, World! (from include2)" |]
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
        expected =! renderTemplate emptyGlobals includes emptyContext templateString



    let private orderOfIncludesTestData: obj [][] = [|
        [| "<%@ include view='include1' %><%@ include view='include2' %><%= greet('World') %>"; renderOk "Hello, World! (from include2)" |]
        [| "<%@ include view='include2' %><%@ include view='include1' %><%= greet('World') %>"; renderOk "Hello, World!" |]

        [| "<%@ include view='include1' %><%= greet('World') %> <%@ include view='include2' %><%= greet('World') %>";
            renderOk "Hello, World! Hello, World! (from include2)" |]
        [| "<%@ include view='include2' %><%= greet('World') %> <%@ include view='include1' %><%= greet('World') %>";
            renderOk "Hello, World! (from include2) Hello, World!" |]
    |]

    [<Test; TestCaseSource("orderOfIncludesTestData")>]
    let ``Order of Include References in Template Matters`` templateString expected =
        expected =! renderTemplate emptyGlobals includes emptyContext templateString



    let private noNestedScopesForIncludesTestData: obj [][] = [|
        [| singleLine """
        <%@ include view='include1' %>
        <%= greet('World') %>
        <% if (true) { %>
            <%@ include view='include2' %>
            <%= greet('World') %>
        <% } %>
        <%= greet('World') %>
        """
        ; renderOk << singleLine <| """
        
        Hello, World!
        
            
            Hello, World! (from include2)
        
        Hello, World! (from include2)
        """ |]



        [| singleLine """
        <%@ include view='include1' %>
        <%= greet('World') %>
        <% if (false) { %>
            42
        <% } else { %>
            <%@ include view='include2' %>
            <%= greet('World') %>
        <% } %>
        <%= greet('World') %>
        """
        ; renderOk << singleLine <| """
        
        Hello, World!
        
            
            Hello, World! (from include2)
        
        Hello, World! (from include2)
        """ |]

    |]

    [<Test; TestCaseSource("noNestedScopesForIncludesTestData")>]
    let ``Template Engine Does Not Currently Support Nested Scopes For Include References`` templateString expected =
        expected =! renderTemplate emptyGlobals includes emptyContext templateString



    let private missingIncludesTestData: obj [][] = [|
        [| "<%@ include view='someInclude' %><%= greet('World') %>"; E.includeNotFound () |]
        |]

    [<Test; TestCaseSource("missingIncludesTestData")>]
    let ``Template Engine Should Signal On Missing Includes Referenced In Template`` templateString expected =
        renderTemplate emptyGlobals includes emptyContext templateString
        |> shouldFailWith expected


    let private malformedIncludesTestData: obj [][] = [|
        [| 
            "<%@ include view='someInclude' %><%= greet('World') %>"
            includes.Add("someInclude", """<%""")
        |]
        [| 
            "<%@ include view='someInclude' %><%= greet('World') %>"
            includes.Add("someInclude", """<%=""")
        |]
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
        |> shouldFailWith (E.parseError())

