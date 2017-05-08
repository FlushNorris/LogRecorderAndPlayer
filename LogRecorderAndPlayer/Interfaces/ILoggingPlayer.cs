using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public interface ILoggingPlayer
    {
        bool StoredLogElementHistory(LogElementDTO previousLogElement, LogElementDTO nextLogElement);

        Uri FinalizeUri(List<Tuple<LogElementDTO, LogElementDTO>> previousServersideLogElements, Uri uri);
    }
}
