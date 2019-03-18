open System.IO

let joinTemplatesBin templatesRoot outputFile =
    let rec walker (fs: Stream) (dir: DirectoryInfo) =
        dir.EnumerateFiles()
        |> Seq.iter(fun fInfo ->
            printfn "%s" fInfo.Name
            if fInfo.Name <> outputFile then
                use fStream = fInfo.OpenRead()
                fStream.CopyTo(fs)
            )
        
        dir.EnumerateDirectories()
        |> Seq.iter (walker fs)

    use fs = File.OpenWrite(Path.Combine(templatesRoot, outputFile))
    walker fs (DirectoryInfo(templatesRoot))


let joinTemplatesStr templatesRoot outputFile =
    let rec walker (fs: Stream) (dir: DirectoryInfo) =
        dir.EnumerateFiles()
        |> Seq.iter(fun fInfo ->
            printfn "%s" fInfo.Name
            if fInfo.Name <> outputFile then
                use fStream = fInfo.OpenRead()
                fStream.CopyTo(fs)
            )
        
        dir.EnumerateDirectories()
        |> Seq.iter (walker fs)

    use fs = File.OpenWrite(Path.Combine(templatesRoot, outputFile))
    walker fs (DirectoryInfo(templatesRoot))

