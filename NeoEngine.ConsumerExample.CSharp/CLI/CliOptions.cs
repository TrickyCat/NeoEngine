using CommandLine;

namespace TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.CSharp.CLI
{
    public class CliOptions
    {
        [Option('t', "template", Required = true, HelpText = "Set path to file with template.")]
        public string TemplateFilePath { get; set; }
        
        [Option('c', "context", Required = true, HelpText = "Set path to JSON file with context data needed for template execution.")]
        public string ContextDataFilePath { get; set; }

        [Option('i', "includes", Required = true,
            HelpText = "Set path to folder that contains all necessary 'include' files. Note: name of the include is the name of each file in specified folder without its extension.")]
        public string IncludesFolderPath { get; set; }

        [Option('g', "globals", Required = true,
            HelpText = "Set path to folder that contains all necessary globally available JS files.")]
        public string GlobalsFolderPath { get; set; }

        [Option('o', "output", Required = false, HelpText = "Set path to file with render result.")]
        public string RenderedOutputFilePath { get; set; }
    }
}
