namespace Svn

module Setup = 
    open System.IO
    
    let create_if_missing path = 
        if Directory.Exists path |> not then Directory.CreateDirectory(path) |> fun x -> printfn "Created dir: %A" x.FullName
        else printfn "folder already exists"

module Download = 
    open System
    open System.IO
    open Config
    
    let private template year repo = (sprintf @"log %s --xml -v -r {%d-01-01}:{%d-12-31}" repo year year)
    let private command year repo = year, Config.Svn.Exe, (template year repo)
    
    let private execute name (year, cmd, argz) = 
        let file = (sprintf "svn.%s.%d.xml" name year)
        let path = Path.Combine((Config.Svn.Temp.Download), file)
        if (File.Exists path) then ()
        else 
            let p = new System.Diagnostics.Process()
            p.StartInfo.FileName <- cmd
            p.StartInfo.Arguments <- argz
            p.StartInfo.UseShellExecute <- false
            p.StartInfo.RedirectStandardOutput <- true
            p.StartInfo.RedirectStandardError <- true
            p.Start() |> ignore
            File.WriteAllText(path, p.StandardOutput.ReadToEnd())
        printfn "DONE: %d" year
    
    let private logs_range p1 p2 name repo = 
        [ p1..p2 ]
        |> List.map command
        |> List.map (fun f -> f repo)
        |> List.map (execute name)
        |> ignore
        printfn "Download for %A %A %s %s is complete" p1 p2 name repo
    
    let logs p1 p2 name repo = 
        try 
            logs_range p1 p2 name repo
        with e -> printfn "Error when downloading logs: %A" e
