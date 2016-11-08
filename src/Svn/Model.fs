namespace Svn

module Model = 
    type time = System.DateTime
    
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
