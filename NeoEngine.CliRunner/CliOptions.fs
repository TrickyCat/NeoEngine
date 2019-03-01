namespace TrickyCat.Text.TemplateEngines.NeoEngine.CliRunner

open CommandLine

type CliOptions () =
    [<Option('t', "template", Required = true, HelpText = "Set path to file with template.")>]
    member val TemplateFilePath = Unchecked.defaultof<string> with get, set

    [<Option('c', "context", Required = true, HelpText = "Set path to JSON file with context data needed for template execution.")>]
    member val ContextDataFilePath = Unchecked.defaultof<string> with get, set

    [<Option('i', "includes", Required = true,
        HelpText = "Set path to folder that contains all necessary 'include' files. Note: name of the include is the name of each file in specified folder without its extension.")>]
    member val IncludesFolderPath = Unchecked.defaultof<string> with get, set

    [<Option('g', "globals", Required = true,
        HelpText = "Set path to folder that contains all necessary globally available JS files.")>]
    member val GlobalsFolderPath = Unchecked.defaultof<string> with get, set

    [<Option('o', "output", Required = false, HelpText = "Set path to file with render result.")>]
    member val RenderedOutputFilePath = Unchecked.defaultof<string> with get, set

    [<Option('s', "no-console-result", Required = false, HelpText = "Suppress output of rendering result to the console.")>]
    member val SuppressOutputToConsole = Unchecked.defaultof<bool> with get, set
