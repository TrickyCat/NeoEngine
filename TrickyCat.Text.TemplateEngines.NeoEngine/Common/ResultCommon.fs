namespace TrickyCat.Text.TemplateEngines.NeoEngine

module ResultCommon =
    let (>>=) x f = match x with
                      Ok x    -> f x
                    | Error e -> Error e
