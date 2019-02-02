namespace TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters

open System

module InterpreterBase =
    type IInterpreter =
        inherit IDisposable
        abstract Run      : string -> unit
        abstract Eval     : string -> obj
        abstract Eval<'a> : string -> 'a option
