//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.ServiceModel;
//using System.Text;
//using System.Threading.Tasks;

//namespace LogRecorderAndPlayer
//{
//    // Define your service contract and specify the callback contract
//    [ServiceContract(CallbackContract = typeof(INamedPipeCallbackService))]
//    public interface INamedPipeService
//    {
//        [OperationContract(IsOneWay = false)]
//        string ProcessData(string someValue);
//    }
//}
