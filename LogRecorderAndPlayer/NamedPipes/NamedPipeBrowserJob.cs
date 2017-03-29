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
        public Guid SessionGUID { get; set; }

        [DataMember]
        public Guid LogElementGUID { get; set; }
    }
}
