namespace Svn

module Program = 
    open Svn.Model
    open Config
    
    [<EntryPoint>]
    let main argv = 
        let temp = conf.Svn.Temp.Download
        printfn "%A" temp
        Setup.create_if_missing temp
        printfn "%A" argv
        let repo = "http://svn.apache.org/repos/asf/subversion/trunk/subversion/"
        let name = "subversion"
        Download.logs 2006 (System.DateTime.Now.Year) name repo
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
                let sumFiles c = c.paths.[action].Length
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
