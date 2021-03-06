﻿namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests

open ``Template Engine Tests Common``
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.Helpers
open Swensen.Unquote

module ``Template Engine Runs On Templates Without Customizations`` =
    let private successTestData: obj [][] = [|
        [| "" |]
        [| "hello" |]
        [| "तकनिकल" |]
        [| "ぼへびえ焦集" |]
        [| "!@#^$&*()_(*&^_)(<>{}+\\|||///" |]
        [| "<html><body><p>Some content</p></body></html>" |]
    |]
    
    [<Test; TestCaseSource("successTestData")>]
    let ``Template Engine Should Run On Templates Without Customizations And Render Result Should Be Identical To The Template Itself`` templateString =
        Ok templateString =! renderTemplate emptyGlobals emptyIncludes emptyContext templateString
