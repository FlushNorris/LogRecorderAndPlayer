using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public class PlayerCommunicationServer : IDisposable
    {
        private ServiceHost ServiceHost { get; set; }

        public PlayerCommunicationService ServiceInstance { get; set; }

        public string ServiceURL
        {
            get
            {
                return ServiceHost.BaseAddresses.ToString().TrimEnd('/') + '/' + ServiceHost.Description.Endpoints[0].Address.Uri.ToString().Trim('/');                 
            }
        }

        public CommunicationState ServerState
        {
            get
            {
                if (ServiceHost == null)
                    return CommunicationState.Faulted;
                return ServiceHost.State;                 
            }
        }

        public PlayerCommunicationServer(Guid serverGUID)
        {
            var baseAddress = new Uri("net.tcp://localhost:445/LogRecorderAndPlayer");

            ServiceInstance = new PlayerCommunicationService();

            ServiceHost = new ServiceHost(ServiceInstance, baseAddress);
            try
            { 
                var binding = new NetTcpBinding(); //WSHttpBinding();
                binding.Security.Mode = SecurityMode.None;
                //binding.Security.Message.NegotiateServiceCredential = false;
                binding.Security.Message.ClientCredentialType = MessageCredentialType.None;

                ServiceHost.AddServiceEndpoint(typeof(PlayerCommunicationServiceInterface), binding, $"{serverGUID.ToString().Replace(" - ", "")}");

                var smb = new ServiceMetadataBehavior();
                //smb.HttpGetEnabled = true;
                ServiceHost.Description.Behaviors.Add(smb);
                
                ServiceHost.Open();                
            }
            catch (Exception ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
                ServiceHost.Abort();
                ServiceHost = null;
                throw;
            }
        }

        public void Dispose()
        {
            ServiceHost.Close();
            ServiceHost = null;
        }
    }
}
