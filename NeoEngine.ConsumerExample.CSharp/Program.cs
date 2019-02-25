using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        static void Main(string[] args)
        {
            var container = ConfigureContainer(new UnityContainer());
            var svc = container.Resolve<IExampleService>();

            TestCli(svc, args);
            //MemoryTest(svc);
            //PasswordResetPerfTest(svc);
            Console.ReadLine();
        }

        static void MemoryTest(IExampleService svc)
        {
            var templateFileName =     "PasswordReset.html";
            var templateFilePath =   $@"c:\Neo\Templates\PasswordReset\{templateFileName}";
            var contextDataFilePath = @"c:\Neo\test.tmpl.data";
            var includesFolderPath =  @"c:\Neo\Includes";
            var globalsFolderPath =   @"c:\Neo\Global Scope";

            var template = GetTemplate(templateFilePath);
            var globals = GetGlobalsFromFolder(globalsFolderPath);
            var includes = GetIncludesFromFolder(includesFolderPath);
            var context = GetContextData(contextDataFilePath);

            #region Warm Up

            Console.WriteLine("About to warm up.");
            Console.ReadLine();
            Console.WriteLine("Warming up...");
            for (int i = 0; i < 1000; i++)
            {
                RunRenderJob(svc, template, globals, includes, context, noConsole: true);
            }
            Console.WriteLine("Warmed up.");

            #endregion

            #region Run

            var n = 100_000_000_000L;
            Console.WriteLine($"About to run {n} times.");
            Console.ReadLine();
            Console.WriteLine("Running...");
            for (var i = 0L; i < n; i++)
            {
                RunRenderJob(svc, template, globals, includes, context, noConsole: true);
            }
            Console.WriteLine("Run.");

            #endregion

            Console.ReadLine();
        }

        static void PasswordResetPerfTest(IExampleService svc, int qtyOfRenderings = 1000)
        {
            var templateFileName =     "PasswordReset.html";
            var templateFilePath =   $@"c:\Neo\Templates\PasswordReset\{templateFileName}";
            var contextDataFilePath = @"c:\Neo\test.tmpl.data";
            var includesFolderPath =  @"c:\Neo\Includes";
            var globalsFolderPath =   @"c:\Neo\Global Scope";

            var template = GetTemplate(templateFilePath);
            var globals = GetGlobalsFromFolder(globalsFolderPath);
            var includes = GetIncludesFromFolder(includesFolderPath);
            var context = GetContextData(contextDataFilePath);

            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < qtyOfRenderings; i++)
            {
                RunRenderJob(svc, template, globals, includes, context, noConsole: true);
            }
            sw.Stop();
            var ms = (double) sw.ElapsedMilliseconds;
            Console.WriteLine($@"Rendered [{templateFileName}] {qtyOfRenderings} times.
Execution time:
Overall     : {ms} ms
Per template: {ms / qtyOfRenderings} ms");
        }

        static IUnityContainer ConfigureContainer(IUnityContainer container) =>
            container
                .RegisterType<ITemplateService, TemplateService>()
                .RegisterType<IExampleService, ExampleService>();

        static void TestCli(IExampleService svc, string[] args) =>
            Parser
                .Default
                .ParseArguments<CliOptions>(args)
                .WithParsed(o =>
                {
                    TestTemplateFile(svc, o.TemplateFilePath, o.ContextDataFilePath, o.IncludesFolderPath, o.GlobalsFolderPath, o.RenderedOutputFilePath);
                });

        static void TestTemplateString(IExampleService svc)
        {
            var template = @"
    <html>
<%
function myFunc(x) { return x * 100 + 23; }
%>
        <head>
            <title>
                <%= myFunc(1) + ' some string ' + myTitle + '_ADDED' %>
                And another instance of the title: <%= myTitle %>
            </title>
        </head>

        <body>
        <p>
            <% if (2 > 1) { %>
            Hello, <%= name %>!
            <% } else { %>
            FALSE
            <% } %>
        </p>
        <p>
            Some text: 報酬アップ
        </p>
        <p>
            Some other text 
        </p>
        </body>
    <html>
    ";
            var includes = new Dictionary<string, string>();
            var globals = new ReadOnlyCollection<string>(new List<string>());
            var context = new Dictionary<string, string>()
            {
                {"myTitle", "Some awesome title with funky characters 報酬アップ and other chars तकनिकल"},
                {"name", "Bobby तकनिकल Doe"}
            };

            RunRenderJob(svc, template, globals, includes, context);
        }

        static IEnumerable<KeyValuePair<string, string>> GetContextData(string contextFilePath)
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
            return context;
        }

        static string GetTemplate(string templateFilePath) =>
            File.Exists(templateFilePath)
                ? File.ReadAllText(templateFilePath)
                : string.Empty;

        static void TestTemplateFile(IExampleService svc, string templateFilePath, string contextFilePath, string includesFolderPath, 
            string globalFolderPath, string renderedOutputFilePath)
        {
            var template = GetTemplate(templateFilePath);
            var context = GetContextData(contextFilePath);
            var includes = GetIncludesFromFolder(includesFolderPath);
            var globals = GetGlobalsFromFolder(globalFolderPath);

            var renderResult = RunRenderJob(svc, template, globals, includes, context);

            if (!string.IsNullOrWhiteSpace(renderedOutputFilePath))
            {
                File.WriteAllText(renderedOutputFilePath, renderResult); 
            }
        }

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

        static string RunRenderJob(IExampleService svc, string template, IEnumerable<string> globals, IReadOnlyDictionary<string, string> includes, IEnumerable<KeyValuePair<string, string>> context, bool noConsole = false)
        {
            var renderResult = svc.RenderTemplate(template, globals, includes, context);
            if (!noConsole && renderResult != string.Empty)
            {
                Console.WriteLine($"Render result:{Environment.NewLine}{renderResult}");
            }
            return renderResult;
        }

    }
}
