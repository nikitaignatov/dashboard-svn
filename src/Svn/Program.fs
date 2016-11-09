namespace Svn

module Program = 
    open Svn.Model
    open Config
    open System
    
    let users cs = 
        cs
        |> List.groupBy (fun c -> c.author)
        |> Map.ofList
        |> Map.map (fun _ v -> Seq.length v)
        |> Map.toList
        |> List.sortByDescending snd
        |> List.truncate 20
    
    let users_files_added cs action = 
        let count (xs : commit seq) = 
            let sumFiles c = c.paths.[action].Length
            xs |> Seq.sumBy sumFiles
        cs
        |> List.groupBy (fun c -> c.author)
        |> Map.ofList
        |> Map.map (fun _ v -> count v)
        |> Map.toList
        |> List.sortByDescending snd
        |> List.truncate 20
    
    let years cs = 
        cs
        |> List.groupBy (fun c -> c.time.Year)
        |> Map.ofList
        |> Map.map (fun _ v -> Seq.length v)
        |> Map.toList
        |> List.sortBy fst
    
    let title = printfn "\n# %s\n\n"
    
    [<EntryPoint>]
    let main argv = 
        let temp = Config.Svn.Temp.Download
        printfn "%A" temp
        Setup.create_if_missing temp
        printfn "%A" argv
        let repo = "http://svn.apache.org/repos/asf/subversion/trunk/subversion/"
        let name = "subversion"
        Download.logs 2006 (DateTime.Now.Year) name repo
        title "Donwload done."
        let cs = Import.logs name |> Seq.toList
        title "Import done."
        title "number of commits by user"
        printfn "|user|commits|\n|---|---|"
        users cs |> List.map (fun (u, c) -> (printfn "|%s|%d|" u c))
        title "number of files added by user"
        printfn "|user|added|\n|---|---|"
        users_files_added cs (action.Added) |> List.map (fun (u, c) -> (printfn "|%s|%d|" u c))
        title "number of files deleted by user"
        printfn "|user|deleted|\n|---|---|"
        users_files_added cs (action.Deleted) |> List.map (fun (u, c) -> (printfn "|%s|%d|" u c))
        title "number of commits for each year"
        printfn "|year|commits|\n|---|---|"
        years cs |> List.map (fun (y, c) -> (printfn "|%d|%d|" y c))
        0
