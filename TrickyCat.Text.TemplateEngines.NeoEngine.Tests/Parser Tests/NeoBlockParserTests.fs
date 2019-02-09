namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.ParserTests

open ``Parser Tests Common``
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserCore

module ``Neo Block Parser Tests`` =

    let private successTestData: obj [] seq = seq {
        yield [| "<% %>"; Neo' "" |]

        yield [| "<% some code block %>"; Neo' "some code block" |]
        yield [| "<% 復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮 %>"; Neo' "復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮" |]
        yield [| "<% 復案ぼへび    復案ぼへび  復案ぼへび %>"; Neo' "復案ぼへび    復案ぼへび  復案ぼへび" |]
        yield [| "<% तकनिकल %>"; Neo' "तकनिकल" |]
        yield [| "<% तकनिकल तकनिकल तकनिकल %>"; Neo' "तकनिकल तकनिकल तकनिकल" |]
        yield [| "<% foo.bar.baz %>"; Neo' "foo.bar.baz" |]

        yield [| "<%= foo.bar.baz %>"; Neo' "= foo.bar.baz" |]
        yield [| "<%= 復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮 %>"; Neo' "= 復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮" |]
        yield [| "<%= 復案  びえ  ヒモ %>"; Neo' "= 復案  びえ  ヒモ" |]
        yield [| "<%= तकनिकलतकनिकलतकनिकल %>"; Neo' "= तकनिकलतकनिकलतकनिकल" |]
        yield [| "<%= तकनिकल   तकनिकल %>"; Neo' "= तकनिकल   तकनिकल" |]
        
        yield [| "<%@ someInclude %>"; Neo' "@ someInclude" |]
        yield [| "<%@ 復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮 %>"; Neo' "@ 復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮" |]
        yield [| "<%@ 復案   びえ ホ %>"; Neo' "@ 復案   びえ ホ" |]
        yield [| "<%@ तकनिकलतकनिकलतकनिकल %>"; Neo' "@ तकनिकलतकनिकलतकनिकल" |]
        yield [| "<%@ तक  निकलत    कनिक  लतकनिकल %>"; Neo' "@ तक  निकलत    कनिक  लतकनिकल" |]
    }

    let private failureTestData: obj [][] = [|
        [|""|]
        [|"start"|]
        [|"<html><body><p>Some content</p></body></html>"|]
        [|"復案ぼへびえ"|]
        [|"तकनिकल"|]
        [|"something before neo<% something %>"|]
        [|"something before neo<%@ something %>"|]
        [|"something before neo<% 案ぼへびえ焦 %>"|]
        [|"something before neo<%@ 案ぼへびえ焦 %>"|]
        [|"something before neo<% तकनिकल %>"|]
        [|"something before neo<%@ तकनिकल %>"|]
     |]

    let private parserUnderTest = neoBlockParser

    [<Test; TestCaseSource("successTestData")>]
    let ``Neo block parser should succeed on expected input`` payload =
        runParserOnSuccessfulData parserUnderTest payload

    [<Test; TestCaseSource("failureTestData")>]
    let ``Neo block parser should fail on unsupported input`` payload =
        runParserOnUnsupportedData parserUnderTest payload


