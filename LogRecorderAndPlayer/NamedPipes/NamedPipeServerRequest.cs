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
        SyncSession = 0,
        ClosingSession = 1,
        BrowserJob = 2,
        BrowserJobComplete = 3,
        FetchLogElement = 4
    }

    [DataContract]
    [KnownType(typeof(NamedPipeSession))]
    [KnownType(typeof(LogElementDTO))]
    [KnownType(typeof(NamedPipeBrowserJob))]
    [KnownType(typeof(NamedPipeFetchLogElement))]
    public class NamedPipeServerRequest
    {
        [DataMember]
        public NamedPipeServerRequestType Type { get; set; }
        
        [DataMember]        
        public object Data { get; set; }
    }
}
