namespace TrickyCat.Text.TemplateEngines.NeoEngine.CliRunner

open Newtonsoft.Json
open System.Collections.Generic
open System.IO
open System
open TrickyCat.Text.TemplateEngines.NeoEngine.Utils

module IoHelpers =

    let getTemplateFromFile (templateFilePath: string) : Result<string, string> =
        if File.Exists templateFilePath then
            try
                templateFilePath |> File.ReadAllText |> Ok
            with
            | e ->
                e
                |> fullMessage
                |> sprintf "Error occured while processing template file: %s%sError: %s" templateFilePath nl
                |> Error
        else
            Error <| sprintf "Template file does not exist: %s" templateFilePath


    let getContextDataFromFile (contextFilePath: string) : Result<KeyValuePair<string, string> seq, string> =
        if File.Exists contextFilePath then
            try
                contextFilePath
                |> File.ReadAllText
                |> JsonConvert.DeserializeObject<Dictionary<string, obj>>
                |> Seq.map (fun kvp ->
                    let value =
                        if kvp.Value :? string then
                            sprintf "'%A'" kvp.Value
                        else
                            if kvp.Value :? bool then
                                kvp.Value.ToString().ToLowerInvariant()
                            else
                                kvp.Value.ToString()

                    KeyValuePair<string, string>(kvp.Key, value)
                )
                |> Ok
            with
            | e ->
                e
                |> fullMessage
                |> sprintf "Error occured while processing context data file: %s%sError: %s" contextFilePath nl
                |> Error
        else
            Error <| sprintf "Context data file does not exist: %s" contextFilePath


    let getIncludesFromFolder (includesFolderPath: string): Result<IReadOnlyDictionary<string, string>, string> =
        if Directory.Exists includesFolderPath then
            try
               includesFolderPath
               |> DirectoryInfo
               |> (fun d -> d.GetFiles("*.*"))
               |> Array.fold (fun (state: Map<string, string>) fileInfo ->
                   state
                       .Add(
                           fileInfo.Name.Substring(0,fileInfo.Name.Length - fileInfo.Extension.Length),
                           File.ReadAllText(fileInfo.FullName)
                       )
               ) Map.empty<string, string>
               |> (fun x -> x :> IReadOnlyDictionary<string, string>)
               |> Ok
            with
            | e ->
                e
                |> fullMessage
                |> sprintf "Error occured while processing includes folder: %s%sError: %s" includesFolderPath nl
                |> Error
        else
            Error <| sprintf "Includes folder does not exist: %s" includesFolderPath


    let getGlobalsFromFolder (globalsFolderPath: string): Result<string seq, string> =
        if Directory.Exists globalsFolderPath then
            try
               globalsFolderPath
               |> DirectoryInfo
               |> (fun d -> d.GetFiles("*.*"))
               |> Seq.map (fun fileInfo -> File.ReadAllText(fileInfo.FullName))
               |> Ok
            with
            | e ->
                e
                |> fullMessage
                |> sprintf "Error occured while processing globals folder: %s%sError: %s" globalsFolderPath nl
                |> Error
        else
            Error <| sprintf "Globals folder does not exist: %s" globalsFolderPath


    let writeAllTextToFile (filePath: string) (content: string): Result<unit, string> =
        try
            File.WriteAllText(filePath, content) |> Ok
        with
        | e ->
            e
            |> fullMessage
            |> sprintf "Error occured while writing to file: %s%sError: %s" filePath nl
            |> Error

    let coloredPrint color printer =
        let oldColor = Console.ForegroundColor
        Console.ForegroundColor <- color
        printer()
        Console.ForegroundColor <- oldColor
