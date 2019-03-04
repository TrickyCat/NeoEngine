namespace TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters

open System

module InterpreterBase =
    // TODO: specify other use cases!

    /// <summary>
    /// Abstracts the interpreter being used for template's rendering.
    /// Inherits <see cref="System.IDisposable" /> thus expecting that each interpreter implementation's instance 
    /// should be used for rendering of single template and then explicitly disposed.
    ///
    /// Its methods execute the provided strings with JavaScript and return
    /// either a value in case of success or a string with an error message in case of failure.
    ///
    /// <see cref="System.IDisposable.Dispose" /> method implementation should clean up JS session values.
    /// </summary>
    type IInterpreter =
        inherit IDisposable
        abstract Run      : string -> Result<unit, string>
        abstract Eval     : string -> Result<obj, string>
        abstract Eval<'a> : string -> Result<'a, string>
