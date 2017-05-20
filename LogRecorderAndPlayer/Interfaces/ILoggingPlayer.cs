using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LogRecorderAndPlayer
{
    public interface ILoggingPlayer
    {
        bool StoreLogElementHistory(LogElementDTO previousLogElement, LogElementDTO nextLogElement);

        AdditionalData BuildAdditionalData(HttpApplication httpApplication);

        string FinalizeUrl(List<Tuple<LogElementDTO, LogElementDTO, AdditionalData>> previousServersideLogElements, string url);
    }

    public class AdditionalData
    {
        public Dictionary<string, string> Data = new Dictionary<string, string>();
    }
}
