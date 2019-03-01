using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommandLine;
using Newtonsoft.Json;
using TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.CSharp.CLI;
using TrickyCat.Text.TemplateEngines.NeoEngine.Services;
using Unity;

namespace TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.CSharp
{
    class Program
    {
        static void Main(string[] args) =>
            TestCli(
                ConfigureContainer(new UnityContainer()).Resolve<IExampleService>(),
                args);

        static IUnityContainer ConfigureContainer(IUnityContainer container) =>
            container
                .RegisterType<ITemplateService, TemplateService>()
                .RegisterType<IExampleService, ExampleService>();

        // ok
        static void TestCli(IExampleService svc, string[] args) =>
            Parser
                .Default
                .ParseArguments<CliOptions>(args)
                .WithParsed(o =>
                {
                    TestTemplateFile(svc, o.TemplateFilePath, o.ContextDataFilePath, o.IncludesFolderPath,
                        o.GlobalsFolderPath, o.RenderedOutputFilePath, o.SuppressOutputToConsole);
                });

        // ok
        static (bool, IEnumerable<KeyValuePair<string, string>>) GetContextDataFromFile(string contextFilePath)
        {
            if (!File.Exists(contextFilePath))
            {
                return (false, Enumerable.Empty<KeyValuePair<string, string>>());
            }
            try
            {
                var contextJson = File.ReadAllText(contextFilePath);
                var context = JsonConvert.DeserializeObject<Dictionary<string, object>>(contextJson)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value is string
                                ? $"'{kvp.Value}'"
                                : (kvp.Value is bool
                                    ? kvp.Value.ToString().ToLowerInvariant()
                                    : kvp.Value.ToString())
                    );
                return (true, context);
            }
            catch (Exception)
            {
                return (false, Enumerable.Empty<KeyValuePair<string, string>>());
            }
        }

        // ok
        static (bool, string) GetTemplate(string templateFilePath)
        {
            try
            {
                return File.Exists(templateFilePath)
                        ? (true, File.ReadAllText(templateFilePath))
                        : (false, string.Empty);
            }
            catch (Exception)
            {
                return (false, "");
            }
        }

        static void TestTemplateFile(IExampleService svc, string templateFilePath, string contextFilePath, string includesFolderPath, 
            string globalFolderPath, string renderedOutputFilePath, bool suppressResultToConsole)
        {
            var (templateOk, template) = GetTemplate(templateFilePath);
            if (!templateOk)
            {
                Console.WriteLine($"Template was not found at location: {templateFilePath}.");
                return;
            }
            var (contextOk, context) = GetContextDataFromFile(contextFilePath);
            if (!contextOk)
            {
                Console.WriteLine($"Problem with context data file at location: {contextFilePath}.");
                return;
            }
            var includes = GetIncludesFromFolder(includesFolderPath);
            var globals = GetGlobalsFromFolder(globalFolderPath);

            var renderResult = RunRenderJob(svc, template, globals, includes, context, suppressResultToConsole);

            if (!string.IsNullOrWhiteSpace(renderedOutputFilePath))
            {
                File.WriteAllText(renderedOutputFilePath, renderResult); 
            }
        }

        // ok
        static IReadOnlyDictionary<string, string> GetIncludesFromFolder(string includesFolderPath)
        {
            if (!Directory.Exists(includesFolderPath))
            {
                return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
            }
            var dict =
                new DirectoryInfo(includesFolderPath)
                    .GetFiles("*.*")
                    .ToDictionary(
                        fileInfo => fileInfo.Name.Substring(
                            0,
                            fileInfo.Name.Length - fileInfo.Extension.Length
                        ),
                        fileInfo => File.ReadAllText(fileInfo.FullName));
            return new ReadOnlyDictionary<string, string>(dict);
        }

        // ok
        static IEnumerable<string> GetGlobalsFromFolder(string globalsFolderPath)
        {
            if (!Directory.Exists(globalsFolderPath))
            {
                return Enumerable.Empty<string>();
            }
            var globals =
                new DirectoryInfo(globalsFolderPath)
                    .GetFiles("*.*")
                    .Select(fileInfo => File.ReadAllText(fileInfo.FullName))
                    .ToList();
            return new ReadOnlyCollection<string>(globals);
        }

        static string RunRenderJob(IExampleService svc, string template, IEnumerable<string> globals, IReadOnlyDictionary<string, string> includes,
            IEnumerable<KeyValuePair<string, string>> context, bool suppressResultToConsole)
        {
            var renderResult = svc.RenderTemplate(template, globals, includes, context);
            if (!suppressResultToConsole && renderResult != string.Empty)
            {
                Console.WriteLine($"Render result:{Environment.NewLine}{renderResult}");
            }
            return renderResult;
        }

    }
}
