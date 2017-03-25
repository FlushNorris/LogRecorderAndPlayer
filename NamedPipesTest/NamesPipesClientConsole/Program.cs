using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using NamedPipesAssembly;

namespace NamesPipesClientConsole
{
    class Program
    {
        private static Whatever whatever = null;

        static void Main(string[] args)
        {
            //new Program().Run();
            whatever = new Whatever();
            whatever.Run();
        }

    }

    public class Whatever : IMyCallbackService
    {
        public void Run()
        {
            // Consume the service
            var factory = new DuplexChannelFactory<ISimpleService>(new InstanceContext(this), new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/SimpleService"));
            var proxy = factory.CreateChannel();

            Console.WriteLine(proxy.ProcessData());
        }

        public void NotifyClient()
        {
            Console.WriteLine("Notification from Server");
        }
    }

}
