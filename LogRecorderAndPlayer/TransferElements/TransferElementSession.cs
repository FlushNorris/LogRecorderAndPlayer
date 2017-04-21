using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    [DataContract]
    public class TransferElementSession
    {
        [DataMember]
        public Guid ProcessGUID { get; set; }

        [DataMember]
        public int ProcessId { get; set; }        
    }
}
