using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace LogRecorderAndPlayer
{
    public static class LoggingWCF
    {
        public static void SetupClientBase<T>(System.ServiceModel.ClientBase<T> client, HttpContext context) where T : class
        {
            var eab = new EndpointAddressBuilder(client.Endpoint.Address);
            
            eab.Headers.Add(AddressHeader.CreateAddressHeader(Consts.GUIDTag,
                                                                string.Empty,
                                                                LoggingHelper.GetInstanceGUID(context, () => Guid.NewGuid()).GetValueOrDefault().ToString()));

            eab.Headers.Add(AddressHeader.CreateAddressHeader(Consts.SessionGUIDTag,
                                                                string.Empty,
                                                                LoggingHelper.GetSessionGUID(context, context?.Handler as Page, () => Guid.NewGuid()).GetValueOrDefault().ToString()));

            eab.Headers.Add(AddressHeader.CreateAddressHeader(Consts.PageGUIDTag,  
                                                                string.Empty,
                                                                LoggingHelper.GetPageGUID(context, context?.Handler as Page, () => Guid.NewGuid()).GetValueOrDefault().ToString()));

            eab.Headers.Add(AddressHeader.CreateAddressHeader(Consts.BundleGUIDTag,
                                                    string.Empty,
                                                    LoggingHelper.GetBundleGUID(context, () => Guid.NewGuid()).GetValueOrDefault().ToString()));

            client.Endpoint.Address = eab.ToEndpointAddress();
        }
    }
}
