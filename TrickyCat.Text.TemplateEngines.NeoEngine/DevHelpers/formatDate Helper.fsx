open System.IO
open System

type NumSpec = {
    maxNum: int
    allowsLeadZero: bool
}

type FChar = {
    char: char
    allowsNum: NumSpec option
    canBeFollowedByIandL: bool
    desc: string
}

let chars =
    Map.empty<char, FChar>
        .Add('Y', { char = 'Y'; allowsNum = Some({ maxNum = (*4*)7; allowsLeadZero = false }); canBeFollowedByIandL = false; desc = "Year" })
        .Add('M', { char = 'M'; allowsNum = Some({ maxNum = (*2*)4; allowsLeadZero = false }); canBeFollowedByIandL = false; desc = "Month of the year (1-12)" })
        .Add('B', { char = 'B'; allowsNum = None;                                         canBeFollowedByIandL = true ; desc = "Month name" })
        .Add('D', { char = 'D'; allowsNum = Some({ maxNum = (*2*)4; allowsLeadZero = false }); canBeFollowedByIandL = false; desc = "Day of the month (1-31)" })
        .Add('A', { char = 'A'; allowsNum = None;                                         canBeFollowedByIandL = true ; desc = "Day name" })
        .Add('J', { char = 'J'; allowsNum = Some({ maxNum = (*3*)4; allowsLeadZero = false }); canBeFollowedByIandL = false; desc = "Day of the year (1-366)" })
        .Add('W', { char = 'W'; allowsNum = Some({ maxNum = (* 2 *)4; allowsLeadZero = false }); canBeFollowedByIandL = false; desc = "Week of the year" })
        .Add('H', { char = 'H'; allowsNum = Some({ maxNum = (* 2 *)4; allowsLeadZero = true });  canBeFollowedByIandL = false; desc = "Hour (0-23)" })
        .Add('I', { char = 'I'; allowsNum = Some({ maxNum = (* 2 *)4; allowsLeadZero = true });  canBeFollowedByIandL = false; desc = "Hour (1-12)" })
        .Add('N', { char = 'N'; allowsNum = Some({ maxNum = (* 2 *)4; allowsLeadZero = true });  canBeFollowedByIandL = false; desc = "Minutes (0-59)" })
        .Add('S', { char = 'S'; allowsNum = Some({ maxNum = (* 2 *)4; allowsLeadZero = true });  canBeFollowedByIandL = false; desc = "Seconds (0-59)" })
        .Add('P', { char = 'P'; allowsNum = None;                                         canBeFollowedByIandL = false; desc = "AM/PM" })


let withNumberSpecifier maxNum (coll: string seq) =
    coll
    |> Seq.collect(fun s -> seq {
        yield s
        for i in 1 .. maxNum do
            yield sprintf "%d%s" i s
        })

let withLeadZero addLeadZero (coll: string seq) =
    coll
    |> Seq.collect(fun s -> seq {
        yield s
        if addLeadZero then yield sprintf "0%s" s
        })

let withIsAndLs canBeFollowedByIAndL (coll: string seq) =
    coll
    |> Seq.collect(fun s -> seq {
        yield s
        if canBeFollowedByIAndL then
            yield sprintf "%si" s
            yield sprintf "%sl" s
    })

let withNumSpec maybeNumSpec (coll: string seq) =
    match maybeNumSpec with
    | None                                                       -> coll
    | Some({ maxNum = maxNum; allowsLeadZero = allowsLeadZero }) -> coll
                                                                    |> withNumberSpecifier maxNum
                                                                    |> withLeadZero allowsLeadZero

let handleChar ({ char = c; allowsNum = allowsNum; canBeFollowedByIandL = canBeFollowedByIAndL }) (coll: string seq) = seq {
    yield! coll
    yield! c
           |> string
           |> Seq.singleton
           |> withNumSpec allowsNum
           |> withIsAndLs canBeFollowedByIAndL
    }

let singleParams () =
    Map.fold (fun state _ v -> handleChar v state) Seq.empty<string> chars
    |> Seq.map (sprintf "%%%s")

let toNeoSub = sprintf "<%%= %s %%>"

let toDataRows dateVar : string seq -> string seq =
    Seq.map (fun s -> sprintf """
                <tr>
                    <td>%s</td>
                    <td>%s</td>
                </tr>""" s (
                            s
                            |> sprintf "formatDate(%s, \"%s\")" dateVar
                            |> toNeoSub
                    ) )

let randomFormats qtyPerPattern qty (coll: string seq) =
    let collA = Array.ofSeq coll
    let length = collA.Length
    let rnd = new Random()
    let picker () = rnd.Next(length) |> Array.get collA

    {1..qty}
    |> Seq.map (fun _ ->
        [1..qtyPerPattern]
        |> List.fold (fun acc _ -> sprintf "%s %s" acc (picker())) ""
        )

let toFile (valueGenerator: unit -> string seq) fileName =
    let date = "var d = new Date(1548261685158);"
    let dateInit = sprintf "<%% %s %%>" date
    let header = sprintf "<html><body><p>%s</p><p>%s</p><table>" date (toNeoSub "d.toString()")
    let calls =
        valueGenerator()
        |> toDataRows "d"
    let footer = "</table></body></html>"

    File.WriteAllLines(
        fileName,
        seq {
            yield dateInit
            yield header
            yield! calls
            yield footer
            }
    )

let singleFormatsAndMultiple qtyPerPattern qty = seq {
    let single = singleParams()
    let multi = single |> randomFormats qtyPerPattern qty
    yield! single
    yield! multi
    }

let singleToFile = toFile singleParams
let multipleToFile qtyPerPattern qty = toFile (singleParams >> (randomFormats qtyPerPattern qty))
let singleAndMultiToFile qtyPerPattern qty = toFile (fun () -> singleFormatsAndMultiple qtyPerPattern qty)

