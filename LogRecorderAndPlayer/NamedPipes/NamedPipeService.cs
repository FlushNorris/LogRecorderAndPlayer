using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)] 
    public class NamedPipeService : INamedPipeService
    {
        public string ProcessData(string someValue)
        {
            var callback = OperationContext.Current.GetCallbackChannel<INamedPipeCallbackService>();

            //System.Threading.Thread.Sleep(3000);

            callback.NotifyClient();

            //System.Threading.Thread.Sleep(3000);

            return $"{someValue} : {DateTime.Now}";
        }
    }
}
