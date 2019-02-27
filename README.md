# NeoEngine
[![Build status](https://ci.appveyor.com/api/projects/status/95sj57qphfy2p0xm?svg=true)](https://ci.appveyor.com/project/TrickyCat/neoengine)
![F# |> I❤️](https://raw.githubusercontent.com/TrickyCat/NeoEngine/dev/docs/img/I_Heart_Fsharp_Long_Black_100x25.png)

A general purpose template engine for .NET platform that allows you to customize your templates with JS or basically execute arbitrary JS code snippets from within your .NET applications.

Proudly built with F#.

## Template Engine's Architecture

![High Level Engine's Architecture](https://raw.githubusercontent.com/TrickyCat/NeoEngine/dev/docs/img/template-engine/architecture_high_level_001.jpg)

## Workflow
* Engine's **Parser component** takes a template string, parses it, and builds an abstract syntax tree (AST) based on its content
* Some AST optimizations are performed
* Engine's **Runner component** takes
  * an optimized AST
  * globals
  * includes lookup dictionary
  * context data

  and traverses the AST generating the result string (aka rendered template or render result).

## Template Engine's Glossary

* **globals** - are script files which define global execution scope for scripts inside templates and includes. For example, it can provide a formatDate function which is used inside templates or any other globally available functionality similarly to the browser's execution environment in which global objects like window or document are present by default in script execution context.

* **includes** - a lookup dictionary for resolution of includes being referenced from the template. Syntactically they are also templates.
* **context** data - conceptually it must be the only varying part during multiple invocations of the engine for rendering the same template. It's a dictionary of named values (similar to JSON objects) available for lookup from the template or include.
* **template** - a string which alongside the string literals may or may not contain some customization blocks