using System.Collections.Generic;

namespace TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.CSharp
{
    interface IExampleService
    {
        string RenderTemplate(string template,
            IEnumerable<string> globalScopeCodeBlocks,
            IReadOnlyDictionary<string, string> includes,
            IEnumerable<KeyValuePair<string, string>> context);
    }
}
