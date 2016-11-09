namespace Svn

module Import = 
    open FSharp.Data
    open System.IO
    open Config
    
    type private Svn = XmlProvider< "samples/subversion.xml" >
    
    module private Mapper = 
        open Svn.Model
        
        let action (a : string) = 
            match a with
            | "A" -> action.Added
            | "M" -> action.Modified
            | "D" -> action.Deleted
            | "R" -> action.Replaced
            | _ -> raise (exn (sprintf "%s was not expected as Path action" a))
        
        let path (e : Svn.Path) = 
            { path = e.Value
              action = action e.Action }
        
        let logentry (e : Svn.Logentry) = 
            { revision = e.Revision
              time = e.Date
              author = e.Author
              message = e.Msg
              files = 
                  e.Paths
                  |> Array.map path
                  |> Array.toList }
    
    let private file_load_pattern name = sprintf @"svn.%s.*.xml" name
    let private files name = Directory.GetFiles(conf.Svn.Temp.Download, file_load_pattern name, SearchOption.TopDirectoryOnly)
    let private openLog path = File.ReadAllText(path)
    
    let private tryMapLogentry (e : Svn.Logentry) = 
        try 
            Mapper.logentry e |> Some
        with e -> 
            eprintfn "%A" e.Message
            None
    
    let private mapLog (log : Svn.Log) = 
        log.Logentries
        |> Array.map tryMapLogentry
        |> Array.choose id
    
    let private tryParse (log : string) = 
        try 
            (Svn.Parse >> mapLog) log
        with e -> 
            eprintfn "ERR: %s" e.Message
            Array.empty
    
    let logs name = 
        files name
        |> Array.sortBy id
        |> Array.map openLog
        |> Array.toSeq
        |> Seq.map tryParse
        |> Seq.collect id
