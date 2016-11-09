namespace Svn

module Model = 
    type private time = System.DateTime
    
    type action = 
        | Added
        | Modified
        | Deleted
        | Replaced
    
    type kind = 
        | File
        | Directory
    
    type path = 
        { path : string
          kind : kind }
    
    type commit = 
        { revision : int
          time : time
          author : string
          message : string
          paths : Map<action, path list> }
        static member create revision time author message paths = 
            { revision = revision
              time = time
              author = author
              message = message
              paths = paths }
