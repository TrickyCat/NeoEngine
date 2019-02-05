namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.ParserTests

open NUnit.Framework
open FsUnitTyped
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserCore
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserApi
open ``Parser Tests Common``

module ``Parser Transformations Tests`` =

    let private successTestData: obj [] seq = seq {
        yield [| [Str' "hello"]; okTemplate [Str "hello"] |]
        yield [| [Str' "hello"; Str' "world"]; okTemplate [Str "hello"; Str "world"] |]
        yield [| [Neo' "hello"]; okTemplate [Neo "hello"] |]
        yield [| [NeoSubstitute' "hello"]; okTemplate [NeoSubstitute "hello"] |]
        yield [| [NeoIncludeView' "hello"]; okTemplate [NeoIncludeView "hello"] |]
        
        yield [| [NeoIfElseTemplate' { condition = "p"; ifBranchBody = []; elseBranchBody = None }];
                 okTemplate [NeoIfElseTemplate { condition = "p"; ifBranchBody = []; elseBranchBody = None }] |]

        yield [| [NeoIfElseTemplate' { condition = "p"; ifBranchBody = [Str' "hello"]; elseBranchBody = None }];
             okTemplate [NeoIfElseTemplate { condition = "p"; ifBranchBody = [Str "hello"]; elseBranchBody = None }] |]

        yield [| [NeoIfElseTemplate' { condition = "p"; ifBranchBody = [Str' "hello"; Neo' "a"]; elseBranchBody = None }];
            okTemplate [NeoIfElseTemplate { condition = "p"; ifBranchBody = [Str "hello"; Neo "a"]; elseBranchBody = None }] |]

        yield [| [NeoIfElseTemplate' { condition = "p"; ifBranchBody = [Str' "hello"; Neo' "a"]; elseBranchBody = Some <| [Str' "world"; Neo' "b"] }];
            okTemplate [NeoIfElseTemplate { condition = "p"; ifBranchBody = [Str "hello"; Neo "a"]; elseBranchBody = Some <| [Str "world"; Neo "b"]  }] |]

        yield [| [NeoIfElseTemplate' {
            condition = "p"
            ifBranchBody = [
                Str' "hello"
                Neo' "a"
                NeoIfElseTemplate' { condition = "c"; ifBranchBody = [Str' "inside"; Neo' "foo"]; elseBranchBody = None }
                ]
            elseBranchBody = None
            }];

            okTemplate [NeoIfElseTemplate {
                condition = "p"
                ifBranchBody = [
                    Str "hello"
                    Neo "a"
                    NeoIfElseTemplate { condition = "c"; ifBranchBody = [Str "inside"; Neo "foo"]; elseBranchBody = None }
                    ]
                elseBranchBody = None
                }]
            |]


        yield [| [NeoIfElseTemplate' {
            condition = "p"
            ifBranchBody = [
                Str' "hello"
                Neo' "a"
                NeoIfElseTemplate' { condition = "c"; ifBranchBody = [Str' "inside"; Neo' "foo"]; elseBranchBody = None }
                ]
            elseBranchBody = Some <| [
                Str' "hello"
                Neo' "a"
                NeoIfElseTemplate' { condition = "c"; ifBranchBody = [Str' "inside"; Neo' "foo"]; elseBranchBody = Some <| [Str' "1"]}
            ]
            }];

            okTemplate [NeoIfElseTemplate {
                condition = "p"
                ifBranchBody = [
                    Str "hello"
                    Neo "a"
                    NeoIfElseTemplate { condition = "c"; ifBranchBody = [Str "inside"; Neo "foo"]; elseBranchBody = None }
                    ]
                elseBranchBody = Some <| [
                    Str "hello"
                    Neo "a"
                    NeoIfElseTemplate { condition = "c"; ifBranchBody = [Str "inside"; Neo "foo"]; elseBranchBody = Some <| [Str "1"]}
                ]
                }]
            |]

        yield [| [NeoIfElseTemplate' {
            condition = "p"
            ifBranchBody = [
                Str' "hello"
                Neo' "a"
                NeoIfElseTemplate' { condition = "c"; ifBranchBody = [
                    Str' "inside"
                    Neo' "foo"
                    NeoIfElseTemplate' { condition = "c2"; ifBranchBody = [Str' "inside2"; Neo' "foo2"]; elseBranchBody = None }
                    ]; elseBranchBody = None }
                ]
            elseBranchBody = None
            }];

            okTemplate [NeoIfElseTemplate {
                condition = "p"
                ifBranchBody = [
                    Str "hello"
                    Neo "a"
                    NeoIfElseTemplate { condition = "c"; ifBranchBody = [
                        Str "inside"
                        Neo "foo"
                        NeoIfElseTemplate { condition = "c2"; ifBranchBody = [Str "inside2"; Neo "foo2"]; elseBranchBody = None }
                        ]; elseBranchBody = None }
                    ]
                elseBranchBody = None
                }]
            |]


        let b1 = BeginOfConditionalTemplate' "hello"
        yield [| [b1]; errorTemplateUnexpectedLowNode b1 |]
        yield [| [Str' "1"; b1]; errorTemplateUnexpectedLowNode b1 |]
        yield [| [b1; Neo' "2"]; errorTemplateUnexpectedLowNode b1 |]
        yield [| [Str' "1"; b1; Neo' "2"]; errorTemplateUnexpectedLowNode b1 |]

        let b2 = BeginOfConditionalTemplate' "hi"
        yield [| [Neo' "foo()"; b1; b2; b1; b2; Str' "s"]; errorTemplateUnexpectedLowNode b1 |]
        yield [| [Neo' "foo()"; b2; b2; b1; b2; Str' "s"]; errorTemplateUnexpectedLowNode b2 |]

        let endOfConditionalBlock = EndOfConditionalTemplate'
        yield [| [endOfConditionalBlock]; errorTemplateUnexpectedLowNode endOfConditionalBlock |]
        yield [| [Str' "foo"; Neo' "bar"; endOfConditionalBlock]; errorTemplateUnexpectedLowNode endOfConditionalBlock |]

        let elseDelim = ElseBranchOfConditionalTemplateDelimiter'
        yield [| [elseDelim]; errorTemplateUnexpectedLowNode elseDelim |]
        yield [| [Str' "foo"; Neo' "bar"; elseDelim]; errorTemplateUnexpectedLowNode elseDelim |]

        let includeNode = NeoInclude' "smth"
        yield [| [includeNode]; errorTemplateUnexpectedLowNode includeNode |]
        yield [| [Str' "foo"; Neo' "bar"; includeNode]; errorTemplateUnexpectedLowNode includeNode |]

        let includeValue = NeoIncludeValue' "smth"
        yield [| [includeValue]; errorTemplateUnexpectedLowNode includeValue |]
        yield [| [Str' "foo"; Neo' "bar"; includeValue]; errorTemplateUnexpectedLowNode includeValue |]
    }


    [<Test; TestCaseSource("successTestData")>]
    let ``Parser Properly Transforms AST from low- to a higher-level one.`` inputAst expected =
        toTemplate inputAst |> shouldEqual expected


