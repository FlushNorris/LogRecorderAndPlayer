using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public class ClientMessageEndPointBehavior : IEndpointBehavior, IServiceBehavior
    {
        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new ClientMessageInspector());            
            //clientRuntime.ClientOperations.Add(new ());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            ChannelDispatcher channelDispatcher = endpointDispatcher.ChannelDispatcher;
            if (channelDispatcher != null)
            {
                foreach (EndpointDispatcher ed in channelDispatcher.Endpoints)
                {
                    ed.DispatchRuntime.MessageInspectors.Add(new ClientMessageMessageInspector());
                }
            }
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            throw new NotImplementedException();
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            throw new NotImplementedException();
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            throw new NotImplementedException();
        }
    }
}
