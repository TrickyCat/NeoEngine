namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.ParserTests

open ``Parser Tests Common``
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.ParserCore
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.ParserApi

module ParserOptimizationsTests =
    let private successTestData: obj [] seq = seq {
        yield [| "<%   %>"; okEmptyTemplate |]
        yield [| "<%%>"; okEmptyTemplate |]

        yield [| "<%=   %>"; okEmptyTemplate |]
        yield [| "<%=%>"; okEmptyTemplate |]

        yield [| "<%@   %>"; okEmptyTemplate |]
        yield [| "<%@%>"; okEmptyTemplate |]

        yield [| "<%@ include view='' %>"; okEmptyTemplate |]
        yield [| "<%@ include view=\"\" %>"; okEmptyTemplate |]

        yield [| "<% if(true) { %><% } %>"; okEmptyTemplate |]
        yield [| "<% if(true) { %><% } else { %><% } %>"; okEmptyTemplate |]
        yield [| "<% if(c) { %><% } else { %>hello<% } %>"; okTemplate [NeoIfElseTemplate {  condition = "!(c)"; ifBranchBody = [Str "hello"]; elseBranchBody = None }] |]
    }

    let private parserUnderTest = templateParser

    [<Test; TestCaseSource("successTestData")>]
    let ``Parser's AST optimizations contract`` templateString expected =
        runParserOnSuccessfulData parserUnderTest (templateString, expected)

