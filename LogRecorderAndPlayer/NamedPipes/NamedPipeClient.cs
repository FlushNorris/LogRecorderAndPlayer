using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public class NamedPipeClient : INamedPipeCallbackService
    {
        private INamedPipeService Proxy { get; set; }

        public string TalkToServer(string someValue = "")
        {
            return Proxy.ProcessData(someValue);
        }

        public static string TalkToServer_Threading(string someValue = "") 
        {
            string result = null;
            var t = new Thread(() =>
            {
                var client = new NamedPipeClient();
                result = client.TalkToServer(someValue);
            });
            t.Start();
            t.Join(60000); //wait 60sec for response
            return result;
        }
        public void NotifyClient()
        {

        }

        public NamedPipeClient()
        {
            var factory = new DuplexChannelFactory<INamedPipeService>(new InstanceContext(this), new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/LRAPService"));
            Proxy = factory.CreateChannel();

            ((IClientChannel)Proxy).Faulted += NamedPipeClient_Faulted;
            ((IClientChannel)Proxy).Opened += NamedPipeClient_Opened;
            ((IClientChannel)Proxy).Open();
        }

        private void NamedPipeClient_Opened(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void NamedPipeClient_Faulted(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }        

    }
}
