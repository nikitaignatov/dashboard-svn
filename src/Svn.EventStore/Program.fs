namespace Svn.EventStore
module Program = 
    open Svn
    open Svn.Model
    [<EntryPoint>]
    let main argv = 
        printfn "%A" (Configuration.svn_download_folder())
        Setup.create_if_missing (Configuration.svn_download_folder())
