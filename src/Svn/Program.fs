open Svn

module Program = 
    open Svn.Model
    open System
    
    [<EntryPoint>]
    let main argv = 
        printfn "%A" (Configuration.svn_download_folder())
        Setup.create_if_missing (Configuration.svn_download_folder())
        printfn "%A" argv
        let repo = "http://svn.apache.org/repos/asf/subversion"
        let name = "subversion"
        Download.logs 2010 (System.DateTime.Now.Year) name repo
        printfn "Donwload done."
        let cs = Import.logs name |> Seq.toList
        printfn "Import done."
        let users() = 
            cs
            |> List.groupBy (fun c -> c.author)
            |> Map.ofList
            |> Map.map (fun _ v -> Seq.length v)
            |> Map.toList
            |> List.sortByDescending snd
            |> List.truncate 20
        
        let users_files_added (action) = 
            let count (xs : commit seq) = 
                let sumFiles c = 
                    c.files
                    |> List.filter (fun q -> q.action = action)
                    |> List.length
                xs |> Seq.sumBy sumFiles
            cs
            |> List.groupBy (fun c -> c.author)
            |> Map.ofList
            |> Map.map (fun _ v -> count v)
            |> Map.toList
            |> List.sortByDescending snd
            |> List.truncate 20
        
        let years() = 
            cs
            |> List.groupBy (fun c -> c.time.Year)
            |> Map.ofList
            |> Map.map (fun _ v -> Seq.length v)
            |> Map.toList
            |> List.sortBy fst
        
        printfn "User commits"
        users()
        |> List.map (printfn "%A")
        |> ignore
        printfn "Files added"
        users_files_added (action.Added)
        |> List.map (printfn "%A")
        |> ignore
        printfn "Files deleted"
        users_files_added (action.Deleted)
        |> List.map (printfn "%A")
        |> ignore
        printfn "Year commits"
        years()
        |> List.map (printfn "%A")
        |> ignore
        0
