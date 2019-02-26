namespace TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.FSharp

module FakeEmailSender =

    let private nl = System.Environment.NewLine
    let private delim = String.replicate 20 "-"

    let sendEmail body =
        printfn "%s%s%sSending email%s%s%s%s%s" delim nl nl nl nl body nl delim

