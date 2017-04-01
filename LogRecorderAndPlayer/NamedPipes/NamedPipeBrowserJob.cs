using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    [DataContract]
    public class NamedPipeBrowserJob
    {       
        [DataMember]
        public Guid PageGUID { get; set; }

        [DataMember]
        public Guid? LogElementGUID { get; set; } 

        [DataMember]
        public LogElementDTO LogElement { get; set; } //Only available when launching browserJob
    }
}
