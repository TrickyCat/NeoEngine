namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.ParserTests

open ``Parser Tests Common``
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserCore
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserApi

module ``Whole Neo Template Parser Tests`` = 

    let private successTestData: obj [] seq = seq {
        yield [| ""; emptyTemplate |]
        yield [| "Hello, world!"; [Str "Hello, world!"] |]

        yield [| @"復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮"; [Str @"復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮"] |]
        yield [| @"परिवहन तकनिकल व्यवहार कार्य भाति उद्योग लक्ष्य अत्यंत जिसे"; [Str @"परिवहन तकनिकल व्यवहार कार्य भाति उद्योग लक्ष्य अत्यंत जिसे"] |]
        
        yield [| @"Begin<% inside %>End";
            [Str "Begin"; Neo "inside"; Str "End"] |]
        yield [| @"<% inside %>"; [Neo "inside"] |]
        yield [| @"<%@ include view='pdcHash' %>"; [NeoIncludeView "pdcHash"] |]
        yield [| @"<%= foo.bar.baz %>"; [NeoSubstitute "foo.bar.baz"] |]
        yield [| @"Begin<% inside %>After<%@ include %>after include<%= key %>End";
            [Str "Begin"; Neo "inside"; Str "After"; NeoInclude "include"; Str "after include"; NeoSubstitute "key"; Str "End"] |]

        yield [| @"begin<% inside %>after<% if (42 > 21) {%>It's true!!<% } %>end";
            [Str "begin"; Neo "inside"; Str "after"; 
            NeoIfElseTemplate
            { 
                condition = "42 > 21";
                ifBranchBody = [Str "It's true!!"];
                elseBranchBody = None
            };
            Str "end"] |]

        yield [| @"begin<% inside %>after<% if (42 > 21) {%>It's true!!<% } else { %>else body<% } %>end";
            [Str "begin"; Neo "inside"; Str "after"; 
            NeoIfElseTemplate
            {
                condition = "42 > 21"; 
                ifBranchBody = [Str "It's true!!"];
                elseBranchBody = Some [Str "else body"]
            }; 
            Str "end"] |]

        yield [| @"begin<% inside %>after<% if (42 > 21) {%>It's true!! <% if (a && (b || c)) { %>first nested if<% } %> inside if but after nested if <% } %>end";
        [Str "begin"; Neo "inside"; Str "after"; 
        NeoIfElseTemplate
        {
            condition = "42 > 21"; 
            ifBranchBody = [Str "It's true!! ";
                NeoIfElseTemplate
                {
                    condition = "a && (b || c)"
                    ifBranchBody = [Str "first nested if"]
                    elseBranchBody = None
                }; Str " inside if but after nested if "];
            elseBranchBody = None
        }; 
        Str "end"] |]

        //yield [| @"hello<% world"; [ Str @"hello<% world" ] |]
    }

    let private failureTestData: obj [][] = [| |]

    let private parserUnderTest = templateParser

    [<Test; TestCaseSource("successTestData")>]
    let ``Whole Neo template parser should succeed on expected input`` payload =
        runParserOnSuccessfulData parserUnderTest payload

    [<Test; TestCaseSource("failureTestData")>]
    let ``Whole Neo template should fail on unsupported input`` payload =
        runParserOnUnsupportedData parserUnderTest payload