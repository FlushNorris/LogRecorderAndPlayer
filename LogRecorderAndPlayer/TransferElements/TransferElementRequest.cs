using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public enum TransferElementRequestType
    {
        SyncSession = 0,
        ClosingSession = 1,
        BrowserJob = 2,
        BrowserJobComplete = 3,
        FetchLogElement = 4
    }

    [DataContract]
    [KnownType(typeof(TransferElementSession))]
    [KnownType(typeof(LogElementDTO))]
    [KnownType(typeof(TransferElementBrowserJob))]
    [KnownType(typeof(TransferElementFetchLogElement))]
    public class TransferElementRequest
    {
        [DataMember]
        public TransferElementRequestType Type { get; set; }
        
        [DataMember]        
        public object Data { get; set; }
    }
}
