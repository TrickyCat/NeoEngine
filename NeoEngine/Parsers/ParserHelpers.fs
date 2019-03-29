namespace TrickyCat.Text.TemplateEngines.NeoEngine.Parsers

open System.Text
open ParserApi
open TrickyCat.Text.TemplateEngines.NeoEngine.Utils

module ParserHelpers =

    let stringify (template: Template) : string =

        let rec stringifier (acc: StringBuilder) = function
            | [] -> acc
            | Str s :: tail -> stringifier (acc.Append s) tail
            | Neo s :: tail -> stringifier (s |> sprintf "<%%%s%%>" |> acc.Append) tail
            | NeoSubstitute s :: tail -> stringifier (s |> sprintf "<%%=%s%%>" |> acc.Append) tail
            | NeoIncludeView s :: tail -> stringifier (s |> sprintf "<%%@ include view='%s' %%>" |> acc.Append) tail
            | NeoIfElseTemplate { condition = condition; ifBranchBody = ifs; elseBranchBody = maybeElses } :: tail ->
                let newAcc =
                    condition
                    |> sprintf "<%% if(%s) { %%>"
                    |> acc.Append
                    |> fun acc' -> stringifier acc' ifs
                    |> fun acc' ->
                        maybeElses
                        |> Option.map (fun e -> (Neo " } else { " :: e) @ [Neo " } "])
                        |> Option.defaultValue [Neo " } "]
                        |> stringifier acc'

                stringifier newAcc tail
        template
        |> stringifier (StringBuilder())
        |> toString



