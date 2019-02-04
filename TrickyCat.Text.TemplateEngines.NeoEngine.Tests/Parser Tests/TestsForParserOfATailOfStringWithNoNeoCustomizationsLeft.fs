namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.ParserTests

open ``Parser Tests Common``
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserCore

module ``Tests For Parser Of A Non-Empty Tail Of String`` =
    
    let private successTestData: obj [] seq = seq {
        yield [|      "Some End Of Template With No Neo customizations open tags left";
                  Str "Some End Of Template With No Neo customizations open tags left" |]

        yield [|      "<html><body><p>Some content</p></body></html>";
                  Str "<html><body><p>Some content</p></body></html>" |]

        yield [|      "<html><body><p>Some content</p><%= footer%></body></html>";
                  Str "<html><body><p>Some content</p><%= footer%></body></html>" |]

        yield [|      "<%";
                  Str "<%" |]

        yield [|      @"परिवहन तकनिकल व्यवहार कार्य भाति उद्योग लक्ष्य अत्यंत जिसे विकास उपेक्ष विश्वास कार्यकर्ता सं";
                  Str @"परिवहन तकनिकल व्यवहार कार्य भाति उद्योग लक्ष्य अत्यंत जिसे विकास उपेक्ष विश्वास कार्यकर्ता सं" |]

        yield [|      @"復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮";
                  Str @"復案ぼへびえ焦集エメシホ方知経ヨマヒモ縮" |]

        yield [|      "some text before <%";
                  Str "some text before <%" |]

        yield [|      "another text before <%= smth %>word<%";
                  Str "another text before <%= smth %>word<%" |]
    }

    let private failureTestData: obj [][] = [|
        [| "" |]
    |]

    let private parserUnderTest = strBeforeEos
    
    [<Test; TestCaseSource("successTestData")>]
    let ``Parser [string with no Neo customizations left] should succeed on expected input`` payload =
        runParserOnSuccessfulData parserUnderTest payload
    
    [<Test; TestCaseSource("failureTestData")>]
    let ``Parser [string with no Neo customizations left] should fail on unsupported input`` payload =
        runParserOnUnsupportedData parserUnderTest payload