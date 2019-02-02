namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests

open ``Template Engine Tests Common``
open FsUnitTyped
open NUnit.Framework

module ``Template Engine Tests`` = 

    let private successTestData: obj [][] = [|
        
        
    |]

    let private globals = seq {
        yield """
        
        """
    }

    [<Test; TestCaseSource("successTestData")>]
    let ``Template Engine Should Run On Templates That Use Values From Global Scope`` templateString expected =
        renderTemplate globals emptyIncludes emptyContext templateString
        |> shouldEqual expected
