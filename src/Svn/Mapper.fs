namespace Svn

module private Mapper = 
    open FSharp.Data
    
    type private Svn = XmlProvider< "samples/subversion.xml" >
    
    open Svn.Model
    
    let actionConvert (a : string) = 
        match a with
        | "A" -> action.Added
        | "M" -> action.Modified
        | "D" -> action.Deleted
        | "R" -> action.Replaced
        | _ -> raise (exn (sprintf "%s was not expected as Path action" a))
    
    let kind (a : string) = 
        match a with
        | "file" -> kind.File
        | "dir" -> kind.Directory
        | _ -> raise (exn (sprintf "%s was not expected as Path action" a))
    
    let path (e : Svn.Path) = 
        { path = e.Value
          kind = kind e.Kind }
    
    let private zeroChanges = 
        let defaultData = 
            [ (action.Added, [])
              (action.Modified, [])
              (action.Deleted, [])
              (action.Replaced, []) ]
        new Map<action, path list>(defaultData)
    
    let paths (e : Svn.Path array) : Map<action, path list> = 
        e
        |> Array.groupBy (fun x -> x.Action)
        |> Array.fold (fun acc (action, items) -> 
               acc.Add(actionConvert action, 
                       items
                       |> Array.map path
                       |> List.ofArray)) zeroChanges
    
    let logentry (e : Svn.Logentry) = 
        let author = defaultArg e.Author "NO_AUTHOR"
        let paths = paths e.Paths
        commit.create e.Revision e.Date author e.Msg paths
    
    let tryMapLogentry (e : Svn.Logentry) = 
        try 
            logentry e |> Some
        with e -> 
            eprintfn "%A" e.Message
            None
    
    let mapLogElement (log : Svn.Log) = 
        log.Logentries
        |> Array.map tryMapLogentry
        |> Array.choose id
    
    let tryParse (log : string) = 
        try 
            (Svn.Parse >> mapLogElement) log
        with e -> 
            eprintfn "ERR: %s" e.Message
            Array.empty
