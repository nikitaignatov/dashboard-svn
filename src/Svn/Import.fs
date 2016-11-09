namespace Svn

module Import = 
    open System.IO
    open Config
    
    let private file_load_pattern name = sprintf @"svn.%s.*.xml" name
    let private files name = Directory.GetFiles(Config.Svn.Temp.Download, file_load_pattern name, SearchOption.TopDirectoryOnly)
    let private openLog path = File.ReadAllText(path)
    
    let logs name = 
        files name
        |> Array.sortBy id
        |> Array.map openLog
        |> Array.toSeq
        |> Seq.map Mapper.tryParse
        |> Seq.collect id
