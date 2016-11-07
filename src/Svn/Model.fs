namespace Svn

module Model = 
    open FSharp.Data
    
    type time = System.DateTime
    
    type Svn = XmlProvider< "samples/subversion.xml", Global=true >
    
    type action = 
        | Added
        | Modified
        | Deleted
        | Replaced
    
    type file = 
        { path : string
          action : action }
    
    type commit = 
        { revision : int
          time : time
          author : string option
          message : string
          files : file list }
