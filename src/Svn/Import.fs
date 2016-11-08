namespace Svn

module Import = 
    open System.IO
    open Svn.Model
    open FSharp.Data
    
    type private Svn = XmlProvider< "samples/subversion.xml" >
    
    let private file_load_pattern name = sprintf @"svn.%s.*.xml" name
    let private files name = Directory.GetFiles(Configuration.svn_download_folder(), file_load_pattern name, SearchOption.TopDirectoryOnly)
    let private openLog path = System.IO.File.ReadAllText(path)
    
    let private action (a : string) = 
        match a with
        | "A" -> action.Added
        | "M" -> action.Modified
        | "D" -> action.Deleted
        | "R" -> action.Replaced
        | _ -> raise (exn (sprintf "%s was not expected as Path action" a))
    
    let private path (e : Svn.Path) = 
        { path = e.Value
          action = action e.Action }
    
    let private logentry (e : Svn.Logentry) = 
        { revision = e.Revision
          time = e.Date
          author = e.Author
          message = e.Msg
          files = 
              e.Paths
              |> Array.map path
              |> Array.toList }
    
    let private tryMapLogentry (e : Svn.Logentry) = 
        try 
            logentry e |> Some
        with e -> 
            printfn "%A" e
            None
    
    let private mapLog (log : Svn.Log) = 
        log.Logentries
        |> Array.map tryMapLogentry
        |> Array.choose id
    
    let logs name = 
        files name
        |> Array.sortBy id
        |> Array.map openLog
        |> Array.toSeq
        |> Seq.map (Svn.Parse >> mapLog)
        |> Seq.collect id
