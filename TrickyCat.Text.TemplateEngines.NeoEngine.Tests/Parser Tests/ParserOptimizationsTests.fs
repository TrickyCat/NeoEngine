namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.ParserTests

open ``Parser Tests Common``
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserCore
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserApi

module ParserOptimizationsTests =
    let private successTestData: obj [] seq = seq {
        yield [| "<%   %>"; emptyTemplate |]
        yield [| "<%%>"; emptyTemplate |]

        yield [| "<%=   %>"; emptyTemplate |]
        yield [| "<%=%>"; emptyTemplate |]

        yield [| "<%@   %>"; emptyTemplate |]
        yield [| "<%@%>"; emptyTemplate |]

        yield [| "<%@ include view='' %>"; emptyTemplate |]
        yield [| "<%@ include view=\"\" %>"; emptyTemplate |]
        
        yield [| "<%@ value   %>"; emptyTemplate |]

        yield [| "<% if(true) { %><% } %>"; emptyTemplate |]
        yield [| "<% if(true) { %><% } else { %><% } %>"; emptyTemplate |]
        yield [| "<% if(c) { %><% } else { %>hello<% } %>"; [NeoIfElseTemplate' {  condition = "!(c)"; ifBranchBody = [Str' "hello"]; elseBranchBody = None }] |]
    }

    let private parserUnderTest = templateParser

    [<Test; TestCaseSource("successTestData")>]
    let ``Parser's AST optimizations contract`` templateString expected =
        runParserOnSuccessfulData parserUnderTest (templateString, expected)

