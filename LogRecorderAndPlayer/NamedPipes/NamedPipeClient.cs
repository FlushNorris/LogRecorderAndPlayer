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

        public string SendRequest(string value = "")
        {
            return Proxy.ProcessData(value);
        }

        public static string SendRequest_Threading(Guid serverId, string value, out string error, bool async = false)
        {
            string result = null;
            string errorTmp = null;
            var t = new Thread(() =>
            {
                try
                {
                    var client = new NamedPipeClient(serverId);
                    result = client.SendRequest(value);
                    client.Close();
                }
                catch (Exception ex)
                {
                    errorTmp = ex.Message;
                }
            });
            t.Start();
            if (async)
            {
                error = null;
                return null;
            }
            t.Join(60000); //wait 60sec for response
            error = errorTmp;
            return result;
        }
        public void NotifyClient()
        {

        }

        public NamedPipeClient(Guid serverId)
        {
            var factory = new DuplexChannelFactory<INamedPipeService>(new InstanceContext(this), new NetNamedPipeBinding(), new EndpointAddress($"net.pipe://localhost/{serverId.ToString().Replace("-", "")}/LRAPService{serverId.ToString().Replace("-", "")}"));
//            var factory = new DuplexChannelFactory<INamedPipeService>(new InstanceContext(this), new NetNamedPipeBinding(), new EndpointAddress($"net.pipe://localhost/{serverId.ToString().Replace("-", "")}/LRAPService"));
            Proxy = factory.CreateChannel();

            ((IClientChannel)Proxy).Faulted += NamedPipeClient_Faulted;
            ((IClientChannel)Proxy).Opened += NamedPipeClient_Opened;
            ((IClientChannel)Proxy).Open();
        }

        public void Close()
        {
            ((IClientChannel)Proxy).Close();
        }

        private void NamedPipeClient_Opened(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void NamedPipeClient_Faulted(object sender, EventArgs e)
        {
            throw new Exception("Connection faulted");
        }        

    }
}
