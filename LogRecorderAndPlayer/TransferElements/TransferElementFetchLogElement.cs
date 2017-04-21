using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    [DataContract]
    public class TransferElementFetchLogElement
    {
        [DataMember]
        public Guid PageGUID { get; set; }

        [DataMember]
        public LogType LogType { get; set; }
    }
}
