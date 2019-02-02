namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.ParserTests

open ``Parser Tests Common``
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserCore

module ``Neo Inlude Parser Tests`` =

    let private successTestData: obj [] seq = seq {
        yield [| "<%@ something  %>"; NeoInclude "something" |]
        yield [| "<%@something%>"; NeoInclude "something" |]

        yield [| "<%@ value rest %>"; NeoIncludeValue "rest" |]
        yield [| "<%@ valuerest %>"; NeoInclude "valuerest" |]

        yield [| "<%@ include view='pdcHash' %>"; NeoIncludeView "pdcHash" |]
        yield [| "<%@ include view=\"pdcHash\" %>"; NeoIncludeView "pdcHash" |]
        yield [| "<%@ includeview='pdcHash' %>"; NeoInclude "includeview='pdcHash'" |]

        yield [| "<%@ 復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮 %>"; NeoInclude "復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮" |]
        yield [| "<%@ तकनिकल %>"; NeoInclude "तकनिकल" |]
    }

    let private failureTestData: obj [][] = [|
        [|""|]
        [|"start"|]
        [|"<html><body><p>Some content</p></body></html>"|]
        [|"復案ぼへびえ"|]
        [|"तकनिकल"|]
        [|"<% something %>"|]
        [|"<%= something %>"|]
        [|"<% 復案ぼへびえ %>"|]
        [|"<%= 復案ぼへびえ %>"|]
        [|"<% तकनिकल %>"|]
        [|"<%= तकनिकल %>"|]
        [|"something before<%@ something %>"|]
        [|"something before<% something %>"|]
        [|"something before<%= something %>"|]
     |]

    let private parserUnderTest = neoIncludeParser
    
    [<Test; TestCaseSource("successTestData")>]
    let ``Neo include parser should succeed on expected input`` payload =
        runParserOnSuccessfulData parserUnderTest payload
    
    [<Test; TestCaseSource("failureTestData")>]
    let ``Neo include parser should fail on unsupported input`` payload =
        runParserOnUnsupportedData parserUnderTest payload
