using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using NamedPipesAssembly;

namespace NamedPipesServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a service host with an named pipe endpoint
            using (var host = new ServiceHost(typeof(SimpleService), new Uri("net.pipe://localhost")))
            {
                host.AddServiceEndpoint(typeof(ISimpleService), new NetNamedPipeBinding(), "SimpleService");
                host.Open();

                Console.WriteLine("Simple Service Running...");
                Console.ReadLine();

                host.Close();
            }
        }
    }

    //    [ServiceBehavior(UseSynchronizationContext = true)] // (InstanceContextMode = InstanceContextMode.PerCall)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class SimpleService : ISimpleService
    {
        public string ProcessData()
        {
            // Get a handle to the call back channel
            var callback = OperationContext.Current.GetCallbackChannel<IMyCallbackService>();

            callback.NotifyClient();
            return DateTime.Now.ToString();
        }
    }
}
