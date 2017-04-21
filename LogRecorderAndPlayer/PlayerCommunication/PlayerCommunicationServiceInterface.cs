using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    [ServiceContract]
    public interface PlayerCommunicationServiceInterface
    {
        [OperationContract]
        string ProcessData(string someValue);
    }
}
