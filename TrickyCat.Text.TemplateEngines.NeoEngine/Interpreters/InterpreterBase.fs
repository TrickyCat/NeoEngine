namespace TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters

open System

module InterpreterBase =
    type IInterpreter =
        inherit IDisposable
        abstract Run      : string -> Result<unit, string>
        abstract Eval     : string -> Result<obj, string>
        abstract Eval<'a> : string -> Result<'a, string>
