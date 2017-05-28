using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
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
            //serverGUID = new Guid("7639dc4b-35c1-4376-abbc-ccfa42e30825");
            //var baseAddress = new Uri("net.pipe://localhost/LogRecorderAndPlayer");

            ServiceInstance = new PlayerCommunicationService();

            //ServiceHost = new ServiceHost(ServiceInstance, baseAddress);
            try
            {
                var binding = new NetNamedPipeBinding();
                binding.Security.Mode = NetNamedPipeSecurityMode.None;
                binding.Security.Transport.ProtectionLevel = ProtectionLevel.None;    
                

                ServiceHost = new ServiceHost(ServiceInstance, new Uri($"net.pipe://localhost/{serverGUID.ToString().Replace("-", "")}"));                
                var endpoint = ServiceHost.AddServiceEndpoint(typeof(PlayerCommunicationServiceInterface), binding, $"LRAPService{serverGUID.ToString().Replace("-", "")}");                
                ServiceHost.Open();


                //var binding = new NetNamedPipeBinding();
                ////binding.Security.Mode = SecurityMode.None;
                ////binding.Security.Message.NegotiateServiceCredential = false;
                ////binding.Security.Message.ClientCredentialType = MessageCredentialType.None;

                //ServiceHost.AddServiceEndpoint(typeof(PlayerCommunicationServiceInterface), binding, $"{serverGUID.ToString().Replace("-", "")}");

                ////var smb = new ServiceMetadataBehavior();
                ////smb.HttpGetEnabled = true;
                ////ServiceHost.Description.Behaviors.Add(smb);
                
                //ServiceHost.Open();                
            }
            catch (Exception ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
                ServiceHost.Abort();
                ServiceHost = null;
                throw;
            }
        }

        //var factory = new DuplexChannelFactory<PlayerCommunicationServiceInterface>(new InstanceContext(this), new NetNamedPipeBinding(), new EndpointAddress($"net.pipe://localhost/{serverId.ToString().Replace("-", "")}/LRAPService{serverId.ToString().Replace("-", "")}"));

        //public NamedPipeServer(Guid serverGUID)
        //{
        //    ServiceInstanse = new NamedPipeService();

        //    //ServiceHost = new ServiceHost(ServiceInstanse, new Uri($"net.pipe://localhost//{serverGUID.ToString().Replace("-", "")}"));
        //    //ServiceHost.AddServiceEndpoint(typeof(INamedPipeService), new NetNamedPipeBinding(), $"LRAPService"); //{serverGUID.ToString().Replace("-", "")}");

        //    ServiceHost = new ServiceHost(ServiceInstanse, new Uri($"net.pipe://localhost/{serverGUID.ToString().Replace("-", "")}"));
        //    ServiceHost.AddServiceEndpoint(typeof(INamedPipeService), new NetNamedPipeBinding(), $"LRAPService{serverGUID.ToString().Replace("-", "")}");
        //    ServiceHost.Open();
        //}

        public void Dispose()
        {
            ServiceHost.Close();
            ServiceHost = null;
        }
    }
}
