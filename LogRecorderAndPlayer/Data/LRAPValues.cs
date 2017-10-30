using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer.Data
{
    public struct LRAPValues
    {
        public Guid? SessionGUID { get; set; }
        public Guid? PageGUID { get; set; }
        public Guid? ServerGUID { get; set; }

        public LRAPValues(Guid? sessionGUID, Guid? pageGUID, Guid? serverGUID)
        {
            this.SessionGUID = sessionGUID;
            this.PageGUID = pageGUID;
            this.ServerGUID = serverGUID;
        }
    }
}
