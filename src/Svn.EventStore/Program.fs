namespace Svn.EventStore

module Program = 
    open Svn
    open Svn.Model
    open System
    open EventStore.ClientAPI
    open Newtonsoft.Json
    
    let conn (url : Net.IPEndPoint) = 
        let conn = EventStoreConnection.Create(url)
        conn.ConnectAsync() |> ignore
        conn
    
    [<EntryPoint>]
    let main argv = 
        // define repository to import
        let repo = "http://svn.apache.org/repos/asf/subversion"
        let name = "subversion"
        Download.logs 2010 (System.DateTime.Now.Year) name repo
        
        // load commits into memory
        let cs = Import.logs name |> Seq.toList
        printfn "commits loaded in memory"

        // convert single to commit to eventstore event
        let commitToEvent (commit : commit) = 
            let meta : byte array = [||]
            let data = JsonConvert.SerializeObject(commit, Formatting.Indented) |> System.Text.Encoding.UTF8.GetBytes
            new EventData(Guid.NewGuid(), "commit", true, data, meta)
        
        use connection = conn (new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 1113))
        
        let submit name (cs : commit list) = 
            let sw = System.Diagnostics.Stopwatch.StartNew()
            let data = List.map commitToEvent cs
            printfn "Submitting %d events" data.Length
            let stream = "svn-commits-" + name
            async { 
                return! connection.AppendToStreamAsync(stream, ExpectedVersion.Any, data)
                        |> Async.AwaitIAsyncResult
                        |> Async.Ignore
            }
            |> Async.RunSynchronously
            printfn "submitted %f seconds" sw.Elapsed.TotalSeconds
        
        let execute name chunkSize (cs : commit list) = 
            printfn "Inserting %d commits" cs.Length
            let sw = System.Diagnostics.Stopwatch.StartNew()
            let data = cs |> List.chunkBySize chunkSize
            printfn "Total partitions: %d" data.Length
            List.iteri (fun i items -> 
                submit name items
                printfn "Stored %d %d, remaining %d" i items.Length (data.Length - i)) data
            printfn "Insertion complete in %f seconds" sw.Elapsed.TotalSeconds
        
        // execute
        execute name 1000 cs
        0
