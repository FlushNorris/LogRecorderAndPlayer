using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public class NamedPipeServer : IDisposable
    {
        private ServiceHost ServiceHost { get; set; }

        public NamedPipeServer()
        {
            var uri = "net.pipe://localhost";
            ServiceHost = new ServiceHost(typeof(NamedPipeService), new Uri(uri));
            ServiceHost.AddServiceEndpoint(typeof(INamedPipeService), new NetNamedPipeBinding(), "LRAPService");
            ServiceHost.Open();
        }

        public void Dispose()
        {
            
        }
    }
}
