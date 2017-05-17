//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.ServiceModel;
//using System.Text;
//using System.Threading.Tasks;

//namespace LogRecorderAndPlayer
//{
//    public class NamedPipeServer : IDisposable
//    {
//        private ServiceHost ServiceHost { get; set; }

//        public NamedPipeService ServiceInstanse { get; set; }

//        public NamedPipeServer(Guid serverGUID)
//        {
//            ServiceInstanse = new NamedPipeService();

//            //ServiceHost = new ServiceHost(ServiceInstanse, new Uri($"net.pipe://localhost//{serverGUID.ToString().Replace("-", "")}"));
//            //ServiceHost.AddServiceEndpoint(typeof(INamedPipeService), new NetNamedPipeBinding(), $"LRAPService"); //{serverGUID.ToString().Replace("-", "")}");
            
//            ServiceHost = new ServiceHost(ServiceInstanse, new Uri($"net.pipe://localhost/{serverGUID.ToString().Replace("-", "")}"));
//            ServiceHost.AddServiceEndpoint(typeof(INamedPipeService), new NetNamedPipeBinding(), $"LRAPService{serverGUID.ToString().Replace("-", "")}");
//            ServiceHost.Open();
//        }

//        public void Dispose()
//        {
            
//        }
//    }
//}
