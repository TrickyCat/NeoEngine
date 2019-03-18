namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.ParserTests

open ``Parser Tests Common``
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.ParserCore

module ``Neo Substitute Parser Tests`` =

    let private successTestData: obj [] seq = seq {
        yield [| "<%= foo.bar.baz %>"; NeoSubstitute' "foo.bar.baz" |]
        yield [| "<%= something %>"; NeoSubstitute' "something" |]
        yield [| "<%= 復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮 %>"; NeoSubstitute' "復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮" |]
        yield [| "<%= तकनिकल %>"; NeoSubstitute' "तकनिकल" |]
    }

    let private failureTestData: obj [][] = [|
        [|""|]
        [|"start"|]
        [|"<html><body><p>Some content</p></body></html>"|]
        [|"復案ぼへびえ"|]
        [|"तकनिकल"|]
        [|"<% something %>"|]
        [|"<%@ something %>"|]
        [|"<% 案ぼへびえ焦 %>"|]
        [|"<%@ 案ぼへびえ焦 %>"|]
        [|"<% तकनिकल %>"|]
        [|"<%@ तकनिकल %>"|]

        [|"something before neo<% something %>"|]
        [|"something before neo<%= something %>"|]
        [|"something before neo<%@ something %>"|]
        [|"something before neo<% 案ぼへびえ焦 %>"|]
        [|"something before neo<%= 案ぼへびえ焦 %>"|]
        [|"something before neo<%@ 案ぼへびえ焦 %>"|]
        [|"something before neo<% तकनिकल %>"|]
        [|"something before neo<%= तकनिकल %>"|]
        [|"something before neo<%@ तकनिकल %>"|]
     |]

    let private parserUnderTest = neoSubstituteParser

    [<Test; TestCaseSource("successTestData")>]
    let ``Neo substitution parser should succeed on expected input`` payload =
        runParserOnSuccessfulData parserUnderTest payload

    [<Test; TestCaseSource("failureTestData")>]
    let ``Neo substitution parser should fail on unsupported input`` payload =
        runParserOnUnsupportedData parserUnderTest payload

