namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.ParserTests

open ``Parser Tests Common``
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserCore

module ``Tests For Parser Of A String Before Neo Customization`` =

    let private successTestData: obj [] seq = seq {
       yield [|"<%"; Str' ""|]
       yield [|"before<%"; Str' "before"|]
       yield [|       "<html><body><p>Some content</p><%= footer%></body></html>";
            Str' "<html><body><p>Some content</p>" |]
    }

    let private failureTestData: obj [] seq = seq {
       yield [|""|]
       yield [|"start"|]
    }

    let private parserUnderTest = strBeforeNeoCustomizationParser

    [<Test; TestCaseSource("successTestData")>]
    let ``Parser [string before Neo customization] should succeed on expected input`` payload =
        runParserOnSuccessfulData parserUnderTest payload

    [<Test; TestCaseSource("failureTestData")>]
    let ``Parser [string before Neo customization] should fail on unsupported input`` payload =
        runParserOnUnsupportedData parserUnderTest payload
