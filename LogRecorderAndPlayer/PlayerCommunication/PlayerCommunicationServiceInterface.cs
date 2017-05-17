using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    //[ServiceContract]
    [ServiceContract(CallbackContract = typeof(PlayerCommunicationCallbackServiceInterface))]
    public interface PlayerCommunicationServiceInterface
    {
        [OperationContract(IsOneWay = false)]
        //[OperationContract]
        string ProcessData(string someValue);
    }
}
