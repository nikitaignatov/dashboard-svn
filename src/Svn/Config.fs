namespace Svn

module private Config = 
    open FSharp.Configuration
    
    type Config = YamlConfig< "config.yaml", ReadOnly=true >
    
    let Config = Config()
