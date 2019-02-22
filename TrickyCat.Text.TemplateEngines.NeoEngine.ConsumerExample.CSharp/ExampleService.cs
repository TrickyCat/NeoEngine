using System;
using System.Collections.Generic;
using TrickyCat.Text.TemplateEngines.NeoEngine.Services;

namespace TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.CSharp
{
    class ExampleService: IExampleService
    {
        private readonly ITemplateService _templateService;

        public ExampleService(ITemplateService templateService)
        {
            _templateService = templateService;
        }
        
        public string RenderTemplate(string template, IEnumerable<string> globalScopeCodeBlocks, IReadOnlyDictionary<string, string> includes, IEnumerable<KeyValuePair<string, string>> context)
        {
            var result = _templateService.RenderTemplateString(globalScopeCodeBlocks, includes, template, context);
            if (result.IsOk)
            {
                return result.ResultValue;
            }
            Log(result.ErrorValue);
            return string.Empty;
        }

        private void Log(string message) => Console.WriteLine(message);
    }
}
