open Svn

module Program = 
    open Svn.Model
    
    [<EntryPoint>]
    let main argv = 
        printfn "%A" (Configuration.svn_download_folder())
        Setup.create_if_missing (Configuration.svn_download_folder())
        printfn "%A" argv
        let repo = "http://svn.apache.org/repos/asf/"
        let name = "asf"
        Download.logs 2010 2011 name repo
        let cs = Import.logs name |> Seq.toList
        printfn "Import done."
        let users = 
            cs
            |> List.groupBy (fun c -> c.author)
            |> Map.ofList
            |> Map.map (fun _ v -> Seq.length v)
            |> Map.toList
            |> List.sortByDescending snd
            |> List.truncate 20
        
        let users_files_added = 
            let count (xs : commit seq) = 
                xs |> Seq.sumBy (fun c -> 
                          c.files
                          |> List.filter (fun q -> q.action = action.Added)
                          |> List.length)
            cs
            |> List.groupBy (fun c -> c.author)
            |> Map.ofList
            |> Map.map (fun _ v -> count v)
            |> Map.toList
            |> List.sortByDescending snd
            |> List.truncate 20
        
        let years = 
            cs
            |> List.groupBy (fun c -> c.time.Year)
            |> Map.ofList
            |> Map.map (fun _ v -> Seq.length v)
            |> Map.toList
            |> List.sortBy fst
        
        printfn "User commits"
        users
        |> List.map (printfn "%A")
        |> ignore
        printfn "Files added"
        users_files_added
        |> List.map (printfn "%A")
        |> ignore
        printfn "Year commits"
        years
        |> List.map (printfn "%A")
        |> ignore
        0 // return an integer exit code
