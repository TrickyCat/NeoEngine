namespace TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters

open InterpreterBase
open EdgeJs
open System
open TrickyCat.Text.TemplateEngines.NeoEngine.Common
open TrickyCat.Text.TemplateEngines.NeoEngine.ExecutionResults.Errors

module EdgeJsInterpreter =
    type EdgeJsInterpreter() =
        let contextId = Guid.NewGuid().ToString()
        let fn = Edge.Func(@"
        const vm = require('vm');

        const contexts = {};
        const createContext = id => contexts[id] = vm.createContext({});
        const getContext    = id => contexts[id] || createContext(id);
        const dropContext   = id => contexts[id] = null;
        
        const exec = (script, contextId) => new vm.Script(script).runInContext(getContext(contextId));

        return function (data, callback) {
            const { script, contextId, drop } = data;
            if (drop) {
                dropContext(contextId);
                callback(null, `Context ${contextId} dropped.`);
            } else {
                const result = exec(script, contextId);
                callback(null, result);
            }
        }
        ")
        let fnArg = Map.empty<string, obj>
                       .Add("contextId", contextId)
    
        let exec drop (jsString: string) =
            fnArg
               .Add("script", jsString)
               .Add("drop", drop)
            |> fn.Invoke
            |> (fun t -> try t.Result |> Ok with e -> e.InnerException |> fullMessage |> jsError |> Error)

        interface IInterpreter with
            member __.Run jsString =
                jsString
                |> exec false
                |> Result.map (fun _ -> ())

            member __.Eval jsString =
                jsString
                |> exec false

            member __.Eval<'a> jsString =
                jsString
                |> exec false
                |> Result.bind (fun x ->
                    try
                        Convert.ChangeType(x, typeof<'a>) :?> 'a |> Ok 
                    with
                        e ->
                          let msg = sprintf "Interpreter module: type conversion error.\nExpected type: %s\nActual value: %A\nDetails: %s" typeof<'a>.FullName x (fullMessage e)
                          msg |> error |> Error)

            member __.Dispose() = exec true String.Empty |> ignore
