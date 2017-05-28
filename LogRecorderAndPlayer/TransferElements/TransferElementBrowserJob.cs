using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    [DataContract]
    public class TransferElementBrowserJob
    {
        [DataMember]
        public Guid? SessionGUID { get; set; }

        [DataMember]
        public Guid? PageGUID { get; set; }

        [DataMember]
        public Guid? LogElementGUID { get; set; } //Either this is used to locate a logElement ...

        [DataMember]
        public LogType? LogType { get; set; } //...or Both LogType and HandlerUrl is used to locate a logElement at a given time

        [DataMember]
        public string HandlerUrl { get; set; }

        [DataMember]
        public JobStatus JobStatus { get; set; }

        [DataMember]
        public LogElementDTO LogElement { get; set; } //Only available when launching browserJob 
    }
}
