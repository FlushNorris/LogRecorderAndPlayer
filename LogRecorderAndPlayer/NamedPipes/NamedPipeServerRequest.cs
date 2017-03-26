using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public enum NamedPipeServerRequestType
    {
        SyncBrowser = 0,
        ClosingBrowser = 1
    }

    [DataContract]
    [KnownType(typeof(NamedPipeBrowser))]
    public class NamedPipeServerRequest
    {
        [DataMember]
        public NamedPipeServerRequestType Type { get; set; }
        
        [DataMember]        
        public object Data { get; set; }
    }
}
