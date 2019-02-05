namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.ParserTests

open NUnit.Framework
open FsUnitTyped
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserCore
open TrickyCat.Text.TemplateEngines.NeoEngine.Parsers.NeoTemplateParserApi

#nowarn "59"
module ``Parser Transformations Tests`` =
    type private T = Result<TemplateNode list, string>

    let errorFmt = sprintf "Unexpected AST element: %O"

    let private successTestData: obj [] seq = seq {
        yield [| [Str' "hello"]; (Ok [Str "hello"]) :> T |]
        yield [| [Str' "hello"; Str' "world"]; (Ok [Str "hello"; Str "world"]) :> T |]
        yield [| [Neo' "hello"]; (Ok [Neo "hello"]) :> T |]
        yield [| [NeoSubstitute' "hello"]; (Ok [NeoSubstitute "hello"]) :> T |]
        yield [| [NeoIncludeView' "hello"]; (Ok [NeoIncludeView "hello"]) :> T |]
        
        yield [| [NeoIfElseTemplate' { condition = "p"; ifBranchBody = []; elseBranchBody = None }];
             (Ok [NeoIfElseTemplate { condition = "p"; ifBranchBody = []; elseBranchBody = None }]) :> T |]

        yield [| [NeoIfElseTemplate' { condition = "p"; ifBranchBody = [Str' "hello"]; elseBranchBody = None }];
             (Ok [NeoIfElseTemplate { condition = "p"; ifBranchBody = [Str "hello"]; elseBranchBody = None }]) :> T |]

        yield [| [NeoIfElseTemplate' { condition = "p"; ifBranchBody = [Str' "hello"; Neo' "a"]; elseBranchBody = None }];
            (Ok [NeoIfElseTemplate { condition = "p"; ifBranchBody = [Str "hello"; Neo "a"]; elseBranchBody = None }]) :> T |]

        yield [| [NeoIfElseTemplate' { condition = "p"; ifBranchBody = [Str' "hello"; Neo' "a"]; elseBranchBody = Some <| [Str' "world"; Neo' "b"] }];
            (Ok [NeoIfElseTemplate { condition = "p"; ifBranchBody = [Str "hello"; Neo "a"]; elseBranchBody = Some <| [Str "world"; Neo "b"]  }]) :> T |]

        yield [| [NeoIfElseTemplate' {
            condition = "p"
            ifBranchBody = [
                Str' "hello"
                Neo' "a"
                NeoIfElseTemplate' { condition = "c"; ifBranchBody = [Str' "inside"; Neo' "foo"]; elseBranchBody = None }
                ]
            elseBranchBody = None
            }];

            (Ok [NeoIfElseTemplate {
                condition = "p"
                ifBranchBody = [
                    Str "hello"
                    Neo "a"
                    NeoIfElseTemplate { condition = "c"; ifBranchBody = [Str "inside"; Neo "foo"]; elseBranchBody = None }
                    ]
                elseBranchBody = None
                }]
                ) :> T |]


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

            (Ok [NeoIfElseTemplate {
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
                ) :> T |]

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

            (Ok [NeoIfElseTemplate {
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
                ) :> T |]



        let b1 = BeginOfConditionalTemplate' "hello"
        yield [| [b1]; (Error <| errorFmt b1) :> T |]
        yield [| [Str' "1"; b1]; (Error <| errorFmt b1) :> T |]
        yield [| [b1; Neo' "2"]; (Error <| errorFmt b1) :> T |]
        yield [| [Str' "1"; b1; Neo' "2"]; (Error <| errorFmt b1) :> T |]

        let b2 = BeginOfConditionalTemplate' "hi"
        yield [| [Neo' "foo()"; b1; b2; b1; b2; Str' "s"]; (Error <| errorFmt b1) :> T |]
        yield [| [Neo' "foo()"; b2; b2; b1; b2; Str' "s"]; (Error <| errorFmt b2) :> T |]

        let endOfConditionalBlock = EndOfConditionalTemplate'
        yield [| [endOfConditionalBlock]; (Error <| errorFmt endOfConditionalBlock) :> T |]
        yield [| [Str' "foo"; Neo' "bar"; endOfConditionalBlock]; (Error <| errorFmt endOfConditionalBlock) :> T |]

        let elseDelim = ElseBranchOfConditionalTemplateDelimiter'
        yield [| [elseDelim]; (Error <| errorFmt elseDelim) :> T |]
        yield [| [Str' "foo"; Neo' "bar"; elseDelim]; (Error <| errorFmt elseDelim) :> T |]

        let includeNode = NeoInclude' "smth"
        yield [| [includeNode]; (Error <| errorFmt includeNode) :> T |]
        yield [| [Str' "foo"; Neo' "bar"; includeNode]; (Error <| errorFmt includeNode) :> T |]

        let includeValue = NeoIncludeValue' "smth"
        yield [| [includeValue]; (Error <| errorFmt includeValue) :> T |]
        yield [| [Str' "foo"; Neo' "bar"; includeValue]; (Error <| errorFmt includeValue) :> T |]
    }


    [<Test; TestCaseSource("successTestData")>]
    let ``Parser Properly Transforms AST from low- to a higher-level one.`` inputAst expected =
        toTemplate inputAst |> shouldEqual expected


