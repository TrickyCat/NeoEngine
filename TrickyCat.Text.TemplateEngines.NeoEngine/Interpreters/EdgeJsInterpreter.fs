namespace TrickyCat.Text.TemplateEngines.NeoEngine.Interpreters

open InterpreterBase
open EdgeJs
open System

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
            |> (fun t -> t.Result)
            
        interface IInterpreter with
            member __.Run jsString =
                jsString
                |> exec false
                |> ignore

            member __.Eval jsString =
                jsString
                |> exec false

            member __.Eval<'a> jsString =
                jsString
                |> exec false
                |> (fun x -> try Some (Convert.ChangeType(x, typeof<'a>) :?> 'a) with _ -> None)

            member __.Dispose() = exec true String.Empty |> ignore
