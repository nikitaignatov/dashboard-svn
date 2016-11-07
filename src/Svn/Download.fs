namespace Svn

module Configuration = 
    let svn_exe_path = @"C:\Program Files (x86)\Subversion\bin\svn"
    let svn_download_folder() = System.IO.Path.GetTempPath() + @"\svn_dumps\"

module Setup = 
    open System.IO
    
    let create_if_missing path = 
        if Directory.Exists path |> not then Directory.CreateDirectory(path) |> ignore
        printfn "Dir: %s" path

module Download = 
    open System
    open System.IO
    
    let private template year repo = (sprintf @"log %s --xml -v -r {%d-01-01}:{%d-12-31}" repo year year)
    let private command year repo = year, Configuration.svn_exe_path, (template year repo)
    
    let private execute name (year, cmd, argz) = 
        let path = (sprintf "%ssvn.%s.%d.xml" (Configuration.svn_download_folder()) name year)
        if (File.Exists path) then ()
        else 
            let p = new System.Diagnostics.Process()
            p.StartInfo.FileName <- cmd
            p.StartInfo.Arguments <- argz
            p.StartInfo.UseShellExecute <- false
            p.StartInfo.RedirectStandardOutput <- true
            p.StartInfo.RedirectStandardError <- true
            p.Start() |> ignore
            let err = p.StandardError.ReadToEnd()
            if (String.IsNullOrWhiteSpace(err)) then File.WriteAllText(path, p.StandardOutput.ReadToEnd())
            else Console.Write(err)
        printfn "DONE: %d" year
    
    let private logs_range p1 p2 name repo = 
        [ p1..p2 ]
        |> List.map command
        |> List.map (fun f -> f repo)
        |> List.map (execute name)
        |> ignore
        printfn "Downloaded all logs."
    
    let logs p1 p2 name repo = 
        try 
            logs_range p1 p2 name repo
        with e -> printfn "Error when downloading logs: %A" e
