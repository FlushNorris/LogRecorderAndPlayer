using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public class TransferLogElementHistory
    {
        public LogElementDTO PreviousLogElement { get; set; }
        public LogElementDTO NextLogElement { get; set; }
        public AdditionalData AdditionalData { get; set; }
    }
}
