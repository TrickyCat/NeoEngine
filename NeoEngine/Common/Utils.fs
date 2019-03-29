namespace TrickyCat.Text.TemplateEngines.NeoEngine

open System
open System.Text

module Utils =

    let nl = Environment.NewLine

    let toString x = x.ToString()

    let trim (s: string) = s.Trim()

    let fullMessage (e: exn) =
        let rec fullMessage' (sb: StringBuilder) (e: exn) =
            if e = null then 
                sb
            else
                fullMessage' (sb.AppendLine e.Message) e.InnerException

        e
        |> fullMessage' (new StringBuilder())
        |> toString
