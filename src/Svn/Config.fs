namespace Svn

module private Config = 
    open FSharp.Configuration
    
    type Config = YamlConfig< "config.yaml" >
    
    let conf = Config()
