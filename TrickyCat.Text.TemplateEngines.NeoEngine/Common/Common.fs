namespace TrickyCat.Text.TemplateEngines.NeoEngine

open System.Text

module Common =
    let toString x = x.ToString()

    let fullMessage (e: exn) =
        let rec fullMessage' (sb: StringBuilder) (e: exn) =
            if e = null then 
                sb
            else
                fullMessage' (sb.AppendLine e.Message) e.InnerException

        e
        |> fullMessage' (new StringBuilder())
        |> toString
